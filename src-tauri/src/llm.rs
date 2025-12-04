//! LLM Service module for AI Anywhere
//! Handles all AI API communication with OpenAI-compatible endpoints

use futures_util::StreamExt;
use reqwest::multipart;
use serde::{Deserialize, Serialize};
use serde_json::{json, Value};
use std::collections::HashMap;
use std::path::Path;
use std::sync::atomic::{AtomicBool, Ordering};
use std::sync::Arc;
use tauri::{AppHandle, Emitter};

use crate::config::Configuration;
use crate::custom_tasks::CustomTasksManager;
use crate::history::HistoryManager;
use crate::operations::OperationType;
use crate::text::{extract_size_dimensions, normalize_transcription, process_llm_response};

/// Maximum estimated tokens allowed in a prompt (security limit)
/// This matches the frontend limit to provide defense in depth
const MAX_ESTIMATED_TOKENS: usize = 16000;

/// Token estimation correction factor (20% safety margin)
const TOKEN_CORRECTION_FACTOR: f64 = 1.20;

/// Estimates the number of tokens in a text string.
/// Uses word count * 1.33 * 1.20 as approximation (matches frontend logic).
fn estimate_tokens(text: &str) -> usize {
    if text.trim().is_empty() {
        return 0;
    }
    let words: usize = text.split_whitespace().count();
    ((words as f64) * 1.33 * TOKEN_CORRECTION_FACTOR).ceil() as usize
}

/// Validates that the prompt doesn't exceed the maximum token limit.
/// Returns an error message if validation fails.
fn validate_prompt_length(prompt: &str) -> Result<(), String> {
    let estimated_tokens = estimate_tokens(prompt);
    if estimated_tokens > MAX_ESTIMATED_TOKENS {
        return Err(format!(
            "Prompt too long (~{} estimated tokens). Maximum allowed: {} tokens.",
            estimated_tokens, MAX_ESTIMATED_TOKENS
        ));
    }
    Ok(())
}

/// LLM Request structure
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct LlmRequest {
    pub operation_type: String,
    pub prompt: String,
    pub selected_text: Option<String>,
    pub options: HashMap<String, String>,
    pub audio_file_path: Option<String>,
}

/// LLM Response structure
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct LlmResponse {
    pub success: bool,
    pub content: Option<String>,
    pub error: Option<String>,
    pub is_image: bool,
    pub image_url: Option<String>,
    pub is_audio: bool,
    pub audio_data: Option<Vec<u8>>,
    pub audio_format: Option<String>,
    pub audio_file_path: Option<String>,
}

impl Default for LlmResponse {
    fn default() -> Self {
        Self {
            success: false,
            content: None,
            error: None,
            is_image: false,
            image_url: None,
            is_audio: false,
            audio_data: None,
            audio_format: None,
            audio_file_path: None,
        }
    }
}

impl LlmResponse {
    pub fn success(content: String) -> Self {
        Self {
            success: true,
            content: Some(content),
            ..Default::default()
        }
    }

    pub fn error(message: String) -> Self {
        Self {
            success: false,
            error: Some(message),
            ..Default::default()
        }
    }

    pub fn image(url: String) -> Self {
        Self {
            success: true,
            content: Some(url.clone()),
            is_image: true,
            image_url: Some(url),
            ..Default::default()
        }
    }

    pub fn audio(file_path: String, format: String) -> Self {
        Self {
            success: true,
            content: Some("Audio generated successfully".to_string()),
            is_audio: true,
            audio_data: None, // No longer send raw bytes
            audio_format: Some(format),
            audio_file_path: Some(file_path),
            ..Default::default()
        }
    }
}

/// Streaming chunk event payload
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct StreamingChunk {
    pub content: String,
    pub done: bool,
}

/// Models response from API
#[derive(Debug, Deserialize)]
pub struct ModelsResponse {
    pub data: Vec<ModelInfo>,
}

#[derive(Debug, Deserialize)]
pub struct ModelInfo {
    pub id: String,
}

/// LLM Service for API communication
pub struct LlmService {
    config: Configuration,
    client: reqwest::Client,
}

impl LlmService {
    pub fn new(config: Configuration) -> Self {
        let client = reqwest::Client::builder()
            .timeout(std::time::Duration::from_secs(300))
            .build()
            .unwrap_or_default();

        Self { config, client }
    }

    /// Process an LLM request based on operation type
    pub async fn process_request(&self, request: LlmRequest) -> LlmResponse {
        // Validate prompt length (security check - defense in depth)
        if let Err(e) = validate_prompt_length(&request.prompt) {
            return LlmResponse::error(e);
        }

        // Try to parse as built-in operation type
        if let Ok(op_type) = serde_json::from_value::<OperationType>(json!(request.operation_type))
        {
            match op_type {
                OperationType::ImageGeneration => self.process_image_generation(&request).await,
                OperationType::SpeechToText => self.process_speech_to_text(&request).await,
                OperationType::TextToSpeech => self.process_text_to_speech(&request).await,
                _ => self.process_text_request(&request).await,
            }
        } else {
            // Assume it's a custom task (text-based)
            self.process_text_request(&request).await
        }
    }

    /// Check if an operation type supports streaming
    pub fn supports_streaming(operation_type: &str) -> bool {
        if let Ok(op_type) = serde_json::from_value::<OperationType>(json!(operation_type)) {
            !matches!(
                op_type,
                OperationType::ImageGeneration
                    | OperationType::SpeechToText
                    | OperationType::TextToSpeech
            )
        } else {
            // Custom tasks are text-based and support streaming
            true
        }
    }

    /// Process a streaming text request with real-time chunk emission
    pub async fn process_streaming_request(
        &self,
        request: &LlmRequest,
        app: &AppHandle,
        cancel_flag: Arc<AtomicBool>,
    ) -> LlmResponse {
        // Validate prompt length (security check - defense in depth)
        if let Err(e) = validate_prompt_length(&request.prompt) {
            return LlmResponse::error(e);
        }

        // Only text operations support streaming
        if !Self::supports_streaming(&request.operation_type) {
            return LlmResponse::error(
                "Streaming not supported for this operation type".to_string(),
            );
        }

        let operations = crate::operations::get_default_operations();

        // Try to find in default operations
        let mut system_prompt = String::new();

        // Check if it's a built-in operation
        if let Ok(op_type) = serde_json::from_value::<OperationType>(json!(request.operation_type))
        {
            if let Some(op) = operations.iter().find(|op| op.operation_type == op_type) {
                system_prompt = op.system_prompt.clone();
            }
        }

        // If not found, check custom tasks
        if system_prompt.is_empty() {
            if let Ok(Some(task)) = CustomTasksManager::get(&request.operation_type) {
                system_prompt = task.system_prompt.clone();
            } else if system_prompt.is_empty() {
                // Fallback or error if not found
                return LlmResponse::error("Unknown operation type".to_string());
            }
        }
        for (key, value) in &request.options {
            system_prompt = system_prompt.replace(&format!("{{{}}}", key), value);
        }

        // Build user prompt
        let user_prompt = if let Some(ref selected_text) = request.selected_text {
            if !selected_text.is_empty() {
                format!("{}\n\nText to process:\n{}", request.prompt, selected_text)
            } else {
                request.prompt.clone()
            }
        } else {
            request.prompt.clone()
        };

        // Build request body with streaming enabled
        let body = json!({
            "model": self.config.llm_model,
            "messages": [
                {"role": "system", "content": system_prompt},
                {"role": "user", "content": user_prompt}
            ],
            "max_tokens": 4096,
            "temperature": 0.6,
            "stream": true
        });

        let url = self.build_api_url("/chat/completions");

        if self.config.enable_debug_logging {
            println!("--- LLM Streaming Request ---");
            println!("Config API Base URL: {}", self.config.api_base_url);
            println!("Final URL: POST {}", url);
            println!("Model: {}", self.config.llm_model);
            println!("-------------------");
        }

        let api_key = self.config.get_api_key();
        if api_key.is_empty() {
            return LlmResponse::error(
                "API key is empty. Please configure your API key in settings.".to_string(),
            );
        }

        let response = self
            .client
            .post(&url)
            .header("Authorization", format!("Bearer {}", api_key))
            .header("Content-Type", "application/json")
            .json(&body)
            .send()
            .await;

        match response {
            Ok(resp) => {
                let status = resp.status();
                if self.config.enable_debug_logging {
                    println!("--- LLM Streaming Response ---");
                    println!("Status: {}", status);
                }

                if !status.is_success() {
                    let error_text = resp.text().await.unwrap_or_default();
                    if self.config.enable_debug_logging {
                        println!("Error Body: {}", error_text);
                        println!("--------------------");
                    }
                    return LlmResponse::error(format!("API Error: {}", error_text));
                }

                // Process the streaming response
                let mut full_content = String::new();
                let mut stream = resp.bytes_stream();
                let mut buffer = String::new(); // Buffer for incomplete SSE lines

                while let Some(chunk_result) = stream.next().await {
                    // Check if cancelled
                    if cancel_flag.load(Ordering::Relaxed) {
                        // Emit cancelled event
                        let _ = app.emit("llm-stream-cancelled", ());
                        return LlmResponse::error("Request cancelled".to_string());
                    }

                    match chunk_result {
                        Ok(chunk) => {
                            let chunk_str = String::from_utf8_lossy(&chunk);
                            buffer.push_str(&chunk_str);

                            // Process complete lines from buffer
                            while let Some(newline_pos) = buffer.find('\n') {
                                let line = buffer[..newline_pos].trim_end_matches('\r').to_string();
                                buffer = buffer[newline_pos + 1..].to_string();

                                if line.starts_with("data: ") {
                                    let data = &line[6..];

                                    if data == "[DONE]" {
                                        // Emit final done event
                                        let _ = app.emit(
                                            "llm-stream-chunk",
                                            StreamingChunk {
                                                content: String::new(),
                                                done: true,
                                            },
                                        );
                                        continue;
                                    }

                                    // Parse JSON data
                                    if let Ok(json) = serde_json::from_str::<Value>(data) {
                                        if let Some(content) =
                                            json["choices"][0]["delta"]["content"].as_str()
                                        {
                                            full_content.push_str(content);

                                            // Emit chunk event
                                            let _ = app.emit(
                                                "llm-stream-chunk",
                                                StreamingChunk {
                                                    content: content.to_string(),
                                                    done: false,
                                                },
                                            );
                                        }
                                    }
                                }
                            }
                        }
                        Err(e) => {
                            if self.config.enable_debug_logging {
                                println!("Stream error: {}", e);
                            }
                            return LlmResponse::error(format!("Stream error: {}", e));
                        }
                    }
                }

                if self.config.enable_debug_logging {
                    println!("Full streamed content length: {}", full_content.len());
                    println!("--------------------");
                }

                let processed = process_llm_response(&full_content);
                LlmResponse::success(processed)
            }
            Err(e) => {
                if self.config.enable_debug_logging {
                    println!("Request Failed: {}", e);
                    println!("--------------------");
                }
                LlmResponse::error(format!("Request failed: {}", e))
            }
        }
    }

    /// Build the correct API URL for the given endpoint path
    fn build_api_url(&self, endpoint: &str) -> String {
        let base = self.config.api_base_url.trim_end_matches('/');

        // Check if the base URL already ends with /v1 or similar API version
        // If not, and it doesn't already contain the endpoint, add /v1
        let needs_v1 = !base.ends_with("/v1")
            && !base.ends_with("/v1/")
            && !base.contains("/chat/")
            && !base.contains("/images/")
            && !base.contains("/audio/")
            && !base.contains("/models");

        if needs_v1 {
            format!("{}/v1{}", base, endpoint)
        } else {
            format!("{}{}", base, endpoint)
        }
    }

    /// Process text-based requests (chat completions)
    async fn process_text_request(&self, request: &LlmRequest) -> LlmResponse {
        let operations = crate::operations::get_default_operations();

        let mut system_prompt = String::new();

        // Check if it's a built-in operation
        if let Ok(op_type) = serde_json::from_value::<OperationType>(json!(request.operation_type))
        {
            if let Some(op) = operations.iter().find(|op| op.operation_type == op_type) {
                system_prompt = op.system_prompt.clone();
            }
        }

        // If not found, check custom tasks
        if system_prompt.is_empty() {
            if let Ok(Some(task)) = CustomTasksManager::get(&request.operation_type) {
                system_prompt = task.system_prompt.clone();
            } else if system_prompt.is_empty() {
                return LlmResponse::error("Unknown operation type".to_string());
            }
        }

        // Build system prompt with options
        for (key, value) in &request.options {
            system_prompt = system_prompt.replace(&format!("{{{}}}", key), value);
        }

        // Build user prompt
        let user_prompt = if let Some(ref selected_text) = request.selected_text {
            if !selected_text.is_empty() {
                format!("{}\n\nText to process:\n{}", request.prompt, selected_text)
            } else {
                request.prompt.clone()
            }
        } else {
            request.prompt.clone()
        };

        // Build request body
        let body = json!({
            "model": self.config.llm_model,
            "messages": [
                {"role": "system", "content": system_prompt},
                {"role": "user", "content": user_prompt}
            ],
            "max_tokens": 4096,
            "temperature": 0.6
        });

        let url = self.build_api_url("/chat/completions");

        if self.config.enable_debug_logging {
            println!("--- LLM Request ---");
            println!("Config API Base URL: {}", self.config.api_base_url);
            println!("Final URL: POST {}", url);
            println!("Model: {}", self.config.llm_model);
            println!(
                "API Key (first 8 chars): {}...",
                self.config
                    .get_api_key()
                    .chars()
                    .take(8)
                    .collect::<String>()
            );
            println!(
                "Body: {}",
                serde_json::to_string_pretty(&body).unwrap_or_default()
            );
            println!("-------------------");
        }

        let api_key = self.config.get_api_key();
        if api_key.is_empty() {
            return LlmResponse::error(
                "API key is empty. Please configure your API key in settings.".to_string(),
            );
        }

        let response = self
            .client
            .post(&url)
            .header("Authorization", format!("Bearer {}", api_key))
            .header("Content-Type", "application/json")
            .json(&body)
            .send()
            .await;

        match response {
            Ok(resp) => {
                let status = resp.status();
                if self.config.enable_debug_logging {
                    println!("--- LLM Response ---");
                    println!("Status: {}", status);
                }

                if !status.is_success() {
                    let error_text = resp.text().await.unwrap_or_default();
                    if self.config.enable_debug_logging {
                        println!("Error Body: {}", error_text);
                        println!("--------------------");
                    }
                    return LlmResponse::error(format!("API Error: {}", error_text));
                }

                let text = resp.text().await.unwrap_or_default();
                if self.config.enable_debug_logging {
                    println!("Body: {}", text);
                    println!("--------------------");
                }

                let json: Result<Value, _> = serde_json::from_str(&text);
                match json {
                    Ok(data) => {
                        if let Some(content) = data["choices"][0]["message"]["content"].as_str() {
                            let processed = process_llm_response(content);
                            LlmResponse::success(processed)
                        } else {
                            LlmResponse::error("No content in response".to_string())
                        }
                    }
                    Err(e) => LlmResponse::error(format!("Failed to parse response: {}", e)),
                }
            }
            Err(e) => {
                if self.config.enable_debug_logging {
                    println!("Request Failed: {}", e);
                    println!("--------------------");
                }
                LlmResponse::error(format!("Request failed: {}", e))
            }
        }
    }

    /// Process image generation requests
    async fn process_image_generation(&self, request: &LlmRequest) -> LlmResponse {
        let size_string = request
            .options
            .get("size")
            .map(|s| s.as_str())
            .unwrap_or("512x512");
        let size = extract_size_dimensions(size_string);
        let quality = request
            .options
            .get("quality")
            .map(|s| s.as_str())
            .unwrap_or("standard");
        let style = request
            .options
            .get("style")
            .map(|s| s.as_str())
            .unwrap_or("vivid");

        let model = if !self.config.image_model.is_empty() {
            &self.config.image_model
        } else {
            "FLUX.1-schnell"
        };

        let body = json!({
            "model": model,
            "prompt": request.prompt,
            "size": size,
            "quality": quality,
            "style": style,
            "response_format": "url",
            "n": 1
        });

        let url = self.build_api_url("/images/generations");

        if self.config.enable_debug_logging {
            println!("--- Image Generation Request ---");
            println!("Config API Base URL: {}", self.config.api_base_url);
            println!("Final URL: POST {}", url);
            println!(
                "Body: {}",
                serde_json::to_string_pretty(&body).unwrap_or_default()
            );
            println!("------------------------------");
        }

        let response = self
            .client
            .post(&url)
            .header(
                "Authorization",
                format!("Bearer {}", self.config.get_api_key()),
            )
            .header("Content-Type", "application/json")
            .json(&body)
            .send()
            .await;

        match response {
            Ok(resp) => {
                let status = resp.status();
                if self.config.enable_debug_logging {
                    println!("--- Image Generation Response ---");
                    println!("Status: {}", status);
                }

                if !status.is_success() {
                    let error_text = resp.text().await.unwrap_or_default();
                    if self.config.enable_debug_logging {
                        println!("Error Body: {}", error_text);
                        println!("-------------------------------");
                    }
                    return LlmResponse::error(format!("Image Generation Error: {}", error_text));
                }

                let text = resp.text().await.unwrap_or_default();
                if self.config.enable_debug_logging {
                    println!("Body: {}", text);
                    println!("-------------------------------");
                }

                let json: Result<Value, _> = serde_json::from_str(&text);
                match json {
                    Ok(data) => {
                        if let Some(url) = data["data"][0]["url"].as_str() {
                            LlmResponse::image(url.to_string())
                        } else {
                            LlmResponse::error("No image URL in response".to_string())
                        }
                    }
                    Err(e) => LlmResponse::error(format!("Failed to parse response: {}", e)),
                }
            }
            Err(e) => {
                if self.config.enable_debug_logging {
                    println!("Request Failed: {}", e);
                    println!("-------------------------------");
                }
                LlmResponse::error(format!("Request failed: {}", e))
            }
        }
    }

    /// Process speech-to-text (audio transcription) requests
    async fn process_speech_to_text(&self, request: &LlmRequest) -> LlmResponse {
        let audio_path = match &request.audio_file_path {
            Some(path) if Path::new(path).exists() => path,
            _ => return LlmResponse::error("Audio file not found or not specified".to_string()),
        };

        let model = if !self.config.audio_model.is_empty() {
            &self.config.audio_model
        } else {
            "whisper-1"
        };

        let language = request
            .options
            .get("language")
            .map(|s| s.as_str())
            .unwrap_or("auto");

        // Read file for multipart upload
        let file_bytes = match std::fs::read(audio_path) {
            Ok(bytes) => bytes,
            Err(e) => return LlmResponse::error(format!("Failed to read audio file: {}", e)),
        };

        let file_name = Path::new(audio_path)
            .file_name()
            .and_then(|n| n.to_str())
            .unwrap_or("audio.mp3")
            .to_string();

        let file_part = multipart::Part::bytes(file_bytes)
            .file_name(file_name)
            .mime_str("audio/mpeg")
            .unwrap();

        let mut form = multipart::Form::new()
            .part("file", file_part)
            .text("model", model.to_string())
            .text("response_format", "text");

        if language != "auto" && !language.is_empty() {
            form = form.text("language", language.to_string());
        }

        let url = self.build_api_url("/audio/transcriptions");

        if self.config.enable_debug_logging {
            println!("--- Speech to Text Request ---");
            println!("Config API Base URL: {}", self.config.api_base_url);
            println!("Final URL: POST {}", url);
            println!("Model: {}", model);
            println!("(Multipart form data not logged)");
            println!("------------------------------");
        }

        let response = self
            .client
            .post(&url)
            .header(
                "Authorization",
                format!("Bearer {}", self.config.get_api_key()),
            )
            .multipart(form)
            .send()
            .await;

        match response {
            Ok(resp) => {
                let status = resp.status();
                if self.config.enable_debug_logging {
                    println!("--- Speech to Text Response ---");
                    println!("Status: {}", status);
                }

                if !status.is_success() {
                    let error_text = resp.text().await.unwrap_or_default();
                    if self.config.enable_debug_logging {
                        println!("Error Body: {}", error_text);
                        println!("-------------------------------");
                    }
                    return LlmResponse::error(format!("Transcription Error: {}", error_text));
                }

                let text = resp.text().await.unwrap_or_default();
                if self.config.enable_debug_logging {
                    println!("Body: {}", text);
                    println!("-------------------------------");
                }

                // Try to parse as JSON first (some APIs return JSON)
                if let Ok(json) = serde_json::from_str::<Value>(&text) {
                    if let Some(transcript) = json["text"].as_str() {
                        let normalized = normalize_transcription(transcript);
                        return LlmResponse::success(normalized);
                    }
                }

                // Otherwise treat as plain text
                let normalized = normalize_transcription(&text);
                LlmResponse::success(normalized)
            }
            Err(e) => {
                if self.config.enable_debug_logging {
                    println!("Request Failed: {}", e);
                    println!("-------------------------------");
                }
                LlmResponse::error(format!("Request failed: {}", e))
            }
        }
    }

    /// Process text-to-speech requests
    async fn process_text_to_speech(&self, request: &LlmRequest) -> LlmResponse {
        if request.prompt.trim().is_empty() {
            return LlmResponse::error("Text prompt is required for Text to Speech".to_string());
        }

        let model = request
            .options
            .get("model")
            .map(|s| s.as_str())
            .unwrap_or(&self.config.tts_model);

        let voice = request
            .options
            .get("voice")
            .map(|s| s.as_str())
            .unwrap_or("alloy");
        let speed: f32 = request
            .options
            .get("speed")
            .and_then(|s| s.parse::<f32>().ok())
            .unwrap_or(1.0_f32)
            .clamp(0.25_f32, 2.0_f32);
        let format = request
            .options
            .get("format")
            .map(|s| s.as_str())
            .unwrap_or("mp3");
        let language = request
            .options
            .get("language")
            .map(|s| s.as_str())
            .unwrap_or("pt");

        let body = json!({
            "model": model,
            "input": request.prompt,
            "voice": voice,
            "response_format": format,
            "speed": speed,
            "language": language
        });

        let url = self.build_api_url("/audio/speech");

        if self.config.enable_debug_logging {
            println!("--- Text to Speech Request ---");
            println!("Config API Base URL: {}", self.config.api_base_url);
            println!("Final URL: POST {}", url);
            println!(
                "Body: {}",
                serde_json::to_string_pretty(&body).unwrap_or_default()
            );
            println!("------------------------------");
        }

        let response = self
            .client
            .post(&url)
            .header(
                "Authorization",
                format!("Bearer {}", self.config.get_api_key()),
            )
            .header("Content-Type", "application/json")
            .json(&body)
            .send()
            .await;

        match response {
            Ok(resp) => {
                let status = resp.status();
                if self.config.enable_debug_logging {
                    println!("--- Text to Speech Response ---");
                    println!("Status: {}", status);
                }

                if !status.is_success() {
                    let error_text = resp.text().await.unwrap_or_default();
                    if self.config.enable_debug_logging {
                        println!("Error Body: {}", error_text);
                        println!("-------------------------------");
                    }
                    return LlmResponse::error(format!("TTS Error: {}", error_text));
                }

                match resp.bytes().await {
                    Ok(bytes) => {
                        if self.config.enable_debug_logging {
                            println!("Received {} bytes of audio data", bytes.len());
                            println!("-------------------------------");
                        }
                        // Save audio to media folder and return file path
                        match HistoryManager::save_audio(&bytes, format) {
                            Ok(file_path) => {
                                if self.config.enable_debug_logging {
                                    println!("Saved audio to: {}", file_path);
                                }
                                LlmResponse::audio(file_path, format.to_string())
                            }
                            Err(e) => {
                                if self.config.enable_debug_logging {
                                    println!("Failed to save audio: {}", e);
                                }
                                LlmResponse::error(format!("Failed to save audio: {}", e))
                            }
                        }
                    }
                    Err(e) => {
                        if self.config.enable_debug_logging {
                            println!("Failed to read audio data: {}", e);
                            println!("-------------------------------");
                        }
                        LlmResponse::error(format!("Failed to read audio data: {}", e))
                    }
                }
            }
            Err(e) => {
                if self.config.enable_debug_logging {
                    println!("Request Failed: {}", e);
                    println!("-------------------------------");
                }
                LlmResponse::error(format!("Request failed: {}", e))
            }
        }
    }

    /// Fetch available models from API
    pub async fn get_models(&self) -> Result<ModelsResponse, String> {
        let url = self.build_api_url("/models");

        if self.config.enable_debug_logging {
            println!("--- Fetching Models ---");
            println!("Config API Base URL: {}", self.config.api_base_url);
            println!("Final URL: GET {}", url);
            println!("-----------------------");
        }

        let response = self
            .client
            .get(&url)
            .header(
                "Authorization",
                format!("Bearer {}", self.config.get_api_key()),
            )
            .send()
            .await
            .map_err(|e| format!("Request failed: {}", e))?;

        let status = response.status();
        if !status.is_success() {
            let error_text = response.text().await.unwrap_or_default();
            if self.config.enable_debug_logging {
                println!("--- Models Response Error ---");
                println!("Status: {}", status);
                println!("Error: {}", error_text);
                println!("-----------------------------");
            }
            return Err(format!(
                "Failed to fetch models ({}): {}",
                status, error_text
            ));
        }

        response
            .json::<ModelsResponse>()
            .await
            .map_err(|e| format!("Failed to parse models: {}", e))
    }

    /// Test API connection
    pub async fn test_connection(&self) -> Result<(), String> {
        self.get_models().await?;
        Ok(())
    }
}

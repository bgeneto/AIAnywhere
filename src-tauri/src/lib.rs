//! AI Anywhere - Universal AI Assistant
//! Tauri 2.0 backend library

mod clipboard;
mod config;
mod custom_tasks;
mod encryption;
mod history;
mod llm;
mod operations;
mod text;

use config::{Configuration, ConfigurationDto, PasteBehavior};
use custom_tasks::{CustomTask, CustomTaskOption, CustomTasksManager};
use history::{HistoryEntry, HistoryManager};
use llm::{LlmRequest, LlmResponse, LlmService};
use operations::Operation;
use serde::{Deserialize, Serialize};
use std::collections::HashMap;
use std::sync::atomic::AtomicBool;
use std::sync::{Arc, Mutex};
use tauri::{
    menu::{Menu, MenuItem},
    tray::{MouseButton, MouseButtonState, TrayIconBuilder, TrayIconEvent},
    AppHandle, Emitter, Manager, State,
};

/// Application state
pub struct AppState {
    config: Mutex<Configuration>,
    cancel_flag: Arc<AtomicBool>,
}

impl AppState {
    fn new() -> Self {
        let config = Configuration::load().unwrap_or_default();
        Self {
            config: Mutex::new(config),
            cancel_flag: Arc::new(AtomicBool::new(false)),
        }
    }
}

// ============================================================================
// Helper Functions
// ============================================================================

/// Download an image from URL and copy it to the system clipboard
async fn copy_image_to_clipboard(
    app: &AppHandle,
    image_url: &str,
    debug_logging: bool,
) -> Result<(), String> {
    use tauri_plugin_clipboard_manager::ClipboardExt;

    if debug_logging {
        println!(
            "[copy_image_to_clipboard] Starting download from: {}",
            image_url
        );
    }

    // Create HTTP client
    let client = reqwest::Client::builder()
        .timeout(std::time::Duration::from_secs(60))
        .build()
        .map_err(|e| format!("Failed to create HTTP client: {}", e))?;

    // Download the image
    let response = client
        .get(image_url)
        .send()
        .await
        .map_err(|e| format!("Failed to download image: {}", e))?;

    let status = response.status();
    if !status.is_success() {
        return Err(format!("Download failed with status: {}", status));
    }

    let bytes = response
        .bytes()
        .await
        .map_err(|e| format!("Failed to read image bytes: {}", e))?;

    if debug_logging {
        println!("[copy_image_to_clipboard] Downloaded {} bytes", bytes.len());
    }

    if bytes.is_empty() {
        return Err("Downloaded image is empty".to_string());
    }

    // Decode the image to get RGBA data
    let img =
        image::load_from_memory(&bytes).map_err(|e| format!("Failed to decode image: {}", e))?;

    let rgba = img.to_rgba8();
    let (width, height) = rgba.dimensions();
    let raw_pixels = rgba.into_raw();

    if debug_logging {
        println!(
            "[copy_image_to_clipboard] Image decoded: {}x{}, {} bytes of RGBA data",
            width,
            height,
            raw_pixels.len()
        );
    }

    // Create Tauri Image from RGBA data
    let tauri_image = tauri::image::Image::new_owned(raw_pixels, width, height);

    // Copy to clipboard
    app.clipboard()
        .write_image(&tauri_image)
        .map_err(|e| format!("Failed to write image to clipboard: {}", e))?;

    if debug_logging {
        println!("[copy_image_to_clipboard] Image copied to clipboard successfully!");
    }

    Ok(())
}

// ============================================================================
// Configuration Commands
// ============================================================================

#[tauri::command]
async fn get_configuration(state: State<'_, AppState>) -> Result<ConfigurationDto, String> {
    let config = state.config.lock().map_err(|e| e.to_string())?;
    Ok(ConfigurationDto::from(&*config))
}

#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
struct SaveConfigRequest {
    hotkey: String,
    api_base_url: String,
    api_key: Option<String>,
    llm_model: String,
    image_model: String,
    audio_model: String,
    tts_model: String,
    paste_behavior: PasteBehavior,
    disable_text_selection: bool,
    enable_debug_logging: bool,
    copy_delay_ms: u64,
    models: Vec<String>,
    image_models: Vec<String>,
    audio_models: Vec<String>,
    tts_models: Vec<String>,
    history_limit: usize,
    media_retention_days: u32,
}

#[tauri::command]
async fn save_configuration(
    state: State<'_, AppState>,
    request: SaveConfigRequest,
) -> Result<(), String> {
    let mut config = state.config.lock().map_err(|e| e.to_string())?;

    config.hotkey = request.hotkey;
    config.api_base_url = request.api_base_url;
    config.llm_model = request.llm_model;
    config.image_model = request.image_model;
    config.audio_model = request.audio_model;
    config.tts_model = request.tts_model;
    config.paste_behavior = request.paste_behavior;
    config.disable_text_selection = request.disable_text_selection;
    config.enable_debug_logging = request.enable_debug_logging;
    config.copy_delay_ms = request.copy_delay_ms;
    config.models = request.models;
    config.image_models = request.image_models;
    config.audio_models = request.audio_models;
    config.tts_models = request.tts_models;
    config.history_limit = request.history_limit;
    config.media_retention_days = request.media_retention_days;

    // Only update API key if provided
    if let Some(key) = request.api_key {
        if !key.is_empty() {
            config.set_api_key(&key);
        }
    }

    config.save()
}

#[tauri::command]
async fn update_models(
    state: State<'_, AppState>,
    models: Vec<String>,
    image_models: Vec<String>,
    audio_models: Vec<String>,
) -> Result<(), String> {
    let mut config = state.config.lock().map_err(|e| e.to_string())?;
    config.models = models;
    config.image_models = image_models;
    config.audio_models = audio_models;
    config.save()
}

// ============================================================================
// Operations Commands
// ============================================================================

#[tauri::command]
async fn get_operations() -> Result<Vec<Operation>, String> {
    Ok(operations::get_default_operations())
}

// ============================================================================
// History Commands
// ============================================================================

#[tauri::command]
async fn get_history(search_query: Option<String>) -> Result<Vec<HistoryEntry>, String> {
    match search_query {
        Some(query) if !query.trim().is_empty() => HistoryManager::search(&query).await,
        _ => HistoryManager::load().await,
    }
}

#[tauri::command]
async fn save_history_entry(
    state: State<'_, AppState>,
    operation_type: String,
    prompt_text: String,
    response_text: Option<String>,
    operation_options: HashMap<String, String>,
    media_path: Option<String>,
) -> Result<HistoryEntry, String> {
    let history_limit = {
        let config = state.config.lock().map_err(|e| e.to_string())?;
        config.history_limit
    }; // Lock is released here

    let entry = HistoryEntry::new(
        operation_type,
        prompt_text,
        response_text,
        operation_options,
        media_path,
    );
    HistoryManager::add_entry(entry.clone(), history_limit).await?;

    Ok(entry)
}

#[tauri::command]
async fn delete_history_entry(id: String) -> Result<(), String> {
    HistoryManager::delete_entry(&id).await
}

#[tauri::command]
async fn clear_history() -> Result<(), String> {
    HistoryManager::clear().await
}

#[tauri::command]
async fn cleanup_old_media(state: State<'_, AppState>) -> Result<u32, String> {
    let retention_days = {
        let config = state.config.lock().map_err(|e| e.to_string())?;
        config.media_retention_days
    }; // Lock is released here
    HistoryManager::cleanup_old_media(retention_days).await
}

// ============================================================================
// Custom Tasks Commands
// ============================================================================

#[tauri::command]
async fn get_custom_tasks() -> Result<Vec<CustomTask>, String> {
    CustomTasksManager::load()
}

#[tauri::command]
async fn get_custom_task(id: String) -> Result<Option<CustomTask>, String> {
    CustomTasksManager::get(&id)
}

#[tauri::command]
async fn create_custom_task(
    name: String,
    description: String,
    system_prompt: String,
    options: Vec<CustomTaskOption>,
) -> Result<CustomTask, String> {
    let task = CustomTask::new(name, description, system_prompt, options);
    CustomTasksManager::create(task)
}

#[tauri::command]
async fn update_custom_task(
    id: String,
    name: String,
    description: String,
    system_prompt: String,
    options: Vec<CustomTaskOption>,
) -> Result<CustomTask, String> {
    let task = CustomTask::new(name, description, system_prompt, options);
    CustomTasksManager::update(&id, task)
}

#[tauri::command]
async fn delete_custom_task(id: String) -> Result<(), String> {
    CustomTasksManager::delete(&id)
}

#[tauri::command]
async fn export_custom_tasks() -> Result<String, String> {
    CustomTasksManager::export()
}

#[tauri::command]
async fn import_custom_tasks(json: String) -> Result<usize, String> {
    CustomTasksManager::import(&json)
}

// ============================================================================
// LLM Commands
// ============================================================================

#[tauri::command]
async fn process_llm_request(
    app: AppHandle,
    state: State<'_, AppState>,
    request: LlmRequest,
) -> Result<LlmResponse, String> {
    let config = {
        let config = state.config.lock().map_err(|e| e.to_string())?;
        config.clone()
    };

    let service = LlmService::new(config.clone());
    let response = service.process_request(request).await;

    if response.success {
        // Handle image responses
        if response.is_image {
            if let Some(image_url) = &response.image_url {
                match config.paste_behavior {
                    PasteBehavior::AutoPaste => {
                        // Download and copy image to clipboard
                        if let Err(e) =
                            copy_image_to_clipboard(&app, image_url, config.enable_debug_logging)
                                .await
                        {
                            if config.enable_debug_logging {
                                println!(
                                    "[process_llm_request] Failed to copy image to clipboard: {}",
                                    e
                                );
                            }
                            // Don't fail the whole request, just log the error
                        } else {
                            // Restore window focus and paste
                            clipboard::restore_foreground_window()?;
                            clipboard::simulate_paste()?;
                        }
                    }
                    PasteBehavior::ClipboardMode => {
                        // Download and copy image to clipboard only
                        if let Err(e) =
                            copy_image_to_clipboard(&app, image_url, config.enable_debug_logging)
                                .await
                        {
                            if config.enable_debug_logging {
                                println!(
                                    "[process_llm_request] Failed to copy image to clipboard: {}",
                                    e
                                );
                            }
                        }
                    }
                    PasteBehavior::ReviewMode => {
                        // Do nothing, frontend handles it
                    }
                }
            }
        } else if let Some(content) = &response.content {
            // Handle text responses
            match config.paste_behavior {
                PasteBehavior::AutoPaste => {
                    // Copy to clipboard
                    use tauri_plugin_clipboard_manager::ClipboardExt;
                    app.clipboard()
                        .write_text(content.clone())
                        .map_err(|e| e.to_string())?;

                    // Restore window focus
                    clipboard::restore_foreground_window()?;

                    // Simulate Paste
                    clipboard::simulate_paste()?;
                }
                PasteBehavior::ClipboardMode => {
                    // Copy to clipboard
                    use tauri_plugin_clipboard_manager::ClipboardExt;
                    app.clipboard()
                        .write_text(content.clone())
                        .map_err(|e| e.to_string())?;
                }
                PasteBehavior::ReviewMode => {
                    // Do nothing, frontend handles it
                }
            }
        }
    }

    Ok(response)
}

/// Process LLM request with streaming (emits llm-stream-chunk events)
#[tauri::command]
async fn process_llm_request_streaming(
    app: AppHandle,
    state: State<'_, AppState>,
    request: LlmRequest,
) -> Result<LlmResponse, String> {
    use std::sync::atomic::Ordering;

    let (config, cancel_flag) = {
        let config = state.config.lock().map_err(|e| e.to_string())?;
        (config.clone(), state.cancel_flag.clone())
    };

    // Reset cancel flag
    cancel_flag.store(false, Ordering::Relaxed);

    // Check if operation supports streaming
    if !LlmService::supports_streaming(&request.operation_type) {
        // Fall back to non-streaming for non-text operations (images, audio, etc.)
        let service = LlmService::new(config.clone());
        let response = service.process_request(request).await;

        // Handle image responses for non-streaming operations
        if response.success && response.is_image {
            if let Some(image_url) = &response.image_url {
                match config.paste_behavior {
                    PasteBehavior::AutoPaste => {
                        if let Err(e) =
                            copy_image_to_clipboard(&app, image_url, config.enable_debug_logging)
                                .await
                        {
                            if config.enable_debug_logging {
                                println!("[process_llm_request_streaming] Failed to copy image to clipboard: {}", e);
                            }
                        } else {
                            // Restore window focus and paste
                            clipboard::restore_foreground_window()?;
                            clipboard::simulate_paste()?;
                        }
                    }
                    PasteBehavior::ClipboardMode => {
                        if let Err(e) =
                            copy_image_to_clipboard(&app, image_url, config.enable_debug_logging)
                                .await
                        {
                            if config.enable_debug_logging {
                                println!("[process_llm_request_streaming] Failed to copy image to clipboard: {}", e);
                            }
                        }
                    }
                    PasteBehavior::ReviewMode => {}
                }
            }
        }

        return Ok(response);
    }

    let service = LlmService::new(config.clone());
    let response = service
        .process_streaming_request(&request, &app, cancel_flag)
        .await;

    // Handle paste behavior for successful responses
    if response.success {
        if let Some(content) = &response.content {
            match config.paste_behavior {
                PasteBehavior::AutoPaste => {
                    use tauri_plugin_clipboard_manager::ClipboardExt;
                    app.clipboard()
                        .write_text(content.clone())
                        .map_err(|e| e.to_string())?;
                    clipboard::restore_foreground_window()?;
                    clipboard::simulate_paste()?;
                }
                PasteBehavior::ClipboardMode => {
                    use tauri_plugin_clipboard_manager::ClipboardExt;
                    app.clipboard()
                        .write_text(content.clone())
                        .map_err(|e| e.to_string())?;
                }
                PasteBehavior::ReviewMode => {
                    // Do nothing, frontend handles it
                }
            }
        }
    }

    Ok(response)
}

/// Cancel the current streaming request
#[tauri::command]
async fn cancel_llm_request(state: State<'_, AppState>) -> Result<(), String> {
    use std::sync::atomic::Ordering;
    state.cancel_flag.store(true, Ordering::Relaxed);
    Ok(())
}

#[tauri::command]
async fn get_models_from_api(state: State<'_, AppState>) -> Result<Vec<String>, String> {
    let config = {
        let config = state.config.lock().map_err(|e| e.to_string())?;
        config.clone()
    };

    let service = LlmService::new(config);
    let response = service.get_models().await?;

    Ok(response.data.into_iter().map(|m| m.id).collect())
}

#[tauri::command]
async fn test_connection(state: State<'_, AppState>) -> Result<(), String> {
    let config = {
        let config = state.config.lock().map_err(|e| e.to_string())?;
        config.clone()
    };

    let service = LlmService::new(config);
    service.test_connection().await
}

#[tauri::command]
async fn get_models_with_endpoint(
    state: State<'_, AppState>,
    api_base_url: String,
    api_key: Option<String>,
) -> Result<Vec<String>, String> {
    // Get the stored config for API key if not provided
    let stored_config = {
        let config = state.config.lock().map_err(|e| e.to_string())?;
        config.clone()
    };

    // Create a temporary config with the provided endpoint
    let mut config = Configuration::default();
    config.api_base_url = api_base_url;

    // Use provided API key if given (as plaintext), otherwise use the stored encrypted one
    if let Some(key) = api_key {
        if !key.is_empty() {
            config.plaintext_api_key = Some(key);
        } else {
            config.api_key = stored_config.api_key;
        }
    } else {
        config.api_key = stored_config.api_key;
    }

    let service = LlmService::new(config);
    let response = service.get_models().await?;

    Ok(response.data.into_iter().map(|m| m.id).collect())
}

#[tauri::command]
async fn test_connection_with_endpoint(
    state: State<'_, AppState>,
    api_base_url: String,
    api_key: Option<String>,
) -> Result<(), String> {
    // Get the stored config for API key if not provided
    let stored_config = {
        let config = state.config.lock().map_err(|e| e.to_string())?;
        config.clone()
    };

    // Create a temporary config with the provided endpoint
    let mut config = Configuration::default();
    config.api_base_url = api_base_url;

    // Use provided API key if given (as plaintext), otherwise use the stored encrypted one
    if let Some(key) = api_key {
        if !key.is_empty() {
            config.plaintext_api_key = Some(key);
        } else {
            config.api_key = stored_config.api_key;
        }
    } else {
        config.api_key = stored_config.api_key;
    }

    let service = LlmService::new(config);
    service.test_connection().await
}

// ============================================================================
// Text Capture Commands
// ============================================================================

#[tauri::command]
async fn simulate_copy() -> Result<(), String> {
    clipboard::simulate_copy()
}

#[tauri::command]
async fn simulate_paste() -> Result<(), String> {
    clipboard::simulate_paste()
}

#[tauri::command]
fn get_platform() -> String {
    clipboard::get_platform().to_string()
}

#[tauri::command]
async fn save_foreground_window() -> Result<bool, String> {
    clipboard::save_foreground_window()
}

#[tauri::command]
async fn restore_foreground_window() -> Result<bool, String> {
    clipboard::restore_foreground_window()
}

/// Capture selected text from the foreground window.
/// This is an atomic operation that:
/// 1. Saves the current foreground window handle (for later auto-paste)
/// 2. Saves the current clipboard content (for later restoration)
/// 3. Simulates Ctrl+C to copy the selected text
/// 4. Reads the new clipboard content
/// 5. Returns the captured text
///
/// Note: Window focus tracking is Windows-only. On macOS/Linux,
/// only clipboard capture works; focus restoration uses OS defaults.
#[tauri::command]
async fn capture_selected_text(
    app: AppHandle,
    state: State<'_, AppState>,
) -> Result<String, String> {
    use tauri_plugin_clipboard_manager::ClipboardExt;

    // Get configuration for delay settings
    let config = {
        let config = state.config.lock().map_err(|e| e.to_string())?;
        config.clone()
    };

    // Check if text selection is disabled
    if config.disable_text_selection {
        return Ok(String::new());
    }

    // 1. Save the current foreground window handle (Windows-only)
    clipboard::save_foreground_window()?;

    // 2. Save the current clipboard content for later restoration
    let original_clipboard = app.clipboard().read_text().unwrap_or_default();

    clipboard::save_clipboard_content(&original_clipboard);

    // 3. Simulate Ctrl+C with configurable delay
    clipboard::simulate_copy_with_delay(config.copy_delay_ms)?;

    // 4. Read the new clipboard content
    let captured_text = app.clipboard().read_text().unwrap_or_default();

    // If clipboard content hasn't changed, return empty (nothing was selected)
    if captured_text == original_clipboard {
        // Restore original clipboard since nothing was captured
        clipboard::clear_saved_clipboard_content();
        return Ok(String::new());
    }

    Ok(captured_text)
}

/// Restore clipboard to its original content before capture
#[tauri::command]
async fn restore_clipboard_content(app: AppHandle) -> Result<(), String> {
    use tauri_plugin_clipboard_manager::ClipboardExt;

    if let Some(original) = clipboard::get_saved_clipboard_content() {
        app.clipboard()
            .write_text(original)
            .map_err(|e| format!("Failed to restore clipboard: {}", e))?;
    }
    clipboard::clear_saved_clipboard_content();
    Ok(())
}

// ============================================================================
// Encryption Commands
// ============================================================================

#[tauri::command]
fn encrypt_api_key(key: String) -> Result<String, String> {
    encryption::encrypt(&key)
}

#[tauri::command]
fn decrypt_api_key(encrypted: String) -> Result<String, String> {
    encryption::decrypt(&encrypted)
}

#[tauri::command]
fn mask_api_key(key: String) -> String {
    encryption::mask_text(&key, 4)
}

// ============================================================================
// File Download Commands
// ============================================================================

/// Download an image from URL and save it to the media folder, returning the local path
/// This is used for persisting generated images to history
#[tauri::command]
async fn save_generated_image(
    state: State<'_, AppState>,
    image_url: String,
) -> Result<String, String> {
    let debug_logging = {
        let config = state.config.lock().map_err(|e| e.to_string())?;
        config.enable_debug_logging
    };

    if debug_logging {
        println!(
            "[save_generated_image] Starting download from: {}",
            image_url
        );
    }

    // Create HTTP client
    let client = reqwest::Client::builder()
        .timeout(std::time::Duration::from_secs(60))
        .build()
        .map_err(|e| format!("Failed to create HTTP client: {}", e))?;

    // Download the image
    let response = client
        .get(&image_url)
        .send()
        .await
        .map_err(|e| format!("Failed to download image: {}", e))?;

    let status = response.status();
    if !status.is_success() {
        let error_text = response.text().await.unwrap_or_default();
        return Err(format!(
            "Download failed with status {}: {}",
            status, error_text
        ));
    }

    let bytes = response
        .bytes()
        .await
        .map_err(|e| format!("Failed to read image bytes: {}", e))?;

    if debug_logging {
        println!("[save_generated_image] Downloaded {} bytes", bytes.len());
    }

    if bytes.is_empty() {
        return Err("Downloaded image is empty".to_string());
    }

    // Determine format from URL or default to png
    let format = if image_url.contains(".jpg") || image_url.contains(".jpeg") {
        "jpg"
    } else if image_url.contains(".webp") {
        "webp"
    } else {
        "png"
    };

    // Save to media folder
    let saved_path = HistoryManager::save_image(&bytes, format)?;

    if debug_logging {
        println!("[save_generated_image] Saved to: {}", saved_path);
    }

    Ok(saved_path)
}

/// Clear all media files in the media folder
#[tauri::command]
async fn clear_all_media() -> Result<u32, String> {
    let media_path = HistoryManager::get_media_path();
    let mut deleted_count = 0u32;

    // Read directory
    let entries = std::fs::read_dir(&media_path)
        .map_err(|e| format!("Failed to read media directory: {}", e))?;

    for entry in entries.flatten() {
        let path = entry.path();
        if path.is_file() {
            if std::fs::remove_file(&path).is_ok() {
                deleted_count += 1;
            }
        }
    }

    Ok(deleted_count)
}

/// Download an image from a URL and save it to a local file
/// This bypasses CORS restrictions by using the backend HTTP client
#[tauri::command]
async fn download_and_save_image(
    state: State<'_, AppState>,
    image_url: String,
    save_path: String,
) -> Result<(), String> {
    let debug_logging = {
        let config = state.config.lock().map_err(|e| e.to_string())?;
        config.enable_debug_logging
    };

    if debug_logging {
        println!("[download_and_save_image] Starting download...");
        println!("[download_and_save_image] URL: {}", image_url);
        println!("[download_and_save_image] Save path: {}", save_path);
    }

    // Create HTTP client with timeout
    let client = reqwest::Client::builder()
        .timeout(std::time::Duration::from_secs(60))
        .build()
        .map_err(|e| {
            let err = format!("Failed to create HTTP client: {}", e);
            if debug_logging {
                println!("[download_and_save_image] Error: {}", err);
            }
            err
        })?;

    // Download the image
    if debug_logging {
        println!("[download_and_save_image] Sending GET request...");
    }

    let response = client.get(&image_url).send().await.map_err(|e| {
        let err = format!("Failed to download image: {}", e);
        if debug_logging {
            println!("[download_and_save_image] Request error: {}", err);
        }
        err
    })?;

    let status = response.status();
    if debug_logging {
        println!("[download_and_save_image] Response status: {}", status);
    }

    if !status.is_success() {
        let error_text = response.text().await.unwrap_or_default();
        let err = format!("Download failed with status {}: {}", status, error_text);
        if debug_logging {
            println!("[download_and_save_image] Error response: {}", err);
        }
        return Err(err);
    }

    // Get the image bytes
    let bytes = response.bytes().await.map_err(|e| {
        let err = format!("Failed to read image bytes: {}", e);
        if debug_logging {
            println!("[download_and_save_image] Error reading bytes: {}", err);
        }
        err
    })?;

    if debug_logging {
        println!("[download_and_save_image] Downloaded {} bytes", bytes.len());
    }

    if bytes.is_empty() {
        let err = "Downloaded image is empty (0 bytes)".to_string();
        if debug_logging {
            println!("[download_and_save_image] Error: {}", err);
        }
        return Err(err);
    }

    // Write to file
    if debug_logging {
        println!("[download_and_save_image] Writing to file: {}", save_path);
    }

    std::fs::write(&save_path, &bytes).map_err(|e| {
        let err = format!("Failed to write image file: {} (path: {})", e, save_path);
        if debug_logging {
            println!("[download_and_save_image] Write error: {}", err);
        }
        err
    })?;

    if debug_logging {
        println!("[download_and_save_image] Successfully saved image!");
    }

    Ok(())
}

/// Copy an audio file from one location to another
/// This bypasses Tauri fs plugin restrictions by using direct std::fs::copy
/// Used for saving TTS-generated audio files to user-selected locations
#[tauri::command]
async fn copy_audio_file(
    state: State<'_, AppState>,
    source_path: String,
    dest_path: String,
) -> Result<(), String> {
    let debug_logging = {
        let config = state.config.lock().map_err(|e| e.to_string())?;
        config.enable_debug_logging
    };

    if debug_logging {
        println!("[copy_audio_file] Copying audio file...");
        println!("[copy_audio_file] Source: {}", source_path);
        println!("[copy_audio_file] Destination: {}", dest_path);
    }

    // Verify source file exists
    if !std::path::Path::new(&source_path).exists() {
        let err = format!("Source file does not exist: {}", source_path);
        if debug_logging {
            println!("[copy_audio_file] Error: {}", err);
        }
        return Err(err);
    }

    // Copy the file
    std::fs::copy(&source_path, &dest_path).map_err(|e| {
        let err = format!("Failed to copy audio file: {} (from {} to {})", e, source_path, dest_path);
        if debug_logging {
            println!("[copy_audio_file] Error: {}", err);
        }
        err
    })?;

    if debug_logging {
        println!("[copy_audio_file] Successfully copied audio file!");
    }

    Ok(())
}

/// Copy an image from URL to clipboard (exposed as Tauri command)
/// This is used by the frontend to copy images to clipboard
#[tauri::command]
async fn copy_image_to_clipboard_command(
    app: AppHandle,
    state: State<'_, AppState>,
    image_url: String,
) -> Result<(), String> {
    let debug_logging = {
        let config = state.config.lock().map_err(|e| e.to_string())?;
        config.enable_debug_logging
    };

    copy_image_to_clipboard(&app, &image_url, debug_logging).await
}

// ============================================================================
// Tray and Window Management
// ============================================================================

fn create_tray(app: &AppHandle) -> Result<(), Box<dyn std::error::Error>> {
    let open_item = MenuItem::with_id(app, "open", "Open AI Anywhere", true, None::<&str>)?;
    let settings_item = MenuItem::with_id(app, "settings", "Settings", true, None::<&str>)?;
    let about_item = MenuItem::with_id(app, "about", "About", true, None::<&str>)?;
    let quit_item = MenuItem::with_id(app, "quit", "Quit", true, None::<&str>)?;

    let menu = Menu::with_items(app, &[&open_item, &settings_item, &about_item, &quit_item])?;

    // Use the default app icon - Tauri handles icon embedding
    let _tray = TrayIconBuilder::new()
        .icon(
            app.default_window_icon()
                .cloned()
                .unwrap_or_else(|| tauri::image::Image::new_owned(vec![0, 0, 0, 255], 1, 1)),
        )
        .menu(&menu)
        .tooltip("AI Anywhere")
        .on_menu_event(|app, event| match event.id.as_ref() {
            "open" => {
                // Save the current foreground window before showing our window
                clipboard::save_foreground_window().ok();
                if let Some(window) = app.get_webview_window("main") {
                    window.show().ok();
                    window.set_focus().ok();
                }
            }
            "settings" => {
                // Save the current foreground window before showing our window
                clipboard::save_foreground_window().ok();
                if let Some(window) = app.get_webview_window("main") {
                    window.show().ok();
                    window.set_focus().ok();
                    window.emit("open-settings", ()).ok();
                }
            }
            "about" => {
                // Save the current foreground window before showing our window
                clipboard::save_foreground_window().ok();
                if let Some(window) = app.get_webview_window("main") {
                    window.show().ok();
                    window.set_focus().ok();
                    window.emit("open-about", ()).ok();
                }
            }
            "quit" => {
                app.exit(0);
            }
            _ => {}
        })
        .on_tray_icon_event(|tray, event| {
            if let TrayIconEvent::Click {
                button: MouseButton::Left,
                button_state: MouseButtonState::Up,
                ..
            } = event
            {
                // Save the current foreground window before showing our window
                clipboard::save_foreground_window().ok();
                let app = tray.app_handle();
                if let Some(window) = app.get_webview_window("main") {
                    window.show().ok();
                    window.set_focus().ok();
                }
            }
        })
        .build(app)?;

    Ok(())
}

// ============================================================================
// Application Entry Point
// ============================================================================

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    tauri::Builder::default()
        .plugin(tauri_plugin_single_instance::init(|app, _argv, _cwd| {
            if let Some(window) = app.get_webview_window("main") {
                let _ = window.show();
                let _ = window.set_focus();
            }
        }))
        .plugin(tauri_plugin_opener::init())
        .plugin(tauri_plugin_dialog::init())
        .plugin(tauri_plugin_clipboard_manager::init())
        .plugin(tauri_plugin_fs::init())
        .plugin(tauri_plugin_shell::init())
        .plugin(tauri_plugin_global_shortcut::Builder::new().build())
        .plugin(tauri_plugin_notification::init())
        .plugin(tauri_plugin_process::init())
        .plugin(tauri_plugin_window_state::Builder::default().build())
        .manage(AppState::new())
        .invoke_handler(tauri::generate_handler![
            // Configuration
            get_configuration,
            save_configuration,
            update_models,
            // Operations
            get_operations,
            // History
            get_history,
            save_history_entry,
            delete_history_entry,
            clear_history,
            cleanup_old_media,
            clear_all_media,
            // Custom Tasks
            get_custom_tasks,
            get_custom_task,
            create_custom_task,
            update_custom_task,
            delete_custom_task,
            export_custom_tasks,
            import_custom_tasks,
            // LLM
            process_llm_request,
            process_llm_request_streaming,
            cancel_llm_request,
            get_models_from_api,
            test_connection,
            get_models_with_endpoint,
            test_connection_with_endpoint,
            // Text capture
            simulate_copy,
            simulate_paste,
            get_platform,
            save_foreground_window,
            restore_foreground_window,
            capture_selected_text,
            restore_clipboard_content,
            // Encryption
            encrypt_api_key,
            decrypt_api_key,
            mask_api_key,
            // File Downloads
            download_and_save_image,
            save_generated_image,
            // Audio file operations
            copy_audio_file,
            // Image to Clipboard
            copy_image_to_clipboard_command,
        ])
        .setup(|app| {
            // Create system tray
            create_tray(app.handle())?;

            // Get retention days from config and spawn async cleanup task
            let state = app.state::<AppState>();
            let retention_days = {
                let config = state.config.lock().map_err(|e| e.to_string())?;
                config.media_retention_days
            };

            // Spawn async task to cleanup old media files on startup
            if retention_days > 0 {
                tauri::async_runtime::spawn(async move {
                    match HistoryManager::cleanup_old_media(retention_days).await {
                        Ok(deleted) if deleted > 0 => {
                            println!(
                                "[startup] Cleaned up {} old media files (retention: {} days)",
                                deleted, retention_days
                            );
                        }
                        Err(e) => {
                            eprintln!("[startup] Failed to cleanup old media: {}", e);
                        }
                        _ => {} // No files deleted
                    }
                });
            }

            Ok(())
        })
        .on_window_event(|window, event| {
            // Intercept window close to minimize to tray instead of closing
            if let tauri::WindowEvent::CloseRequested { api, .. } = event {
                // Prevent the default close behavior
                api.prevent_close();
                // Hide the window to the system tray
                window.hide().ok();
            }
        })
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}

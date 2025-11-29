//! Configuration module for AI Anywhere
//! Handles loading, saving, and managing application settings

use serde::{Deserialize, Serialize};
use std::collections::HashMap;
use std::fs;
use std::path::PathBuf;

use crate::encryption;
use crate::operations::get_default_system_prompts;

/// Paste behavior options
#[derive(Debug, Clone, Copy, PartialEq, Eq, Serialize, Deserialize, Default)]
#[serde(rename_all = "camelCase")]
pub enum PasteBehavior {
    AutoPaste,
    ClipboardMode,
    #[default]
    ReviewMode,
}

/// Application configuration
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct Configuration {
    /// Global hotkey string (e.g., "Ctrl+Space")
    #[serde(default = "default_hotkey")]
    pub hotkey: String,
    
    /// API base URL for OpenAI-compatible endpoints
    #[serde(default = "default_api_base_url")]
    pub api_base_url: String,
    
    /// Encrypted API key
    #[serde(default)]
    pub api_key: String,
    
    /// Plaintext API key (not persisted, used for testing with unsaved keys)
    #[serde(skip)]
    pub plaintext_api_key: Option<String>,
    
    /// Text/chat model name
    #[serde(default)]
    pub llm_model: String,
    
    /// Image generation model name
    #[serde(default)]
    pub image_model: String,
    
    /// Audio transcription model name
    #[serde(default)]
    pub audio_model: String,
    
    /// Text-to-speech model name
    #[serde(default = "default_tts_model")]
    pub tts_model: String,
    
    /// Paste behavior setting
    #[serde(default)]
    pub paste_behavior: PasteBehavior,
    
    /// Disable automatic text selection capture
    #[serde(default)]
    pub disable_text_selection: bool,
    
    /// Disable thinking mode for LLM models
    #[serde(default = "default_disable_thinking")]
    pub disable_thinking: bool,
    
    /// Enable debug logging for API requests
    #[serde(default)]
    pub enable_debug_logging: bool,
    
    /// Custom system prompts for each operation type
    #[serde(default = "get_default_system_prompts")]
    pub system_prompts: HashMap<String, String>,
    
    /// Cached list of available text models
    #[serde(default)]
    pub models: Vec<String>,
    
    /// Cached list of available image models
    #[serde(default)]
    pub image_models: Vec<String>,
    
    /// Cached list of available audio models
    #[serde(default)]
    pub audio_models: Vec<String>,
}

fn default_hotkey() -> String {
    "Ctrl+Space".to_string()
}

fn default_api_base_url() -> String {
    "https://api.openai.com/v1".to_string()
}

fn default_tts_model() -> String {
    "tts-1-hd".to_string()
}

fn default_disable_thinking() -> bool {
    true
}

impl Default for Configuration {
    fn default() -> Self {
        Self {
            hotkey: default_hotkey(),
            api_base_url: default_api_base_url(),
            api_key: String::new(),
            plaintext_api_key: None,
            llm_model: String::new(),
            image_model: String::new(),
            audio_model: String::new(),
            tts_model: default_tts_model(),
            paste_behavior: PasteBehavior::default(),
            disable_text_selection: false,
            disable_thinking: true,
            enable_debug_logging: false,
            system_prompts: get_default_system_prompts(),
            models: Vec::new(),
            image_models: Vec::new(),
            audio_models: Vec::new(),
        }
    }
}

impl Configuration {
    /// Get the configuration file path
    pub fn get_config_path() -> PathBuf {
        let config_dir = dirs::config_dir()
            .unwrap_or_else(|| PathBuf::from("."))
            .join("ai-anywhere");
        
        fs::create_dir_all(&config_dir).ok();
        config_dir.join("config.json")
    }
    
    /// Load configuration from file
    pub fn load() -> Result<Self, String> {
        let config_path = Self::get_config_path();
        
        if !config_path.exists() {
            let default_config = Configuration::default();
            default_config.save()?;
            return Ok(default_config);
        }
        
        let content = fs::read_to_string(&config_path)
            .map_err(|e| format!("Failed to read config file: {}", e))?;
        
        let mut config: Configuration = serde_json::from_str(&content)
            .map_err(|e| format!("Failed to parse config file: {}", e))?;
        
        // Ensure system prompts have all default keys
        let defaults = get_default_system_prompts();
        for (key, value) in defaults {
            config.system_prompts.entry(key).or_insert(value);
        }
        
        Ok(config)
    }
    
    /// Save configuration to file
    pub fn save(&self) -> Result<(), String> {
        let config_path = Self::get_config_path();
        
        let content = serde_json::to_string_pretty(self)
            .map_err(|e| format!("Failed to serialize config: {}", e))?;
        
        fs::write(&config_path, content)
            .map_err(|e| format!("Failed to write config file: {}", e))?;
        
        Ok(())
    }
    
    /// Get decrypted API key
    pub fn get_api_key(&self) -> String {
        // If plaintext API key is set (from form input), use it directly
        if let Some(ref plaintext) = self.plaintext_api_key {
            return plaintext.clone();
        }
        // Otherwise decrypt the stored encrypted key
        if self.api_key.is_empty() {
            return String::new();
        }
        encryption::decrypt(&self.api_key).unwrap_or_default()
    }
    
    /// Set API key (encrypts before storing)
    pub fn set_api_key(&mut self, key: &str) {
        if key.is_empty() {
            self.api_key = String::new();
        } else {
            self.api_key = encryption::encrypt(key).unwrap_or_default();
        }
    }
}

/// Configuration for frontend (with decrypted API key masked)
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct ConfigurationDto {
    pub hotkey: String,
    pub api_base_url: String,
    pub api_key_set: bool,
    pub llm_model: String,
    pub image_model: String,
    pub audio_model: String,
    pub tts_model: String,
    pub paste_behavior: PasteBehavior,
    pub disable_text_selection: bool,
    pub disable_thinking: bool,
    pub enable_debug_logging: bool,
    pub models: Vec<String>,
    pub image_models: Vec<String>,
    pub audio_models: Vec<String>,
}

impl From<&Configuration> for ConfigurationDto {
    fn from(config: &Configuration) -> Self {
        Self {
            hotkey: config.hotkey.clone(),
            api_base_url: config.api_base_url.clone(),
            api_key_set: !config.api_key.is_empty(),
            llm_model: config.llm_model.clone(),
            image_model: config.image_model.clone(),
            audio_model: config.audio_model.clone(),
            tts_model: config.tts_model.clone(),
            paste_behavior: config.paste_behavior,
            disable_text_selection: config.disable_text_selection,
            disable_thinking: config.disable_thinking,
            enable_debug_logging: config.enable_debug_logging,
            models: config.models.clone(),
            image_models: config.image_models.clone(),
            audio_models: config.audio_models.clone(),
        }
    }
}

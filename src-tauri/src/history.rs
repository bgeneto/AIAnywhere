//! History module for AI Anywhere
//! Manages conversation history storage and retrieval

use serde::{Deserialize, Serialize};
use std::collections::HashMap;
use std::fs;
use std::path::PathBuf;
use uuid::Uuid;

/// Response data stored in history
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct HistoryEntryResponse {
    pub text: Option<String>,
    pub image_path: Option<String>,
    pub audio_path: Option<String>,
}

/// A single history entry
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct HistoryEntry {
    pub id: String,
    pub timestamp: String,
    pub operation_type: String,
    pub operation_name: String,
    pub prompt: String,
    pub options: HashMap<String, String>,
    pub response: HistoryEntryResponse,
}

impl HistoryEntry {
    pub fn new(
        operation_type: String,
        operation_name: String,
        prompt: String,
        options: HashMap<String, String>,
        response: HistoryEntryResponse,
    ) -> Self {
        Self {
            id: Uuid::new_v4().to_string(),
            timestamp: chrono::Utc::now().to_rfc3339(),
            operation_type,
            operation_name,
            prompt,
            options,
            response,
        }
    }

    /// Check if this entry matches a search query (searches prompt and response text)
    pub fn matches_search(&self, query: &str) -> bool {
        let query_lower = query.to_lowercase();
        
        // Search in prompt
        if self.prompt.to_lowercase().contains(&query_lower) {
            return true;
        }
        
        // Search in response text
        if let Some(ref text) = self.response.text {
            if text.to_lowercase().contains(&query_lower) {
                return true;
            }
        }
        
        false
    }
}

/// History manager for loading and saving history
pub struct HistoryManager;

impl HistoryManager {
    /// Get the history file path
    pub fn get_history_path() -> PathBuf {
        let config_dir = dirs::config_dir()
            .unwrap_or_else(|| PathBuf::from("."))
            .join("ai-anywhere");
        
        fs::create_dir_all(&config_dir).ok();
        config_dir.join("history.json")
    }

    /// Get the media folder path
    pub fn get_media_path() -> PathBuf {
        let config_dir = dirs::config_dir()
            .unwrap_or_else(|| PathBuf::from("."))
            .join("ai-anywhere")
            .join("media");
        
        fs::create_dir_all(&config_dir).ok();
        config_dir
    }

    /// Load history from file
    pub fn load() -> Result<Vec<HistoryEntry>, String> {
        let history_path = Self::get_history_path();
        
        if !history_path.exists() {
            return Ok(Vec::new());
        }
        
        let content = fs::read_to_string(&history_path)
            .map_err(|e| format!("Failed to read history file: {}", e))?;
        
        if content.trim().is_empty() {
            return Ok(Vec::new());
        }
        
        let history: Vec<HistoryEntry> = serde_json::from_str(&content)
            .map_err(|e| format!("Failed to parse history file: {}", e))?;
        
        Ok(history)
    }

    /// Save history to file
    pub fn save(history: &[HistoryEntry]) -> Result<(), String> {
        let history_path = Self::get_history_path();
        
        let content = serde_json::to_string_pretty(history)
            .map_err(|e| format!("Failed to serialize history: {}", e))?;
        
        fs::write(&history_path, content)
            .map_err(|e| format!("Failed to write history file: {}", e))?;
        
        Ok(())
    }

    /// Add a new entry to history (respects limit)
    pub fn add_entry(entry: HistoryEntry, limit: usize) -> Result<(), String> {
        let mut history = Self::load()?;
        
        // Add new entry at the beginning
        history.insert(0, entry);
        
        // Enforce limit (remove oldest entries)
        if limit > 0 && history.len() > limit {
            // Get entries to remove (will also delete their media files)
            let entries_to_remove: Vec<HistoryEntry> = history.drain(limit..).collect();
            
            // Delete associated media files
            for entry in entries_to_remove {
                Self::delete_entry_media(&entry);
            }
        }
        
        Self::save(&history)
    }

    /// Delete a specific entry by ID
    pub fn delete_entry(id: &str) -> Result<(), String> {
        let mut history = Self::load()?;
        
        // Find and remove the entry
        if let Some(pos) = history.iter().position(|e| e.id == id) {
            let entry = history.remove(pos);
            Self::delete_entry_media(&entry);
        }
        
        Self::save(&history)
    }

    /// Delete media files associated with an entry
    fn delete_entry_media(entry: &HistoryEntry) {
        if let Some(ref path) = entry.response.image_path {
            fs::remove_file(path).ok();
        }
        if let Some(ref path) = entry.response.audio_path {
            fs::remove_file(path).ok();
        }
    }

    /// Clear all history
    pub fn clear() -> Result<(), String> {
        let history = Self::load()?;
        
        // Delete all media files
        for entry in &history {
            Self::delete_entry_media(entry);
        }
        
        // Save empty history
        Self::save(&Vec::new())
    }

    /// Search history entries
    pub fn search(query: &str) -> Result<Vec<HistoryEntry>, String> {
        let history = Self::load()?;
        
        if query.trim().is_empty() {
            return Ok(history);
        }
        
        let filtered: Vec<HistoryEntry> = history
            .into_iter()
            .filter(|e| e.matches_search(query))
            .collect();
        
        Ok(filtered)
    }

    /// Save image to media folder and return the path
    pub fn save_image(data: &[u8], format: &str) -> Result<String, String> {
        let media_path = Self::get_media_path();
        let timestamp = chrono::Utc::now().format("%Y%m%d_%H%M%S");
        let filename = format!("img_{}_{}.{}", timestamp, Uuid::new_v4().to_string()[..8].to_string(), format);
        let file_path = media_path.join(&filename);
        
        fs::write(&file_path, data)
            .map_err(|e| format!("Failed to save image: {}", e))?;
        
        Ok(file_path.to_string_lossy().to_string())
    }

    /// Save audio to media folder and return the path
    pub fn save_audio(data: &[u8], format: &str) -> Result<String, String> {
        let media_path = Self::get_media_path();
        let timestamp = chrono::Utc::now().format("%Y%m%d_%H%M%S");
        let filename = format!("audio_{}_{}.{}", timestamp, Uuid::new_v4().to_string()[..8].to_string(), format);
        let file_path = media_path.join(&filename);
        
        fs::write(&file_path, data)
            .map_err(|e| format!("Failed to save audio: {}", e))?;
        
        Ok(file_path.to_string_lossy().to_string())
    }

    /// Clean up old media files based on retention policy
    pub fn cleanup_old_media(retention_days: u32) -> Result<u32, String> {
        if retention_days == 0 {
            return Ok(0); // Disabled
        }

        let media_path = Self::get_media_path();
        let cutoff = chrono::Utc::now() - chrono::Duration::days(retention_days as i64);
        let mut deleted_count = 0u32;

        // Read directory
        let entries = fs::read_dir(&media_path)
            .map_err(|e| format!("Failed to read media directory: {}", e))?;

        for entry in entries.flatten() {
            let path = entry.path();
            if path.is_file() {
                // Get file metadata to check modification time
                if let Ok(metadata) = fs::metadata(&path) {
                    if let Ok(modified) = metadata.modified() {
                        let modified_datetime: chrono::DateTime<chrono::Utc> = modified.into();
                        if modified_datetime < cutoff {
                            if fs::remove_file(&path).is_ok() {
                                deleted_count += 1;
                            }
                        }
                    }
                }
            }
        }

        // Also update history to remove entries with deleted media
        let mut history = Self::load()?;
        history.retain(|entry| {
            // Keep entries without media or with existing media
            let image_exists = entry.response.image_path.as_ref()
                .map(|p| std::path::Path::new(p).exists())
                .unwrap_or(true);
            let audio_exists = entry.response.audio_path.as_ref()
                .map(|p| std::path::Path::new(p).exists())
                .unwrap_or(true);
            image_exists && audio_exists
        });
        Self::save(&history)?;

        Ok(deleted_count)
    }
}

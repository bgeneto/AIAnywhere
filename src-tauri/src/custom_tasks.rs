//! Custom Tasks module for AI Anywhere
//! Manages user-created custom AI tasks with prompt templates

use regex::Regex;
use serde::{Deserialize, Serialize};
use std::fs;
use std::path::PathBuf;
use uuid::Uuid;

/// Option types for custom task form controls
#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
#[serde(rename_all = "camelCase")]
pub enum CustomOptionType {
    Select,
    Text,
    Number,
    Textarea,
    Checkbox,
}

/// A single option for a custom task
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct CustomTaskOption {
    pub key: String,
    pub name: String,
    #[serde(rename = "type")]
    pub option_type: CustomOptionType,
    pub values: Vec<String>, // For select type
    pub default_value: String,
    pub required: bool,
}

/// A user-created custom task
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct CustomTask {
    pub id: String,
    pub name: String,
    pub description: String,
    pub system_prompt: String,
    pub options: Vec<CustomTaskOption>,
    pub created_at: String,
    pub updated_at: String,
}

impl CustomTask {
    pub fn new(
        name: String,
        description: String,
        system_prompt: String,
        options: Vec<CustomTaskOption>,
    ) -> Self {
        let now = chrono::Utc::now().to_rfc3339();
        Self {
            id: Uuid::new_v4().to_string(),
            name,
            description,
            system_prompt,
            options,
            created_at: now.clone(),
            updated_at: now,
        }
    }

    /// Validate that the system prompt contains all required placeholders for defined options
    pub fn validate(&self) -> Result<(), String> {
        // Extract placeholders from system prompt using regex
        let placeholder_regex = Regex::new(r"\{(\w+)\}").unwrap();
        let prompt_placeholders: Vec<String> = placeholder_regex
            .captures_iter(&self.system_prompt)
            .map(|cap| cap[1].to_string())
            .collect();

        // Check that all option keys have corresponding placeholders
        let mut missing_placeholders = Vec::new();
        for option in &self.options {
            if !prompt_placeholders.contains(&option.key) {
                missing_placeholders.push(option.key.clone());
            }
        }

        if !missing_placeholders.is_empty() {
            return Err(format!(
                "System prompt is missing placeholders for options: {}. Add {{{}}} to your prompt.",
                missing_placeholders.join(", "),
                missing_placeholders.join("}}, {{")
            ));
        }

        // Check that all placeholders have corresponding options
        let option_keys: Vec<String> = self.options.iter().map(|o| o.key.clone()).collect();
        let mut extra_placeholders = Vec::new();
        for placeholder in &prompt_placeholders {
            if !option_keys.contains(placeholder) {
                extra_placeholders.push(placeholder.clone());
            }
        }

        if !extra_placeholders.is_empty() {
            return Err(format!(
                "System prompt has placeholders without matching options: {}. Create options for these or remove them from the prompt.",
                extra_placeholders.join(", ")
            ));
        }

        // Validate task name is not empty
        if self.name.trim().is_empty() {
            return Err("Task name cannot be empty".to_string());
        }

        // Validate system prompt is not empty
        if self.system_prompt.trim().is_empty() {
            return Err("System prompt cannot be empty".to_string());
        }

        Ok(())
    }

    /// Build the final system prompt by replacing placeholders with option values
    #[allow(dead_code)]
    pub fn build_prompt(&self, options: &std::collections::HashMap<String, String>) -> String {
        let mut prompt = self.system_prompt.clone();
        for (key, value) in options {
            prompt = prompt.replace(&format!("{{{}}}", key), value);
        }
        prompt
    }
}

/// Custom tasks manager for loading and saving tasks
pub struct CustomTasksManager;

impl CustomTasksManager {
    /// Get the app data directory path (cross-platform)
    /// - Windows: C:\Users\<user>\AppData\Roaming\ai-anywhere
    /// - macOS: ~/Library/Application Support/ai-anywhere
    /// - Linux: ~/.local/share/ai-anywhere
    fn get_app_data_dir() -> PathBuf {
        dirs::data_dir()
            .unwrap_or_else(|| PathBuf::from("."))
            .join("ai-anywhere")
    }

    /// Get the custom tasks file path
    pub fn get_tasks_path() -> PathBuf {
        let data_dir = Self::get_app_data_dir();

        if let Err(e) = fs::create_dir_all(&data_dir) {
            eprintln!("[CustomTasksManager] Failed to create data directory {:?}: {}", data_dir, e);
        }
        data_dir.join("custom_tasks.json")
    }

    /// Load custom tasks from file
    pub fn load() -> Result<Vec<CustomTask>, String> {
        let tasks_path = Self::get_tasks_path();

        if !tasks_path.exists() {
            return Ok(Vec::new());
        }

        let content = fs::read_to_string(&tasks_path)
            .map_err(|e| format!("Failed to read custom tasks file: {}", e))?;

        if content.trim().is_empty() {
            return Ok(Vec::new());
        }

        let tasks: Vec<CustomTask> = serde_json::from_str(&content)
            .map_err(|e| format!("Failed to parse custom tasks file: {}", e))?;

        Ok(tasks)
    }

    /// Save custom tasks to file
    pub fn save(tasks: &[CustomTask]) -> Result<(), String> {
        let tasks_path = Self::get_tasks_path();

        let content = serde_json::to_string_pretty(tasks)
            .map_err(|e| format!("Failed to serialize custom tasks: {}", e))?;

        fs::write(&tasks_path, content)
            .map_err(|e| format!("Failed to write custom tasks file: {}", e))?;

        Ok(())
    }

    /// Create a new custom task
    pub fn create(task: CustomTask) -> Result<CustomTask, String> {
        // Validate the task
        task.validate()?;

        let mut tasks = Self::load()?;
        tasks.push(task.clone());
        Self::save(&tasks)?;

        Ok(task)
    }

    /// Update an existing custom task
    pub fn update(id: &str, mut task: CustomTask) -> Result<CustomTask, String> {
        // Validate the task
        task.validate()?;

        let mut tasks = Self::load()?;

        if let Some(existing) = tasks.iter_mut().find(|t| t.id == id) {
            task.id = id.to_string();
            task.created_at = existing.created_at.clone();
            task.updated_at = chrono::Utc::now().to_rfc3339();
            *existing = task.clone();
            Self::save(&tasks)?;
            Ok(task)
        } else {
            Err(format!("Custom task with id '{}' not found", id))
        }
    }

    /// Delete a custom task by ID
    pub fn delete(id: &str) -> Result<(), String> {
        let mut tasks = Self::load()?;

        if tasks.iter().any(|t| t.id == id) {
            tasks.retain(|t| t.id != id);
            Self::save(&tasks)?;
            Ok(())
        } else {
            Err(format!("Custom task with id '{}' not found", id))
        }
    }

    /// Get a single custom task by ID
    pub fn get(id: &str) -> Result<Option<CustomTask>, String> {
        let tasks = Self::load()?;
        Ok(tasks.into_iter().find(|t| t.id == id))
    }

    /// Export tasks to JSON string
    pub fn export() -> Result<String, String> {
        let tasks = Self::load()?;
        serde_json::to_string_pretty(&tasks).map_err(|e| format!("Failed to export tasks: {}", e))
    }

    /// Import tasks from JSON string (merges with existing, updates by name)
    pub fn import(json: &str) -> Result<usize, String> {
        let imported: Vec<CustomTask> = serde_json::from_str(json)
            .map_err(|e| format!("Failed to parse import data: {}", e))?;

        let mut existing = Self::load()?;
        let mut imported_count = 0;

        for mut task in imported {
            // Validate each imported task
            if let Err(e) = task.validate() {
                println!("Skipping invalid task '{}': {}", task.name, e);
                continue;
            }

            // Check if task with same name exists
            if let Some(existing_task) = existing.iter_mut().find(|t| t.name == task.name) {
                // Update existing task
                task.id = existing_task.id.clone();
                task.created_at = existing_task.created_at.clone();
                task.updated_at = chrono::Utc::now().to_rfc3339();
                *existing_task = task;
            } else {
                // Add new task with new ID
                task.id = Uuid::new_v4().to_string();
                task.created_at = chrono::Utc::now().to_rfc3339();
                task.updated_at = task.created_at.clone();
                existing.push(task);
            }
            imported_count += 1;
        }

        Self::save(&existing)?;
        Ok(imported_count)
    }
}

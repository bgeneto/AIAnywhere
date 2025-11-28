//! AI Anywhere - Universal AI Assistant
//! Tauri 2.0 backend library

mod clipboard;
mod config;
mod encryption;
mod llm;
mod operations;
mod text;

use config::{Configuration, ConfigurationDto, PasteBehavior};
use llm::{LlmRequest, LlmResponse, LlmService};
use operations::Operation;
use serde::{Deserialize, Serialize};
use std::sync::Mutex;
use tauri::{
    menu::{Menu, MenuItem},
    tray::{MouseButton, MouseButtonState, TrayIconBuilder, TrayIconEvent},
    AppHandle, Emitter, Manager, State,
};

/// Application state
pub struct AppState {
    config: Mutex<Configuration>,
}

impl AppState {
    fn new() -> Self {
        let config = Configuration::load().unwrap_or_default();
        Self {
            config: Mutex::new(config),
        }
    }
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
    disable_thinking: bool,
    enable_debug_logging: bool,
    models: Vec<String>,
    image_models: Vec<String>,
    audio_models: Vec<String>,
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
    config.disable_thinking = request.disable_thinking;
    config.enable_debug_logging = request.enable_debug_logging;
    config.models = request.models;
    config.image_models = request.image_models;
    config.audio_models = request.audio_models;

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
async fn get_operations(state: State<'_, AppState>) -> Result<Vec<Operation>, String> {
    let config = state.config.lock().map_err(|e| e.to_string())?;
    Ok(operations::get_default_operations(&config.system_prompts))
}

// ============================================================================
// LLM Commands
// ============================================================================

#[tauri::command]
async fn process_llm_request(
    state: State<'_, AppState>,
    request: LlmRequest,
) -> Result<LlmResponse, String> {
    let config = {
        let config = state.config.lock().map_err(|e| e.to_string())?;
        config.clone()
    };

    let service = LlmService::new(config);
    Ok(service.process_request(request).await)
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
        .icon(app.default_window_icon().cloned().unwrap_or_else(|| {
            tauri::image::Image::new_owned(vec![0, 0, 0, 255], 1, 1)
        }))
        .menu(&menu)
        .tooltip("AI Anywhere")
        .on_menu_event(|app, event| match event.id.as_ref() {
            "open" => {
                if let Some(window) = app.get_webview_window("main") {
                    window.show().ok();
                    window.set_focus().ok();
                }
            }
            "settings" => {
                if let Some(window) = app.get_webview_window("main") {
                    window.show().ok();
                    window.set_focus().ok();
                    window.emit("open-settings", ()).ok();
                }
            }
            "about" => {
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
        .plugin(tauri_plugin_opener::init())
        .plugin(tauri_plugin_dialog::init())
        .plugin(tauri_plugin_clipboard_manager::init())
        .plugin(tauri_plugin_fs::init())
        .plugin(tauri_plugin_shell::init())
        .plugin(tauri_plugin_global_shortcut::Builder::new().build())
        .plugin(tauri_plugin_notification::init())
        .manage(AppState::new())
        .invoke_handler(tauri::generate_handler![
            // Configuration
            get_configuration,
            save_configuration,
            update_models,
            // Operations
            get_operations,
            // LLM
            process_llm_request,
            get_models_from_api,
            test_connection,
            get_models_with_endpoint,
            test_connection_with_endpoint,
            // Text capture
            simulate_copy,
            simulate_paste,
            get_platform,
            // Encryption
            encrypt_api_key,
            decrypt_api_key,
            mask_api_key,
        ])
        .setup(|app| {
            // Create system tray
            create_tray(app.handle())?;

            Ok(())
        })
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}

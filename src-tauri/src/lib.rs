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
    enable_debug_logging: bool,
    copy_delay_ms: u64,
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
    config.enable_debug_logging = request.enable_debug_logging;
    config.copy_delay_ms = request.copy_delay_ms;
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
        if let Some(content) = &response.content {
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
    let original_clipboard = app
        .clipboard()
        .read_text()
        .unwrap_or_default();
    
    clipboard::save_clipboard_content(&original_clipboard);

    // 3. Simulate Ctrl+C with configurable delay
    clipboard::simulate_copy_with_delay(config.copy_delay_ms)?;

    // 4. Read the new clipboard content
    let captured_text = app
        .clipboard()
        .read_text()
        .unwrap_or_default();

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
        .plugin(tauri_plugin_opener::init())
        .plugin(tauri_plugin_dialog::init())
        .plugin(tauri_plugin_clipboard_manager::init())
        .plugin(tauri_plugin_fs::init())
        .plugin(tauri_plugin_shell::init())
        .plugin(tauri_plugin_global_shortcut::Builder::new().build())
        .plugin(tauri_plugin_notification::init())
        .plugin(tauri_plugin_process::init())
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
            save_foreground_window,
            restore_foreground_window,
            capture_selected_text,
            restore_clipboard_content,
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

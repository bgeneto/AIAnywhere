//! Platform-specific text capture module for AI Anywhere
//! Implements cross-platform text selection capture using clipboard simulation
//!
//! Key features:
//! - Text capture via keyboard simulation (Ctrl+C / Cmd+C)
//! - Window focus tracking for auto-paste functionality (Windows-only)
//! - Clipboard preservation to restore user's original clipboard content

use std::sync::atomic::{AtomicIsize, Ordering};
use std::sync::Mutex;
use std::thread;
use std::time::Duration;

/// Global storage for the foreground window handle before our app was activated
static PREVIOUS_FOREGROUND_WINDOW: AtomicIsize = AtomicIsize::new(0);

/// Global storage for preserving clipboard content during capture operations
static SAVED_CLIPBOARD_CONTENT: Mutex<Option<String>> = Mutex::new(None);

/// Platform detection
pub fn get_platform() -> &'static str {
    #[cfg(target_os = "windows")]
    {
        "windows"
    }
    #[cfg(target_os = "macos")]
    {
        "macos"
    }
    #[cfg(target_os = "linux")]
    {
        "linux"
    }
    #[cfg(not(any(target_os = "windows", target_os = "macos", target_os = "linux")))]
    {
        "unknown"
    }
}

/// Simulate Ctrl+C (or Cmd+C on macOS) to copy selected text
/// This is the most portable approach across platforms
pub fn simulate_copy() -> Result<(), String> {
    #[cfg(any(target_os = "windows", target_os = "macos", target_os = "linux"))]
    {
        use enigo::{Enigo, Key, Keyboard, Settings};

        let mut enigo = Enigo::new(&Settings::default())
            .map_err(|e| format!("Failed to initialize keyboard simulation: {}", e))?;

        // Small delay to ensure the system is ready
        thread::sleep(Duration::from_millis(50));

        #[cfg(target_os = "macos")]
        {
            // macOS uses Command+C
            enigo
                .key(Key::Meta, enigo::Direction::Press)
                .map_err(|e| format!("Failed to press Meta key: {}", e))?;
            enigo
                .key(Key::Unicode('c'), enigo::Direction::Click)
                .map_err(|e| format!("Failed to press C key: {}", e))?;
            enigo
                .key(Key::Meta, enigo::Direction::Release)
                .map_err(|e| format!("Failed to release Meta key: {}", e))?;
        }

        #[cfg(not(target_os = "macos"))]
        {
            // Windows and Linux use Ctrl+C
            enigo
                .key(Key::Control, enigo::Direction::Press)
                .map_err(|e| format!("Failed to press Ctrl key: {}", e))?;
            enigo
                .key(Key::Unicode('c'), enigo::Direction::Click)
                .map_err(|e| format!("Failed to press C key: {}", e))?;
            enigo
                .key(Key::Control, enigo::Direction::Release)
                .map_err(|e| format!("Failed to release Ctrl key: {}", e))?;
        }

        // Wait for clipboard to be updated
        thread::sleep(Duration::from_millis(100));

        Ok(())
    }

    #[cfg(not(any(target_os = "windows", target_os = "macos", target_os = "linux")))]
    {
        Err("Keyboard simulation not supported on this platform".to_string())
    }
}

/// Simulate Ctrl+V (or Cmd+V on macOS) to paste text
pub fn simulate_paste() -> Result<(), String> {
    #[cfg(any(target_os = "windows", target_os = "macos", target_os = "linux"))]
    {
        use enigo::{Enigo, Key, Keyboard, Settings};

        let mut enigo = Enigo::new(&Settings::default())
            .map_err(|e| format!("Failed to initialize keyboard simulation: {}", e))?;

        // Small delay to ensure the system is ready
        thread::sleep(Duration::from_millis(50));

        #[cfg(target_os = "macos")]
        {
            // macOS uses Command+V
            enigo
                .key(Key::Meta, enigo::Direction::Press)
                .map_err(|e| format!("Failed to press Meta key: {}", e))?;
            enigo
                .key(Key::Unicode('v'), enigo::Direction::Click)
                .map_err(|e| format!("Failed to press V key: {}", e))?;
            enigo
                .key(Key::Meta, enigo::Direction::Release)
                .map_err(|e| format!("Failed to release Meta key: {}", e))?;
        }

        #[cfg(not(target_os = "macos"))]
        {
            // Windows and Linux use Ctrl+V
            enigo
                .key(Key::Control, enigo::Direction::Press)
                .map_err(|e| format!("Failed to press Ctrl key: {}", e))?;
            enigo
                .key(Key::Unicode('v'), enigo::Direction::Click)
                .map_err(|e| format!("Failed to press V key: {}", e))?;
            enigo
                .key(Key::Control, enigo::Direction::Release)
                .map_err(|e| format!("Failed to release Ctrl key: {}", e))?;
        }

        // Wait for paste to complete
        thread::sleep(Duration::from_millis(100));

        Ok(())
    }

    #[cfg(not(any(target_os = "windows", target_os = "macos", target_os = "linux")))]
    {
        Err("Keyboard simulation not supported on this platform".to_string())
    }
}

/// Simulate Ctrl+C with configurable delay
/// Returns the captured text from clipboard after the copy operation
pub fn simulate_copy_with_delay(delay_ms: u64) -> Result<(), String> {
    #[cfg(any(target_os = "windows", target_os = "macos", target_os = "linux"))]
    {
        use enigo::{Enigo, Key, Keyboard, Settings};

        let mut enigo = Enigo::new(&Settings::default())
            .map_err(|e| format!("Failed to initialize keyboard simulation: {}", e))?;

        // Small delay to ensure the system is ready
        thread::sleep(Duration::from_millis(50));

        #[cfg(target_os = "macos")]
        {
            // macOS uses Command+C
            enigo
                .key(Key::Meta, enigo::Direction::Press)
                .map_err(|e| format!("Failed to press Meta key: {}", e))?;
            enigo
                .key(Key::Unicode('c'), enigo::Direction::Click)
                .map_err(|e| format!("Failed to press C key: {}", e))?;
            enigo
                .key(Key::Meta, enigo::Direction::Release)
                .map_err(|e| format!("Failed to release Meta key: {}", e))?;
        }

        #[cfg(not(target_os = "macos"))]
        {
            // Windows and Linux use Ctrl+C
            enigo
                .key(Key::Control, enigo::Direction::Press)
                .map_err(|e| format!("Failed to press Ctrl key: {}", e))?;
            enigo
                .key(Key::Unicode('c'), enigo::Direction::Click)
                .map_err(|e| format!("Failed to press C key: {}", e))?;
            enigo
                .key(Key::Control, enigo::Direction::Release)
                .map_err(|e| format!("Failed to release Ctrl key: {}", e))?;
        }

        // Wait for clipboard to be updated with configurable delay
        thread::sleep(Duration::from_millis(delay_ms));

        Ok(())
    }

    #[cfg(not(any(target_os = "windows", target_os = "macos", target_os = "linux")))]
    {
        Err("Keyboard simulation not supported on this platform".to_string())
    }
}

/// Save current clipboard content for later restoration
pub fn save_clipboard_content(content: &str) {
    if let Ok(mut saved) = SAVED_CLIPBOARD_CONTENT.lock() {
        *saved = Some(content.to_string());
    }
}

/// Get saved clipboard content
pub fn get_saved_clipboard_content() -> Option<String> {
    if let Ok(saved) = SAVED_CLIPBOARD_CONTENT.lock() {
        saved.clone()
    } else {
        None
    }
}

/// Clear saved clipboard content
pub fn clear_saved_clipboard_content() {
    if let Ok(mut saved) = SAVED_CLIPBOARD_CONTENT.lock() {
        *saved = None;
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_get_platform() {
        let platform = get_platform();
        assert!(["windows", "macos", "linux", "unknown"].contains(&platform));
    }
}

// ============================================================================
// Window Focus Tracking (for paste functionality)
// ============================================================================

/// Save the currently focused window (before our app activates)
/// Returns true if a window handle was saved
pub fn save_foreground_window() -> Result<bool, String> {
    #[cfg(target_os = "windows")]
    {
        use windows::Win32::UI::WindowsAndMessaging::GetForegroundWindow;

        let hwnd = unsafe { GetForegroundWindow() };
        let handle = hwnd.0 as isize;

        if handle != 0 {
            PREVIOUS_FOREGROUND_WINDOW.store(handle, Ordering::SeqCst);
            Ok(true)
        } else {
            Ok(false)
        }
    }

    #[cfg(not(target_os = "windows"))]
    {
        // On macOS and Linux, we'll rely on the OS to handle focus
        // when our window is hidden
        Ok(false)
    }
}

/// Restore focus to the previously saved window
/// Returns true if focus was restored
pub fn restore_foreground_window() -> Result<bool, String> {
    #[cfg(target_os = "windows")]
    {
        use windows::Win32::Foundation::HWND;
        use windows::Win32::System::Threading::{AttachThreadInput, GetCurrentThreadId};
        use windows::Win32::UI::WindowsAndMessaging::{
            GetForegroundWindow, GetWindowThreadProcessId, SetForegroundWindow,
        };

        let handle = PREVIOUS_FOREGROUND_WINDOW.load(Ordering::SeqCst);

        if handle != 0 {
            let target_hwnd = HWND(handle as *mut std::ffi::c_void);
            let current_hwnd = unsafe { GetForegroundWindow() };

            // If already focused, do nothing
            if target_hwnd == current_hwnd {
                return Ok(true);
            }

            let target_thread_id = unsafe { GetWindowThreadProcessId(target_hwnd, None) };
            let current_thread_id = unsafe { GetCurrentThreadId() };

            let mut result = false;

            // If threads are different, we need to attach input to set foreground window reliably
            if target_thread_id != current_thread_id {
                unsafe {
                    let attached = AttachThreadInput(current_thread_id, target_thread_id, true);
                    if attached.as_bool() {
                        result = SetForegroundWindow(target_hwnd).as_bool();
                        let _ = AttachThreadInput(current_thread_id, target_thread_id, false);
                    }
                }
            } else {
                result = unsafe { SetForegroundWindow(target_hwnd).as_bool() };
            }

            // Clear the stored handle
            PREVIOUS_FOREGROUND_WINDOW.store(0, Ordering::SeqCst);

            Ok(result)
        } else {
            Ok(false)
        }
    }

    #[cfg(not(target_os = "windows"))]
    {
        // On macOS and Linux, focusing happens automatically when our window hides
        Ok(true)
    }
}

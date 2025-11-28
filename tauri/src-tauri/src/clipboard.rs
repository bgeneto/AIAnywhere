//! Platform-specific text capture module for AI Anywhere
//! Implements cross-platform text selection capture using clipboard simulation

use std::time::Duration;
use std::thread;

/// Platform detection
pub fn get_platform() -> &'static str {
    #[cfg(target_os = "windows")]
    { "windows" }
    #[cfg(target_os = "macos")]
    { "macos" }
    #[cfg(target_os = "linux")]
    { "linux" }
    #[cfg(not(any(target_os = "windows", target_os = "macos", target_os = "linux")))]
    { "unknown" }
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
            enigo.key(Key::Meta, enigo::Direction::Press)
                .map_err(|e| format!("Failed to press Meta key: {}", e))?;
            enigo.key(Key::Unicode('c'), enigo::Direction::Click)
                .map_err(|e| format!("Failed to press C key: {}", e))?;
            enigo.key(Key::Meta, enigo::Direction::Release)
                .map_err(|e| format!("Failed to release Meta key: {}", e))?;
        }
        
        #[cfg(not(target_os = "macos"))]
        {
            // Windows and Linux use Ctrl+C
            enigo.key(Key::Control, enigo::Direction::Press)
                .map_err(|e| format!("Failed to press Ctrl key: {}", e))?;
            enigo.key(Key::Unicode('c'), enigo::Direction::Click)
                .map_err(|e| format!("Failed to press C key: {}", e))?;
            enigo.key(Key::Control, enigo::Direction::Release)
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
            enigo.key(Key::Meta, enigo::Direction::Press)
                .map_err(|e| format!("Failed to press Meta key: {}", e))?;
            enigo.key(Key::Unicode('v'), enigo::Direction::Click)
                .map_err(|e| format!("Failed to press V key: {}", e))?;
            enigo.key(Key::Meta, enigo::Direction::Release)
                .map_err(|e| format!("Failed to release Meta key: {}", e))?;
        }
        
        #[cfg(not(target_os = "macos"))]
        {
            // Windows and Linux use Ctrl+V
            enigo.key(Key::Control, enigo::Direction::Press)
                .map_err(|e| format!("Failed to press Ctrl key: {}", e))?;
            enigo.key(Key::Unicode('v'), enigo::Direction::Click)
                .map_err(|e| format!("Failed to press V key: {}", e))?;
            enigo.key(Key::Control, enigo::Direction::Release)
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

#[cfg(test)]
mod tests {
    use super::*;
    
    #[test]
    fn test_get_platform() {
        let platform = get_platform();
        assert!(["windows", "macos", "linux", "unknown"].contains(&platform));
    }
}

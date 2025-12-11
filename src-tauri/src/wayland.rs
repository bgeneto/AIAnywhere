//! Wayland-specific functionality for AI Anywhere
//!
//! This module provides:
//! - Wayland session detection
//! - xdg-desktop-portal GlobalShortcuts integration via ashpd
//!
//! On Wayland, applications cannot directly register global hotkeys.
//! Instead, they must use the xdg-desktop-portal GlobalShortcuts interface.

use std::env;
use std::sync::Arc;
use tokio::sync::Mutex;

use ashpd::desktop::global_shortcuts::{GlobalShortcuts, NewShortcut, Shortcut};
use futures_util::StreamExt;

/// Check if the current session is running under Wayland
pub fn is_wayland_session() -> bool {
    match env::var("XDG_SESSION_TYPE") {
        Ok(session_type) => session_type.to_lowercase() == "wayland",
        Err(_) => {
            // Fallback: check for WAYLAND_DISPLAY
            env::var("WAYLAND_DISPLAY").is_ok()
        }
    }
}

/// Manager for portal-based global shortcuts on Wayland
#[allow(dead_code)]
pub struct PortalShortcutManager {
    shortcuts: Arc<Mutex<Option<GlobalShortcuts<'static>>>>,
    _session_token: Arc<Mutex<Option<String>>>,
}

impl PortalShortcutManager {
    /// Create a new PortalShortcutManager
    pub fn new() -> Self {
        Self {
            shortcuts: Arc::new(Mutex::new(None)),
            _session_token: Arc::new(Mutex::new(None)),
        }
    }

    /// Initialize the portal connection and create a session
    pub async fn initialize(&self) -> Result<(), String> {
        let shortcuts = GlobalShortcuts::new()
            .await
            .map_err(|e| format!("Failed to connect to GlobalShortcuts portal: {}", e))?;

        // Create a session
        let _session = shortcuts
            .create_session()
            .await
            .map_err(|e| format!("Failed to create portal session: {}", e))?;

        // We need to keep the GlobalShortcuts alive, but ashpd's lifetime handling
        // makes this tricky. For now, we'll create new instances as needed.
        // This is a known limitation of ashpd's API design.

        Ok(())
    }

    /// Register a global shortcut with the portal
    ///
    /// # Arguments
    /// * `shortcut_id` - Unique identifier for the shortcut (e.g., "ai-anywhere-trigger")
    /// * `description` - User-visible description (e.g., "Activate AI Anywhere")
    /// * `preferred_trigger` - Preferred key combination (e.g., "Control+space")
    ///
    /// Note: The desktop environment may ignore the preferred trigger and let the user
    /// configure their own key combination.
    pub async fn bind_shortcut(
        &self,
        shortcut_id: &str,
        description: &str,
        preferred_trigger: Option<&str>,
    ) -> Result<Vec<Shortcut>, String> {
        let shortcuts = GlobalShortcuts::new()
            .await
            .map_err(|e| format!("Failed to connect to GlobalShortcuts portal: {}", e))?;

        let session = shortcuts
            .create_session()
            .await
            .map_err(|e| format!("Failed to create portal session: {}", e))?;

        // Build the shortcut request
        let mut new_shortcut = NewShortcut::new(shortcut_id, description);
        if let Some(trigger) = preferred_trigger {
            new_shortcut = new_shortcut.preferred_trigger(trigger);
        }

        // Request to bind the shortcut
        // This may trigger a system dialog for the user to confirm
        // In ashpd 0.10+, bind_shortcuts returns Request<BindShortcuts>
        // We need to call .response() to get the actual BindShortcuts
        let request = shortcuts
            .bind_shortcuts(&session, &[new_shortcut], None)
            .await
            .map_err(|e| format!("Failed to bind shortcut: {}", e))?;

        let bound_shortcuts = request
            .response()
            .map_err(|e| format!("Failed to get bind shortcuts response: {}", e))?;

        println!(
            "[Portal] Bound {} shortcut(s) via xdg-desktop-portal",
            bound_shortcuts.shortcuts().len()
        );

        for shortcut in bound_shortcuts.shortcuts() {
            println!(
                "[Portal] Shortcut '{}' bound with trigger: {:?}",
                shortcut.id(),
                shortcut.trigger_description()
            );
        }

        Ok(bound_shortcuts.shortcuts().to_vec())
    }

    /// Listen for shortcut activation events
    ///
    /// Returns a stream of (shortcut_id, timestamp) tuples when shortcuts are activated.
    /// This should be called in a background task that emits events to the frontend.
    pub async fn listen_for_activations<F>(&self, on_activated: F) -> Result<(), String>
    where
        F: Fn(String, u64) + Send + 'static,
    {
        let shortcuts = GlobalShortcuts::new()
            .await
            .map_err(|e| format!("Failed to connect to GlobalShortcuts portal: {}", e))?;

        let _session = shortcuts
            .create_session()
            .await
            .map_err(|e| format!("Failed to create portal session: {}", e))?;

        // In ashpd 0.10+, receive_activated takes no arguments and returns a Stream
        let mut activated_stream = shortcuts
            .receive_activated()
            .await
            .map_err(|e| format!("Failed to receive activated stream: {}", e))?;

        // Listen for activated signals
        loop {
            match activated_stream.next().await {
                Some(activated) => {
                    let shortcut_id = activated.shortcut_id().to_string();
                    let timestamp = activated.timestamp().as_secs();
                    println!(
                        "[Portal] Shortcut '{}' activated at timestamp {}",
                        shortcut_id, timestamp
                    );
                    on_activated(shortcut_id, timestamp);
                }
                None => {
                    eprintln!("[Portal] Activation stream ended unexpectedly");
                    // Try to reconnect after a delay
                    tokio::time::sleep(std::time::Duration::from_millis(100)).await;
                }
            }
        }
    }
}

impl Default for PortalShortcutManager {
    fn default() -> Self {
        Self::new()
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_is_wayland_session_detection() {
        // This test just ensures the function doesn't panic
        // Actual value depends on the environment
        let _ = is_wayland_session();
    }
}

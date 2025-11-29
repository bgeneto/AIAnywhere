import { useEffect, useRef, useCallback } from 'react';
import { register, unregister, isRegistered } from '@tauri-apps/plugin-global-shortcut';
import { WebviewWindow } from '@tauri-apps/api/webviewWindow';
import { invoke } from '@tauri-apps/api/core';

/**
 * Converts a hotkey string from our format to Tauri's expected format.
 * Our format: "Ctrl+Alt+Space" 
 * Tauri format: "Control+Alt+Space"
 */
function normalizeHotkeyForTauri(hotkey: string): string {
  if (!hotkey) return '';
  
  // Replace common key names with Tauri-compatible versions
  const normalized = hotkey
    .replace(/\bCtrl\b/gi, 'Control')
    .replace(/\bMeta\b/gi, 'Super')
    .replace(/\bEsc\b/gi, 'Escape');
  
  return normalized;
}

export interface UseGlobalShortcutOptions {
  /** The hotkey to register (e.g., "Ctrl+Alt+Space") */
  hotkey: string;
  /** Callback when the hotkey is triggered */
  onTrigger: () => void;
  /** Whether the shortcut should be registered */
  enabled?: boolean;
  /** Callback for registration errors */
  onError?: (error: string) => void;
  /** Callback for successful registration */
  onRegistered?: (hotkey: string) => void;
  /** Callback when hotkey is changed and successfully re-registered */
  onHotkeyChanged?: (oldHotkey: string | null, newHotkey: string) => void;
}

/**
 * Hook to register a global keyboard shortcut with the OS.
 * 
 * - Automatically unregisters old shortcuts when the hotkey changes
 * - Handles cleanup on unmount
 * - Shows/focuses the window when the shortcut is triggered
 */
export function useGlobalShortcut(options: UseGlobalShortcutOptions) {
  const { hotkey, onTrigger, enabled = true, onError, onRegistered, onHotkeyChanged } = options;
  
  // Track the currently registered hotkey (normalized version)
  const registeredNormalizedKeyRef = useRef<string | null>(null);
  // Track the original (non-normalized) hotkey for callbacks
  const registeredOriginalKeyRef = useRef<string | null>(null);

  // Stable callback ref to avoid unnecessary re-registrations
  const onTriggerRef = useRef(onTrigger);
  const onErrorRef = useRef(onError);
  const onRegisteredRef = useRef(onRegistered);
  const onHotkeyChangedRef = useRef(onHotkeyChanged);
  
  useEffect(() => {
    onTriggerRef.current = onTrigger;
    onErrorRef.current = onError;
    onRegisteredRef.current = onRegistered;
    onHotkeyChangedRef.current = onHotkeyChanged;
  }, [onTrigger, onError, onRegistered, onHotkeyChanged]);

  const unregisterShortcut = useCallback(async (normalizedKey: string): Promise<boolean> => {
    if (!normalizedKey) return false;

    try {
      const currentlyRegistered = await isRegistered(normalizedKey);
      if (currentlyRegistered) {
        await unregister(normalizedKey);
        console.log(`Global shortcut unregistered: ${normalizedKey}`);
      }
      return true;
    } catch (error) {
      console.error('Failed to unregister global shortcut:', error);
      return false;
    }
  }, []);

  const registerShortcut = useCallback(async (normalizedKey: string, originalKey: string): Promise<boolean> => {
    if (!normalizedKey) return false;

    try {
      // Always try to unregister first to clear any stale registrations
      // This handles the case where the app crashed and left a registration behind
      try {
        const alreadyRegistered = await isRegistered(normalizedKey);
        if (alreadyRegistered) {
          console.log(`Shortcut ${normalizedKey} was already registered, unregistering first...`);
          await unregister(normalizedKey);
        }
      } catch (e) {
        // Ignore errors during pre-cleanup
        console.debug('Pre-cleanup check failed:', e);
      }

      // Now register the shortcut fresh
      await register(normalizedKey, async (event) => {
        if (event.state === 'Pressed') {
          console.log('Global shortcut triggered:', normalizedKey);
          
          try {
            // First, capture selected text from the currently focused window
            // This also saves the foreground window handle for auto-paste
            console.log('Capturing selected text...');
            const capturedText = await invoke<string>('capture_selected_text');
            console.log('Captured text length:', capturedText?.length || 0);
            
            // Show and focus the main window
            const appWindow = await WebviewWindow.getByLabel('main');
            if (appWindow) {
              const isVisible = await appWindow.isVisible();
              const isMinimized = await appWindow.isMinimized();
              console.log(`Window state - visible: ${isVisible}, minimized: ${isMinimized}`);
              
              if (isMinimized) {
                console.log('Unminimizing window...');
                await appWindow.unminimize();
              }
              
              if (!isVisible) {
                console.log('Showing window...');
                await appWindow.show();
              }
              
              console.log('Setting focus...');
              await appWindow.setFocus();
              
              // Emit the captured text to the window so HomePage can use it
              if (capturedText && capturedText.trim()) {
                console.log('Emitting captured text to window...');
                await appWindow.emit('text-captured', capturedText);
              }
              
              console.log('Window operations completed');
            } else {
              console.error('Could not find window with label "main"');
            }
          } catch (e) {
            console.error('Failed to capture text or show window:', e);
          }

          // Call the trigger callback
          onTriggerRef.current();
        }
      });

      console.log(`Global shortcut registered: ${normalizedKey}`);
      onRegisteredRef.current?.(originalKey);
      return true;
    } catch (error) {
      console.error('Failed to register global shortcut:', error);
      onErrorRef.current?.(String(error));
      return false;
    }
  }, []);

  // Register/update the shortcut when hotkey or enabled state changes
  useEffect(() => {
    const normalizedKey = normalizeHotkeyForTauri(hotkey);
    const previousKey = registeredNormalizedKeyRef.current;
    const previousOriginalKey = registeredOriginalKeyRef.current;

    // Handle disabled state
    if (!enabled || !normalizedKey) {
      if (previousKey) {
        unregisterShortcut(previousKey);
        registeredNormalizedKeyRef.current = null;
        registeredOriginalKeyRef.current = null;
      }
      return;
    }

    // If the hotkey is the same, don't re-register
    if (previousKey === normalizedKey) {
      return;
    }

    // Async function to handle the registration
    const updateRegistration = async () => {
      // Unregister the old hotkey if different
      if (previousKey && previousKey !== normalizedKey) {
        await unregisterShortcut(previousKey);
      }

      // Register the new hotkey
      const success = await registerShortcut(normalizedKey, hotkey);
      if (success) {
        registeredNormalizedKeyRef.current = normalizedKey;
        registeredOriginalKeyRef.current = hotkey;
        
        // Notify that the hotkey was changed (only if there was a previous one)
        if (previousOriginalKey && previousOriginalKey !== hotkey) {
          onHotkeyChangedRef.current?.(previousOriginalKey, hotkey);
        }
      } else {
        registeredNormalizedKeyRef.current = null;
        registeredOriginalKeyRef.current = null;
      }
    };

    updateRegistration();

    // Note: We don't return a cleanup function here because we manage
    // unregistration manually when the hotkey changes. The unmount cleanup
    // is handled by a separate effect below.
  }, [hotkey, enabled, registerShortcut, unregisterShortcut]);

  // Separate cleanup effect that only runs on unmount
  useEffect(() => {
    return () => {
      const keyToUnregister = registeredNormalizedKeyRef.current;
      if (keyToUnregister) {
        unregisterShortcut(keyToUnregister);
        registeredNormalizedKeyRef.current = null;
        registeredOriginalKeyRef.current = null;
      }
    };
  }, [unregisterShortcut]);

  // Manual function to force re-registration (useful after saving settings)
  const forceReregister = useCallback(async () => {
    const normalizedKey = normalizeHotkeyForTauri(hotkey);
    if (!normalizedKey || !enabled) return;

    // Unregister current
    const previousKey = registeredNormalizedKeyRef.current;
    if (previousKey) {
      await unregisterShortcut(previousKey);
    }

    // Register new
    const success = await registerShortcut(normalizedKey, hotkey);
    if (success) {
      registeredNormalizedKeyRef.current = normalizedKey;
      registeredOriginalKeyRef.current = hotkey;
    } else {
      registeredNormalizedKeyRef.current = null;
      registeredOriginalKeyRef.current = null;
    }
  }, [hotkey, enabled, registerShortcut, unregisterShortcut]);

  return {
    isRegistered: !!registeredNormalizedKeyRef.current,
    registeredHotkey: registeredOriginalKeyRef.current,
    forceReregister,
  };
}

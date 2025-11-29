import { useState, useCallback, KeyboardEvent } from 'react';
import { register, unregister } from '@tauri-apps/plugin-global-shortcut';

// Blocklist of OS-reserved or problematic key combinations
const BLOCKED_HOTKEYS: string[] = [
  'Alt+Space',      // Windows: Opens window system menu
  'Alt+F4',         // Windows: Close window
  'Ctrl+Alt+Delete',// Windows: Security options
  'Alt+Tab',        // Windows: Switch windows
  'Alt+Escape',     // Windows: Cycle windows
  'Ctrl+Escape',    // Windows: Open Start menu
  'Alt+Enter',      // Windows: Toggle fullscreen in some apps
  'Meta+D',         // Windows: Show desktop
  'Meta+E',         // Windows: Open Explorer
  'Meta+L',         // Windows: Lock screen
  'Meta+R',         // Windows: Open Run dialog
  'Meta+Tab',       // Windows: Task view
];

// Keys that need special name normalization
const KEY_NORMALIZATIONS: Record<string, string> = {
  ' ': 'Space',
  'ArrowUp': 'Up',
  'ArrowDown': 'Down',
  'ArrowLeft': 'Left',
  'ArrowRight': 'Right',
  'Escape': 'Esc',
};

/**
 * Converts a hotkey string to Tauri's expected format for validation.
 */
function normalizeForTauri(hotkey: string): string {
  if (!hotkey) return '';
  return hotkey
    .replace(/\bCtrl\b/gi, 'Control')
    .replace(/\bMeta\b/gi, 'Super')
    .replace(/\bEsc\b/gi, 'Escape');
}

export interface UseHotkeyCaptureOptions {
  onHotkeyCapture: (hotkey: string) => void;
  onBlockedHotkey?: (hotkey: string, reason: string) => void;
  onUnavailableHotkey?: (hotkey: string, reason: string) => void;
  /** The currently configured hotkey (to allow re-selecting the same one) */
  currentHotkey?: string;
}

export interface UseHotkeyCaptureResult {
  isCapturing: boolean;
  isValidating: boolean;
  startCapture: () => void;
  stopCapture: () => void;
  handleKeyDown: (e: KeyboardEvent<HTMLInputElement>) => void;
}

/**
 * Custom hook for capturing hotkey combinations from user input.
 * 
 * Features:
 * - Normalizes special keys (Space, Arrow keys, etc.)
 * - Blocks OS-reserved key combinations
 * - Validates if hotkey is available (not used by another program)
 * - Handles modifier keys (Ctrl, Alt, Shift, Meta)
 */
export function useHotkeyCapture(options: UseHotkeyCaptureOptions): UseHotkeyCaptureResult {
  const { onHotkeyCapture, onBlockedHotkey, onUnavailableHotkey, currentHotkey } = options;
  const [isCapturing, setIsCapturing] = useState(false);
  const [isValidating, setIsValidating] = useState(false);

  const startCapture = useCallback(() => {
    setIsCapturing(true);
  }, []);

  const stopCapture = useCallback(() => {
    setIsCapturing(false);
  }, []);

  /**
   * Validates if a hotkey is available by attempting to register it.
   * Returns true if available, false if already in use by another app.
   * 
   * IMPORTANT: This validation is tricky because:
   * - We can't distinguish between "registered by us" vs "registered by another app"
   * - If we unregister to test, we might break our own active hotkey
   * 
   * Strategy: Only do a simple availability check without unregistering existing ones.
   * If the hotkey is the same as current, it's always valid.
   * If it's different and already registered, we try to register it - if it fails, it's taken.
   */
  const validateHotkeyAvailability = useCallback(async (_hotkey: string, tauriKey: string): Promise<boolean> => {
    // If it's the same as the current hotkey, it's valid (user is keeping the same one)
    if (currentHotkey && normalizeForTauri(currentHotkey) === tauriKey) {
      console.log('Hotkey is same as current, skipping validation');
      return true;
    }

    try {
      // Try to register the hotkey directly
      // If it fails, it means another app has it registered
      // We use a no-op callback for the test
      try {
        await register(tauriKey, () => {});
        // Registration succeeded - unregister immediately and return true
        await unregister(tauriKey);
        console.log(`Hotkey ${tauriKey} is available`);
        return true;
      } catch (error) {
        // Registration failed - hotkey is likely taken by another app
        console.log(`Hotkey ${tauriKey} is not available:`, error);
        return false;
      }
    } catch (error) {
      console.error('Hotkey availability check failed:', error);
      // On error, assume it's available and let the actual registration handle it
      return true;
    }
  }, [currentHotkey]);

  const handleKeyDown = useCallback(async (e: KeyboardEvent<HTMLInputElement>) => {
    if (!isCapturing || isValidating) return;

    e.preventDefault();
    e.stopPropagation();

    const parts: string[] = [];
    if (e.ctrlKey) parts.push('Ctrl');
    if (e.altKey) parts.push('Alt');
    if (e.shiftKey) parts.push('Shift');
    if (e.metaKey) parts.push('Meta');

    const rawKey = e.key;
    
    // Skip if only modifier keys are pressed
    if (['Control', 'Alt', 'Shift', 'Meta'].includes(rawKey)) {
      return;
    }

    // Normalize the key name
    let normalizedKey = KEY_NORMALIZATIONS[rawKey] || rawKey;
    
    // Uppercase single character keys
    if (normalizedKey.length === 1) {
      normalizedKey = normalizedKey.toUpperCase();
    }

    parts.push(normalizedKey);
    const hotkey = parts.join('+');

    // Check if hotkey is in our blocklist
    if (BLOCKED_HOTKEYS.includes(hotkey)) {
      if (onBlockedHotkey) {
        onBlockedHotkey(hotkey, `"${hotkey}" is reserved by the operating system and cannot be used as a global hotkey.`);
      }
      setIsCapturing(false);
      return;
    }

    // Validate hotkey availability with the OS
    setIsValidating(true);
    const tauriKey = normalizeForTauri(hotkey);
    
    try {
      const isAvailable = await validateHotkeyAvailability(hotkey, tauriKey);
      
      if (!isAvailable) {
        if (onUnavailableHotkey) {
          onUnavailableHotkey(hotkey, `"${hotkey}" is already in use by another application. Please choose a different hotkey combination.`);
        }
        setIsCapturing(false);
        setIsValidating(false);
        return;
      }

      onHotkeyCapture(hotkey);
    } finally {
      setIsCapturing(false);
      setIsValidating(false);
    }
  }, [isCapturing, isValidating, onHotkeyCapture, onBlockedHotkey, onUnavailableHotkey, validateHotkeyAvailability]);

  return {
    isCapturing,
    isValidating,
    startCapture,
    stopCapture,
    handleKeyDown,
  };
}
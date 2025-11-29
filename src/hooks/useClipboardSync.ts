import { useEffect, useCallback, useRef } from 'react';
import { getCurrentWindow } from '@tauri-apps/api/window';
import { readText } from '@tauri-apps/plugin-clipboard-manager';

export interface UseClipboardSyncOptions {
  /** Whether sync is enabled */
  enabled?: boolean;
  /** Callback when clipboard content is read */
  onClipboardRead: (text: string) => void;
  /** Whether to only sync if the current text is empty */
  onlyIfEmpty?: boolean;
  /** Current text value (used when onlyIfEmpty is false) */
  currentText?: string;
}

/**
 * Hook to synchronize clipboard content when the app window gains focus.
 * 
 * This solves the issue where users copy text, switch to the app, but
 * the prompt textarea still shows old content.
 */
export function useClipboardSync(options: UseClipboardSyncOptions) {
  const { 
    enabled = true, 
    onClipboardRead, 
    onlyIfEmpty = false,
    currentText = '' 
  } = options;

  // Track the last clipboard content to avoid unnecessary updates
  const lastClipboardRef = useRef<string>('');

  // Stable callback ref
  const onClipboardReadRef = useRef(onClipboardRead);
  useEffect(() => {
    onClipboardReadRef.current = onClipboardRead;
  }, [onClipboardRead]);

  const syncClipboard = useCallback(async () => {
    if (!enabled) return;

    // If onlyIfEmpty is true and there's already text, don't sync
    if (onlyIfEmpty && currentText.trim()) {
      return;
    }

    try {
      const clipboardText = await readText();
      
      // Only update if clipboard has content and is different from last read
      if (clipboardText && clipboardText.trim()) {
        // Skip if it's the same content we already synced
        if (clipboardText === lastClipboardRef.current) {
          return;
        }

        lastClipboardRef.current = clipboardText;
        onClipboardReadRef.current(clipboardText);
      }
    } catch (error) {
      console.debug('Could not read clipboard:', error);
    }
  }, [enabled, onlyIfEmpty, currentText]);

  // Listen for window focus events
  useEffect(() => {
    if (!enabled) return;

    let unlistenFocus: (() => void) | undefined;

    const setupListener = async () => {
      try {
        const appWindow = getCurrentWindow();
        
        // Listen for focus events
        unlistenFocus = await appWindow.onFocusChanged(({ payload: focused }) => {
          if (focused) {
            console.debug('Window focused, syncing clipboard...');
            syncClipboard();
          }
        });

        // Also sync on initial mount
        syncClipboard();
      } catch (error) {
        console.error('Failed to set up window focus listener:', error);
      }
    };

    setupListener();

    return () => {
      unlistenFocus?.();
    };
  }, [enabled, syncClipboard]);

  // Manual sync function for refresh button
  const manualSync = useCallback(async () => {
    // Reset the last clipboard ref to force an update
    lastClipboardRef.current = '';
    await syncClipboard();
  }, [syncClipboard]);

  return {
    syncClipboard: manualSync,
  };
}

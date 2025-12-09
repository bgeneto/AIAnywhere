
import { useState, useRef, useEffect } from 'react';
import { writeText } from '@tauri-apps/plugin-clipboard-manager';
import { save } from '@tauri-apps/plugin-dialog';
import { writeFile } from '@tauri-apps/plugin-fs';
import { invoke } from '@tauri-apps/api/core';
import { getCurrentWindow } from '@tauri-apps/api/window';
import { convertFileSrc } from '@tauri-apps/api/core';
import { open } from '@tauri-apps/plugin-shell';
import { downloadDir } from '@tauri-apps/api/path';
import { useApp } from '../context/AppContext';
import { MarkdownRenderer } from './MarkdownRenderer';
import { useI18n } from '../i18n/index';
import { ToastType } from '../types';

// Utility function for chunked base64 encoding to avoid stack overflow
function uint8ArrayToBase64(bytes: Uint8Array | number[]): string {
  const arr = bytes instanceof Uint8Array ? bytes : new Uint8Array(bytes);
  const CHUNK_SIZE = 8192;
  let result = '';
  for (let i = 0; i < arr.length; i += CHUNK_SIZE) {
    const chunk = arr.slice(i, i + CHUNK_SIZE);
    result += String.fromCharCode.apply(null, Array.from(chunk));
  }
  return btoa(result);
}

interface ReviewModalProps {
  onShowToast: (type: ToastType, title: string, message?: string) => void;
}

export function ReviewModal({ onShowToast }: ReviewModalProps) {
  const { result, closeModal, clearResult, selectedOperation, config } = useApp();
  const { t } = useI18n();
  const [isEditing, setIsEditing] = useState(false);
  const [editedContent, setEditedContent] = useState('');
  const [imageLoading, setImageLoading] = useState(true);
  const [imageError, setImageError] = useState(false);
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  useEffect(() => {
    if (result?.content) {
      setEditedContent(result.content);
    }
  }, [result?.content]);

  if (!result) return null;

  const handleBack = () => {
    closeModal();
  };

  const handleCopy = async () => {
    try {
      const textToCopy = isEditing ? editedContent : (result.content || '');
      await writeText(textToCopy);
      onShowToast('success', t.review.copied, t.review.copied);
    } catch (error) {
      onShowToast('error', t.toast.error, 'Failed to copy to clipboard');
    }
  };

  const handlePaste = async () => {
    try {
      const textToPaste = isEditing ? editedContent : (result.content || '');
      await writeText(textToPaste);

      // Close the modal first
      closeModal();
      clearResult();

      // Hide the window to allow focus to return to original app
      const appWindow = getCurrentWindow();
      await appWindow.hide();

      // Small delay to allow window to hide
      await new Promise(resolve => setTimeout(resolve, 100));

      // Restore focus to the previously active window (Windows-specific)
      // On macOS/Linux, focus should return automatically when our window hides
      await invoke('restore_foreground_window');

      // Another small delay to ensure focus is restored (configurable via copyDelayMs)
      await new Promise(resolve => setTimeout(resolve, config?.copyDelayMs || 200));

      // Simulate paste (Ctrl+V / Cmd+V)
      await invoke('simulate_paste');

      onShowToast('success', t.review.pasted, t.review.pasted);
    } catch (error) {
      onShowToast('error', t.toast.error, 'Failed to paste content');
    }
  };

  const handleSaveImage = async () => {
    if (!result.imageUrl) {
      console.error('[SaveImage] No image URL available in result');
      onShowToast('error', 'Error', 'No image URL available');
      return;
    }

    console.log('[SaveImage] Starting save process...');
    console.log('[SaveImage] Image URL:', result.imageUrl);

    try {
      // Show save dialog
      console.log('[SaveImage] Opening save dialog...');
      const savePath = await save({
        filters: [{
          name: 'Images',
          extensions: ['png', 'jpg', 'jpeg', 'webp'],
        }],
        defaultPath: 'generated-image.png',
      });

      console.log('[SaveImage] Save path selected:', savePath);

      if (!savePath) {
        console.log('[SaveImage] User cancelled save dialog');
        return;
      }

      // Try to download and save via backend command (bypasses CORS)
      console.log('[SaveImage] Attempting to download image via backend...');
      try {
        await invoke('download_and_save_image', {
          imageUrl: result.imageUrl,
          savePath: savePath
        });
        console.log('[SaveImage] Backend download successful!');
        onShowToast('success', 'Saved', 'Image saved successfully');
        return;
      } catch (backendError) {
        console.warn('[SaveImage] Backend download failed, trying frontend fetch...', backendError);
      }

      // Fallback: Try frontend fetch (may fail due to CORS)
      console.log('[SaveImage] Attempting frontend fetch...');
      console.log('[SaveImage] Fetching from URL:', result.imageUrl);

      const response = await fetch(result.imageUrl);
      console.log('[SaveImage] Fetch response status:', response.status);
      console.log('[SaveImage] Fetch response ok:', response.ok);
      console.log('[SaveImage] Fetch response headers:', Object.fromEntries(response.headers.entries()));

      if (!response.ok) {
        throw new Error(`Fetch failed with status ${response.status}: ${response.statusText} `);
      }

      const arrayBuffer = await response.arrayBuffer();
      console.log('[SaveImage] ArrayBuffer size:', arrayBuffer.byteLength);

      if (arrayBuffer.byteLength === 0) {
        throw new Error('Downloaded image is empty (0 bytes)');
      }

      const uint8Array = new Uint8Array(arrayBuffer);
      console.log('[SaveImage] Uint8Array length:', uint8Array.length);

      console.log('[SaveImage] Writing file to:', savePath);
      await writeFile(savePath, uint8Array);
      console.log('[SaveImage] File written successfully!');

      onShowToast('success', 'Saved', 'Image saved successfully');
    } catch (error) {
      console.error('[SaveImage] Error details:', {
        name: error instanceof Error ? error.name : 'Unknown',
        message: error instanceof Error ? error.message : String(error),
        stack: error instanceof Error ? error.stack : undefined,
        error: error
      });
      onShowToast('error', 'Error', `Failed to save image: ${error instanceof Error ? error.message : String(error)} `);
    }
  };

  const handleCopyImage = async () => {
    if (!result.imageUrl) return;

    try {
      await invoke('copy_image_to_clipboard_command', { imageUrl: result.imageUrl });
      onShowToast('success', t.review.copied, t.review.copied);
    } catch (error) {
      onShowToast('error', t.toast.error, `Failed to copy image: ${error instanceof Error ? error.message : String(error)} `);
    }
  };

  const handlePasteImage = async () => {
    if (!result.imageUrl) return;

    try {
      // First copy the image to clipboard
      await invoke('copy_image_to_clipboard_command', { imageUrl: result.imageUrl });

      // Close the modal first
      closeModal();
      clearResult();

      // Hide the window to allow focus to return to original app
      const appWindow = getCurrentWindow();
      await appWindow.hide();

      // Small delay to allow window to hide
      await new Promise(resolve => setTimeout(resolve, 100));

      // Restore focus to the previously active window
      await invoke('restore_foreground_window');

      // Another small delay to ensure focus is restored
      await new Promise(resolve => setTimeout(resolve, config?.copyDelayMs || 200));

      // Simulate paste (Ctrl+V / Cmd+V)
      await invoke('simulate_paste');

      onShowToast('success', t.review.pasted, t.review.pasted);
    } catch (error) {
      onShowToast('error', t.toast.error, `Failed to paste image: ${error instanceof Error ? error.message : String(error)} `);
    }
  };

  const handleSaveAudio = async () => {
    // Support both audioFilePath (new) and audioData (legacy fallback)
    if (!result.audioFilePath && !result.audioData) return;

    try {
      const extension = result.audioFormat || 'mp3';
      const defaultDir = await downloadDir();
      const savePath = await save({
        filters: [{
          name: 'Audio Files',
          extensions: [extension],
        }],
        defaultPath: `${defaultDir}/generated-audio.${extension}`,
      });

      if (savePath) {
        if (result.audioFilePath) {
          // Copy from saved file to user-selected location via backend command
          // This bypasses Tauri fs plugin restrictions on Linux
          if (config?.enableDebugLogging) {
            console.log('[SaveAudio] Copying from:', result.audioFilePath, 'to:', savePath);
          }
          await invoke('copy_audio_file', {
            sourcePath: result.audioFilePath,
            destPath: savePath
          });
        } else if (result.audioData) {
          // Legacy: write from memory (shouldn't happen with new flow)
          if (config?.enableDebugLogging) {
            console.log('[SaveAudio] Writing from memory, size:', result.audioData.length);
          }
          const uint8Array = new Uint8Array(result.audioData);
          await writeFile(savePath, uint8Array);
        }

        onShowToast('success', 'Saved', 'Audio saved successfully');

        // Open the containing folder
        const folderPath = savePath.substring(0, Math.max(savePath.lastIndexOf('\\'), savePath.lastIndexOf('/')));
        if (folderPath) {
          try {
            await open(folderPath);
          } catch (e) {
            console.warn('Failed to open folder:', e);
          }
        }
      }
    } catch (error) {
      console.error('[SaveAudio] Error:', error);
      onShowToast('error', 'Error', `Failed to save audio: ${error instanceof Error ? error.message : String(error)}`);
    }
  };

  const handlePrint = () => {
    window.print();
  };

  const characterCount = (isEditing ? editedContent : (result.content || '')).length;

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4 print-modal-overlay">
      <div className="bg-white dark:bg-slate-900 rounded-xl shadow-2xl w-[70%] max-w-[95%] min-w-[400px] min-h-[300px] max-h-[99vh] flex flex-col animate-in fade-in zoom-in-95 duration-200 resize overflow-auto print-modal-container">
        {/* Header */}
        <div className="flex items-center justify-between p-4 border-b border-slate-200 dark:border-slate-700 no-print">
          <div className="flex items-center gap-3">
            <div className="w-8 h-8 rounded-full bg-green-100 dark:bg-green-900/30 flex items-center justify-center">
              <span className="text-green-600 dark:text-green-400">✓</span>
            </div>
            <div>
              <h2 className="text-lg font-semibold text-slate-900 dark:text-white">
                Result
              </h2>
              <p className="text-sm text-slate-500 dark:text-slate-400">
                {selectedOperation?.name}
              </p>
            </div>
          </div>
          <button
            onClick={handleBack}
            className="btn-ghost"
          >
            ✕
          </button>
        </div>

        {/* Content */}
        <div className="flex-1 overflow-y-auto p-4 print-modal-content">
          {result.isImage && result.imageUrl ? (
            <div className="space-y-4">
              {imageLoading && (
                <div className="flex items-center justify-center h-64">
                  <div className="animate-spin text-4xl">⏳</div>
                </div>
              )}
              {imageError ? (
                <div className="flex items-center justify-center h-64 bg-slate-100 dark:bg-slate-800 rounded-lg">
                  <p className="text-slate-500 dark:text-slate-400">Failed to load image</p>
                </div>
              ) : (
                <img
                  src={result.imageUrl}
                  alt="Generated image"
                  className={`w-full rounded-lg ${imageLoading ? 'hidden' : 'block'}`}
                  onLoad={() => setImageLoading(false)}
                  onError={() => {
                    setImageLoading(false);
                    setImageError(true);
                  }}
                />
              )}
            </div>
          ) : result.isAudio ? (
            <div className="space-y-4">
              <div className="flex items-center justify-center h-32 bg-slate-100 dark:bg-slate-800 rounded-lg">
                <div className="text-center">
                  <span className="text-5xl">🔊</span>
                  <p className="text-sm text-slate-600 dark:text-slate-400 mt-2">
                    Audio generated ({result.audioFormat?.toUpperCase()})
                  </p>
                </div>
              </div>
              {/* Audio player - use file path or fallback to base64 data */}
              {result.audioFilePath ? (
                <audio
                  controls
                  className="w-full"
                  src={convertFileSrc(result.audioFilePath)}
                />
              ) : result.audioData && (
                <audio
                  controls
                  className="w-full"
                  src={`data:audio/${result.audioFormat || 'mp3'};base64,${uint8ArrayToBase64(result.audioData)}`}
                />
              )}
            </div>
          ) : (
            <div className="space-y-2">
              {isEditing ? (
                <textarea
                  ref={textareaRef}
                  value={editedContent}
                  onChange={(e) => setEditedContent(e.target.value)}
                  className="w-full min-h-[300px] px-3 py-2 text-sm rounded-lg border border-slate-300 dark:border-slate-600 
                             bg-white dark:bg-slate-800 text-slate-900 dark:text-white
                             focus:ring-2 focus:ring-blue-500 focus:border-transparent
                             resize-none transition-colors duration-200"
                />
              ) : (
                <MarkdownRenderer
                  content={editedContent || result.content || ''}
                  className="w-full min-h-[300px] px-4 py-3 rounded-lg border border-slate-200 dark:border-slate-700 
                             bg-slate-50 dark:bg-slate-800 overflow-y-auto"
                  id="printable-area"
                />
              )}
              <div className="flex items-center justify-between text-xs text-slate-500 dark:text-slate-400 no-print">
                <span>{characterCount} characters</span>
                <button
                  onClick={() => setIsEditing(!isEditing)}
                  className="hover:text-slate-700 dark:hover:text-slate-300 transition-colors"
                >
                  {isEditing ? 'Preview' : 'Edit'}
                </button>
              </div>
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="flex items-center justify-between p-4 border-t border-slate-200 dark:border-slate-700 no-print">
          <button
            onClick={handleBack}
            className="btn-outline"
          >
            <span>←</span>
            {t.review.back}
          </button>

          <div className="flex items-center gap-2">
            {result.isImage && (
              <>
                <button
                  onClick={handleCopyImage}
                  className="btn-outline"
                >
                  📋 {t.review.copy}
                </button>

                {config?.pasteBehavior !== 'clipboardMode' && (
                  <button
                    onClick={handlePasteImage}
                    className="btn-primary"
                  >
                    📋 {t.review.paste}
                  </button>
                )}

                <button
                  onClick={handleSaveImage}
                  className="btn-outline"
                >
                  💾 {t.review.saveImage}
                </button>
              </>
            )}

            {result.isAudio && (
              <button
                onClick={handleSaveAudio}
                className="btn-outline"
              >
                💾 {t.review.save}
              </button>
            )}



            {!result.isImage && !result.isAudio && (
              <>
                <button
                  onClick={handleCopy}
                  className="btn-outline"
                >
                  📋 {t.review.copy}
                </button>

                {config?.pasteBehavior !== 'clipboardMode' && (
                  <button
                    onClick={handlePaste}
                    className="btn-outline"
                  >
                    📋 {t.review.paste}
                  </button>
                )}

                <button
                  onClick={handlePrint}
                  className="btn-outline"
                >
                  🖨️ PDF
                </button>
              </>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}


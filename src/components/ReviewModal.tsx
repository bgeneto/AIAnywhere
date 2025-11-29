import { useState, useRef, useEffect } from 'react';
import { writeText } from '@tauri-apps/plugin-clipboard-manager';
import { save } from '@tauri-apps/plugin-dialog';
import { writeFile } from '@tauri-apps/plugin-fs';
import { invoke } from '@tauri-apps/api/core';
import { useApp } from '../context/AppContext';
import { ToastType } from '../types';

interface ReviewModalProps {
  onShowToast: (type: ToastType, title: string, message?: string) => void;
}

export function ReviewModal({ onShowToast }: ReviewModalProps) {
  const { result, closeModal, clearResult, selectedOperation, config } = useApp();
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
      onShowToast('success', 'Copied', 'Content copied to clipboard');
    } catch (error) {
      onShowToast('error', 'Error', 'Failed to copy to clipboard');
    }
  };

  const handlePaste = async () => {
    try {
      const textToPaste = isEditing ? editedContent : (result.content || '');
      await writeText(textToPaste);
      
      // Simulate paste (Ctrl+V)
      await invoke('simulate_paste');
      
      onShowToast('success', 'Pasted', 'Content pasted to application');
      closeModal();
      clearResult();
    } catch (error) {
      onShowToast('error', 'Error', 'Failed to paste content');
    }
  };

  const handleSaveImage = async () => {
    if (!result.imageUrl) return;
    
    try {
      const savePath = await save({
        filters: [{
          name: 'Images',
          extensions: ['png', 'jpg', 'jpeg', 'webp'],
        }],
        defaultPath: 'generated-image.png',
      });
      
      if (savePath) {
        // Fetch the image
        const response = await fetch(result.imageUrl);
        const arrayBuffer = await response.arrayBuffer();
        const uint8Array = new Uint8Array(arrayBuffer);
        
        await writeFile(savePath, uint8Array);
        onShowToast('success', 'Saved', 'Image saved successfully');
      }
    } catch (error) {
      onShowToast('error', 'Error', 'Failed to save image');
    }
  };

  const handleSaveAudio = async () => {
    if (!result.audioData) return;
    
    try {
      const extension = result.audioFormat || 'mp3';
      const savePath = await save({
        filters: [{
          name: 'Audio Files',
          extensions: [extension],
        }],
        defaultPath: `generated-audio.${extension}`,
      });
      
      if (savePath) {
        const uint8Array = new Uint8Array(result.audioData);
        await writeFile(savePath, uint8Array);
        onShowToast('success', 'Saved', 'Audio saved successfully');
      }
    } catch (error) {
      onShowToast('error', 'Error', 'Failed to save audio');
    }
  };

  const characterCount = (isEditing ? editedContent : (result.content || '')).length;

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
      <div className="bg-white dark:bg-slate-900 rounded-xl shadow-2xl w-full max-w-2xl max-h-[90vh] flex flex-col animate-in fade-in zoom-in-95 duration-200">
        {/* Header */}
        <div className="flex items-center justify-between p-4 border-b border-slate-200 dark:border-slate-700">
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
            className="p-2 text-slate-400 hover:text-slate-600 dark:hover:text-slate-300 
                       hover:bg-slate-100 dark:hover:bg-slate-800 rounded-lg transition-colors"
          >
            ✕
          </button>
        </div>

        {/* Content */}
        <div className="flex-1 overflow-y-auto p-4">
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
              {result.audioData && (
                <audio
                  controls
                  className="w-full"
                  src={`data:audio/${result.audioFormat || 'mp3'};base64,${btoa(String.fromCharCode(...result.audioData))}`}
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
                <div 
                  className="w-full min-h-[300px] px-3 py-2 text-sm rounded-lg border border-slate-200 dark:border-slate-700 
                             bg-slate-50 dark:bg-slate-800 text-slate-900 dark:text-white
                             whitespace-pre-wrap overflow-y-auto"
                  onClick={() => setIsEditing(true)}
                >
                  {result.content}
                </div>
              )}
              <div className="flex items-center justify-between text-xs text-slate-500 dark:text-slate-400">
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
        <div className="flex items-center justify-between p-4 border-t border-slate-200 dark:border-slate-700">
          <button
            onClick={handleBack}
            className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-slate-900 dark:text-white
                       border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-800
                       hover:bg-slate-50 dark:hover:bg-slate-700 hover:border-slate-400 dark:hover:border-slate-500
                       rounded-lg transition-colors duration-200"
          >
            <span>←</span>
            Back
          </button>

          <div className="flex items-center gap-2">
            {result.isImage && (
              <button
                onClick={handleSaveImage}
                className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-slate-700 dark:text-slate-300
                           border border-slate-300 dark:border-slate-600 rounded-lg
                           hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors"
              >
                💾 Save Image
              </button>
            )}

            {result.isAudio && (
              <button
                onClick={handleSaveAudio}
                className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-slate-700 dark:text-slate-300
                           border border-slate-300 dark:border-slate-600 rounded-lg
                           hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors"
              >
                💾 Save Audio
              </button>
            )}

            {!result.isImage && !result.isAudio && (
              <>
                <button
                  onClick={handleCopy}
                  className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-slate-700 dark:text-slate-300
                             border border-slate-300 dark:border-slate-600 rounded-lg
                             hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors"
                >
                  📋 Copy
                </button>

                {config?.pasteBehavior !== 'clipboardMode' && (
                  <button
                    onClick={handlePaste}
                    className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-white
                               bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors"
                  >
                    📋 Paste
                  </button>
                )}
              </>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

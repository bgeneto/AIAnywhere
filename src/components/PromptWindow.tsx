import { useEffect, useRef, KeyboardEvent, useCallback } from 'react';
import { getCurrentWindow } from '@tauri-apps/api/window';
import { useApp } from '../context/AppContext';
import { useClipboardSync } from '../hooks/useClipboardSync';
import { OperationOptionsPanel } from './OperationOptionsPanel';
import { AudioUpload } from './AudioUpload';
import { ThemeToggle } from './ThemeToggle';
import { ToastType } from '../types';

interface PromptWindowProps {
  onShowToast: (type: ToastType, title: string, message?: string) => void;
}

export function PromptWindow({ onShowToast }: PromptWindowProps) {
  const {
    operations,
    selectedOperation,
    setSelectedOperation,
    promptText,
    setPromptText,
    isProcessing,
    processRequest,
    openModal,
  } = useApp();

  const textareaRef = useRef<HTMLTextAreaElement>(null);

  // Clipboard sync callback - updates prompt text with clipboard content
  const handleClipboardSync = useCallback((text: string) => {
    setPromptText(text);
  }, [setPromptText]);

  // Sync clipboard when window gains focus
  const { syncClipboard } = useClipboardSync({
    enabled: true,
    onClipboardRead: handleClipboardSync,
    onlyIfEmpty: false, // Always sync to keep in sync with latest clipboard
    currentText: promptText,
  });

  // Focus textarea on mount
  useEffect(() => {
    textareaRef.current?.focus();
  }, []);

  const handleOperationChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const operation = operations.find(op => op.type === e.target.value);
    if (operation) {
      setSelectedOperation(operation);
    }
  };

  const handleKeyDown = (e: KeyboardEvent<HTMLTextAreaElement>) => {
    // Ctrl/Cmd + Enter to send
    if (e.key === 'Enter' && (e.ctrlKey || e.metaKey)) {
      e.preventDefault();
      handleSend();
    }
    // Escape to cancel/close
    if (e.key === 'Escape') {
      handleCancel();
    }
  };

  const handleSend = async () => {
    if (!selectedOperation) {
      onShowToast('error', 'Error', 'Please select an operation');
      return;
    }

    if (!promptText.trim() && selectedOperation.type !== 'speechToText') {
      onShowToast('error', 'Error', 'Please enter a prompt');
      return;
    }

    const response = await processRequest();
    if (response && !response.success) {
      onShowToast('error', 'Error', response.error || 'Request failed');
    }
  };

  const handleCancel = async () => {
    // Hide the window instead of closing when used as popup
    try {
      const window = getCurrentWindow();
      await window.hide();
    } catch {
      // If hide fails, just clear the prompt
      setPromptText('');
    }
  };

  const handleClear = () => {
    setPromptText('');
    textareaRef.current?.focus();
  };

  const isSpeechToText = selectedOperation?.type === 'speechToText';

  return (
    <div className="flex flex-col h-full bg-white dark:bg-slate-900">
      {/* Header */}
      <div className="flex items-center justify-between p-4 border-b border-slate-200 dark:border-slate-700">
        <h1 className="text-xl font-bold text-slate-900 dark:text-white">
          AI Anywhere
        </h1>
        <div className="flex items-center gap-2">
          <ThemeToggle />
          <button
            onClick={() => openModal('settings')}
            className="p-2 text-slate-500 hover:text-slate-700 dark:text-slate-400 dark:hover:text-slate-200 
                       hover:bg-slate-100 dark:hover:bg-slate-800 rounded-lg transition-colors"
            title="Settings"
          >
            ⚙️
          </button>
        </div>
      </div>

      {/* Content */}
      <div className="flex-1 overflow-y-auto p-4 space-y-4">
        {/* Task Selection */}
        <div className="space-y-1">
          <label 
            htmlFor="operation"
            className="block text-sm font-medium text-slate-700 dark:text-slate-300"
          >
            Task Selection:
          </label>
          <div className="relative">
            <select
              id="operation"
              value={selectedOperation?.type || ''}
              onChange={handleOperationChange}
              className="w-full px-3 py-2.5 text-sm rounded-lg border border-slate-300 dark:border-slate-600 
                         bg-white dark:bg-slate-800 text-slate-900 dark:text-white
                         focus:ring-2 focus:ring-blue-500 focus:border-transparent
                         transition-colors duration-200 appearance-none cursor-pointer"
            >
              {operations.map((op) => (
                <option key={op.type} value={op.type}>
                  {op.name}
                </option>
              ))}
            </select>
            <span className="absolute right-3 top-1/2 -translate-y-1/2 pointer-events-none text-slate-400">
              ▼
            </span>
          </div>
          {selectedOperation && (
            <p className="text-xs text-slate-500 dark:text-slate-400 italic">
              {selectedOperation.description}
            </p>
          )}
        </div>

        {/* Operation Options */}
        <OperationOptionsPanel />

        {/* Audio Upload (for STT) */}
        {isSpeechToText && (
          <AudioUpload onError={(msg) => onShowToast('error', 'Audio Error', msg)} />
        )}

        {/* Prompt Content */}
        <div className="space-y-1 flex-1 flex flex-col">
          <div className="flex items-center justify-between">
            <label 
              htmlFor="prompt"
              className="block text-sm font-medium text-slate-700 dark:text-slate-300"
            >
              Prompt Content:
            </label>
            <div className="flex items-center gap-2">
              <button
                onClick={syncClipboard}
                className="flex items-center gap-1 px-2 py-1 text-xs text-slate-600 dark:text-slate-400 
                           hover:text-slate-900 dark:hover:text-white
                           bg-slate-100 dark:bg-slate-800 hover:bg-slate-200 dark:hover:bg-slate-700
                           rounded transition-colors"
                title="Refresh from clipboard"
              >
                📋 Sync
              </button>
              <button
                onClick={handleClear}
                className="flex items-center gap-1 px-2 py-1 text-xs text-slate-600 dark:text-slate-400 
                           hover:text-slate-900 dark:hover:text-white
                           bg-slate-100 dark:bg-slate-800 hover:bg-slate-200 dark:hover:bg-slate-700
                           rounded transition-colors"
              >
                🗑️ Clear
              </button>
            </div>
          </div>
          <textarea
            ref={textareaRef}
            id="prompt"
            value={promptText}
            onChange={(e) => setPromptText(e.target.value)}
            onKeyDown={handleKeyDown}
            placeholder={isSpeechToText 
              ? "Optional: Add instructions for transcription..." 
              : "Enter your prompt here... (Ctrl+Enter to send)"
            }
            className="flex-1 min-h-[200px] w-full px-3 py-2 text-sm rounded-lg border border-slate-300 dark:border-slate-600 
                       bg-white dark:bg-slate-800 text-slate-900 dark:text-white
                       placeholder-slate-400 dark:placeholder-slate-500
                       focus:ring-2 focus:ring-blue-500 focus:border-transparent
                       resize-none transition-colors duration-200"
          />
        </div>
      </div>

      {/* Footer */}
      <div className="flex items-center justify-between p-4 border-t border-slate-200 dark:border-slate-700">
        <button
          onClick={() => openModal('settings')}
          className="flex items-center gap-2 px-4 py-2 text-sm font-medium
                     text-slate-700 dark:text-slate-300
                     bg-slate-100 dark:bg-slate-800 hover:bg-slate-200 dark:hover:bg-slate-700
                     rounded-lg transition-colors duration-200"
        >
          Settings
        </button>
        
        <div className="flex items-center gap-3">
          <button
            onClick={handleSend}
            disabled={isProcessing}
            className="flex items-center gap-2 px-5 py-2.5 text-sm font-medium text-white
                       bg-green-600 hover:bg-green-700 disabled:bg-green-400
                       rounded-lg transition-colors duration-200
                       focus:outline-none focus:ring-2 focus:ring-green-500 focus:ring-offset-2
                       dark:focus:ring-offset-slate-900"
          >
            {isProcessing ? (
              <>
                <span className="animate-spin">⏳</span>
                Processing...
              </>
            ) : (
              <>
                <span>✓</span>
                Send
              </>
            )}
          </button>
          <button
            onClick={handleCancel}
            className="flex items-center gap-2 px-5 py-2.5 text-sm font-medium text-white
                       bg-red-600 hover:bg-red-700
                       rounded-lg transition-colors duration-200
                       focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2
                       dark:focus:ring-offset-slate-900"
          >
            <span>✕</span>
            Cancel
          </button>
        </div>
      </div>
    </div>
  );
}

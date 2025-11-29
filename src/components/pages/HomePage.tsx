import { useEffect, useRef, KeyboardEvent, useCallback } from 'react';
import { useApp } from '../../context/AppContext';
import { useI18n } from '../../i18n/index';
import { useClipboardSync } from '../../hooks/useClipboardSync';
import { OperationOptionsPanel } from '../OperationOptionsPanel';
import { AudioUpload } from '../AudioUpload';
import { ToastType } from '../../types';

interface HomePageProps {
  onShowToast: (type: ToastType, title: string, message?: string) => void;
}

export function HomePage({ onShowToast }: HomePageProps) {
  const {
    config,
    operations,
    selectedOperation,
    setSelectedOperation,
    promptText,
    setPromptText,
    isProcessing,
    processRequest,
  } = useApp();

  const { t } = useI18n();
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  // Clipboard sync callback - updates prompt text with clipboard content
  const handleClipboardSync = useCallback((text: string) => {
    setPromptText(text);
  }, [setPromptText]);

  // Sync clipboard when window gains focus (respects disableTextSelection setting)
  // Default to disabled when config hasn't loaded yet
  const clipboardSyncEnabled = config ? !config.disableTextSelection : false;
  const { syncClipboard } = useClipboardSync({
    enabled: clipboardSyncEnabled,
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
  };

  const handleSend = async () => {
    if (!selectedOperation) {
      onShowToast('error', t.toast.error, 'Please select an operation');
      return;
    }

    if (!promptText.trim() && selectedOperation.type !== 'speechToText') {
      onShowToast('error', t.toast.error, 'Please enter a prompt');
      return;
    }

    const response = await processRequest();
    if (response && !response.success) {
      onShowToast('error', t.toast.error, response.error || 'Request failed');
    }
  };

  const handleClear = () => {
    setPromptText('');
    textareaRef.current?.focus();
  };

  const isSpeechToText = selectedOperation?.type === 'speechToText';

  return (
    <div className="flex flex-col h-full">
      {/* Page Header */}
      <div className="p-6 border-b border-slate-200 dark:border-slate-800">
        <h2 className="text-2xl font-bold text-slate-900 dark:text-white">
          {t.home.title}
        </h2>
      </div>

      {/* Content */}
      <div className="flex-1 overflow-y-auto p-6">
        <div className="max-w-3xl space-y-6">
          {/* Task Selection */}
          <div className="space-y-2">
            <label
              htmlFor="operation"
              className="block text-sm font-medium text-slate-700 dark:text-slate-300"
            >
              {t.home.taskSelection}
            </label>
            <div className="relative">
              <select
                id="operation"
                value={selectedOperation?.type || ''}
                onChange={handleOperationChange}
                className="w-full px-4 py-3 text-sm rounded-lg border border-slate-300 dark:border-slate-600 
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
            <AudioUpload onError={(msg) => onShowToast('error', t.audio.title, msg)} />
          )}

          {/* Prompt Content */}
          <div className="space-y-2">
            <div className="flex items-center justify-between">
              <label
                htmlFor="prompt"
                className="block text-sm font-medium text-slate-700 dark:text-slate-300"
              >
                {t.home.promptContent}
              </label>
              <div className="flex items-center gap-2">
                {clipboardSyncEnabled && (
                  <button
                    onClick={syncClipboard}
                    className="flex items-center gap-1 px-3 py-1.5 text-xs font-medium
                               text-slate-600 dark:text-slate-400 
                               hover:text-slate-900 dark:hover:text-white
                               bg-slate-100 dark:bg-slate-800 hover:bg-slate-200 dark:hover:bg-slate-700
                               rounded-lg transition-colors"
                    title="Refresh from clipboard"
                  >
                    📋 Sync
                  </button>
                )}
                <button
                  onClick={handleClear}
                  className="flex items-center gap-1 px-3 py-1.5 text-xs font-medium
                             text-slate-600 dark:text-slate-400 
                             hover:text-slate-900 dark:hover:text-white
                             bg-slate-100 dark:bg-slate-800 hover:bg-slate-200 dark:hover:bg-slate-700
                             rounded-lg transition-colors"
                >
                  🗑️ {t.home.clear}
                </button>
              </div>
            </div>
            <textarea
              ref={textareaRef}
              id="prompt"
              value={promptText}
              onChange={(e) => setPromptText(e.target.value)}
              onKeyDown={handleKeyDown}
              placeholder={t.home.enterPrompt}
              className="w-full h-48 px-4 py-3 text-sm rounded-lg border border-slate-300 dark:border-slate-600 
                         bg-white dark:bg-slate-800 text-slate-900 dark:text-white
                         placeholder-slate-400 dark:placeholder-slate-500
                         focus:ring-2 focus:ring-blue-500 focus:border-transparent
                         resize-none transition-colors duration-200"
            />
          </div>
        </div>
      </div>

      {/* Footer */}
      <div className="p-6 border-t border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900">
        <div className="max-w-3xl flex items-center justify-end gap-3">
          <button
            onClick={handleSend}
            disabled={isProcessing}
            className="flex items-center gap-2 px-6 py-2.5 text-sm font-medium text-white
                       bg-green-600 hover:bg-green-700 disabled:bg-green-400
                       rounded-lg transition-colors duration-200 shadow-sm
                       focus:outline-none focus:ring-2 focus:ring-green-500 focus:ring-offset-2
                       dark:focus:ring-offset-slate-900"
          >
            {isProcessing ? (
              <>
                <span className="animate-spin">⏳</span>
                {t.home.processing}
              </>
            ) : (
              <>
                <span>✓</span>
                {t.home.send}
              </>
            )}
          </button>
        </div>
      </div>
    </div>
  );
}

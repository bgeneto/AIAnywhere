import { useEffect, useRef, KeyboardEvent, useCallback, useMemo } from 'react';
import { useApp } from '../../context/AppContext';
import { useI18n } from '../../i18n/index';
import { useClipboardSync } from '../../hooks/useClipboardSync';
import { OperationOptionsPanel } from '../OperationOptionsPanel';
import { AudioUpload } from '../AudioUpload';
import { ToastType } from '../../types';
import { SearchableSelect, Option } from '../ui/SearchableSelect';

interface HomePageProps {
  onShowToast: (type: ToastType, title: string, message?: string) => void;
}

export function HomePage({ onShowToast }: HomePageProps) {
  const {
    config,
    operations,
    customTasks,
    selectedOperation,
    setSelectedOperation,
    promptText,
    setPromptText,
    promptLoadedFromHistory,
    clearPromptLoadedFromHistory,
    isProcessing,
    streamingContent,
    isStreaming,
    clearStreamingContent,
    processRequestStreaming,
    cancelRequest,
  } = useApp();

  const { t } = useI18n();
  const textareaRef = useRef<HTMLTextAreaElement>(null);
  const streamingRef = useRef<HTMLDivElement>(null);

  // Clipboard sync callback - updates prompt text with clipboard content
  // Skip if prompt was loaded from history
  const handleClipboardSync = useCallback((text: string) => {
    if (promptLoadedFromHistory) {
      clearPromptLoadedFromHistory();
      return; // Don't overwrite history-loaded prompt
    }
    setPromptText(text);
  }, [setPromptText, promptLoadedFromHistory, clearPromptLoadedFromHistory]);

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

  // Auto-scroll streaming content to bottom on every update
  useEffect(() => {
    if (streamingRef.current && streamingContent) {
      streamingRef.current.scrollTop = streamingRef.current.scrollHeight;
    }
  }, [streamingContent]);

  const handleOperationSelect = (operationType: string) => {
    // First check default operations
    const operation = operations.find(op => op.type === operationType);
    if (operation) {
      setSelectedOperation(operation);
      return;
    }

    // Then check custom tasks (use id as type for custom tasks)
    const customTask = customTasks.find(task => task.id === operationType);
    if (customTask) {
      // Convert custom task to operation-like structure
      setSelectedOperation({
        type: customTask.id as any, // Custom task uses ID as type
        name: customTask.name,
        description: customTask.description,
        systemPrompt: customTask.systemPrompt,
        options: customTask.options.map(opt => ({
          key: opt.key, // Use key as key for custom tasks
          name: opt.name,
          type: opt.type as 'select' | 'text' | 'number',
          values: opt.values || [],
          defaultValue: opt.defaultValue || '',
          required: opt.required,
        })),
      });
    }
  };

  const selectOptions = useMemo(() => {
    const opts: Option[] = [];

    // Default operations
    operations.forEach(op => {
      opts.push({
        value: op.type,
        label: op.name,
        group: 'Default Tasks'
      });
    });

    // Custom tasks
    customTasks.forEach(task => {
      opts.push({
        value: task.id,
        label: task.name,
        group: t.home.customTasks || 'My Tasks'
      });
    });

    return opts;
  }, [operations, customTasks, t.home.customTasks]);

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

    const response = await processRequestStreaming();
    if (response) {
      if (!response.success) {
        onShowToast('error', t.toast.error, response.error || 'Request failed');
      } else if (config?.pasteBehavior === 'clipboardMode') {
        // Notify user that content was copied to clipboard
        onShowToast('success', t.toast.success, t.review.copied);
      }
    }
  };

  const handleCancel = async () => {
    await cancelRequest();
  };

  const handleClear = () => {
    setPromptText('');
    clearStreamingContent();
    textareaRef.current?.focus();
  };

  const isSpeechToText = selectedOperation?.type === 'speechToText';

  return (
    <div className="flex flex-col h-full">
      {/* Page Header */}
      <div className="px-6 py-4 border-b border-slate-200 dark:border-slate-800 flex-shrink-0">
        <h2 className="page-title">
          {t.home.title}
        </h2>
      </div>

      {/* Content */}
      <div className="flex-1 overflow-hidden p-6 min-h-0">
        <div className="max-w-3xl h-full flex flex-col space-y-4">
          {/* Task Selection */}
          <div className="space-y-2 flex-shrink-0">
            <label
              htmlFor="operation"
              className="form-label"
            >
              {t.home.taskSelection}
            </label>
            <div className="relative">
              <SearchableSelect
                value={selectedOperation?.type || ''}
                onChange={handleOperationSelect}
                options={selectOptions}
                placeholder={t.home.taskSelection}
              />
            </div>
            {selectedOperation && (
              <p className="help-text italic">
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

          {/* Prompt Content and Streaming Preview - Flex container for dynamic height */}
          {/* Hidden for STT since audio is the input */}
          {!isSpeechToText && (
            <div className="flex flex-col gap-4 flex-1 min-h-0">
              {/* Prompt Content */}
              <div className={`flex flex-col gap-2 ${(isStreaming || streamingContent) ? 'flex-shrink-0' : 'flex-1 min-h-0'}`}>
                <div className="flex items-center justify-between">
                  <label
                    htmlFor="prompt"
                    className="form-label"
                  >
                    {t.home.promptContent}
                  </label>
                  <div className="flex items-center gap-2">
                    {clipboardSyncEnabled && (
                      <button
                        onClick={syncClipboard}
                        className="btn-secondary"
                        title="Refresh from clipboard"
                      >
                        📋 Sync
                      </button>
                    )}
                    <button
                      onClick={handleClear}
                      className="btn-secondary"
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
                  className={`w-full px-4 py-3 text-sm rounded-lg border border-slate-300 dark:border-slate-600 
                             bg-white dark:bg-slate-800 text-slate-900 dark:text-white
                             placeholder-slate-400 dark:placeholder-slate-500
                             focus:ring-2 focus:ring-blue-500 focus:border-transparent
                             resize-none transition-colors duration-200
                             ${(isStreaming || streamingContent) ? 'h-24' : 'flex-1 min-h-[100px]'}`}
                />
              </div>

              {/* Streaming Preview */}
              {(isStreaming || streamingContent) && (
                <div className="flex flex-col gap-2 flex-shrink-0">
                  <div className="flex items-center gap-2">
                    <label className="form-label">
                      {t.home.generating || 'Generating...'}
                    </label>
                    {isStreaming && (
                      <span className="inline-block w-2 h-2 bg-green-500 rounded-full animate-pulse" />
                    )}
                  </div>
                  <div
                    ref={streamingRef}
                    className="h-16 w-full px-4 py-2 text-sm rounded-lg border border-slate-200 dark:border-slate-700 
                               bg-slate-50 dark:bg-slate-800/50 text-slate-600 dark:text-slate-300
                               overflow-y-auto whitespace-pre-wrap"
                  >
                    {streamingContent ? (
                      <>
                        {streamingContent}
                        {isStreaming && <span className="inline-block w-0.5 h-4 ml-0.5 bg-green-500 animate-cursor align-middle" />}
                      </>
                    ) : (
                      <span className="text-slate-400 dark:text-slate-500 italic">
                        {t.home.waitingForResponse || 'Waiting for response...'}
                      </span>
                    )}
                  </div>
                </div>
              )}
            </div>
          )}
        </div>
      </div>

      {/* Footer */}
      <div className="px-6 py-4 border-t border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 flex-shrink-0">
        <div className="max-w-3xl flex items-center justify-end gap-3">
          {isProcessing && (
            <button
              onClick={handleCancel}
              className="btn-outline text-red-600 dark:text-red-400 border-red-300 dark:border-red-700 
                         hover:bg-red-50 dark:hover:bg-red-900/20"
            >
              <span>✕</span>
              {t.home.cancel}
            </button>
          )}
          <button
            onClick={handleSend}
            disabled={isProcessing}
            className="btn-primary"
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

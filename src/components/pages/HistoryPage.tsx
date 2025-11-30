import React, { useState, useEffect, useCallback } from 'react';
import { invoke } from '@tauri-apps/api/core';
import { convertFileSrc } from '@tauri-apps/api/core';
import { confirm, save } from '@tauri-apps/plugin-dialog';
import { copyFile } from '@tauri-apps/plugin-fs';
import { open } from '@tauri-apps/plugin-shell';
import { downloadDir } from '@tauri-apps/api/path';
import { useI18n } from '../../i18n/index';
import { useApp } from '../../context/AppContext';
import { HistoryEntry, HistoryEntryResponse } from '../../types';
import ReactMarkdown from 'react-markdown';
import { Prism as SyntaxHighlighter } from 'react-syntax-highlighter';
import { oneDark } from 'react-syntax-highlighter/dist/esm/styles/prism';
import remarkMath from 'remark-math';
import remarkBreaks from 'remark-breaks';
import rehypeKatex from 'rehype-katex';
import 'katex/dist/katex.min.css';

interface HistoryPageProps {
  onNavigateToHome: () => void;
}

export function HistoryPage({ onNavigateToHome }: HistoryPageProps) {
  const { t } = useI18n();
  const { loadHistoryEntry, customTasks, operations } = useApp();
  const [history, setHistory] = useState<HistoryEntryResponse[]>([]);
  const [searchQuery, setSearchQuery] = useState('');
  const [loading, setLoading] = useState(true);
  const [expandedId, setExpandedId] = useState<string | null>(null);

  // Helper to get display name for operation type
  const getOperationName = useCallback((type: string) => {
    // Check custom tasks first (since type is ID)
    const customTask = customTasks.find(t => t.id === type);
    if (customTask) return customTask.name;

    // Check default operations
    const operation = operations.find(op => op.type === type);
    if (operation) return operation.name;

    return type;
  }, [customTasks, operations]);

  // Load history on mount
  const loadHistory = useCallback(async () => {
    try {
      setLoading(true);
      // Always fetch all history, we'll filter on client side
      const entries = await invoke<HistoryEntryResponse[]>('get_history', { searchQuery: null });
      setHistory(entries);
    } catch (error) {
      console.error('Failed to load history:', error);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadHistory();
  }, [loadHistory]);

  // Filter history based on search query
  const filteredHistory = React.useMemo(() => {
    if (!searchQuery.trim()) return history;

    // Normalize query: trim and replace multiple spaces with single space
    const query = searchQuery.trim().replace(/\s+/g, ' ').toLowerCase();

    return history.filter(entry => {
      // Search in prompt
      if (entry.promptText.toLowerCase().includes(query)) return true;

      // Search in response
      if (entry.responseText && entry.responseText.toLowerCase().includes(query)) return true;

      // Search in task type name
      const taskName = getOperationName(entry.operationType);
      if (taskName.toLowerCase().includes(query)) return true;

      return false;
    });
  }, [history, searchQuery, getOperationName]);

  // Handle search with debounce
  const handleSearchChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    setSearchQuery(e.target.value);
  }, []);

  // Re-run a history entry
  const handleRerun = useCallback((entry: HistoryEntryResponse) => {
    const historyEntry: HistoryEntry = {
      id: entry.id,
      operationType: entry.operationType,
      promptText: entry.promptText,
      responseText: entry.responseText,
      operationOptions: entry.operationOptions,
      mediaPath: entry.mediaPath,
      createdAt: entry.createdAt,
    };
    loadHistoryEntry(historyEntry);
    onNavigateToHome();
  }, [loadHistoryEntry, onNavigateToHome]);

  // Delete a single entry
  const handleDelete = useCallback(async (id: string) => {
    const confirmed = await confirm(t.history.confirmDelete, {
      title: t.history.delete,
      kind: 'warning',
    });

    if (confirmed) {
      try {
        await invoke('delete_history_entry', { id });
        setHistory(prev => prev.filter(entry => entry.id !== id));
      } catch (error) {
        console.error('Failed to delete entry:', error);
      }
    }
  }, [t]);

  // Clear all history
  const handleClearAll = useCallback(async () => {
    const confirmed = await confirm(t.history.confirmClearAll, {
      title: t.history.clearAll,
      kind: 'warning',
    });

    if (confirmed) {
      try {
        await invoke('clear_history');
        // Also clear all media files
        await invoke('clear_all_media');
        setHistory([]);
      } catch (error) {
        console.error('Failed to clear history:', error);
      }
    }
  }, [t]);

  // Save media file to user-selected location
  const handleSaveMedia = useCallback(async (mediaPath: string, _operationType: string) => {
    try {
      // Determine file extension and type
      const isImage = mediaPath.match(/\.(png|jpg|jpeg|webp|gif)$/i);
      const isAudio = mediaPath.match(/\.(mp3|wav|ogg|m4a|flac|webm)$/i);
      const extension = mediaPath.split('.').pop() || 'bin';

      const defaultPath = await downloadDir();
      const fileName = isImage ? `image.${extension}` : isAudio ? `audio.${extension}` : `file.${extension}`;

      const savePath = await save({
        filters: [{
          name: isImage ? 'Images' : isAudio ? 'Audio Files' : 'Files',
          extensions: [extension],
        }],
        defaultPath: `${defaultPath}/${fileName}`,
      });

      if (savePath) {
        await copyFile(mediaPath, savePath);
        // Open containing folder
        const folderPath = savePath.substring(0, Math.max(savePath.lastIndexOf('\\'), savePath.lastIndexOf('/')));
        if (folderPath) {
          await open(folderPath);
        }
      }
    } catch (error) {
      console.error('Failed to save media:', error);
    }
  }, []);

  // Toggle expanded entry
  const toggleExpanded = useCallback((id: string) => {
    setExpandedId(prev => prev === id ? null : id);
  }, []);

  // Format date
  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleString();
  };

  // Truncate text for preview
  const truncateText = (text: string, maxLength: number = 150) => {
    if (text.length <= maxLength) return text;
    return text.slice(0, maxLength) + '...';
  };

  return (
    <div className="flex flex-col h-full">
      {/* Header */}
      <div className="p-6 border-b border-slate-200 dark:border-slate-700">
        <div className="flex items-center justify-between mb-4">
          <h1 className="page-title flex items-center gap-2">
            <span>📜</span>
            {t.history.title}
          </h1>
          {history.length > 0 && (
            <button
              onClick={handleClearAll}
              className="btn-text-danger"
            >
              {t.history.clearAll}
            </button>
          )}
        </div>

        {/* Search Bar */}
        <div className="relative">
          <span className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400">🔍</span>
          <input
            type="text"
            value={searchQuery}
            onChange={handleSearchChange}
            placeholder={t.history.searchPlaceholder}
            className="form-input !pl-12"
          />
        </div>
      </div>

      {/* History List */}
      <div className="flex-1 overflow-y-auto p-6">
        {loading ? (
          <div className="flex items-center justify-center h-full text-slate-500 dark:text-slate-400">
            {t.common.loading}
          </div>
        ) : filteredHistory.length === 0 ? (
          <div className="flex flex-col items-center justify-center h-full text-slate-500 dark:text-slate-400">
            <span className="text-4xl mb-4">📭</span>
            <p>{searchQuery ? t.history.noResults : t.history.noHistory}</p>
          </div>
        ) : (
          <div className="space-y-4">
            {filteredHistory.map(entry => (
              <div
                key={entry.id}
                className="card-elevated overflow-hidden"
              >
                {/* Entry Header */}
                <div
                  className="p-4 cursor-pointer hover:bg-slate-100 dark:hover:bg-slate-700/50"
                  onClick={() => toggleExpanded(entry.id)}
                >
                  <div className="flex items-start justify-between gap-4">
                    {/* Thumbnail for media entries */}
                    {entry.mediaPath && entry.mediaPath.match(/\.(png|jpg|jpeg|webp|gif)$/i) && (
                      <div className="flex-shrink-0 w-16 h-16 rounded-lg overflow-hidden bg-slate-100 dark:bg-slate-800">
                        <img
                          src={convertFileSrc(entry.mediaPath)}
                          alt="Thumbnail"
                          className="w-full h-full object-cover"
                          onError={(e) => {
                            // Hide broken images
                            (e.target as HTMLImageElement).style.display = 'none';
                          }}
                        />
                      </div>
                    )}
                    {/* Audio icon for audio entries */}
                    {entry.mediaPath && entry.mediaPath.match(/\.(mp3|wav|ogg|m4a|flac|webm)$/i) && (
                      <div className="flex-shrink-0 w-16 h-16 rounded-lg overflow-hidden bg-slate-100 dark:bg-slate-800 flex items-center justify-center">
                        <span className="text-2xl">🔊</span>
                      </div>
                    )}
                    <div className="flex-1 min-w-0">
                      {/* Task Type Badge */}
                      <div className="flex items-center gap-2 mb-2">
                        <span className="badge-primary">
                          {getOperationName(entry.operationType)}
                        </span>
                        <span className="text-xs text-slate-500 dark:text-slate-400">
                          {formatDate(entry.createdAt)}
                        </span>
                      </div>

                      {/* Prompt Preview */}
                      <p className="text-slate-900 dark:text-white font-medium truncate">
                        {truncateText(entry.promptText, 100)}
                      </p>

                      {/* Response Preview - hide URL for image entries with mediaPath */}
                      {entry.mediaPath && entry.operationType === 'imageGeneration' ? (
                        <p className="text-sm text-slate-600 dark:text-slate-400 mt-1 italic">
                          Image saved to media folder
                        </p>
                      ) : entry.mediaPath && entry.operationType === 'textToSpeech' ? (
                        <p className="text-sm text-slate-600 dark:text-slate-400 mt-1 italic">
                          Audio saved to media folder
                        </p>
                      ) : (
                        <p className="text-sm text-slate-600 dark:text-slate-400 mt-1 line-clamp-2">
                          {truncateText(entry.responseText || '', 150)}
                        </p>
                      )}
                    </div>

                    {/* Expand Icon */}
                    <span className={`text-slate-400 transition-transform ${expandedId === entry.id ? 'rotate-180' : ''}`}>
                      ▼
                    </span>
                  </div>
                </div>

                {/* Expanded Content */}
                {expandedId === entry.id && (
                  <div className="border-t border-slate-200 dark:border-slate-700">
                    {/* Full Prompt */}
                    <div className="p-4 border-b border-slate-200 dark:border-slate-700">
                      <h4 className="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase mb-2">
                        {t.history.prompt}
                      </h4>
                      <p className="text-slate-900 dark:text-white whitespace-pre-wrap text-sm">
                        {entry.promptText}
                      </p>
                    </div>

                    {/* Full Response with Markdown */}
                    <div className="p-4 border-b border-slate-200 dark:border-slate-700">
                      <h4 className="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase mb-2">
                        {t.history.response}
                      </h4>
                      {/* Media thumbnail in expanded view */}
                      {entry.mediaPath && entry.mediaPath.match(/\.(png|jpg|jpeg|webp|gif)$/i) && (
                        <div className="mb-4">
                          <img
                            src={convertFileSrc(entry.mediaPath)}
                            alt="Generated image"
                            className="max-w-full max-h-64 rounded-lg object-contain"
                          />
                        </div>
                      )}
                      {/* Audio player in expanded view */}
                      {entry.mediaPath && entry.mediaPath.match(/\.(mp3|wav|ogg|m4a|flac|webm)$/i) && (
                        <div className="mb-4">
                          <audio
                            controls
                            className="w-full"
                            src={convertFileSrc(entry.mediaPath)}
                          />
                        </div>
                      )}
                      {/* Only show response text if it's not just an image URL */}
                      {entry.responseText && !(entry.mediaPath && entry.operationType === 'imageGeneration') && (
                        <div className="prose prose-slate dark:prose-invert prose-sm max-w-none text-slate-900 dark:text-slate-200 text-sm prose-p:my-1 prose-headings:mt-3 prose-headings:mb-1 [&_.katex-display]:my-2 [&_.katex]:text-inherit">
                          <ReactMarkdown
                            remarkPlugins={[remarkMath, remarkBreaks]}
                            rehypePlugins={[rehypeKatex]}
                            components={{
                              code({ className, children, ...props }) {
                                const match = /language-(\w+)/.exec(className || '');
                                const isInline = !match;
                                return isInline ? (
                                  <code className="bg-slate-100 dark:bg-slate-700 px-1 py-0.5 rounded text-sm" {...props}>
                                    {children}
                                  </code>
                                ) : (
                                  <SyntaxHighlighter
                                    style={oneDark as { [key: string]: React.CSSProperties }}
                                    language={match[1]}
                                    PreTag="div"
                                  >
                                    {String(children).replace(/\n$/, '')}
                                  </SyntaxHighlighter>
                                );
                              },
                            }}
                          >
                            {entry.responseText || ''}
                          </ReactMarkdown>
                        </div>
                      )}
                    </div>

                    {/* Operation Options (if any) */}
                    {entry.operationOptions && Object.keys(entry.operationOptions).length > 0 && (
                      <div className="p-4 border-b border-slate-200 dark:border-slate-700">
                        <h4 className="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase mb-2">
                          Options
                        </h4>
                        <div className="flex flex-wrap gap-2">
                          {Object.entries(entry.operationOptions).map(([key, value]) => (
                            <span
                              key={key}
                              className="badge-neutral"
                            >
                              {key}: {String(value)}
                            </span>
                          ))}
                        </div>
                      </div>
                    )}

                    {/* Actions */}
                    <div className="p-4 flex items-center justify-end gap-3">
                      {/* Save As button for media entries */}
                      {entry.mediaPath && (
                        <button
                          onClick={(e) => {
                            e.stopPropagation();
                            handleSaveMedia(entry.mediaPath!, entry.operationType);
                          }}
                          className="btn-text flex items-center gap-2"
                        >
                          💾 Save As
                        </button>
                      )}
                      <button
                        onClick={(e) => {
                          e.stopPropagation();
                          handleDelete(entry.id);
                        }}
                        className="btn-text-danger"
                      >
                        {t.history.delete}
                      </button>
                      <button
                        onClick={(e) => {
                          e.stopPropagation();
                          handleRerun(entry);
                        }}
                        className="btn-primary"
                      >
                        {t.history.rerun}
                      </button>
                    </div>
                  </div>
                )}
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

import { useState, useEffect } from 'react';
import { open } from '@tauri-apps/plugin-dialog';
import { listen } from '@tauri-apps/api/event';
import { useApp } from '../context/AppContext';
import { formatFileSize, MAX_AUDIO_FILE_SIZE, SUPPORTED_AUDIO_FORMATS } from '../types';

interface AudioUploadProps {
  onError: (message: string) => void;
}

interface TauriFileDrop {
  paths: string[];
  position: { x: number; y: number };
}

export function AudioUpload({ onError }: AudioUploadProps) {
  const { audioFilePath, setAudioFilePath } = useApp();
  const [isDragging, setIsDragging] = useState(false);

  // Listen for Tauri file drop events
  useEffect(() => {
    const unlistenDrop = listen<TauriFileDrop>('tauri://drag-drop', (event) => {
      const paths = event.payload.paths;
      if (paths.length > 0) {
        const filePath = paths[0];
        const extension = filePath.split('.').pop()?.toLowerCase() || '';
        
        if (SUPPORTED_AUDIO_FORMATS.includes(extension)) {
          setAudioFilePath(filePath);
        } else {
          onError(`Unsupported format. Supported: ${SUPPORTED_AUDIO_FORMATS.join(', ')}`);
        }
      }
      setIsDragging(false);
    });

    const unlistenEnter = listen('tauri://drag-enter', () => {
      setIsDragging(true);
    });

    const unlistenLeave = listen('tauri://drag-leave', () => {
      setIsDragging(false);
    });

    return () => {
      unlistenDrop.then(fn => fn());
      unlistenEnter.then(fn => fn());
      unlistenLeave.then(fn => fn());
    };
  }, [setAudioFilePath, onError]);

  const handleBrowse = async () => {
    try {
      const selected = await open({
        multiple: false,
        filters: [{
          name: 'Audio Files',
          extensions: SUPPORTED_AUDIO_FORMATS,
        }],
      });
      
      if (selected && typeof selected === 'string') {
        setAudioFilePath(selected);
      }
    } catch (error) {
      console.error('Failed to open file dialog:', error);
      onError('Failed to open file dialog');
    }
  };

  const handleClear = () => {
    setAudioFilePath('');
  };

  const fileName = audioFilePath ? audioFilePath.split(/[/\\]/).pop() : '';

  return (
    <div className="space-y-2">
      <label className="block text-sm font-medium text-slate-700 dark:text-slate-300">
        Audio File
      </label>
      
      <div
        className={`
          relative border-2 border-dashed rounded-lg p-4 transition-all duration-200
          ${isDragging 
            ? 'border-blue-500 bg-blue-50 dark:bg-blue-900/20' 
            : 'border-slate-300 dark:border-slate-600 hover:border-blue-400 dark:hover:border-blue-500'
          }
        `}
      >
        {audioFilePath ? (
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2 text-sm text-slate-700 dark:text-slate-300">
              <span className="text-xl">🎵</span>
              <span className="truncate max-w-xs">{fileName}</span>
            </div>
            <button
              onClick={handleClear}
              className="text-slate-400 hover:text-red-500 transition-colors"
              title="Remove file"
            >
              ✕
            </button>
          </div>
        ) : (
          <div className="text-center">
            <div className="text-3xl mb-2">{isDragging ? '📥' : '🎤'}</div>
            <p className="text-sm text-slate-600 dark:text-slate-400 mb-2">
              {isDragging ? 'Drop audio file here' : 'Drag and drop an audio file here, or'}
            </p>
            {!isDragging && (
              <button
                onClick={handleBrowse}
                className="btn-outline"
              >
                Browse Files
              </button>
            )}
            <p className="text-xs text-slate-500 dark:text-slate-500 mt-2">
              Supported: {SUPPORTED_AUDIO_FORMATS.slice(0, 5).join(', ')}... (Max {formatFileSize(MAX_AUDIO_FILE_SIZE)})
            </p>
          </div>
        )}
      </div>
    </div>
  );
}

import React, { useEffect, useState } from 'react';

type ToastType = 'success' | 'error' | 'warning' | 'info';

export interface ToastMessage {
  id: string;
  type: ToastType;
  title: string;
  message?: string;
}

interface ToastProps {
  toast: ToastMessage;
  onClose: (id: string) => void;
  duration?: number;
}

const Toast: React.FC<ToastProps> = ({ toast, onClose, duration = 2500 }) => {
  const [progress, setProgress] = useState(100);

  useEffect(() => {
    const timer = setTimeout(() => {
      onClose(toast.id);
    }, duration);

    // Update progress bar every 50ms for smooth animation
    const progressInterval = setInterval(() => {
      setProgress((prev) => Math.max(0, prev - (100 / (duration / 50))));
    }, 50);

    return () => {
      clearTimeout(timer);
      clearInterval(progressInterval);
    };
  }, [toast.id, onClose, duration]);

  const icons = {
    success: '✓',
    error: '⚠️',
    warning: '⚡',
    info: 'ℹ️',
  };

  const bgColors = {
    success: 'bg-slate-50 dark:bg-slate-800 border-slate-200 dark:border-slate-700',
    error: 'bg-slate-50 dark:bg-slate-800 border-slate-200 dark:border-slate-700',
    warning: 'bg-slate-50 dark:bg-slate-800 border-slate-200 dark:border-slate-700',
    info: 'bg-slate-50 dark:bg-slate-800 border-slate-200 dark:border-slate-700',
  };

  const titleColors = {
    success: 'text-slate-900 dark:text-white',
    error: 'text-slate-900 dark:text-white',
    warning: 'text-slate-900 dark:text-white',
    info: 'text-slate-900 dark:text-white',
  };

  const iconColors = {
    success: 'text-emerald-600 dark:text-emerald-400',
    error: 'text-red-600 dark:text-red-400',
    warning: 'text-amber-600 dark:text-amber-400',
    info: 'text-blue-600 dark:text-blue-400',
  };

  const progressBarColors = {
    success: 'bg-emerald-500',
    error: 'bg-red-500',
    warning: 'bg-amber-500',
    info: 'bg-blue-500',
  };

  return (
    <div
      className={`
        border rounded-lg overflow-hidden flex flex-col animate-in fade-in slide-in-from-top-2 duration-300 shadow-lg
        ${bgColors[toast.type]}
      `}
    >
      {/* Toast content */}
      <div className="flex gap-3 items-start p-4">
        <span className={`text-xl shrink-0 ${iconColors[toast.type]}`}>{icons[toast.type]}</span>
        <div className="flex-1">
          <div className={`font-semibold ${titleColors[toast.type]}`}>{toast.title}</div>
          {toast.message && <div className="text-sm mt-1">{toast.message}</div>}
        </div>
        <button
          onClick={() => onClose(toast.id)}
          className="shrink-0 text-xl opacity-50 hover:opacity-100 transition-opacity"
        >
          ✕
        </button>
      </div>

      {/* Progress bar */}
      <div className="h-1 bg-slate-300 dark:bg-slate-700 overflow-hidden">
        <div
          className={`h-full ${progressBarColors[toast.type]} transition-all duration-75`}
          style={{ width: `${progress}%` }}
        />
      </div>
    </div>
  );
};

export default Toast;

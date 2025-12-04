import React, { useEffect, useRef, useState } from 'react';

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
  const [isPaused, setIsPaused] = useState(false);
  const progressRef = useRef(progress);
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null);

  // Keep progressRef in sync
  useEffect(() => {
    progressRef.current = progress;
  }, [progress]);

  useEffect(() => {
    if (isPaused) {
      // Clear timers when paused
      if (timerRef.current) {
        clearTimeout(timerRef.current);
        timerRef.current = null;
      }
      if (intervalRef.current) {
        clearInterval(intervalRef.current);
        intervalRef.current = null;
      }
      return;
    }

    // Calculate remaining time based on current progress
    const remainingTime = (progressRef.current / 100) * duration;

    timerRef.current = setTimeout(() => {
      onClose(toast.id);
    }, remainingTime);

    // Update progress bar every 50ms for smooth animation
    intervalRef.current = setInterval(() => {
      setProgress((prev) => Math.max(0, prev - (100 / (duration / 50))));
    }, 50);

    return () => {
      if (timerRef.current) clearTimeout(timerRef.current);
      if (intervalRef.current) clearInterval(intervalRef.current);
    };
  }, [toast.id, onClose, duration, isPaused]);

  const handleMouseEnter = () => setIsPaused(true);
  const handleMouseLeave = () => setIsPaused(false);

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
        border rounded-lg overflow-hidden flex flex-col animate-in fade-in slide-in-from-top-2 duration-300 shadow-lg cursor-default
        ${bgColors[toast.type]}
      `}
      onMouseEnter={handleMouseEnter}
      onMouseLeave={handleMouseLeave}
    >
      {/* Toast content */}
      <div className="flex gap-3 items-start p-4">
        <span className={`text-xl shrink-0 ${iconColors[toast.type]}`}>{icons[toast.type]}</span>
        <div className="flex-1">
          <div className={`font-semibold ${titleColors[toast.type]}`}>{toast.title}</div>
          {toast.message && <div className="text-sm mt-1 text-slate-600 dark:text-slate-300">{toast.message}</div>}
        </div>
        <button
          onClick={() => onClose(toast.id)}
          className="shrink-0 text-xl text-slate-600 dark:text-slate-400 hover:text-slate-900 dark:hover:text-slate-200 transition-colors"
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

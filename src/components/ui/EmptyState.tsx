import React from 'react';

interface EmptyStateProps {
  icon: string;
  message: string;
  action?: React.ReactNode;
}

export function EmptyState({ icon, message, action }: EmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center h-full text-slate-500 dark:text-slate-400">
      <span className="text-4xl mb-4">{icon}</span>
      <p className="mb-4">{message}</p>
      {action}
    </div>
  );
}

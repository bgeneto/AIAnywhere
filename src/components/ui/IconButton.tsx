import React from 'react';

interface IconButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  icon: string;
  variant?: 'ghost' | 'subtle';
  size?: 'sm' | 'md';
  label?: string;
}

export function IconButton({ 
  icon, 
  variant = 'ghost', 
  size = 'md', 
  label, 
  className = '', 
  ...props 
}: IconButtonProps) {
  const baseClass = variant === 'ghost' 
    ? 'btn-ghost' 
    : 'p-1 text-slate-400 hover:text-slate-600 dark:hover:text-slate-300 transition-colors';
  const sizeClass = size === 'sm' ? 'text-sm' : '';
  
  return (
    <button 
      className={`${baseClass} ${sizeClass} ${className}`} 
      aria-label={label}
      {...props}
    >
      {icon}
    </button>
  );
}

import React from 'react';
import { IconButton } from './IconButton';

interface ModalProps {
  title: string;
  subtitle?: string;
  icon?: React.ReactNode;
  onClose: () => void;
  footer?: React.ReactNode;
  children: React.ReactNode;
  size?: 'sm' | 'md' | 'lg';
}

export function Modal({ 
  title, 
  subtitle, 
  icon, 
  onClose, 
  footer, 
  children, 
  size = 'md' 
}: ModalProps) {
  const sizeClass = { 
    sm: 'max-w-sm', 
    md: 'max-w-2xl', 
    lg: 'max-w-4xl' 
  }[size];
  
  return (
    <div className="modal-overlay">
      <div className={`modal-container w-full ${sizeClass} max-h-[90vh] flex flex-col`}>
        {/* Header */}
        <div className="flex items-center justify-between p-4 border-b border-slate-200 dark:border-slate-700">
          <div className="flex items-center gap-3">
            {icon && <div>{icon}</div>}
            <div>
              <h2 className="text-lg font-semibold text-slate-900 dark:text-white">{title}</h2>
              {subtitle && <p className="text-sm text-slate-500 dark:text-slate-400">{subtitle}</p>}
            </div>
          </div>
          <IconButton icon="✕" onClick={onClose} label="Close" />
        </div>
        
        {/* Content */}
        <div className="flex-1 overflow-y-auto p-4">
          {children}
        </div>
        
        {/* Footer */}
        {footer && (
          <div className="flex items-center justify-end gap-2 p-4 border-t border-slate-200 dark:border-slate-700">
            {footer}
          </div>
        )}
      </div>
    </div>
  );
}

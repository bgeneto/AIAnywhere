import React from 'react';

interface PageLayoutProps {
  title: React.ReactNode;
  headerActions?: React.ReactNode;
  footer?: React.ReactNode;
  children: React.ReactNode;
  maxWidth?: 'sm' | 'md' | 'lg' | 'full';
}

export function PageLayout({ 
  title, 
  headerActions, 
  footer, 
  children, 
  maxWidth = 'full' 
}: PageLayoutProps) {
  const maxWidthClass = {
    sm: 'max-w-lg',
    md: 'max-w-2xl', 
    lg: 'max-w-4xl',
    full: '',
  }[maxWidth];

  return (
    <div className="flex flex-col h-full">
      <div className="page-header">
        <div className="flex items-center justify-between">
          <h2 className="page-title">{title}</h2>
          {headerActions}
        </div>
      </div>
      <div className="flex-1 overflow-y-auto p-6">
        <div className={maxWidthClass}>{children}</div>
      </div>
      {footer && (
        <div className="page-footer">
          <div className="flex items-center justify-end gap-3">
            {footer}
          </div>
        </div>
      )}
    </div>
  );
}

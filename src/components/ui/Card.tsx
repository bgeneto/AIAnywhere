import React from 'react';

interface CardProps {
  children: React.ReactNode;
  elevated?: boolean;
  padding?: 'none' | 'sm' | 'md';
  className?: string;
}

export function Card({ children, elevated, padding = 'md', className = '' }: CardProps) {
  const paddingClass = { none: '', sm: 'p-3', md: 'p-4' }[padding];
  const elevatedClass = elevated ? 'card-elevated' : 'card';
  
  return (
    <div className={`${elevatedClass} ${paddingClass} ${className}`}>
      {children}
    </div>
  );
}

import React from 'react';

interface BadgeProps {
  children: React.ReactNode;
  variant?: 'neutral' | 'primary' | 'success' | 'warning' | 'danger' | 'mono';
}

export function Badge({ children, variant = 'neutral' }: BadgeProps) {
  const variantClass = `badge-${variant}`;
  return <span className={variantClass}>{children}</span>;
}

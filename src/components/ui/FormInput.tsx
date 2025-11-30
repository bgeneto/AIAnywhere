import React from 'react';

interface FormInputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  size?: 'sm' | 'md';
}

export function FormInput({ size = 'md', className = '', ...props }: FormInputProps) {
  const sizeClasses = size === 'sm' ? 'form-input-sm' : 'form-input';
  return <input className={`${sizeClasses} ${className}`} {...props} />;
}

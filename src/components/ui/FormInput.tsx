import React from 'react';

interface FormInputProps extends Omit<React.InputHTMLAttributes<HTMLInputElement>, 'size'> {
  size?: 'sm' | 'md';
}

export function FormInput({ size = 'md', className = '', ...props }: FormInputProps) {
  const sizeClasses = size === 'sm' ? 'form-input-sm' : 'form-input';
  return <input className={`${sizeClasses} ${className}`} {...props} />;
}

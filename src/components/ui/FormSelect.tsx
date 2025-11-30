import React from 'react';

interface FormSelectProps extends React.SelectHTMLAttributes<HTMLSelectElement> {
  options: { value: string; label: string }[];
  size?: 'sm' | 'md';
  placeholder?: string;
}

export function FormSelect({ 
  options, 
  size = 'sm', 
  placeholder,
  className = '', 
  ...props 
}: FormSelectProps) {
  const sizeClasses = size === 'sm' ? 'form-input-sm' : 'form-input';
  
  return (
    <div className="relative w-full">
      <select 
        className={`${sizeClasses} ${className}`} 
        {...props}
      >
        {placeholder && <option value="">{placeholder}</option>}
        {options.map(opt => (
          <option key={opt.value} value={opt.value}>{opt.label}</option>
        ))}
      </select>
    </div>
  );
}

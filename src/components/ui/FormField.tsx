import React from 'react';

interface FormFieldProps {
  label: string;
  helpText?: string;
  required?: boolean;
  error?: string;
  children: React.ReactNode;
}

export function FormField({ label, helpText, required, error, children }: FormFieldProps) {
  return (
    <div className="space-y-2">
      <label className="form-label">
        {label}
        {required && <span className="text-red-500 ml-1">*</span>}
      </label>
      {children}
      {helpText && <p className="help-text">{helpText}</p>}
      {error && <p className="text-xs text-red-500">{error}</p>}
    </div>
  );
}

import { useApp } from '../context/AppContext';
import { OperationOption } from '../types';

export function OperationOptionsPanel() {
  const { selectedOperation, operationOptions, setOperationOption } = useApp();

  if (!selectedOperation || selectedOperation.options.length === 0) {
    return null;
  }

  const renderOption = (option: OperationOption) => {
    const value = operationOptions[option.key] || option.defaultValue;

    switch (option.type) {
      case 'select':
        return (
          <div key={option.key} className="space-y-1">
            <label 
              htmlFor={option.key}
              className="block text-sm font-medium text-slate-700 dark:text-slate-300"
            >
              {option.name}
              {option.required && <span className="text-red-500 ml-1">*</span>}
            </label>
            <select
              id={option.key}
              value={value}
              onChange={(e) => setOperationOption(option.key, e.target.value)}
              className="w-full px-3 py-2 text-sm rounded-lg border border-slate-300 dark:border-slate-600 
                         bg-white dark:bg-slate-800 text-slate-900 dark:text-white
                         focus:ring-2 focus:ring-blue-500 focus:border-transparent
                         transition-colors duration-200"
            >
              {option.values.map((v) => (
                <option key={v} value={v}>
                  {v}
                </option>
              ))}
            </select>
          </div>
        );

      case 'text':
        return (
          <div key={option.key} className="space-y-1">
            <label 
              htmlFor={option.key}
              className="block text-sm font-medium text-slate-700 dark:text-slate-300"
            >
              {option.name}
              {option.required && <span className="text-red-500 ml-1">*</span>}
            </label>
            <input
              type="text"
              id={option.key}
              value={value}
              onChange={(e) => setOperationOption(option.key, e.target.value)}
              placeholder={option.defaultValue}
              className="w-full px-3 py-2 text-sm rounded-lg border border-slate-300 dark:border-slate-600 
                         bg-white dark:bg-slate-800 text-slate-900 dark:text-white
                         placeholder-slate-400 dark:placeholder-slate-500
                         focus:ring-2 focus:ring-blue-500 focus:border-transparent
                         transition-colors duration-200"
            />
          </div>
        );

      case 'number':
        return (
          <div key={option.key} className="space-y-1">
            <label 
              htmlFor={option.key}
              className="block text-sm font-medium text-slate-700 dark:text-slate-300"
            >
              {option.name}
              {option.required && <span className="text-red-500 ml-1">*</span>}
            </label>
            <input
              type="number"
              id={option.key}
              value={value}
              onChange={(e) => setOperationOption(option.key, e.target.value)}
              placeholder={option.defaultValue}
              className="w-full px-3 py-2 text-sm rounded-lg border border-slate-300 dark:border-slate-600 
                         bg-white dark:bg-slate-800 text-slate-900 dark:text-white
                         placeholder-slate-400 dark:placeholder-slate-500
                         focus:ring-2 focus:ring-blue-500 focus:border-transparent
                         transition-colors duration-200"
            />
          </div>
        );

      default:
        return null;
    }
  };

  return (
    <div className="grid grid-cols-2 gap-3">
      {selectedOperation.options.map(renderOption)}
    </div>
  );
}

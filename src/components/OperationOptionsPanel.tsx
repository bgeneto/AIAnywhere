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
              className="form-label"
            >
              {option.name}
              {option.required && <span className="text-red-500 ml-1">*</span>}
            </label>
            <select
              id={option.key}
              value={value}
              onChange={(e) => setOperationOption(option.key, e.target.value)}
              className="form-input-sm"
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
              className="form-label"
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
              className="form-input-sm"
            />
          </div>
        );

      case 'number':
        return (
          <div key={option.key} className="space-y-1">
            <label 
              htmlFor={option.key}
              className="form-label"
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
              className="form-input-sm"
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

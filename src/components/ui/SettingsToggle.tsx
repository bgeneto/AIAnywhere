interface SettingsToggleProps {
  title: string;
  description?: string;
  checked: boolean;
  onChange: (checked: boolean) => void;
}

export function SettingsToggle({ title, description, checked, onChange }: SettingsToggleProps) {
  return (
    <label className="settings-row flex items-center justify-between cursor-pointer">
      <div>
        <div className="text-sm font-medium text-slate-700 dark:text-slate-300">{title}</div>
        {description && <div className="help-text">{description}</div>}
      </div>
      <input
        type="checkbox"
        checked={checked}
        onChange={(e) => onChange(e.target.checked)}
        className="w-5 h-5 rounded border-slate-300 dark:border-slate-600 text-blue-600 focus:ring-blue-500"
      />
    </label>
  );
}

import { useState, useEffect, useCallback } from 'react';
import { invoke } from '@tauri-apps/api/core';
import { confirm } from '@tauri-apps/plugin-dialog';
import { save, open } from '@tauri-apps/plugin-dialog';
import { readTextFile, writeTextFile } from '@tauri-apps/plugin-fs';
import { useI18n } from '../../i18n/index';
import { useApp } from '../../context/AppContext';
import { CustomTask, CustomTaskOption } from '../../types';
import { 
  FormField, FormInput, FormSelect, Card, PageLayout, EmptyState, Badge 
} from '../ui';

interface CustomTasksPageProps {
  showToast: (type: 'success' | 'error' | 'warning' | 'info', title: string, message?: string) => void;
}

export function CustomTasksPage({ showToast }: CustomTasksPageProps) {
  const { t } = useI18n();
  const { customTasks, loadCustomTasks } = useApp();
  const [editingTask, setEditingTask] = useState<CustomTask | null>(null);
  const [isCreating, setIsCreating] = useState(false);

  // Form state
  const [formName, setFormName] = useState('');
  const [formDescription, setFormDescription] = useState('');
  const [formSystemPrompt, setFormSystemPrompt] = useState('');
  const [formOptions, setFormOptions] = useState<CustomTaskOption[]>([]);
  const [validationError, setValidationError] = useState<string | null>(null);

  // Load tasks on mount
  useEffect(() => {
    loadCustomTasks();
  }, [loadCustomTasks]);

  // Reset form when editing changes
  useEffect(() => {
    if (editingTask) {
      setFormName(editingTask.name);
      setFormDescription(editingTask.description);
      setFormSystemPrompt(editingTask.systemPrompt);
      setFormOptions(editingTask.options || []);
    } else if (isCreating) {
      setFormName('');
      setFormDescription('');
      setFormSystemPrompt('');
      setFormOptions([]);
    }
    setValidationError(null);
  }, [editingTask, isCreating]);

  // Create new task
  const handleCreate = useCallback(() => {
    setIsCreating(true);
    setEditingTask(null);
  }, []);

  // Edit existing task
  const handleEdit = useCallback((task: CustomTask) => {
    setEditingTask(task);
    setIsCreating(false);
  }, []);

  // Cancel editing
  const handleCancel = useCallback(() => {
    setEditingTask(null);
    setIsCreating(false);
    setValidationError(null);
  }, []);

  // Add option
  const handleAddOption = useCallback(() => {
    const newOption: CustomTaskOption = {
      name: '',
      optionType: 'select',
      required: false,
      values: [],
      defaultValue: '',
    };
    setFormOptions(prev => [...prev, newOption]);
  }, []);

  // Remove option
  const handleRemoveOption = useCallback((index: number) => {
    setFormOptions(prev => prev.filter((_, i) => i !== index));
  }, []);

  // Update option
  const handleUpdateOption = useCallback((index: number, updates: Partial<CustomTaskOption>) => {
    setFormOptions(prev => prev.map((opt, i) => {
      if (i !== index) return opt;
      return { ...opt, ...updates };
    }));
  }, []);

  // Save task
  const handleSave = useCallback(async () => {
    // Validate
    if (!formName.trim()) {
      setValidationError('Name is required');
      return;
    }
    if (!formSystemPrompt.trim()) {
      setValidationError('System prompt is required');
      return;
    }

    // Validate option placeholders match
    const optionNames = formOptions.map(opt => opt.name).filter(n => n);
    const placeholderRegex = /\{(\w+)\}/g;
    const placeholders = [...formSystemPrompt.matchAll(placeholderRegex)].map(m => m[1]);
    
    for (const optName of optionNames) {
      if (!placeholders.includes(optName)) {
        setValidationError(`System prompt missing placeholder for option: {${optName}}`);
        return;
      }
    }

    try {
      const taskData = {
        name: formName.trim(),
        description: formDescription.trim(),
        systemPrompt: formSystemPrompt,
        options: formOptions.filter(opt => opt.name.trim()),
      };

      if (editingTask) {
        await invoke('update_custom_task', { id: editingTask.id, task: taskData });
      } else {
        await invoke('create_custom_task', { task: taskData });
      }

      await loadCustomTasks();
      handleCancel();
      showToast('success', editingTask ? 'Task updated' : 'Task created');
    } catch (error) {
      console.error('Failed to save task:', error);
      setValidationError(String(error));
    }
  }, [formName, formDescription, formSystemPrompt, formOptions, editingTask, loadCustomTasks, handleCancel, showToast]);

  // Delete task
  const handleDelete = useCallback(async (id: string) => {
    const confirmed = await confirm(t.customTasks.confirmDelete, {
      title: t.customTasks.delete,
      kind: 'warning',
    });

    if (confirmed) {
      try {
        await invoke('delete_custom_task', { id });
        await loadCustomTasks();
        showToast('success', t.history.deleted);
      } catch (error) {
        console.error('Failed to delete task:', error);
      }
    }
  }, [t, loadCustomTasks, showToast]);

  // Export tasks
  const handleExport = useCallback(async () => {
    try {
      const filePath = await save({
        defaultPath: 'custom-tasks.json',
        filters: [{ name: 'JSON', extensions: ['json'] }],
      });

      if (filePath) {
        const json = await invoke<string>('export_custom_tasks');
        await writeTextFile(filePath, json);
        showToast('success', t.customTasks.exported);
      }
    } catch (error) {
      console.error('Failed to export tasks:', error);
      showToast('error', 'Export failed');
    }
  }, [t, showToast]);

  // Import tasks
  const handleImport = useCallback(async () => {
    try {
      const filePath = await open({
        filters: [{ name: 'JSON', extensions: ['json'] }],
        multiple: false,
      });

      if (filePath && typeof filePath === 'string') {
        const content = await readTextFile(filePath);
        await invoke('import_custom_tasks', { json: content });
        await loadCustomTasks();
        showToast('success', t.customTasks.imported);
      }
    } catch (error) {
      console.error('Failed to import tasks:', error);
      showToast('error', t.customTasks.importFailed);
    }
  }, [t, loadCustomTasks, showToast]);

  // Render option editor
  const renderOptionEditor = (option: CustomTaskOption, index: number) => (
    <div key={index} className="p-4 bg-slate-50 dark:bg-slate-800/50 rounded-lg border border-slate-200 dark:border-slate-700">
      <div className="flex items-center justify-between mb-3">
        <span className="text-sm font-medium text-slate-700 dark:text-slate-300">
          Option {index + 1}
        </span>
        <button
          onClick={() => handleRemoveOption(index)}
          className="text-xs text-red-600 dark:text-red-400 hover:text-red-700 dark:hover:text-red-300"
        >
          {t.customTasks.removeOption}
        </button>
      </div>

      <div className="grid grid-cols-2 gap-3">
        {/* Name */}
        <div>
          <label className="block text-xs font-medium text-slate-600 dark:text-slate-400 mb-1">
            {t.customTasks.optionName}
          </label>
          <input
            type="text"
            value={option.name}
            onChange={(e) => handleUpdateOption(index, { name: e.target.value.replace(/\s/g, '_').toLowerCase() })}
            placeholder={t.customTasks.optionNamePlaceholder}
            className="w-full px-3 py-2 text-sm rounded border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-700 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        {/* Type */}
        <div>
          <label className="block text-xs font-medium text-slate-600 dark:text-slate-400 mb-1">
            {t.customTasks.optionType}
          </label>
          <select
            value={option.optionType}
            onChange={(e) => handleUpdateOption(index, { optionType: e.target.value as 'select' | 'text' | 'number' })}
            className="w-full px-3 py-2 text-sm rounded border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-700 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="select">{t.customTasks.optionTypeSelect}</option>
            <option value="text">{t.customTasks.optionTypeText}</option>
            <option value="number">{t.customTasks.optionTypeNumber}</option>
          </select>
        </div>

        {/* Values (for select type) */}
        {option.optionType === 'select' && (
          <div className="col-span-2">
            <label className="block text-xs font-medium text-slate-600 dark:text-slate-400 mb-1">
              {t.customTasks.optionValues}
            </label>
            <input
              type="text"
              value={option.values?.join(', ') || ''}
              onChange={(e) => handleUpdateOption(index, { values: e.target.value.split(',').map(v => v.trim()).filter(v => v) })}
              placeholder={t.customTasks.optionValuesPlaceholder}
              className="w-full px-3 py-2 text-sm rounded border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-700 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
        )}

        {/* Number options */}
        {option.optionType === 'number' && (
          <>
            <div>
              <label className="block text-xs font-medium text-slate-600 dark:text-slate-400 mb-1">
                {t.customTasks.optionMin}
              </label>
              <input
                type="number"
                value={option.min ?? ''}
                onChange={(e) => handleUpdateOption(index, { min: e.target.value ? Number(e.target.value) : undefined })}
                className="w-full px-3 py-2 text-sm rounded border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-700 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-slate-600 dark:text-slate-400 mb-1">
                {t.customTasks.optionMax}
              </label>
              <input
                type="number"
                value={option.max ?? ''}
                onChange={(e) => handleUpdateOption(index, { max: e.target.value ? Number(e.target.value) : undefined })}
                className="w-full px-3 py-2 text-sm rounded border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-700 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
          </>
        )}

        {/* Default Value */}
        <div className="col-span-2">
          <label className="block text-xs font-medium text-slate-600 dark:text-slate-400 mb-1">
            {t.customTasks.optionDefault}
          </label>
          {option.optionType === 'select' && option.values && option.values.length > 0 ? (
            <select
              value={option.defaultValue || ''}
              onChange={(e) => handleUpdateOption(index, { defaultValue: e.target.value })}
              className="w-full px-3 py-2 text-sm rounded border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-700 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="">Select default...</option>
              {option.values.map(v => (
                <option key={v} value={v}>{v}</option>
              ))}
            </select>
          ) : (
            <input
              type={option.optionType === 'number' ? 'number' : 'text'}
              value={option.defaultValue || ''}
              onChange={(e) => handleUpdateOption(index, { defaultValue: e.target.value })}
              className="w-full px-3 py-2 text-sm rounded border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-700 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          )}
        </div>
      </div>
    </div>
  );

  // Show editor form
  if (isCreating || editingTask) {
    return (
      <PageLayout
        title={editingTask ? t.customTasks.edit : t.customTasks.create}
        footer={
          <>
            <button
              onClick={handleCancel}
              className="btn-outline"
            >
              {t.customTasks.cancel}
            </button>
            <button
              onClick={handleSave}
              className="btn-primary"
            >
              {t.customTasks.save}
            </button>
          </>
        }
      >
          {/* Validation Error */}
          {validationError && (
            <div className="p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg text-red-700 dark:text-red-300 text-sm">
              {validationError}
            </div>
          )}

          {/* Name */}
          <FormField label={t.customTasks.name} required>
            <FormInput
              type="text"
              value={formName}
              onChange={(e) => setFormName(e.target.value)}
              placeholder={t.customTasks.namePlaceholder}
            />
          </FormField>

          {/* Description */}
          <FormField label={t.customTasks.description}>
            <FormInput
              type="text"
              value={formDescription}
              onChange={(e) => setFormDescription(e.target.value)}
              placeholder={t.customTasks.descriptionPlaceholder}
            />
          </FormField>

          {/* System Prompt */}
          <FormField label={t.customTasks.systemPrompt} required helpText={t.customTasks.systemPromptHelp}>
            <textarea
              value={formSystemPrompt}
              onChange={(e) => setFormSystemPrompt(e.target.value)}
              placeholder={t.customTasks.systemPromptPlaceholder}
              rows={6}
              className="w-full px-4 py-3 rounded-lg border border-slate-300 dark:border-slate-600 bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500 font-mono text-sm"
            />
          </FormField>

          {/* Options */}
          <div>
            <div className="flex items-center justify-between mb-3">
              <label className="block text-sm font-medium text-slate-700 dark:text-slate-300">
                {t.customTasks.options}
              </label>
              <button
                onClick={handleAddOption}
                className="btn-text-primary"
              >
                + {t.customTasks.addOption}
              </button>
            </div>

            <div className="space-y-3">
              {formOptions.map((option, index) => renderOptionEditor(option, index))}
              {formOptions.length === 0 && (
                <p className="text-sm text-slate-500 dark:text-slate-400 text-center py-4">
                  No options yet. Add options to create dynamic form fields.
                </p>
              )}
            </div>
          </div>
      </PageLayout>
    );
  }

  // Show task list
  return (
    <PageLayout
      title={<><span>✨</span> {t.customTasks.title}</>}
      headerActions={
        customTasks.length > 0 && (
          <div className="flex items-center gap-2">
            <button
              onClick={handleExport}
              className="btn-text-neutral"
            >
              {t.customTasks.exportTasks}
            </button>
            <button
              onClick={handleImport}
              className="btn-text-neutral"
            >
              {t.customTasks.importTasks}
            </button>
            <button
              onClick={handleCreate}
              className="btn-primary"
            >
              + {t.customTasks.create}
            </button>
          </div>
        )
      }
    >
      {customTasks.length === 0 ? (
        <EmptyState
          icon="✨"
          message={t.customTasks.noTasks}
          action={
            <div className="flex gap-2">
              <button
                onClick={handleImport}
                className="btn-outline"
              >
                {t.customTasks.importTasks}
              </button>
              <button
                onClick={handleCreate}
                className="btn-primary"
              >
                + {t.customTasks.create}
              </button>
            </div>
          }
        />
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {customTasks.map(task => (
            <Card key={task.id} elevated>
              <div className="flex items-start justify-between mb-3">
                <h3 className="font-semibold text-slate-900 dark:text-white">
                  {task.name}
                </h3>
                <div className="flex items-center gap-1">
                  <button
                    onClick={() => handleEdit(task)}
                    className="p-1 text-slate-400 hover:text-slate-600 dark:hover:text-slate-300"
                    title={t.customTasks.edit}
                  >
                    ✏️
                  </button>
                  <button
                    onClick={() => handleDelete(task.id)}
                    className="p-1 text-slate-400 hover:text-red-600 dark:hover:text-red-400"
                    title={t.customTasks.delete}
                  >
                    🗑️
                  </button>
                </div>
              </div>

              {task.description && (
                <p className="text-sm text-slate-600 dark:text-slate-400 mb-3">
                  {task.description}
                </p>
              )}

              {task.options && task.options.length > 0 && (
                <div className="flex flex-wrap gap-1">
                  {task.options.map((opt, i) => (
                    <Badge key={i} variant="neutral">
                      {opt.name}
                    </Badge>
                  ))}
                </div>
              )}
            </Card>
          ))}
        </div>
      )}
    </PageLayout>
  );
}

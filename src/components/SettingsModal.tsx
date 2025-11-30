import { useState, useEffect } from 'react';
import { useApp } from '../context/AppContext';
import { useHotkeyCapture } from '../hooks/useHotkeyCapture';
import { SaveConfigRequest, PasteBehavior, ToastType } from '../types';

interface SettingsModalProps {
  onShowToast: (type: ToastType, title: string, message?: string) => void;
}

export function SettingsModal({ onShowToast }: SettingsModalProps) {
  const { 
    config, 
    closeModal, 
    saveConfig, 
    fetchModels, 
    testConnection,
    loadConfig 
  } = useApp();

  // Form state
  const [hotkey, setHotkey] = useState('');
  const [apiBaseUrl, setApiBaseUrl] = useState('');
  const [apiKey, setApiKey] = useState('');
  const [llmModel, setLlmModel] = useState('');
  const [imageModel, setImageModel] = useState('');
  const [audioModel, setAudioModel] = useState('');
  const [ttsModel, setTtsModel] = useState('');
  const [pasteBehavior, setPasteBehavior] = useState<PasteBehavior>('reviewMode');
  const [disableTextSelection, setDisableTextSelection] = useState(false);
  const [enableDebugLogging, setEnableDebugLogging] = useState(false);
  const [copyDelayMs, setCopyDelayMs] = useState(200);
  const [historyLimit, setHistoryLimit] = useState(500);
  const [mediaRetentionDays, setMediaRetentionDays] = useState(0);

  // Model lists
  const [models, setModels] = useState<string[]>([]);
  const [imageModels, setImageModels] = useState<string[]>([]);
  const [audioModels, setAudioModels] = useState<string[]>([]);

  // UI state
  const [isFetchingModels, setIsFetchingModels] = useState(false);
  const [isTesting, setIsTesting] = useState(false);
  const [isSaving, setIsSaving] = useState(false);

  // Hotkey capture hook
  const {
    isCapturing: isCapturingHotkey,
    isValidating: isValidatingHotkey,
    startCapture: startHotkeyCapture,
    stopCapture: stopHotkeyCapture,
    handleKeyDown: handleHotkeyKeyDown,
  } = useHotkeyCapture({
    onHotkeyCapture: setHotkey,
    onBlockedHotkey: (_hotkey, reason) => {
      onShowToast('warning', 'Blocked Hotkey', reason);
    },
    onUnavailableHotkey: (_hotkey, reason) => {
      onShowToast('error', 'Hotkey Unavailable', reason);
    },
    currentHotkey: config?.hotkey,
  });

  // Initialize form from config
  useEffect(() => {
    if (config) {
      setHotkey(config.hotkey);
      setApiBaseUrl(config.apiBaseUrl);
      setLlmModel(config.llmModel);
      setImageModel(config.imageModel);
      setAudioModel(config.audioModel);
      setTtsModel(config.ttsModel);
      setPasteBehavior(config.pasteBehavior);
      setDisableTextSelection(config.disableTextSelection);
      setEnableDebugLogging(config.enableDebugLogging);
      setCopyDelayMs(config.copyDelayMs);
      setModels(config.models);
      setImageModels(config.imageModels);
      setAudioModels(config.audioModels);
      setHistoryLimit(config.historyLimit ?? 500);
      setMediaRetentionDays(config.mediaRetentionDays ?? 0);
    }
  }, [config]);

  const handleFetchModels = async () => {
    setIsFetchingModels(true);
    try {
      const allModels = await fetchModels();
      
      // Categorize models
      const textModels = allModels.filter(m => 
        !m.includes('dall-e') && 
        !m.includes('whisper') && 
        !m.includes('tts') &&
        !m.includes('embedding')
      );
      const imgModels = allModels.filter(m => 
        m.includes('dall-e') || 
        m.includes('flux') || 
        m.includes('image') ||
        m.includes('FLUX')
      );
      const audModels = allModels.filter(m => 
        m.includes('whisper') || 
        m.includes('tts') ||
        m.includes('audio')
      );
      
      setModels(textModels);
      setImageModels(imgModels);
      setAudioModels(audModels);
      
      onShowToast('success', 'Models Loaded', `Found ${allModels.length} models`);
    } catch (error) {
      onShowToast('error', 'Error', String(error));
    } finally {
      setIsFetchingModels(false);
    }
  };

  const handleTestConnection = async () => {
    setIsTesting(true);
    try {
      await testConnection();
      onShowToast('success', 'Connected', 'API connection successful');
    } catch (error) {
      onShowToast('error', 'Connection Failed', String(error));
    } finally {
      setIsTesting(false);
    }
  };

  const handleSave = async () => {
    // Validation
    if (!llmModel) {
      onShowToast('error', 'Validation Error', 'LLM Model is required');
      return;
    }

    setIsSaving(true);
    try {
      const request: SaveConfigRequest = {
        hotkey,
        apiBaseUrl,
        apiKey: apiKey || undefined,
        llmModel,
        imageModel,
        audioModel,
        ttsModel,
        pasteBehavior,
        disableTextSelection,
        enableDebugLogging,
        copyDelayMs,
        models,
        imageModels,
        audioModels,
        historyLimit,
        mediaRetentionDays,
      };
      
      await saveConfig(request);
      await loadConfig();
      
      onShowToast('success', 'Saved', 'Settings saved successfully');
      closeModal();
    } catch (error) {
      onShowToast('error', 'Error', String(error));
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
      <div className="bg-white dark:bg-slate-900 rounded-xl shadow-2xl w-full max-w-lg max-h-[90vh] flex flex-col animate-in fade-in zoom-in-95 duration-200">
        {/* Header */}
        <div className="flex items-center justify-between p-4 border-b border-slate-200 dark:border-slate-700">
          <h2 className="text-lg font-semibold text-slate-900 dark:text-white">
            ⚙️ Settings
          </h2>
          <button
            onClick={closeModal}
            className="btn-ghost"
          >
            ✕
          </button>
        </div>

        {/* Content */}
        <div className="flex-1 overflow-y-auto p-4 space-y-6">
          {/* General Settings */}
          <section className="space-y-4">
            <h3 className="text-sm font-semibold text-slate-700 dark:text-slate-300 uppercase tracking-wider">
              General
            </h3>
            
            {/* Hotkey */}
            <div className="space-y-1">
              <label className="form-label">
                Global Hotkey
              </label>
              <input
                type="text"
                value={hotkey}
                readOnly
                disabled={isValidatingHotkey}
                onFocus={startHotkeyCapture}
                onBlur={stopHotkeyCapture}
                onKeyDown={handleHotkeyKeyDown}
                placeholder={isValidatingHotkey ? 'Validating...' : (isCapturingHotkey ? 'Press key combination...' : 'Click to capture')}
                className={`w-full px-3 py-2 text-sm rounded-lg border 
                           ${isCapturingHotkey || isValidatingHotkey
                             ? 'border-blue-500 ring-2 ring-blue-200 dark:ring-blue-800' 
                             : 'border-slate-300 dark:border-slate-600'
                           }
                           bg-white dark:bg-slate-800 text-slate-900 dark:text-white
                           cursor-pointer transition-all duration-200 disabled:opacity-50`}
              />
            </div>

            {/* Paste Behavior */}
            <div className="space-y-1">
              <label className="form-label">
                Paste Behavior
              </label>
              <select
                value={pasteBehavior}
                onChange={(e) => setPasteBehavior(e.target.value as PasteBehavior)}
                className="form-input-sm"
              >
                <option value="autoPaste">Auto Paste</option>
                <option value="clipboardMode">Clipboard Mode</option>
                <option value="reviewMode">Review Mode</option>
              </select>
            </div>
          </section>

          {/* API Settings */}
          <section className="space-y-4">
            <h3 className="text-sm font-semibold text-slate-700 dark:text-slate-300 uppercase tracking-wider">
              API Configuration
            </h3>
            
            {/* API Base URL */}
            <div className="space-y-1">
              <label className="form-label">
                API Base URL
              </label>
              <input
                type="url"
                value={apiBaseUrl}
                onChange={(e) => setApiBaseUrl(e.target.value)}
                placeholder="https://api.openai.com/v1"
                className="form-input-sm"
              />
            </div>

            {/* API Key */}
            <div className="space-y-1">
              <label className="form-label">
                API Key {config?.apiKeySet && <span className="text-green-500">(Set)</span>}
              </label>
              <input
                type="password"
                value={apiKey}
                onChange={(e) => setApiKey(e.target.value)}
                placeholder={config?.apiKeySet ? '••••••••••••' : 'Enter your API key'}
                className="form-input-sm"
              />
            </div>

            {/* Test & Fetch buttons */}
            <div className="flex gap-2">
              <button
                onClick={handleTestConnection}
                disabled={isTesting}
                className="flex-1 px-3 py-2 text-sm font-medium text-slate-700 dark:text-slate-300
                           border border-slate-300 dark:border-slate-600 rounded-lg
                           hover:bg-slate-100 dark:hover:bg-slate-800 
                           disabled:opacity-50 transition-colors"
              >
                {isTesting ? '⏳ Testing...' : '📡 Test Connection'}
              </button>
              <button
                onClick={handleFetchModels}
                disabled={isFetchingModels}
                className="flex-1 px-3 py-2 text-sm font-medium text-slate-700 dark:text-slate-300
                           border border-slate-300 dark:border-slate-600 rounded-lg
                           hover:bg-slate-100 dark:hover:bg-slate-800 
                           disabled:opacity-50 transition-colors"
              >
                {isFetchingModels ? '⏳ Loading...' : '🔄 Get Models'}
              </button>
            </div>
          </section>

          {/* Model Selection */}
          <section className="space-y-4">
            <h3 className="text-sm font-semibold text-slate-700 dark:text-slate-300 uppercase tracking-wider">
              Models
            </h3>
            
            {/* Text Model */}
            <div className="space-y-1">
              <label className="form-label">
                Text Model <span className="text-red-500">*</span>
              </label>
              <select
                value={llmModel}
                onChange={(e) => setLlmModel(e.target.value)}
                className="form-input-sm"
              >
                <option value="">Select a model...</option>
                {models.map((m) => (
                  <option key={m} value={m}>{m}</option>
                ))}
              </select>
            </div>

            {/* Image Model */}
            <div className="space-y-1">
              <label className="form-label">
                Image Model
              </label>
              <select
                value={imageModel}
                onChange={(e) => setImageModel(e.target.value)}
                className="form-input-sm"
              >
                <option value="">Select a model...</option>
                {imageModels.map((m) => (
                  <option key={m} value={m}>{m}</option>
                ))}
              </select>
            </div>

            {/* Audio Model */}
            <div className="space-y-1">
              <label className="form-label">
                Audio Model (STT)
              </label>
              <select
                value={audioModel}
                onChange={(e) => setAudioModel(e.target.value)}
                className="form-input-sm"
              >
                <option value="">Select a model...</option>
                {audioModels.map((m) => (
                  <option key={m} value={m}>{m}</option>
                ))}
              </select>
            </div>
          </section>

          {/* Performance Settings */}
          <section className="space-y-4">
            <h3 className="text-sm font-semibold text-slate-700 dark:text-slate-300 uppercase tracking-wider">
              Performance
            </h3>
            
            <div className="space-y-3">
              <label className="flex items-center gap-3 cursor-pointer">
                <input
                  type="checkbox"
                  checked={disableTextSelection}
                  onChange={(e) => setDisableTextSelection(e.target.checked)}
                  className="w-4 h-4 rounded border-slate-300 dark:border-slate-600 
                             text-blue-600 focus:ring-blue-500"
                />
                <div>
                  <span className="form-label">
                    Disable Text Selection
                  </span>
                  <p className="help-text">
                    Faster window opening, no auto text capture
                  </p>
                </div>
              </label>

              <label className="flex items-center gap-3 cursor-pointer">
                <input
                  type="checkbox"
                  checked={enableDebugLogging}
                  onChange={(e) => setEnableDebugLogging(e.target.checked)}
                  className="w-4 h-4 rounded border-slate-300 dark:border-slate-600 
                             text-blue-600 focus:ring-blue-500"
                />
                <div>
                  <span className="form-label">
                    Enable Debug Logging
                  </span>
                  <p className="help-text">
                    Log API requests for troubleshooting
                  </p>
                </div>
              </label>
            </div>
          </section>
        </div>

        {/* Footer */}
        <div className="flex items-center justify-end gap-3 p-4 border-t border-slate-200 dark:border-slate-700">
          <button
            onClick={closeModal}
            className="btn-outline"
          >
            Cancel
          </button>
          <button
            onClick={handleSave}
            disabled={isSaving}
            className="btn-primary"
          >
            {isSaving ? '⏳ Saving...' : '✓ Save'}
          </button>
        </div>
      </div>
    </div>
  );
}

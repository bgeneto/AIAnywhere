import { useState, useEffect } from 'react';
import { useApp } from '../../context/AppContext';
import { useI18n } from '../../i18n/index';
import { useHotkeyCapture } from '../../hooks/useHotkeyCapture';
import { ToastType, SaveConfigRequest, PasteBehavior } from '../../types';

type SettingsTab = 'api' | 'audio' | 'general';

interface SettingsPageProps {
  onShowToast: (type: ToastType, title: string, message?: string) => void;
}

export function SettingsPage({ onShowToast }: SettingsPageProps) {
  const { config, saveConfig, fetchModelsWithEndpoint, testConnectionWithEndpoint, loadConfig } = useApp();
  const { t } = useI18n();
  const [activeTab, setActiveTab] = useState<SettingsTab>('api');

  // Form state
  const [hotkey, setHotkey] = useState('');
  const [apiBaseUrl, setApiBaseUrl] = useState('');
  const [apiKey, setApiKey] = useState('');
  const [showApiKey, setShowApiKey] = useState(false);
  const [llmModel, setLlmModel] = useState('');
  const [imageModel, setImageModel] = useState('');
  const [audioModel, setAudioModel] = useState('');
  const [ttsModel, setTtsModel] = useState('');
  const [pasteBehavior, setPasteBehavior] = useState<PasteBehavior>('reviewMode');
  const [disableTextSelection, setDisableTextSelection] = useState(false);
  const [enableDebugLogging, setEnableDebugLogging] = useState(false);
  const [copyDelayMs, setCopyDelayMs] = useState(200);

  // Model lists
  const [models, setModels] = useState<string[]>([]);
  const [imageModels, setImageModels] = useState<string[]>([]);
  const [audioModels, setAudioModels] = useState<string[]>([]);

  // UI state
  const [isTesting, setIsTesting] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [isFetchingModels, setIsFetchingModels] = useState(false);

  // Hotkey capture hook
  const {
    isCapturing: isCapturingHotkey,
    isValidating: isValidatingHotkey,
    startCapture: startHotkeyCapture,
    stopCapture: stopHotkeyCapture,
    handleKeyDown: hotkeyKeyDownHandler,
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
    }
  }, [config]);

  const tabs: { id: SettingsTab; label: string; icon: string }[] = [
    { id: 'api', label: t.nav.apiSettings, icon: '⚙' },
    { id: 'audio', label: t.settings.models.title, icon: '🤖' },
    { id: 'general', label: t.settings.general.title, icon: '⚙️' },
  ];

  const handleHotkeyKeyDown = hotkeyKeyDownHandler;

  const handleFetchModels = async () => {
    setIsFetchingModels(true);
    try {
      const allModels = await fetchModelsWithEndpoint(apiBaseUrl, apiKey || undefined);

      // Filter models more accurately - exclude image/audio/embedding models from text models
      const textModels = allModels.filter((m: string) => {
        const lower = m.toLowerCase();
        return !lower.includes('dall-e') &&
          !lower.includes('whisper') &&
          !lower.includes('tts') &&
          !lower.includes('embedding') &&
          !lower.includes('flux') &&
          !lower.includes('image') &&
          !lower.includes('stable-diffusion') &&
          !lower.includes('audio');
      });
      const imgModels = allModels.filter((m: string) => {
        const lower = m.toLowerCase();
        return lower.includes('dall-e') ||
          lower.includes('flux') ||
          lower.includes('image') ||
          lower.includes('stable-diffusion');
      });
      const audModels = allModels.filter((m: string) => {
        const lower = m.toLowerCase();
        return lower.includes('whisper') ||
          lower.includes('tts') ||
          lower.includes('audio');
      });

      setModels(textModels);
      setImageModels(imgModels);
      setAudioModels(audModels);

      // Auto-select first model if current selection is empty or not in the new list
      if (textModels.length > 0 && (!llmModel || !textModels.includes(llmModel))) {
        setLlmModel(textModels[0]);
      }
      if (imgModels.length > 0 && (!imageModel || !imgModels.includes(imageModel))) {
        setImageModel(imgModels[0]);
      }
      if (audModels.length > 0 && (!audioModel || !audModels.includes(audioModel))) {
        setAudioModel(audModels[0]);
      }

      onShowToast('success', 'Models Loaded', `Found ${allModels.length} models`);
    } catch (error) {
      onShowToast('error', t.toast.error, String(error));
    } finally {
      setIsFetchingModels(false);
    }
  };

  const handleTestConnection = async () => {
    setIsTesting(true);
    try {
      await testConnectionWithEndpoint(apiBaseUrl, apiKey || undefined);
      onShowToast('success', t.settings.api.testSuccess);
      // Auto-refresh models after successful connection test
      await handleFetchModels();
    } catch (error) {
      onShowToast('error', t.settings.api.testFailed, String(error));
    } finally {
      setIsTesting(false);
    }
  };

  const handleSave = async () => {
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
      };
      await saveConfig(request);
      await loadConfig();
      onShowToast('success', t.toast.configSaved);
    } catch (error) {
      onShowToast('error', t.toast.configSaveFailed, String(error));
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <div className="flex flex-col h-full">
      {/* Page Header */}
      <div className="p-6 border-b border-slate-200 dark:border-slate-800">
        <h2 className="text-2xl font-bold text-slate-900 dark:text-white">
          {t.settings.title}
        </h2>
      </div>

      {/* Tabs */}
      <div className="flex border-b border-slate-200 dark:border-slate-800 bg-slate-50 dark:bg-slate-900/50">
        {tabs.map((tab) => (
          <button
            key={tab.id}
            onClick={() => setActiveTab(tab.id)}
            className={`flex items-center gap-2 px-6 py-3 text-sm font-medium transition-colors
                        ${activeTab === tab.id
                ? 'text-blue-600 dark:text-blue-400 border-b-2 border-blue-600 dark:border-blue-400 bg-white dark:bg-slate-800'
                : 'text-slate-600 dark:text-slate-400 hover:text-slate-900 dark:hover:text-white'
              }`}
          >
            <span>{tab.icon}</span>
            {tab.label}
          </button>
        ))}
      </div>

      {/* Content */}
      <div className="flex-1 overflow-y-auto p-6">
        <div className="max-w-2xl space-y-6">
          {/* API Settings Tab */}
          {activeTab === 'api' && (
            <>
              <div className="space-y-4">
                <h3 className="text-lg font-semibold text-slate-900 dark:text-white">
                  {t.settings.api.configuration}
                </h3>

                {/* Endpoint */}
                <div className="space-y-2">
                  <label className="block text-sm font-medium text-slate-700 dark:text-slate-300">
                    {t.settings.api.endpoint}
                  </label>
                  <input
                    type="url"
                    value={apiBaseUrl}
                    onChange={(e) => setApiBaseUrl(e.target.value)}
                    placeholder={t.settings.api.endpointPlaceholder}
                    className="w-full px-4 py-2.5 text-sm rounded-lg border border-slate-300 dark:border-slate-600 
                               bg-white dark:bg-slate-800 text-slate-900 dark:text-white
                               focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  />
                </div>

                {/* API Key */}
                <div className="space-y-2">
                  <label className="block text-sm font-medium text-slate-700 dark:text-slate-300">
                    {t.settings.api.apiKey}
                  </label>
                  <div className="relative">
                    <input
                      type={showApiKey ? 'text' : 'password'}
                      value={apiKey}
                      onChange={(e) => setApiKey(e.target.value)}
                      placeholder={config?.apiKeySet ? '••••••••••••••••' : t.settings.api.apiKeyPlaceholder}
                      className="w-full px-4 py-2.5 pr-20 text-sm rounded-lg border border-slate-300 dark:border-slate-600 
                                 bg-white dark:bg-slate-800 text-slate-900 dark:text-white
                                 focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    />
                    <button
                      type="button"
                      onClick={() => setShowApiKey(!showApiKey)}
                      className="absolute right-2 top-1/2 -translate-y-1/2 px-3 py-1 text-xs font-medium
                                 text-slate-600 dark:text-slate-400 hover:text-slate-900 dark:hover:text-white
                                 bg-slate-100 dark:bg-slate-700 rounded transition-colors"
                    >
                      {showApiKey ? t.settings.api.hide : t.settings.api.show}
                    </button>
                  </div>
                  <p className="text-xs text-slate-500 dark:text-slate-400">
                    {t.settings.api.apiKeyHelp}
                  </p>
                </div>

                {/* Test Connection */}
                <button
                  onClick={handleTestConnection}
                  disabled={isTesting || !apiBaseUrl}
                  className="flex items-center gap-2 px-4 py-2 text-sm font-medium
                             text-blue-600 dark:text-blue-400 
                             bg-blue-50 dark:bg-blue-900/20 hover:bg-blue-100 dark:hover:bg-blue-900/40
                             disabled:opacity-50 disabled:cursor-not-allowed
                             rounded-lg transition-colors"
                >
                  {isTesting ? '⏳' : '🔌'} {t.settings.api.testConnection}
                </button>
              </div>
            </>
          )}

          {/* Models Settings Tab */}
          {activeTab === 'audio' && (
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <h3 className="text-lg font-semibold text-slate-900 dark:text-white">
                  {t.settings.models.title}
                </h3>
                <button
                  onClick={handleFetchModels}
                  disabled={isFetchingModels}
                  className="flex items-center gap-2 px-3 py-1.5 text-xs font-medium
                             text-slate-600 dark:text-slate-400 
                             bg-slate-100 dark:bg-slate-800 hover:bg-slate-200 dark:hover:bg-slate-700
                             disabled:opacity-50 disabled:cursor-not-allowed
                             rounded-lg transition-colors"
                >
                  {isFetchingModels ? '⏳' : '🔄'} {t.settings.api.refreshModels}
                </button>
              </div>

              {/* Text Model */}
              <div className="space-y-2">
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300">
                  {t.settings.api.textModel}
                </label>
                <select
                  value={llmModel}
                  onChange={(e) => setLlmModel(e.target.value)}
                  className="w-full px-4 py-2.5 text-sm rounded-lg border border-slate-300 dark:border-slate-600 
                             bg-white dark:bg-slate-800 text-slate-900 dark:text-white
                             focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                >
                  <option value="">Select a model</option>
                  {models.map((model) => (
                    <option key={model} value={model}>{model}</option>
                  ))}
                </select>
              </div>

              {/* Image Model */}
              <div className="space-y-2">
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300">
                  {t.settings.api.imageModel}
                </label>
                <select
                  value={imageModel}
                  onChange={(e) => setImageModel(e.target.value)}
                  className="w-full px-4 py-2.5 text-sm rounded-lg border border-slate-300 dark:border-slate-600 
                             bg-white dark:bg-slate-800 text-slate-900 dark:text-white
                             focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                >
                  <option value="">Select a model</option>
                  {imageModels.map((model) => (
                    <option key={model} value={model}>{model}</option>
                  ))}
                </select>
              </div>

              {/* Audio Model (STT) */}
              <div className="space-y-2">
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300">
                  {t.settings.api.audioModel}
                </label>
                <select
                  value={audioModel}
                  onChange={(e) => setAudioModel(e.target.value)}
                  className="w-full px-4 py-2.5 text-sm rounded-lg border border-slate-300 dark:border-slate-600 
                             bg-white dark:bg-slate-800 text-slate-900 dark:text-white
                             focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                >
                  <option value="">Select a model</option>
                  {audioModels.filter((m) => !m.toLowerCase().includes('tts')).map((model) => (
                    <option key={model} value={model}>{model}</option>
                  ))}
                </select>
              </div>

              {/* TTS Model */}
              <div className="space-y-2">
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300">
                  {t.settings.api.ttsModel}
                </label>
                <input
                  type="text"
                  value={ttsModel}
                  onChange={(e) => setTtsModel(e.target.value)}
                  placeholder="e.g., tts-1"
                  className="w-full px-4 py-2.5 text-sm rounded-lg border border-slate-300 dark:border-slate-600 
                             bg-white dark:bg-slate-800 text-slate-900 dark:text-white
                             focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
            </div>
          )}

          {/* General Settings Tab */}
          {activeTab === 'general' && (
            <div className="space-y-4">
              <h3 className="text-lg font-semibold text-slate-900 dark:text-white">
                {t.settings.general.title}
              </h3>

              {/* Global Hotkey */}
              <div className="space-y-2">
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300">
                  {t.settings.general.hotkey}
                </label>
                <div className="relative">
                  <input
                    type="text"
                    value={hotkey}
                    readOnly
                    disabled={isValidatingHotkey}
                    onKeyDown={handleHotkeyKeyDown}
                    onFocus={startHotkeyCapture}
                    onBlur={stopHotkeyCapture}
                    placeholder={t.settings.general.hotkeyPlaceholder}
                    className={`w-full px-4 py-2.5 text-sm rounded-lg border 
                               ${isCapturingHotkey || isValidatingHotkey
                        ? 'border-blue-500 ring-2 ring-blue-500'
                        : 'border-slate-300 dark:border-slate-600'} 
                               bg-white dark:bg-slate-800 text-slate-900 dark:text-white
                               cursor-pointer disabled:opacity-50`}
                  />
                  {(isCapturingHotkey || isValidatingHotkey) && (
                    <span className="absolute right-3 top-1/2 -translate-y-1/2 text-xs text-blue-500">
                      {isValidatingHotkey ? 'Validating...' : 'Press keys...'}
                    </span>
                  )}
                </div>
                <p className="text-xs text-slate-500 dark:text-slate-400">
                  {t.settings.general.hotkeyDesc}
                </p>
              </div>

              {/* Paste Behavior */}
              <div className="space-y-2">
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300">
                  {t.settings.general.pasteBehavior}
                </label>
                <select
                  value={pasteBehavior}
                  onChange={(e) => setPasteBehavior(e.target.value as PasteBehavior)}
                  className="w-full px-4 py-2.5 text-sm rounded-lg border border-slate-300 dark:border-slate-600 
                             bg-white dark:bg-slate-800 text-slate-900 dark:text-white
                             focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                >
                  <option value="autoPaste">{t.settings.general.autoPaste}</option>
                  <option value="clipboardMode">{t.settings.general.clipboardMode}</option>
                  <option value="reviewMode">{t.settings.general.reviewMode}</option>
                </select>
                <p className="text-xs text-slate-500 dark:text-slate-400">
                  {t.settings.general.pasteBehaviorDesc}
                </p>
              </div>

              {/* Toggle Options */}
              <div className="space-y-3 pt-4">
                <label className="flex items-center justify-between p-3 bg-slate-50 dark:bg-slate-800/50 rounded-lg cursor-pointer">
                  <div>
                    <div className="text-sm font-medium text-slate-700 dark:text-slate-300">
                      {t.settings.general.disableTextSelection}
                    </div>
                    <div className="text-xs text-slate-500 dark:text-slate-400">
                      {t.settings.general.disableTextSelectionDesc}
                    </div>
                  </div>
                  <input
                    type="checkbox"
                    checked={disableTextSelection}
                    onChange={(e) => setDisableTextSelection(e.target.checked)}
                    className="w-5 h-5 rounded border-slate-300 dark:border-slate-600 
                               text-blue-600 focus:ring-blue-500"
                  />
                </label>

                <label className="flex items-center justify-between p-3 bg-slate-50 dark:bg-slate-800/50 rounded-lg cursor-pointer">
                  <div>
                    <div className="text-sm font-medium text-slate-700 dark:text-slate-300">
                      {t.settings.general.enableDebugLogging}
                    </div>
                    <div className="text-xs text-slate-500 dark:text-slate-400">
                      {t.settings.general.enableDebugLoggingDesc}
                    </div>
                  </div>
                  <input
                    type="checkbox"
                    checked={enableDebugLogging}
                    onChange={(e) => setEnableDebugLogging(e.target.checked)}
                    className="w-5 h-5 rounded border-slate-300 dark:border-slate-600 
                               text-blue-600 focus:ring-blue-500"
                  />
                </label>
              </div>

              {/* Copy Delay Setting */}
              <div className="space-y-2 pt-4">
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300">
                  {t.settings.general.copyDelay || 'Copy Delay (ms)'}
                </label>
                <input
                  type="number"
                  min="50"
                  max="1000"
                  step="50"
                  value={copyDelayMs}
                  onChange={(e) => setCopyDelayMs(Math.max(50, Math.min(1000, parseInt(e.target.value) || 200)))}
                  className="w-full px-4 py-2.5 text-sm rounded-lg border border-slate-300 dark:border-slate-600 
                             bg-white dark:bg-slate-800 text-slate-900 dark:text-white
                             focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
                <p className="text-xs text-slate-500 dark:text-slate-400">
                  {t.settings.general.copyDelayDesc || 'Time waited before relying on the copied content in the clipboard.'}
                </p>
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Footer */}
      <div className="p-6 border-t border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900">
        <div className="max-w-2xl flex items-center justify-end gap-3">
          <button
            onClick={handleSave}
            disabled={isSaving}
            className="flex items-center gap-2 px-6 py-2.5 text-sm font-medium text-white
                       bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400
                       rounded-lg transition-colors duration-200 shadow-sm
                       focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2
                       dark:focus:ring-offset-slate-900"
          >
            {isSaving ? '⏳' : '💾'} {t.settings.save}
          </button>
        </div>
      </div>
    </div>
  );
}

import { useState, useEffect } from 'react';
import { useApp } from '../../context/AppContext';
import { useI18n } from '../../i18n/index';
import { useHotkeyCapture } from '../../hooks/useHotkeyCapture';
import { ToastType, SaveConfigRequest, PasteBehavior } from '../../types';
import {
  FormField, FormInput, FormSelect, SettingsToggle, PageLayout
} from '../ui';

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
  const [historyLimit, setHistoryLimit] = useState(500);
  const [mediaRetentionDays, setMediaRetentionDays] = useState(0);

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
      setHistoryLimit(config.historyLimit ?? 500);
      setMediaRetentionDays(config.mediaRetentionDays ?? 0);
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
        historyLimit,
        mediaRetentionDays,
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
    <PageLayout
      title={t.settings.title}
      footer={
        <button
          onClick={handleSave}
          disabled={isSaving}
          className="btn-primary"
        >
          {isSaving ? '⏳' : '💾'} {t.settings.save}
        </button>
      }
    >
      {/* Tabs */}
      <div className="flex border-b border-slate-200 dark:border-slate-800 bg-slate-50 dark:bg-slate-900/50 -mx-6 mb-6 px-6">
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

      <div className="space-y-6">
        {/* API Settings Tab */}
        {activeTab === 'api' && (
          <>
            <div className="space-y-4">
              <h3 className="section-title">
                {t.settings.api.configuration}
              </h3>

              {/* Endpoint */}
              <FormField label={t.settings.api.endpoint}>
                <FormInput
                  type="url"
                  value={apiBaseUrl}
                  onChange={(e) => setApiBaseUrl(e.target.value)}
                  placeholder={t.settings.api.endpointPlaceholder}
                />
              </FormField>

              {/* API Key */}
              <FormField label={t.settings.api.apiKey} helpText={t.settings.api.apiKeyHelp}>
                <div className="relative">
                  <FormInput
                    type={showApiKey ? 'text' : 'password'}
                    value={apiKey}
                    onChange={(e) => setApiKey(e.target.value)}
                    placeholder={config?.apiKeySet ? '••••••••••••••••' : t.settings.api.apiKeyPlaceholder}
                    className="pr-20"
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
              </FormField>

              {/* Test Connection */}
              <button
                onClick={handleTestConnection}
                disabled={isTesting || !apiBaseUrl}
                className="btn-outline disabled:opacity-50 disabled:cursor-not-allowed"
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
              <h3 className="section-title">
                {t.settings.models.title}
              </h3>
              <button
                onClick={handleFetchModels}
                disabled={isFetchingModels}
                className="btn-secondary disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isFetchingModels ? '⏳' : '🔄'} {t.settings.api.refreshModels}
              </button>
            </div>

            {/* Text Model */}
            <FormField label={t.settings.api.textModel}>
              <FormSelect
                value={llmModel}
                onChange={(e) => setLlmModel(e.target.value)}
                options={models.map(m => ({ value: m, label: m }))}
                placeholder="Select a model"
              />
            </FormField>

            {/* Image Model */}
            <FormField label={t.settings.api.imageModel}>
              <FormSelect
                value={imageModel}
                onChange={(e) => setImageModel(e.target.value)}
                options={imageModels.map(m => ({ value: m, label: m }))}
                placeholder="Select a model"
              />
            </FormField>

            {/* Audio Model (STT) */}
            <FormField label={t.settings.api.audioModel}>
              <FormSelect
                value={audioModel}
                onChange={(e) => setAudioModel(e.target.value)}
                options={audioModels.filter((m) => !m.toLowerCase().includes('tts')).map(m => ({ value: m, label: m }))}
                placeholder="Select a model"
              />
            </FormField>

            {/* TTS Model */}
            <FormField label={t.settings.api.ttsModel}>
              <FormInput
                type="text"
                value={ttsModel}
                onChange={(e) => setTtsModel(e.target.value)}
                placeholder="e.g., tts-1"
              />
            </FormField>
          </div>
        )}

        {/* General Settings Tab */}
        {activeTab === 'general' && (
          <div className="space-y-4">
            <h3 className="section-title">
              {t.settings.general.title}
            </h3>

            {/* Global Hotkey */}
            <FormField label={t.settings.general.hotkey} helpText={t.settings.general.hotkeyDesc}>
              <div className="relative">
                <FormInput
                  type="text"
                  value={hotkey}
                  readOnly
                  disabled={isValidatingHotkey}
                  onKeyDown={handleHotkeyKeyDown}
                  onFocus={startHotkeyCapture}
                  onBlur={stopHotkeyCapture}
                  placeholder={t.settings.general.hotkeyPlaceholder}
                  className={isCapturingHotkey || isValidatingHotkey ? 'border-blue-500 ring-2 ring-blue-500' : ''}
                />
                {(isCapturingHotkey || isValidatingHotkey) && (
                  <span className="absolute right-3 top-1/2 -translate-y-1/2 text-xs text-blue-500">
                    {isValidatingHotkey ? 'Validating...' : 'Press keys...'}
                  </span>
                )}
              </div>
            </FormField>

            {/* Paste Behavior */}
            <FormField label={t.settings.general.pasteBehavior} helpText={t.settings.general.pasteBehaviorDesc}>
              <FormSelect
                value={pasteBehavior}
                onChange={(e) => setPasteBehavior(e.target.value as PasteBehavior)}
                options={[
                  { value: 'autoPaste', label: t.settings.general.autoPaste },
                  { value: 'clipboardMode', label: t.settings.general.clipboardMode },
                  { value: 'reviewMode', label: t.settings.general.reviewMode }
                ]}
              />
            </FormField>

            {/* Toggle Options */}
            <div className="space-y-3 pt-4">
              <SettingsToggle
                title={t.settings.general.disableTextSelection}
                description={t.settings.general.disableTextSelectionDesc}
                checked={disableTextSelection}
                onChange={setDisableTextSelection}
              />

              <SettingsToggle
                title={t.settings.general.enableDebugLogging}
                description={t.settings.general.enableDebugLoggingDesc}
                checked={enableDebugLogging}
                onChange={setEnableDebugLogging}
              />
            </div>

            {/* Copy Delay Setting */}
            <FormField
              label={t.settings.general.copyDelay || 'Copy Delay (ms)'}
              helpText={t.settings.general.copyDelayDesc || 'Time waited before relying on the copied content in the clipboard.'}
            >
              <FormInput
                type="number"
                min="50"
                max="1000"
                step="50"
                value={copyDelayMs}
                onChange={(e) => setCopyDelayMs(Math.max(50, Math.min(1000, parseInt(e.target.value) || 200)))}
              />
            </FormField>

            {/* History Settings Section */}
            <div className="pt-6 border-t border-slate-200 dark:border-slate-700 mt-6">
              <h3 className="section-title mb-4">
                History Settings
              </h3>

              {/* History Limit */}
              <FormField
                label="History Limit"
                helpText="Maximum number of history entries to keep. Older entries will be automatically deleted."
              >
                <FormInput
                  type="number"
                  min="10"
                  max="10000"
                  step="10"
                  value={historyLimit}
                  onChange={(e) => setHistoryLimit(Math.max(10, Math.min(500, parseInt(e.target.value) || 100)))}
                />
              </FormField>

              {/* Media Retention Days */}
              <FormField
                label="Media Retention (Days)"
                helpText="Number of days to keep generated images and audio files. Set to 0 to keep forever."
              >
                <FormInput
                  type="number"
                  min="0"
                  max="365"
                  step="1"
                  value={mediaRetentionDays}
                  onChange={(e) => setMediaRetentionDays(Math.max(0, Math.min(365, parseInt(e.target.value) || 30)))}
                />
              </FormField>
            </div>
          </div>
        )}
      </div>
    </PageLayout>
  );
}

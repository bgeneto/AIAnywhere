import { createContext, useContext, useState, useCallback, useEffect, ReactNode } from 'react';
import { invoke } from '@tauri-apps/api/core';
import { listen, UnlistenFn } from '@tauri-apps/api/event';
import {
  Configuration,
  Operation,
  LlmRequest,
  LlmResponse,
  ModalType,
  SaveConfigRequest,
  StreamingChunk,
  CustomTask,
  HistoryEntry,
} from '../types';

interface AppContextType {
  // Configuration
  config: Configuration | null;
  configLoading: boolean;
  loadConfig: () => Promise<void>;
  saveConfig: (request: SaveConfigRequest) => Promise<void>;

  // Operations
  operations: Operation[];
  selectedOperation: Operation | null;
  setSelectedOperation: (op: Operation | null) => void;
  operationOptions: Record<string, string>;
  setOperationOption: (key: string, value: string) => void;
  resetOperationOptions: () => void;

  // Custom Tasks
  customTasks: CustomTask[];
  loadCustomTasks: () => Promise<void>;

  // Prompt
  promptText: string;
  setPromptText: (text: string) => void;
  selectedText: string;
  setSelectedText: (text: string) => void;
  audioFilePath: string;
  setAudioFilePath: (path: string) => void;

  // Processing & Streaming
  isProcessing: boolean;
  streamingContent: string;
  isStreaming: boolean;
  clearStreamingContent: () => void;
  processRequest: () => Promise<LlmResponse | null>;
  processRequestStreaming: () => Promise<LlmResponse | null>;
  cancelRequest: () => Promise<void>;

  // Result
  result: LlmResponse | null;
  setResult: (result: LlmResponse | null) => void;
  clearResult: () => void;

  // History
  loadHistoryEntry: (entry: HistoryEntry) => void;

  // UI State
  activeModal: ModalType;
  openModal: (modal: ModalType) => void;
  closeModal: () => void;

  // API helpers
  fetchModels: () => Promise<string[]>;
  testConnection: () => Promise<void>;
  fetchModelsWithEndpoint: (apiBaseUrl: string, apiKey?: string) => Promise<string[]>;
  testConnectionWithEndpoint: (apiBaseUrl: string, apiKey?: string) => Promise<void>;
}

const AppContext = createContext<AppContextType | null>(null);

export function AppProvider({ children }: { children: ReactNode }) {
  // Configuration state
  const [config, setConfig] = useState<Configuration | null>(null);
  const [configLoading, setConfigLoading] = useState(false);

  // Operations state
  const [operations, setOperations] = useState<Operation[]>([]);
  const [selectedOperation, setSelectedOperationState] = useState<Operation | null>(null);
  const [operationOptions, setOperationOptions] = useState<Record<string, string>>({});

  // Custom Tasks state
  const [customTasks, setCustomTasks] = useState<CustomTask[]>([]);

  // Prompt state
  const [promptText, setPromptText] = useState('');
  const [selectedText, setSelectedText] = useState('');
  const [audioFilePath, setAudioFilePath] = useState('');

  // Processing & streaming state
  const [isProcessing, setIsProcessing] = useState(false);
  const [streamingContent, setStreamingContent] = useState('');
  const [isStreaming, setIsStreaming] = useState(false);

  // Result state
  const [result, setResult] = useState<LlmResponse | null>(null);

  // UI state
  const [activeModal, setActiveModal] = useState<ModalType>('none');

  // Set up streaming event listeners
  useEffect(() => {
    let unlistenChunk: UnlistenFn | null = null;
    let unlistenCancelled: UnlistenFn | null = null;
    let isMounted = true; // Track if effect is still active

    const setupListeners = async () => {
      const chunkListener = await listen<StreamingChunk>('llm-stream-chunk', (event) => {
        if (!isMounted) return; // Don't update state if unmounted
        if (event.payload.done) {
          setIsStreaming(false);
        } else {
          setStreamingContent(prev => prev + event.payload.content);
        }
      });

      const cancelledListener = await listen('llm-stream-cancelled', () => {
        if (!isMounted) return; // Don't update state if unmounted
        setIsStreaming(false);
        setStreamingContent('');
      });

      // Only assign if still mounted
      if (isMounted) {
        unlistenChunk = chunkListener;
        unlistenCancelled = cancelledListener;
      } else {
        // Component unmounted before listeners were set up, clean them up immediately
        chunkListener();
        cancelledListener();
      }
    };

    setupListeners();

    return () => {
      isMounted = false;
      unlistenChunk?.();
      unlistenCancelled?.();
    };
  }, []);

  // Load configuration
  const loadConfig = useCallback(async () => {
    setConfigLoading(true);
    try {
      const cfg = await invoke<Configuration>('get_configuration');
      setConfig(cfg);

      const ops = await invoke<Operation[]>('get_operations');
      setOperations(ops);

      // Load custom tasks
      const tasks = await invoke<CustomTask[]>('get_custom_tasks');
      setCustomTasks(tasks);

      // Select first operation by default
      if (ops.length > 0 && !selectedOperation) {
        setSelectedOperationState(ops[0]);
        initializeOptions(ops[0]);
      }
    } catch (error) {
      console.error('Failed to load configuration:', error);
    } finally {
      setConfigLoading(false);
    }
  }, []);

  // Save configuration
  const saveConfig = useCallback(async (request: SaveConfigRequest) => {
    await invoke('save_configuration', { request });
    await loadConfig();
  }, [loadConfig]);

  // Load custom tasks
  const loadCustomTasks = useCallback(async () => {
    try {
      const tasks = await invoke<CustomTask[]>('get_custom_tasks');
      setCustomTasks(tasks);
    } catch (error) {
      console.error('Failed to load custom tasks:', error);
    }
  }, []);

  // Initialize operation options with defaults
  const initializeOptions = (operation: Operation) => {
    const defaults: Record<string, string> = {};
    operation.options.forEach(opt => {
      defaults[opt.key] = opt.defaultValue;
    });
    setOperationOptions(defaults);
  };

  // Set selected operation
  const setSelectedOperation = useCallback((op: Operation | null) => {
    setSelectedOperationState(op);
    if (op) {
      initializeOptions(op);
    } else {
      setOperationOptions({});
    }
  }, []);

  // Set a single operation option
  const setOperationOption = useCallback((key: string, value: string) => {
    setOperationOptions(prev => ({ ...prev, [key]: value }));
  }, []);

  // Reset operation options to defaults
  const resetOperationOptions = useCallback(() => {
    if (selectedOperation) {
      initializeOptions(selectedOperation);
    }
  }, [selectedOperation]);

  // Save to history helper
  const saveToHistory = useCallback(async (
    operationType: string,
    promptText: string,
    responseText: string | undefined,
    options: Record<string, string>,
    mediaPath?: string
  ) => {
    try {
      await invoke('save_history_entry', {
        operationType,
        promptText,
        responseText: responseText || null,
        operationOptions: options,
        mediaPath: mediaPath || null,
      });
    } catch (error) {
      console.error('Failed to save to history:', error);
    }
  }, []);

  // Process LLM request
  const processRequest = useCallback(async (): Promise<LlmResponse | null> => {
    if (!selectedOperation) return null;

    setIsProcessing(true);
    try {
      const request: LlmRequest = {
        operationType: selectedOperation.type,
        prompt: promptText,
        selectedText: selectedText || undefined,
        options: operationOptions,
        audioFilePath: audioFilePath || undefined,
      };

      const response = await invoke<LlmResponse>('process_llm_request', { request });
      setResult(response);

      if (response.success) {
        let mediaPath: string | undefined;

        // For image responses, save to media folder
        if (response.isImage && response.imageUrl) {
          try {
            mediaPath = await invoke<string>('save_generated_image', { imageUrl: response.imageUrl });
          } catch (e) {
            console.error('Failed to save generated image:', e);
          }
        }

        // For audio responses, the audio file path IS the media path
        if (response.isAudio && response.audioFilePath) {
          mediaPath = response.audioFilePath;
        }

        // Save to history with media path if available
        await saveToHistory(
          selectedOperation.type,
          promptText,
          response.content,
          operationOptions,
          mediaPath
        );

        // For TTS responses, always show review modal for Save As dialog
        // regardless of paste behavior setting
        if (response.isAudio) {
          setActiveModal('review');
        } else if (config?.pasteBehavior === 'reviewMode') {
          // Open review modal only if in review mode (for non-audio)
          setActiveModal('review');
        }
      }

      return response;
    } catch (error) {
      const errorResponse: LlmResponse = {
        success: false,
        error: String(error),
        isImage: false,
        isAudio: false,
      };
      setResult(errorResponse);
      return errorResponse;
    } finally {
      setIsProcessing(false);
    }
  }, [selectedOperation, promptText, selectedText, operationOptions, audioFilePath, saveToHistory, config?.pasteBehavior]);

  // Process LLM request with streaming
  const processRequestStreaming = useCallback(async (): Promise<LlmResponse | null> => {
    if (!selectedOperation) return null;

    setIsProcessing(true);
    setStreamingContent('');
    setIsStreaming(true);

    try {
      const request: LlmRequest = {
        operationType: selectedOperation.type,
        prompt: promptText,
        selectedText: selectedText || undefined,
        options: operationOptions,
        audioFilePath: audioFilePath || undefined,
      };

      const response = await invoke<LlmResponse>('process_llm_request_streaming', { request });
      setResult(response);
      setIsStreaming(false);

      if (response.success) {
        let mediaPath: string | undefined;

        // For image responses, save to media folder
        if (response.isImage && response.imageUrl) {
          try {
            mediaPath = await invoke<string>('save_generated_image', { imageUrl: response.imageUrl });
          } catch (e) {
            console.error('Failed to save generated image:', e);
          }
        }

        // For audio responses, the audio file path IS the media path
        if (response.isAudio && response.audioFilePath) {
          mediaPath = response.audioFilePath;
        }

        // Save to history with media path if available
        await saveToHistory(
          selectedOperation.type,
          promptText,
          response.content,
          operationOptions,
          mediaPath
        );

        // For TTS responses, always show review modal for Save As dialog
        // regardless of paste behavior setting
        if (response.isAudio) {
          setActiveModal('review');
        } else if (config?.pasteBehavior === 'reviewMode') {
          // Open review modal only if in review mode (for non-audio)
          setActiveModal('review');
        }
      }

      return response;
    } catch (error) {
      const errorResponse: LlmResponse = {
        success: false,
        error: String(error),
        isImage: false,
        isAudio: false,
      };
      setResult(errorResponse);
      setIsStreaming(false);
      return errorResponse;
    } finally {
      setIsProcessing(false);
    }
  }, [selectedOperation, promptText, selectedText, operationOptions, audioFilePath, config?.pasteBehavior, saveToHistory]);

  // Cancel the current request
  const cancelRequest = useCallback(async () => {
    try {
      await invoke('cancel_llm_request');
      setIsStreaming(false);
      setIsProcessing(false);
      setStreamingContent('');
    } catch (error) {
      console.error('Failed to cancel request:', error);
    }
  }, []);

  // Load history entry (for re-running from history)
  const loadHistoryEntry = useCallback((entry: HistoryEntry) => {
    // Find the operation by type
    const operation = operations.find(op => op.type === entry.operationType);
    if (operation) {
      setSelectedOperationState(operation);
      setOperationOptions(entry.operationOptions || {});
    }
    setPromptText(entry.promptText);
  }, [operations]);

  // Clear result
  const clearResult = useCallback(() => {
    setResult(null);
  }, []);

  // Clear streaming content
  const clearStreamingContent = useCallback(() => {
    setStreamingContent('');
  }, []);

  // Modal controls
  const openModal = useCallback((modal: ModalType) => {
    setActiveModal(modal);
  }, []);

  const closeModal = useCallback(() => {
    setActiveModal('none');
  }, []);

  // Fetch models from API
  const fetchModels = useCallback(async (): Promise<string[]> => {
    return await invoke<string[]>('get_models_from_api');
  }, []);

  // Test API connection
  const testConnection = useCallback(async () => {
    await invoke('test_connection');
  }, []);

  // Fetch models with custom endpoint
  const fetchModelsWithEndpoint = useCallback(async (apiBaseUrl: string, apiKey?: string): Promise<string[]> => {
    return await invoke<string[]>('get_models_with_endpoint', { apiBaseUrl, apiKey });
  }, []);

  // Test connection with custom endpoint
  const testConnectionWithEndpoint = useCallback(async (apiBaseUrl: string, apiKey?: string) => {
    await invoke('test_connection_with_endpoint', { apiBaseUrl, apiKey });
  }, []);

  const value: AppContextType = {
    config,
    configLoading,
    loadConfig,
    saveConfig,
    operations,
    selectedOperation,
    setSelectedOperation,
    operationOptions,
    setOperationOption,
    resetOperationOptions,
    customTasks,
    loadCustomTasks,
    promptText,
    setPromptText,
    selectedText,
    setSelectedText,
    audioFilePath,
    setAudioFilePath,
    isProcessing,
    streamingContent,
    isStreaming,
    clearStreamingContent,
    processRequest,
    processRequestStreaming,
    cancelRequest,
    result,
    setResult,
    clearResult,
    loadHistoryEntry,
    activeModal,
    openModal,
    closeModal,
    fetchModels,
    testConnection,
    fetchModelsWithEndpoint,
    testConnectionWithEndpoint,
  };

  return (
    <AppContext.Provider value={value}>
      {children}
    </AppContext.Provider>
  );
}

export function useApp() {
  const context = useContext(AppContext);
  if (!context) {
    throw new Error('useApp must be used within an AppProvider');
  }
  return context;
}

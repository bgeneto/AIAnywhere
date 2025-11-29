import { createContext, useContext, useState, useCallback, ReactNode } from 'react';
import { invoke } from '@tauri-apps/api/core';
import { 
  Configuration, 
  Operation, 
  LlmRequest, 
  LlmResponse, 
  ModalType,
  SaveConfigRequest 
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
  
  // Prompt
  promptText: string;
  setPromptText: (text: string) => void;
  selectedText: string;
  setSelectedText: (text: string) => void;
  audioFilePath: string;
  setAudioFilePath: (path: string) => void;
  
  // Processing
  isProcessing: boolean;
  processRequest: () => Promise<LlmResponse | null>;
  
  // Result
  result: LlmResponse | null;
  setResult: (result: LlmResponse | null) => void;
  clearResult: () => void;
  
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
  
  // Prompt state
  const [promptText, setPromptText] = useState('');
  const [selectedText, setSelectedText] = useState('');
  const [audioFilePath, setAudioFilePath] = useState('');
  
  // Processing state
  const [isProcessing, setIsProcessing] = useState(false);
  
  // Result state
  const [result, setResult] = useState<LlmResponse | null>(null);
  
  // UI state
  const [activeModal, setActiveModal] = useState<ModalType>('none');
  
  // Load configuration
  const loadConfig = useCallback(async () => {
    setConfigLoading(true);
    try {
      const cfg = await invoke<Configuration>('get_configuration');
      setConfig(cfg);
      
      const ops = await invoke<Operation[]>('get_operations');
      setOperations(ops);
      
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
        // Open review modal for successful responses
        setActiveModal('review');
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
  }, [selectedOperation, promptText, selectedText, operationOptions, audioFilePath]);
  
  // Clear result
  const clearResult = useCallback(() => {
    setResult(null);
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
    promptText,
    setPromptText,
    selectedText,
    setSelectedText,
    audioFilePath,
    setAudioFilePath,
    isProcessing,
    processRequest,
    result,
    setResult,
    clearResult,
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

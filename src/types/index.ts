// AI Anywhere TypeScript Types

// ============================================================================
// Configuration Types
// ============================================================================

export type PasteBehavior = 'autoPaste' | 'clipboardMode' | 'reviewMode';

export interface Configuration {
  hotkey: string;
  apiBaseUrl: string;
  apiKeySet: boolean;
  llmModel: string;
  imageModel: string;
  audioModel: string;
  ttsModel: string;
  pasteBehavior: PasteBehavior;
  disableTextSelection: boolean;
  enableDebugLogging: boolean;
  copyDelayMs: number;
  models: string[];
  imageModels: string[];
  audioModels: string[];
  // History settings
  historyLimit: number;
  mediaRetentionDays: number;
}

export interface SaveConfigRequest {
  hotkey: string;
  apiBaseUrl: string;
  apiKey?: string;
  llmModel: string;
  imageModel: string;
  audioModel: string;
  ttsModel: string;
  pasteBehavior: PasteBehavior;
  disableTextSelection: boolean;
  enableDebugLogging: boolean;
  copyDelayMs: number;
  models: string[];
  imageModels: string[];
  audioModels: string[];
  // History settings
  historyLimit: number;
  mediaRetentionDays: number;
}

// ============================================================================
// Operation Types
// ============================================================================

export type OperationType =
  | 'generalChat'
  | 'imageGeneration'
  | 'textRewrite'
  | 'textTranslation'
  | 'textSummarization'
  | 'textToSpeech'
  | 'emailReply'
  | 'whatsAppResponse'
  | 'speechToText'
  | 'unicodeSymbols';

export type OptionType = 'select' | 'text' | 'number';

export interface OperationOption {
  key: string;
  name: string;
  type: OptionType;
  values: string[];
  defaultValue: string;
  required: boolean;
}

export interface Operation {
  type: OperationType;
  name: string;
  description: string;
  systemPrompt: string;
  options: OperationOption[];
}

// ============================================================================
// LLM Request/Response Types
// ============================================================================

export interface LlmRequest {
  operationType: OperationType;
  prompt: string;
  selectedText?: string;
  options: Record<string, string>;
  audioFilePath?: string;
}

export interface LlmResponse {
  success: boolean;
  content?: string;
  error?: string;
  isImage: boolean;
  imageUrl?: string;
  isAudio: boolean;
  audioData?: number[];
  audioFormat?: string;
  audioFilePath?: string;
}

// ============================================================================
// UI State Types
// ============================================================================

export type ModalType = 'none' | 'settings' | 'about' | 'review';

export interface AppState {
  // Configuration
  config: Configuration | null;
  configLoading: boolean;

  // Operations
  operations: Operation[];
  selectedOperation: Operation | null;
  operationOptions: Record<string, string>;

  // Prompt
  promptText: string;
  selectedText: string;
  audioFilePath: string;

  // Processing
  isProcessing: boolean;

  // Result
  result: LlmResponse | null;

  // UI State
  activeModal: ModalType;
}

// ============================================================================
// Toast Types
// ============================================================================

export type ToastType = 'success' | 'error' | 'warning' | 'info';

export interface ToastMessage {
  id: string;
  type: ToastType;
  title: string;
  message?: string;
}

// ============================================================================
// Drag and Drop Types
// ============================================================================

export interface DragState {
  isDragging: boolean;
  isValidFile: boolean;
}

// Supported audio formats for STT
export const SUPPORTED_AUDIO_FORMATS = [
  'mp3', 'mp4', 'mpeg', 'mpga', 'm4a', 'wav', 'webm', 'ogg', 'flac', 'aac', 'wma'
];

export const MAX_AUDIO_FILE_SIZE = 25 * 1024 * 1024; // 25MB

export function isValidAudioFile(file: File): boolean {
  const extension = file.name.split('.').pop()?.toLowerCase() || '';
  return SUPPORTED_AUDIO_FORMATS.includes(extension) && file.size <= MAX_AUDIO_FILE_SIZE;
}

export function formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

// ============================================================================
// History Types
// ============================================================================

export interface HistoryEntryResponse {
  id: string;
  operationType: OperationType | string;
  promptText: string;
  responseText?: string;
  operationOptions: Record<string, string>;
  mediaPath?: string;
  createdAt: string;
}

export interface HistoryEntry {
  id: string;
  operationType: OperationType | string;
  promptText: string;
  responseText?: string;
  operationOptions: Record<string, string>;
  mediaPath?: string;
  createdAt: string;
}

// ============================================================================
// Custom Task Types
// ============================================================================

export interface CustomTaskOption {
  key: string;
  name: string;
  type: 'select' | 'text' | 'number';
  required: boolean;
  values?: string[];
  defaultValue?: string;
  min?: number;
  max?: number;
  step?: number;
}

export interface CustomTask {
  id: string;
  name: string;
  description: string;
  systemPrompt: string;
  options: CustomTaskOption[];
  createdAt: string;
  updatedAt: string;
}

// ============================================================================
// Streaming Types
// ============================================================================

export interface StreamingState {
  isStreaming: boolean;
  content: string;
  error?: string;
}

export interface StreamingChunk {
  content: string;
  done: boolean;
}

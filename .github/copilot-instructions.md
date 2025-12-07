# AI Anywhere - Copilot/Agent Instructions

## Project Overview
**AI Anywhere** is a Tauri 2.0 desktop application (Rust + TypeScript + Tailwind CSS) that provides universal AI assistance via global hotkeys. The architecture separates concerns into a Rust backend (`src-tauri/`) for system integration and a React frontend (`src/`) for UI.

## Architecture Patterns

### Frontend-Backend Communication
- **Frontend**: Uses `@tauri-apps/api` to invoke Rust commands via `invoke(commandName, params)`
- **Backend**: Rust commands decorated with `#[tauri::command]` in `src-tauri/src/lib.rs`
- **State Management**: React Context (`AppContext.tsx`) holds config, operations, prompt state, and processing status
- **Type Sync**: TypeScript types in `src/types/index.ts` mirror Rust structs (use `#[serde(rename_all = "camelCase")]` for consistency)

### Key Command Categories (in `lib.rs`)
1. **Configuration**: `get_configuration`, `save_configuration`, `update_models`
2. **Operations**: `get_operations` (retrieves AI task definitions)
3. **LLM Processing**: `process_llm_request` (sends prompt to OpenAI-compatible APIs)
4. **Text Capture**: `simulate_copy`, `simulate_paste` (Windows text selection via enigo)
5. **Encryption**: `encrypt_api_key`, `decrypt_api_key` (AES-GCM encryption in `encryption.rs`)

### Component Structure
- **Pages** (`components/pages/`): HomePage, SettingsPage, AboutPage—rendered in MainLayout
- **Modals**: ReviewModal (preview before paste), SettingsModal, AboutModal
- **Context Flow**: App → AppProvider → pages consume `useApp()` and `useI18n()`
- **Operations**: Defined in `operations.rs` with system prompts; each has options (select, text, number)

## Development Workflow

### Build & Run
```bash
npm run dev           # Starts Vite dev server (port 1420) + Tauri in dev mode
npm run build         # Compiles TypeScript, builds Vite, bundles Tauri
npm run tauri dev     # Alternative to npm run dev
npm run tauri build   # Full production build
```

### Dev Environment
- **Vite** watches `src/` (port 1420, HMR on 1421); ignores `src-tauri/`
- **Tauri** uses `src/dist` as frontend output
- **Rust** recompiles on changes; logs printed to console
- **Config File**: `src-tauri/tauri.conf.json` defines window (1100×700), tray icon, plugins

### Windows-Specific
- Text selection uses `enigo` crate (keyboard/mouse simulation)
- `#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]` prevents console window in release

## Configuration & State

### Config Files
- **Path**: `{AppData}/ai-anywhere/config.json` (cross-platform via `dirs` crate)
- **Structure**: Hotkey, API endpoint, encrypted API key, model names, paste behavior, system prompts
- **Encryption**: API key stored encrypted; decrypted in memory only via `decrypt_api_key` command

### AppContext API Methods
- `loadConfig()`: Loads from disk (happens on mount)
- `saveConfig(request)`: Persists configuration
- `processRequest()`: Calls `process_llm_request` with prompt + operation options
- `fetchModelsWithEndpoint()`: Tests API connection and lists available models
- `testConnectionWithEndpoint()`: Validates API credentials without fetching models

## Operation Types & System Prompts

Operations are defined in `operations.rs` with:
- **Type**: Enum (GeneralChat, ImageGeneration, TextRewrite, TextTranslation, TextSummarization, TextToSpeech, EmailReply, WhatsAppResponse, SpeechToText, UnicodeSymbols)
- **Options**: Dynamic form fields (tone, length, size, etc.) stored in operation options HashMap
- **System Prompts**: Customizable per operation in config; default prompts enforce language matching and non-interactive behavior

**Key Pattern**: Prompts use placeholders like `{tone}` and `{length}` which are replaced from operation options before sending to LLM.

## LLM Integration

### Request Flow
1. Frontend collects prompt text, operation type, and options in `LlmRequest`
2. `process_llm_request` command builds the final system + user prompt
3. `LlmService::send_request()` calls OpenAI-compatible API (base URL configurable)
4. Response handling: text responses go to clipboard/paste, image URLs opened in browser, audio data buffered

### Multi-Modal Support
- **Text**: Standard chat completion
- **Images**: Uses `/v1/images/generations` endpoint; opens result in browser
- **Audio (TTS)**: Generates audio blob, attempts playback
- **Audio (STT)**: Sends multipart form with audio file to transcription endpoint

### Error Handling
- Failed requests return `LlmResponse { success: false, error: Some(message) }`
- Connection tests via `/v1/models` endpoint to validate credentials early
- Debug logging available via `enable_debug_logging` config flag

## Internationalization (i18n)

- **System**: React Context (`I18nContext`) with language state (stored in localStorage)
- **Languages**: English (`en`) and Portuguese-Brazil (`pt-BR`)
- **Translations**: Defined in `i18n/translations.ts` as nested object structure
- **Usage**: `const { t } = useI18n()` → access via `t.section.key` (e.g., `t.about.title`)
- **Auto-Detection**: Falls back to browser language if not in localStorage

## Common Development Tasks

### Adding a New Operation Type
1. Add variant to `OperationType` enum in `operations.rs`
2. Define options and system prompt in `get_operations()` function
3. Add default system prompt in `get_default_system_prompts()`
4. Update TypeScript type in `src/types/index.ts`
5. Component will automatically render in OperationOptionsPanel via options array

### Modifying System Prompts
- Edit in `src-tauri/src/operations.rs` (default) or allow user override via Settings page
- Use placeholders like `{tone}`, `{length}` for dynamic substitution
- Test with `enable_debug_logging: true` to see final prompt sent to API

### Adding UI Components
- Keep components in `src/components/` with `.tsx` extension
- Use React hooks (useState, useContext) with `useApp()` and `useI18n()`
- Style with Tailwind CSS classes; dark mode via `dark:` prefix
- Toast notifications via `showToast(type, title, message?)` callback from parent

### Changing Paste Behavior
- `PasteBehavior` enum: `autoPaste`, `clipboardMode`, `reviewMode`
- Frontend handles paste logic in `processRequest()` callback
- `ReviewMode` shows `ReviewModal` before pasting; others handle automatically

## Type Safety & Serialization

- **Rust→TS**: Serialize Rust structs with `#[serde(rename_all = "camelCase")]` to match TypeScript naming
- **Commands**: Always return `Result<T, String>` from Tauri commands (error is string message)
- **Config DTO**: Use `ConfigurationDto` for frontend serialization (hides sensitive fields like plaintext API key)
- **Skip Serialization**: Use `#[serde(skip)]` for non-persisted fields (e.g., `plaintext_api_key`)

## Performance & Optimization

- **Vite Build**: No source maps in production (`vite.config.ts` sets `sourcemap: false`)
- **Text Selection**: Optional `disable_text_selection` flag skips UI Automation calls
- **Async/Await**: Use Tokio for async I/O in Rust (HTTP requests, file I/O)
- **Hot Module Replacement**: Works for React components; restart Tauri if modifying Rust code

## Key Files Reference

- **Frontend Entry**: `src/main.tsx` → `App.tsx` → AppProvider + MainLayout + page components
- **Backend Entry**: `src-tauri/src/lib.rs` → command handlers → specialized modules
- **Configuration**: `src-tauri/src/config.rs` (load/save), `src/context/AppContext.tsx` (state)
- **Operations**: `src-tauri/src/operations.rs` (definitions + prompts)
- **LLM**: `src-tauri/src/llm.rs` (API communication)
- **Types**: `src/types/index.ts` (TypeScript contracts)
- **Styles**: `src/styles.css` (Tailwind imports) + `tailwind.config.ts`

## Known Constraints

- **Windows-Only Text Selection**: Text capture via UI Automation only works on Windows; fallback to clipboard on macOS/Linux
- **API Key Encryption**: Uses portable AES-GCM; ensure cross-platform compatibility when changing encryption logic
- **Clipboard Ops**: Requires `tauri-plugin-clipboard-manager`; sensitive on some systems (WSL, restricted environments)
- **Tray Icon**: Windows tray integration via `tauri-plugin-global-shortcut` + custom tray menu; paths relative to bundled resources

## Important Notes
- This Tauri 2.0 app that was migrated from C# .net project contained in the /dotnet folder.
- You can inspect the legacy C# .net code in order to understand the complete legacy app    features and to adapt to our new multiplatform Tauri app, the legacy code is in /dotnet folder.
- Please make sure to keep both the Rust backend and TypeScript frontend code well-documented and maintain type safety across the boundary.
- Follow best practices for React component design and Tauri command implementation.
- Ensure cross-platform compatibility, especially for Windows-specific features.

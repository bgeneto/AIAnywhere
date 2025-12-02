# AI Anywhere

A powerful multiplatform desktop application that provides universal AI assistance through global hotkeys. Access any LLM model from anywhere on your system - Word documents, IDEs, browsers, or any text field. Built with Tauri 2.0 (Rust + Typescript + Tailwind CSS) for modern environments.

## ✨ Key Features

### 🚀 Global Access
- **Universal Hotkey**: Press `Ctrl+Space` (fully configurable) from any application
- **Smart Text Capture**: Automatically detects selected text using UI Automation and clipboard fallback
- **System Tray Integration**: Runs minimized in system tray with right-click context menu
- **Background Operation**: Non-intrusive operation that doesn't interfere with your workflow

### 🤖 AI Tasks
- **Custom Task**: Flexible AI help for any task or question
- **Email Reply**: Generate professional email replies with customizable tone and length
- **Text Summarization**: Condense long text into key points with various formats and lengths
- **Text Translation**: Translate to 14+ languages including Spanish, French, German, Chinese, Japanese, Arabic, and more
- **Text Rewriting**: Improve text with configurable tones (formal, informal, professional, casual, academic, creative)
- **Image Generation**: Generate images with DALL-E, FLUX.1 or any compatible models with size and quality options
- **Speech-to-Text**: Transcribe audio files using Whisper or compatible models
- **Text-to-Speech**: Convert text to natural-sounding audio
- **Unicode Symbols**: Quick access to special characters and symbols

### 📝 Markdown Rendering
- **Rich Text Display**: AI responses render with full Markdown support in the Review Modal
- **Syntax Highlighting**: Code blocks are beautifully highlighted with OneDark theme
- **Professional Formatting**: Headers, lists, bold, italics, links, and more render properly
- **LaTeX/Math Support**: Full KaTeX integration for mathematical equations
  - Inline math: `$E = mc^2$` renders as $E = mc^2$
  - Display math blocks using `$$...$$` or `\[...\]`

### 🌊 Streaming Responses
- **Real-time Output**: Watch AI responses generate token-by-token as they're created
- **Cancel Anytime**: Stop generation mid-stream with the Cancel button
- **Live Preview**: See streaming content in real-time before final result

### 📜 Conversation History
- **Automatic Saving**: All successful AI interactions are automatically saved to history
- **Searchable**: Search through past prompts and responses by keyword
- **Re-run Prompts**: Quickly re-run any previous prompt with one click
- **Expandable View**: Click any history entry to see full details including response and options used
- **Easy Management**: Delete individual entries or clear entire history
- **Persistent Storage**: History survives app restarts (stored in `{AppData}/ai-anywhere/history.json`)

### ✨ Custom Tasks Library
- **Create Your Own**: Build custom AI tasks with personalized system prompts
- **Dynamic Options**: Add select dropdowns, text inputs, or number fields to your tasks
- **Placeholder Support**: Use `{placeholders}` in prompts that get replaced with option values
- **Import/Export**: Share your custom tasks as JSON files with others
- **Validation**: Built-in validation ensures placeholders match your defined options

### 🔧 Advanced Features
- **Multiple Response Modes**: Auto-paste, clipboard mode, or review mode with preview window
- **Encrypted Configuration**: API keys are encrypted and stored securely
- **Custom System Prompts**: Advanced users can customize AI behaviour per task/operation type
- **Flexible API Support**: Works with OpenAI, Anthropic Claude, local LLMs, or any OpenAI-compatible endpoint (can be used with e.g. LiteLLM or Openrouter)
- **Robust Text Handling**: Intelligent text selection using both UI Automation and clipboard methods
- **History Limits**: Configure maximum history entries and media retention days

## 🛠️ Technology Stack

### Frontend
- **Framework**: React 18.3 with TypeScript
- **Build Tool**: Vite 6.0
- **Styling**: Tailwind CSS 4.0 + PostCSS
- **State Management**: React Context API
- **Desktop Framework**: Tauri 2.0
- **API Client**: @tauri-apps/api for IPC with Rust backend

### Backend (Rust)
- **Language**: Rust 2021 Edition
- **Desktop Runtime**: Tauri 2.0
- **Async Runtime**: Tokio 1.0 (with full features)
- **HTTP Client**: Reqwest 0.12 (with JSON & multipart support)
- **Serialization**: Serde 1.0 + serde_json
- **Encryption**: AES-GCM + SHA2 + Base64
- **Platform Integration**: 
  - Keyboard/mouse simulation: enigo 0.2 (Windows/macOS/Linux)
  - Clipboard: tauri-plugin-clipboard-manager
  - File dialogs: tauri-plugin-dialog
  - File system: tauri-plugin-fs
  - Shell commands: tauri-plugin-shell
  - Global hotkeys: tauri-plugin-global-shortcut
  - Notifications: tauri-plugin-notification
- **System Paths**: dirs 5.0 (cross-platform config storage)

### Development Tools
- **Language**: TypeScript 5.6
- **Package Manager**: npm/yarn
- **Version Control**: Git

## 📥 Installation

### Download & Install
1. Download the latest release from [Releases](../../releases)
2. Run the installer (`.msi` for Windows, `.dmg` for macOS, `.AppImage` for Linux)
3. AI Anywhere will launch automatically and appear in your system tray
4. Complete first-time setup (see Quick Start below)

### From Source
```bash
# Clone the repository
git clone https://github.com/bgeneto/AIAnywhere.git
cd AIAnywhere

# Install dependencies
npm install

# Run in development mode
npm run dev

# Or build production release
npm run build
npm run tauri build
```


### Linux Requirements

To run AI Anywhere on Linux, you need to install a few system dependencies.

**Debian/Ubuntu:**
```bash
sudo apt update
sudo apt install libwebkit2gtk-4.0-dev \
    build-essential \
    curl \
    wget \
    file \
    libssl-dev \
    libgtk-3-dev \
    libayatana-appindicator3-dev \
    librsvg2-dev \
    libxdo-dev
```

**Fedora/RedHat:**
```bash
sudo dnf install webkit2gtk3-devel \
    openssl-devel \
    curl \
    wget \
    file \
    libappindicator-gtk3-devel \
    librsvg2-devel \
    libxdo-devel
```

**Note on Wayland:**
If you are using Wayland, ensure you have a compatible portal implementation installed (e.g., `xdg-desktop-portal-gnome`, `xdg-desktop-portal-kde`, or `xdg-desktop-portal-hyprland`) for global shortcuts to work.

## 🚀 Quick Start

### First Time Setup
1. **Right-click** the AI Anywhere icon in your system tray
2. Select **"Settings"** from the context menu
3. Configure your settings:
   - **Global Hotkey**: Set your preferred keyboard shortcut (default: `Ctrl+Space`)
   - **API Base URL**: Enter your LLM API endpoint
     - OpenAI: `https://api.openai.com/v1`
     - Anthropic: `https://api.anthropic.com/v1`
     - Local LLMs: Your local endpoint URL, e.g. `http://localhost:11434/v1` (Ollama)
   - **API Key**: Your API key (encrypted automatically) **(Required)**
   - **LLM Model**: Click "Get Models" to populate available options, then select your preferred model **(Required)**
   - **Paste Behaviour**: Choose how responses are handled
   - **Performance Settings**: Enable/disable automatic text selection for optimal performance
4. Click **"Save"** to apply settings (validation ensures required fields are completed)

## 🎯 How to Use

### Basic Workflow
1. **Select text** in any application (optional - will fallback to clipboard, then empty)
2. **Press your hotkey** (default: `Ctrl+Space`)
3. **Select task** from the dropdown menu
4. **Configure options** (language for translation, tone for rewriting, etc.)
5. **Add or edit prompt content** (selected text is automatically prefilled)
6. **Click "Process"** to send to AI
7. **Review and apply** the response based on your paste behavior setting

### Text Selection Priority
AIAnywhere uses an intelligent text capture system with the following priority:
1. **Selected Text**: Uses UI Automation to detect highlighted text
2. **Clipboard Fallback**: If no selection found, uses clipboard content
3. **Empty Prompt**: If both above fail, starts with an empty prompt

This ensures maximum compatibility across different applications and scenarios.

## 👨‍💻 Development Setup

### Prerequisites
- **Node.js** 18+ (with npm)
- **Rust** 1.70+ (install from [rustup.rs](https://rustup.rs/))
- **Visual Studio Build Tools** (Windows) or XCode (macOS) or build-essentials (Linux)

### Development Environment

```bash
# Install project dependencies
npm install

# Start development server (runs Vite + Tauri in watch mode)
npm run dev

# This will:
# 1. Start Vite dev server on http://localhost:1420
# 2. Watch TypeScript source in src/
# 3. Auto-reload Rust code in src-tauri/
# 4. Open Tauri dev window

# Build for production
npm run build && npm run tauri build

# Run Tauri commands directly
npm run tauri dev     # Alternative to npm run dev
npm run tauri build   # Build production bundle
```

### Project Structure
```
AIAnywhere/
├── src/                          # React frontend
│   ├── components/               # React components
│   │   ├── pages/               # Page components (Home, Settings, About)
│   │   ├── *.tsx                # UI components (modals, panels, etc)
│   ├── context/                  # React Context (AppContext.tsx)
│   ├── i18n/                     # Internationalization (en, pt-BR)
│   ├── types/                    # TypeScript type definitions
│   ├── utils/                    # Utility functions
│   ├── styles.css                # Tailwind imports
│   └── main.tsx                  # React entry point
│
├── src-tauri/                    # Rust backend
│   ├── src/
│   │   ├── lib.rs               # Main Tauri commands (15+ handlers)
│   │   ├── config.rs            # Configuration loading/saving
│   │   ├── llm.rs               # LLM service (OpenAI-compatible API)
│   │   ├── operations.rs        # Operation types & system prompts
│   │   ├── encryption.rs        # AES-GCM API key encryption
│   │   ├── clipboard.rs         # Clipboard operations
│   │   ├── text.rs              # Text processing utilities
│   │   └── main.rs              # Rust entry point
│   ├── tauri.conf.json           # Tauri app configuration
│   └── Cargo.toml                # Rust dependencies
│
├── vite.config.ts                # Vite build configuration
├── tsconfig.json                 # TypeScript configuration
├── tailwind.config.ts            # Tailwind CSS configuration
└── package.json                  # Node.js dependencies
```

### Common Development Tasks

**Adding a new operation type:**
1. Add variant to `OperationType` enum in `src-tauri/src/operations.rs`
2. Define options and system prompt in `get_operations()`
3. Add default prompt in `get_default_system_prompts()`
4. Update TypeScript type in `src/types/index.ts`

**Modifying UI:**
- Edit React components in `src/components/`
- Use `useApp()` hook to access AppContext
- Use `useI18n()` hook for translations
- Tailwind CSS for styling (dark mode via `dark:` prefix)

**Testing API connectivity:**
- Use `testConnectionWithEndpoint()` to validate without fetching models
- Check `enable_debug_logging` config flag to see API requests

## 🎛️ Available Tasks

### 🤖 Custom AI Task
**Purpose**: Flexible AI help for any task or question
- **Use Cases**: Writing assistance, code help, explanations, creative tasks, problem-solving
- **System Prompt**: Optimized for non-interactive, immediate responses without follow-up questions
- **Best For**: Quick answers, content generation, analysis

### 📧 Email Reply
**Purpose**: Generate professional email replies to received messages
- **Features**:
  - **Smart Reply Generation**: Analyzes the original email and creates contextually appropriate responses
  - **Tone Options**: Professional, Friendly, Formal, Urgent, Apologetic, Enthusiastic
  - **Length Control**: Brief (2-4 sentences), Standard (1-2 paragraphs), Detailed (2-3 paragraphs)
  - **Professional Structure**: Includes greeting, acknowledgment, responses to key points, and closing
- **Use Cases**: Business correspondence, customer service, follow-up communications
- **Auto-Format**: Generates complete email with proper greeting and signature placeholder

### 📄 Text Summarization
**Purpose**: Condense long text into key points and essential information
- **Summary Lengths**:
  - **Brief**: 2-3 sentences or 3-5 bullet points maximum
  - **Medium**: 1 paragraph or 5-8 bullet points
  - **Detailed**: 2-3 paragraphs or 8-12 bullet points
- **Format Options**:
  - **Paragraph**: Flowing, connected sentences
  - **Bullet Points**: Clear, actionable points with consistent structure
  - **Executive Summary**: Overview, key findings, and implications
  - **Key Takeaways**: Most important insights and actionable points
- **Use Cases**: Research papers, meeting notes, articles, reports, documentation
- **Features**: Preserves core message, maintains logical flow, focuses on facts and decisions

### 🌍 Text Translation
**Purpose**: Professional-quality translation between languages
- **Supported Languages**:
  - **European**: English, Spanish, French, German, Italian, Portuguese, Russian
  - **Asian**: Chinese, Japanese, Korean, Hindi, Bengali, Punjabi
  - **Middle Eastern/African**: Arabic
- **Features**:
  - Maintains original writing style and formatting
  - Context-aware translation
  - Professional translator-quality output
- **Auto-Replace**: Automatically replaces selected text with translation

### ✍️ Text Rewriting
**Purpose**: Improve and refine existing text with different tones
- **Available Tones**:
  - **Formal**: Professional, structured language
  - **Informal**: Conversational, relaxed tone
  - **Professional**: Business-appropriate, polished
  - **Casual**: Friendly, approachable style
  - **Academic**: Scholarly, research-oriented
  - **Creative**: Imaginative, expressive language
- **Features**:
  - Maintains original meaning
  - Improves clarity and flow
  - Keeps similar length (±20%)
  - Corrects grammar and enhances readability

### 🎨 Image Generation
**Purpose**: Create images from text descriptions using AI
- **Supported Sizes**:
  - `1024x1024` - Square format, ideal for social media
  - `1024x768` - Landscape format, great for presentations
  - `512x512` - Smaller square format, faster generation
- **Quality Options**:
  - **Standard**: Higher quality, more detailed images
  - **Low**: Faster generation, reduced detail
- **Output**: Opens generated images directly in your default browser
- **Compatible**: Works with DALL-E and other OpenAI-compatible image models

### 🎤 Speech-to-Text
**Purpose**: Transcribe audio to text
- **Supported Formats**: MP3, WAV, M4A, FLAC, OGG, WEBM
- **Max File Size**: 25 MB
- **Compatible Models**: OpenAI Whisper API and compatible endpoints
- **Features**: Automatic language detection, high accuracy transcription

### 🔤 Unicode Symbols
**Purpose**: Generate Unicode symbols and special characters
- **Use Cases**: Mathematical symbols, arrows, decorative characters, emoji variants
- **Features**: Fast symbol generation, searchable results

### 💬 WhatsApp Response
**Purpose**: Generate WhatsApp-style messages
- **Tone Options**: Friendly, Professional, Casual, Formal, Humorous
- **Length Control**: Brief, Medium, Detailed
- **Features**: Natural conversational style, emoji support, message formatting

## 🏗️ Architecture Overview

### Communication Flow
```
User Interaction (UI)
    ↓
React Component (HomePage, SettingsPage)
    ↓
AppContext (state management)
    ↓
Tauri invoke() [TypeScript]
    ↓
Rust #[tauri::command] handler
    ↓
Specialized modules (config, llm, operations, etc)
    ↓
External APIs (OpenAI, local LLMs, etc)
    ↓
Response → Clipboard/Paste/ReviewModal
```

### Configuration Storage
- **Location**: `{UserAppData}/ai-anywhere/config.json`
  - Windows: `C:\Users\<user>\AppData\Roaming\ai-anywhere\config.json`
  - macOS: `~/Library/Application Support/ai-anywhere/config.json`
  - Linux: `~/.local/share/ai-anywhere/config.json`
- **Format**: JSON with encrypted API key (AES-GCM)
- **Sensitive Data**: API key encrypted at rest, decrypted in memory only

### LLM Request Pipeline
1. Frontend collects user input + operation type + options
2. Rust backend matches operation to system prompt
3. System prompt placeholders replaced with selected options
4. Request sent to configured API endpoint
5. Response processed based on type (text/image/audio)
6. Result returned to UI with success/error status

## ⚙️ Configuration Options

### Basic Settings
| Setting | Description | Default | Examples |
|---------|-------------|---------|----------|
| **Global Hotkey** | Keyboard combination to activate | `Ctrl+Space` | `Ctrl+Space`, `Alt+Space` |
| **API Base URL** | Your LLM service endpoint | `https://api.openai.com/v1` | Local: `http://localhost:11434/v1` |
| **API Key*** | Authentication key (encrypted) | _(empty)_ | `sk-...` (OpenAI format) |
| **LLM Model*** | Model name to use (required) | _(empty)_ | `gpt-4o`, `claude-3-opus`, `llama3.2` |

**\*Required fields** - Configuration cannot be saved without these values.

### Response Behavior
| Mode | Description | When to Use |
|------|-------------|-------------|
| **Auto Paste** | Automatically replaces selected text | Quick edits, translations |
| **Clipboard Mode** | Copies to clipboard, manual paste | Protected fields, careful placement |
| **Review Mode** | Shows preview before applying | Verification, multiple options |

### Performance Settings

#### Text Selection and Clipboard Detection
**Setting**: Disable automatic text selection and clipboard detection

AIAnywhere offers flexible performance optimization to match your workflow preferences:

**✅ Enabled (Default)**
- **Smart Text Prefilling**: Automatically detects selected text using UI Automation
- **Clipboard Fallback**: Uses clipboard content when no text is selected
- **Seamless Workflow**: Selected text appears instantly in the prompt window
- **Full Compatibility**: Works with most applications and text fields

**⚡ Disabled (Performance Mode)**
- **Lightning-Fast Opening**: Window opens instantly without text detection delays
- **Predictable Response**: Consistent performance regardless of target application
- **System Efficiency**: Reduced CPU usage and better performance on slower systems
- **Clean Start**: Always begins with an empty prompt for manual input

### Advanced Configuration

#### Custom System Prompts
Power users can customize AI behavior by editing the `config.json` file directly, including LLM prompt editing:

```json
{
  "hotkey": "Ctrl+Space",
  "apiBaseUrl": "https://api.openai.com/v1",
  "apiKey": "encrypted_key_here",
  "llmModel": "gpt-4o",
  "imageModel": "dall-e-3",
  "audioModel": "whisper-1",
  "ttsModel": "tts-1-hd",
  "pasteBehavior": "reviewMode",
  "disableTextSelection": false,
  "disableThinking": true,
  "enableDebugLogging": false,
  "models": ["gpt-4o", "gpt-4-turbo", "gpt-3.5-turbo"],
  "imageModels": ["dall-e-3", "dall-e-2"],
  "audioModels": ["whisper-1"],
  "systemPrompts": {
    "generalChat": "LANGUAGE RULE: Always respond in the same language as the user's input...",
    "emailReply": "LANGUAGE RULE: Respond in the EXACT same language as the original email...",
    "textSummarization": "LANGUAGE RULE: Summarize in the same language as the input...",
    "textTranslation": "Translate the text to {targetLanguage}...",
    "textRewrite": "LANGUAGE RULE: Rewrite in {tone} tone in the same language...",
    "imageGeneration": "Create an image based on this description...",
    "whatsAppResponse": "Generate a WhatsApp response with {tone} tone...",
    "textToSpeech": "Generate audio for this text...",
    "speechToText": "Transcribe this audio to text...",
    "unicodeSymbols": "Generate Unicode symbols for..."
  }
}
```

#### Environment Configuration

| Setting | Type | Default | Purpose |
|---------|------|---------|----------|
| `hotkey` | string | `Ctrl+Space` | Global activation hotkey |
| `apiBaseUrl` | string | `https://api.openai.com/v1` | LLM API endpoint |
| `llmModel` | string | _(empty)_ | Primary text/chat model |
| `imageModel` | string | _(empty)_ | Image generation model |
| `audioModel` | string | _(empty)_ | Speech-to-text model |
| `ttsModel` | string | `tts-1-hd` | Text-to-speech model |
| `disableTextSelection` | boolean | `false` | Skip UI Automation text capture |
| `disableThinking` | boolean | `true` | Disable extended thinking (o1, etc) |
| `enableDebugLogging` | boolean | `false` | Log API requests/responses |

#### Security Features
- **API Key Encryption**: Keys are encrypted using AES-GCM-256 with PBKDF2 key derivation
- **Local Storage**: All settings stored locally in AppData, never transmitted to external servers
- **Memory Protection**: Sensitive data (API keys) only decrypted in memory when needed
- **No Telemetry**: Zero tracking or analytics
- **Cross-Platform**: Encryption uses portable CSPRNG (cryptographically secure random)

## 🌐 Supported AI Providers

AI Anywhere works with any **OpenAI-compatible API** endpoint. The app auto-detects available models via the `/v1/models` endpoint.

### OpenAI (Official)
- **Base URL**: `https://api.openai.com/v1`
- **Chat Models**: `gpt-4o`, `gpt-4-turbo`, `gpt-4`, `gpt-3.5-turbo`
- **Image Models**: `dall-e-3`, `dall-e-2`
- **Audio Models**: `whisper-1` (STT), `tts-1`, `tts-1-hd` (TTS)
- **Features**: Full support for all operations
- **Setup**: [Get API key](https://platform.openai.com/api-keys)

### Anthropic Claude
- **Base URL**: `https://api.anthropic.com/v1`
- **Models**: `claude-3-5-sonnet-20241022`, `claude-3-opus-20240229`, `claude-3-sonnet-20240229`, `claude-3-haiku-20240307`
- **Features**: All text operations (no image generation)
- **Setup**: [Get API key](https://console.anthropic.com/)

### Google Gemini
- **Base URL**: `https://generativelanguage.googleapis.com/v1beta/openai/`
- **Models**: `gemini-2.0-flash`, `gemini-1.5-pro`, `gemini-1.5-flash`
- **Features**: Text operations with vision support
- **Note**: Requires OpenAI-compatible proxy or use direct HTTP

### Local LLMs

#### Ollama
- **Base URL**: `http://localhost:11434/v1`
- **Setup**: [Install Ollama](https://ollama.ai/), then `ollama serve`
- **Popular Models**: `llama3.2`, `mistral`, `neural-chat`, `orca-mini`
- **Command**: `ollama pull llama3.2 && ollama serve`

#### LM Studio
- **Base URL**: `http://localhost:1234/v1`
- **Setup**: Download from [lmstudio.ai](https://lmstudio.ai/), load model, enable API
- **Popular Models**: Llama, Mistral, Qwen, Phi variants

#### Text Generation WebUI
- **Base URL**: `http://localhost:5000/v1`
- **Setup**: [GitHub](https://github.com/oobabooga/text-generation-webui)
- **Features**: Advanced model management and tuning

#### vLLM
- **Base URL**: `http://localhost:8000/v1`
- **Setup**: `pip install vllm && vllm serve <model>`
- **Best For**: High-throughput inference

### Proxy/Aggregator Services

#### OpenRouter
- **Base URL**: `https://openrouter.ai/api/v1`
- **Models**: 100+ models (OpenAI, Claude, local, etc)
- **Setup**: [Get API key](https://openrouter.ai/)
- **Best For**: Model experimentation, usage aggregation

#### LiteLLM
- **Base URL**: Custom (usually `http://localhost:4000/v1`)
- **Setup**: `pip install litellm && litellm --model`
- **Best For**: Using multiple APIs with unified interface

#### Together AI
- **Base URL**: `https://api.together.xyz/v1`
- **Models**: Open-source models (Llama, Mistral, etc)
- **Setup**: [Get API key](https://www.together.ai/)

#### Azure OpenAI
- **Base URL**: `https://<your-resource>.openai.azure.com/v1`
- **Setup**: Use Azure resource name and API key from Azure portal
- **Best For**: Enterprise deployments

### Configuration by Provider

**To use any provider:**
1. Open Settings (right-click tray icon)
2. Enter **API Base URL** (from table above)
3. Enter **API Key**
4. Click **"Refresh Models"** to auto-detect available models
5. Select your preferred **Text Model** and others
6. Save and start using!

## 💻 System Requirements

### Minimum Requirements
- **OS**: Windows 10 version 1809 or later
- **Architecture**: x64 (64-bit)
- **RAM**: 512 MB available memory
- **Storage**: 100 MB free disk space
- **Network**: Internet connection for AI API access

### Recommended Requirements
- **OS**: Windows 11, Linux or MacOS
- **RAM**: 1 GB available memory
- **Storage**: 15 MB free disk space
- **Network**: Stable broadband connection

### Compatibility
- **Applications**: Works with most Windows applications including:
  - Microsoft Office (Word, Excel, PowerPoint, Outlook)
  - Web browsers (Chrome, Firefox, Edge)
  - Code editors (VS Code, Visual Studio, Notepad++)
  - Chat applications (Discord, Slack, Teams)
  - Text editors and IDEs

## ⌨️ Keyboard Shortcuts & Tips

### Default Shortcuts
- **`Ctrl+Space`**: Activate AI Anywhere (globally configurable)
- **`Ctrl+C`**: Copy text before using hotkey (if selection doesn't work)
- **`Esc`**: Close any AI Anywhere window
- **`Ctrl+Enter`**: Submit prompt (in prompt window)

### Pro Tips
1. **Quick Translation**: Select text → Hotkey → Choose "Text Translation" → Process
2. **Improve Writing**: Select text → Hotkey → "Text Rewrite" → Choose tone → Process
3. **Email Replies**: Select received email → Hotkey → "Email Reply" → Choose tone → Process
4. **Summarize Content**: Select long text → Hotkey → "Text Summarization" → Choose format → Process
5. **Clipboard Mode**: Perfect for protected fields where auto-paste doesn't work
6. **Review Mode**: Great for checking AI responses before applying them
7. **Custom Prompts**: Combine operations/tasks (e.g., "Translate to Spanish and make it formal")

## 🌍 Internationalization (i18n)

### Supported Languages
- **English** (`en`) - Default
- **Portuguese-Brazil** (`pt-BR`)

More languages can be added by contributing translations.

### Changing Language
1. Open Settings (right-click tray icon → Settings)
2. Navigate to **Language Settings**
3. Select your preferred language
4. Changes apply immediately

### Adding a New Language
1. Edit `src/i18n/translations.ts`
2. Add new language object with all translation keys
3. Add language to `Language` type
4. Update `I18nProvider` auto-detection if needed
5. Submit PR

## 🐛 Troubleshooting

### "API Key not working"
- Verify your API key is correct (no spaces, full key copied)
- Check API endpoint URL is correct (e.g., `https://api.openai.com/v1`)
- Click "Test API Connectivity" to validate credentials
- Check `enableDebugLogging: true` in config to see API errors
- Verify API account has available credits/balance

### "Text selection not working"
- Try copying text manually (`Ctrl+C`) before pressing hotkey
- Enable "Clipboard Mode" in Settings
- Some applications (web browsers in Protected Mode) may block text capture
- Check `disableTextSelection: false` to ensure feature is enabled
- Windows 10: Ensure UI Automation is enabled

### "Application won't start"
- Check Windows Defender/antivirus isn't blocking the executable
- Verify Windows 10 version 1809 or later
- Check disk space (minimum 100 MB)
- Try uninstalling and reinstalling

### "Paste not working as expected"
- Try "Review Mode" to verify output before pasting
- Use "Clipboard Mode" for protected fields (passwords, forms)
- Some apps don't support automated pasting - use clipboard mode
- Restart the application if paste mode stops responding

### "API requests very slow"
- Check internet connection speed
- Try different API endpoint (local LLM vs cloud)
- Larger models take longer (use faster models for quick responses)
- Check `enableDebugLogging: true` to see request/response times

### "Custom system prompts not working"
- Verify JSON syntax in `config.json` (use JSON validator)
- Restart application after editing config file
- Placeholder names must match exactly: `{tone}`, `{language}`, `{length}`
- Check operation type name matches enum (camelCase in JSON)

## 📝 Configuration File Location

**Windows**:
```
C:\Users\<username>\AppData\Roaming\ai-anywhere\config.json
```

**macOS**:
```
~/Library/Application Support/ai-anywhere/config.json
```

**Linux**:
```
~/.local/share/ai-anywhere/config.json
```

You can edit this file directly to customize system prompts or other settings. Remember to restart the app after changes.

## 🤝 Contributing

We welcome contributions from the community! This project is actively maintained and we're looking to expand its capabilities.

### How to Contribute
1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/AmazingFeature`)
3. **Commit** your changes (`git commit -m 'Add some AmazingFeature'`)
4. **Push** to the branch (`git push origin feature/AmazingFeature`)
5. **Open** a Pull Request

### Development Guidelines

#### Code Style
- **Rust**: Follow Rust conventions (use `cargo fmt` and `cargo clippy`)
- **TypeScript/React**: Use existing patterns in `src/components/` and `src/context/`
- **Naming**: 
  - Rust: `snake_case` for functions/variables
  - TypeScript: `camelCase` for variables/functions, `PascalCase` for types/components
- **Comments**: Document public APIs, complex logic, and non-obvious behavior

#### Serialization
- Always use `#[serde(rename_all = "camelCase")]` in Rust for TypeScript compatibility
- Keep TypeScript types in `src/types/index.ts` synchronized with Rust structs
- Use `Result<T, String>` as return type for all Tauri commands

#### Testing
- Test new operations with real API calls (use test API key)
- Test text capture with different applications
- Verify paste behavior works in protected fields (Word, password managers, etc)
- Test with multiple Windows versions if possible
- Test with both local and cloud-based LLM endpoints

#### Documentation
- Update README for user-facing changes
- Add comments for complex algorithms or business logic
- Document new operation types and their system prompts
- Update `.github/copilot-instructions.md` for architecture changes
- Include examples in PR description

#### Before Submitting PR
```bash
# Format Rust code
cd src-tauri && cargo fmt && cargo clippy && cd ..

# Type-check TypeScript
npm run build

# Test in dev mode
npm run dev

# Create production build
npm run tauri build
```

### Bug Reports & Feature Requests
- Use the [Issues](../../issues) tab to report bugs
- Provide detailed reproduction steps
- Include system information and error messages
- Search existing issues before creating new ones

## ❓ Frequently Asked Questions

**Q: Is my API key safe?**
A: Yes. API keys are encrypted with AES-GCM-256 and stored locally on your computer. They're never sent anywhere except to your configured API endpoint.

**Q: Can I use this with local LLMs?**
A: Yes! Any OpenAI-compatible endpoint works. We recommend Ollama or LM Studio for ease of setup.

**Q: Does this work on macOS and Linux?**
A: The codebase supports macOS and Linux, but the binary releases are currently Windows-only. You can build from source on other platforms.

**Q: Can I customize the system prompts?**
A: Yes! Edit `config.json` directly (located in AppData). Use placeholders like `{tone}`, `{language}` that are replaced at runtime.

**Q: Why does text selection sometimes not work?**
A: Text selection uses Windows UI Automation which some apps disable for security. Use clipboard mode or copy text manually as a fallback.

**Q: How do I report a bug?**
A: Open an issue on [GitHub Issues](../../issues) with:
- Exact steps to reproduce
- Error message (if any)
- Windows version
- API provider and model name
- Config settings (hide sensitive values)

**Q: How can I contribute a translation?**
A: Edit `src/i18n/translations.ts`, add your language, and submit a PR.

**Q: Can I change the hotkey?**
A: Yes, in Settings → API Settings → Global Hotkey. Any standard keyboard combination works.

**Q: Does this work in web browsers?**
A: Yes, with limitations. Text selection works in Chrome, Firefox, and Edge. Some protected web apps may block clipboard access.

**Q: Can I use multiple API keys?**
A: Not yet. Currently one key per endpoint. You can switch endpoints in Settings and use different keys for each.

**Q: What data is collected?**
A: None. AI Anywhere has zero telemetry. No data is sent anywhere except to your configured LLM API endpoint.

## 📚 Resources

### Documentation
- [API Configuration Guide](docs/api-configuration.md) _(coming soon)_
- [Custom System Prompts Guide](docs/system-prompts.md) _(coming soon)_
- [Architecture Overview](.github/copilot-instructions.md)

### Links
- **Issue Tracker**: [GitHub Issues](../../issues)
- **Discussions**: [GitHub Discussions](../../discussions)
- **Releases**: [Download Latest](../../releases)
- **License**: [MIT License](LICENSE)

### Supported LLM Providers
- [OpenAI](https://openai.com) - GPT-4, GPT-3.5, DALL-E, Whisper
- [Anthropic](https://anthropic.com) - Claude models
- [Ollama](https://ollama.ai) - Local LLMs
- [LM Studio](https://lmstudio.ai) - Desktop local models
- [OpenRouter](https://openrouter.ai) - Model aggregator
- [Together AI](https://together.ai) - Open-source models
- [Azure OpenAI](https://azure.microsoft.com/openai) - Enterprise

## 📄 License

**AI Anywhere** is released under the **MIT License**.

```
MIT License

Copyright (c) 2025 LABiA-FUP/UnB

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

---

---

**Made with ❤️ for productivity enthusiasts who want AI assistance everywhere they work.**

*Questions? Issues? Ideas? Open an [issue](../../issues) or start a [discussion](../../discussions)!*

# AI Anywhere

A powerful multiplatform desktop application that provides universal AI assistance through global hotkeys. Access any LLM model from anywhere on your system - Word documents, IDEs, browsers, or any text field. Built with Tauri 2.0 (Rust + TypeScript + Tailwind CSS).

<img width="674" height="470" alt="image" src="https://github.com/user-attachments/assets/0f44a96a-94da-4ce6-aea5-de82212be6c9" />

## ✨ Key Features

- **Universal Hotkey**: Press `Ctrl+Space` (configurable) from any application
- **Smart Text Capture**: Automatically detects selected text with clipboard fallback
- **System Tray Integration**: Runs minimized with right-click context menu
- **Streaming Responses**: Real-time token-by-token output with cancel support
- **Conversation History**: Searchable, persistent history with re-run capability
- **Custom Tasks Library**: Create your own AI tasks with dynamic options
- **Multiple Response Modes**: Auto-paste, clipboard mode, or review mode
- **Encrypted Configuration**: API keys stored securely with AES-GCM encryption
- **Markdown & LaTeX**: Full rendering support including syntax highlighting and KaTeX math

### 🤖 Built-in AI Tasks

| Task | Description |
|------|-------------|
| **Custom Task** | Flexible AI help for any question |
| **Email Reply** | Professional replies with tone/length options |
| **Text Summarization** | Condense text with format options |
| **Text Translation** | 14+ languages supported |
| **Text Rewriting** | Improve text with configurable tones |
| **Image Generation** | DALL-E, FLUX.1, or compatible models |
| **Speech-to-Text** | Whisper-compatible transcription |
| **Text-to-Speech** | Natural-sounding audio generation |
| **Unicode Symbols** | Quick access to special characters |
| **WhatsApp Response** | Casual message replies |

## 📥 Installation

### Download & Install
1. Download from [Releases](../../releases) (`.msi` for Windows, `.AppImage` for Linux)
2. Run the installer - AI Anywhere appears in your system tray
3. Right-click tray icon → Settings to configure

### From Source
```bash
git clone https://github.com/bgeneto/AIAnywhere.git
cd AIAnywhere
npm install
npm run dev          # Development mode
npm run tauri build  # Production build
```

### Linux Dependencies

<details>
<summary>Debian/Ubuntu</summary>

```bash
sudo apt install libwebkit2gtk-4.1-0 libayatana-appindicator3-1 libxdo3
```
</details>

<details>
<summary>Fedora/RedHat</summary>

```bash
sudo dnf install webkit2gtk4.1 libayatana-appindicator3 libxdo
```
</details>

**Wayland**: Ensure you have a compatible portal (e.g., `xdg-desktop-portal-gnome`) for global shortcuts.

## 🚀 Quick Start

1. **Right-click** the system tray icon → **Settings**
2. Configure:
   - **API Base URL**: Your LLM endpoint (e.g., `https://api.openai.com/v1`)
   - **API Key**: Your API key (encrypted automatically)
   - **LLM Model**: Click "Get Models" and select one
3. **Save** and start using!

### Basic Workflow
1. Select text in any application (optional)
2. Press hotkey (`Ctrl+Space`)
3. Choose task and configure options
4. Click **Send** or press `Ctrl+Enter`

## 🌐 Supported AI Providers

Works with any **OpenAI-compatible API**. Auto-detects available models.

| Provider | Base URL | Notes |
|----------|----------|-------|
| **OpenAI** | `https://api.openai.com/v1` | Full support |
| **Anthropic** | `https://api.anthropic.com/v1` | Text only |
| **Google Gemini** | `https://generativelanguage.googleapis.com/v1beta/openai/` | Text + vision |
| **Ollama** | `http://localhost:11434/v1` | Local LLMs |
| **LM Studio** | `http://localhost:1234/v1` | Local LLMs |
| **OpenRouter** | `https://openrouter.ai/api/v1` | 100+ models |
| **Together AI** | `https://api.together.xyz/v1` | Open-source models |
| **Azure OpenAI** | `https://<resource>.openai.azure.com/v1` | Enterprise |

## ⚙️ Configuration

### Settings Overview

| Setting | Default | Description |
|---------|---------|-------------|
| `hotkey` | `Ctrl+Space` | Global activation |
| `pasteBehavior` | `reviewMode` | `autoPaste`, `clipboardMode`, or `reviewMode` |
| `disableTextSelection` | `false` | Skip auto text capture for faster startup |
| `enableDebugLogging` | `false` | Log API requests |

### Config File Locations
- **Windows**: `%APPDATA%\ai-anywhere\config.json`
- **macOS**: `~/Library/Application Support/ai-anywhere/config.json`
- **Linux**: `~/.local/share/ai-anywhere/config.json`

### Custom System Prompts
Edit `config.json` to customize AI behavior per task. Use placeholders like `{tone}`, `{language}` that are replaced at runtime.

## 🐛 Troubleshooting

<details>
<summary><strong>API Key not working</strong></summary>

- Verify key is correct (no spaces)
- Check endpoint URL format
- Enable `enableDebugLogging` to see errors
- Verify account has credits
</details>

<details>
<summary><strong>Text selection not working</strong></summary>

- Copy text manually (`Ctrl+C`) before hotkey
- Use "Clipboard Mode" in Settings
- Some protected apps block text capture
</details>

<details>
<summary><strong>Paste not working</strong></summary>

- Try "Review Mode" first
- Use "Clipboard Mode" for protected fields
- Restart app if paste stops responding
</details>

<details>
<summary><strong>Linux: Application won't start</strong></summary>

Install missing dependencies:
```bash
# Debian/Ubuntu
sudo apt install libayatana-appindicator3-1 libxdo3 libwebkit2gtk-4.1-0

# Fedora
sudo dnf install libayatana-appindicator3 libxdo webkit2gtk4.1
```

For Wayland + NVIDIA issues:
```bash
WEBKIT_DISABLE_DMABUF_RENDERER=1 ai-anywhere
```
</details>

<details>
<summary><strong>Linux Wayland: Global hotkey and text capture limitations</strong></summary>

Due to Wayland's security model, the following limitations apply:

- **Global hotkeys may not work**: Wayland prevents applications from registering global shortcuts directly. You may need to configure a system-level shortcut in your desktop environment (GNOME/KDE Settings → Keyboard Shortcuts) to launch AI Anywhere.

- **Automatic text capture is not possible**: Wayland blocks applications from simulating keystrokes (like Ctrl+C) in other apps. **You must manually copy (Ctrl+C) your selected text before triggering the hotkey.**

**Recommended workflow on Wayland:**
1. Select text in any application
2. Press `Ctrl+C` to copy
3. Press your system shortcut to open AI Anywhere (or use the tray icon)
4. The copied text will automatically appear in the prompt

**Note:** These are fundamental Wayland security restrictions, not bugs. Consider using an X11 session if you need full global hotkey support.
</details>

## 👨‍💻 Development

### Prerequisites
- Node.js 18+
- Rust 1.70+ ([rustup.rs](https://rustup.rs/))
- Platform build tools (VS Build Tools / XCode / build-essential)

### Project Structure
```
AIAnywhere/
├── src/                    # React frontend
│   ├── components/         # UI components
│   ├── context/           # AppContext state
│   ├── i18n/              # Translations (en, pt-BR)
│   └── types/             # TypeScript definitions
├── src-tauri/             # Rust backend
│   └── src/
│       ├── lib.rs         # Tauri commands
│       ├── llm.rs         # API communication
│       ├── operations.rs  # Task definitions
│       └── config.rs      # Configuration
└── package.json
```

### Commands
```bash
npm run dev           # Development with hot reload
npm run build         # Production build
npm run tauri build   # Create installers
cargo fmt && cargo clippy  # Format/lint Rust
```

### Adding a New Task
1. Add to `OperationType` enum in `operations.rs`
2. Define options and system prompt in `get_operations()`
3. Update TypeScript type in `src/types/index.ts`

## 🤝 Contributing

1. Fork the repository
2. Create feature branch (`git checkout -b feature/Amazing`)
3. Commit changes (`git commit -m 'Add Amazing'`)
4. Push and open a Pull Request

See [Architecture Overview](docs/SOFTWARE_ARCHITECTURE.md) for detailed guidelines.

## ❓ FAQ

**Q: Is my API key safe?**  
A: Yes. Encrypted with AES-GCM-256, stored locally, never transmitted except to your configured endpoint.

**Q: Does it work on macOS/Linux?**  
A: Yes! Build from source, or use Linux releases. macOS binaries coming soon.

**Q: Can I customize prompts?**  
A: Yes. Edit `config.json` directly with placeholders like `{tone}`, `{language}`.

**Q: What data is collected?**  
A: None. Zero telemetry. Data only goes to your configured LLM API.

## 📚 Links

- [Releases](../../releases) | [Issues](../../issues) | [Discussions](../../discussions)
- [Architecture Docs](docs/SOFTWARE_ARCHITECTURE.md)
- [User Manual](USER_MANUAL.md)

## 📄 License

MIT License - Copyright (c) 2025 LABiA-FUP/UnB

---

**Made with ❤️ for productivity enthusiasts who want AI assistance everywhere they work.**



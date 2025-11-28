# AI Anywhere

A powerful Windows desktop application that provides universal AI assistance through global hotkeys. Access any LLM model from anywhere on your system - Word documents, IDEs, browsers, or any text field. Built with WPF and .NET 10.0 for modern Windows environments.

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

### 🔧 Advanced Features
- **Multiple Response Modes**: Auto-paste, clipboard mode, or review mode with preview window
- **Encrypted Configuration**: API keys are encrypted and stored securely
- **Custom System Prompts**: Advanced users can customize AI behaviour per task/operation type
- **Flexible API Support**: Works with OpenAI, Anthropic Claude, local LLMs, or any OpenAI-compatible endpoint (can be used with e.g. LiteLLM or Openrouter)
- **Robust Text Handling**: Intelligent text selection using both UI Automation and clipboard methods

![screen-shot](https://github.com/user-attachments/assets/c9247a50-48ad-4c60-a85d-d62761911fc0)

![screen-shot2](https://github.com/user-attachments/assets/2389bd40-0765-40f2-a26b-e69c9b081300)

## 🚀 Quick Start

### Prerequisites
- **Windows 10/11** (64-bit)
- **.NET 10.0 Runtime** ([Download here](https://dotnet.microsoft.com/download/dotnet/10.0))

### Installation
1. Download the latest release from the [releases page](../../releases)
2. Extract the ZIP file to your preferred location
3. Run `AIAnywhere.exe`
4. The app will minimize to your system tray

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
  "Hotkey": "Ctrl+Space",
  "ApiBaseUrl": "https://api.openai.com/v1",
  "ApiKey": "encrypted_key_here",
  "LlmModel": "gpt-4o",
  "PasteBehavior": "ReviewMode",
  "DisableTextSelection": false,
  "SystemPrompts": {
    "GeneralChat": "Your custom system prompt for general tasks...",
    "EmailReply": "Custom email reply instructions...",
    "TextSummarization": "Custom summarization guidelines...",
    "TextTranslation": "Custom translation instructions...",
    "TextRewrite": "Custom rewriting guidelines...",
    "ImageGeneration": "Custom image generation prompt..."
  }
}
```



#### Security Features
- **API Key Encryption**: Keys are automatically encrypted using portable encryption
- **Local Storage**: All settings stored locally, never transmitted to third parties
- **Memory Protection**: Sensitive data cleared from memory after use

## 🌐 Supported AI Providers

### OpenAI
- **Base URL**: `https://api.openai.com/v1`
- **Models**: `gpt-4o`, `gpt-4.1`, `gpt-3.5-turbo`, etc...
- **Features**: Full support for all tasks including image generation

### Anthropic Claude
- **Base URL**: `https://api.anthropic.com/v1`
- **Models**: `claude-3-opus`, `claude-3-sonnet`, `claude-3-haiku`
- **Features**: Text operations (email replies, summarization, translation, rewriting, general tasks)
- **Note**: Image generation not supported

### Local LLMs
- **Ollama**: `http://localhost:11434/v1`
- **LM Studio**: `http://localhost:1234/v1`
- **Text Generation WebUI**: `http://localhost:5000/v1`
- **Models**: Any local model compatible with OpenAI API format like `gemma3`, `llama3.2`...
- **Features**: Text operations (email replies, summarization, translation, rewriting, general tasks), image generation (varies by model capabilities)

### Other Compatible Services
- **Azure OpenAI**: Use your Azure endpoint URL
- **OpenRouter**: `https://openrouter.ai/api/v1`
- **Together AI**: `https://api.together.xyz/v1`
- **Any OpenAI-compatible API**: Just set the correct base URL

## 💻 System Requirements

### Minimum Requirements
- **OS**: Windows 10 version 1809 or later
- **Architecture**: x64 (64-bit)
- **RAM**: 512 MB available memory
- **Storage**: 100 MB free disk space
- **Network**: Internet connection for AI API access

### Recommended Requirements
- **OS**: Windows 11 (latest version)
- **RAM**: 1 GB available memory
- **Storage**: 500 MB free disk space
- **Network**: Stable broadband connection
- **.NET**: .NET 10.0 Runtime (automatically installed if needed)

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

## 🤝 Contributing

We welcome contributions from the community! This project is actively maintained and we're looking to expand its capabilities.

### How to Contribute
1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/AmazingFeature`)
3. **Commit** your changes (`git commit -m 'Add some AmazingFeature'`)
4. **Push** to the branch (`git push origin feature/AmazingFeature`)
5. **Open** a Pull Request

### Development Guidelines
- Follow existing code style and patterns
- Add unit tests for new features
- Update documentation for any API changes
- Test with multiple Windows versions and applications
- Consider accessibility and internationalization

### Bug Reports & Feature Requests
- Use the [Issues](../../issues) tab to report bugs
- Provide detailed reproduction steps
- Include system information and error messages
- Search existing issues before creating new ones

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

## 🎉 Acknowledgments

- Built with **WPF** and **.NET 10.0** for modern Windows development
- **ModernWPF** for contemporary UI styling
- **UI Automation** for advanced text selection capabilities
- **OpenAI API** specification for broad LLM compatibility
- Community feedback and contributions that make this project better

---

**Made with ❤️ for productivity enthusiasts who want AI assistance everywhere they work.**

*Questions? Issues? Ideas? Open an [issue](../../issues) or start a [discussion](../../discussions)!*

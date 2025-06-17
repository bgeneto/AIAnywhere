# AI Anywhere

A Windows desktop application that provides universal AI assistance through global hotkeys. Access any LLM model from anywhere on your system - Word documents, IDEs, browsers, or any text field.

## Features

- **Global Hotkey Access**: Press `Ctrl+Alt+A` (configurable) from any application
- **Multiple AI Operations**:
  - Text Translation (to various languages)
  - Text Rewriting (with different tones)
  - Image Generation (DALL-E compatible)
  - General Chat
- **OpenAI Compatible**: Works with OpenAI API, Anthropic Claude, local LLMs, or any OpenAI-compatible endpoint
- **Text Integration**: Automatically captures selected text and can replace or insert AI responses
- **System Tray**: Runs minimized in system tray, accessible via hotkey or right-click menu

## Setup

### 1. Prerequisites
- Windows 10/11
- .NET 9.0 Runtime

### 2. Configuration
1. Launch the application
2. Right-click the system tray icon and select "Configuration"
3. Configure the following settings:
   - **Global Hotkey**: Keyboard shortcut to activate (default: Ctrl+Alt+A)
   - **API Base URL**: Your LLM API endpoint (e.g., https://api.openai.com/v1)
   - **API Key**: Your API key for the LLM service
   - **LLM Model**: Model name (e.g., gpt-4, gpt-3.5-turbo, claude-3-opus)

### 3. Usage
1. Select text in any application (optional)
2. Press your configured hotkey (default: `Ctrl+Alt+A`)
3. Choose an operation from the dropdown
4. Configure operation-specific options (language, tone, etc.)
5. Enter your prompt
6. Click "Process"
7. Choose to replace selected text or insert the response

## Supported Operations

### Text Translation
- Translate to: Spanish, French, German, Italian, Portuguese, Japanese, Chinese, Korean, Russian, Arabic
- Automatically replaces selected text with translation

### Text Rewriting
- Tones: Formal, Informal, Professional, Casual, Academic, Creative
- Perfect for improving clarity and style

### Image Generation
- Sizes: 1024x1024, 1024x768, 512x512
- Quality: Standard, HD
- Opens generated images in browser

## Building from Source

```powershell
git clone <repository-url>
cd AIAnywhere
dotnet build
dotnet run
```

## Configuration File

Settings are stored in `config.json` in the application directory:

```json
{
  "Hotkey": "Ctrl+Alt+A",
  "ApiBaseUrl": "https://api.openai.com/v1",
  "ApiKey": "your-api-key-here",
  "LlmModel": "gpt-4"
}
```

## Troubleshooting

### Hotkey Not Working
- Check if another application is using the same hotkey
- Try a different key combination in settings
- Run as administrator if needed

### API Errors
- Verify your API key is correct
- Check if you have sufficient API credits
- Ensure the base URL is correct for your provider

### Text Integration Issues
- Some applications may have protected text fields
- Try running as administrator for better compatibility
- Use copy/paste manually if automatic insertion fails

## Security Notes

- API keys are stored locally in plain text
- Consider using environment variables for sensitive keys
- The application requires clipboard access for text operations

## Contributing

This is an MVP implementation. Potential improvements:
- Encrypted configuration storage
- Plugin system for custom operations
- Batch processing capabilities
- Custom prompt templates
- Conversation history
- Offline mode with local LLMs

## License

[Your license here]

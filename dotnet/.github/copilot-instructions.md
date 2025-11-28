# AI Anywhere - Copilot Instructions

## Project Overview
AI Anywhere is a WPF desktop application built with .NET 10.0 that provides universal AI assistance through global hotkeys. It captures text from any Windows application using advanced UI Automation techniques and processes it through various AI operations (chat, translation, summarization, image generation, text-to-speech, speech-to-text, etc.) using OpenAI-compatible APIs including OpenAI, Anthropic Claude, and local LLMs.

## Architecture & Key Components

### Core Application Structure
- **MainWindow**: System tray integration, global hotkey registration, application lifecycle management
- **PromptWindow**: Main AI interaction UI with operation selection, dynamic option panels, and audio file upload support
- **Services Layer**: Comprehensive service architecture handling AI communication, text capture, configuration, encryption, hotkeys, and image processing
- **Models**: Strongly-typed data structures for configuration, operations, API requests/responses, and custom image sizes
- **Views**: Modal dialog windows (ConfigWindow, ReviewWindow, AboutWindow) with ModernWpfUI styling

### Service Boundaries & Responsibilities

#### Core Services
- **LLMService**: Handles all AI API communication with dual-path approach:
  - OpenAI library for standard endpoints (avoids token parsing issues)
  - HTTP fallback for custom endpoints (handles malformed responses)
  - Supports image generation, text-to-speech, speech-to-text, and text operations
  - Debug logging to `api_debug/` directory with structured JSON analysis

- **ConfigurationService**: JSON-based settings management:
  - Encrypted API keys using portable AES encryption
  - Automatic migration from plain text to encrypted keys
  - System prompts dictionary with per-operation customization
  - Model lists caching with auto-population from APIs

- **TextService**: Multi-layered text capture system:
  - **Primary**: UI Automation selected text capture
  - **Secondary**: Clipboard content fallback
  - **Fallback**: Keyboard simulation (Ctrl+C) for non-UI Automation apps
  - VS Code detection to prevent hanging

#### Supporting Services
- **UIAutomationTextService**: Advanced text capture using Windows UI Automation:
  - Element tree traversal with timeout protection
  - Focus element detection and text pattern extraction
  - VS Code compatibility handling

- **HotkeyService**: Global keyboard shortcut system:
  - Win32 API integration for system-wide hotkeys
  - Key combination parsing and registration
  - Conflict detection and error handling

- **ImageService**: Image processing and clipboard operations:
  - Async image downloading with WPF BitmapImage conversion
  - Clipboard image operations for paste functionality
  - URL-to-clipboard direct copying

- **TextProcessor**: Text normalization and AI response processing:
  - Escaped sequence unescaping (\n, \t, \", etc.)
  - Thinking token removal (<think>...</think>)
  - Whitespace normalization and punctuation cleanup

#### Security & Encryption Services
- **PortableEncryptionService**: Cross-platform AES encryption:
  - Deterministic key derivation for portability
  - Base64 encoding with IV prepending
  - Masked text display for UI security

- **EncryptionService**: Windows DPAPI encryption:
  - Machine-specific encryption for local security
  - Backward compatibility with plain text migration
  - Masked sensitive text display

### Model Layer Architecture

#### Configuration Models
- **Configuration**: Central settings class with INotifyPropertyChanged:
  - Encrypted API key properties with automatic encryption/decryption
  - System prompts dictionary with operation-specific customization
  - Model lists caching with API population
  - Performance settings (text selection, thinking mode, debug logging)

#### Operation Models
- **Operation**: AI operation definitions with dynamic options:
  - System prompt templates with variable substitution
  - Configurable option panels (dropdowns, text inputs, numbers)
  - Operation type enumeration with extensible design

- **OperationOption**: Dynamic UI option configuration:
  - Type-based rendering (Select/Text/Number)
  - Validation rules and default values
  - Required field enforcement

#### API Communication Models
- **LLMRequest/LLMResponse**: Structured API communication:
  - Operation type, prompt, selected text, options dictionary
  - Success/error status, content, image/audio data
  - Generation timing for performance metrics

- **ModelsResponse**: API model discovery structures:
  - OpenAI-compatible model list parsing
  - Model categorization (text/image/audio)
  - ID-based model identification

#### Custom Extensions
- **CustomImageSizes**: Extended image size definitions:
  - Portrait/landscape orientations beyond OpenAI defaults
  - 1:1, 2:3, 3:2, 3:4 aspect ratios
  - Seamless integration with OpenAI API

### View Layer Architecture

#### Main Windows
- **PromptWindow**: Complex AI interaction interface:
  - Dynamic operation selection with ComboBox binding
  - Runtime option panel generation based on operation type
  - Audio file drag-and-drop with validation (25MB limit, format checking)
  - Context menu with full text editing capabilities

- **ReviewWindow**: Result preview and action system:
  - Text display with selectable, editable content
  - Image loading with progress animation and error handling
  - Three-action workflow: Back/Paste/Copy with keyboard shortcuts
  - Image save functionality with format detection

#### Configuration Windows
- **ConfigWindow**: Comprehensive settings interface:
  - Hotkey capture with visual feedback and validation
  - API model fetching with categorization and caching
  - Performance settings with tooltips and explanations
  - Encrypted API key handling with masked display

- **AboutWindow**: Application information and links:
  - Version information from assembly metadata
  - GitHub integration with process launching
  - Copyright year auto-updating

### MainWindow (System Tray)
- **System Tray Integration**: NotifyIcon with context menu
- **Hotkey Management**: Global shortcut registration and processing
- **Application Lifecycle**: Minimize to tray, single instance enforcement

## Critical Developer Workflows

### Building & Running
```bash
# Standard build
dotnet build AIAnywhere.sln

# Optimized release build (GitHub Actions / Build-Release.ps1)
dotnet publish AIAnywhere.sln -c Release -r win-x64 --self-contained false /p:PublishReadyToRun=true

# Run in watch mode
dotnet watch run --project AIAnywhere.sln
```

### Configuration Management
- Settings stored in `config.json` with indented formatting
- API keys automatically encrypted using `PortableEncryptionService`
- System prompts customizable per operation type in `Configuration.SystemPrompts`
- Automatic migration from plain text to encrypted API keys

### Text Capture Priority
1. **UI Automation selected text** (if enabled in config)
2. **Clipboard content** fallback
3. **Empty prompt** if both fail

## Project-Specific Patterns & Conventions

### System Prompt Engineering
All AI operations use structured system prompts with specific rules:
- **Language preservation**: "Use the EXACT same language as the user's input"
- **No interaction**: "NO greetings, introductions, or opening phrases"
- **Template variables**: `{tone}`, `{length}`, `{language}` replaced at runtime
- **Operation-specific formatting**: Email replies include structure requirements

Example from `Configuration.GetDefaultSystemPrompts()`:
```
"LANGUAGE RULE: Always respond in the same language as the user's input text.

TASK: Provide helpful assistance without interaction.

RULES:
1. Use the EXACT same language as the user's input
2. NO greetings, introductions, or opening phrases
...
```

### Operation Definition Pattern
Operations defined in `Operation.GetDefaultOperations()` with:
- **SystemPrompt**: Base prompt with template variables
- **Options**: Configurable parameters (tone, length, size, etc.)
- **Type**: Enum-based operation identification

### API Communication Patterns
- **OpenAI library** for standard endpoints (`api.openai.com`)
- **HTTP fallback** for custom endpoints (avoids token parsing issues)
- **Debug logging** to `api_debug/` directory when enabled
- **Error handling** with structured `LLMResponse` objects

### UI Timing Considerations
- `Dispatcher.BeginInvoke` for post-initialization UI updates
- Window handles captured before showing prompt dialogs
- System tray prevents accidental application closure

### WPF-Specific Patterns
- **Nullable reference types** enabled throughout
- **INotifyPropertyChanged** for data binding
- **Window ownership** for modal dialogs
- **Resource management** with proper cleanup in `OnClosed`

## Integration Points & Dependencies

### External APIs
- **OpenAI-compatible endpoints**: GPT, Claude, local LLMs (Ollama, LM Studio)
- **UI Automation**: `System.Windows.Automation` for text capture
- **Windows Forms**: System tray integration via `NotifyIcon`

### Key Dependencies
- **OpenAI** (2.1.0): Primary API client
- **ModernWpfUI** (0.9.6): Contemporary Windows styling
- **Microsoft.Extensions.***: Configuration and dependency injection
- **Newtonsoft.Json**: JSON processing where needed

## Common Development Tasks

### Adding New AI Operations
1. Add enum value to `OperationType` in `Models/Operation.cs`
2. Define system prompt in `Configuration.GetDefaultSystemPrompts()`
3. Create operation definition in `Operation.GetDefaultOperations()`
4. Add UI handling in `PromptWindow.xaml.cs`
5. Update LLMService processing logic

### Modifying System Prompts
- Edit `Configuration.SystemPrompts` dictionary
- Use template variables for dynamic content
- Maintain language preservation rules
- Test with various input types

### Configuration Changes
- Update `Models/Configuration.cs` for new settings
- Add migration logic in `ConfigurationService` if needed
- Ensure JSON serialization compatibility
- Update UI in `ConfigWindow.xaml`

### Build Optimization
- Use `PublishReadyToRun=true` for performance
- Target `win-x64` runtime
- Self-contained false (requires .NET 10.0 runtime)
- Satellite resource languages limited to English

### Encryption & Security Patterns
- **Portable AES Encryption**: Cross-machine compatible using deterministic key derivation
- **Windows DPAPI Encryption**: Machine-specific for local security with migration support
- **Automatic API Key Migration**: Seamless upgrade from plain text to encrypted storage
- **Masked Display**: Sensitive text display with configurable visible characters
- **Base64 with IV Prepending**: Encrypted data format for portable storage

### UI Automation Patterns
- **Element Tree Traversal**: Depth-first search with timeout protection (2-second limit)
- **Pattern Detection**: TextPattern availability checking before extraction
- **VS Code Compatibility**: Application-specific detection to prevent hanging
- **Focus Element Priority**: Current focus element checked before broader search
- **Cancellation Token Support**: Graceful timeout handling for complex applications

### Drag-and-Drop File Handling
- **Format Validation**: Supported audio formats (MP3, MP4, WAV, M4A, OGG, AAC, FLAC, WMA, WebM)
- **Size Limits**: 25MB maximum file size for OpenAI Whisper API compatibility
- **Visual Feedback**: Background color changes during drag operations
- **Error Handling**: User-friendly messages for unsupported formats or oversized files

### Async Operation Handling
- **Task-Based APIs**: All network operations use async/await pattern
- **Cancellation Support**: Timeout handling for long-running operations
- **Progress Feedback**: Loading animations and status updates for image generation
- **Error Recovery**: Graceful fallback mechanisms for failed operations

## Debugging & Troubleshooting

### Text Capture Issues
- Check `DisableTextSelection` configuration
- Verify UI Automation permissions
- Test clipboard fallback behavior
- Enable debug logging for API calls

### Hotkey Registration Failures
- Check for conflicting global shortcuts
- Verify administrator privileges if needed
- Test with different hotkey combinations
- Check Windows hotkey registration logs

### API Connection Problems
- Verify `ApiBaseUrl` format and accessibility
- Check API key encryption status
- Test with debug logging enabled
- Validate OpenAI-compatible endpoint behavior

### Tooling for shell interactions (install if missing)

- Is it about finding FILES? use `fd`
- Is it about finding TEXT/strings? use `rg`
- Is it about finding CODE STRUCTURE? use `ast-grep`
- Is it about SELECTING from multiple results? pipe to `fzf`
- Is it about interacting with JSON? use `jq`
- Is it about interacting with YAML or XML? use `yq`

## File Organization Reference

### Key Directories
- `Services/`: Business logic and external integrations
- `Models/`: Data structures and configuration
- `Views/`: XAML windows and dialogs
- `AIAnywhere-Optimized/`: Release distribution (generated)

### Important Files
- `MainWindow.xaml.cs`: System tray and hotkey management
- `PromptWindow.xaml.cs`: Main AI interaction UI
- `LLMService.cs`: AI API communication (800+ lines)
- `Configuration.cs`: Settings and system prompts
- `Operation.cs`: AI operation definitions
- `Build-Release.ps1`: Release packaging script</content>
<parameter name="filePath">c:\Users\bernh\Documents\GitHub\AIAnywhere\.github\copilot-instructions.md

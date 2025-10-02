# AI Anywhere - Copilot Instructions

## Project Overview
AI Anywhere is a WPF desktop application that provides universal AI assistance through global hotkeys. It captures text from any Windows application and processes it through various AI operations (chat, translation, summarization, image generation, etc.) using OpenAI-compatible APIs.

## Architecture & Key Components

### Core Application Structure
- **MainWindow**: System tray integration, hotkey registration, application lifecycle
- **PromptWindow**: Main UI for AI interactions, operation selection, and response handling
- **Services Layer**: LLMService (API calls), TextService (text capture), ConfigurationService (settings)
- **Models**: Operation definitions, configuration, request/response structures
- **Views**: Dialog windows (ConfigWindow, ReviewWindow, AboutWindow)

### Service Boundaries
- **LLMService**: Handles all AI API communication, supports multiple providers via OpenAI-compatible endpoints
- **TextService**: UI Automation text capture with clipboard fallback
- **ConfigurationService**: JSON-based settings with encrypted API keys and automatic migration
- **HotkeyService**: Global keyboard shortcut registration and processing

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
- Self-contained false (requires .NET 9.0 runtime)
- Satellite resource languages limited to English

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
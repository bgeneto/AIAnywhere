using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using AIAnywhere.Services;

namespace AIAnywhere.Models
{
    public enum PasteBehavior
    {
        AutoPaste,    // Automatically replace selected text
        ClipboardMode, // Copy to clipboard, user pastes manually
        ReviewMode    // Show preview window for confirmation
    }

    public class Configuration : INotifyPropertyChanged
    {
        private string _hotkey = "Ctrl+Alt+Shift+A";
        private string _apiBaseUrl = "https://api.openai.com/v1";
        private string _apiKey = "";
        private string _llmModel = "gpt-4.1";
        private PasteBehavior _pasteBehavior = PasteBehavior.ReviewMode;
        private bool _enableTextSelection = true;

        public string Hotkey
        {
            get => _hotkey;
            set { _hotkey = value; OnPropertyChanged(nameof(Hotkey)); }
        }

        public string ApiBaseUrl
        {
            get => _apiBaseUrl;
            set { _apiBaseUrl = value; OnPropertyChanged(nameof(ApiBaseUrl)); }
        }

        /// <summary>
        /// Encrypted API key stored in the configuration file
        /// </summary>
        [JsonPropertyName("ApiKey")]
        public string EncryptedApiKey { get; set; } = "";

        /// <summary>
        /// Decrypted API key for use in the application (not serialized)
        /// </summary>
        [JsonIgnore]
        public string ApiKey
        {
            get => string.IsNullOrEmpty(EncryptedApiKey) ? "" : PortableEncryptionService.Decrypt(EncryptedApiKey) ?? "";
            set
            {
                _apiKey = value ?? "";
                EncryptedApiKey = string.IsNullOrEmpty(value) ? "" : PortableEncryptionService.Encrypt(value) ?? "";
                OnPropertyChanged(nameof(ApiKey));
            }
        }

        public string LlmModel
        {
            get => _llmModel;
            set { _llmModel = value; OnPropertyChanged(nameof(LlmModel)); }
        }

        public PasteBehavior PasteBehavior
        {
            get => _pasteBehavior;
            set { _pasteBehavior = value; OnPropertyChanged(nameof(PasteBehavior)); }
        }

        /// <summary>
        /// Enable automatic text selection and clipboard detection for prompt prefilling.
        /// When disabled, the prompt window opens faster but without automatic text prefilling.
        /// </summary>
        public bool EnableTextSelection
        {
            get => _enableTextSelection;
            set { _enableTextSelection = value; OnPropertyChanged(nameof(EnableTextSelection)); }
        }

        /// <summary>
        /// Custom system prompts for each operation type. Advanced users can modify these in the config.json file.
        /// </summary>
        public Dictionary<string, string> SystemPrompts { get; set; } = GetDefaultSystemPrompts();

        /// <summary>
        /// Get default system prompts for all operation types
        /// </summary>
        public static Dictionary<string, string> GetDefaultSystemPrompts()
        {
            return new Dictionary<string, string>
            {
                [nameof(OperationType.GeneralChat)] =
                    "You are operating in a non-interactive mode.\n " +
                    "Do NOT use introductory phrases, greetings, or opening messages.\n " +
                    "You CANNOT ask the user for clarification, additional details, or preferences.\n " +
                    "When given a request, make reasonable assumptions based on the context " +
                    "and provide a complete, helpful response immediately.\n " +
                    "If a request is ambiguous, choose the most common or logical interpretation " +
                    "and proceed accordingly.\n " +
                    "Always deliver a substantive response rather than asking questions.\n " +
                    "NEVER ask the user for follow-up questions or clarifications.",
                [nameof(OperationType.ImageGeneration)] =
                    "Generate an image with that follows the description.",
                [nameof(OperationType.TextTranslation)] =
                    "You are a professional translator.\n " +
                    "Translate the provided text to {language} following the original writting style.\n " +
                    "You are operating in a non-interactive mode. " +
                    "Return only the translated text without any explanations or introdutory texts.",
                [nameof(OperationType.TextRewrite)] =
                    "You are a professional editor and copywriter. Your task is to rewrite the user-supplied text so that it:\n " +
                    "- Reads like native-level text in its corresponding language\n " +
                    "- Is grammatically correct and idiomatic\n " +
                    "- Retains the original meaning and follows a {tone} tone\n " +
                    "- Maintain the original meaning but improve clarity and flow.\n " +
                    "# Instructions:\n " +
                    "- Do not add new ideas, only restate what's already there.\n " +
                    "- Try to keep the length roughly the same (±20%).\n " +
                    "- Use natural phrasing and transitions.\n " +
                    "- You are operating in a non-interactive mode, so deliver ONLY the rewritten text, NO commentary or changes explanations, JUST the rewriten text."
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
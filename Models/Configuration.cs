using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;
using AIAnywhere.Services;

namespace AIAnywhere.Models
{
    public enum PasteBehavior
    {
        AutoPaste, // Automatically replace selected text
        ClipboardMode, // Copy to clipboard, user pastes manually
        ReviewMode, // Show preview window for confirmation
    }

    public class Configuration : INotifyPropertyChanged
    {
        private string _hotkey = "Ctrl+Alt+Shift+A";
        private string _apiBaseUrl = "https://api.openai.com/v1";
        private string _apiKey = "";
        private string _llmModel = "";
        private PasteBehavior _pasteBehavior = PasteBehavior.ReviewMode;
        private bool _enableTextSelection = true;

        public string Hotkey
        {
            get => _hotkey;
            set
            {
                _hotkey = value;
                OnPropertyChanged(nameof(Hotkey));
            }
        }

        public string ApiBaseUrl
        {
            get => _apiBaseUrl;
            set
            {
                _apiBaseUrl = value;
                OnPropertyChanged(nameof(ApiBaseUrl));
            }
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
            get =>
                string.IsNullOrEmpty(EncryptedApiKey)
                    ? ""
                    : PortableEncryptionService.Decrypt(EncryptedApiKey) ?? "";
            set
            {
                _apiKey = value ?? "";
                EncryptedApiKey = string.IsNullOrEmpty(value)
                    ? ""
                    : PortableEncryptionService.Encrypt(value) ?? "";
                OnPropertyChanged(nameof(ApiKey));
            }
        }

        public string LlmModel
        {
            get => _llmModel;
            set
            {
                _llmModel = value;
                OnPropertyChanged(nameof(LlmModel));
            }
        }

        public PasteBehavior PasteBehavior
        {
            get => _pasteBehavior;
            set
            {
                _pasteBehavior = value;
                OnPropertyChanged(nameof(PasteBehavior));
            }
        }

        /// <summary>
        /// Enable automatic text selection and clipboard detection for prompt prefilling.
        /// When disabled, the prompt window opens faster but without automatic text prefilling.
        /// </summary>
        public bool EnableTextSelection
        {
            get => _enableTextSelection;
            set
            {
                _enableTextSelection = value;
                OnPropertyChanged(nameof(EnableTextSelection));
            }
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
                    "LANGUAGE RULE: Always respond in the same language as the user's input text.\n\n"
                    + "TASK: Provide helpful assistance without interaction.\n\n"
                    + "RULES:\n"
                    + "1. Use the EXACT same language as the user's input\n"
                    + "2. NO greetings, introductions, or opening phrases\n"
                    + "3. NO questions or requests for clarification\n"
                    + "4. Make reasonable assumptions and respond immediately\n"
                    + "5. Choose the most logical interpretation if unclear\n"
                    + "6. Provide complete, substantive answers\n\n"
                    + "Start your response directly with the helpful content.\n",
                [nameof(OperationType.ImageGeneration)] =
                    "LANGUAGE RULE: Use the same language as the user's description for any text in the image.\n\n"
                    + "TASK: Generate an image based on the user's description.\n\n"
                    + "RULES:\n"
                    + "1. Create exactly what is described\n"
                    + "2. If text appears in the image, use the same language as the input\n"
                    + "3. Follow the description precisely\n"
                    + "4. Make the image high quality and detailed\n\n"
                    + "Generate the image now.\n",
                [nameof(OperationType.TextTranslation)] =
                    "CRITICAL: You are translating TO {language}. The output must be in {language} only.\n\n"
                    + "TASK: Translate the provided text to {language}.\n\n"
                    + "TRANSLATION RULES:\n"
                    + "1. Output language: {language} ONLY\n"
                    + "2. Keep the original writing style and tone\n"
                    + "3. Maintain the same formality level\n"
                    + "4. Preserve the original meaning exactly\n"
                    + "5. NO explanations or comments\n"
                    + "6. Return ONLY the translated text\n\n"
                    + "Translate this text to {language}:\n",
                [nameof(OperationType.TextRewrite)] =
                    "LANGUAGE RULE: Keep the EXACT same language as the original text.\n\n"
                    + "TASK: Rewrite text to improve quality while maintaining {tone} tone.\n\n"
                    + "REWRITING RULES:\n"
                    + "1. Use the SAME language as the input text\n"
                    + "2. Apply {tone} tone consistently\n"
                    + "3. Fix grammar, spelling, and punctuation errors\n"
                    + "4. Improve clarity and flow\n"
                    + "5. Keep the same meaning - NO new ideas\n"
                    + "6. Maintain similar length (±20%)\n"
                    + "7. Use natural, native-level phrasing\n"
                    + "8. NO explanations or comments\n\n"
                    + "Return ONLY the rewritten text:\n",
                [nameof(OperationType.TextSummarization)] =
                    "LANGUAGE RULE: Use the EXACT same language as the original text.\n\n"
                    + "TASK: Create a {length} summary in {format} format.\n\n"
                    + "SUMMARY RULES:\n"
                    + "1. Use the SAME language as the input text\n"
                    + "2. Length: {length} (BRIEF=2-3 sentences, MEDIUM=1 paragraph, DETAILED=2-3 paragraphs)\n"
                    + "3. Format: {format} (PARAGRAPH=flowing text, BULLET POINTS=clear bullets, EXECUTIVE SUMMARY=overview+findings, KEY TAKEAWAYS=main insights)\n"
                    + "4. Keep core message and critical details\n"
                    + "5. Use clear, professional language\n"
                    + "6. Focus on facts and actionable items\n"
                    + "7. NO explanations or meta-commentary\n\n"
                    + "Create the {length} {format} summary:\n",
                [nameof(OperationType.EmailEnhancement)] =
                    "LANGUAGE RULE: Write your reply in the EXACT same language as the original email.\n\n"
                    + "TASK: Generate an email reply with {tone} tone and {length} length.\n\n"
                    + "EMAIL REPLY RULES:\n"
                    + "1. Use the SAME language as the original email\n"
                    + "2. Apply {tone} tone: PROFESSIONAL=business-appropriate, FRIENDLY=warm but professional, FORMAL=traditional business, URGENT=time-sensitive, APOLOGETIC=acknowledges issues, ENTHUSIASTIC=positive energy\n"
                    + "3. Length: {length} (BRIEF=2-4 sentences, STANDARD=1-2 paragraphs, DETAILED=2-3 paragraphs)\n"
                    + "4. Structure: Greeting → Acknowledge original → Address key points → Next steps → Professional closing + [Your Name]\n"
                    + "5. Address ALL questions from the original email\n"
                    + "6. Match the formality level of the original\n"
                    + "7. NO subject line (replies keep original subject)\n"
                    + "8. NO explanations or meta-commentary\n\n"
                    + "Write a proper reply for this email message:\n",
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

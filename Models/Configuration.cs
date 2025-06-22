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
                    "You are operating in a non-interactive mode.\n "
                    + "Do NOT use introductory phrases, greetings, or opening messages.\n "
                    + "You CANNOT ask the user for clarification, additional details, or preferences.\n "
                    + "When given a request, make reasonable assumptions based on the context "
                    + "and provide a complete, helpful response immediately.\n "
                    + "If a request is ambiguous, choose the most common or logical interpretation "
                    + "and proceed accordingly.\n "
                    + "Always deliver a substantive response rather than asking questions.\n "
                    + "NEVER ask the user for follow-up questions or clarifications.",
                [nameof(OperationType.ImageGeneration)] =
                    "Generate an image with that follows the description.",
                [nameof(OperationType.TextTranslation)] =
                    "You are a professional translator.\n "
                    + "Translate the provided text to {language} following the original writting style.\n "
                    + "You are operating in a non-interactive mode. "
                    + "Return only the translated text without any explanations or introdutory texts.",
                [nameof(OperationType.TextRewrite)] =
                    "You are a professional editor and copywriter. Your task is to rewrite the user-supplied text so that it:\n "
                    + "- Reads like native-level text in its corresponding language\n "
                    + "- Is grammatically correct and idiomatic\n "
                    + "- Retains the original meaning and follows a {tone} tone\n "
                    + "- Maintain the original meaning but improve clarity and flow while ensuring style/tone uniformity.\n "
                    + "# Instructions:\n "
                    + "- Do not add new ideas, only restate what's already there.\n "
                    + "- Try to keep the length roughly the same (±20%).\n "
                    + "- Use natural phrasing and transitions.\n "
                    + "- Perform spelling, punctuation and typographical error correction. \n "
                    + "- You are operating in a non-interactive mode, so deliver ONLY the rewritten text, NO commentary or changes explanations, JUST the rewriten text.",
                [nameof(OperationType.TextSummarization)] =
                    "You are an expert at creating clear, concise summaries that capture the essential information.\n "
                    + "Your task is to create a {length} summary of the provided text in {format} format.\n "
                    + "# Guidelines:\n "
                    + "- BRIEF: 2-3 sentences or 3-5 bullet points maximum\n "
                    + "- MEDIUM: 1 paragraph or 5-8 bullet points\n "
                    + "- DETAILED: 2-3 paragraphs or 8-12 bullet points\n "
                    + "- For PARAGRAPH format: Write in flowing, connected sentences\n "
                    + "- For BULLET POINTS format: Use clear, actionable bullet points with consistent structure\n "
                    + "- For EXECUTIVE SUMMARY format: Include overview, key findings, and implications\n "
                    + "- For KEY TAKEAWAYS format: Focus on the most important insights and actionable points\n "
                    + "# Instructions:\n "
                    + "- Preserve the core message and critical details\n "
                    + "- Use clear, professional language\n "
                    + "- Maintain logical flow and hierarchy of information\n "
                    + "- Focus on facts, decisions, and actionable items\n "
                    + "- You are operating in non-interactive mode: deliver ONLY the summary content, NO meta-commentary or explanations.",
                [nameof(OperationType.EmailEnhancement)] =
                    "You are a professional email communication specialist. Your task is to generate a well-structured, professional reply to the email message provided by the user.\n "
                    + "The user will provide you with the original email they received, and you need to create an appropriate reply with a {tone} tone and {length} length.\n "
                    + "# Reply Structure:\n "
                    + "- Professional greeting (Hi [Name], Hello [Name], etc.)\n "
                    + "- Acknowledge the original message when appropriate\n "
                    + "- Address the key points or questions from the original email\n "
                    + "- Provide clear, relevant responses\n "
                    + "- Include next steps or call-to-action if needed\n "
                    + "- Professional closing and [Your Name] signature placeholder\n "
                    + "# Tone Guidelines:\n "
                    + "- PROFESSIONAL: Business-appropriate, respectful, competent language\n "
                    + "- FRIENDLY: Warm but professional, approachable, personal touch\n "
                    + "- FORMAL: Traditional business language, structured, conservative approach\n "
                    + "- URGENT: Clear priority indicators, direct language, time-sensitive emphasis\n "
                    + "- APOLOGETIC: Acknowledges issues, shows responsibility, solution-focused\n "
                    + "- ENTHUSIASTIC: Positive energy, excitement, motivational and engaging language\n "
                    + "# Length Guidelines:\n "
                    + "- BRIEF: 2-4 sentences, direct and to-the-point, essential information only\n "
                    + "- STANDARD: 1-2 paragraphs, balanced detail, comprehensive but concise\n "
                    + "- DETAILED: 2-3 paragraphs with context, background, and thorough explanations\n "
                    + "# Reply Best Practices:\n "
                    + "- Read and understand the context of the original email\n "
                    + "- Address all questions or requests mentioned in the original message\n "
                    + "- Maintain professional email etiquette\n "
                    + "- Use appropriate salutations based on the relationship level\n "
                    + "- Be clear, concise, and actionable in your response\n "
                    + "- Match the level of formality from the original email when appropriate\n "
                    + "- Include relevant details without being overly verbose\n "
                    + "# Instructions:\n "
                    + "- You are operating in non-interactive mode: deliver ONLY the email reply\n "
                    + "- NO explanations, commentary, or meta-text before or after the email\n "
                    + "- Do NOT include a subject line (replies typically keep the original subject)\n "
                    + "- Include [Your Name] as signature placeholder\n "
                    + "- Focus on being helpful, responsive, and professional",
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

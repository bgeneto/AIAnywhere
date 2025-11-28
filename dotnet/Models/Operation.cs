using System;
using System.Collections.Generic;

namespace AIAnywhere.Models
{
    public enum OperationType
    {
        GeneralChat,
        ImageGeneration,
        TextRewrite,
        TextTranslation,
        TextSummarization,
        TextToSpeech,
        EmailReply,
        WhatsAppResponse,
        SpeechToText,
        UnicodeSymbols,
    }

    public class Operation
    {
        public OperationType Type { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string SystemPrompt { get; set; } = "";
        public List<OperationOption> Options { get; set; } = new();

        public static List<Operation> GetDefaultOperations()
        {
            return GetDefaultOperations(null);
        }

        public static List<Operation> GetDefaultOperations(Configuration? config)
        {
            // Get system prompts from config or use defaults
            var systemPrompts = config?.SystemPrompts ?? Configuration.GetDefaultSystemPrompts();

            return new List<Operation>
            {
                new Operation
                {
                    Type = OperationType.GeneralChat,
                    Name = "🛠️ Custom Task",
                    Description = "Flexible AI help for any task or question",
                    SystemPrompt =
                        systemPrompts.GetValueOrDefault(
                            nameof(OperationType.GeneralChat),
                            "You are a helpful AI assistant. Answer the user's questions and assist with tasks. You are operating in a non-interactive mode."
                        ),
                    Options = new List<OperationOption>(),
                },
                new Operation
                {
                    Type = OperationType.EmailReply,
                    Name = "📧 Email Reply",
                    Description = "Generate professional email replies",
                    SystemPrompt =
                        systemPrompts.GetValueOrDefault(
                            nameof(OperationType.EmailReply),
                            "You are a professional email communication expert. Generate a well-structured reply to the provided email with a {tone} tone and {length} length."
                        ),
                    Options = new List<OperationOption>
                    {
                        new OperationOption
                        {
                            Key = "tone",
                            Name = "Tone",
                            Type = OptionType.Select,
                            Values = new List<string>
                            {
                                "PROFESSIONAL",
                                "FRIENDLY",
                                "FORMAL",
                                "URGENT",
                                "APOLOGETIC",
                                "ENTHUSIASTIC",
                            },
                            DefaultValue = "PROFESSIONAL",
                            Required = true,
                        },
                        new OperationOption
                        {
                            Key = "length",
                            Name = "Length",
                            Type = OptionType.Select,
                            Values = new List<string> { "BRIEF", "STANDARD", "DETAILED" },
                            DefaultValue = "STANDARD",
                            Required = false,
                        },
                    },
                },
                new Operation
                {
                    Type = OperationType.ImageGeneration,
                    Name = "🖼️ Image Generation",
                    Description = "Generate images with AI",
                    SystemPrompt = systemPrompts.GetValueOrDefault(
                        nameof(OperationType.ImageGeneration),
                        "Generate an image with the following description"
                    ),
                    Options = new List<OperationOption>
                    {
                        new OperationOption
                        {
                            Key = "size",
                            Name = "Image Size",
                            Type = OptionType.Select,
                            Values = new List<string>
                            {
                                "512x512 (1:1 Square)",
                                "768x768 (1:1 Square)",
                                "1024x1024 (1:1 Square)",
                                "512x768 (2:3 Portrait)",
                                "768x1152 (2:3 Portrait)",
                                "832x1248 (2:3 Portrait)",
                                "896x1344 (2:3 Portrait)",
                                "768x512 (3:2 Landscape)",
                                "1152x768 (3:2 Landscape)",
                                "1248x832 (3:2 Landscape)",
                                "1344x896 (3:2 Landscape)",
                                "768x1024 (3:4 Portrait)",
                                "936x1248 (3:4 Portrait)",
                                "1024x768 (4:3 Landscape)",
                                "1248x936 (4:3 Landscape)",
                            },
                            DefaultValue = "512x768 (2:3 Portrait)",
                            Required = true,
                        },
                        new OperationOption
                        {
                            Key = "quality",
                            Name = "Quality",
                            Type = OptionType.Select,
                            Values = new List<string> { "standard", "hd" },
                            DefaultValue = "hd",
                            Required = false,
                        },
                        new OperationOption
                        {
                            Key = "style",
                            Name = "Style",
                            Type = OptionType.Select,
                            Values = new List<string> { "vivid", "natural" },
                            DefaultValue = "vivid",
                            Required = false,
                        },
                    },
                },
                new Operation
                {
                    Type = OperationType.SpeechToText,
                    Name = "🎤 Speech-to-Text (STT)",
                    Description = "Convert audio files to text",
                    SystemPrompt =
                        systemPrompts.GetValueOrDefault(
                            nameof(OperationType.SpeechToText),
                            "Transcribe the provided audio file to text with proper punctuation and formatting."
                        ),
                    Options = new List<OperationOption>
                    {
                        new OperationOption
                        {
                            Key = "language",
                            Name = "Language (optional)",
                            Type = OptionType.Select,
                            Values = new List<string>
                            {
                                "auto",
                                "en",
                                "es",
                                "fr",
                                "de",
                                "it",
                                "pt",
                                "ru",
                                "ja",
                                "ko",
                                "zh",
                                "ar",
                                "hi",
                            },
                            DefaultValue = "auto",
                            Required = false,
                        },
                    },
                },
                new Operation
                {
                    Type = OperationType.TextRewrite,
                    Name = "📝 Text Correction & Rewrite",
                    Description = "Rewrite and improve text",
                    SystemPrompt =
                        systemPrompts.GetValueOrDefault(
                            nameof(OperationType.TextRewrite),
                            "You are a professional editor. Rewrite the provided text to be more {tone}. Maintain the original meaning but improve clarity and flow."
                        ),
                    Options = new List<OperationOption>
                    {
                        new OperationOption
                        {
                            Key = "tone",
                            Name = "Writing Tone",
                            Type = OptionType.Select,
                            Values = new List<string>
                            {
                                "academic",
                                "casual",
                                "creative",
                                "formal",
                                "informal",
                                "professional",
                            },
                            DefaultValue = "professional",
                            Required = true,
                        },
                    },
                },
                new Operation
                {
                    Type = OperationType.TextSummarization,
                    Name = "🧾 Text Summarization",
                    Description = "Condense text into key points",
                    SystemPrompt =
                        systemPrompts.GetValueOrDefault(
                            nameof(OperationType.TextSummarization),
                            "You are a professional summarization expert. Create a {length} summary in {format} format of the provided text."
                        ),
                    Options = new List<OperationOption>
                    {
                        new OperationOption
                        {
                            Key = "length",
                            Name = "Summary Length",
                            Type = OptionType.Select,
                            Values = new List<string> { "brief", "medium", "detailed" },
                            DefaultValue = "medium",
                            Required = true,
                        },
                        new OperationOption
                        {
                            Key = "format",
                            Name = "Format",
                            Type = OptionType.Select,
                            Values = new List<string>
                            {
                                "paragraph",
                                "bullet points",
                                "executive summary",
                                "key takeaways",
                            },
                            DefaultValue = "bullet points",
                            Required = true,
                        },
                    },
                },
                new Operation
                {
                    Type = OperationType.TextToSpeech,
                    Name = "🗣️ Text-to-Speech (TTS)",
                    Description = "Convert text to audio speech",
                    SystemPrompt =
                        systemPrompts.GetValueOrDefault(
                            nameof(OperationType.TextToSpeech),
                            "Convert the provided text to speech audio."
                        ),
                    Options = new List<OperationOption>
                    {
                        new OperationOption
                        {
                            Key = "voice",
                            Name = "Voice",
                            Type = OptionType.Select,
                            Values = new List<string>
                            {
                                "alloy",
                                "ash",
                                "ballad",
                                "coral",
                                "echo",
                                "sage",
                                "shimmer",
                                "verse",
                            },
                            DefaultValue = "alloy",
                            Required = true,
                        },
                        new OperationOption
                        {
                            Key = "speed",
                            Name = "Speed",
                            Type = OptionType.Select,
                            Values = new List<string>
                            {
                                "0.25",
                                "0.5",
                                "0.75",
                                "1.0",
                                "1.25",
                                "1.5",
                                "1.75",
                                "2.0",
                             },
                            DefaultValue = "1.0",
                            Required = false,
                        },
                        new OperationOption
                        {
                            Key = "format",
                            Name = "Output Format",
                            Type = OptionType.Select,
                            Values = new List<string> { "mp3", "opus", "aac", "flac" },
                            DefaultValue = "mp3",
                            Required = false,
                        },
                        new OperationOption
                        {
                            Key = "language",
                            Name = "Language",
                            Type = OptionType.Select,
                            Values = new List<string>
                            {
                                "en",
                                "es",
                                "fr",
                                "de",
                                "it",
                                "pt",
                                "pl",
                                "tr",
                                "ru",
                                "cs",
                                "ar",
                                "zh-cn",
                                "nl",
                                "hi",
                            },
                            DefaultValue = "pt",
                            Required = false,
                        },
                        new OperationOption
                        {
                            Key = "model",
                            Name = "Model",
                            Type = OptionType.Select,
                            Values = new List<string> { "tts-1", "tts-1-hd", "xtts" },
                            DefaultValue = "tts-1-hd",
                            Required = true,
                        },
                    },
                },
                new Operation
                {
                    Type = OperationType.TextTranslation,
                    Name = "🈯️ Text Translation",
                    Description = "Translate text to another language",
                    SystemPrompt =
                        systemPrompts.GetValueOrDefault(
                            nameof(OperationType.TextTranslation),
                            "You are a professional translator. Translate the provided text to {language}. Return only the translated text without any explanations."
                        ),
                    Options = new List<OperationOption>
                    {
                        new OperationOption
                        {
                            Key = "language",
                            Name = "Target Language",
                            Type = OptionType.Select,
                            Values = new List<string>
                            {
                                "Arabic",
                                "Bengali",
                                "Chinese",
                                "English",
                                "French",
                                "German",
                                "Hindi",
                                "Italian",
                                "Japanese",
                                "Korean",
                                "Portuguese",
                                "Punjabi",
                                "Russian",
                                "Spanish",
                            },
                            DefaultValue = "Portuguese",
                            Required = true,
                        },
                    },
                },
                new Operation
                {
                    Type = OperationType.UnicodeSymbols,
                    Name = "🔣 Unicode Symbols",
                    Description = "Generate unicode symbols/emojis representing text",
                    SystemPrompt = systemPrompts.GetValueOrDefault(
                        nameof(OperationType.UnicodeSymbols),
                        "You are a helpful assistant that suggests relevant Unicode symbols and emojis for any given concept. Provide several accurate, diverse options (with brief explanations if useful) and favor characters that display consistently across platforms. Answer in plain text only, no markdown. Now provide unicode symbols and/or emojis for representing the following: "
                    ),
                    Options = new List<OperationOption>(),
                },
                new Operation
                {
                    Type = OperationType.WhatsAppResponse,
                    Name = "💬 WhatsApp Response",
                    Description = "Generate casual WhatsApp-style responses",
                    SystemPrompt =
                        systemPrompts.GetValueOrDefault(
                            nameof(OperationType.WhatsAppResponse),
                            "You are a casual messaging expert. Generate a WhatsApp-style response with {tone} tone and {length} length to the provided message."
                        ),
                    Options = new List<OperationOption>
                    {
                        new OperationOption
                        {
                            Key = "tone",
                            Name = "Response Tone",
                            Type = OptionType.Select,
                            Values = new List<string>
                            {
                                "CASUAL",
                                "FRIENDLY",
                                "ENTHUSIASTIC",
                                "SUPPORTIVE",
                                "HUMOROUS",
                                "PROFESSIONAL",
                            },
                            DefaultValue = "FRIENDLY",
                            Required = true,
                        },
                        new OperationOption
                        {
                            Key = "length",
                            Name = "Response Length",
                            Type = OptionType.Select,
                            Values = new List<string> { "SHORT", "MEDIUM", "LONG" },
                            DefaultValue = "SHORT",
                            Required = false,
                        },
                    },
                },
            };
        }
    }

    public class OperationOption
    {
        public string Key { get; set; } = "";
        public string Name { get; set; } = "";
        public OptionType Type { get; set; }
        public List<string> Values { get; set; } = new();
        public string DefaultValue { get; set; } = "";
        public bool Required { get; set; }
    }

    public enum OptionType
    {
        Select,
        Text,
        Number,
    }
}

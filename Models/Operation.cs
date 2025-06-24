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
        EmailEnhancement,
        WhatsAppResponse,
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
                    Name = "Custom Task",
                    Description = "Flexible AI help for any task or question",
                    SystemPrompt =
                        "/no_think\n"
                        + systemPrompts.GetValueOrDefault(
                            nameof(OperationType.GeneralChat),
                            "You are a helpful AI assistant. Answer the user's questions and assist with tasks. You are operating in a non-interactive mode."
                        ),
                    Options = new List<OperationOption>(),
                },
                new Operation
                {
                    Type = OperationType.EmailEnhancement,
                    Name = "Email Reply",
                    Description = "Generate professional email replies",
                    SystemPrompt =
                        "/no_think\n"
                        + systemPrompts.GetValueOrDefault(
                            nameof(OperationType.EmailEnhancement),
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
                    Name = "Image Generation",
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
                            Values = new List<string> { "1024x1024", "1024x768", "512x512" },
                            DefaultValue = "1024x768",
                            Required = true,
                        },
                        new OperationOption
                        {
                            Key = "quality",
                            Name = "Quality",
                            Type = OptionType.Select,
                            Values = new List<string> { "low", "standard" },
                            DefaultValue = "standard",
                            Required = false,
                        },
                    },
                },
                new Operation
                {
                    Type = OperationType.TextRewrite,
                    Name = "Text Rewrite",
                    Description = "Rewrite and improve text",
                    SystemPrompt =
                        "/no_think\n"
                        + systemPrompts.GetValueOrDefault(
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
                    Name = "Text Summarization",
                    Description = "Condense text into key points",
                    SystemPrompt =
                        "/no_think\n"
                        + systemPrompts.GetValueOrDefault(
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
                    Type = OperationType.TextTranslation,
                    Name = "Text Translation",
                    Description = "Translate text to another language",
                    SystemPrompt =
                        "/no_think\n"
                        + systemPrompts.GetValueOrDefault(
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
                    Type = OperationType.WhatsAppResponse,
                    Name = "WhatsApp Response",
                    Description = "Generate casual WhatsApp-style responses",
                    SystemPrompt =
                        "/no_think\n"
                        + systemPrompts.GetValueOrDefault(
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

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
    }

    public class Operation
    {
        public OperationType Type { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string SystemPrompt { get; set; } = "";
        public List<OperationOption> Options { get; set; } = new(); public static List<Operation> GetDefaultOperations()
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
                    Name = "General Chat",
                    Description = "General AI assistance",
                    SystemPrompt = systemPrompts.GetValueOrDefault(nameof(OperationType.GeneralChat),
                        "You are operating in a non-interactive mode.\n" +
                        "Do NOT use introductory phrases, greetings, or opening messages.\n" +
                        "You CANNOT ask the user for clarification, additional details, or preferences.\n" +
                        "When given a request, make reasonable assumptions based on the context and provide a complete, helpful response immediately.\n" +
                        "If a request is ambiguous, choose the most common or logical interpretation and proceed accordingly.\n" +
                        "Always deliver a substantive response rather than asking questions.\n" +
                        "NEVER ask the user for follow-up questions or clarifications."),
                    Options = new List<OperationOption>()
                },
                new Operation
                {
                    Type = OperationType.ImageGeneration,
                    Name = "Image Generation",
                    Description = "Generate images with AI",
                    SystemPrompt = systemPrompts.GetValueOrDefault(nameof(OperationType.ImageGeneration),
                        "Generate an image with the following description"),
                    Options = new List<OperationOption>
                    {
                        new OperationOption
                        {
                            Key = "size",
                            Name = "Image Size",
                            Type = OptionType.Select,
                            Values = new List<string> { "1024x1024", "1024x768", "512x512" },
                            DefaultValue = "1024x768",
                            Required = true
                        },
                        new OperationOption
                        {
                            Key = "quality",
                            Name = "Quality",
                            Type = OptionType.Select,
                            Values = new List<string> { "low", "standard" },
                            DefaultValue = "standard",
                            Required = false
                        }
                    }
                },
                new Operation
                {
                    Type = OperationType.TextRewrite,
                    Name = "Text Rewrite",
                    Description = "Rewrite and improve text",
                    SystemPrompt = systemPrompts.GetValueOrDefault(nameof(OperationType.TextRewrite),
                        "You are a professional editor. Rewrite the provided text to be more {tone}. Maintain the original meaning but improve clarity and flow."),
                    Options = new List<OperationOption>
                    {
                        new OperationOption
                        {
                            Key = "tone",
                            Name = "Writing Tone",
                            Type = OptionType.Select,
                            Values = new List<string> { "formal", "informal", "professional", "casual", "academic", "creative" },
                            DefaultValue = "professional",
                            Required = true
                        }
                    }
                },
                new Operation
                {
                    Type = OperationType.TextTranslation,
                    Name = "Text Translation",
                    Description = "Translate text to another language",
                    SystemPrompt = systemPrompts.GetValueOrDefault(nameof(OperationType.TextTranslation),
                        "You are a professional translator. Translate the provided text to {language}. Return only the translated text without any explanations."),
                    Options = new List<OperationOption>
                    {
                        new OperationOption
                        {
                            Key = "language",
                            Name = "Target Language",
                            Type = OptionType.Select,
                            Values = new List<string> { "Arabic", "Bengali", "Chinese", "English", "French", "German", "Hindi", "Italian", "Japanese", "Korean", "Portuguese", "Punjabi", "Russian", "Spanish" },
                            DefaultValue = "Portuguese",
                            Required = true
                        }
                    }
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
        Number
    }
}

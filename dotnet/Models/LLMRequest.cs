using System;
using System.Collections.Generic;

namespace AIAnywhere.Models
{
    public class LLMRequest
    {
        public OperationType OperationType { get; set; }
        public string Prompt { get; set; } = "";
        public string SelectedText { get; set; } = "";
        public Dictionary<string, string> Options { get; set; } = new();
        public string? AudioFilePath { get; set; }
    }

    public class LLMResponse
    {
        public bool Success { get; set; }
        public string Content { get; set; } = "";
        public string Error { get; set; } = "";
        public bool IsImage { get; set; } = false;
        public string? ImageUrl { get; set; }
        public bool IsAudio { get; set; } = false;
        public byte[]? AudioData { get; set; }
        public string? AudioFormat { get; set; }
        public DateTime? GenerationStartTime { get; set; }
    }
}

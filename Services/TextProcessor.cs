using System;
using System.Text.RegularExpressions;

namespace AIAnywhere.Services
{
    public static class TextProcessor
    {
        public static string NormalizeText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // Handle the most common escaped sequences from LLM responses
            // Based on your example: "Olá [Nome],\\n\\nEspero..."
            text = text.Replace("\\n", "\n"); // \n becomes actual newline
            text = text.Replace("\\r\\n", "\n"); // \r\n becomes newline
            text = text.Replace("\\r", "\n"); // \r becomes newline
            text = text.Replace("\\t", "\t"); // \t becomes actual tab
            text = text.Replace("\\\"", "\""); // \" becomes actual quote
            text = text.Replace("\\'", "'"); // \' becomes actual apostrophe

            // Clean up excessive whitespace
            text = Regex.Replace(text, @"\n\s*\n\s*\n", "\n\n");
            text = Regex.Replace(text, @"[ \t]+", " ");

            return text.Trim();
        }

        public static string FormatForDisplay(string text)
        {
            text = NormalizeText(text);

            // Additional formatting for display
            text = text.Replace("\n", Environment.NewLine);

            return text;
        }

        public static string FormatForClipboard(string text)
        {
            return NormalizeText(text);
        }

        public static string FormatForAutoPaste(string text)
        {
            return NormalizeText(text);
        }

        public static string ProcessLLMResponse(string rawResponse)
        {
            if (string.IsNullOrEmpty(rawResponse))
                return rawResponse;

            // Apply text processing - simplified to just normalize
            return NormalizeText(rawResponse);
        }

        // Debug method to see what's actually in the text
        public static string DebugText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "Empty or null";

            var debugInfo = $"Length: {text.Length}\nFirst 100 chars with escapes visible:\n";

            for (int i = 0; i < Math.Min(text.Length, 100); i++)
            {
                char c = text[i];
                if (c == '\n')
                    debugInfo += "[LF]";
                else if (c == '\r')
                    debugInfo += "[CR]";
                else if (c == '\t')
                    debugInfo += "[TAB]";
                else if (c == '\\')
                    debugInfo += "[\\]";
                else
                    debugInfo += c;
            }

            return debugInfo;
        }
    }
}

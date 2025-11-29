//! Text processing module for AI Anywhere
//! Handles text normalization and LLM response processing

use regex::Regex;

/// Process LLM response text
/// - Removes thinking tokens
/// - Unescapes special characters
/// - Normalizes whitespace
pub fn process_llm_response(text: &str) -> String {
    let mut result = text.to_string();
    
    // Remove thinking tokens (<think>...</think>)
    result = remove_thinking_tokens(&result);
    
    // Unescape common escape sequences
    result = unescape_text(&result);
    
    // Normalize whitespace
    result = normalize_whitespace(&result);
    
    result.trim().to_string()
}

/// Remove thinking tokens from text
fn remove_thinking_tokens(text: &str) -> String {
    // Pattern to match <think>...</think> blocks (case-insensitive, multiline)
    let re = Regex::new(r"(?is)<think>.*?</think>").unwrap();
    re.replace_all(text, "").to_string()
}

/// Unescape common escape sequences in text
fn unescape_text(text: &str) -> String {
    text.replace("\\n", "\n")
        .replace("\\t", "\t")
        .replace("\\r", "\r")
        .replace("\\\"", "\"")
        .replace("\\'", "'")
        .replace("\\\\", "\\")
}

/// Normalize whitespace in text
fn normalize_whitespace(text: &str) -> String {
    // Replace multiple consecutive whitespace (except newlines) with single space
    let re = Regex::new(r"[^\S\n]+").unwrap();
    let result = re.replace_all(text, " ");
    
    // Replace multiple consecutive newlines with double newline
    let re_newlines = Regex::new(r"\n{3,}").unwrap();
    let result = re_newlines.replace_all(&result, "\n\n");
    
    result.to_string()
}

/// Normalize text for transcription (speech-to-text)
/// - Collapses all whitespace to single spaces
/// - Ensures proper spacing after punctuation
pub fn normalize_transcription(text: &str) -> String {
    // Collapse all whitespace to single spaces
    let re = Regex::new(r"\s+").unwrap();
    let result = re.replace_all(text, " ");
    
    // Ensure single space after punctuation marks
    let re_punct = Regex::new(r"([.,:;!?])\s*").unwrap();
    let result = re_punct.replace_all(&result, "$1 ");
    
    result.trim().to_string()
}

/// Extract size dimensions from size string (e.g., "512x768 (2:3 Portrait)" -> "512x768")
pub fn extract_size_dimensions(size_string: &str) -> String {
    // Try to extract just the dimensions part
    if let Some(space_idx) = size_string.find(' ') {
        size_string[..space_idx].to_string()
    } else {
        size_string.to_string()
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    
    #[test]
    fn test_process_llm_response() {
        let input = "<think>Let me think about this...</think>Hello world!";
        let result = process_llm_response(input);
        assert_eq!(result, "Hello world!");
    }
    
    #[test]
    fn test_unescape_text() {
        let input = "Line 1\\nLine 2\\tTabbed";
        let result = unescape_text(input);
        assert_eq!(result, "Line 1\nLine 2\tTabbed");
    }
    
    #[test]
    fn test_normalize_whitespace() {
        let input = "Hello    world\n\n\n\nTest";
        let result = normalize_whitespace(input);
        assert_eq!(result, "Hello world\n\nTest");
    }
    
    #[test]
    fn test_extract_size() {
        let input = "512x768 (2:3 Portrait)";
        let result = extract_size_dimensions(input);
        assert_eq!(result, "512x768");
    }
}

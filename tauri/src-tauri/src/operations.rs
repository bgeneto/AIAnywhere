//! Operations module for AI Anywhere
//! Defines AI operation types and their configurations

use serde::{Deserialize, Serialize};
use std::collections::HashMap;

/// Operation type enumeration
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub enum OperationType {
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

/// Option type for dynamic form controls
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub enum OptionType {
    Select,
    Text,
    Number,
}

/// Operation option configuration
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct OperationOption {
    pub key: String,
    pub name: String,
    #[serde(rename = "type")]
    pub option_type: OptionType,
    pub values: Vec<String>,
    pub default_value: String,
    pub required: bool,
}

/// Operation definition
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct Operation {
    #[serde(rename = "type")]
    pub operation_type: OperationType,
    pub name: String,
    pub description: String,
    pub system_prompt: String,
    pub options: Vec<OperationOption>,
}

/// Get default system prompts for all operation types
pub fn get_default_system_prompts() -> HashMap<String, String> {
    let mut prompts = HashMap::new();
    
    prompts.insert(
        "GeneralChat".to_string(),
        r#"LANGUAGE RULE: Always respond in the same language as the user's input text.

TASK: Provide helpful assistance without interaction.

RULES:
1. Use the EXACT same language as the user's input
2. NO greetings, introductions, or opening phrases
3. NO questions or requests for clarification
4. Make reasonable assumptions and respond immediately
5. Choose the most logical interpretation if unclear
6. Provide complete, substantive answers

Start your response directly with the helpful content."#.to_string(),
    );
    
    prompts.insert(
        "ImageGeneration".to_string(),
        r#"LANGUAGE RULE: Use the same language as the user's description for any text in the image.

TASK: Generate an image based on the user's description.

RULES:
1. Create exactly what is described
2. If text appears in the image, use the same language as the input
3. Follow the description precisely
4. Make the image high quality and detailed

Generate the image now."#.to_string(),
    );
    
    prompts.insert(
        "TextTranslation".to_string(),
        r#"CRITICAL: You are translating TO {language}. The output must be in {language} only.

TASK: Translate the provided text to {language}.

TRANSLATION RULES:
1. Output language: {language} ONLY
2. Keep the original writing style and tone
3. Maintain the same formality level
4. Preserve the original meaning exactly
5. NO explanations or comments
6. Return ONLY the translated text

Translate this text to {language}:"#.to_string(),
    );
    
    prompts.insert(
        "TextRewrite".to_string(),
        r#"LANGUAGE RULE: Keep the EXACT same language as the original text.

TASK: Rewrite text to improve quality while maintaining {tone} tone.

REWRITING RULES:
1. Use the SAME language as the input text
2. Apply {tone} tone consistently
3. Fix grammar, spelling, and punctuation errors
4. Improve clarity and flow
5. Keep the same meaning - NO new ideas
6. Maintain similar length (±20%)
7. Use natural, native-level phrasing
8. NO explanations or comments

Return ONLY the rewritten text:"#.to_string(),
    );
    
    prompts.insert(
        "TextSummarization".to_string(),
        r#"LANGUAGE RULE: Use the EXACT same language as the original text.

TASK: Create a {length} summary in {format} format.

SUMMARY RULES:
1. Use the SAME language as the input text
2. Length: {length} (BRIEF=2-3 sentences, MEDIUM=1 paragraph, DETAILED=2-3 paragraphs)
3. Format: {format} (PARAGRAPH=flowing text, BULLET POINTS=clear bullets, EXECUTIVE SUMMARY=overview+findings, KEY TAKEAWAYS=main insights)
4. Keep core message and critical details
5. Use clear, professional language
6. Focus on facts and actionable items
7. NO explanations or meta-commentary

Create the {length} {format} summary:"#.to_string(),
    );
    
    prompts.insert(
        "TextToSpeech".to_string(),
        r#"TASK: Convert text to speech audio file.

TEXT-TO-SPEECH RULES:
1. Use the provided text exactly as given
2. Apply the selected voice and speed settings
3. Generate high-quality audio output
4. Maintain natural speech patterns and pronunciation
5. Process the complete text without truncation

Convert this text to speech:"#.to_string(),
    );
    
    prompts.insert(
        "EmailReply".to_string(),
        r#"LANGUAGE RULE: Write your reply in the EXACT same language as the original email.

TASK: Generate an email reply with {tone} tone and {length} length.

EMAIL REPLY RULES:
1. Use the SAME language as the original email
2. Apply {tone} tone: PROFESSIONAL=business-appropriate, FRIENDLY=warm but professional, FORMAL=traditional business, URGENT=time-sensitive, APOLOGETIC=acknowledges issues, ENTHUSIASTIC=positive energy
3. Length: {length} (BRIEF=2-4 sentences, STANDARD=1-2 paragraphs, DETAILED=2-3 paragraphs)
4. Structure: Greeting → Acknowledge original → Address key points → Next steps → Professional closing + [Your Name]
5. Address ALL questions from the original email
6. Match the formality level of the original
7. NO subject line (replies keep original subject)
8. NO explanations or meta-commentary

Write a proper reply for this email message:"#.to_string(),
    );
    
    prompts.insert(
        "WhatsAppResponse".to_string(),
        r#"LANGUAGE RULE: Respond in the EXACT same language as the original message.

TASK: Generate a WhatsApp-style response with {tone} tone and {length} length.

WHATSAPP RESPONSE RULES:
1. Use the SAME language as the original message
2. Apply {tone} tone: CASUAL=relaxed everyday chat, FRIENDLY=warm and welcoming, ENTHUSIASTIC=excited and energetic, SUPPORTIVE=encouraging and helpful, HUMOROUS=light and funny, PROFESSIONAL=polite but approachable
3. Length: {length} (SHORT=1-2 sentences, MEDIUM=2-4 sentences, LONG=4-6 sentences)
4. Use natural, conversational language typical of WhatsApp
5. Include appropriate emojis when they fit naturally (don't overuse)
6. Match the informality level of the original message
7. Be responsive to the context and emotion of the message
8. NO formal greetings or closings unless appropriate
9. Keep it authentic and human-like
10. NO explanations or meta-commentary

Generate a natural WhatsApp response to this message:"#.to_string(),
    );
    
    prompts.insert(
        "SpeechToText".to_string(),
        r#"TASK: Transcribe the provided audio file to text.

TRANSCRIPTION RULES:
1. Return only the transcribed text, no explanations
2. Use proper punctuation and formatting
3. Maintain speaker distinctions if multiple speakers
4. Keep the same language as the audio
5. Include relevant non-speech sounds in [brackets] if significant

Transcribe this audio:"#.to_string(),
    );
    
    prompts.insert(
        "UnicodeSymbols".to_string(),
        r#"You are a helpful assistant that suggests relevant Unicode symbols and emojis for any given concept.
Provide several accurate, diverse options (with brief explanations if useful) and favor characters that display consistently across platforms.
Answer in plain text only, no markdown.
Now provide unicode symbols and/or emojis for representing the following: "#.to_string(),
    );
    
    prompts
}

/// Get default operations with their configurations
pub fn get_default_operations(system_prompts: &HashMap<String, String>) -> Vec<Operation> {
    vec![
        Operation {
            operation_type: OperationType::GeneralChat,
            name: "🛠️ Custom Task".to_string(),
            description: "Flexible AI help for any task or question".to_string(),
            system_prompt: system_prompts
                .get("GeneralChat")
                .cloned()
                .unwrap_or_default(),
            options: vec![],
        },
        Operation {
            operation_type: OperationType::EmailReply,
            name: "📧 Email Reply".to_string(),
            description: "Generate professional email replies".to_string(),
            system_prompt: system_prompts
                .get("EmailReply")
                .cloned()
                .unwrap_or_default(),
            options: vec![
                OperationOption {
                    key: "tone".to_string(),
                    name: "Tone".to_string(),
                    option_type: OptionType::Select,
                    values: vec![
                        "PROFESSIONAL".to_string(),
                        "FRIENDLY".to_string(),
                        "FORMAL".to_string(),
                        "URGENT".to_string(),
                        "APOLOGETIC".to_string(),
                        "ENTHUSIASTIC".to_string(),
                    ],
                    default_value: "PROFESSIONAL".to_string(),
                    required: true,
                },
                OperationOption {
                    key: "length".to_string(),
                    name: "Length".to_string(),
                    option_type: OptionType::Select,
                    values: vec![
                        "BRIEF".to_string(),
                        "STANDARD".to_string(),
                        "DETAILED".to_string(),
                    ],
                    default_value: "STANDARD".to_string(),
                    required: false,
                },
            ],
        },
        Operation {
            operation_type: OperationType::ImageGeneration,
            name: "🖼️ Image Generation".to_string(),
            description: "Generate images with AI".to_string(),
            system_prompt: system_prompts
                .get("ImageGeneration")
                .cloned()
                .unwrap_or_default(),
            options: vec![
                OperationOption {
                    key: "size".to_string(),
                    name: "Image Size".to_string(),
                    option_type: OptionType::Select,
                    values: vec![
                        "512x512 (1:1 Square)".to_string(),
                        "768x768 (1:1 Square)".to_string(),
                        "1024x1024 (1:1 Square)".to_string(),
                        "512x768 (2:3 Portrait)".to_string(),
                        "768x1152 (2:3 Portrait)".to_string(),
                        "832x1248 (2:3 Portrait)".to_string(),
                        "896x1344 (2:3 Portrait)".to_string(),
                        "768x512 (3:2 Landscape)".to_string(),
                        "1152x768 (3:2 Landscape)".to_string(),
                        "1248x832 (3:2 Landscape)".to_string(),
                        "1344x896 (3:2 Landscape)".to_string(),
                        "768x1024 (3:4 Portrait)".to_string(),
                        "936x1248 (3:4 Portrait)".to_string(),
                        "1024x768 (4:3 Landscape)".to_string(),
                        "1248x936 (4:3 Landscape)".to_string(),
                    ],
                    default_value: "512x768 (2:3 Portrait)".to_string(),
                    required: true,
                },
                OperationOption {
                    key: "quality".to_string(),
                    name: "Quality".to_string(),
                    option_type: OptionType::Select,
                    values: vec!["standard".to_string(), "hd".to_string()],
                    default_value: "hd".to_string(),
                    required: false,
                },
                OperationOption {
                    key: "style".to_string(),
                    name: "Style".to_string(),
                    option_type: OptionType::Select,
                    values: vec!["vivid".to_string(), "natural".to_string()],
                    default_value: "vivid".to_string(),
                    required: false,
                },
            ],
        },
        Operation {
            operation_type: OperationType::SpeechToText,
            name: "🎤 Speech-to-Text (STT)".to_string(),
            description: "Convert audio files to text".to_string(),
            system_prompt: system_prompts
                .get("SpeechToText")
                .cloned()
                .unwrap_or_default(),
            options: vec![OperationOption {
                key: "language".to_string(),
                name: "Language (optional)".to_string(),
                option_type: OptionType::Select,
                values: vec![
                    "auto".to_string(),
                    "en".to_string(),
                    "es".to_string(),
                    "fr".to_string(),
                    "de".to_string(),
                    "it".to_string(),
                    "pt".to_string(),
                    "ru".to_string(),
                    "ja".to_string(),
                    "ko".to_string(),
                    "zh".to_string(),
                    "ar".to_string(),
                    "hi".to_string(),
                ],
                default_value: "auto".to_string(),
                required: false,
            }],
        },
        Operation {
            operation_type: OperationType::TextRewrite,
            name: "📝 Text Correction & Rewrite".to_string(),
            description: "Rewrite and improve text".to_string(),
            system_prompt: system_prompts
                .get("TextRewrite")
                .cloned()
                .unwrap_or_default(),
            options: vec![OperationOption {
                key: "tone".to_string(),
                name: "Writing Tone".to_string(),
                option_type: OptionType::Select,
                values: vec![
                    "academic".to_string(),
                    "casual".to_string(),
                    "creative".to_string(),
                    "formal".to_string(),
                    "informal".to_string(),
                    "professional".to_string(),
                ],
                default_value: "professional".to_string(),
                required: true,
            }],
        },
        Operation {
            operation_type: OperationType::TextSummarization,
            name: "🧾 Text Summarization".to_string(),
            description: "Condense text into key points".to_string(),
            system_prompt: system_prompts
                .get("TextSummarization")
                .cloned()
                .unwrap_or_default(),
            options: vec![
                OperationOption {
                    key: "length".to_string(),
                    name: "Summary Length".to_string(),
                    option_type: OptionType::Select,
                    values: vec![
                        "brief".to_string(),
                        "medium".to_string(),
                        "detailed".to_string(),
                    ],
                    default_value: "medium".to_string(),
                    required: true,
                },
                OperationOption {
                    key: "format".to_string(),
                    name: "Format".to_string(),
                    option_type: OptionType::Select,
                    values: vec![
                        "paragraph".to_string(),
                        "bullet points".to_string(),
                        "executive summary".to_string(),
                        "key takeaways".to_string(),
                    ],
                    default_value: "bullet points".to_string(),
                    required: true,
                },
            ],
        },
        Operation {
            operation_type: OperationType::TextToSpeech,
            name: "🗣️ Text-to-Speech (TTS)".to_string(),
            description: "Convert text to audio speech".to_string(),
            system_prompt: system_prompts
                .get("TextToSpeech")
                .cloned()
                .unwrap_or_default(),
            options: vec![
                OperationOption {
                    key: "voice".to_string(),
                    name: "Voice".to_string(),
                    option_type: OptionType::Select,
                    values: vec![
                        "alloy".to_string(),
                        "ash".to_string(),
                        "ballad".to_string(),
                        "coral".to_string(),
                        "echo".to_string(),
                        "sage".to_string(),
                        "shimmer".to_string(),
                        "verse".to_string(),
                    ],
                    default_value: "alloy".to_string(),
                    required: true,
                },
                OperationOption {
                    key: "speed".to_string(),
                    name: "Speed".to_string(),
                    option_type: OptionType::Select,
                    values: vec![
                        "0.25".to_string(),
                        "0.5".to_string(),
                        "0.75".to_string(),
                        "1.0".to_string(),
                        "1.25".to_string(),
                        "1.5".to_string(),
                        "1.75".to_string(),
                        "2.0".to_string(),
                    ],
                    default_value: "1.0".to_string(),
                    required: false,
                },
                OperationOption {
                    key: "format".to_string(),
                    name: "Output Format".to_string(),
                    option_type: OptionType::Select,
                    values: vec![
                        "mp3".to_string(),
                        "opus".to_string(),
                        "aac".to_string(),
                        "flac".to_string(),
                    ],
                    default_value: "mp3".to_string(),
                    required: false,
                },
                OperationOption {
                    key: "language".to_string(),
                    name: "Language".to_string(),
                    option_type: OptionType::Select,
                    values: vec![
                        "en".to_string(),
                        "es".to_string(),
                        "fr".to_string(),
                        "de".to_string(),
                        "it".to_string(),
                        "pt".to_string(),
                        "pl".to_string(),
                        "tr".to_string(),
                        "ru".to_string(),
                        "cs".to_string(),
                        "ar".to_string(),
                        "zh-cn".to_string(),
                        "nl".to_string(),
                        "hi".to_string(),
                    ],
                    default_value: "pt".to_string(),
                    required: false,
                },
                OperationOption {
                    key: "model".to_string(),
                    name: "Model".to_string(),
                    option_type: OptionType::Select,
                    values: vec![
                        "tts-1".to_string(),
                        "tts-1-hd".to_string(),
                        "xtts".to_string(),
                    ],
                    default_value: "tts-1-hd".to_string(),
                    required: true,
                },
            ],
        },
        Operation {
            operation_type: OperationType::TextTranslation,
            name: "🈯️ Text Translation".to_string(),
            description: "Translate text to another language".to_string(),
            system_prompt: system_prompts
                .get("TextTranslation")
                .cloned()
                .unwrap_or_default(),
            options: vec![OperationOption {
                key: "language".to_string(),
                name: "Target Language".to_string(),
                option_type: OptionType::Select,
                values: vec![
                    "Arabic".to_string(),
                    "Bengali".to_string(),
                    "Chinese".to_string(),
                    "English".to_string(),
                    "French".to_string(),
                    "German".to_string(),
                    "Hindi".to_string(),
                    "Italian".to_string(),
                    "Japanese".to_string(),
                    "Korean".to_string(),
                    "Portuguese".to_string(),
                    "Punjabi".to_string(),
                    "Russian".to_string(),
                    "Spanish".to_string(),
                ],
                default_value: "Portuguese".to_string(),
                required: true,
            }],
        },
        Operation {
            operation_type: OperationType::UnicodeSymbols,
            name: "🔣 Unicode Symbols".to_string(),
            description: "Generate unicode symbols/emojis representing text".to_string(),
            system_prompt: system_prompts
                .get("UnicodeSymbols")
                .cloned()
                .unwrap_or_default(),
            options: vec![],
        },
        Operation {
            operation_type: OperationType::WhatsAppResponse,
            name: "💬 WhatsApp Response".to_string(),
            description: "Generate casual WhatsApp-style responses".to_string(),
            system_prompt: system_prompts
                .get("WhatsAppResponse")
                .cloned()
                .unwrap_or_default(),
            options: vec![
                OperationOption {
                    key: "tone".to_string(),
                    name: "Response Tone".to_string(),
                    option_type: OptionType::Select,
                    values: vec![
                        "CASUAL".to_string(),
                        "FRIENDLY".to_string(),
                        "ENTHUSIASTIC".to_string(),
                        "SUPPORTIVE".to_string(),
                        "HUMOROUS".to_string(),
                        "PROFESSIONAL".to_string(),
                    ],
                    default_value: "FRIENDLY".to_string(),
                    required: true,
                },
                OperationOption {
                    key: "length".to_string(),
                    name: "Response Length".to_string(),
                    option_type: OptionType::Select,
                    values: vec![
                        "SHORT".to_string(),
                        "MEDIUM".to_string(),
                        "LONG".to_string(),
                    ],
                    default_value: "SHORT".to_string(),
                    required: false,
                },
            ],
        },
    ]
}

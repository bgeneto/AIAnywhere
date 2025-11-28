using System;
using System.ClientModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AIAnywhere.Models;
using OpenAI;
using OpenAI.Audio;
using OpenAI.Chat;
using OpenAI.Images;

namespace AIAnywhere.Services
{
    public class LLMService
    {
        private readonly OpenAIClient _openAIClient;
        private readonly Configuration _config;

        // Debug logging - conditional based on configuration
        private static void LogApiDebug(
            string endpoint,
            string? request,
            string? response,
            Exception? ex = null
        )
        {
            // Only log if debug logging is enabled in configuration
            if (_staticConfig?.EnableDebugLogging != true)
                return;

            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff");
                var debugDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "api_debug");
                Directory.CreateDirectory(debugDir);

                var filename = $"api_debug_{timestamp}.log";
                var filePath = Path.Combine(debugDir, filename);

                var logContent = new StringBuilder();
                logContent.AppendLine($"=== API DEBUG LOG - {timestamp} ===");
                logContent.AppendLine($"Endpoint: {endpoint}");
                logContent.AppendLine($"Config ApiBaseUrl: {_staticConfig?.ApiBaseUrl ?? "null"}");
                logContent.AppendLine($"Config LlmModel: {_staticConfig?.LlmModel ?? "null"}");
                logContent.AppendLine($"Config ImageModel: {_staticConfig?.ImageModel ?? "null"}");
                logContent.AppendLine();

                logContent.AppendLine("=== REQUEST ===");
                logContent.AppendLine(request ?? "null");
                logContent.AppendLine();

                logContent.AppendLine("=== RESPONSE ===");
                logContent.AppendLine(response ?? "null");
                logContent.AppendLine();

                if (ex != null)
                {
                    logContent.AppendLine("=== EXCEPTION ===");
                    logContent.AppendLine($"Type: {ex.GetType().Name}");
                    logContent.AppendLine($"Message: {ex.Message}");
                    logContent.AppendLine($"StackTrace: {ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        logContent.AppendLine($"InnerException: {ex.InnerException.Message}");
                    }
                    logContent.AppendLine();
                }

                logContent.AppendLine("=== RESPONSE ANALYSIS ===");
                if (!string.IsNullOrEmpty(response))
                {
                    logContent.AppendLine($"Response Length: {response.Length}");
                    logContent.AppendLine(
                        $"Starts with: {(response.Length > 100 ? response.Substring(0, 100) + "..." : response)}"
                    );

                    // Try to parse as JSON
                    try
                    {
                        using var doc = JsonDocument.Parse(response);
                        logContent.AppendLine("✅ Valid JSON");

                        // Log JSON structure
                        logContent.AppendLine("JSON Properties:");
                        foreach (var prop in doc.RootElement.EnumerateObject())
                        {
                            logContent.AppendLine($"  - {prop.Name}: {prop.Value.ValueKind}");
                            if (prop.Value.ValueKind == JsonValueKind.Object)
                            {
                                foreach (var subProp in prop.Value.EnumerateObject())
                                {
                                    logContent.AppendLine(
                                        $"    - {subProp.Name}: {subProp.Value.ValueKind} = {subProp.Value}"
                                    );
                                }
                            }
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        logContent.AppendLine($"❌ Invalid JSON: {jsonEx.Message}");
                    }
                }

                File.WriteAllText(filePath, logContent.ToString());
                Console.WriteLine($"🔍 API Debug logged to: {filePath}");
            }
            catch (Exception logEx)
            {
                Console.WriteLine($"⚠️ Failed to log API debug: {logEx.Message}");
            }
        }

        // Static config reference for logging
        private static Configuration? _staticConfig;

        public LLMService(Configuration config)
        {
            _config = config;
            _staticConfig = config; // Set static reference for debugging

            // Create OpenAI client with custom endpoint support
            var clientOptions = new OpenAIClientOptions();
            if (
                !string.IsNullOrEmpty(_config.ApiBaseUrl)
                && _config.ApiBaseUrl != "https://api.openai.com/v1"
            )
            {
                clientOptions.Endpoint = new Uri(_config.ApiBaseUrl.TrimEnd('/'));
            }

            _openAIClient = new OpenAIClient(new ApiKeyCredential(_config.ApiKey), clientOptions);
        }

        public async Task<LLMResponse> ProcessRequestAsync(LLMRequest request)
        {
            try
            {
                if (request.OperationType == OperationType.ImageGeneration)
                {
                    return await ProcessImageGenerationAsync(request);
                }
                else if (request.OperationType == OperationType.SpeechToText)
                {
                    return await ProcessAudioTranscriptionAsync(request);
                }
                else if (request.OperationType == OperationType.TextToSpeech)
                {
                    return await ProcessTextToSpeechAsync(request);
                }
                else
                {
                    return await ProcessTextRequestAsync(request);
                }
            }
            catch (Exception ex)
            {
                return new LLMResponse { Success = false, Error = ex.Message };
            }
        }

        private async Task<LLMResponse> ProcessTextRequestAsync(LLMRequest request)
        {
            var operation = Operation
                .GetDefaultOperations(_config)
                .FirstOrDefault(o => o.Type == request.OperationType);

            if (operation == null)
            {
                return new LLMResponse { Success = false, Error = "Unknown operation type" };
            }

            // DEBUG: Prepare request info outside try block for exception logging
            string requestInfo = "";

            try
            {
                // Build the system prompt with options
                var systemPrompt = operation.SystemPrompt;
                foreach (var option in request.Options)
                {
                    systemPrompt = systemPrompt.Replace($"{{{option.Key}}}", option.Value);
                }

                // Build the user prompt
                var userPrompt = request.Prompt;
                if (!string.IsNullOrEmpty(request.SelectedText))
                {
                    userPrompt = $"{request.Prompt}\n\nText to process:\n{request.SelectedText}";
                }

                // DEBUG: Log request details
                requestInfo =
                    $"Operation: {request.OperationType}\nModel: {_config.LlmModel}\nSystem: {systemPrompt}\nUser: {userPrompt}";

                // Use HTTP method for custom endpoints to avoid token parsing issues
                if (
                    !string.IsNullOrEmpty(_config.ApiBaseUrl)
                    && _config.ApiBaseUrl != "https://api.openai.com/v1"
                )
                {
                    return await ProcessTextRequestHttpAsync(systemPrompt, userPrompt, requestInfo);
                }

                // Use OpenAI library for standard OpenAI endpoints
                var messages = new List<ChatMessage>
                {
                    ChatMessage.CreateSystemMessage(systemPrompt),
                    ChatMessage.CreateUserMessage(userPrompt),
                };

                var chatOptions = new ChatCompletionOptions
                {
                    MaxOutputTokenCount = 2000,
                    Temperature = 0.6f,
                };

                // Add reasoning_effort when thinking is disabled
                // Models that don't support it will simply ignore this parameter
                if (_config.DisableThinking)
                {
                    try
                    {
                        // Set ReasoningEffort property - models that don't support it will ignore this
                        var property = chatOptions.GetType().GetProperty("ReasoningEffort");
                        if (property != null)
                        {
                            property.SetValue(chatOptions, "low");
                        }
                    }
                    catch
                    {
                        // Ignore if setting ReasoningEffort fails
                    }
                }

                var chatClient = _openAIClient.GetChatClient(_config.LlmModel);
                var completion = await chatClient.CompleteChatAsync(messages, chatOptions);

                // DEBUG: Log the completion response
                var debugResponse = completion?.Value?.ToString() ?? "null completion";
                LogApiDebug(
                    $"chat/completions ({request.OperationType})",
                    requestInfo,
                    debugResponse
                );

                if (completion?.Value?.Content?.Count > 0)
                {
                    var rawContent = string.Join("", completion.Value.Content.Select(c => c.Text));
                    var processedContent = TextProcessor.ProcessLLMResponse(rawContent);
                    return new LLMResponse { Success = true, Content = processedContent };
                }

                return new LLMResponse { Success = false, Error = "No response received from API" };
            }
            catch (Exception ex)
            {
                // DEBUG: Log the exception
                LogApiDebug(
                    $"chat/completions ({request.OperationType}) ERROR",
                    requestInfo,
                    null,
                    ex
                );
                return new LLMResponse { Success = false, Error = $"API Error: {ex.Message}" };
            }
        }

        /// <summary>
        /// HTTP-based text completion that bypasses OpenAI library token usage parsing.
        /// This method avoids JSON parsing errors when custom API endpoints return null
        /// values for token usage fields that the OpenAI library expects to be numeric.
        /// </summary>
        private async Task<LLMResponse> ProcessTextRequestHttpAsync(
            string systemPrompt,
            string userPrompt,
            string requestInfo
        )
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _config.ApiKey);

                var baseUrl = _config.ApiBaseUrl.TrimEnd('/');
                var url = $"{baseUrl}/chat/completions";

                // Create OpenAI-compatible request
                var requestBody = new Dictionary<string, object>
                {
                    ["model"] = _config.LlmModel,
                    ["messages"] = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userPrompt },
                    },
                    ["max_tokens"] = 4096,
                    ["temperature"] = 0.6,
                };

                // Add reasoning_effort when thinking is disabled
                // Models that don't support it will simply ignore this parameter
                if (_config.DisableThinking)
                {
                    requestBody["reasoning_effort"] = "low";
                }

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // DEBUG: Log HTTP request
                LogApiDebug("chat/completions (HTTP)", json, null);

                var response = await client.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    LogApiDebug("chat/completions (HTTP) ERROR", json, errorContent);

                    return new LLMResponse
                    {
                        Success = false,
                        Error =
                            $"HTTP Chat Completion Failed: {response.StatusCode} - {errorContent}",
                    };
                }

                var responseContent = await response.Content.ReadAsStringAsync();

                // DEBUG: Log successful response
                LogApiDebug("chat/completions (HTTP) SUCCESS", json, responseContent);

                // Parse response manually, ignoring usage data
                using var doc = JsonDocument.Parse(responseContent);

                if (
                    doc.RootElement.TryGetProperty("choices", out var choicesArray)
                    && choicesArray.GetArrayLength() > 0
                )
                {
                    var firstChoice = choicesArray[0];
                    if (
                        firstChoice.TryGetProperty("message", out var messageObj)
                        && messageObj.TryGetProperty("content", out var contentProp)
                    )
                    {
                        var rawContent = contentProp.GetString();
                        if (!string.IsNullOrEmpty(rawContent))
                        {
                            var processedContent = TextProcessor.ProcessLLMResponse(rawContent);
                            return new LLMResponse { Success = true, Content = processedContent };
                        }
                    }
                }

                return new LLMResponse
                {
                    Success = false,
                    Error = "No valid content found in HTTP response",
                };
            }
            catch (Exception ex)
            {
                LogApiDebug("chat/completions (HTTP) EXCEPTION", requestInfo, null, ex);
                return new LLMResponse
                {
                    Success = false,
                    Error = $"HTTP Chat Completion Error: {ex.Message}",
                };
            }
        }

        private async Task<LLMResponse> ProcessImageGenerationAsync(LLMRequest request)
        {
            try
            {
                // Try HTTP-based generation first (supports exact sizes and custom parameters)
                var httpResult = await ProcessImageGenerationHttpAsync(request);
                if (httpResult.Success)
                {
                    return httpResult;
                }

                // Fallback to library-based generation
                return await ProcessImageGenerationLibraryAsync(request);
            }
            catch (Exception ex)
            {
                return new LLMResponse
                {
                    Success = false,
                    Error = $"Image Generation Error: {ex.Message}",
                };
            }
        }

        /// <summary>
        /// HTTP-based image generation that supports exact sizes and custom parameters
        /// </summary>
        private async Task<LLMResponse> ProcessImageGenerationHttpAsync(LLMRequest request)
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _config.ApiKey);

                // Use user-configured ApiBaseUrl if provided, otherwise default
                var baseUrl = string.IsNullOrWhiteSpace(_config.ApiBaseUrl)
                    ? "https://api.openai.com/v1"
                    : _config.ApiBaseUrl.TrimEnd('/');
                var url = $"{baseUrl}/images/generations";

                // Get parameters from request options
                var sizeString = request.Options.GetValueOrDefault("size", "512x512");
                var quality = request.Options.GetValueOrDefault("quality", "standard");
                var imageModel = !string.IsNullOrEmpty(_config.ImageModel)
                    ? _config.ImageModel
                    : "FLUX.1-schnell";

                // Create request body with exact size support
                var requestBody = new Dictionary<string, object>
                {
                    ["model"] = imageModel,
                    ["prompt"] = request.Prompt,
                    ["size"] = sizeString, // Use exact size string
                    ["quality"] = quality,
                    ["response_format"] = "url",
                    ["n"] = 1,
                };

                // Add custom parameters if supported (like steps for some models)
                if (request.Options.ContainsKey("steps"))
                {
                    if (int.TryParse(request.Options["steps"], out int steps))
                    {
                        requestBody["steps"] = steps;
                    }
                }

                // Add style parameter if provided
                if (request.Options.ContainsKey("style"))
                {
                    requestBody["style"] = request.Options["style"];
                }

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // DEBUG: Log request
                LogApiDebug("images/generations (HTTP)", json, null);

                var response = await client.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();

                    // DEBUG: Log error response
                    LogApiDebug("images/generations (HTTP) ERROR", json, errorContent);

                    return new LLMResponse
                    {
                        Success = false,
                        Error =
                            $"HTTP Image Generation Failed: {response.StatusCode} - {errorContent}",
                    };
                }

                var responseContent = await response.Content.ReadAsStringAsync();

                // DEBUG: Log successful response
                LogApiDebug("images/generations (HTTP) SUCCESS", json, responseContent);

                using var doc = JsonDocument.Parse(responseContent);

                if (
                    doc.RootElement.TryGetProperty("data", out var dataArray)
                    && dataArray.GetArrayLength() > 0
                )
                {
                    var firstItem = dataArray[0];
                    if (firstItem.TryGetProperty("url", out var urlProp))
                    {
                        var imageUrl = urlProp.GetString();
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            return new LLMResponse
                            {
                                Success = true,
                                Content = imageUrl,
                                IsImage = true,
                                ImageUrl = imageUrl,
                            };
                        }
                    }
                }

                return new LLMResponse
                {
                    Success = false,
                    Error = "No image URL found in HTTP response",
                };
            }
            catch (Exception ex)
            {
                // DEBUG: Log HTTP image generation exception
                LogApiDebug(
                    "images/generations (HTTP) EXCEPTION",
                    "HTTP Image Generation",
                    null,
                    ex
                );

                return new LLMResponse
                {
                    Success = false,
                    Error = $"HTTP Image Generation Error: {ex.Message}",
                };
            }
        }

        /// <summary>
        /// Library-based image generation (fallback method)
        /// </summary>
        private async Task<LLMResponse> ProcessImageGenerationLibraryAsync(LLMRequest request)
        {
            try
            {
                // Parse image size from user options
                var sizeString = request.Options.GetValueOrDefault("size", "512x512");
                var imageSize = ParseImageSize(sizeString);
                var quality = request.Options.GetValueOrDefault("quality", "standard");
                var imageQuality =
                    quality == "standard"
                        ? GeneratedImageQuality.High
                        : GeneratedImageQuality.Standard;

                // Create image generation options
                var imageOptions = new ImageGenerationOptions
                {
                    Size = imageSize,
                    Quality = imageQuality,
                    ResponseFormat = GeneratedImageFormat.Uri,
                };

                // Get image client with configured image model or fallback
                var imageModel = !string.IsNullOrEmpty(_config.ImageModel)
                    ? _config.ImageModel
                    : "FLUX.1-schnell";
                var imageClient = _openAIClient.GetImageClient(imageModel);

                var imageResult = await imageClient.GenerateImageAsync(
                    request.Prompt,
                    imageOptions
                );

                if (imageResult.Value?.ImageUri != null)
                {
                    var imageUrl = imageResult.Value.ImageUri.ToString();
                    return new LLMResponse
                    {
                        Success = true,
                        Content = imageUrl,
                        IsImage = true,
                        ImageUrl = imageUrl,
                    };
                }

                return new LLMResponse
                {
                    Success = false,
                    Error = "No image generated by library method",
                };
            }
            catch (Exception ex)
            {
                return new LLMResponse
                {
                    Success = false,
                    Error = $"Library Image Generation Error: {ex.Message}",
                };
            }
        }

        private GeneratedImageSize ParseImageSize(string sizeString)
        {
            return sizeString switch
            {
                // 1:1 Square sizes
                "512x512" => CustomImageSizes.W512xH512,
                "768x768" => CustomImageSizes.W768xH768,
                "1024x1024" => CustomImageSizes.W1024xH1024,

                // 2:3 Portrait sizes
                "512x768" => CustomImageSizes.W512xH768,
                "768x1152" => CustomImageSizes.W768xH1152,
                "832x1248" => CustomImageSizes.W832xH1248,
                "896x1344" => CustomImageSizes.W896xH1344,
                "1024x1536" => CustomImageSizes.W1024xH1536,

                // 3:2 Landscape sizes
                "768x512" => CustomImageSizes.W768xH512,
                "1152x768" => CustomImageSizes.W1152xH768,
                "1248x832" => CustomImageSizes.W1248xH832,
                "1344x896" => CustomImageSizes.W1344xH896,
                "1536x1024" => CustomImageSizes.W1536xH1024,

                // 3:4 Portrait sizes
                "768x1024" => CustomImageSizes.W768xH1024,
                "936x1248" => CustomImageSizes.W936xH1248,

                // 4:3 Landscape sizes
                "1024x768" => CustomImageSizes.W1024xH768,
                "1248x936" => CustomImageSizes.W1248xH936,

                // OpenAI standard sizes (for compatibility)
                "1792x1024" => GeneratedImageSize.W1792xH1024,
                "1024x1792" => GeneratedImageSize.W1024xH1792,

                // Default fallback
                _ => CustomImageSizes.W1024xH1024,
            };
        }

        private async Task<string> TranscribeAudioHttpAsync(
            string filePath,
            string model,
            string language
        )
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(300); // Set timeout to 5 minutes for large files
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _config.ApiKey);
            // Use user-configured ApiBaseUrl if provided, otherwise default
            var baseUrl = string.IsNullOrWhiteSpace(_config.ApiBaseUrl)
                ? "https://api.openai.com/v1"
                : _config.ApiBaseUrl.TrimEnd('/');
            var url = $"{baseUrl}/audio/transcriptions";

            // DEBUG: Log transcription request
            var requestInfo =
                $"File: {Path.GetFileName(filePath)}\nModel: {model}\nLanguage: {language}";
            LogApiDebug("audio/transcriptions", requestInfo, null);

            using var form = new MultipartFormDataContent
            {
                { new StreamContent(File.OpenRead(filePath)), "file", Path.GetFileName(filePath) },
                { new StringContent(model), "model" },
                { new StringContent("text"), "response_format" },
            };
            if (!string.IsNullOrEmpty(language) && language != "auto")
                form.Add(new StringContent(language), "language");

            var response = await client.PostAsync(url, form);
            response.EnsureSuccessStatusCode();
            var raw = await response.Content.ReadAsStringAsync();

            // DEBUG: Log transcription response
            LogApiDebug("audio/transcriptions RESPONSE", requestInfo, raw);

            try
            {
                using var doc = JsonDocument.Parse(raw);
                if (doc.RootElement.TryGetProperty("text", out var textProp))
                {
                    return textProp.GetString() ?? raw;
                }
            }
            catch (JsonException jsonEx)
            {
                // DEBUG: Log JSON parsing error
                LogApiDebug("audio/transcriptions JSON ERROR", requestInfo, raw, jsonEx);
                // If parsing fails, return raw response
            }
            return raw;
        }

        private async Task<LLMResponse> ProcessAudioTranscriptionAsync(LLMRequest request)
        {
            try
            {
                if (
                    string.IsNullOrEmpty(request.AudioFilePath)
                    || !File.Exists(request.AudioFilePath)
                )
                    return new LLMResponse
                    {
                        Success = false,
                        Error = "Audio file not found or not specified",
                    };

                var audioModel = !string.IsNullOrEmpty(_config.AudioModel)
                    ? _config.AudioModel
                    : "whisper-1";
                var language = request.Options.GetValueOrDefault("language", "auto");

                // Use HTTP transcription
                var rawText = await TranscribeAudioHttpAsync(
                    request.AudioFilePath,
                    audioModel,
                    language
                );

                // Normalize whitespace: collapse all whitespace characters to single spaces
                rawText = Regex.Replace(rawText, @"\s+", " ").Trim();

                // Ensure single space after punctuation marks (., , ; ! ? :)
                rawText = Regex.Replace(rawText, @"([.,:;!?])\s*", "$1 ");

                // Clean up any trailing spaces at the end
                rawText = rawText.Trim();

                var processed = TextProcessor.ProcessLLMResponse(rawText);
                return new LLMResponse { Success = true, Content = processed };
            }
            catch (Exception ex)
            {
                return new LLMResponse
                {
                    Success = false,
                    Error = $"Audio Transcription Error: {ex.Message}",
                };
            }
        }

        private async Task<LLMResponse> ProcessTextToSpeechAsync(LLMRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Prompt))
                    return new LLMResponse
                    {
                        Success = false,
                        Error = "Text prompt is required for Text to Speech",
                    };

                // Get TTS model from request options or configuration
                var ttsModel = request.Options.GetValueOrDefault("model", _config.TtsModel);

                // Get options from request
                var voice = request.Options.GetValueOrDefault("voice", "alloy");
                var speedStr = request.Options.GetValueOrDefault("speed", "1.0");
                var format = request.Options.GetValueOrDefault("format", "mp3");
                var language = request.Options.GetValueOrDefault("language", "pt");

                // Parse speed to float
                if (!float.TryParse(speedStr, out float speed))
                {
                    speed = 1.0f;
                }

                // Clamp speed to valid range (0.25 to 2.0)
                speed = Math.Max(0.25f, Math.Min(2.0f, speed));

                // Generate speech using HTTP API
                var audioData = await GenerateSpeechHttpAsync(
                    request.Prompt,
                    ttsModel,
                    voice,
                    speed,
                    format,
                    language
                );

                return new LLMResponse
                {
                    Success = true,
                    Content = "Audio generated successfully",
                    IsAudio = true,
                    AudioData = audioData,
                    AudioFormat = format,
                };
            }
            catch (Exception ex)
            {
                // DEBUG: Log TTS exception
                LogApiDebug("Text-to-Speech ERROR", $"Text: {request.Prompt}", null, ex);

                return new LLMResponse
                {
                    Success = false,
                    Error = $"Text to Speech Error: {ex.Message}",
                };
            }
        }

        private async Task<byte[]> GenerateSpeechHttpAsync(
            string text,
            string model,
            string voice,
            float speed,
            string format,
            string language
        )
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(300); // Set timeout to 5 minutes for large files
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _config.ApiKey);

            // Use user-configured ApiBaseUrl if provided, otherwise default
            var baseUrl = string.IsNullOrWhiteSpace(_config.ApiBaseUrl)
                ? "https://api.openai.com/v1"
                : _config.ApiBaseUrl.TrimEnd('/');
            var url = $"{baseUrl}/audio/speech";

            var requestBody = new
            {
                model = model,
                input = text,
                voice = voice,
                response_format = format,
                speed = speed,
                language = language,
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // DEBUG: Log TTS request
            LogApiDebug("audio/speech", json, null);

            var response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            // DEBUG: Log TTS response (note: this is binary data)
            var responseSize = response.Content.Headers.ContentLength ?? 0;
            LogApiDebug(
                "audio/speech RESPONSE",
                json,
                $"Binary audio data, size: {responseSize} bytes"
            );

            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}

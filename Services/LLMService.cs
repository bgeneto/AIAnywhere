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

        public LLMService(Configuration config)
        {
            _config = config;

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

                // Create chat messages
                var messages = new List<ChatMessage>
                {
                    ChatMessage.CreateSystemMessage(systemPrompt),
                    ChatMessage.CreateUserMessage(userPrompt),
                }; // Create chat completion options
                var chatOptions = new ChatCompletionOptions
                {
                    MaxOutputTokenCount = 2000,
                    Temperature = 0.6f,
                };

                // Get chat client and make request
                var chatClient = _openAIClient.GetChatClient(_config.LlmModel);
                var completion = await chatClient.CompleteChatAsync(messages, chatOptions);
                if (completion.Value?.Content?.Count > 0)
                {
                    var rawContent = string.Join("", completion.Value.Content.Select(c => c.Text));

                    // Process the raw response to normalize formatting and line breaks
                    var processedContent = TextProcessor.ProcessLLMResponse(rawContent);

                    return new LLMResponse { Success = true, Content = processedContent };
                }

                return new LLMResponse { Success = false, Error = "No response received from API" };
            }
            catch (Exception ex)
            {
                return new LLMResponse { Success = false, Error = $"API Error: {ex.Message}" };
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

                var response = await client.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new LLMResponse
                    {
                        Success = false,
                        Error =
                            $"HTTP Image Generation Failed: {response.StatusCode} - {errorContent}",
                    };
                }

                var responseContent = await response.Content.ReadAsStringAsync();
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
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _config.ApiKey);
            // Use user-configured ApiBaseUrl if provided, otherwise default
            var baseUrl = string.IsNullOrWhiteSpace(_config.ApiBaseUrl)
                ? "https://api.openai.com/v1"
                : _config.ApiBaseUrl.TrimEnd('/');
            var url = $"{baseUrl}/audio/transcriptions";
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
            try
            {
                using var doc = JsonDocument.Parse(raw);
                if (doc.RootElement.TryGetProperty("text", out var textProp))
                {
                    return textProp.GetString() ?? raw;
                }
            }
            catch
            {
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

                // Get TTS model from configuration (hardcoded to tts-1-hd)
                var ttsModel = _config.TtsModel;

                // Get options from request
                var voice = request.Options.GetValueOrDefault("voice", "alloy");
                var speedStr = request.Options.GetValueOrDefault("speed", "1.0");
                var format = request.Options.GetValueOrDefault("format", "mp3");

                // Parse speed to float
                if (!float.TryParse(speedStr, out float speed))
                {
                    speed = 1.0f;
                }

                // Clamp speed to valid range (0.25 to 4.0)
                speed = Math.Max(0.25f, Math.Min(4.0f, speed));

                // Generate speech using HTTP API
                var audioData = await GenerateSpeechHttpAsync(
                    request.Prompt,
                    ttsModel,
                    voice,
                    speed,
                    format
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
            string format
        )
        {
            using var client = new HttpClient();
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
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}

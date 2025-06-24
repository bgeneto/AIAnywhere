using System;
using System.ClientModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AIAnywhere.Models;
using OpenAI;
using OpenAI.Audio;
using OpenAI.Chat;
using OpenAI.Images;
using System.Text.RegularExpressions;

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
                else if (request.OperationType == OperationType.AudioTranscription)
                {
                    return await ProcessAudioTranscriptionAsync(request);
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
                // Parse image size from user options
                var sizeString = request.Options.GetValueOrDefault("size", "1024x1024");
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

                return new LLMResponse { Success = false, Error = "No image generated" };
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

        private GeneratedImageSize ParseImageSize(string sizeString)
        {
            return sizeString switch
            {
                "512x512" => GeneratedImageSize.W512xH512,
                "768x768" => GeneratedImageSize.W1024xH1024, // Map to closest available
                "1024x768" => GeneratedImageSize.W1024xH1024, // Map to closest available
                "768x1024" => GeneratedImageSize.W1024xH1024, // Map to closest available
                "1024x1024" => GeneratedImageSize.W1024xH1024,
                "1792x1024" => GeneratedImageSize.W1792xH1024,
                "1024x1792" => GeneratedImageSize.W1024xH1792,
                _ => GeneratedImageSize.W1024xH1024,
            };
        }

        private async Task<string> TranscribeAudioRawAsync(string filePath, string model, string language)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _config.ApiKey);
            // Use user-configured ApiBaseUrl if provided, otherwise default
            var baseUrl = string.IsNullOrWhiteSpace(_config.ApiBaseUrl)
                ? "https://api.openai.com/v1"
                : _config.ApiBaseUrl.TrimEnd('/');
            var url = $"{baseUrl}/audio/transcriptions";
            using var form = new MultipartFormDataContent();
            form.Add(new StreamContent(File.OpenRead(filePath)), "file", Path.GetFileName(filePath));
            form.Add(new StringContent(model), "model");
            form.Add(new StringContent("text"), "response_format");
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

        // Backup HTTP fallback transcription for debugging
        private async Task<LLMResponse> ProcessAudioTranscriptionHttpAsync(LLMRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.AudioFilePath) || !File.Exists(request.AudioFilePath))
                {
                    return new LLMResponse { Success = false, Error = "Audio file not found or not specified" };
                }

                var audioModel = !string.IsNullOrEmpty(_config.AudioModel) ? _config.AudioModel : "whisper-1";
                var language = request.Options.GetValueOrDefault("language", "auto");
                var rawText = await TranscribeAudioRawAsync(request.AudioFilePath, audioModel, language);
                var processed = TextProcessor.ProcessLLMResponse(rawText);
                return new LLMResponse { Success = true, Content = processed };
            }
            catch (Exception ex)
            {
                return new LLMResponse { Success = false, Error = $"Audio Transcription Error: {ex.Message}" };
            }
        }

        private async Task<LLMResponse> ProcessAudioTranscriptionAsync(LLMRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.AudioFilePath) || !File.Exists(request.AudioFilePath))
                    return new LLMResponse { Success = false, Error = "Audio file not found or not specified" };

                var audioModel = !string.IsNullOrEmpty(_config.AudioModel) ? _config.AudioModel : "whisper-1";
                var language = request.Options.GetValueOrDefault("language", "auto");

                // Use HTTP fallback transcription
                var rawText = await TranscribeAudioRawAsync(request.AudioFilePath, audioModel, language);

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
                return new LLMResponse { Success = false, Error = $"Audio Transcription Error: {ex.Message}" };
            }
        }
    }
}

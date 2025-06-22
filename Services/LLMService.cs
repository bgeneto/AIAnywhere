using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AIAnywhere.Models;
using OpenAI;
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
                    Temperature = 0.7f,
                };

                // Get chat client and make request
                var chatClient = _openAIClient.GetChatClient(_config.LlmModel);
                var completion = await chatClient.CompleteChatAsync(messages, chatOptions);                if (completion.Value?.Content?.Count > 0)
                {
                    var rawContent = string.Join("", completion.Value.Content.Select(c => c.Text));

                    // DEBUG: Test with your example to see what's happening
                    var testExample = "Olá [Nome do destinatário],\\n\\nEspero que esteja bem. Sim, como coordenador de área, é você quem deve iniciar os trâmites no SEI para o processo de concurso de professor substituto de química.\\n\\nAtenciosamente,\\n";
                    System.Diagnostics.Debug.WriteLine($"DEBUG: Test example BEFORE processing: {TextProcessor.DebugText(testExample)}");
                    var processedExample = TextProcessor.ProcessLLMResponse(testExample);
                    System.Diagnostics.Debug.WriteLine($"DEBUG: Test example AFTER processing: {TextProcessor.DebugText(processedExample)}");

                    // DEBUG: Log the raw content to understand what we're getting
                    System.Diagnostics.Debug.WriteLine($"DEBUG: Raw LLM response: {TextProcessor.DebugText(rawContent)}");

                    // Process the raw response to normalize formatting and line breaks
                    var processedContent = TextProcessor.ProcessLLMResponse(rawContent);

                    // DEBUG: Log the processed content
                    System.Diagnostics.Debug.WriteLine($"DEBUG: Processed LLM response: {TextProcessor.DebugText(processedContent)}");

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

                // Get image client with FLUX.1-schnell model and generate image
                var imageClient = _openAIClient.GetImageClient("FLUX.1-schnell");
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
    }
}

using ChatApp.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace ChatApp.Infrastructure.Services
{
    public class OpenAIService : IAIService
    {
        private readonly OpenAIClient _openAIClient;
        private readonly ChatApp.Application.Settings.OpenAISettings _settings;
        private readonly ILogger<OpenAIService> _logger;

        public OpenAIService(IOptions<ChatApp.Application.Settings.OpenAISettings> openAISettingsOptions, ILogger<OpenAIService> logger)
        {
            _settings = openAISettingsOptions?.Value ?? throw new ArgumentNullException(nameof(openAISettingsOptions), "OpenAI Settings cannot be null.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                _logger.LogCritical("OpenAI API Key is missing in configuration.");
                throw new InvalidOperationException("OpenAI API Key is not configured.");
            }

            _openAIClient = new OpenAIClient(_settings.ApiKey);
            _logger.LogInformation("OpenAIClient initialized using configured settings.");
        }

        public async Task<string?> GetChatCompletionAsync(string userPrompt, Guid? userIdForContext = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userPrompt))
            {
                _logger.LogWarning("GetChatCompletionAsync called with empty or null prompt.");
                return "I need a question or a prompt to respond to!";
            }

            _logger.LogInformation("Getting chat completion for UserId: {UserId}, Prompt (first 50 chars): '{PromptStart}'",
                userIdForContext?.ToString() ?? "N/A",
                userPrompt.Substring(0, Math.Min(userPrompt.Length, 50)));

            try
            {
                var messages = new List<Message>
                {
                    new Message(Role.System, "You are a helpful AI assistant in a chat application. Keep your responses concise and friendly."),
                    new Message(Role.User, userPrompt)
                };

                var chatRequest = new ChatRequest(messages, model: _settings.DefaultModel, temperature: 0.7, maxTokens: 200);
                var result = await _openAIClient.ChatEndpoint.GetCompletionAsync(chatRequest, cancellationToken);

                if (result?.FirstChoice?.Message?.Content != null)
                {
                    var aiResponse = result.FirstChoice.Message.Content.Trim();
                    return aiResponse;
                }
                else
                {
                    _logger.LogWarning("OpenAI did not return a valid message content. API Result: {@ResultDetails}", result);
                    return "Sorry, I couldn't get a response from the AI assistant at this moment.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling OpenAI API for prompt: '{PromptStart}'", userPrompt.Substring(0, Math.Min(userPrompt.Length, 50)));
                return $"Sorry, an error occurred while contacting the AI assistant. Please try again later.";
            }
        }
    }
}
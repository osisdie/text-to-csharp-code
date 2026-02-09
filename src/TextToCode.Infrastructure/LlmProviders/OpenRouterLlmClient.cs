using System.ClientModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using TextToCode.Core.Interfaces;
using TextToCode.Infrastructure.Prompts;

namespace TextToCode.Infrastructure.LlmProviders;

public sealed class OpenRouterLlmClient : ILlmClient
{
    private readonly ChatClient _chatClient;
    private readonly OpenRouterOptions _options;
    private readonly ILogger<OpenRouterLlmClient> _logger;

    public OpenRouterLlmClient(IOptions<OpenRouterOptions> options, ILogger<OpenRouterLlmClient> logger)
    {
        _options = options.Value;
        _logger = logger;

        var credential = new ApiKeyCredential(_options.ApiKey);
        var clientOptions = new OpenAIClientOptions { Endpoint = new Uri(_options.BaseUrl) };
        var openAiClient = new OpenAIClient(credential, clientOptions);
        _chatClient = openAiClient.GetChatClient(_options.Model);
    }

    public async Task<string> GenerateCodeAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating code with model {Model}", _options.Model);

        var messages = new List<ChatMessage>
        {
            ChatMessage.CreateSystemMessage(systemPrompt),
            ChatMessage.CreateUserMessage(userPrompt)
        };

        var chatOptions = new ChatCompletionOptions
        {
            Temperature = (float)_options.Temperature,
            MaxOutputTokenCount = _options.MaxTokens
        };

        var response = await _chatClient.CompleteChatAsync(messages, chatOptions, cancellationToken);
        var rawContent = response.Value.Content[0].Text;

        _logger.LogDebug("LLM response length: {Length} chars", rawContent.Length);

        return LlmResponseParser.ExtractCode(rawContent);
    }

    public async Task<string> DiagnoseAndFixAsync(string systemPrompt, string code, string error, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Self-healing: diagnosing and fixing code with model {Model}", _options.Model);

        var messages = new List<ChatMessage>
        {
            ChatMessage.CreateSystemMessage(systemPrompt),
            ChatMessage.CreateUserMessage($"Code:\n```csharp\n{code}\n```\n\nError:\n{error}")
        };

        var chatOptions = new ChatCompletionOptions
        {
            Temperature = (float)_options.Temperature,
            MaxOutputTokenCount = _options.MaxTokens
        };

        var response = await _chatClient.CompleteChatAsync(messages, chatOptions, cancellationToken);
        var rawContent = response.Value.Content[0].Text;

        return LlmResponseParser.ExtractCode(rawContent);
    }
}

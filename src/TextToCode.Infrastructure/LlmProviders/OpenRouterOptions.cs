namespace TextToCode.Infrastructure.LlmProviders;

public sealed class OpenRouterOptions
{
    public const string SectionName = "OpenRouter";

    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://openrouter.ai/api/v1";
    public string Model { get; set; } = "anthropic/claude-sonnet-4";
    public double Temperature { get; set; } = 0.3;
    public int MaxTokens { get; set; } = 4096;
}

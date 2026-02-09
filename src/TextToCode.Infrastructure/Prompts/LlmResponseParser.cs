using System.Text.RegularExpressions;

namespace TextToCode.Infrastructure.Prompts;

public static partial class LlmResponseParser
{
    public static string ExtractCode(string llmResponse)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(llmResponse);

        // Try to extract from ```csharp ... ``` fence first
        var match = CSharpFencePattern().Match(llmResponse);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        // Fall back to generic ``` ... ``` fence
        match = GenericFencePattern().Match(llmResponse);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        // If no fences found, return the raw response trimmed
        return llmResponse.Trim();
    }

    [GeneratedRegex(@"```csharp\s*\n(.*?)```", RegexOptions.Singleline)]
    private static partial Regex CSharpFencePattern();

    [GeneratedRegex(@"```\s*\n(.*?)```", RegexOptions.Singleline)]
    private static partial Regex GenericFencePattern();
}

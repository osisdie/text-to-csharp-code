namespace TextToCode.Core.ValueObjects;

public sealed record LlmModelIdentifier
{
    public string Provider { get; }
    public string Model { get; }
    public string FullId { get; }

    public LlmModelIdentifier(string fullId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullId);
        FullId = fullId;

        var parts = fullId.Split('/', 2);
        if (parts.Length == 2)
        {
            Provider = parts[0];
            Model = parts[1];
        }
        else
        {
            Provider = string.Empty;
            Model = fullId;
        }
    }

    public override string ToString() => FullId;
}

using System.Reflection;
using System.Text.RegularExpressions;
using TextToCode.Core.Interfaces;

namespace TextToCode.Infrastructure.Prompts;

public sealed partial class EmbeddedPromptTemplate : IPromptTemplate
{
    private readonly string _template;

    public string Name { get; }

    private EmbeddedPromptTemplate(string name, string template)
    {
        Name = name;
        _template = template;
    }

    public string Render(IDictionary<string, string> variables)
    {
        return VariablePattern().Replace(_template, match =>
        {
            var key = match.Groups[1].Value;
            return variables.TryGetValue(key, out var value) ? value : match.Value;
        });
    }

    public static EmbeddedPromptTemplate Load(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var fullName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase))
            ?? throw new FileNotFoundException($"Embedded resource '{resourceName}' not found.");

        using var stream = assembly.GetManifestResourceStream(fullName)!;
        using var reader = new StreamReader(stream);

        var name = Path.GetFileNameWithoutExtension(resourceName);
        var content = reader.ReadToEnd();

        return new EmbeddedPromptTemplate(name, content);
    }

    [GeneratedRegex(@"\{\{(\w+)\}\}")]
    private static partial Regex VariablePattern();
}

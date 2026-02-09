using TextToCode.Core.Interfaces;

namespace TextToCode.Infrastructure.Prompts;

public sealed class PromptTemplateFactory : IPromptTemplateFactory
{
    private readonly Dictionary<string, IPromptTemplate> _cache = new(StringComparer.OrdinalIgnoreCase);

    public IPromptTemplate GetCodeGenerationTemplate() =>
        GetOrLoad("CodeGeneration", "CodeGeneration.md");

    public IPromptTemplate GetSelfHealingTemplate() =>
        GetOrLoad("SelfHealing", "SelfHealing.md");

    private IPromptTemplate GetOrLoad(string key, string resourceName)
    {
        if (!_cache.TryGetValue(key, out var template))
        {
            template = EmbeddedPromptTemplate.Load(resourceName);
            _cache[key] = template;
        }

        return template;
    }
}

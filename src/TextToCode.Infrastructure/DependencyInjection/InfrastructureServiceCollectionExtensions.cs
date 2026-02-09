using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TextToCode.Core.Interfaces;
using TextToCode.Infrastructure.Compilation;
using TextToCode.Infrastructure.LlmProviders;
using TextToCode.Infrastructure.Prompts;
using TextToCode.Infrastructure.Validation;

namespace TextToCode.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddTextToCodeInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Options
        services.Configure<OpenRouterOptions>(configuration.GetSection(OpenRouterOptions.SectionName));

        // Core services
        services.AddSingleton<ICodeCompiler, RoslynCodeCompiler>();
        services.AddSingleton<ICodeValidator, RoslynCodeValidator>();
        services.AddSingleton<ICodeExecutor, SandboxedCodeExecutor>();
        services.AddSingleton<ILlmClient, OpenRouterLlmClient>();

        // Prompts
        services.AddSingleton<IPromptTemplateFactory, PromptTemplateFactory>();

        return services;
    }
}

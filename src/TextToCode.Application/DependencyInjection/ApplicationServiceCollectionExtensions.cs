using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TextToCode.Application.Pipelines;
using TextToCode.Application.Services;

namespace TextToCode.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddTextToCodeApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PipelineOptions>(configuration.GetSection(PipelineOptions.SectionName));
        services.AddScoped<CodeGenerationPipeline>();
        services.AddScoped<ICodeGenerationService, CodeGenerationService>();

        return services;
    }
}

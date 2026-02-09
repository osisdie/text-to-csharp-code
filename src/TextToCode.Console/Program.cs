using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TextToCode.Application.DependencyInjection;
using TextToCode.Application.Services;
using TextToCode.Console.Commands;
using TextToCode.Infrastructure.DependencyInjection;

Console.OutputEncoding = Encoding.UTF8;

var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
    ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
    ?? "Development";

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true)
    .AddEnvironmentVariables("TEXTTOCODE_")
    .Build();

var services = new ServiceCollection();
services.AddSingleton<IConfiguration>(configuration);
services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
services.AddTextToCodeApplication(configuration);
services.AddTextToCodeInfrastructure(configuration);

var serviceProvider = services.BuildServiceProvider();
GenerateCommandState.Service = serviceProvider.GetRequiredService<ICodeGenerationService>();

var rootCommand = new RootCommand("TextToCode - AI-powered C# code generation engine");
rootCommand.AddCommand(new GenerateCommand());
rootCommand.AddCommand(new InteractiveReplCommand());

// Default to REPL mode when no subcommand specified
rootCommand.SetHandler((InvocationContext ctx) =>
{
    var replCommand = new InteractiveReplCommand();
    return replCommand.InvokeAsync(Array.Empty<string>());
});

return await rootCommand.InvokeAsync(args);

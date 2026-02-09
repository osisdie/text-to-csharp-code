using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using TextToCode.Application.DTOs;
using Xunit;
using TextToCode.Application.Pipelines;
using TextToCode.Core.Entities;
using TextToCode.Core.Enums;
using TextToCode.Core.Interfaces;
using TextToCode.Core.ValueObjects;

namespace TextToCode.Application.Tests.Pipelines;

public class CodeGenerationPipelineTests
{
    private readonly Mock<ILlmClient> _llmClient = new();
    private readonly Mock<ICodeValidator> _validator = new();
    private readonly Mock<ICodeCompiler> _compiler = new();
    private readonly Mock<ICodeExecutor> _executor = new();
    private readonly PipelineOptions _options = new() { MaxRetries = 2 };

    private CodeGenerationPipeline CreatePipeline() => new(
        _llmClient.Object,
        _validator.Object,
        _compiler.Object,
        _executor.Object,
        Options.Create(_options),
        NullLogger<CodeGenerationPipeline>.Instance);

    [Fact]
    public async Task Should_complete_on_first_attempt_when_everything_succeeds()
    {
        var code = "Console.WriteLine(\"test\");";
        var assemblyBytes = new byte[] { 1, 2, 3 };

        _llmClient.Setup(x => x.GenerateCodeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(code);
        _validator.Setup(x => x.Validate(code)).Returns(ValidationResult.Valid());
        _compiler.Setup(x => x.Compile(code)).Returns(CompilationResult.Succeeded(assemblyBytes, []));
        _executor.Setup(x => x.ExecuteAsync(assemblyBytes, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ExecutionResult.Succeeded("test output", TimeSpan.FromMilliseconds(50)));

        var pipeline = CreatePipeline();
        var context = await pipeline.ExecuteAsync("test prompt", "system", "heal");

        context.Status.Should().Be(PipelineStatus.Completed);
        context.AttemptNumber.Should().Be(1);
        context.Execution!.ConsoleOutput.Should().Be("test output");
    }

    [Fact]
    public async Task Should_self_heal_on_compilation_failure()
    {
        var badCode = "bad code";
        var goodCode = "Console.WriteLine(\"fixed\");";
        var assemblyBytes = new byte[] { 1, 2, 3 };

        _llmClient.SetupSequence(x => x.GenerateCodeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(badCode);
        _llmClient.Setup(x => x.DiagnoseAndFixAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(goodCode);

        _validator.Setup(x => x.Validate(It.IsAny<string>())).Returns(ValidationResult.Valid());

        _compiler.Setup(x => x.Compile(badCode))
            .Returns(CompilationResult.Failed([new CompilerDiagnostic("CS0001", "Error", "Error", 1, 1)]));
        _compiler.Setup(x => x.Compile(goodCode))
            .Returns(CompilationResult.Succeeded(assemblyBytes, []));

        _executor.Setup(x => x.ExecuteAsync(assemblyBytes, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ExecutionResult.Succeeded("fixed output", TimeSpan.FromMilliseconds(50)));

        var pipeline = CreatePipeline();
        var context = await pipeline.ExecuteAsync("test", "system", "heal");

        context.Status.Should().Be(PipelineStatus.Completed);
        context.AttemptNumber.Should().Be(2);
        context.DiagnosticHistory.Should().HaveCount(1);
    }

    [Fact]
    public async Task Should_fail_after_exhausting_retries()
    {
        _llmClient.Setup(x => x.GenerateCodeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("bad code");
        _llmClient.Setup(x => x.DiagnoseAndFixAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("still bad");

        _validator.Setup(x => x.Validate(It.IsAny<string>())).Returns(ValidationResult.Valid());
        _compiler.Setup(x => x.Compile(It.IsAny<string>()))
            .Returns(CompilationResult.Failed([new CompilerDiagnostic("CS0001", "Persistent error", "Error", 1, 1)]));

        var pipeline = CreatePipeline();
        var context = await pipeline.ExecuteAsync("test", "system", "heal");

        context.Status.Should().Be(PipelineStatus.Failed);
        context.DiagnosticHistory.Should().HaveCount(3); // 1 initial + 2 retries
    }

    [Fact]
    public async Task Should_report_progress_updates()
    {
        var updates = new List<PipelineProgressUpdate>();
        var progress = new Progress<PipelineProgressUpdate>(u => updates.Add(u));

        _llmClient.Setup(x => x.GenerateCodeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Console.WriteLine(\"ok\");");
        _validator.Setup(x => x.Validate(It.IsAny<string>())).Returns(ValidationResult.Valid());
        _compiler.Setup(x => x.Compile(It.IsAny<string>()))
            .Returns(CompilationResult.Succeeded([1, 2, 3], []));
        _executor.Setup(x => x.ExecuteAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ExecutionResult.Succeeded("output", TimeSpan.Zero));

        var pipeline = CreatePipeline();
        await pipeline.ExecuteAsync("test", "system", "heal", progress);

        // Allow Progress<T> callbacks to complete
        await Task.Delay(100);

        updates.Should().Contain(u => u.Status == PipelineStatus.Generating);
        updates.Should().Contain(u => u.Status == PipelineStatus.Completed);
    }

    [Fact]
    public async Task Should_self_heal_on_validation_failure()
    {
        var unsafeCode = "Process.Start(\"cmd\");";
        var safeCode = "Console.WriteLine(\"safe\");";
        var assemblyBytes = new byte[] { 1, 2, 3 };

        _llmClient.SetupSequence(x => x.GenerateCodeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(unsafeCode);
        _llmClient.Setup(x => x.DiagnoseAndFixAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(safeCode);

        _validator.Setup(x => x.Validate(unsafeCode))
            .Returns(ValidationResult.Invalid([new ValidationViolation("DangerousApi", "Not allowed", ValidationSeverity.Error)]));
        _validator.Setup(x => x.Validate(safeCode))
            .Returns(ValidationResult.Valid());

        _compiler.Setup(x => x.Compile(safeCode))
            .Returns(CompilationResult.Succeeded(assemblyBytes, []));
        _executor.Setup(x => x.ExecuteAsync(assemblyBytes, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ExecutionResult.Succeeded("safe output", TimeSpan.Zero));

        var pipeline = CreatePipeline();
        var context = await pipeline.ExecuteAsync("test", "system", "heal");

        context.Status.Should().Be(PipelineStatus.Completed);
        context.AttemptNumber.Should().Be(2);
    }
}

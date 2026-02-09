using FluentAssertions;
using TextToCode.Infrastructure.Compilation;
using Xunit;

namespace TextToCode.Infrastructure.Tests.Compilation;

public class SandboxedCodeExecutorTests
{
    private readonly RoslynCodeCompiler _compiler = new();
    private readonly SandboxedCodeExecutor _executor = new();

    [Fact]
    public async Task Should_execute_and_capture_console_output()
    {
        var code = """
            using System;
            Console.WriteLine("Hello from sandbox!");
            """;

        var compilation = _compiler.Compile(code);
        compilation.Success.Should().BeTrue();

        var result = await _executor.ExecuteAsync(compilation.AssemblyBytes!);

        result.Success.Should().BeTrue();
        result.ConsoleOutput.Should().Contain("Hello from sandbox!");
    }

    [Fact]
    public async Task Should_capture_runtime_exceptions()
    {
        var code = """
            using System;
            throw new InvalidOperationException("Test runtime error");
            """;

        var compilation = _compiler.Compile(code);
        compilation.Success.Should().BeTrue();

        var result = await _executor.ExecuteAsync(compilation.AssemblyBytes!);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Test runtime error");
        result.ExceptionType.Should().Contain("InvalidOperationException");
    }

    [Fact]
    public async Task Should_execute_linq_computations()
    {
        var code = """
            using System;
            using System.Linq;

            var primes = Enumerable.Range(2, 20)
                .Where(n => Enumerable.Range(2, (int)Math.Sqrt(n) - 1)
                    .All(d => n % d != 0))
                .ToList();

            Console.WriteLine(string.Join(", ", primes));
            """;

        var compilation = _compiler.Compile(code);
        compilation.Success.Should().BeTrue();

        var result = await _executor.ExecuteAsync(compilation.AssemblyBytes!);

        result.Success.Should().BeTrue();
        result.ConsoleOutput.Should().Contain("2");
        result.ConsoleOutput.Should().Contain("19");
    }
}

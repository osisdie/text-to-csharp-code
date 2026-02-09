using FluentAssertions;
using TextToCode.Infrastructure.Compilation;
using Xunit;

namespace TextToCode.Infrastructure.Tests.Compilation;

public class RoslynCodeCompilerTests
{
    private readonly RoslynCodeCompiler _compiler = new();

    [Fact]
    public void Should_compile_valid_hello_world()
    {
        var code = """
            using System;
            Console.WriteLine("Hello, World!");
            """;

        var result = _compiler.Compile(code);

        result.Success.Should().BeTrue();
        result.AssemblyBytes.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Should_compile_top_level_statements_with_linq()
    {
        var code = """
            using System;
            using System.Linq;

            var numbers = Enumerable.Range(1, 10);
            var sum = numbers.Sum();
            Console.WriteLine($"Sum: {sum}");
            """;

        var result = _compiler.Compile(code);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public void Should_fail_on_syntax_errors()
    {
        var code = """
            Console.WriteLine("missing import and broken syntax
            """;

        var result = _compiler.Compile(code);

        result.Success.Should().BeFalse();
        result.Diagnostics.Should().NotBeEmpty();
    }

    [Fact]
    public void Should_fail_on_undefined_references()
    {
        var code = """
            using System;
            var foo = new NonExistentClass();
            """;

        var result = _compiler.Compile(code);

        result.Success.Should().BeFalse();
        result.Diagnostics.Should().Contain(d => d.Message.Contains("NonExistentClass"));
    }

    [Fact]
    public void Should_compile_class_with_main_method()
    {
        var code = """
            using System;

            class Program
            {
                static void Main(string[] args)
                {
                    Console.WriteLine("From Main");
                }
            }
            """;

        var result = _compiler.Compile(code);

        result.Success.Should().BeTrue();
    }
}

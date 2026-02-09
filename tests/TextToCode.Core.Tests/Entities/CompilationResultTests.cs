using FluentAssertions;
using TextToCode.Core.Entities;
using TextToCode.Core.ValueObjects;
using Xunit;

namespace TextToCode.Core.Tests.Entities;

public class CompilationResultTests
{
    [Fact]
    public void Succeeded_should_contain_assembly_bytes()
    {
        var bytes = new byte[] { 1, 2, 3 };
        var result = CompilationResult.Succeeded(bytes, []);

        result.Success.Should().BeTrue();
        result.AssemblyBytes.Should().BeEquivalentTo(bytes);
        result.Diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void Failed_should_have_no_assembly_bytes()
    {
        var diags = new List<CompilerDiagnostic>
        {
            new("CS0001", "Test error", "Error", 1, 1)
        };

        var result = CompilationResult.Failed(diags);

        result.Success.Should().BeFalse();
        result.AssemblyBytes.Should().BeNull();
        result.Diagnostics.Should().HaveCount(1);
    }

    [Fact]
    public void GetDiagnosticsSummary_should_format_diagnostics()
    {
        var diags = new List<CompilerDiagnostic>
        {
            new("CS1001", "Missing semicolon", "Error", 5, 10)
        };

        var result = CompilationResult.Failed(diags);
        var summary = result.GetDiagnosticsSummary();

        summary.Should().Contain("CS1001");
        summary.Should().Contain("Missing semicolon");
        summary.Should().Contain("(5,10)");
    }
}

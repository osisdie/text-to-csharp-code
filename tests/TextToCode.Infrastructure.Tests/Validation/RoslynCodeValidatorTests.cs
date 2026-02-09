using FluentAssertions;
using TextToCode.Infrastructure.Validation;
using Xunit;

namespace TextToCode.Infrastructure.Tests.Validation;

public class RoslynCodeValidatorTests
{
    private readonly RoslynCodeValidator _validator = new();

    [Fact]
    public void Should_accept_safe_code()
    {
        var code = """
            using System;
            using System.Linq;
            Console.WriteLine("Safe code");
            """;

        var result = _validator.Validate(code);

        result.IsValid.Should().BeTrue();
        result.Violations.Should().BeEmpty();
    }

    [Fact]
    public void Should_reject_process_start()
    {
        var code = """
            using System;
            using System.Diagnostics;
            Process.Start("malicious.exe");
            """;

        var result = _validator.Validate(code);

        result.IsValid.Should().BeFalse();
        result.Violations.Should().Contain(v => v.Rule == "DangerousApi");
    }

    [Fact]
    public void Should_reject_file_operations()
    {
        var code = """
            using System;
            using System.IO;
            File.Delete("/etc/passwd");
            """;

        var result = _validator.Validate(code);

        result.IsValid.Should().BeFalse();
        result.Violations.Should().Contain(v => v.Description.Contains("File.Delete"));
    }

    [Fact]
    public void Should_reject_network_namespaces()
    {
        var code = """
            using System.Net.Http;
            var client = new HttpClient();
            """;

        var result = _validator.Validate(code);

        result.IsValid.Should().BeFalse();
        result.Violations.Should().Contain(v => v.Rule == "DangerousNamespace" || v.Rule == "DangerousType");
    }

    [Fact]
    public void Should_reject_empty_code()
    {
        var result = _validator.Validate("");

        result.IsValid.Should().BeFalse();
        result.Violations.Should().Contain(v => v.Rule == "EmptyCode");
    }

    [Fact]
    public void Should_reject_syntax_errors()
    {
        var code = """
            using System;
            Console.WriteLine("unclosed string
            """;

        var result = _validator.Validate(code);

        result.IsValid.Should().BeFalse();
        result.Violations.Should().Contain(v => v.Rule.StartsWith("SyntaxError"));
    }

    [Fact]
    public void Should_reject_environment_exit()
    {
        var code = """
            using System;
            Environment.Exit(0);
            """;

        var result = _validator.Validate(code);

        result.IsValid.Should().BeFalse();
        result.Violations.Should().Contain(v => v.Description.Contains("Environment.Exit"));
    }
}

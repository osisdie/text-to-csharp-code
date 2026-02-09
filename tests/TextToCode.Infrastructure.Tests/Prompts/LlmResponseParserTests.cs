using FluentAssertions;
using TextToCode.Infrastructure.Prompts;
using Xunit;

namespace TextToCode.Infrastructure.Tests.Prompts;

public class LlmResponseParserTests
{
    [Fact]
    public void Should_extract_code_from_csharp_fence()
    {
        var response = """
            Here's the code:

            ```csharp
            Console.WriteLine("Hello");
            ```

            This will print Hello.
            """;

        var code = LlmResponseParser.ExtractCode(response);

        code.Should().Be("Console.WriteLine(\"Hello\");");
    }

    [Fact]
    public void Should_extract_code_from_generic_fence()
    {
        var response = """
            ```
            Console.WriteLine("Generic fence");
            ```
            """;

        var code = LlmResponseParser.ExtractCode(response);

        code.Should().Be("Console.WriteLine(\"Generic fence\");");
    }

    [Fact]
    public void Should_return_raw_text_when_no_fences()
    {
        var response = "Console.WriteLine(\"No fences\");";

        var code = LlmResponseParser.ExtractCode(response);

        code.Should().Be("Console.WriteLine(\"No fences\");");
    }

    [Fact]
    public void Should_prefer_csharp_fence_over_generic()
    {
        var response = """
            ```csharp
            Console.WriteLine("CSharp");
            ```

            ```
            Console.WriteLine("Generic");
            ```
            """;

        var code = LlmResponseParser.ExtractCode(response);

        code.Should().Contain("CSharp");
    }

    [Fact]
    public void Should_throw_on_empty_response()
    {
        var act = () => LlmResponseParser.ExtractCode("");
        act.Should().Throw<ArgumentException>();
    }
}

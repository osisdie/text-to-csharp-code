using FluentAssertions;
using TextToCode.Core.ValueObjects;
using Xunit;

namespace TextToCode.Core.Tests.ValueObjects;

public class LlmModelIdentifierTests
{
    [Fact]
    public void Should_parse_provider_and_model_from_full_id()
    {
        var id = new LlmModelIdentifier("anthropic/claude-sonnet-4");

        id.Provider.Should().Be("anthropic");
        id.Model.Should().Be("claude-sonnet-4");
        id.FullId.Should().Be("anthropic/claude-sonnet-4");
    }

    [Fact]
    public void Should_handle_model_without_provider()
    {
        var id = new LlmModelIdentifier("gpt-4o");

        id.Provider.Should().BeEmpty();
        id.Model.Should().Be("gpt-4o");
    }

    [Fact]
    public void Should_throw_on_null_or_empty()
    {
        var act = () => new LlmModelIdentifier("");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Should_display_full_id_in_ToString()
    {
        var id = new LlmModelIdentifier("openai/gpt-4");
        id.ToString().Should().Be("openai/gpt-4");
    }
}

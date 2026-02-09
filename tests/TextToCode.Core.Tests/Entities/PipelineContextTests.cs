using FluentAssertions;
using TextToCode.Core.Entities;
using TextToCode.Core.Enums;
using Xunit;

namespace TextToCode.Core.Tests.Entities;

public class PipelineContextTests
{
    [Fact]
    public void Should_initialize_with_pending_status()
    {
        var ctx = new PipelineContext("Write hello world");

        ctx.Status.Should().Be(PipelineStatus.Pending);
        ctx.UserPrompt.Should().Be("Write hello world");
        ctx.AttemptNumber.Should().Be(0);
    }

    [Fact]
    public void Should_track_status_transitions()
    {
        var ctx = new PipelineContext("test");

        ctx.SetGenerating(1);
        ctx.Status.Should().Be(PipelineStatus.Generating);
        ctx.AttemptNumber.Should().Be(1);

        ctx.SetGeneratedCode("Console.WriteLine(\"hi\");");
        ctx.GeneratedCode.Should().NotBeNullOrEmpty();

        ctx.SetCompiling();
        ctx.Status.Should().Be(PipelineStatus.Compiling);

        ctx.SetCompleted();
        ctx.Status.Should().Be(PipelineStatus.Completed);
        ctx.CompletedAt.Should().NotBeNull();
        ctx.TotalDuration.Should().NotBeNull();
    }

    [Fact]
    public void Should_accumulate_diagnostic_history()
    {
        var ctx = new PipelineContext("test");

        ctx.SetSelfHealing("Error 1");
        ctx.SetSelfHealing("Error 2");

        ctx.DiagnosticHistory.Should().HaveCount(2);
        ctx.DiagnosticHistory[0].Should().Be("Error 1");
    }

    [Fact]
    public void Should_throw_on_empty_prompt()
    {
        var act = () => new PipelineContext("");
        act.Should().Throw<ArgumentException>();
    }
}

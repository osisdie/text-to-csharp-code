namespace TextToCode.Core.Entities;

public sealed class ExecutionResult
{
    public bool Success { get; }
    public string ConsoleOutput { get; }
    public string? ErrorMessage { get; }
    public string? ExceptionType { get; }
    public TimeSpan Duration { get; }

    private ExecutionResult(bool success, string consoleOutput, string? errorMessage, string? exceptionType, TimeSpan duration)
    {
        Success = success;
        ConsoleOutput = consoleOutput;
        ErrorMessage = errorMessage;
        ExceptionType = exceptionType;
        Duration = duration;
    }

    public static ExecutionResult Succeeded(string consoleOutput, TimeSpan duration) =>
        new(true, consoleOutput, null, null, duration);

    public static ExecutionResult Failed(string consoleOutput, string errorMessage, string? exceptionType, TimeSpan duration) =>
        new(false, consoleOutput, errorMessage, exceptionType, duration);
}

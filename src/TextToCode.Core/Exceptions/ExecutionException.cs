namespace TextToCode.Core.Exceptions;

public class ExecutionException : CodeGenerationException
{
    public string? ExceptionType { get; }

    public ExecutionException(string message, string? exceptionType = null)
        : base(message)
    {
        ExceptionType = exceptionType;
    }

    public ExecutionException(string message, Exception innerException, string? exceptionType = null)
        : base(message, innerException)
    {
        ExceptionType = exceptionType;
    }
}

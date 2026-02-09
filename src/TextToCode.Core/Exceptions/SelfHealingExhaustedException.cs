namespace TextToCode.Core.Exceptions;

public class SelfHealingExhaustedException : CodeGenerationException
{
    public int AttemptsExhausted { get; }

    public SelfHealingExhaustedException(int attempts, string lastError)
        : base($"Self-healing exhausted after {attempts} attempts. Last error: {lastError}")
    {
        AttemptsExhausted = attempts;
    }
}

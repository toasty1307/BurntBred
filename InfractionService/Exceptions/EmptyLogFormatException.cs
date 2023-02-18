namespace InfractionService.Exceptions;

public sealed class EmptyLogFormatException : Exception
{
    public EmptyLogFormatException(string message) : base(message)
    {
    }
}
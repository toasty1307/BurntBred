namespace InfractionService.Exceptions;

public sealed class InvalidConfigException : Exception
{
    public InvalidConfigException(string message) : base(message)
    {
    }
}
namespace MyEstore.Exceptions;

public class ValidationException : AppException
{
    public ValidationException(string message) : base(message)
    {
    }
}

namespace Wedding_Proposal_BE.Shared.Exceptions;

public class ValidationException : AppException
{
    public ValidationException(string message)
        : base(message, "VALIDATION_ERROR", StatusCodes.Status400BadRequest)
    {
    }
}

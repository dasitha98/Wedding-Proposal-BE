namespace Wedding_Proposal_BE.Shared.Exceptions;

public class NotFoundException : AppException
{
    public NotFoundException(string message)
        : base(message, "NOT_FOUND", StatusCodes.Status404NotFound)
    {
    }
}

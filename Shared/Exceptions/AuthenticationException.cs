namespace Wedding_Proposal_BE.Shared.Exceptions;

public class AuthenticationException : AppException
{
    public AuthenticationException(string message)
        : base(message, "AUTHENTICATION_ERROR", StatusCodes.Status401Unauthorized)
    {
    }
}

namespace Wedding_Proposal_BE.Shared.Exceptions;

public abstract class AppException : Exception
{
    public string ErrorCode { get; }
    public int StatusCode { get; }

    protected AppException(string message, string errorCode, int statusCode)
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
}

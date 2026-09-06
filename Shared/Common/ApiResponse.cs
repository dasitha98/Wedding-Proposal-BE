namespace Wedding_Proposal_BE.Shared.Common;

public class ApiResponse<T>
{
    public bool Success { get; }
    public T? Data { get; }
    public string? Error { get; }
    public string? ErrorCode { get; }

    private ApiResponse(bool success, T? data, string? error, string? errorCode)
    {
        Success = success;
        Data = data;
        Error = error;
        ErrorCode = errorCode;
    }

    public static ApiResponse<T> Ok(T data) => new(true, data, null, null);

    public static ApiResponse<T> Fail(string error, string? errorCode = null) =>
        new(false, default, error, errorCode);
}

public class ApiResponse
{
    public bool Success { get; }
    public string? Error { get; }
    public string? ErrorCode { get; }

    private ApiResponse(bool success, string? error, string? errorCode)
    {
        Success = success;
        Error = error;
        ErrorCode = errorCode;
    }

    public static ApiResponse Ok() => new(true, null, null);

    public static ApiResponse Fail(string error, string? errorCode = null) =>
        new(false, error, errorCode);
}

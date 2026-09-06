namespace Wedding_Proposal_BE.Shared.Common;

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public string? ErrorCode { get; }

    protected Result(bool isSuccess, string? error, string? errorCode)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;
    }

    public bool IsFailure => !IsSuccess;

    public static Result Success() => new(true, null, null);

    public static Result Failure(string error, string? errorCode = null) =>
        new(false, error, errorCode);

    public static Result<T> Success<T>(T value) => Result<T>.Success(value);

    public static Result<T> Failure<T>(string error, string? errorCode = null) =>
        Result<T>.Failure(error, errorCode);
}

public class Result<T> : Result
{
    private readonly T? _value;

    private Result(bool isSuccess, T? value, string? error, string? errorCode)
        : base(isSuccess, error, errorCode)
    {
        _value = value;
    }

    public T Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException("Cannot access the value of a failed result.");

    public static Result<T> Success(T value) => new(true, value, null, null);

    public new static Result<T> Failure(string error, string? errorCode = null) =>
        new(false, default, error, errorCode);
}

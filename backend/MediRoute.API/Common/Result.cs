namespace MediRoute.API.Common;

/// <summary>Generic Result wrapper for service-layer operations (no exceptions for expected failures).</summary>
public class Result
{
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }
    public int StatusCode { get; init; } = 200;

    public static Result Success() => new() { IsSuccess = true };
    public static Result Failure(string error, int statusCode = 400) =>
        new() { IsSuccess = false, Error = error, StatusCode = statusCode };
}

public class Result<T> : Result
{
    public T? Value { get; init; }

    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public new static Result<T> Failure(string error, int statusCode = 400) =>
        new() { IsSuccess = false, Error = error, StatusCode = statusCode };
}

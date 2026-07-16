using Microsoft.AspNetCore.Http;

namespace BancoCenit.Features;

public sealed class OperationResult<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public string? Error { get; }
    public int StatusCode { get; }

    private OperationResult(bool isSuccess, T? value, string? error, int statusCode)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        StatusCode = statusCode;
    }

    public static OperationResult<T> Ok(T value) => new(true, value, null, StatusCodes.Status200OK);
    public static OperationResult<T> BadRequest(string error) => new(false, default, error, StatusCodes.Status400BadRequest);
    public static OperationResult<T> NotFound(string error) => new(false, default, error, StatusCodes.Status404NotFound);
    public static OperationResult<T> Fail(int statusCode, string error) => new(false, default, error, statusCode);

    public OperationResult<U> Map<U>(Func<T, U> mapper)
    {
        return IsSuccess
            ? OperationResult<U>.Ok(mapper(Value!))
            : OperationResult<U>.Fail(StatusCode, Error!);
    }

    public async Task<OperationResult<U>> MapAsync<U>(Func<T, Task<U>> mapper)
    {
        return IsSuccess
            ? OperationResult<U>.Ok(await mapper(Value!))
            : OperationResult<U>.Fail(StatusCode, Error!);
    }

    public OperationResult<U> Bind<U>(Func<T, OperationResult<U>> binder)
    {
        return IsSuccess
            ? binder(Value!)
            : OperationResult<U>.Fail(StatusCode, Error!);
    }

    public async Task<OperationResult<U>> BindAsync<U>(Func<T, Task<OperationResult<U>>> binder)
    {
        return IsSuccess
            ? await binder(Value!)
            : OperationResult<U>.Fail(StatusCode, Error!);
    }
}

public static class OperationResultExtensions
{
    public static async Task<OperationResult<U>> MapAsync<T, U>(this Task<OperationResult<T>> resultTask, Func<T, U> mapper)
    {
        OperationResult<T> result = await resultTask;
        return result.Map(mapper);
    }

    public static async Task<OperationResult<U>> BindAsync<T, U>(this Task<OperationResult<T>> resultTask, Func<T, Task<OperationResult<U>>> binder)
    {
        OperationResult<T> result = await resultTask;
        return await result.BindAsync(binder);
    }
}

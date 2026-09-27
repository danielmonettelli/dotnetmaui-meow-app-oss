namespace Meow.Core.Common;

/// <summary>
/// Generic result wrapper for operation outcomes with proper error handling
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string? ErrorMessage { get; }
    public ResultSource Source { get; }

    private Result(bool isSuccess, T? data, string? errorMessage, ResultSource source)
    {
        IsSuccess = isSuccess;
        Data = data;
        ErrorMessage = errorMessage;
        Source = source;
    }

    public static Result<T> Success(T data, ResultSource source = ResultSource.Api)
        => new(true, data, null, source);

    public static Result<T> Failure(string errorMessage, T? fallbackData = default, ResultSource source = ResultSource.None)
        => new(fallbackData is not null, fallbackData, errorMessage, source);

    public static Result<T> FromCache(T data)
        => new(true, data, null, ResultSource.Cache);

    public static Result<T> Offline(T? cachedData = default)
        => new(cachedData is not null, cachedData, "No internet connection. Showing cached data.", ResultSource.Cache);
}

/// <summary>
/// Indicates where the data came from
/// </summary>
public enum ResultSource
{
    None,
    Api,
    Cache
}

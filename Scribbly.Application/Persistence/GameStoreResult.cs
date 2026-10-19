namespace Scribbly.Application.Persistence;

public record GameStoreResult<T>(bool Found, T? Value)
{
    public static GameStoreResult<T> Missing() => new(false, default);
    public static GameStoreResult<T> Success(T value) => new(true, value);
}

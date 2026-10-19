namespace Scribbly.Application.Abstractions.Persistence;

public interface IGameConnectionIndex
{
    Task<string?> FindGameHashAsync(string connectionId, CancellationToken cancellationToken = default);

    Task RemoveAsync(string connectionId, string expectedGameHash, CancellationToken cancellationToken = default);
}

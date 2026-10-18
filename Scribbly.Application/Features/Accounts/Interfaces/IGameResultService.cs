namespace Scribbly.Application.Features.Accounts;

public interface IGameResultService
{
    Task<bool> CaptureAsync(string gameHash, CancellationToken cancellationToken);
}

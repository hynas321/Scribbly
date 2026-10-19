namespace Scribbly.Application.GameSessions;

public class RandomDrawingPlayerSelector : IDrawingPlayerSelector
{
    public string Select(IReadOnlyCollection<string> availableCredentialHashes)
    {
        ArgumentNullException.ThrowIfNull(availableCredentialHashes);
        if (availableCredentialHashes.Count == 0)
        {
            throw new ArgumentException(
                "At least one drawing-player credential hash is required.",
                nameof(availableCredentialHashes));
        }

        int index = Random.Shared.Next(availableCredentialHashes.Count);
        return availableCredentialHashes.ElementAt(index);
    }
}

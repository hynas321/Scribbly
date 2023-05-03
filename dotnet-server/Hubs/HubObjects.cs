using Dotnet.Server.Models;

namespace Dotnet.Server.Hubs;

class JoinGameResponse {
    public string GameHash { get; set; }
    public List<PlayerScore> playerScores { get; set; }
}
using Dotnet.Server.Models;

namespace Dotnet.Server.Hubs;

class JoinGameResponse {
    public List<PlayerScore> playerScores { get; set; }
}
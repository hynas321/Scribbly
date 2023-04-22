using Dotnet.Server.Models;

namespace Dotnet.Server.Managers;

class LobbyManager
{
    private int maxChatMessageCount = 30;
    private static Dictionary<string, Lobby> lobbies = new Dictionary<string, Lobby>()
    {
        { "TestLobbyUrl", new Lobby() }
    };

    public void AddLobby(string lobbyUrl, Lobby lobby)
    {
        lobbies.Add(lobbyUrl, lobby);
    }

    public void RemoveLobby(string lobbyUrl)
    {
        lobbies.Remove(lobbyUrl);
    }

    public int GetLobbiesCount()
    {
        return lobbies.Count;
    }

    public Lobby GetLobby(string lobbyUrl)
    {
        return lobbies[lobbyUrl];
    }

    public void AddPlayerToLobby(string lobbyUrl, Player player)
    {
        lobbies[lobbyUrl].players.Add(player);
    }

    public void RemovePlayerFromLobby(string lobbyUrl, string username)
    {
        lobbies[lobbyUrl].players.RemoveAll(p => p.Username == username);
    }

    public int GetPlayersCount(string lobbyUrl)
    {
        return lobbies[lobbyUrl].players.Count();
    }

    public List<Player> GetPlayers(string lobbyUrl)
    {
        return lobbies[lobbyUrl].players;
    }

    public void AddChatMessage(string lobbyUrl, ChatMessage message)
    {
        List<ChatMessage> messages = lobbies[lobbyUrl].chatMessages;

        if (messages.Count == maxChatMessageCount)
        {
            messages.RemoveAt(0);
        }

        messages.Add(message);
        
    }

    public List<ChatMessage> GetMessages(string lobbyUrl)
    {
        return lobbies[lobbyUrl].chatMessages;
    }
}
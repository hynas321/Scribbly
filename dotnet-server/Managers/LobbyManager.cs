using Dotnet.Server.Models;

namespace Dotnet.Server.Managers;

class LobbyManager
{
    private int maxChatMessageCount = 30;
    private static Dictionary<string, Lobby> lobbies = new Dictionary<string, Lobby>()
    {
        { "TestLobbyHash", new Lobby() }
    };

    public void AddLobby(string lobbyHash, Lobby lobby)
    {
        lobbies.Add(lobbyHash, lobby);
    }

    public void RemoveLobby(string lobbyHash)
    {
        lobbies.Remove(lobbyHash);
    }

    public int GetLobbiesCount()
    {
        return lobbies.Count;
    }

    public Lobby GetLobby(string lobbyHash)
    {
        return lobbies[lobbyHash];
    }

    public void AddPlayerToLobby(string lobbyHash, Player player)
    {
        lobbies[lobbyHash].Players.Add(player);
    }

    public void RemovePlayerFromLobby(string lobbyHash, string username)
    {
        lobbies[lobbyHash].Players.RemoveAll(p => p.Username == username);
    }

    public int GetPlayersCount(string lobbyHash)
    {
        return lobbies[lobbyHash].Players.Count();
    }

    public List<Player> GetPlayers(string lobbyHash)
    {
        return lobbies[lobbyHash].Players;
    }

    public void AddChatMessage(string lobbyHash, ChatMessage message)
    {
        List<ChatMessage> messages = lobbies[lobbyHash].ChatMessages;

        if (messages.Count == maxChatMessageCount)
        {
            messages.RemoveAt(0);
        }

        messages.Add(message);
        
    }

    public List<ChatMessage> GetMessages(string lobbyHash)
    {
        return lobbies[lobbyHash].ChatMessages;
    }

    public GameSettings GetGameSettings(string lobbyHash)
    {   
        return lobbies[lobbyHash].GameSettings;
    }

    public void ChangeGameSettings(string lobbyHash, GameSettings settings)
    {
        lobbies[lobbyHash].GameSettings = settings;
    }
}
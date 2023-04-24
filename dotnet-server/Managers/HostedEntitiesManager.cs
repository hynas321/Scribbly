using Dotnet.Server.Models;

namespace Dotnet.Server.Managers;

abstract class HostedEntitiesManager<T> where T : HostedEntity
{
    private int maxChatMessageCount;
    public static List<T> entityList = new List<T>();
    
    public HostedEntitiesManager(int maxChatMessageCount)
    {
        this.maxChatMessageCount = maxChatMessageCount;
    }

    public void Add(T entity)
    {
        entityList.Add(entity);
    }

    public void Remove(string hash)
    {
        entityList.RemoveAll(obj => obj.Hash == hash);
    }

    public int GetCount()
    {
        return entityList.Count;
    }

    public T Get(string hash)
    {
        return entityList.Find(obj => obj.Hash == hash);
    }

    public void AddPlayer(string hash, Player player)
    {   
        Get(hash).Players.Add(player);
    }

    public void RemovePlayer(string hash, string username)
    {
       Get(hash).Players.RemoveAll(p => p.Username == username);
        
    }

    public int GetPlayersCount(string hash)
    {
        return Get(hash).Players.Count;
    }

    public List<Player> GetPlayers(string hash)
    {
        return Get(hash).Players;
    }

    public void AddChatMessage(string hash, ChatMessage message)
    {
        List<ChatMessage> messages = Get(hash).ChatMessages;

        if (messages.Count == maxChatMessageCount)
        {
            messages.RemoveAt(0);
        }

        messages.Add(message);
        
    }

    public List<ChatMessage> GetMessages(string hash)
    {
        return Get(hash).ChatMessages;
    }

    public GameSettings GetGameSettings(string hash)
    {      
        return Get(hash).GameSettings;
    }

    public void ChangeGameSettings(string hash, GameSettings settings)
    {
        Get(hash).GameSettings = settings;
    }
}
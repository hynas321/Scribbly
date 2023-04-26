namespace Dotnet.Server.Hubs;

public static class HubEvents
{
    //Lobby
    public const string JoinLobby = "JoinLobby";
    public const string LeaveLobby = "LeaveLobby";
    public const string StartGame = "StartGame";
    public const string OnPlayerJoinedLobby = "OnPlayerJoinedLobby";
    public const string OnPlayerLeftLobby = "OnPlayerLeftLobby";
    public const string OnStartGame = "OnStartGame";
    
    //GameSettings
    public const string LoadGameSettings = "LoadGameSettings";
    public const string SetAbstractNouns = "SetAbstractNouns";
    public const string SetDrawingTimeSeconds = "SetDrawingTimeSeconds";
    public const string SetRoundsCount = "SetRoundsCount";
    public const string SetWordLanguage = "SetWordLanguage";
    public const string OnLoadGameSettings = "OnLoadGameSettings";
    public const string OnSetAbstractNouns = "OnSetAbstractNouns";
    public const string OnSetDrawingTimeSeconds = "OnSetDrawingTimeSeconds";
    public const string OnSetRoundsCount = "OnSetRoundsCount";
    public const string OnSetWordLanguage = "OnSetWordLanguage";

    //Chat
    public const string SendChatMessage = "SendChatMessage";
    public const string LoadChatMessages = "LoadChatMessages";
    public const string OnLoadChatMessages = "OnLoadChatMessages";

    //Game
    public const string JoinGame = "JoinGame";
    public const string LeaveGame = "LeaveGame";
    public const string OnPlayerJoinedGame = "OnPlayerJoinedGame";
    public const string OnPlayerLeftGame = "OnPlayerLeftGame";

    // Canvas
    public const string LoadCanvas = "LoadCanvas";
    public const string OnLoadCanvas = "OnLoadCanvas";
    public const string DrawOnCanvas = "DrawnOnCanvas";
    public const string OnDrawOnCanvas = "OnDrawOnCanvas";
    public const string ClearCanvas = "ClearCanvas";
    public const string OnClearCanvas = "OnClearCanvas";
}
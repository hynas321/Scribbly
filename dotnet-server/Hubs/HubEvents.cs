namespace Dotnet.Server.Hubs;

public static class HubEvents
{
    
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
    public const string OnSendChatMessage = "OnSendChatMessage";
    public const string OnLoadChatMessages = "OnLoadChatMessages";

    //Game
    public const string StartGame = "StartGame";
    public const string JoinGame = "JoinGame";
    public const string LeaveGame = "LeaveGame";
    public const string OnPlayerJoinedGame = "OnPlayerJoinedGame";
    public const string OnPlayerLeftGame = "OnPlayerLeftGame";
    public const string OnStartGame = "OnStartGame";

    // Canvas
    public const string LoadCanvas = "LoadCanvas";
    public const string OnLoadCanvas = "OnLoadCanvas";
    public const string DrawOnCanvas = "DrawnOnCanvas";
    public const string OnDrawOnCanvas = "OnDrawOnCanvas";
    public const string ClearCanvas = "ClearCanvas";
    public const string OnClearCanvas = "OnClearCanvas";
}
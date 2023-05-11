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
    public const string OnSendAnnouncement = "OnSendAnnouncement";

    //Game
    public const string JoinGame = "JoinGame";
    public const string LeaveGame = "LeaveGame";
    public const string FindPlayerHost = "FindPlayerHost";
    public const string OnJoinGame = "OnJoinGame";
    public const string OnJoinGameError = "OnJoinGameError";
    public const string OnGameProblem = "OnGameProblem";
    public const string OnFindPlayerHost = "OnFindPlayerHost";
    public const string OnEndGame = "OnEndGame";

    //GameState
    public const string StartGame = "StartGame";
    public const string GetSecretWord = "GetSecretWord";
    public const string OnStartGame = "OnStartGame";
    public const string OnGetSecretWord = "OnGetSecretWord";
    public const string OnRequestSecretWord = "OnRequestSecretWord";
    public const string OnUpdatePlayerScores = "OnUpdatePlayerScores";
    public const string OnUpdateTimer = "OnUpdateTimer";
    public const string OnUpdateTimerVisibility = "OnUpdateTimerVisibility";
    public const string OnGameFinished = "OnGameFinished";
    public const string OnUpdateDrawingPlayer = "OnUpdateDrawingPlayer";
    public const string OnUpdateCurrentRound = "OnUpdateCurrentRound";
    
    // Canvas
    public const string LoadCanvas = "LoadCanvas";
    public const string DrawOnCanvas = "DrawOnCanvas";
    public const string ClearCanvas = "ClearCanvas";
    public const string OnLoadCanvas = "OnLoadCanvas";
    public const string OnDrawOnCanvas = "OnDrawOnCanvas";
    public const string OnClearCanvas = "OnClearCanvas";
    public const string OnSetCanvasText = "OnSetCanvasText";
}
namespace Scribbly.WebApi.Hubs.Static;

public static class HubMessages
{
    public const string JoinGame = "JoinGame";
    public const string ReconnectGame = "ReconnectGame";
    public const string LeaveGame = "LeaveGame";
    public const string StartGame = "StartGame";
    public const string GetPrompt = "GetPrompt";
    public const string SendChatMessage = "SendChatMessage";
    public const string LoadChatMessages = "LoadChatMessages";
    public const string KickPlayer = "KickPlayer";
    public const string SetDrawingTimeSeconds = "SetDrawingTimeSeconds";
    public const string SetRoundsCount = "SetRoundsCount";
    public const string SetPromptLanguage = "SetPromptLanguage";
    public const string SetMaxPlayers = "SetMaxPlayers";
    public const string SetPromptCategories = "SetPromptCategories";
    public const string SetHintSpeed = "SetHintSpeed";
    public const string SetHintPercentage = "SetHintPercentage";
    public const string LoadCanvas = "LoadCanvas";
    public const string AppendCanvasChunk = "AppendCanvasChunk";
    public const string ClearCanvas = "ClearCanvas";
    public const string UndoStroke = "UndoStroke";

    public const string OnGameProblem = "OnGameProblem";
    public const string OnGameStarted = "OnGameStarted";
    public const string OnGameEnded = "OnGameEnded";
    public const string OnGameSessionFailed = "OnGameSessionFailed";
    public const string OnChatMessage = "OnChatMessage";
    public const string OnAnnouncement = "OnAnnouncement";
    public const string OnPlayerScoresUpdated = "OnPlayerScoresUpdated";
    public const string OnTimerUpdated = "OnTimerUpdated";
    public const string OnTimerVisibilityUpdated = "OnTimerVisibilityUpdated";
    public const string OnDrawingPlayerUpdated = "OnDrawingPlayerUpdated";
    public const string OnCurrentRoundUpdated = "OnCurrentRoundUpdated";
    public const string OnCorrectGuessPlayersUpdated = "OnCorrectGuessPlayersUpdated";
    public const string OnPromptRefreshRequested = "OnPromptRefreshRequested";
    public const string OnCanvasChunk = "OnCanvasChunk";
    public const string OnCanvasCleared = "OnCanvasCleared";
    public const string OnCanvasStrokeUndone = "OnCanvasStrokeUndone";
    public const string OnCanvasText = "OnCanvasText";
    public const string OnDrawingTimeChanged = "OnDrawingTimeChanged";
    public const string OnRoundsCountChanged = "OnRoundsCountChanged";
    public const string OnPromptLanguageChanged = "OnPromptLanguageChanged";
    public const string OnPlayerKicked = "OnPlayerKicked";
    public const string OnHostChanged = "OnHostChanged";
    public const string OnMaxPlayersChanged = "OnMaxPlayersChanged";
    public const string OnPromptCategoriesChanged = "OnPromptCategoriesChanged";
    public const string OnHintSpeedChanged = "OnHintSpeedChanged";
    public const string OnHintPercentageChanged = "OnHintPercentageChanged";
}

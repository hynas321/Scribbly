class HubEvents
{
    //Lobby
    static joinLobby: string = "JoinLobby";
    static leaveLobby: string = "LeaveLobby";
    static startGame: string = "StartGame";
    static onPlayerJoinedLobby: string = "OnPlayerJoinedLobby";
    static onPlayerLeftLobby: string = "OnPlayerLeftLobby";
    static onStartGame: string = "OnStartGame";

    //GameSettings
    static LoadGameSettings: string = "LoadGameSettings";
    static setAbstractNouns: string = "SetAbstractNouns";
    static setDrawingTimeSeconds: string = "SetDrawingTimeSeconds";
    static setRoundsCount: string = "SetRoundsCount";
    static setWordLanguage: string = "SetWordLanguage";
    static onLoadGameSettings: string = "OnLoadGameSettings";
    static onSetAbstractNouns: string = "OnSetAbstractNouns";
    static onSetDrawingTimeSeconds: string = "OnSetDrawingTimeSeconds";
    static onSetRoundsCount: string = "OnSetRoundsCount";
    static onSetWordLanguage: string = "OnSetWordLanguage";

    //Chat
    static sendChatMessage: string = "SendChatMessage";
    static loadChatMessages: string = "LoadChatMessages";
    static onLoadChatMessages: string = "OnLoadChatMessages";

    //Game
    static joinGame: string = "JoinGame";
    static leaveGame: string = "LeaveGame";
    static onPlayerJoinedGame: string = "OnPlayerJoinedGame";
    static onPlayerLeftGame: string = "OnPlayerLeftGame";
    
    //Canvas
    static loadCanvas: string = "LoadCanvas";
    static drawOnCanvas: string = "DrawnOnCanvas";
    static clearCanvas: string = "ClearCanvas";
    static onLoadCanvas: string = "OnLoadCanvas";
    static onDrawOnCanvas: string = "OnDrawOnCanvas";
    static onClearCanvas: string = "OnClearCanvas";
}

export default HubEvents;
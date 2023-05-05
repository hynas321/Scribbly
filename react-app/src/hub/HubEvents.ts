class HubEvents {

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
    static onSendChatMessage: string = "OnSendChatMessage";
    static onLoadChatMessages: string = "OnLoadChatMessages";

    //Game

    static joinGame: string = "JoinGame";
    static leaveGame: string = "LeaveGame";
    static findPlayerHost: string = "FindPlayerHost";
    static onPlayerJoinedGame: string = "OnPlayerJoinedGame";
    static onPlayerLeftGame: string = "OnPlayerLeftGame";
    static onJoinGame: string = "OnJoinGame";
    static onFindPlayerHost: string = "OnFindPlayerHost";

    //GameState
    static startGame: string = "StartGame";
    static startTimer: string = "StartTimer";
    static onStartGame: string = "onStartGame";
    static onStartTimer: string = "onStartTimer";
    //Canvas
    static loadCanvas: string = "LoadCanvas";
    static drawOnCanvas: string = "DrawOnCanvas";
    static clearCanvas: string = "ClearCanvas";
    static onLoadCanvas: string = "OnLoadCanvas";
    static onDrawOnCanvas: string = "OnDrawOnCanvas";
    static onClearCanvas: string = "OnClearCanvas";
}

export default HubEvents;
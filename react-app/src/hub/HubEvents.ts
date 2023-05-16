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
  static onSendAnnouncement: string = "OnSendAnnouncement";

  //Game
  static joinGame: string = "JoinGame";
  static leaveGame: string = "LeaveGame";
  static findPlayerHost: string = "FindPlayerHost";
  static onGameProblem: string = "OnGameProblem";
  static onJoinGame: string = "OnJoinGame";
  static onJoinGameError: string = "OnJoinGameError";
  static onFindPlayerHost: string = "OnFindPlayerHost";
  static onEndGame: string = "OnEndGame";

  //GameState
  static startGame: string = "StartGame";
  static getSecretWord: string = "GetSecretWord";
  static onStartGame: string = "OnStartGame";
  static onGetSecretWord: string = "OnGetSecretWord";
  static onRequestSecretWord: string = "OnRequestSecretWord";
  static onUpdatePlayerScores: string = "OnUpdatePlayerScores";
  static onUpdateTimer: string = "OnUpdateTimer";
  static onUpdateTimerVisibility: string = "OnUpdateTimerVisibility";
  static onUpdateDrawingPlayer: string = "OnUpdateDrawingPlayer";
  static onUpdateCurrentRound: string = "OnUpdateCurrentRound";
  static onUpdateCorrectGuessPlayerUsernames: string = "OnUpdateCorrectGuessPlayerUsernames";

  //Canvas
  static loadCanvas: string = "LoadCanvas";
  static drawOnCanvas: string = "DrawOnCanvas";
  static clearCanvas: string = "ClearCanvas";
  static undoLine: string = "UndoLine";
  static onLoadCanvas: string = "OnLoadCanvas";
  static onDrawOnCanvas: string = "OnDrawOnCanvas";
  static onClearCanvas: string = "OnClearCanvas";
  static OnSetCanvasText: string = "OnSetCanvasText";

  //Account
  static createSession = "CreateSession";
  static endSession = "EndSession";  
  static onSessionEnded = "OnSessionEnded";
  static onUpdateAccountScore: string = "OnUpdateAccountScore";

}

export default HubEvents;
class ApiEndpoints {
  //Hub connections
  static hubConnectionEndpoint: string = "/hub/connection";
  static longRunningHubConnectionEndpoint: string = "/long-running-hub/connection";
  static accountHubConnectionEndpoint: string = "/account-hub/connection";

  //GameController
  static gameCreate: string = "/api/Game/Create";
  static gameExists: string = "/api/Game/Exists";

  //PlayerController
  static playerJoinGame: string = "/api/Player/JoinGame";
  static playerExists: string = "/api/Player/Exists";

  //AccountController
  static accountAddIfNotExists: string = "/api/Account/Add";
  static accountIncrementScore: string = "/api/Account/IncrementScore";
  static accountGetScore: string = "/api/Account/GetScore";
  static accountGetTopScores: string = "/api/Account/GetTop";
}

export default ApiEndpoints;
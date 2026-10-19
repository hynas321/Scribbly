class ApiEndpoints {
  static hubConnectionEndpoint = "/hub/connection";
  static gameCreate = "/api/Game/Create";
  static gameExists = "/api/Game/Exists";
  static gameCompleted = "/api/Game/Completed";
  static gamePlayers = "/api/Game/Players";
  static joinCredential = (gameHash: string) =>
    `/api/Game/${encodeURIComponent(gameHash)}/JoinCredential`;
  static promptCategories = (lang: string) =>
    `/api/Prompt/Categories?lang=${encodeURIComponent(lang)}`;
  static accountAddIfNotExists = "/api/Account/Add";
  static accountIncrementScore = "/api/Account/IncrementScore";
  static accountGetScore = "/api/Account/GetScore";
  static accountGetTopScores = "/api/Account/GetTop";
  static accountSession = "/api/Account/Session";
  static accountLogout = "/api/Account/Logout";
}
export default ApiEndpoints;

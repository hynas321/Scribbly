class ApiEndpoints {
    static hubConnectionEndpoint: string = "/hub/connection";
    static gameCreate: string = "/api/Game/Create";
    static gameIsStarted: string = "/api/Game/IsStarted";
    static gameExists: string = "/api/Game/Exists";
    static gameGetHash: string = "/api/Game/GetHash";
    static playerJoinGame: string = "/api/Player/JoinGame";
    static playerHost: string = "/api/Player/IsHost";
    static playerExists: string = "/api/Player/Exists";
    static playerUsernameExists: string = "/api/Player/UsernameExists/";
    static scoreboardGet: string = "/api/Scoreboard/Get";
}

export default ApiEndpoints;
class ApiEndpoints {
    static hubConnectionEndpoint: string = "/hub/connection";
    static gameCreate: string = "/api/Game/Create";
    static gameIsStarted: string = "/api/Game/IsStarted";
    static gameExists: string = "/api/Game/Exists";
    static gameGetHash: string = "/api/Game/GetHash";
    static playerJoinGame: string = "/api/Player/JoinGame";
    static playerIsHost: string = "/api/Player/IsHost";
    static scoreboardGet: string = "/api/Scoreboard/Get";
}

export default ApiEndpoints;
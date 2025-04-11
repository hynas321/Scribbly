namespace WebApi.Api.Models.DTO
{
    public class GameStateDTO
    {
        public int CurrentDrawingTimeSeconds { get; set; } = 75;
        public int CurrentRound { get; set; } = 1;
        public string HiddenSecretWord { get; set; } = "? ? ?";
        public string DrawingPlayerUsername { get; set; } = string.Empty;
        public string HostPlayerUsername { get; set; } = string.Empty;
        public bool IsGameStarted { get; set; } = false;
        public bool IsTimerVisible { get; set; } = false;
        public List<string> CorrectGuessPlayerUsernames { get; set; } = new List<string>();
    }
}

namespace Scribbly.WebApi.Models.HttpResponse;

public class CreateGameResponse
{
    public string GameHash { get; set; } = string.Empty;
    public string RoomAccessToken { get; set; } = string.Empty;
    public int MaxPlayers { get; set; }
}

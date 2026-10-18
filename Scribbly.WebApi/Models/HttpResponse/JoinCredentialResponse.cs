namespace Scribbly.WebApi.Models.HttpResponse;

public class JoinCredentialResponse
{
    public string RoomAccessToken { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public int MaxPlayers { get; set; }
}

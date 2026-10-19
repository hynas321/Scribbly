namespace Scribbly.Application.Abstractions.Security;

public interface IRoomCredentialService
{
    string GenerateRoomCode();
    string GenerateAccessToken();
    string ComputeCredentialHash(string accessToken);
    bool Matches(string accessToken, string hash);
}

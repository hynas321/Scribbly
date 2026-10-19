namespace Scribbly.Domain.Entities;

public record FinalPlayerScore(string CredentialHash, int Score, string? Username = null);

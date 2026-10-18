using Scribbly.Application.Features.Games;
using Scribbly.Application.Security;
using Scribbly.Domain.Entities;
using Scribbly.WebApi.UnitTests.TestDoubles;

namespace Scribbly.WebApi.UnitTests.Lifecycle;

public class GameLifecycleServiceTests
{
    private static (GameLifecycleService Service, InMemoryGameStateStore Games, RoomCredentialService Credentials) Create(
        int maxPlayers = 4, TimeSpan? joinCredentialLifetime = null)
    {
        var games = new InMemoryGameStateStore();
        var canvas = new InMemoryCanvasStateStore();
        var credentials = new RoomCredentialService();
        var options = new GameLifecycleOptions(TimeSpan.FromMinutes(2), TimeSpan.FromSeconds(10),
            joinCredentialLifetime ?? TimeSpan.FromMinutes(1), maxPlayers);
        return (new GameLifecycleService(games, games, canvas, credentials, options, GameSettingsTestOptions.Create()), games, credentials);
    }

    [Fact]
    public async Task CreateThenHostJoin_ReturnsJoinedWithSinglePlayerSnapshot()
    {
        var (service, _, _) = Create();

        CreateGameResult room = await service.CreateAsync("host");
        JoinGameResult joined = await service.JoinAsync(room.GameHash, room.RoomAccessToken, "host-connection");

        Assert.False(string.IsNullOrWhiteSpace(room.GameHash));
        Assert.False(string.IsNullOrWhiteSpace(room.RoomAccessToken));
        Assert.Equal(JoinGameStatus.Joined, joined.Status);
        Assert.Equal("host", joined.Snapshot!.Username);
        Assert.True(joined.Snapshot.IsHost);
        Assert.Equal(1, joined.Snapshot.PlayerCount);
    }

    [Fact]
    public async Task GuestWithIssuedCredential_JoinsAndSeesTwoPlayers()
    {
        var (service, _, _) = Create();
        CreateGameResult room = await service.CreateAsync("host");
        await service.JoinAsync(room.GameHash, room.RoomAccessToken, "host-connection");

        JoinCredentialResult credential = await service.IssueJoinCredentialAsync(room.GameHash, "alice");
        JoinGameResult joined = await service.JoinAsync(room.GameHash, credential.RoomAccessToken!, "alice-connection");

        Assert.Equal(JoinCredentialStatus.Issued, credential.Status);
        Assert.Equal(JoinGameStatus.Joined, joined.Status);
        Assert.Equal("alice", joined.Snapshot!.Username);
        Assert.False(joined.Snapshot.IsHost);
        Assert.Equal(2, joined.Snapshot.PlayerCount);
    }

    [Fact]
    public async Task CreateAsync_AcceptsUsernameWithTwentyFiveCharacters()
    {
        var (service, games, _) = Create();

        CreateGameResult room = await service.CreateAsync(new string('a', 25));
        JoinGameResult joined = await service.JoinAsync(room.GameHash, room.RoomAccessToken, "host-connection");
        Game snapshot = (await games.GetSnapshotAsync(room.GameHash))!;

        Assert.Equal(JoinGameStatus.Joined, joined.Status);
        Assert.Equal(new string('a', 25), snapshot.GameState.HostPlayerUsername);
    }

    [Fact]
    public async Task CreateAsync_RejectsUsernameWithTwentySixCharacters()
    {
        var (service, _, _) = Create();

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(new string('a', 26)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("this-username-is-way-too-long")]
    public async Task CreateAsync_RejectsInvalidUsernames(string username)
    {
        var (service, _, _) = Create();

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(username));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task IssueJoinCredentialAsync_RejectsBlankUsernames(string username)
    {
        var (service, _, _) = Create();
        CreateGameResult room = await service.CreateAsync("host");

        await Assert.ThrowsAsync<ArgumentException>(() => service.IssueJoinCredentialAsync(room.GameHash, username));
    }

    [Fact]
    public async Task IssueJoinCredentialAsync_RejectsUsernameWithTwentySixCharacters()
    {
        var (service, _, _) = Create();
        CreateGameResult room = await service.CreateAsync("host");

        await Assert.ThrowsAsync<ArgumentException>(() => service.IssueJoinCredentialAsync(room.GameHash, new string('b', 26)));
    }

    [Fact]
    public async Task JoinAsync_WithUnissuedToken_ReturnsInvalidCredential()
    {
        var (service, _, credentials) = Create();
        CreateGameResult room = await service.CreateAsync("host");

        JoinGameResult result = await service.JoinAsync(room.GameHash, credentials.GenerateAccessToken(), "connection-1");

        Assert.Equal(JoinGameStatus.InvalidCredential, result.Status);
    }

    [Fact]
    public async Task JoinAsync_UnknownGameHash_ReturnsGameNotFound()
    {
        var (service, _, credentials) = Create();

        JoinGameResult result = await service.JoinAsync("UNKNOWNHASH00000", credentials.GenerateAccessToken(), "connection-1");

        Assert.Equal(JoinGameStatus.GameNotFound, result.Status);
    }

    [Fact]
    public async Task IssueJoinCredentialAsync_UnknownGameHash_ReturnsGameNotFound()
    {
        var (service, _, _) = Create();

        JoinCredentialResult result = await service.IssueJoinCredentialAsync("UNKNOWNHASH00000", "alice");

        Assert.Equal(JoinCredentialStatus.GameNotFound, result.Status);
    }

    [Theory]
    [InlineData("host")]
    [InlineData("HOST")]
    public async Task IssueJoinCredentialAsync_DuplicateOfHostUsername_ReturnsDuplicateUsername(string username)
    {
        var (service, _, _) = Create();
        CreateGameResult room = await service.CreateAsync("host");

        JoinCredentialResult result = await service.IssueJoinCredentialAsync(room.GameHash, username);

        Assert.Equal(JoinCredentialStatus.DuplicateUsername, result.Status);
    }

    [Fact]
    public async Task IssueJoinCredentialAsync_DuplicateOfJoinedPlayer_ReturnsDuplicateUsername()
    {
        var (service, _, _) = Create();
        CreateGameResult room = await service.CreateAsync("host");
        await service.JoinAsync(room.GameHash, room.RoomAccessToken, "host-connection");
        JoinCredentialResult alice = await service.IssueJoinCredentialAsync(room.GameHash, "alice");
        await service.JoinAsync(room.GameHash, alice.RoomAccessToken!, "alice-connection");

        JoinCredentialResult result = await service.IssueJoinCredentialAsync(room.GameHash, "ALICE");

        Assert.Equal(JoinCredentialStatus.DuplicateUsername, result.Status);
    }

    [Fact]
    public async Task RoomCapacity_PlayerAtMaxPlayersJoins_NextPlayerRejected()
    {
        var (service, games, _) = Create(maxPlayers: 2);
        CreateGameResult room = await service.CreateAsync("host");
        await service.JoinAsync(room.GameHash, room.RoomAccessToken, "host-connection");

        JoinCredentialResult guest = await service.IssueJoinCredentialAsync(room.GameHash, "alice");
        JoinGameResult joined = await service.JoinAsync(room.GameHash, guest.RoomAccessToken!, "alice-connection");
        JoinCredentialResult overflow = await service.IssueJoinCredentialAsync(room.GameHash, "bob");
        Game snapshot = (await games.GetSnapshotAsync(room.GameHash))!;

        Assert.Equal(JoinCredentialStatus.Issued, guest.Status);
        Assert.Equal(JoinGameStatus.Joined, joined.Status);
        Assert.Equal(2, snapshot.GameState.Players.Count);
        Assert.Equal(JoinCredentialStatus.RoomFull, overflow.Status);
    }

    [Fact]
    public async Task ConsumedCredential_CannotBeUsedForASecondJoin()
    {
        var (service, _, _) = Create();
        CreateGameResult room = await service.CreateAsync("host");
        await service.JoinAsync(room.GameHash, room.RoomAccessToken, "host-connection");
        JoinCredentialResult credential = await service.IssueJoinCredentialAsync(room.GameHash, "alice");
        JoinGameResult first = await service.JoinAsync(room.GameHash, credential.RoomAccessToken!, "alice-1");

        JoinGameResult second = await service.JoinAsync(room.GameHash, credential.RoomAccessToken!, "alice-2");

        Assert.Equal(JoinGameStatus.Joined, first.Status);
        Assert.Equal(JoinGameStatus.InvalidCredential, second.Status);
    }

    [Fact]
    public async Task ExpiredCredential_IsRejectedOnJoin()
    {
        var (service, _, _) = Create(joinCredentialLifetime: TimeSpan.Zero);
        CreateGameResult room = await service.CreateAsync("host");
        await service.JoinAsync(room.GameHash, room.RoomAccessToken, "host-connection");

        JoinCredentialResult credential = await service.IssueJoinCredentialAsync(room.GameHash, "alice");
        JoinGameResult joined = await service.JoinAsync(room.GameHash, credential.RoomAccessToken!, "alice-connection");

        Assert.Equal(JoinGameStatus.InvalidCredential, joined.Status);
        Game snapshot = (await service.GetSnapshotAsync(room.GameHash))!;
        Assert.DoesNotContain(snapshot.GameState.Players, p => p.Username == "alice");
    }

    [Fact]
    public async Task RemoveByConnectionIdAsync_UnknownConnection_DoesNotThrow()
    {
        var (service, _, _) = Create();

        DisconnectedPlayerResult result = await service.RemoveByConnectionIdAsync("unknown-connection");

        Assert.Null(result.Username);
        Assert.Null(result.GameHash);
        Assert.Equal(0, result.PlayerCount);
        Assert.False(result.GameRemoved);
    }

    [Fact]
    public async Task ReconnectAsync_UnknownGameHash_ReturnsGameNotFound()
    {
        var (service, _, credentials) = Create();

        JoinGameResult result = await service.ReconnectAsync("UNKNOWNHASH00000", credentials.GenerateAccessToken(), "connection-1");

        Assert.Equal(JoinGameStatus.GameNotFound, result.Status);
    }

    [Fact]
    public async Task ReconnectAsync_DeletedGame_ReturnsGameNotFound()
    {
        var (service, _, _) = Create();
        CreateGameResult room = await service.CreateAsync("host");
        await service.JoinAsync(room.GameHash, room.RoomAccessToken, "host-connection");
        await service.DeleteAsync(room.GameHash);

        JoinGameResult result = await service.ReconnectAsync(room.GameHash, room.RoomAccessToken, "host-2");

        Assert.Equal(JoinGameStatus.GameNotFound, result.Status);
    }

    [Fact]
    public async Task LeaveAsync_HostLeavesLobby_OldestRemainingPlayerBecomesHost()
    {
        var (service, games, _) = Create();
        CreateGameResult room = await service.CreateAsync("host");
        await service.JoinAsync(room.GameHash, room.RoomAccessToken, "host-connection");
        JoinCredentialResult alice = await service.IssueJoinCredentialAsync(room.GameHash, "alice");
        JoinCredentialResult bob = await service.IssueJoinCredentialAsync(room.GameHash, "bob");
        await service.JoinAsync(room.GameHash, alice.RoomAccessToken!, "alice-connection");
        await service.JoinAsync(room.GameHash, bob.RoomAccessToken!, "bob-connection");

        LeaveGameResult left = await service.LeaveAsync(room.GameHash, room.RoomAccessToken);
        var snapshot = await games.GetSnapshotAsync(room.GameHash);

        Assert.Equal(LeaveGameStatus.Left, left.Status);
        Assert.Equal("alice", left.NewHostUsername);
        Assert.False(left.HostLeftLobby);
        Assert.False(left.GameRemovedBecauseEmpty);
        Assert.NotNull(snapshot);
        Assert.Equal("alice", snapshot!.GameState.HostPlayerUsername);
        JoinGameResult hostAgain = await service.JoinAsync(room.GameHash, room.RoomAccessToken, "host-connection-2");
        Assert.Equal(JoinGameStatus.InvalidCredential, hostAgain.Status);
    }
}

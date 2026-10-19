using Scribbly.Application.Features.Games;
using Scribbly.Application.Features.Rounds;
using Scribbly.Application.Features.Settings;
using Scribbly.Application.Security;
using Scribbly.Domain.Entities;
using Scribbly.WebApi.UnitTests.TestDoubles;

namespace Scribbly.WebApi.UnitTests.Settings;

public class GameSettingsServiceTests
{
    private static async Task<(GameSettingsService Service, InMemoryGameStateStore Games,
        CreateGameResult Room)> CreateLobby()
    {
        var games = new InMemoryGameStateStore();
        var canvas = new InMemoryCanvasStateStore();
        var credentials = new RoomCredentialService();
        var lifecycle = new GameLifecycleService(games, games, canvas, credentials,
            new GameLifecycleOptions(TimeSpan.FromMinutes(2), TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(1), 4), GameSettingsTestOptions.Create());
        CreateGameResult room = await lifecycle.CreateAsync("host");
        await lifecycle.JoinAsync(room.GameHash, room.RoomAccessToken, "host-connection");
        return (new GameSettingsService(games, credentials, new FakePromptRepository(("en", new[] { "food", "animals" })), GameSettingsTestOptions.Create()), games, room);
    }

    [Theory]
    [InlineData(15)]
    [InlineData(130)]
    public async Task SetDrawingTimeAsync_OutOfRange_IsRejectedNotClamped(int seconds)
    {
        var (service, _, room) = await CreateLobby();

        GameSettingsCommandResult result = await service.SetDrawingTimeAsync(room.GameHash, room.RoomAccessToken, seconds);

        Assert.Equal(GameSettingsCommandStatus.InvalidSetting, result.Status);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(9)]
    public async Task SetRoundsCountAsync_OutOfRange_IsRejected(int rounds)
    {
        var (service, _, room) = await CreateLobby();

        GameSettingsCommandResult result = await service.SetRoundsCountAsync(room.GameHash, room.RoomAccessToken, rounds);

        Assert.Equal(GameSettingsCommandStatus.InvalidSetting, result.Status);
    }

    [Fact]
    public async Task SetPromptLanguageAsync_UnsupportedLanguage_IsRejected()
    {
        var (service, _, room) = await CreateLobby();

        GameSettingsCommandResult result = await service.SetPromptLanguageAsync(room.GameHash, room.RoomAccessToken, "de");

        Assert.Equal(GameSettingsCommandStatus.InvalidSetting, result.Status);
    }

    [Fact]
    public async Task ValidSettings_AreAppliedIntoSnapshot()
    {
        var (service, games, room) = await CreateLobby();

        GameSettingsCommandResult time = await service.SetDrawingTimeAsync(room.GameHash, room.RoomAccessToken, 60);
        GameSettingsCommandResult rounds = await service.SetRoundsCountAsync(room.GameHash, room.RoomAccessToken, 3);
        GameSettingsCommandResult language = await service.SetPromptLanguageAsync(room.GameHash, room.RoomAccessToken, "pl");
        Game snapshot = (await games.GetSnapshotAsync(room.GameHash))!;

        Assert.Equal(GameSettingsCommandStatus.Applied, time.Status);
        Assert.Equal(GameSettingsCommandStatus.Applied, rounds.Status);
        Assert.Equal(GameSettingsCommandStatus.Applied, language.Status);
        Assert.Equal(60, snapshot.GameSettings.DrawingTimeSeconds);
        Assert.Equal(3, snapshot.GameSettings.RoundsCount);
        Assert.Equal("pl", snapshot.GameSettings.PromptLanguage);
    }

    [Fact]
    public async Task NonHost_CannotChangeSettings()
    {
        var (service, _, room) = await CreateLobby();
        var credentials = new RoomCredentialService();

        GameSettingsCommandResult result = await service.SetDrawingTimeAsync(room.GameHash, credentials.GenerateAccessToken(), 60);

        Assert.Equal(GameSettingsCommandStatus.Unauthorized, result.Status);
    }

    [Fact]
    public async Task UnknownGame_ReturnsGameNotFound()
    {
        var (service, _, _) = await CreateLobby();

        GameSettingsCommandResult result = await service.SetDrawingTimeAsync("UNKNOWNHASH00000", "token", 60);

        Assert.Equal(GameSettingsCommandStatus.GameNotFound, result.Status);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(11)]
    public async Task SetMaxPlayersAsync_OutOfRange_IsRejected(int maxPlayers)
    {
        var (service, _, room) = await CreateLobby();

        GameSettingsCommandResult result = await service.SetMaxPlayersAsync(room.GameHash, room.RoomAccessToken, maxPlayers);

        Assert.Equal(GameSettingsCommandStatus.InvalidSetting, result.Status);
    }

    [Fact]
    public async Task SetMaxPlayersAsync_NonHost_IsUnauthorized()
    {
        var (service, _, room) = await CreateLobby();
        var credentials = new RoomCredentialService();

        GameSettingsCommandResult result = await service.SetMaxPlayersAsync(room.GameHash, credentials.GenerateAccessToken(), 4);

        Assert.Equal(GameSettingsCommandStatus.Unauthorized, result.Status);
    }

    [Fact]
    public async Task SetMaxPlayersAsync_ValidValue_IsPersistedIntoSnapshot()
    {
        var (service, games, room) = await CreateLobby();

        GameSettingsCommandResult result = await service.SetMaxPlayersAsync(room.GameHash, room.RoomAccessToken, 8);
        Game snapshot = (await games.GetSnapshotAsync(room.GameHash))!;

        Assert.Equal(GameSettingsCommandStatus.Applied, result.Status);
        Assert.Equal(8, result.Settings!.MaxPlayers);
        Assert.Equal(8, snapshot.GameSettings.MaxPlayers);
    }

    [Fact]
    public async Task SetPromptCategoriesAsync_UnknownSlug_IsRejected()
    {
        var (service, _, room) = await CreateLobby();

        GameSettingsCommandResult result = await service.SetPromptCategoriesAsync(room.GameHash, room.RoomAccessToken, new[] { "unknown" });

        Assert.Equal(GameSettingsCommandStatus.InvalidCategories, result.Status);
    }

    [Fact]
    public async Task SetPromptCategoriesAsync_NonHost_IsUnauthorized()
    {
        var (service, _, room) = await CreateLobby();
        var credentials = new RoomCredentialService();

        GameSettingsCommandResult result = await service.SetPromptCategoriesAsync(room.GameHash, credentials.GenerateAccessToken(), new[] { "food" });

        Assert.Equal(GameSettingsCommandStatus.Unauthorized, result.Status);
    }

    [Fact]
    public async Task SetPromptCategoriesAsync_EmptyArray_IsAcceptedAndMeansAllCategories()
    {
        var (service, games, room) = await CreateLobby();

        GameSettingsCommandResult result = await service.SetPromptCategoriesAsync(room.GameHash, room.RoomAccessToken, Array.Empty<string>());
        Game snapshot = (await games.GetSnapshotAsync(room.GameHash))!;

        Assert.Equal(GameSettingsCommandStatus.Applied, result.Status);
        Assert.Empty(result.Settings!.PromptCategories);
        Assert.Empty(snapshot.GameSettings.PromptCategories);
    }

    [Fact]
    public async Task SetPromptCategoriesAsync_ValidCategories_AreNormalizedAndPersisted()
    {
        var (service, games, room) = await CreateLobby();

        GameSettingsCommandResult result = await service.SetPromptCategoriesAsync(
            room.GameHash, room.RoomAccessToken, new[] { " FOOD ", "Animals", "food" });
        Game snapshot = (await games.GetSnapshotAsync(room.GameHash))!;

        Assert.Equal(GameSettingsCommandStatus.Applied, result.Status);
        Assert.Equal(new[] { "food", "animals" }, snapshot.GameSettings.PromptCategories);
    }

    [Fact]
    public async Task SetPromptCategoriesAsync_InvalidSlug_IsRejected()
    {
        var (service, _, room) = await CreateLobby();

        GameSettingsCommandResult result = await service.SetPromptCategoriesAsync(room.GameHash, room.RoomAccessToken, new[] { "food2" });

        Assert.Equal(GameSettingsCommandStatus.InvalidSetting, result.Status);
    }

    [Fact]
    public async Task JoinCapacity_HonorsLoweredMaxPlayersSetting()
    {
        var (service, games, room) = await CreateLobby();
        var credentials = new RoomCredentialService();
        var lifecycle = new GameLifecycleService(games, games, new InMemoryCanvasStateStore(), credentials,
            new GameLifecycleOptions(TimeSpan.FromMinutes(2), TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(1), 4), GameSettingsTestOptions.Create());

        GameSettingsCommandResult lowered = await service.SetMaxPlayersAsync(room.GameHash, room.RoomAccessToken, 2);
        JoinCredentialResult alice = await lifecycle.IssueJoinCredentialAsync(room.GameHash, "alice");
        JoinCredentialResult bob = await lifecycle.IssueJoinCredentialAsync(room.GameHash, "bob");

        Assert.Equal(GameSettingsCommandStatus.Applied, lowered.Status);
        Assert.Equal(JoinCredentialStatus.Issued, alice.Status);
        Assert.Equal(JoinCredentialStatus.RoomFull, bob.Status);
    }

    [Fact]
    public async Task LoweringMaxPlayersBelowCurrentCount_BlocksNewJoinsButKeepsPlayers()
    {
        var (service, games, room) = await CreateLobby();
        var credentials = new RoomCredentialService();
        var lifecycle = new GameLifecycleService(games, games, new InMemoryCanvasStateStore(), credentials,
            new GameLifecycleOptions(TimeSpan.FromMinutes(2), TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(1), 4), GameSettingsTestOptions.Create());
        JoinCredentialResult alice = await lifecycle.IssueJoinCredentialAsync(room.GameHash, "alice");
        await lifecycle.JoinAsync(room.GameHash, alice.RoomAccessToken!, "alice-connection");
        JoinCredentialResult bob = await lifecycle.IssueJoinCredentialAsync(room.GameHash, "bob");
        await lifecycle.JoinAsync(room.GameHash, bob.RoomAccessToken!, "bob-connection");

        GameSettingsCommandResult lowered = await service.SetMaxPlayersAsync(room.GameHash, room.RoomAccessToken, 2);
        JoinCredentialResult carol = await lifecycle.IssueJoinCredentialAsync(room.GameHash, "carol");
        Game snapshot = (await games.GetSnapshotAsync(room.GameHash))!;

        Assert.Equal(GameSettingsCommandStatus.Applied, lowered.Status);
        Assert.Equal(JoinCredentialStatus.RoomFull, carol.Status);
        Assert.Equal(3, snapshot.GameState.Players.Count);
    }

    [Fact]
    public async Task SettingsCannotChangeAfterGameStarted()
    {
        var (service, games, room) = await CreateLobby();
        var rounds = new GameRoundService(games, new InMemoryCanvasStateStore(), new RoomCredentialService(), GameSettingsTestOptions.Create());

        await games.UpdateAsync(room.GameHash, game =>
        {
            game.GameState.Players.Add(new Player
            {
                Username = "guest",
                CredentialHash = "guest-hash",
                ConnectionId = "guest-connection"
            });
            return 0;
        });
        await rounds.StartAuthorizedSessionAsync(room.GameHash, room.RoomAccessToken,
            new GameSettings { DrawingTimeSeconds = 75, RoundsCount = 2, PromptLanguage = "en" });

        GameSettingsCommandResult result = await service.SetDrawingTimeAsync(room.GameHash, room.RoomAccessToken, 60);

        Assert.Equal(GameSettingsCommandStatus.GameAlreadyStarted, result.Status);
    }

    [Theory]
    [InlineData("off")]
    [InlineData("slow")]
    [InlineData("normal")]
    [InlineData("fast")]
    public async Task SetHintSpeedAsync_ValidValues_ArePersisted(string speed)
    {
        var (service, games, room) = await CreateLobby();

        GameSettingsCommandResult result = await service.SetHintSpeedAsync(room.GameHash, room.RoomAccessToken, speed);
        var snapshot = await games.GetSnapshotAsync(room.GameHash);

        Assert.Equal(GameSettingsCommandStatus.Applied, result.Status);
        Assert.Equal(speed, snapshot!.GameSettings.HintSpeed);
        Assert.Equal(speed, (await service.GetAsync(room.GameHash, room.RoomAccessToken))!.HintSpeed);
    }

    [Theory]
    [InlineData("")]
    [InlineData("instant")]
    [InlineData("FAST")]
    public async Task SetHintSpeedAsync_InvalidValues_AreRejected(string speed)
    {
        var (service, _, room) = await CreateLobby();

        GameSettingsCommandResult result = await service.SetHintSpeedAsync(room.GameHash, room.RoomAccessToken, speed);

        Assert.Equal(GameSettingsCommandStatus.InvalidSetting, result.Status);
    }

    [Fact]
    public async Task HintSettings_NonHost_IsUnauthorized()
    {
        var (service, _, room) = await CreateLobby();
        var credentials = new RoomCredentialService();

        string token = credentials.GenerateAccessToken();
        GameSettingsCommandResult result = await service.SetHintSpeedAsync(room.GameHash, token, "fast");

        Assert.Equal(GameSettingsCommandStatus.Unauthorized, result.Status);
        Assert.Equal(GameSettingsCommandStatus.Unauthorized,
            (await service.SetHintPercentageAsync(room.GameHash, token, 50)).Status);
    }

    [Theory]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(75)]
    public async Task SetHintPercentageAsync_ValidValues_ArePersisted(int percentage)
    {
        var (service, games, room) = await CreateLobby();
        var result = await service.SetHintPercentageAsync(room.GameHash, room.RoomAccessToken, percentage);
        Assert.Equal(GameSettingsCommandStatus.Applied, result.Status);
        Assert.Equal(percentage, (await games.GetSnapshotAsync(room.GameHash))!.GameSettings.HintPercentage);
        Assert.Equal(percentage, (await service.GetAsync(room.GameHash, room.RoomAccessToken))!.HintPercentage);
    }

    [Theory]
    [InlineData(-5)]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(1)]
    [InlineData(15)]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(30)]
    [InlineData(40)]
    [InlineData(26)]
    [InlineData(35)]
    [InlineData(45)]
    [InlineData(55)]
    [InlineData(76)]
    [InlineData(100)]
    public async Task SetHintPercentageAsync_InvalidValues_AreRejected(int percentage)
    {
        var (service, _, room) = await CreateLobby();
        Assert.Equal(GameSettingsCommandStatus.InvalidSetting,
            (await service.SetHintPercentageAsync(room.GameHash, room.RoomAccessToken, percentage)).Status);
    }

    [Fact]
    public async Task HintSettings_SurviveJoinAndReconnectSnapshots()
    {
        var (service, games, room) = await CreateLobby();
        await service.SetHintSpeedAsync(room.GameHash, room.RoomAccessToken, "fast");
        await service.SetHintPercentageAsync(room.GameHash, room.RoomAccessToken, 75);
        var lifecycle = new GameLifecycleService(
            games,
            games,
            new InMemoryCanvasStateStore(),
            new RoomCredentialService(),
            null,
            GameSettingsTestOptions.Create());
        var credential = await lifecycle.IssueJoinCredentialAsync(room.GameHash, "guest");
        var joined = await lifecycle.JoinAsync(room.GameHash, credential.RoomAccessToken!, "guest-hints");
        Assert.Equal("fast", joined.Snapshot!.Settings.HintSpeed);
        Assert.Equal(75, joined.Snapshot.Settings.HintPercentage);
        await lifecycle.RemoveByConnectionIdAsync("guest-hints");
        var resumed = await lifecycle.ReconnectAsync(room.GameHash, credential.RoomAccessToken!, "guest-reconnected");
        Assert.Equal(JoinGameStatus.Resumed, resumed.Status);
        Assert.Equal("fast", resumed.Snapshot!.Settings.HintSpeed);
        Assert.Equal(75, resumed.Snapshot.Settings.HintPercentage);
    }

    [Fact]
    public async Task HintSettings_CannotChangeDuringTheGame()
    {
        var (service, games, room) = await CreateLobby();
        await games.UpdateAsync(room.GameHash, game => game.GameState.IsGameStarted = true);
        Assert.Equal(GameSettingsCommandStatus.GameAlreadyStarted,
            (await service.SetHintSpeedAsync(room.GameHash, room.RoomAccessToken, "fast")).Status);
        Assert.Equal(GameSettingsCommandStatus.GameAlreadyStarted,
            (await service.SetHintPercentageAsync(room.GameHash, room.RoomAccessToken, 50)).Status);
    }
}

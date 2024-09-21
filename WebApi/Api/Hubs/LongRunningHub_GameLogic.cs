using WebApi.Hubs;
using WebApi.Api.Hubs.Static;
using WebApi.Api.Utilities;
using WebApi.Application.Managers.Interfaces;
using WebApi.Application.Services.Interfaces;
using WebApi.Domain.Entities;
using Microsoft.AspNetCore.SignalR;
using WebApi.Hubs;

namespace WebApi.Api.Hubs;

public partial class LongRunningHubConnection : Hub
{
    private readonly IGameManager _gameManager;
    private readonly IPlayerManager _playerManager;
    private readonly ILogger<HubConnection> _logger;
    private readonly IHubContext<HubConnection> _connectionHubContext;
    private readonly IHubContext<AccountHubConnection> _accountHubContext;
    private readonly IRandomWordService _randomWordService;

    public LongRunningHubConnection(
        IGameManager gameManager,
        IPlayerManager playerManager,
        ILogger<HubConnection> logger,
        IHubContext<HubConnection> connectionHubContext,
        IHubContext<AccountHubConnection> accountHubContext,
        IRandomWordService randomWordService
    )
    {
        _gameManager = gameManager;
        _playerManager = playerManager;
        _logger = logger;
        _connectionHubContext = connectionHubContext;
        _accountHubContext = accountHubContext;
        _randomWordService = randomWordService;
    }

    [HubMethodName(HubMessages.StartGame)]
    public async Task StartGame(string gameHash, string token, GameSettings settings)
    {
        try
        {
            Game game = _gameManager.GetGame(gameHash);

            if (game == null)
            {
                _logger.LogError($"Game #{gameHash} StartGame: Game does not exist");
                return;
            }

            if (token != game.HostToken)
            {
                _logger.LogError($"Game #{gameHash} StartGame: Token is not a host token");
                return;
            }

            if (game.GameState.Players.Count < 2)
            {
                _logger.LogError($"Game #{gameHash} StartGame: Too few players to start the game");
                return;
            }

            if (settings.DrawingTimeSeconds < 30 ||
                settings.DrawingTimeSeconds > 120 ||
                settings.RoundsCount < 1 ||
                settings.RoundsCount > 6
            )
            {
                _logger.LogError($"Game #{gameHash} StartGame: Incorrect settings data");
                return;
            }

            game.ChatMessages.Clear();
            game.GameState.IsGameStarted = true;

            game.GameSettings.DrawingTimeSeconds = settings.DrawingTimeSeconds;
            game.GameSettings.RoundsCount = settings.RoundsCount;
            game.GameSettings.WordLanguage = settings.WordLanguage;

            await _connectionHubContext.Clients.Group(gameHash).SendAsync(HubMessages.OnStartGame);
            _logger.LogInformation($"Game #{gameHash} StartGame: Game started");

            Context.Abort();

            await Task.Run(() => ManageGameFlow(gameHash));
        }
        catch (Exception ex)
        {
            _logger.LogError(Convert.ToString(ex));
        }
    }

    public async Task ManageGameFlow(string gameHash)
    {
        try
        {
            Game game = _gameManager.GetGame(gameHash);

            while (true)
            {
                game.GameState.DrawingPlayersTokens = _playerManager.GetOnlinePlayersTokens(gameHash);

                await _connectionHubContext.Clients.Group(gameHash).SendAsync(HubMessages.OnClearCanvas);
                await SetCanvasText(gameHash, $"Round {game.GameState.CurrentRound}", BootstrapColors.Green);
                await Task.Delay(5000);

                while (game.GameState.DrawingPlayersTokens.Count != 0)
                {
                    List<string> drawingPlayersTokens = game.GameState.DrawingPlayersTokens;
                    List<string> onlinePlayerTokens = _playerManager.GetOnlinePlayersTokens(gameHash);

                    if (drawingPlayersTokens.Count != onlinePlayerTokens.Count)
                    {
                        game.GameState.DrawingPlayersTokens = drawingPlayersTokens
                            .Where(onlinePlayerTokens.Contains)
                            .ToList();
                    }

                    game.GameState.DrawingPlayerUsername = "";
                    game.GameState.DrawingToken = "";
                    game.GameState.NoChatPermissionTokens.Clear();
                    game.GameState.DrawnLines.Clear();
                    game.GameState.CorrectAnswerCount = 0;
                    game.GameState.CorrectGuessPlayerUsernames.Clear();

                    await _connectionHubContext.Clients.Group(gameHash).SendAsync(HubMessages.OnClearCanvas);

                    Random random = new Random();

                    int randomTokenIndex = random.Next(game.GameState.DrawingPlayersTokens.Count);
                    string drawingToken = game.GameState.DrawingPlayersTokens[randomTokenIndex];
                    string drawingPlayerUsername = _playerManager.GetPlayerByToken(gameHash, drawingToken).Username;
                    string actualSecretWord = await _randomWordService.FetchWordAsync(gameHash) ?? throw new NullReferenceException();
                    string hiddenSecretWord = Convert.ToString(actualSecretWord.Length);

                    game.GameState.DrawingPlayerUsername = drawingPlayerUsername;
                    game.GameState.DrawingPlayersTokens.RemoveAt(randomTokenIndex);
                    game.GameState.IsTimerVisible = true;
                    game.GameState.HiddenSecretWord = "? ? ?";

                    await _connectionHubContext.Clients.Group(gameHash).SendAsync(HubMessages.onUpdateCorrectGuessPlayerUsernames, JsonHelper.Serialize(game.GameState.CorrectGuessPlayerUsernames));
                    await _connectionHubContext.Clients.Group(gameHash).SendAsync(HubMessages.OnUpdateDrawingPlayer, drawingPlayerUsername);
                    await _connectionHubContext.Clients.Group(gameHash).SendAsync(HubMessages.OnRequestSecretWord);
                    await SetCanvasText(gameHash, $"{drawingPlayerUsername} is going to draw in 5s", BootstrapColors.Green);
                    await Task.Delay(5000);

                    game.GameState.ActualSecretWord = actualSecretWord;
                    game.GameState.HiddenSecretWord = $"Secret word length: {hiddenSecretWord}";

                    game.GameState.DrawingToken = drawingToken;

                    await _connectionHubContext.Clients.Group(gameHash).SendAsync(HubMessages.OnRequestSecretWord);
                    await _connectionHubContext.Clients.Group(gameHash).SendAsync(HubMessages.OnUpdateTimerVisibility, true);

                    bool isTimerFinishedSuccessfuly = await Task.Run(() => UpdateTimer(gameHash));

                    if (!isTimerFinishedSuccessfuly)
                    {
                        return;
                    }

                    int correctAnswers = game.GameState.CorrectAnswerCount;
                    int playersCount = game.GameState.Players.Count;

                    if (correctAnswers == playersCount - 1)
                    {
                        int pointsForDrawing = 10;

                        _playerManager.UpdatePlayerScore(gameHash, drawingToken, pointsForDrawing);

                        await SendAnnouncement(gameHash, $"{drawingPlayerUsername} received the drawing bonus (+{pointsForDrawing} points)", BootstrapColors.Green);
                    }
                    else if (correctAnswers < playersCount - 1 && correctAnswers > 0)
                    {
                        int pointsForDrawing = 5;

                        _playerManager.UpdatePlayerScore(gameHash, drawingToken, pointsForDrawing);

                        await SendAnnouncement(gameHash, $"{drawingPlayerUsername} received the drawing bonus (+{pointsForDrawing} points)", BootstrapColors.Green);
                    }
                    else
                    {
                        await SendAnnouncement(gameHash, $"{drawingPlayerUsername} received no drawing bonus", BootstrapColors.Red);
                    }

                    List<PlayerScore> playerScores = _playerManager.GetPlayerScores(gameHash);

                    await _connectionHubContext.Clients.Group(gameHash).SendAsync(HubMessages.OnUpdatePlayerScores, JsonHelper.Serialize(playerScores));

                    game.GameState.DrawingToken = "";
                    game.GameState.IsTimerVisible = false;

                    await _connectionHubContext.Clients.Group(gameHash).SendAsync(HubMessages.OnUpdateTimerVisibility, false);
                    await SetCanvasText(gameHash, $"The drawing phase has ended", BootstrapColors.Green);
                    await SendAnnouncement(gameHash, $"The answer was: {actualSecretWord}", BootstrapColors.Yellow);

                    game.GameState.HiddenSecretWord = actualSecretWord;
                    await _connectionHubContext.Clients.Group(gameHash).SendAsync(HubMessages.OnRequestSecretWord);
                    await Task.Delay(8000);

                    game.GameState.HiddenSecretWord = "? ? ?";
                    await _connectionHubContext.Clients.Group(gameHash).SendAsync(HubMessages.OnRequestSecretWord);
                }

                game.GameState.CurrentRound++;

                if (game.GameState.CurrentRound > game.GameSettings.RoundsCount)
                {
                    await _accountHubContext.Clients.All.SendAsync(HubMessages.OnUpdateAccountScore, gameHash);
                    await SetCanvasText(gameHash, $"Thank you for playing! You may leave the game :)", BootstrapColors.Green);
                    await Task.Delay(10000);
                    _gameManager.RemoveGame(gameHash);
                    //await hubContext.Clients.Group(gameHash).SendAsync(HubEvents.OnEndGame);
                    break;
                }

                await _connectionHubContext.Clients.Group(gameHash).SendAsync(HubMessages.OnUpdateCurrentRound, game.GameState.CurrentRound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInformation(Convert.ToString(ex));
            await _connectionHubContext.Clients.Group(gameHash).SendAsync(HubMessages.OnEndGame);
            _gameManager.RemoveGame(gameHash);
        }
    }

    public async Task<bool> UpdateTimer(string gameHash)
    {
        try
        {
            Game game = _gameManager.GetGame(gameHash);

            if (game == null)
            {
                return false;
            }

            game.GameState.CurrentDrawingTimeSeconds = game.GameSettings.DrawingTimeSeconds;

            int initialTime = game.GameState.CurrentDrawingTimeSeconds;
            CancellationTokenSource cancellationToken = new CancellationTokenSource();

            for (int i = 0; i < initialTime; i++)
            {
                if (_gameManager.GetGame(gameHash) == null)
                {
                    cancellationToken.Cancel();
                    return false;
                }

                if (game.GameState.CorrectAnswerCount == game.GameState.Players.Count - 1)
                {
                    cancellationToken.Cancel();
                    return true;
                }

                if (game.GameState.CurrentDrawingTimeSeconds < 0)
                {
                    cancellationToken.Cancel();
                    return true;
                }

                await _connectionHubContext.Clients.Group(gameHash).SendAsync(HubMessages.OnUpdateTimer, game.GameState.CurrentDrawingTimeSeconds);

                game.GameState.CurrentDrawingTimeSeconds--;

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken.Token);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(Convert.ToString(ex));
            return false;
        }
    }

    public async Task SetCanvasText(string gameHash, string text, string color)
    {
        try
        {
            Game game = new Game();

            if (game == null)
            {
                return;
            }

            AnnouncementMessage message = new AnnouncementMessage()
            {
                Text = text,
                BootstrapBackgroundColor = color
            };

            await _connectionHubContext.Clients.Group(gameHash).SendAsync(HubMessages.OnSetCanvasText, JsonHelper.Serialize(message));
        }
        catch (Exception ex)
        {
            _logger.LogError(Convert.ToString(ex));
        }
    }

    public async Task SendAnnouncement(string gameHash, string text, string backgroundColor)
    {
        try
        {
            Game game = new Game();

            if (game == null)
            {
                _logger.LogError($"Game #{gameHash} SendAnnouncement: Game does not exist");
                return;
            }

            AnnouncementMessage message = new AnnouncementMessage()
            {
                Text = text,
                BootstrapBackgroundColor = backgroundColor
            };

            //gameManager.AddChatMessage(message);

            await _connectionHubContext.Clients.Group(gameHash).SendAsync(HubMessages.OnSendAnnouncement, JsonHelper.Serialize(message));
        }
        catch (Exception ex)
        {
            _logger.LogError(Convert.ToString(ex));
        }
    }
}
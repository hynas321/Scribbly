using Dotnet.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Dotnet.Server.Http.Requests;
using Dotnet.Server.Managers;
using Dotnet.Server.Json;
using Microsoft.AspNetCore.SignalR;
using Dotnet.Server.Hubs;
using Dotnet.Server.Http;

namespace Dotnet.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameStateController : ControllerBase
{
    private readonly GamesManager gamesManager = new GamesManager(25);
    private readonly IHubContext<HubConnection> hubContext;
    private readonly ILogger<GameStateController> logger;

    public GameStateController(ILogger<GameStateController> logger, IHubContext<HubConnection> hubContext)
    {
        this.logger = logger;
        this.hubContext = hubContext;
    }

    [HttpPut(HubEvents.StartGame)]
    [HubMethodName(HubEvents.StartGame)]
    public async Task<IActionResult> StartGame(
        [FromHeader(Name = Headers.Token)] string token,
        [FromHeader(Name = Headers.GameHash)] string gameHash
    )
    {
        try
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("Status: 400. Invalid received request body.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Game game = gamesManager.GetGameByHash(gameHash);

            if (game == null)
            {
                logger.LogError("Status: 404. Not found.");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            if (token != game.HostToken)
            {
                logger.LogError("Status: 401. Unauthorized.");
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            game.GameState.IsStarted = true;

            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                string connectionId = HttpContext.Request.Query["connectionId"];

                await hubContext.Clients
                    .Group(gameHash)
                    .SendAsync(HubEvents.OnStartGame);
            }

            logger.LogInformation("Status: 200. OK.");

            return StatusCode(StatusCodes.Status200OK);
        }
        catch (Exception ex)
        {
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPut(HubEvents.StartTimer)]
    [HubMethodName(HubEvents.StartTimer)]
    public async Task<IActionResult> StartTimer(
        [FromHeader(Name = Headers.Token)] string token,
        [FromHeader(Name = Headers.GameHash)] string gameHash
    )
    {
        try
        {  
            if (!ModelState.IsValid)
            {   
                logger.LogError("Status: 400. Invalid received request body.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Game game = gamesManager.GetGameByHash(gameHash);

            int initialTime = game.GameState.CurrentDrawingTimeSeconds;
            int currentTime = initialTime;
            CancellationTokenSource cancellationToken = new CancellationTokenSource();

            await Task.Run(async () =>
            {
                for (int i = 0; i < initialTime; i++)
                {   
                    string connectionId = HttpContext.Request.Query["connectionId"];

                    await hubContext.Clients
                        .Group(gameHash)
                        .SendAsync(HubEvents.OnStartTimer, currentTime);

                    currentTime--;

                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken.Token);

                    if (currentTime <= 0)
                    {
                        cancellationToken.Cancel();
                        break;
                    }
                }
            });

            return StatusCode(StatusCodes.Status200OK);
        }
        catch(Exception ex)
        {
            logger.LogInformation($"Game #{gameHash}: Progress bar could not be started. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
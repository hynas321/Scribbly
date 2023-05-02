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
public class GameSettingsController : ControllerBase
{
    private readonly GamesManager gamesManager = new GamesManager(25);
    private readonly IHubContext<HubConnection> hubContext;
    private readonly ILogger<GameSettingsController> logger;

    public GameSettingsController(ILogger<GameSettingsController> logger, IHubContext<HubConnection> hubContext)
    {
        this.logger = logger;
        this.hubContext = hubContext;
    }
    
    [HttpPut(HubEvents.SetAbstractNouns)]
    [HubMethodName(HubEvents.SetAbstractNouns)]
    public async Task<IActionResult> SetAbstractNouns(
        [FromHeader(Name = Headers.Token)] string token,
        [FromHeader(Name = Headers.GameHash)] string gameHash,
        [FromBody] SetSettingBody body)
    {
        try
        {
            if (!ModelState.IsValid || body.Setting is not bool)
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

            GameSettings settings = game.GameSettings;

            settings.NonAbstractNounsOnly = (bool)body.Setting;
            game.GameSettings = settings;

            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                string connectionId = HttpContext.Request.Query["connectionId"];

                await hubContext.Clients
                    .Group(gameHash)
                    .SendAsync(HubEvents.OnSetAbstractNouns, (bool)body.Setting);
            }

            logger.LogInformation("Status: 200. OK.");

            return StatusCode(StatusCodes.Status200OK, (bool)body.Setting);
        }
        catch (Exception ex)
        {
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPut(HubEvents.SetDrawingTimeSeconds)]
    [HubMethodName(HubEvents.SetDrawingTimeSeconds)]
    public async Task<IActionResult> SetDrawingTimeSeconds(
        [FromHeader(Name = Headers.Token)] string token,
        [FromHeader(Name = Headers.GameHash)] string gameHash,
        [FromBody] SetSettingBody body
    )
    {
        try
        {
            if (!ModelState.IsValid || body.Setting is not int)
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

            GameSettings settings = game.GameSettings;

            settings.DrawingTimeSeconds = (int)body.Setting;
            game.GameSettings = settings;

            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                string connectionId = HttpContext.Request.Query["connectionId"];

                await hubContext.Clients
                    .Group(gameHash)
                    .SendAsync(HubEvents.OnSetDrawingTimeSeconds, (int)body.Setting);
            }

            logger.LogInformation("Status: 200. OK.");

            return StatusCode(StatusCodes.Status200OK, (int)body.Setting);
        }
        catch (Exception ex)
        {
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPut(HubEvents.SetRoundsCount)]
    [HubMethodName(HubEvents.SetRoundsCount)]
    public async Task<IActionResult> SetRoundsCount(
        [FromHeader(Name = Headers.Token)] string token,
        [FromHeader(Name = Headers.GameHash)] string gameHash,
        [FromBody] SetSettingBody body
    )
    {
        try
        {
            if (!ModelState.IsValid || body.Setting is not int)
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

            GameSettings settings = game.GameSettings;

            settings.RoundsCount = (int)body.Setting;
            game.GameSettings = settings;

            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                string connectionId = HttpContext.Request.Query["connectionId"];

                await hubContext.Clients
                    .Group(gameHash)
                    .SendAsync(HubEvents.OnSetRoundsCount, (int)body.Setting);
            }

            logger.LogInformation("Status: 200. OK.");

            return StatusCode(StatusCodes.Status200OK, (int)body.Setting);
        }
        catch (Exception ex)
        {
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPut(HubEvents.SetWordLanguage)]
    [HubMethodName(HubEvents.SetWordLanguage)]
    public async Task<IActionResult> ChangeWordLanguageSetting(
        [FromHeader(Name = Headers.Token)] string token,
        [FromHeader(Name = Headers.GameHash)] string gameHash,
        [FromBody] SetSettingBody body
    )
    {
        try
        {
            if (!ModelState.IsValid || body.Setting is not string)
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

            GameSettings settings = game.GameSettings;

            settings.DrawingTimeSeconds = (int)body.Setting;
            game.GameSettings = settings;

            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                string connectionId = HttpContext.Request.Query["connectionId"];

                await hubContext.Clients
                    .Group(gameHash)
                    .SendAsync(HubEvents.OnSetRoundsCount, (string)body.Setting);
            }

            logger.LogInformation("Status: 200. OK.");

            return StatusCode(StatusCodes.Status200OK, (string)body.Setting);
        }
        catch (Exception ex)
        {
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet(HubEvents.LoadGameSettings)]
    [HubMethodName(HubEvents.LoadGameSettings)]
    public async Task<IActionResult> LoadGameSettings(
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

            GameSettings settings = game.GameSettings;

            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                string connectionId = HttpContext.Request.Query["connectionId"];

                await hubContext.Clients
                    .Client(connectionId)
                    .SendAsync(HubEvents.OnSetRoundsCount, JsonHelper.Serialize(settings));
            }

            logger.LogInformation("Status: 200. OK.");

            return StatusCode(StatusCodes.Status200OK, JsonHelper.Serialize(settings));
        }
        catch (Exception ex)
        {
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
using Dotnet.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Dotnet.Server.Http.Requests;
using Dotnet.Server.Managers;
using Dotnet.Server.Json;
using Microsoft.AspNetCore.SignalR;
using Dotnet.Server.Hubs;

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
    
    [HttpPost(HubEvents.SetAbstractNouns)]
    [HubMethodName(HubEvents.SetAbstractNouns)]
    public async Task<IActionResult> SetAbstractNouns([FromBody] SetSettingBody requestBody)
    {
        try
        {
            if (!ModelState.IsValid || requestBody.Setting is not bool)
            {   
                logger.LogError("Status: 400. Invalid received request body.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            bool gameExists = gamesManager.CheckIfGameExistsByHash(requestBody.GameHash);

            if (!gameExists)
            {
                logger.LogError("Status: 404. Not found.");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            GameSettings settings = gamesManager.GetGameSettings(requestBody.GameHash);

            settings.NonAbstractNounsOnly = (bool)requestBody.Setting;
            gamesManager.ChangeGameSettings(requestBody.GameHash, settings);

            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                string connectionId = HttpContext.Request.Query["connectionId"];

                await hubContext.Clients
                    .Group(requestBody.GameHash)
                    .SendAsync(HubEvents.OnSetAbstractNouns, (bool)requestBody.Setting);
            }

            return StatusCode(StatusCodes.Status200OK, (bool)requestBody.Setting);
        }
        catch (Exception ex)
        {
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost(HubEvents.SetDrawingTimeSeconds)]
    [HubMethodName(HubEvents.SetDrawingTimeSeconds)]
    public async Task<IActionResult> SetDrawingTimeSeconds([FromBody] SetSettingBody requestBody)
    {
        try
        {
            if (!ModelState.IsValid || requestBody.Setting is not int)
            {   
                logger.LogError("Status: 400. Invalid received request body.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            bool gameExists = gamesManager.CheckIfGameExistsByHash(requestBody.GameHash);

            if (!gameExists)
            {
                logger.LogError("Status: 404. Not found.");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            GameSettings settings = gamesManager.GetGameSettings(requestBody.GameHash);

            settings.DrawingTimeSeconds = (int)requestBody.Setting;
            gamesManager.ChangeGameSettings(requestBody.GameHash, settings);

            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                string connectionId = HttpContext.Request.Query["connectionId"];

                await hubContext.Clients
                    .Group(requestBody.GameHash)
                    .SendAsync(HubEvents.OnSetDrawingTimeSeconds, (int)requestBody.Setting);
            }

            return StatusCode(StatusCodes.Status200OK, (int)requestBody.Setting);
        }
        catch (Exception ex)
        {
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost(HubEvents.SetRoundsCount)]
    [HubMethodName(HubEvents.SetRoundsCount)]
    public async Task<IActionResult> SetRoundsCount([FromBody] SetSettingBody requestBody)
    {
        try
        {
            if (!ModelState.IsValid || requestBody.Setting is not int)
            {   
                logger.LogError("Status: 400. Invalid received request body.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            bool gameExists = gamesManager.CheckIfGameExistsByHash(requestBody.GameHash);

            if (!gameExists)
            {
                logger.LogError("Status: 404. Not found.");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            GameSettings settings = gamesManager.GetGameSettings(requestBody.GameHash);

            settings.RoundsCount = (int)requestBody.Setting;
            gamesManager.ChangeGameSettings(requestBody.GameHash, settings);

            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                string connectionId = HttpContext.Request.Query["connectionId"];

                await hubContext.Clients
                    .Group(requestBody.GameHash)
                    .SendAsync(HubEvents.OnSetRoundsCount, (int)requestBody.Setting);
            }

            return StatusCode(StatusCodes.Status200OK, (int)requestBody.Setting);
        }
        catch (Exception ex)
        {
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost(HubEvents.SetWordLanguage)]
    [HubMethodName(HubEvents.SetWordLanguage)]
    public async Task<IActionResult> ChangeWordLanguageSetting([FromBody] SetSettingBody requestBody)
    {
        try
        {
            if (!ModelState.IsValid || requestBody.Setting is not string)
            {   
                logger.LogError("Status: 400. Invalid received request body.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            bool gameExists = gamesManager.CheckIfGameExistsByHash(requestBody.GameHash);

            if (!gameExists)
            {
                logger.LogError("Status: 404. Not found.");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            GameSettings settings = gamesManager.GetGameSettings(requestBody.GameHash);

            settings.WordLanguage = (string)requestBody.Setting;
            gamesManager.ChangeGameSettings(requestBody.GameHash, settings);

            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                string connectionId = HttpContext.Request.Query["connectionId"];

                await hubContext.Clients
                    .Group(requestBody.GameHash)
                    .SendAsync(HubEvents.OnSetRoundsCount, (string)requestBody.Setting);
            }

            return StatusCode(StatusCodes.Status200OK, (string)requestBody.Setting);
        }
        catch (Exception ex)
        {
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost(HubEvents.LoadGameSettings)]
    [HubMethodName(HubEvents.LoadGameSettings)]
    public async Task<IActionResult> LoadGameSettings([FromBody] LoadGameSettingsBody requestBody)
    {
        try
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("Status: 400. Invalid received request body.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            bool gameExists = gamesManager.CheckIfGameExistsByHash(requestBody.GameHash);

            if (!gameExists)
            {
                logger.LogError("Status: 404. Not found.");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            GameSettings settings = gamesManager.GetGameSettings(requestBody.GameHash);

            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                string connectionId = HttpContext.Request.Query["connectionId"];

                await hubContext.Clients
                    .Group(requestBody.GameHash)
                    .SendAsync(HubEvents.OnSetRoundsCount, JsonHelper.Serialize(settings));
            }

            return StatusCode(StatusCodes.Status200OK, settings);
        }
        catch (Exception ex)
        {
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
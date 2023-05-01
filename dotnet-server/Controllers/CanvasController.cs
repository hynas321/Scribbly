using Dotnet.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Dotnet.Server.Http.Requests;
using Dotnet.Server.Managers;
using Microsoft.AspNetCore.SignalR;
using Dotnet.Server.Hubs;
using Dotnet.Server.Json;
using Newtonsoft.Json;

namespace Dotnet.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CanvasController : ControllerBase
{
    private readonly GamesManager gamesManager = new GamesManager(25);
    private readonly IHubContext<HubConnection> hubContext;
    private readonly ILogger<CanvasController> logger;

    public CanvasController(IHubContext<HubConnection> hubContext, ILogger<CanvasController> logger)
    {
        this.hubContext = hubContext;
        this.logger = logger;
    }

    [HttpPost(HubEvents.LoadCanvas)]
    [HubMethodName(HubEvents.LoadCanvas)]
    public async Task<IActionResult> LoadCanvas(
        [FromHeader(Name = "Token")] string token,
        [FromHeader(Name = "GameHash")] string gameHash
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

            List<DrawnLine> drawnLines = gamesManager.GetGameByHash(gameHash).DrawnLines;

            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                string connectionId = HttpContext.Request.Query["connectionId"];

                await hubContext.Clients
                    .Client(connectionId)
                    .SendAsync(HubEvents.OnLoadCanvas, JsonHelper.Serialize(drawnLines));
            }

            return StatusCode(StatusCodes.Status200OK, drawnLines);
        }
        catch (Exception ex)
        {
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HubMethodName(HubEvents.DrawOnCanvas)]
    public async Task<IActionResult> DrawOnCanvas(
        [FromHeader(Name = "Token")] string token,
        [FromHeader(Name = "GameHash")] string gameHash,
        [FromBody] DrawOnCanvasBody body
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

            //TODO: Verify if player has permission to draw

            DrawnLine drawnLine = JsonConvert.DeserializeObject<DrawnLine>(body.DrawnLineSerialized);
            gamesManager.AddDrawnLine(gameHash, drawnLine);

            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                string connectionId = HttpContext.Request.Query["connectionId"];

                await hubContext.Clients
                    .Group(gameHash)
                    .SendAsync(HubEvents.OnSendChatMessage, JsonHelper.Serialize(drawnLine));
            }

            return StatusCode(StatusCodes.Status200OK, drawnLine);
        }
        catch (Exception ex)
        {
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HubMethodName(HubEvents.ClearCanvas)]
    public async Task<IActionResult> ClearCanvas(
        [FromHeader(Name = "Token")] string token,
        [FromHeader(Name = "GameHash")] string gameHash
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

            //TODO: Verify if player has permission to draw

            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                string connectionId = HttpContext.Request.Query["connectionId"];

                await hubContext.Clients
                    .Group(gameHash)
                    .SendAsync(HubEvents.OnClearCanvas);
            }

            return StatusCode(StatusCodes.Status200OK);
        }
        catch (Exception ex)
        {
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}

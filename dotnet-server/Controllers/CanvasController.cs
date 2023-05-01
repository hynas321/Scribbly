using Dotnet.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Dotnet.Server.Http.Requests;
using Dotnet.Server.Managers;
using Microsoft.AspNetCore.SignalR;
using Dotnet.Server.Hubs;
using Dotnet.Server.Json;
using Newtonsoft.Json;
using Dotnet.Server.Http;

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

    [HttpPost(HubEvents.DrawOnCanvas)]
    [HubMethodName(HubEvents.DrawOnCanvas)]
    public async Task<IActionResult> DrawOnCanvas(
        [FromHeader(Name = Headers.Token)] string token,
        [FromHeader(Name = Headers.GameHash)] string gameHash,
        [FromBody] DrawOnCanvasBody body
    )
    {   
        try 
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("Status: 400. Bad request.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Game game = gamesManager.GetGameByHash(gameHash);

            if (game == null)
            {
                logger.LogError("Status: 404. Not found.");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            if (token != game.GameState.DrawingToken)
            {
                logger.LogError("Status: 401. Unauthorized.");

                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            DrawnLine drawnLine = JsonConvert.DeserializeObject<DrawnLine>(body.DrawnLineSerialized);

            if (drawnLine == null)
            {
                logger.LogError("Status: 400. Bad request.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            game.DrawnLines.Add(drawnLine);

            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                string connectionId = HttpContext.Request.Query["connectionId"];

                await hubContext.Clients
                    .Group(gameHash)
                    .SendAsync(HubEvents.OnDrawOnCanvas, JsonHelper.Serialize(drawnLine));
            }

            logger.LogInformation("Status: 201. Created.");

            return StatusCode(StatusCodes.Status201Created, JsonHelper.Serialize(drawnLine));
        }
        catch (Exception ex)
        {
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet(HubEvents.LoadCanvas)]
    [HubMethodName(HubEvents.LoadCanvas)]
    public async Task<IActionResult> LoadCanvas(
        [FromHeader(Name = Headers.Token)] string token,
        [FromHeader(Name = Headers.GameHash)] string gameHash
    )
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("Status: 400. Bad request.");

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

            logger.LogInformation("Status: 200. OK.");

            return StatusCode(StatusCodes.Status200OK, JsonHelper.Serialize(drawnLines));
        }
        catch (Exception ex)
        {
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpDelete(HubEvents.ClearCanvas)]
    [HubMethodName(HubEvents.ClearCanvas)]
    public async Task<IActionResult> ClearCanvas(
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

            if (token != game.GameState.DrawingToken)
            {
                logger.LogError("Status: 401. Unauthorized.");

                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                string connectionId = HttpContext.Request.Query["connectionId"];

                await hubContext.Clients
                    .Group(gameHash)
                    .SendAsync(HubEvents.OnClearCanvas);
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
}

using Dotnet.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Dotnet.Server.Http.Requests;
using Dotnet.Server.Managers;
using Microsoft.AspNetCore.SignalR;
using Dotnet.Server.Hubs;
using Dotnet.Server.Json;
using Dotnet.Server.Http;

namespace Dotnet.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly GamesManager gamesManager = new GamesManager(25);
    private readonly IHubContext<HubConnection> hubContext;
    private readonly ILogger<ChatController> logger;

    public ChatController(IHubContext<HubConnection> hubContext, ILogger<ChatController> logger)
    {
        this.hubContext = hubContext;
        this.logger = logger;
    }

    [HttpPost(HubEvents.SendChatMessage)]
    [HubMethodName(HubEvents.SendChatMessage)]
    public async Task<IActionResult> SendChatMessage(
        [FromHeader(Name = Headers.Token)] string token,
        [FromHeader(Name = Headers.GameHash)] string gameHash,
        [FromBody] SendChatMessageBody body
    )
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("Status: 400. Invalid request.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Game game = gamesManager.GetGameByHash(gameHash);

            if (game == null)
            {
                logger.LogError("Status: 404. Not found.");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            Player player = gamesManager.GetPlayerByToken(game, token);

            if (player == null)
            {
                logger.LogError("Status: 404. Not found.");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            if (token != game.GameState.DrawingToken)
            {
                logger.LogError("Status: 401. Unauthorized.");

                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            ChatMessage message = new ChatMessage()
            {
                Username = player.Username,
                Text = body.Text
            };

            gamesManager.AddChatMessage(gameHash, message);

            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                string connectionId = HttpContext.Request.Query["connectionId"];

                await hubContext.Clients
                    .Group(gameHash)
                    .SendAsync(HubEvents.OnSendChatMessage, JsonHelper.Serialize(message));
            }

            logger.LogInformation("Status: 201. Created.");

            return StatusCode(StatusCodes.Status201Created, JsonHelper.Serialize(message));
        }
        catch (Exception ex)
        {
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet(HubEvents.LoadChatMessages)]
    [HubMethodName(HubEvents.LoadChatMessages)]
    public async Task<IActionResult> LoadChatMessages(
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

            Player player = gamesManager.GetPlayerByToken(game, token);

            if (player == null)
            {
                logger.LogError("Status: 404. Not found.");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                string connectionId = HttpContext.Request.Query["connectionId"];

                await hubContext.Clients
                    .Client(connectionId)
                    .SendAsync(HubEvents.OnLoadChatMessages, JsonHelper.Serialize(game.ChatMessages));
            }

            logger.LogInformation("Status: 201. Created.");

            return StatusCode(StatusCodes.Status201Created, JsonHelper.Serialize(game.ChatMessages));
        }
        catch (Exception ex)
        {
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
using Dotnet.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Dotnet.Server.Http.Requests;
using Dotnet.Server.Managers;
using Microsoft.AspNetCore.SignalR;
using Dotnet.Server.Hubs;
using Dotnet.Server.Json;

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
    public async Task<IActionResult> SendChatMessage([FromBody] SendChatMessageBody requestBody)
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("Status: 400. Invalid received request body.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            bool gameExists = gamesManager.CheckIfGameExistsByHash(requestBody.GameHash);
            bool playerExists = gamesManager.CheckIfPlayerExistsByToken(requestBody.GameHash, requestBody.Token);

            if (!gameExists || !playerExists)
            {
                logger.LogError("Status: 404. Not found.");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            Game game = gamesManager.GetGameByHash(requestBody.GameHash);
            Player player = gamesManager.GetPlayerByToken(requestBody.GameHash, requestBody.Token);
            ChatMessage message = new ChatMessage()
            {
                Username = player.Username,
                Text = requestBody.Text
            };

            gamesManager.AddChatMessage(requestBody.GameHash, message);

            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                string connectionId = HttpContext.Request.Query["connectionId"];

                await hubContext.Clients
                    .Group(requestBody.GameHash)
                    .SendAsync(HubEvents.OnSendChatMessage, JsonHelper.Serialize(message));
            }

            return StatusCode(StatusCodes.Status201Created, message);
        }
        catch (Exception ex)
        {
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost(HubEvents.LoadChatMessages)]
    [HubMethodName(HubEvents.LoadChatMessages)]
    public async Task<IActionResult> LoadChatMessages([FromBody] SendChatMessageBody requestBody)
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("Status: 400. Invalid received request body.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            bool gameExists = gamesManager.CheckIfGameExistsByHash(requestBody.GameHash);
            bool playerExists = gamesManager.CheckIfPlayerExistsByToken(requestBody.GameHash, requestBody.Token);

            if (!gameExists || !playerExists)
            {
                logger.LogError("Status: 404. Not found.");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            Game game = gamesManager.GetGameByHash(requestBody.GameHash);
            Player player = gamesManager.GetPlayerByToken(requestBody.GameHash, requestBody.Token);
            ChatMessage message = new ChatMessage()
            {
                Username = player.Username,
                Text = requestBody.Text
            };

            gamesManager.AddChatMessage(requestBody.GameHash, message);

            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                string connectionId = HttpContext.Request.Query["connectionId"];

                await hubContext.Clients
                    .Client(connectionId)
                    .SendAsync(HubEvents.OnLoadChatMessages, JsonHelper.Serialize(message));
            }

            return StatusCode(StatusCodes.Status201Created, message);
        }
        catch (Exception ex)
        {
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
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
public class GameController : ControllerBase
{
    private readonly GamesManager gamesManager = new GamesManager(25);
    private readonly IHubContext<HubConnection> hubContext;
    private readonly ILogger<PlayerController> logger;

    public GameController(ILogger<PlayerController> logger, IHubContext<HubConnection> hubContext)
    {
        this.logger = logger;
    }

    [HttpPost("Create")]
    public IActionResult Create([FromBody] CreateGameBody requestBody)
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("Status: 400. Invalid received request body.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Game game = new Game() 
            {
                GameHash = Guid.NewGuid().ToString().Replace("-", ""),
                HostToken = Guid.NewGuid().ToString().Replace("-", ""),
                ChatMessages = new List<ChatMessage>(),
                GameSettings = new GameSettings(),
                DrawnLines = new List<DrawnLine>(),
                GameState = new GameState(),
                IsStarted = false
            };

            gamesManager.AddGame(game);

            CreateGameResponse response = new CreateGameResponse()
            {
                GameHash = game.GameHash,
                HostToken = game.HostToken
            };

            logger.LogInformation($"Status: 201. Game with the hash {game.GameHash} and the host username '{game.HostToken}' has been created.");
            
            return StatusCode(StatusCodes.Status201Created, JsonHelper.Serialize(response));
        }
        catch (Exception ex)
        {   
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpDelete("Remove")]
    public IActionResult Remove([FromBody] RemoveGameBody requestBody)
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

            Game game = gamesManager.GetGameByHash(requestBody.GameHash);

            if (game.HostToken != requestBody.HostToken)
            {
                logger.LogError("Status: 404. Unauthorized.");
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            gamesManager.RemoveGame(requestBody.GameHash);
            return StatusCode(StatusCodes.Status200OK);
        }
        catch (Exception ex)
        {   
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost(HubEvents.StartGame)]
    [HubMethodName(HubEvents.StartGame)]
    public async Task<IActionResult> StartGame([FromBody] SetSettingBody requestBody)
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

    [HttpPost("Exists")]
    public IActionResult Exists([FromBody] GameExistsBody requestBody)
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("Status: 400. Invalid received request body.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            bool gameExists = gamesManager.CheckIfGameExistsByHash(requestBody.GameHash);

            return StatusCode(StatusCodes.Status200OK, gameExists);
        }
        catch (Exception ex)
        {
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("GetHash")]
    public IActionResult GetHash([FromHeader] string token)
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("Status: 400. Invalid received request body.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Game game = gamesManager.GetGameByPlayerToken(token);

            if (game == null)
            {
                logger.LogError("Status: 404. Not found.");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            return StatusCode(StatusCodes.Status200OK, game.GameHash);

        }
        catch (Exception ex)
        {
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("GetAll")]
    public IActionResult GetAll()
    {
        return StatusCode(StatusCodes.Status200OK, gamesManager.GetAllGames());
    }
}
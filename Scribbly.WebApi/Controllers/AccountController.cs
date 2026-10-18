using Microsoft.AspNetCore.Mvc;
using Scribbly.Application.Abstractions.Persistence;
using Scribbly.Application.Features.Accounts;
using Scribbly.Domain.Entities;
using Scribbly.WebApi.Auth;
using Scribbly.WebApi.Models.HttpRequest;

namespace Scribbly.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAccountRepository _accounts;
    private readonly IAccountScoreService _scores;
    private readonly AccountSignInService _signIn;
    private readonly AccountSessionService _sessions;
    private readonly AccountSessionOptions _sessionOptions;

    public AccountController(IAccountRepository accounts, IAccountScoreService scores, AccountSignInService signIn,
        AccountSessionService sessions, AccountSessionOptions sessionOptions)
    {
        _accounts = accounts;
        _scores = scores;
        _signIn = signIn;
        _sessions = sessions;
        _sessionOptions = sessionOptions;
    }

    [HttpPost("Add")]
    public async Task<IActionResult> Add([FromBody] AddAccountBody body, CancellationToken cancellationToken)
    {
        AccountSignInResult? result = await _signIn.SignInAsync(body.IdToken, cancellationToken);
        if (result is null)
        {
            return Unauthorized();
        }
        string token = await _sessions.CreateAsync(result.Profile.Id, cancellationToken);
        AccountSessionCookie.Append(Response, token, _sessionOptions.Lifetime, Request.IsHttps);
        return result.Created ? StatusCode(StatusCodes.Status201Created, result.Profile) : Ok(result.Profile);
    }

    [HttpGet("Session")]
    public async Task<IActionResult> Session(CancellationToken cancellationToken)
    {
        AccountProfile? profile = await _sessions.GetProfileAsync(Request.Cookies[AccountSessionCookie.Name], cancellationToken);
        return profile is null ? Unauthorized() : Ok(profile);
    }

    [HttpPost("Logout")]
    public async Task<IActionResult> Logout(
        [FromHeader(Name = "X-Scribbly-Request")] string requestMarker,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(requestMarker, "1", StringComparison.Ordinal))
        {
            return BadRequest();
        }
        await _sessions.DeleteAsync(Request.Cookies[AccountSessionCookie.Name], cancellationToken);
        AccountSessionCookie.Delete(Response, Request.IsHttps);
        return NoContent();
    }

    [HttpPut("IncrementScore/{gameHash}")]
    public async Task<IActionResult> IncrementScore([FromRoute] string gameHash, CancellationToken cancellationToken)
    {
        string? roomAccessToken = BearerTokenResolver.FromHttpRequest(Request);
        string? accountId = await _sessions.GetAccountIdAsync(Request.Cookies[AccountSessionCookie.Name], cancellationToken);
        if (roomAccessToken is null || accountId is null)
        {
            return Unauthorized();
        }
        AccountScoreUpdateResult result = await _scores.SubmitAsync(gameHash, roomAccessToken, accountId, cancellationToken);
        return result.Status switch
        {
            AccountScoreUpdateStatus.InvalidCredential => Unauthorized(),
            AccountScoreUpdateStatus.Applied or AccountScoreUpdateStatus.AlreadyApplied => Ok(result.Score),
            AccountScoreUpdateStatus.AccountNotFound or AccountScoreUpdateStatus.PlayerNotFound => NotFound(),
            AccountScoreUpdateStatus.GameNotFinished => Conflict("Game has not finished yet."),
            AccountScoreUpdateStatus.ConflictingSubmission => Conflict(),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpGet("GetScore/{id}")]
    public async Task<IActionResult> GetScore([FromRoute] string id, CancellationToken cancellationToken)
    {
        Account? account = await _accounts.GetAccountAsync(id, cancellationToken);
        return account is null ? NotFound() : Ok(account.Score);
    }

    [HttpGet("GetTop")]
    public async Task<IActionResult> GetTopAccounts(CancellationToken cancellationToken) =>
        Ok(await _accounts.GetTopAccountPlayerScoresAsync(cancellationToken));
}

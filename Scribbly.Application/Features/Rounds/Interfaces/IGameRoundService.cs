using Scribbly.Application.Features.Prompts;
using Scribbly.Domain.Entities;

namespace Scribbly.Application.Features.Rounds;

public interface IGameRoundService
{
    Task<StartSessionResult> StartAuthorizedSessionAsync(
        string gameHash,
        string roomAccessToken,
        GameSettings settings,
        CancellationToken cancellationToken = default);
    Task<StartSessionResult> StartSessionAsync(
        string gameHash,
        GameSettings settings,
        CancellationToken cancellationToken = default);
    Task<StartSessionResult> ResumeSessionAsync(
        string gameHash,
        GameSettings settings,
        CancellationToken cancellationToken = default);
    Task<StartSessionResult> ResumeAuthorizedSessionAsync(
        string gameHash,
        string roomAccessToken,
        GameSettings settings,
        CancellationToken cancellationToken = default);
    Task<PrepareRoundResult> PrepareRoundAsync(string gameHash, CancellationToken cancellationToken = default);
    Task<StartTurnResult> StartTurnAsync(
        string gameHash,
        string drawingCredentialHash,
        CancellationToken cancellationToken = default);
    Task<RoundCommandStatus> ActivatePromptAsync(
        string gameHash,
        string drawingCredentialHash,
        GamePrompt prompt,
        CancellationToken cancellationToken = default);
    Task<TimerTickResult> InitializeTimerAsync(string gameHash, CancellationToken cancellationToken = default);
    Task<TimerTickResult> TickTimerAsync(string gameHash, CancellationToken cancellationToken = default);
    Task<CompleteTurnResult> CompleteTurnAsync(
        string gameHash,
        string drawingCredentialHash,
        CancellationToken cancellationToken = default);
    Task<RoundCommandStatus> RevealAnswerAsync(string gameHash, CancellationToken cancellationToken = default);
    Task<RoundCommandStatus> HideAnswerAsync(string gameHash, CancellationToken cancellationToken = default);
    Task<AdvanceRoundResult> AdvanceRoundAsync(string gameHash, CancellationToken cancellationToken = default);
    Task ResetSessionAsync(string gameHash, CancellationToken cancellationToken = default);
}

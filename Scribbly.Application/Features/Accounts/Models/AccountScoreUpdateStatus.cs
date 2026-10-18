namespace Scribbly.Application.Features.Accounts;

public enum AccountScoreUpdateStatus
{
    Applied,
    InvalidCredential,
    AlreadyApplied,
    AccountNotFound,
    PlayerNotFound,
    GameNotFinished,
    ConflictingSubmission
}

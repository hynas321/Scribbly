using Scribbly.Application.Features.Accounts;

namespace Scribbly.Application.Abstractions.Identity;

public interface IGoogleIdentityVerifier
{
    Task<VerifiedGoogleIdentity?> VerifyAsync(string idToken, CancellationToken cancellationToken);
}

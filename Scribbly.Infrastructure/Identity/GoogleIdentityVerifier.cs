using Google.Apis.Auth;
using Scribbly.Application.Abstractions.Identity;
using Scribbly.Application.Features.Accounts;
using Scribbly.Application.Features.Accounts.Configuration;

namespace Scribbly.Infrastructure.Identity;

public class GoogleIdentityVerifier : IGoogleIdentityVerifier
{
    private readonly string _clientId;

    public GoogleIdentityVerifier(AuthenticationOptions options) =>
        _clientId = options.GoogleClientId;

    public async Task<VerifiedGoogleIdentity?> VerifyAsync(string idToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idToken) || idToken.Length > 16384)
        {
            return null;
        }
        if (string.IsNullOrWhiteSpace(_clientId))
        {
            throw new InvalidOperationException(
                $"{AuthenticationOptions.SectionName}:{nameof(AuthenticationOptions.GoogleClientId)} must be configured to sign in.");
        }

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken,
                new GoogleJsonWebSignature.ValidationSettings { Audience = new[] { _clientId } })
                .WaitAsync(cancellationToken);
            return string.IsNullOrWhiteSpace(payload.Subject) || !payload.EmailVerified
                ? null
                : new VerifiedGoogleIdentity(payload.Subject, payload.Name ?? string.Empty, payload.Email ?? string.Empty);
        }
        catch (InvalidJwtException)
        {
            return null;
        }
    }
}

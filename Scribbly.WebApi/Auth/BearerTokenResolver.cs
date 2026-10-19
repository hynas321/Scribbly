namespace Scribbly.WebApi.Auth;

public static class BearerTokenResolver
{
    private const string Prefix = "Bearer ";

    public static string? FromHttpRequest(HttpRequest request, bool allowSignalRQueryFallback = false)
    {
        string authorization = request.Headers.Authorization.ToString();
        if (authorization.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
        {
            string token = authorization[Prefix.Length..].Trim();
            if (token.Length > 0)
            {
                return token;
            }
        }

        if (allowSignalRQueryFallback)
        {
            string queryToken = request.Query["access_token"].ToString().Trim();
            if (queryToken.Length > 0)
            {
                return queryToken;
            }
        }

        return null;
    }
}

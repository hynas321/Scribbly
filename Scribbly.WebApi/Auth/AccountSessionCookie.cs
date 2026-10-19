namespace Scribbly.WebApi.Auth;

public static class AccountSessionCookie
{
    public const string Name = "scribbly_account_session";

    public static void Append(HttpResponse response, string token, TimeSpan lifetime, bool secure)
    {
        response.Cookies.Append(Name, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = secure ? SameSiteMode.None : SameSiteMode.Lax,
            IsEssential = true,
            Path = "/",
            MaxAge = lifetime
        });
    }

    public static void Delete(HttpResponse response, bool secure) => response.Cookies.Delete(Name, new CookieOptions
    {
        HttpOnly = true,
        Secure = secure,
        SameSite = secure ? SameSiteMode.None : SameSiteMode.Lax,
        Path = "/"
    });
}

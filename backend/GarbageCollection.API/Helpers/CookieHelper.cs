namespace GarbageCollection.API.Helpers
{
    public static class CookieHelper
    {
        private const string AccessTokenCookieName = "accessToken";
        private const string RefreshTokenCookieName = "refreshToken";

        public static void SetAuthCookies(
            HttpResponse response,
            string accessToken,
            string refreshToken,
            IConfiguration configuration)
        {
            var accessExpiryMinutes = int.TryParse(configuration["Jwt:AccessTokenExpiryMinutes"], out var a) ? a : 15;
            var refreshExpiryDays = int.TryParse(configuration["Jwt:RefreshTokenExpiryDays"], out var r) ? r : 7;

            var baseOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,               // set to false in dev if not using HTTPS
                SameSite = SameSiteMode.Strict,
                Path = "/"
            };

            // Access token cookie
            var accessOptions = new CookieOptions
            {
                HttpOnly = baseOptions.HttpOnly,
                Secure = baseOptions.Secure,
                SameSite = baseOptions.SameSite,
                Path = baseOptions.Path,
                Expires = DateTimeOffset.UtcNow.AddMinutes(accessExpiryMinutes)
            };

            // Refresh token cookie
            var refreshOptions = new CookieOptions
            {
                HttpOnly = baseOptions.HttpOnly,
                Secure = baseOptions.Secure,
                SameSite = baseOptions.SameSite,
                Path = baseOptions.Path,
                Expires = DateTimeOffset.UtcNow.AddDays(refreshExpiryDays)
            };

            response.Cookies.Append(AccessTokenCookieName, accessToken, accessOptions);
            response.Cookies.Append(RefreshTokenCookieName, refreshToken, refreshOptions);
        }

        public static void ClearAuthCookies(HttpResponse response)
        {
            response.Cookies.Delete(AccessTokenCookieName);
            response.Cookies.Delete(RefreshTokenCookieName);
        }
    }
}

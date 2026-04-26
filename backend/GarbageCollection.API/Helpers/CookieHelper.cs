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
            var accessExpiryDays = int.TryParse(configuration["Jwt:AccessTokenExpiryDays"], out var a) ? a : 3;
            var refreshExpiryDays = int.TryParse(configuration["Jwt:RefreshTokenExpiryDays"], out var r) ? r : 7;

            var baseOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,               // set to false in dev if not using HTTPS
                SameSite = SameSiteMode.None,
                Path = "/"
            };

            // Access token cookie
            var accessOptions = new CookieOptions
            {
                HttpOnly = baseOptions.HttpOnly,
                Secure = baseOptions.Secure,
                SameSite = baseOptions.SameSite,
                Path = baseOptions.Path,
                Expires = DateTimeOffset.UtcNow.AddDays(accessExpiryDays)
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

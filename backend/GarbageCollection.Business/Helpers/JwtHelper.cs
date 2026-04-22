using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.Business.Helpers
{
    public sealed class JwtHelper
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _accessTokenExpiryMinutes;
        private readonly int _refreshTokenExpiryDays;

        public JwtHelper(IConfiguration configuration)
        {
            var jwtSection = configuration.GetSection("Jwt");
            _secretKey = jwtSection["SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey is missing.");
            _issuer = jwtSection["Issuer"] ?? "GarbageCollection";
            _audience = jwtSection["Audience"] ?? "GarbageCollection";
            _accessTokenExpiryMinutes = int.TryParse(jwtSection["AccessTokenExpiryMinutes"], out var a) ? a : 15;
            _refreshTokenExpiryDays = int.TryParse(jwtSection["RefreshTokenExpiryDays"], out var r) ? r : 7;
        }

        // ── Access Token ──────────────────────────────────────────────────────

        public (string token, DateTime expiresAt) GenerateAccessToken(string email, string fullName, int loginTerm)
        {
            var expiresAt = DateTime.UtcNow.AddMinutes(_accessTokenExpiryMinutes);

            var claims = new[]
            {
                new Claim(ClaimTypes.Email,      email),
                new Claim("full_name",           fullName),
                new Claim("login_term",          loginTerm.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = BuildToken(claims, expiresAt);
            return (token, expiresAt);
        }

        // ── Refresh Token ─────────────────────────────────────────────────────

        /// <summary>
        /// Returns the raw (unhashed) refresh token string and the JWT that wraps it,
        /// along with the expiry date so the caller can persist the hash.
        /// </summary>
        public (string rawToken, string jwt, DateTime expiresAt) GenerateRefreshToken(string email)
        {
            var expiresAt = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

            // 64 cryptographically random bytes → base64 raw token
            var rawBytes = RandomNumberGenerator.GetBytes(64);
            var rawToken = Convert.ToBase64String(rawBytes);

            var claims = new[]
            {
                new Claim("refresh_token", rawToken),
                new Claim(ClaimTypes.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var jwt = BuildToken(claims, expiresAt);
            return (rawToken, jwt, expiresAt);
        }
        public ClaimsPrincipal? ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,

                    ValidateAudience = true,
                    ValidAudience = _audience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return principal;
            }
            catch
            {
                return null;
            }
        }
        // ── SHA-256 hash ──────────────────────────────────────────────────────

        public static string HashToken(string rawToken)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        // ── Expiry accessors ──────────────────────────────────────────────────

        public int AccessTokenExpiryMinutes => _accessTokenExpiryMinutes;
        public int RefreshTokenExpiryDays => _refreshTokenExpiryDays;

        // ── Private helpers ───────────────────────────────────────────────────

        private string BuildToken(IEnumerable<Claim> claims, DateTime expiresAt)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiresAt,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}

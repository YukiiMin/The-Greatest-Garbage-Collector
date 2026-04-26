# 03 — Architecture

## Clean Architecture — 4 Layers

```
┌─────────────────────────────────────────────────┐
│         GarbageCollection.API                   │  Presentation
│  Controllers / Validators / Middleware / Program │
└──────────────────┬──────────────────────────────┘
                   │ uses interfaces
┌──────────────────▼──────────────────────────────┐
│         GarbageCollection.Business              │  Application Logic
│  Services / Helpers / Interfaces (IXxxService)  │
└──────────────────┬──────────────────────────────┘
                   │ uses interfaces
┌──────────────────▼──────────────────────────────┐
│         GarbageCollection.DataAccess            │  Data
│  Repositories / AppDbContext / Migrations        │
└──────────────────┬──────────────────────────────┘
                   │ references only
┌──────────────────▼──────────────────────────────┐
│         GarbageCollection.Common                │  Shared (no dependency)
│  Models / DTOs / Enums                          │
└─────────────────────────────────────────────────┘
```

**Dependency rule:** Mỗi layer chỉ reference vào layer bên dưới (hoặc Common). `Common` không import gì từ các layer khác.

**DI Inversion:** API và Business communicate qua interfaces (`IAdminService`, `IStaffRepository`, v.v.). Implementation đăng ký trong `Program.cs`.

---

## Middleware Pipeline (thứ tự trong Program.cs)

```
Request →
  1. ForwardedHeaders         X-Forwarded-For, X-Forwarded-Proto (Railway reverse proxy)
  2. UseCors("AllowFrontend") Cho phép cross-origin từ frontend URLs
  3. UseSwagger / SwaggerUI   Dev: expose /swagger endpoint
  4. UseAuthentication        Đọc cookie "accessToken" → validate JWT → set ClaimsPrincipal
  5. UseAuthorization         Kiểm tra [Authorize] attribute trên controller/action
  6. MapControllers           Route đến controller actions
→ Response
```

---

## CORS Configuration

Policy name: `"AllowFrontend"`

| Origin | Môi trường |
|--------|-----------|
| `https://ecoconnect-citizen.lovable.app` | Production (Citizen app) |
| `https://eco-connect-admin-re.lovable.app` | Production (Admin app) |
| `https://eco-connect-collector.lovable.app` | Production (Collector app) |
| `https://eco-conect-landing-page.lovable.app` | Production (Landing page) |
| `https://collect-garbage-production.up.railway.app` | Production (API self) |
| `http://localhost:3000` | Dev (React CRA) |
| `http://localhost:5173` | Dev (Vite) |
| `http://localhost:4200` | Dev (Angular) |

Config: `AllowAnyHeader()`, `AllowAnyMethod()`, `AllowCredentials()` (bắt buộc để cookie được gửi kèm request).

---

## Dependency Injection Registration

| Type | Lifetime | Số lượng |
|------|----------|---------|
| `JwtHelper` | **Singleton** | 1 — stateless, thread-safe |
| Repositories (IXxxRepository) | **Scoped** | 14 |
| Services (IXxxService) | **Scoped** | 16 |
| FluentValidation validators | **Scoped** (auto) | ~25 |

**Registration pattern (Program.cs):**
```csharp
// JwtHelper singleton
builder.Services.AddSingleton<JwtHelper>();

// Repositories scoped
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IStaffRepository, StaffRepository>();
// ... 14 total

// Services scoped
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IEnterpriseService, EnterpriseService>();
// ... 16 total

// FluentValidation — tự scan assembly
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
```

---

## JWT Authentication Setup

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Đọc token từ HttpOnly cookie "accessToken"
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                ctx.Token = ctx.Request.Cookies["accessToken"];
                return Task.CompletedTask;
            },
            // Custom 401 response
            OnChallenge = ctx => { ... return ApiResponse.Fail("UNAUTHORIZED") ... },
            // Custom 403 response
            OnForbidden = ctx => { ... return ApiResponse.Fail("FORBIDDEN") ... }
        };
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,         ValidIssuer = config["Jwt:Issuer"],
            ValidateAudience = true,       ValidAudience = config["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:SecretKey"])),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero      // Không có grace period
        };
    });
```

---

## Global Exception Handler

Bắt tất cả unhandled exception, map sang HTTP response chuẩn:

| Exception Type | HTTP Status | Error Code |
|----------------|-------------|------------|
| `KeyNotFoundException` | 404 | `NOT_FOUND` |
| `UnauthorizedAccessException` | 403 | `FORBIDDEN` |
| `InvalidOperationException` | 409 | `CONFLICT` |
| `TooManyRequestsException` | 429 | `TOO_MANY_REQUESTS` |
| `ArgumentException` | 400 | `BAD_REQUEST` |
| (all others) | 500 | `INTERNAL_SERVER_ERROR` |

Response luôn là `ApiResponse<object>` envelope (snake_case JSON). Xem format ở [06_BUSINESS_RULES.md](06_BUSINESS_RULES.md).

---

## JSON Serialization Config

```csharp
builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});
```

- `SnakeCaseLower`: `FullName` → `"full_name"`, `AvatarUrl` → `"avatar_url"`
- `JsonStringEnumConverter`: `ReportStatus.Pending` → `"Pending"` (không phải `1`)
- `WhenWritingNull`: fields null không xuất hiện trong response

---

## Cookie Configuration

| Cookie | HttpOnly | Secure | SameSite | Path | TTL |
|--------|----------|--------|----------|------|-----|
| `accessToken` | ✓ | ✓ | Strict | `/` | 15 phút |
| `refreshToken` | ✓ | ✓ | Strict | `/` | 7 ngày |

`HttpOnly`: JavaScript không thể đọc → chống XSS.
`Secure`: Chỉ gửi qua HTTPS → chống sniffing.
`SameSite=Strict`: Chỉ gửi với same-site request → chống CSRF.

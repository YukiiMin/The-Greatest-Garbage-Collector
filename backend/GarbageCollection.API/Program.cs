using System.Text;
using System.Reflection;
using FluentValidation;
using FluentValidation.AspNetCore;
using System.Text.Json;
using System.Text.Json.Serialization;
using CloudinaryDotNet;
using GarbageCollection.Common.Exceptions;
using DotNetEnv;
using GarbageCollection.Business.Helpers;
using GarbageCollection.Business.Interfaces;
using GarbageCollection.Business.Services;
using GarbageCollection.Common.Settings;
using GarbageCollection.DataAccess.Data;
using GarbageCollection.DataAccess.Interfaces;
using GarbageCollection.DataAccess.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

// ── 1. Nạp biến môi trường từ file .env ─────────────────────────────────────────
Env.Load();

// Khai báo sớm để dùng trong JwtBearer events và global exception handler
var exceptionJsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    Converters           = { new JsonStringEnumConverter() }
};

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

// ── 2. Database ────────────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// ── 3. Cloudinary ──────────────────────────────────────────────────────────────
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("Cloudinary"));

// ── 4. CORS ────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "https://ecoconnect-citizen.lovable.app",
                "https://eco-connect-admin-re.lovable.app",
                "https://eco-connect-collector.lovable.app",
                "https://eco-conect-landing-page.lovable.app",
                "https://collect-garbage-production.up.railway.app",
                "http://localhost:3000",
                "http://localhost:5173",
                "http://localhost:4200"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ── 5. JWT Authentication ──────────────────────────────────────────────────────
var jwtSection = builder.Configuration.GetSection("Jwt");
var secretKey  = jwtSection["SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey is missing.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // .NET 8 defaults to JsonWebTokenHandler which ignores claim type maps.
        // UseSecurityTokenValidators = true forces the old JwtSecurityTokenHandler
        // which correctly maps "role" → ClaimTypes.Role and "email" → ClaimTypes.Email,
        // so [Authorize(Roles = "...")] and User.GetEmail() both work.
        options.UseSecurityTokenValidators = true;
        options.MapInboundClaims = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtSection["Issuer"]   ?? "GarbageCollection",
            ValidAudience            = jwtSection["Audience"] ?? "GarbageCollection",
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };

        // Đọc access token từ HttpOnly cookie thay vì Authorization header
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                if (ctx.Request.Cookies.TryGetValue("accessToken", out var token))
                    ctx.Token = token;
                return Task.CompletedTask;
            },
            // Trả custom JSON thay vì 401 empty body
            OnChallenge = async ctx =>
            {
                ctx.HandleResponse();
                ctx.Response.StatusCode  = 401;
                ctx.Response.ContentType = "application/json";
                await ctx.Response.WriteAsJsonAsync(
                    new GarbageCollection.Common.DTOs.ApiResponse<object>
                    {
                        Status  = "failed",
                        Message = "unauthorized",
                        Data    = null,
                        Error   = new GarbageCollection.Common.DTOs.ApiError
                        {
                            Code        = "UNAUTHORIZED",
                            Description = "Access token is missing or invalid."
                        }
                    },
                    exceptionJsonOptions);
            },
            OnForbidden = async ctx =>
            {
                ctx.Response.StatusCode  = 403;
                ctx.Response.ContentType = "application/json";
                await ctx.Response.WriteAsJsonAsync(
                    new GarbageCollection.Common.DTOs.ApiResponse<object>
                    {
                        Status  = "failed",
                        Message = "forbidden",
                        Data    = null,
                        Error   = new GarbageCollection.Common.DTOs.ApiError
                        {
                            Code        = "FORBIDDEN",
                            Description = "You do not have permission to access this resource."
                        }
                    },
                    exceptionJsonOptions);
            }
        };
    });

// ── 6. Dependency Injection ────────────────────────────────────────────────────
builder.Services.AddSingleton<JwtHelper>();

// Repositories
builder.Services.AddScoped<ICitizenReportRepository,    CitizenReportRepository>();
builder.Services.AddScoped<IComplaintRepository,        ComplaintRepository>();
builder.Services.AddScoped<IEnterpriseRepository,       EnterpriseRepository>();
builder.Services.AddScoped<IEnterpriseStaffRepository,  EnterpriseStaffRepository>();
builder.Services.AddScoped<ICollectorRepository,        CollectorRepository>();
builder.Services.AddScoped<ICollectorHubRepository,     CollectorHubRepository>();
builder.Services.AddScoped<ICollectorStaffRepository,   CollectorStaffRepository>();
builder.Services.AddScoped<IPointCategoryRepository,    PointCategoryRepository>();
builder.Services.AddScoped<ITeamRepository,             TeamRepository>();
builder.Services.AddScoped<IUserRepository,             UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository,     RefreshTokenRepository>();
builder.Services.AddScoped<IEmailOtpRepository,         EmailOtpRepository>();
builder.Services.AddScoped<IPasswordOtpRepository,      PasswordOtpRepository>();
builder.Services.AddScoped<IPointTransactionRepository, PointTransactionRepository>();
builder.Services.AddScoped<ICollectorReportRepository,  CollectorReportRepository>();
builder.Services.AddScoped<ITeamSessionRepository,      TeamSessionRepository>();
builder.Services.AddScoped<IWorkAreaRepository,         WorkAreaRepository>();

// Services
builder.Services.AddScoped<IUploadImageService,  UploadImageService>();
builder.Services.AddScoped<ICitizenReportService, CitizenReportService>();
builder.Services.AddScoped<IComplaintService,   ComplaintService>();
builder.Services.AddScoped<IUserService,        UserService>();
builder.Services.AddScoped<IAuthService,        AuthService>();
builder.Services.AddScoped<ILocalAuthService,   LocalAuthService>();
builder.Services.AddScoped<ILocalLoginService,  LocalLoginService>();
builder.Services.AddScoped<IVerifyEmailService, VerifyEmailService>();
builder.Services.AddScoped<IEmailService,       SmtpEmailService>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
builder.Services.AddScoped<ICollectorReportService, CollectorReportService>();
builder.Services.AddScoped<IAdminService,      AdminService>();
builder.Services.AddScoped<IEnterpriseService, EnterpriseService>();
builder.Services.AddScoped<IWorkAreaService,   WorkAreaService>();

builder.Services.AddScoped<ICollectorService,  CollectorService>();
builder.Services.AddScoped<IResendOtpService, ResendOtpService>();
builder.Services.AddScoped<IPasswordOtpService, PasswordOtpService>();
builder.Services.AddScoped<IAccountVerificationService, AccountVerificationService>();

// Background services
builder.Services.AddHostedService<GarbageCollection.API.Helpers.PointsResetBackgroundService>();


// ── 7. Controllers + JSON ──────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    });

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 10 * 1024 * 1024;
});

// ── 8. Swagger ─────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "EcoConnect API",
        Version     = "v1",
        Description = "Test accounts — " +
                      "Admin: admin@ecoconnect.vn / Admin@123456 | " +
                      "Enterprise 1-3: enterprise@ecoconnect.vn / Enterprise@123456 | " +
                      "Collector staff 1-3: collectorstaff1@ecoconnect.vn / Collectorstaff@123456 | " +
                      "Citizen 1-10: citizen1@ecoconnect.vn / Citizen@123456"
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);

    // ── Group endpoints by role · resource ─────────────────────────────
    c.TagActionsBy(api =>
    {
        var route = api.RelativePath ?? "";
        string tag = route switch
        {
            // Auth
            _ when route.StartsWith("api/v1/auth")                                                   => "Auth",

            // Citizen
            _ when route.StartsWith("api/v1/users/citizen-reports") && route.Contains("complaints") => "Citizen · Complaints",
            _ when route.StartsWith("api/v1/users/citizen-reports")                                 => "Citizen · Reports",
            _ when route.StartsWith("api/v1/users/leaderboard")                                     => "Citizen · Leaderboard",
            _ when route.StartsWith("api/v1/users")                                                 => "Citizen · Profile",

            // Admin
            _ when route.StartsWith("api/v1/admin/complaints")                                      => "Admin · Complaints",
            _ when route.StartsWith("api/v1/admin/users")                                           => "Admin · Users",
            _ when route.StartsWith("api/v1/admin/enterprises") && route.Contains("/staff")         => "Admin · Enterprise Staff",
            _ when route.StartsWith("api/v1/admin/enterprises")                                     => "Admin · Enterprises",
            _ when route.StartsWith("api/v1/admin/work-areas")                                      => "Admin · Work Areas",
            _ when route.StartsWith("api/v1/admin/setup")                                           => "Admin · Setup",

            // Enterprise staff
            _ when route.StartsWith("api/v1/staff")                                                 => "Enterprise Staff · My Hub",
            _ when route.StartsWith("api/v1/enterprise/dashboard")                                  => "Enterprise · Overview",
            _ when route.StartsWith("api/v1/enterprise/reports")                                    => "Enterprise · Reports",
            _ when route.StartsWith("api/v1/enterprise/hubs")                                       => "Enterprise · My Hub",
            _ when route.StartsWith("api/v1/enterprise/collector-hubs")                             => "Enterprise · Collector Hubs",
            _ when route.StartsWith("api/v1/enterprise/collectors")                                 => "Enterprise · Collectors",
            _ when route.StartsWith("api/v1/enterprise/teams")                                      => "Enterprise · Teams",
            _ when route.StartsWith("api/v1/enterprise/point-categories")                           => "Enterprise · Point Categories",

            // Collector
            _ when route.StartsWith("api/v1/collector/dashboard")                                   => "Collector · Overview",
            _ when route.StartsWith("api/v1/collector/reports")                                     => "Collector · Reports",
            _ when route.StartsWith("api/v1/collector/my-hub")                                      => "Collector · My Hub",
            _ when route.StartsWith("api/v1/collector/staff")                                       => "Collector · Staff",

            _ => api.ActionDescriptor.RouteValues["controller"] ?? "Other"
        };
        return [tag];
    });

    // Sort within each tag: GET → POST → PUT → PATCH → DELETE, then by path
    var methodOrder = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        { ["GET"] = 0, ["POST"] = 1, ["PUT"] = 2, ["PATCH"] = 3, ["DELETE"] = 4 };
    c.OrderActionsBy(api =>
    {
        var order = methodOrder.TryGetValue(api.HttpMethod ?? "", out var o) ? o : 9;
        return $"{api.RelativePath}_{order}";
    });

    c.UseInlineDefinitionsForEnums();

    // Cookie-based auth (login trước rồi Swagger tự gửi cookie)
    c.AddSecurityDefinition("cookieAuth", new OpenApiSecurityScheme
    {
        Name        = "accessToken",
        Type        = SecuritySchemeType.ApiKey,
        In          = ParameterLocation.Cookie,
        Description = "HttpOnly cookie được set tự động sau khi login. Gọi POST /api/v1/auth/local-auth/login trước."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "cookieAuth" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ── 10. Global Exception Handler ───────────────────────────────────────────────

app.UseExceptionHandler(err => err.Run(async ctx =>
{
    var ex = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;

    ctx.Response.ContentType = "application/json";
    ctx.Response.StatusCode  = ex switch
    {
        KeyNotFoundException        => StatusCodes.Status404NotFound,
        UnauthorizedAccessException => StatusCodes.Status403Forbidden,
        InvalidOperationException   => StatusCodes.Status409Conflict,
        TooManyRequestsException    => StatusCodes.Status429TooManyRequests,
        ArgumentException           => StatusCodes.Status400BadRequest,
        _                           => StatusCodes.Status500InternalServerError
    };

    var errorCode = ctx.Response.StatusCode switch
    {
        404 => "NOT_FOUND",
        403 => "FORBIDDEN",
        409 => "CONFLICT",
        429 => "TOO_MANY_REQUESTS",
        400 => "BAD_REQUEST",
        _   => "INTERNAL_SERVER_ERROR"
    };

    await ctx.Response.WriteAsJsonAsync(
        new GarbageCollection.Common.DTOs.ApiResponse<object>
        {
            Status  = "failed",
            Message = ex?.Message ?? "Đã xảy ra lỗi không xác định.",
            Data    = null,
            Error   = new GarbageCollection.Common.DTOs.ApiError
            {
                Code        = errorCode,
                Description = ex?.Message ?? "Unknown error"
            }
        },
        exceptionJsonOptions);
}));

// ── 11. Middleware Pipeline ────────────────────────────────────────────────────
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseCors("AllowFrontend");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "EcoConnect API v1");
    c.RoutePrefix            = "swagger";
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List); // mỗi tag thu gọn, click để mở
    c.DefaultModelsExpandDepth(-1);  // ẩn phần Schemas ở dưới cho gọn
    c.DisplayRequestDuration();      // hiện thời gian request để debug
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


// ── 12. Seed ────────────────────────────────────────────────────────────────────
// RESEED_DB=true  → xóa toàn bộ và seed lại (dùng khi cần reset data)
// SEED_DB=true    → chỉ seed nếu bảng trống (mặc định development)
if (Environment.GetEnvironmentVariable("RESEED_DB") == "true")
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<GarbageCollection.DataAccess.Data.AppDbContext>();
    await GarbageCollection.DataAccess.Data.DbSeeder.ReseedAsync(db);
}
else if (app.Environment.IsDevelopment() || Environment.GetEnvironmentVariable("SEED_DB") == "true")
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<GarbageCollection.DataAccess.Data.AppDbContext>();
    await GarbageCollection.DataAccess.Data.DbSeeder.SeedAsync(db);
}

app.Run();

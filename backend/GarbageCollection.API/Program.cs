using System.Text;
using System.Reflection;
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
builder.Services.AddScoped<ICitizenReportRepository, CitizenReportRepository>();
builder.Services.AddScoped<IComplaintRepository,     ComplaintRepository>();
builder.Services.AddScoped<IEnterpriseRepository,    EnterpriseRepository>();
builder.Services.AddScoped<IStaffRepository,         StaffRepository>();
builder.Services.AddScoped<IPointCategoryRepository, PointCategoryRepository>();
builder.Services.AddScoped<ICollectorRepository,     CollectorRepository>();
builder.Services.AddScoped<ITeamRepository,          TeamRepository>();
builder.Services.AddScoped<IUserRepository,          UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository,  RefreshTokenRepository>();
builder.Services.AddScoped<IEmailOtpRepository,      EmailOtpRepository>();

// Services
builder.Services.AddScoped<ICloudinaryService,  CloudinaryService>();
builder.Services.AddScoped<ICitizenReportService, CitizenReportService>();
builder.Services.AddScoped<IComplaintService,   ComplaintService>();
builder.Services.AddScoped<IUserService,        UserService>();
builder.Services.AddScoped<IAuthService,        AuthService>();
builder.Services.AddScoped<ILocalAuthService,   LocalAuthService>();
builder.Services.AddScoped<ILocalLoginService,  LocalLoginService>();
builder.Services.AddScoped<IVerifyEmailService, VerifyEmailService>();
builder.Services.AddScoped<IEmailService,       SmtpEmailService>();

builder.Services.AddScoped<IResendOtpService, ResendOtpService>();
builder.Services.AddScoped<IAccountVerificationService, AccountVerificationService>();


// ── 7. Controllers + JSON ──────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    });

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
        Title   = "GarbageCollection API",
        Version = "v1"
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name        = "Authorization",
        Type        = SecuritySchemeType.Http,
        Scheme      = "Bearer",
        BearerFormat = "JWT",
        In          = ParameterLocation.Header,
        Description = "Nhập token theo định dạng: Bearer {token}"
    });

    c.UseInlineDefinitionsForEnums();
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
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
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GarbageCollection API v1");
    c.RoutePrefix = "swagger";
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();

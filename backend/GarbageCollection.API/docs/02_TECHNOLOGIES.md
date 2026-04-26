# 02 — Technologies & Packages

Tất cả công nghệ, package NuGet, và kỹ thuật được sử dụng trong project.

---

## Runtime & Framework

| Package | Version | Mục đích | Ghi chú |
|---------|---------|----------|---------|
| `.NET` | 8.0 | Runtime C# | LTS version |
| `ASP.NET Core Web API` | 8.0 | HTTP server, routing, middleware, DI container | Built-in |
| `Microsoft.AspNetCore.OpenApi` | 8.x | OpenAPI metadata | Kết hợp với Swagger |

---

## Database & ORM

| Package | Mục đích | Cách dùng |
|---------|----------|-----------|
| `PostgreSQL` | Database chính | Chạy local hoặc cloud (Railway) |
| `Microsoft.EntityFrameworkCore` 8.x | ORM: query, migrate, change tracking | LINQ queries trong repositories |
| `Npgsql.EntityFrameworkCore.PostgreSQL` 8.x | EF Core provider cho PostgreSQL | `UseNpgsql(connectionString)` trong `Program.cs` |
| `EFCore.NamingConventions` | Tự động snake_case column naming | `UseSnakeCaseNamingConvention()` trong `AppDbContext.OnConfiguring` |
| `Microsoft.EntityFrameworkCore.Design` | Hỗ trợ `dotnet ef migrations add` | Dev-time only |

**Lưu ý:** Bảng `citizen_reports` và `complaints` có cột PK là `"Id"` (PascalCase, quoted) vì migration khởi tạo trước khi áp convention. Xem chi tiết ở [04_DATABASE.md](04_DATABASE.md).

---

## Authentication & Security

| Package | Mục đích | File liên quan |
|---------|----------|----------------|
| `Microsoft.AspNetCore.Authentication.JwtBearer` 8.x | Validate JWT access token từ HttpOnly cookie | `Program.cs` (AddAuthentication) |
| `System.IdentityModel.Tokens.Jwt` | Generate + validate JWT token | `JwtHelper.cs` |
| `Microsoft.IdentityModel.Tokens` | `TokenValidationParameters`, `SymmetricSecurityKey`, `SigningCredentials` | `JwtHelper.cs` |
| `Google.Apis.Auth` | Validate Google ID token (Google OAuth) | `AuthService.cs` — `GoogleJsonWebSignature.ValidateAsync()` |
| `BCrypt.Net-Next` | Hash + verify password; hash OTP | `LocalAuthService`, `VerifyEmailService`, `PasswordOtpService` |
| `System.Security.Cryptography` (built-in) | SHA-256 hash refresh token; `RandomNumberGenerator.GetBytes(64)` | `JwtHelper.HashToken()`, `JwtHelper.GenerateRefreshToken()` |

**JWT flow:**
- Access token: 15 phút, claim: `email`, `full_name`, `login_term`, `jti`
- Refresh token: 7 ngày, raw 64-byte base64 → SHA-256 hash lưu DB
- Cookie: `HttpOnly=true`, `Secure=true`, `SameSite=Strict`

---

## Validation

| Package | Mục đích | Cách dùng |
|---------|----------|-----------|
| `FluentValidation.AspNetCore` **11.3.0** | Validate request DTO tự động, tích hợp `ModelState` | `AddFluentValidationAutoValidation()` + `AddValidatorsFromAssemblyContaining<Program>()` |
| `System.ComponentModel.DataAnnotations` (built-in) | `[Required]`, `[MaxLength]` trên DTO properties | DTO files trong `GarbageCollection.Common/DTOs/` |

**Cấu trúc validator:**
```
GarbageCollection.API/Validators/
├── ValidatorConstants.cs       — Shared regex: EmailRegex, PasswordRegex, PhoneRegex, OtpRegex
├── Auth/                       — 7 validators (register, login, otp, reset password...)
├── User/                       — ChangePassword, UpdateProfile
├── Admin/                      — Role, ban, enterprise, setup
├── Enterprise/                 — Collector, team, assign, reject, staff
├── Citizen/                    — CreateReport, UpdateReport
├── Complaint/                  — Create, resolve, send message
└── Collector/                  — StartShift, EndShift
```

**Regex patterns (ValidatorConstants.cs):**
```csharp
EmailRegex    = @"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$"
PasswordRegex = @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[!@#$%^&*()\-_=+\[\]{}|;':"",./<>?`~\\]).{8,16}$"
PhoneRegex    = @"^\d{10,11}$"
OtpRegex      = @"^\d{6}$"
```

---

## Image Upload

| Package | Mục đích | File liên quan |
|---------|----------|----------------|
| `CloudinaryDotNet` | Upload ảnh lên Cloudinary CDN, trả về URL | `UploadImageService.cs` |

**Cách dùng:**
```csharp
var uploadParams = new ImageUploadParams
{
    File = new FileDescription(fileName, stream),
    Folder = "waste-reports"
};
var result = await _cloudinary.UploadAsync(uploadParams);
var url = result.SecureUrl.ToString();
```

**Folders Cloudinary:**
- `waste-reports` — ảnh báo cáo từ citizen
- `citizen-reports` — ảnh từ collector khi thu gom
- `avatars` — ảnh đại diện user

---

## Email

| Package | Mục đích | File liên quan |
|---------|----------|----------------|
| `System.Net.Mail` (built-in) / MailKit | Gửi email OTP qua Gmail SMTP | `SmtpEmailService.cs` |

**Config:** `smtp.gmail.com:587`, TLS enabled, App Password (Google)

**Kỹ thuật Fire-and-forget:** Email gửi không-await trong background. Lỗi gửi mail không ảnh hưởng flow chính (đăng ký vẫn thành công):
```csharp
_ = Task.Run(() => _emailService.SendOtpAsync(email, otp), ct);
```

---

## Environment & Config

| Package | Mục đích | File liên quan |
|---------|----------|----------------|
| `DotNetEnv` | Load biến môi trường từ file `.env` | `Program.cs` đầu file |
| `Microsoft.Extensions.Configuration` (built-in) | Đọc `appsettings.json` + env vars | Inject `IConfiguration` |

**Config sections:**
- `Jwt:SecretKey`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:AccessTokenExpiryMinutes`, `Jwt:RefreshTokenExpiryDays`
- `Cloudinary:CloudName`, `Cloudinary:ApiKey`, `Cloudinary:ApiSecret`
- `Email:SmtpHost`, `Email:SmtpPort`, `Email:SmtpUser`, `Email:SmtpPassword`, `Email:FromAddress`
- `ConnectionStrings:DefaultConnection`
- `AllowedOrigins` (array)

---

## Serialization

| Kỹ thuật | Mô tả | Config ở đâu |
|----------|--------|--------------|
| `System.Text.Json` | Built-in JSON serializer | `Program.cs` `.AddControllers().AddJsonOptions()` |
| `JsonNamingPolicy.SnakeCaseLower` | Tự động convert PascalCase → snake_case trong JSON output | `JsonSerializerOptions.PropertyNamingPolicy` |
| `JsonStringEnumConverter` | Serialize enum thành string (`"Pending"` thay vì `1`) | `JsonSerializerOptions.Converters` |
| `[JsonPropertyName("...")]` | Override tên field khi naming policy không đủ (e.g., `Fullname` → `"full_name"`) | DTO files |

---

## API Documentation

| Package | Mục đích |
|---------|----------|
| `Swashbuckle.AspNetCore` | Generate Swagger UI + OpenAPI JSON tự động |

**Endpoint:** `/swagger`
**Features:** Bearer token input trong UI, XML comments từ code.

---

## Logging

`Microsoft.Extensions.Logging` (built-in) — `ILogger<T>` được inject vào mọi service:

| Level | Dùng khi |
|-------|----------|
| `LogInformation` | Flow bình thường (login thành công, token rotation) |
| `LogWarning` | Security events (Google token invalid, refresh token reuse, user banned) |
| `LogError` | Unexpected exceptions |

---

## Hosting & Deployment

| Package | Mục đích |
|---------|----------|
| `Microsoft.AspNetCore.HttpOverrides` | Xử lý `X-Forwarded-For`, `X-Forwarded-Proto` khi deploy behind Railway reverse proxy |

---

## Kỹ thuật đặc biệt

| Kỹ thuật | Mô tả chi tiết |
|----------|----------------|
| **Refresh Token Rotation** | Mỗi lần refresh (`GET /auth/account-auth/license`): revoke token cũ → generate token mới → lưu hash mới. Token chỉ dùng được 1 lần. |
| **Reuse Detection** | Nếu client gửi token đã bị revoke → server revoke toàn bộ refresh tokens của user + tăng `login_term` → mọi access token cũ trở thành invalid. Ngăn tấn công token replay. |
| **SHA-256 Refresh Token** | Raw refresh token (64 bytes base64) không bao giờ lưu DB. Chỉ lưu `SHA-256(raw)` dạng hex lowercase. DB leak không expose raw token. |
| **BCrypt OTP Hash** | OTP 6 số (email verification, password reset) cũng được BCrypt hash trước khi lưu DB. Verify qua `BCrypt.Verify(input, hash)`. |
| **Login Term Invalidation** | JWT access token chứa claim `login_term`. AccountVerificationService kiểm tra claim này với DB. Khi reuse detected → increment DB `login_term` → toàn bộ access token cũ (có `login_term` cũ) fail validation. |
| **JSONB PostgreSQL** | Các field list được lưu dạng JSONB: `CitizenImageUrls[]`, `CollectorImageUrls[]`, `Types[]` (báo cáo), `Messages[]` (complaint), `ImageUrls[]` (complaint), `Mechanic` (point category). EF Core dùng custom ValueConverter để serialize/deserialize. |
| **snake_case EF Convention** | `UseSnakeCaseNamingConvention()` tự động map `MyProperty` → `my_property` trong DB. **Ngoại lệ:** `citizen_reports."Id"` và `complaints."Id"` là PascalCase (quoted) vì migration khởi tạo trước khi áp convention. Raw SQL với các bảng này phải dùng `WHERE "Id" = @id`. |
| **Fire-and-forget Email** | Email OTP gửi qua `_ = Task.Run(...)` không block request. Lỗi email không fail request. |
| **UserPoints Upsert** | Leaderboard dùng `INSERT INTO user_points ... ON CONFLICT (user_id) DO UPDATE SET ...` để upsert điểm nguyên tử. |
| **Enterprise Auth by Email** | Không có FK trực tiếp giữa `users` và `enterprise_hub`. Auth bằng: `Enterprise.Email == User.Email`. Khi admin setup enterprise user → tạo `Enterprise` entity với email của User. |
| **Staff Soft-Unassign** | Khi remove staff khỏi team: không xóa record `staffs`, chỉ SET `collector_id=null`, `team_id=null`, `join_team_at=null`. Staff vẫn thuộc Enterprise. |
| **Single Session** | Khi login → revoke ALL refresh tokens cũ của user → chỉ 1 session active tại 1 thời điểm. |

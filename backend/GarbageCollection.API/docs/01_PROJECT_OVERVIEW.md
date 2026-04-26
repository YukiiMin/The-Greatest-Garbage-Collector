# 01 — Project Overview

## Tên dự án
**EcoConnect** — Hệ thống quản lý thu gom rác thải đô thị

## Mục tiêu
Kết nối 3 bên:
1. **Citizen** báo cáo điểm rác thải qua app
2. **Enterprise** (công ty thu gom) xét duyệt và phân công
3. **Collector** (đội thu gom) thực hiện thu gom thực địa

**Admin** quản lý toàn bộ hệ thống, setup tài khoản, xử lý khiếu nại.

---

## Deployed URL

```
https://collect-garbage-production.up.railway.app
```

Swagger UI: `https://collect-garbage-production.up.railway.app/swagger`

---

## Actors

| Role | Enum | Mô tả | Quyền chính |
|------|------|--------|-------------|
| Citizen | 1 | Người dân | Tạo/xem/update/cancel báo cáo rác; tạo khiếu nại; xem leaderboard |
| Collector | 2 | Nhân viên thu gom | Xem báo cáo được giao; start/end ca; submit kết quả thu gom |
| Enterprise | 3 | Công ty thu gom | Quản lý collector/team/staff; duyệt và phân công báo cáo |
| Admin | 4 | Quản trị viên | Quản lý user (ban/role); CRUD enterprise; xử lý khiếu nại |

---

## Tech Stack (tóm tắt)

| Lớp | Công nghệ |
|-----|----------|
| Runtime | .NET 8 (C#) |
| Framework | ASP.NET Core 8 Web API |
| Database | PostgreSQL |
| ORM | Entity Framework Core 8 + Npgsql |
| Auth | JWT Bearer (HttpOnly cookie) + Google OAuth |
| Validation | FluentValidation 11.3.0 |
| Image | Cloudinary CDN |
| Email | Gmail SMTP |
| Deploy | Railway (Docker container) |

Chi tiết đầy đủ: [02_TECHNOLOGIES.md](02_TECHNOLOGIES.md)

---

## Folder Structure

```
backend/
├── GarbageCollection.API/                  # Layer: Presentation
│   ├── Controllers/                        # 7 controllers, 50+ HTTP endpoints
│   │   ├── AuthController.cs               # Đăng ký, đăng nhập, token
│   │   ├── UsersController.cs              # Profile, leaderboard
│   │   ├── CitizenReportController.cs      # CRUD báo cáo rác
│   │   ├── ComplaintsController.cs         # Khiếu nại + tin nhắn
│   │   ├── EnterpriseController.cs         # Collector/team/report management
│   │   ├── CollectorController.cs          # Ca làm, thu gom
│   │   ├── AdminController.cs              # Admin tools
│   │   └── ImageController.cs             # Upload Cloudinary
│   ├── Validators/                         # ~25 FluentValidation validators
│   │   ├── ValidatorConstants.cs           # Shared regex patterns
│   │   ├── Auth/                           # 7 auth validators
│   │   ├── User/                           # 2 user validators
│   │   ├── Admin/                          # 4 admin validators
│   │   ├── Enterprise/                     # 6 enterprise validators
│   │   ├── Citizen/                        # 2 citizen validators
│   │   ├── Complaint/                      # 3 complaint validators
│   │   └── Collector/                      # 2 collector validators
│   └── Program.cs                          # DI, middleware, JWT, CORS config
│
├── GarbageCollection.Business/             # Layer: Application Logic
│   ├── Interfaces/                         # 16 service interfaces (IXxxService)
│   ├── Services/                           # 16 service implementations
│   └── Helpers/                            # JwtHelper, OtpHelper, PasswordHelper, ...
│
├── GarbageCollection.Common/              # Layer: Shared Contracts (không dependency)
│   ├── Models/                             # 14 EF Core entity models
│   ├── DTOs/                               # ~50 request/response DTOs
│   │   ├── Auth/, User/, Admin/
│   │   ├── CitizenReport/, Complaint/
│   │   ├── Collector/, Enterprise/
│   │   └── Leaderboard/
│   └── Enums/                              # UserRole, WasteType, ReportStatus, ...
│
├── GarbageCollection.DataAccess/          # Layer: Data
│   ├── Data/AppDbContext.cs                # EF Core DbContext (13 DbSets, full column config)
│   ├── Interfaces/                         # 14 repository interfaces
│   ├── Repositories/                       # 14 repository implementations
│   └── Migrations/                         # EF Core migration history
│
└── docs/                                   # Tài liệu dự án (thư mục này)
```

---

## Environment Variables

Đặt trong file `.env` ở root `backend/`:

```
# Database
DB_HOST=...
DB_PORT=5432
DB_NAME=...
DB_USER=...
DB_PASSWORD=...

# JWT
Jwt__SecretKey=<chuỗi bí mật dài ≥32 ký tự>
Jwt__Issuer=GarbageCollection
Jwt__Audience=GarbageCollection
Jwt__AccessTokenExpiryMinutes=15
Jwt__RefreshTokenExpiryDays=7

# Cloudinary
Cloudinary__CloudName=...
Cloudinary__ApiKey=...
Cloudinary__ApiSecret=...

# Email (Gmail SMTP)
Email__SmtpHost=smtp.gmail.com
Email__SmtpPort=587
Email__SmtpUser=...
Email__SmtpPassword=...
Email__FromAddress=...
Email__FromName=EcoConnect
```

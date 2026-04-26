# Bug Fix & Feature Report — API Testing Session

> Branch: BE | Cập nhật: 2026-04-26

---

## Bug 1 — Token Refresh luôn trả về `INVALID_REFRESH_TOKEN`

**File:** `GarbageCollection.Business/Services/AuthService.cs`

**Lỗi cụ thể:**
- Field `_refreshTokenValidationParams` (dòng 32) được khai báo nhưng **không bao giờ được gán giá trị** trong constructor.
- Khi `IssueLicenseAsync` gọi `handler.ValidateToken(rawRefreshTokenJwt, _refreshTokenValidationParams, out _)`, giá trị là `null` → ném `NullReferenceException` → catch block trả về `401 INVALID_REFRESH_TOKEN`.

**Kết quả gọi API trước fix:**
```json
{ "status": "failed", "message": "no license", "code": "INVALID_REFRESH_TOKEN" }
```
(Xảy ra với mọi refresh token hợp lệ, không có ngoại lệ.)

**Fix:**
1. Thêm method `GetRefreshTokenValidationParams()` vào `JwtHelper.cs` — trả về `TokenValidationParameters` với `ValidateLifetime = false` (để token hết hạn vẫn parse được, expiry check thực sự nằm ở DB `ExpiresAt`).
2. Gán field trong constructor `AuthService`:
   ```csharp
   _refreshTokenValidationParams = jwtHelper.GetRefreshTokenValidationParams();
   ```

**Files đã sửa:**
- `GarbageCollection.Business/Helpers/JwtHelper.cs` — thêm method `GetRefreshTokenValidationParams()`
- `GarbageCollection.Business/Services/AuthService.cs` — gán field trong constructor

---

## Bug 2 — `POST /complaint/{id}/message` trả về 500

**File:** `GarbageCollection.DataAccess/Repositories/ComplaintRepository.cs` (dòng 57–61)

**Lỗi cụ thể:**
PostgreSQL error `42703: column "id" does not exist`

**Nguyên nhân gốc rễ:**
Migration `20260423105231_InitialCreate.cs` tạo bảng `complaints` với cột primary key là:
```csharp
Id = table.Column<Guid>(type: "uuid", nullable: false),
```
EF Core dùng dấu ngoặc kép khi generate SQL → cột thực tế trong DB là `"Id"` (có phân biệt hoa thường — PostgreSQL case-sensitive với quoted identifiers).

Raw SQL cũ dùng `id` (không có quotes) → PostgreSQL fold về lowercase → không tìm thấy cột → 500.

Ngoài ra, cú pháp `@msg::jsonb` trong Npgsql raw SQL có thể bị parse sai (Npgsql không nhận ra `@msg` nếu có `::` ngay sau).

**SQL cũ (lỗi):**
```sql
UPDATE complaints
SET messages = COALESCE(messages, '[]'::jsonb) || @msg::jsonb
WHERE id = @id
```

**SQL mới (đã fix):**
```sql
UPDATE complaints
SET messages = COALESCE(messages, '[]'::jsonb) || CAST(@msg AS jsonb)
WHERE "Id" = @id
```

**Files đã sửa:**
- `GarbageCollection.DataAccess/Repositories/ComplaintRepository.cs` — dòng 58

---

## Bug 3 — Upload ảnh không cần đăng nhập

**File:** `GarbageCollection.API/Controllers/ImageController.cs`

**Lỗi cụ thể:**
Endpoint `POST /api/v1/image/upload` không có attribute `[Authorize]`. Bất kỳ client nào (không có token) đều có thể upload file lên Cloudinary → tốn quota, bảo mật kém.

**Fix:**
Thêm `[Authorize]` lên class `ImageController`:
```csharp
[ApiController]
[Authorize]          // <-- thêm mới
[Route("api/v1/[controller]")]
public class ImageController : ControllerBase
```

**Files đã sửa:**
- `GarbageCollection.API/Controllers/ImageController.cs`

---

## Bug 4 — `POST /user/change-password` không nhận được body

**File:** `GarbageCollection.Common/DTOs/User/ChangePasswordRequest.cs`

**Lỗi cụ thể:**
`ChangePasswordData` có các property PascalCase (`OldPassword`, `NewPassword`, `LogoutAllDevices`) nhưng global JSON config của project dùng `snake_case_naming_policy`. Khi client gửi:
```json
{ "data": { "old_password": "...", "new_password": "...", "logout_all_devices": true } }
```
ASP.NET Core không map được vào properties → giá trị là `null`/`false` → validation fail hoặc business logic nhận dữ liệu sai.

**Fix:**
Thêm `[JsonPropertyName]` cho từng property:
```csharp
[JsonPropertyName("old_password")]
public string OldPassword { get; set; }

[JsonPropertyName("new_password")]
public string NewPassword { get; set; }

[JsonPropertyName("logout_all_devices")]
public bool LogoutAllDevices { get; set; }
```

**Files đã sửa:**
- `GarbageCollection.Common/DTOs/User/ChangePasswordRequest.cs`

---

## Bug 5 — `GET /user/profile` trả về sai tên field

**File:** `GarbageCollection.Common/DTOs/User/UserProfileDto.cs`

**Lỗi cụ thể:**
Response JSON trả về:
```json
{
  "Fullname": "Nguyen Van A",
  "AvatarUrl": "...",
  "CreatedAt": "...",
  "UpdatedAt": "..."
}
```
Thay vì snake_case chuẩn:
```json
{
  "full_name": "Nguyen Van A",
  "avatar_url": "...",
  "created_at": "...",
  "updated_at": "..."
}
```

**Nguyên nhân:** Các property trong `UserProfileDto` không có `[JsonPropertyName]`, trong khi global `JsonNamingPolicy` không đủ để xử lý `Fullname` → `full_name` (chỉ thêm underscore trước uppercase, nhưng `Fullname` là một từ liền).

**Fix:**
Thêm `[JsonPropertyName]` cho các fields cần:
```csharp
[JsonPropertyName("full_name")]
public string Fullname { get; set; }

[JsonPropertyName("avatar_url")]
public string? AvatarUrl { get; set; }

[JsonPropertyName("created_at")]
public DateTime CreatedAt { get; set; }

[JsonPropertyName("updated_at")]
public DateTime? UpdatedAt { get; set; }
```

**Files đã sửa:**
- `GarbageCollection.Common/DTOs/User/UserProfileDto.cs`

---

## Feature 6 — Thiết kế lại work_area (WorkArea restructure)

### Vấn đề trước đây (4 bug tiềm ẩn)

| # | Vấn đề | Hệ quả |
|---|--------|--------|
| 1 | `User.WorkArea` và `User.Area` là text tự do, không chuẩn hóa | Leaderboard scope=Area luôn broken vì `user_points.work_area_name` không bao giờ được gán |
| 2 | `Team.work_area_id` là UUID orphan — không có FK constraint, không có entity tương ứng | EF Core không enforce integrity, có thể chứa UUID rác |
| 3 | `Enterprise.WorkArea` và `Collector.WorkArea` là free text | Không thể validate collector thuộc đúng quận của enterprise |
| 4 | Không có API để quản lý/liệt kê work areas | Admin không thể tạo/assign area có cấu trúc |

### Thiết kế mới

**Bảng `work_areas` — tự tham chiếu (self-referencing hierarchy):**
```sql
CREATE TABLE work_areas (
    id         UUID PRIMARY KEY,
    name       VARCHAR(256) NOT NULL,
    type       VARCHAR(50) NOT NULL,     -- 'District' | 'Ward'
    parent_id  UUID NULL REFERENCES work_areas(id) ON DELETE SET NULL,
    created_at TIMESTAMPTZ NOT NULL
);
```

**Hierarchy:** Ward.parent_id → District (parent_id = null nghĩa là District)

**Business rule chính:**
> Collector.work_area phải là Ward có `parent_id == Enterprise.work_area_id`
> → Enterprise quản lý District nào thì Collector chỉ được gán Ward thuộc District đó

### Schema thay đổi (migration `AddWorkAreasTable`)

| Bảng | Xóa | Thêm |
|------|-----|------|
| `work_areas` | — | Tạo mới toàn bộ |
| `enterprise_hub` | `work_area TEXT` | `work_area_id UUID NULL FK → work_areas` |
| `collector_hub` | `work_area TEXT` | `work_area_id UUID NULL FK → work_areas` |
| `teams` | — | FK constraint cho `work_area_id` (đã có column, thiếu FK) |
| `users` | `work_area TEXT`, `area TEXT` | `work_area_id UUID NULL FK → work_areas` |

### Files đã tạo mới (7 files)

| File | Mô tả |
|------|-------|
| `GarbageCollection.Common/Models/WorkArea.cs` | Model entity với nav properties `Parent` + `Children` |
| `GarbageCollection.Common/DTOs/Admin/WorkAreaDto.cs` | Response DTO + `SaveWorkAreaRequest` |
| `GarbageCollection.DataAccess/Interfaces/IWorkAreaRepository.cs` | Interface CRUD + `HasDependentsAsync` |
| `GarbageCollection.DataAccess/Repositories/WorkAreaRepository.cs` | Triển khai EF Core, include Parent/Children khi query |
| `GarbageCollection.Business/Interfaces/IWorkAreaService.cs` | Interface service |
| `GarbageCollection.Business/Services/WorkAreaService.cs` | Validate hierarchy, District/Ward type check |
| `GarbageCollection.API/Validators/Admin/SaveWorkAreaRequestValidator.cs` | FluentValidation: name, type enum, parent_id required nếu Ward |

### Files đã sửa (13 files)

**Models:**
- `Enterprise.cs` — `string WorkArea` → `Guid? WorkAreaId` + `WorkArea? WorkArea` (nav)
- `Collector.cs` — tương tự Enterprise
- `Team.cs` — thêm `WorkArea? WorkArea` nav property
- `User.cs` — xóa `string? WorkArea` + `string? Area`, thêm `Guid? WorkAreaId` + `WorkArea? WorkArea` nav

**DTOs:**
- `AdminEnterpriseDto.cs` — `WorkArea string` → `WorkAreaId Guid?` + `WorkAreaName string?`
- `AdminSetupEnterpriseRequest.cs` — `WorkArea string` → `WorkAreaId Guid?` (optional)
- `CollectorDto.cs` — tương tự AdminEnterpriseDto; thêm `[JsonPropertyName]` cho tất cả fields
- `UpdateUserProfileRequest.cs` — thêm `WorkAreaId Guid?`
- `UserProfileDto.cs` — thêm `WorkAreaId Guid?`

**AppDbContext:**
- Thêm `DbSet<WorkArea> WorkAreas`
- Thêm config entity `WorkArea` với self-referencing FK
- Enterprise: `work_area text` → `work_area_id uuid` + HasOne FK
- Collector: tương tự Enterprise
- Team: thêm `HasOne(t => t.WorkArea)` FK
- User: xóa `WorkArea` + `Area` mapping, thêm `work_area_id` + HasOne FK

**Services:**
- `AdminService.cs` — `WorkArea = req.Data.WorkArea` → `WorkAreaId = req.Data.WorkAreaId` trong Create/Update/Setup; fix `MapToEnterpriseDto`
- `EnterpriseService.cs` — inject `IWorkAreaRepository`; thêm validation ward thuộc district trong `CreateCollectorAsync` + `UpdateCollectorAsync`; fix `MapToCollectorDto`
- `UserService.cs` — inject `IWorkAreaRepository` + `IUserPointsRepository`; trong `UpdateProfileAsync` validate Ward type + ghi cache `user_points.work_area_name` để fix leaderboard scope=Area

**Repositories:**
- `EnterpriseRepository.cs` — thêm `.Include(e => e.WorkArea)` vào GetById/GetByEmail/GetAll
- `CollectorRepository.cs` — thêm `.Include(c => c.WorkArea)` vào GetById/GetByEnterpriseId
- `IUserPointsRepository.cs` + `UserPointsRepository.cs` — thêm `UpdateWorkAreaNameAsync`

**Controllers + DI:**
- `AdminController.cs` — inject `IWorkAreaService`; thêm 5 endpoints WorkArea CRUD
- `Program.cs` — đăng ký `IWorkAreaRepository` + `IWorkAreaService`

**Validators:**
- `AdminSetupEnterpriseRequestValidator.cs` — xóa rule `WorkArea required` (field không còn tồn tại)
- `SaveAdminEnterpriseRequestValidator.cs` — tương tự
- `SaveCollectorRequestValidator.cs` — tương tự

### API mới thêm (`/api/v1/admin/work-areas`)

| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/admin/work-areas` | Danh sách (filter: `?type=District\|Ward`) |
| GET | `/admin/work-areas/{id}` | Chi tiết + children |
| POST | `/admin/work-areas` | Tạo District hoặc Ward |
| PATCH | `/admin/work-areas/{id}` | Cập nhật name/type/parent |
| DELETE | `/admin/work-areas/{id}` | Xóa nếu không có entity FK vào |

**Request tạo District:**
```json
{ "data": { "name": "Quận 1", "type": "District" } }
```

**Request tạo Ward:**
```json
{ "data": { "name": "Phường Bến Nghé", "type": "Ward", "parent_id": "<district-id>" } }
```

### Validation logic (EnterpriseService)

```
POST/PATCH /enterprise/collectors với work_area_id:
  1. workArea = GetById(work_area_id)
  2. if workArea.Type != "Ward" → 422 INVALID_WORK_AREA
  3. if enterprise.WorkAreaId != null && workArea.ParentId != enterprise.WorkAreaId
     → 409 WORK_AREA_MISMATCH "This ward does not belong to the enterprise's district"
```

### Fix leaderboard scope=Area

Trước: `user_points.work_area_name` không bao giờ được gán → scope=Area luôn trả về empty.

Sau: Khi citizen gọi `PATCH /users/profile` với `work_area_id` → service validate Ward type → ghi `workArea.Name` vào `user_points.work_area_name` (denormalized cache) → leaderboard filter theo tên phường hoạt động đúng.

---

## Fix 7 — Đổi `DELETE` → `PATCH` cho endpoint cancel báo cáo

**File:** `GarbageCollection.API/Controllers/CitizenReportController.cs`

**Thay đổi:**

| | Trước | Sau |
|---|-------|-----|
| Method | `DELETE` | `PATCH` |
| URL | `/api/v1/users/citizen-reports/{id}` | `/api/v1/users/citizen-reports/{id}/cancel` |

**Lý do:**
- `DELETE` ngụ ý xóa dữ liệu vĩnh viễn — không đúng vì hành động này chỉ chuyển trạng thái báo cáo sang `Cancelled`.
- `PATCH` + suffix `/cancel` thể hiện đúng bản chất: state transition, record vẫn còn trong DB.
- Nhất quán với các endpoint khác cùng pattern: `/queue`, `/assign`, `/reject`, `/complete`.

**Files đã sửa:**
- `GarbageCollection.API/Controllers/CitizenReportController.cs` — dòng 151

---

---

## Fix 8 — `GET /users/leaderboard?scope=Area` trao kết quả sai khi user chưa chọn phường

**Files:**
- `GarbageCollection.Business/Services/LeaderboardService.cs`
- `GarbageCollection.API/Controllers/UsersController.cs`

**Lỗi cụ thể:**

Khi user gọi `scope=Area` nhưng chưa set `work_area_id` trong profile thì `UserPoints.WorkAreaName = null`.

Repository filter:
```csharp
if (scope == LeaderboardScope.Area && workAreaName != null)
    query = query.Where(p => p.WorkAreaName == workAreaName);
```
Vì `workAreaName == null` → điều kiện if bị bỏ qua → filter không được áp dụng → trả về **toàn bộ leaderboard** thay vì báo lỗi. User nhận dữ liệu sai mà không biết.

**Fix:**

Trong `LeaderboardService.GetLeaderboardAsync()`, thêm guard trước khi query:
```csharp
if (scope == LeaderboardScope.Area && string.IsNullOrEmpty(myWorkArea))
    throw new InvalidOperationException("WORK_AREA_NOT_SET");
```

Trong `UsersController.GetLeaderboard()`, catch exception:
```csharp
catch (InvalidOperationException ex) when (ex.Message == "WORK_AREA_NOT_SET")
{
    return UnprocessableEntity(ApiResponse<object>.Fail(
        "work area not set", "WORK_AREA_NOT_SET",
        "Ban chua chon phuong cu tru..."));
}
```

**Kết quả:** Trả về `422 WORK_AREA_NOT_SET` thay vì dữ liệu sai.

---

## Fix 9 — `WeekPoints`/`MonthPoints`/`YearPoints` không bao giờ reset → period filter vô nghĩa

**Files:**
- `GarbageCollection.DataAccess/Interfaces/IUserPointsRepository.cs`
- `GarbageCollection.DataAccess/Repositories/UserPointsRepository.cs`
- `GarbageCollection.API/Helpers/PointsResetBackgroundService.cs` *(mới tạo)*
- `GarbageCollection.API/Program.cs`

**Lỗi cụ thể:**

Trong `CollectorReportRepository.CollectWithPointsAsync()`:
```csharp
userPoints.WeekPoints  += pointsEarned;
userPoints.MonthPoints += pointsEarned;
userPoints.YearPoints  += pointsEarned;
userPoints.TotalPoints += pointsEarned;
```
Cả 4 counter tăng giống nhau, không có code nào reset. Sau tuần đầu tiên, `WeekPoints == MonthPoints == YearPoints == TotalPoints` → leaderboard `period=Week` và `period=Month` cho kết quả y hệt nhau.

**Fix:**

1. Thêm 3 method vào `IUserPointsRepository`:
   - `ResetWeekPointsAsync()` — bulk update `week_points = 0`
   - `ResetMonthPointsAsync()` — bulk update `month_points = 0`
   - `ResetYearPointsAsync()` — bulk update `year_points = 0`
   (dùng `ExecuteUpdateAsync` — không load từng row, 1 câu SQL)

2. Tạo `PointsResetBackgroundService` (IHostedService) — chạy mỗi đêm 00:00 UTC, kiểm tra:

| Điều kiện | Hành động |
|-----------|-----------|
| Hôm nay là thứ Hai | `ResetWeekPointsAsync()` |
| Hôm nay là ngày 1 | `ResetMonthPointsAsync()` |
| Hôm nay là 1/1 | `ResetYearPointsAsync()` |

3. Đăng ký trong `Program.cs`:
```csharp
builder.Services.AddHostedService<PointsResetBackgroundService>();
```

---

## Fix 10 — `min_weight_grams` định nghĩa trong PointCategory nhưng không bao giờ được kiểm tra

**File:** `GarbageCollection.Business/Services/CollectorReportService.cs`

**Lỗi cụ thể:**

`PointMechanic` lưu `MinWeightGrams` cho từng loại rác, nhưng `CalculatePoints()` chỉ đọc `Points`, bỏ qua `MinWeightGrams` hoàn toàn:
```csharp
// Trước — bất kể thu 10g hay 1000g, điểm vẫn như nhau
total += type switch {
    WasteType.Organic => mechanic.Organic.Points,
    ...
};
```
Ví dụ: `organic.min_weight_grams = 500`, nhưng collector thu 50g vẫn nhận đủ 10 điểm.

**Fix:**

Cập nhật `CalculatePoints()` nhận thêm `decimal? actualCapacityKg`:
```csharp
private static int CalculatePoints(
    List<WasteType> types, PointMechanic mechanic, decimal? actualCapacityKg = null)
{
    var actualGrams = actualCapacityKg.HasValue ? actualCapacityKg.Value * 1000 : (decimal?)null;
    foreach (var type in types)
    {
        var criteria = type switch { ... };
        // Neu co trong luong thuc te, check nguong toi thieu
        if (actualGrams.HasValue && criteria.MinWeightGrams > 0 && actualGrams.Value < criteria.MinWeightGrams)
            continue; // Chua du kg -> khong duoc diem cho loai nay
        total += criteria.Points;
    }
}
```

Truyền `request.ActualCapacityKg` khi gọi từ `UpdateReportAsync()`.

> **Note:** Endpoint legacy (`/collect`) không có `actual_capacity_kg` → truyền `null` → bỏ qua check, giữ tương thích ngược.

---

## Kết quả cuối cùng

```
dotnet build → Build succeeded. 0 Error(s)
```

Tổng cộng: 5 bug fix + 1 feature lớn (WorkArea restructure) + 2 fix bổ sung (nullable warnings, cancel endpoint) + **3 fix points system (Fix 8–10)**, không có breaking change với API khác.

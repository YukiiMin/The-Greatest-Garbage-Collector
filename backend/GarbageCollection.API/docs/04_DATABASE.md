# 04 — Database

## Overview

- Database: **PostgreSQL**
- ORM: **Entity Framework Core 8** + Npgsql
- Naming convention: `UseSnakeCaseNamingConvention()` — tự động snake_case
- Context file: `GarbageCollection.DataAccess/Data/AppDbContext.cs`

---

## Lưu ý quan trọng — Column Name Đặc biệt

> Bảng `citizen_reports` và `complaints` có cột Primary Key là **`"Id"`** (PascalCase, quoted) vì migration khởi tạo (`20260423105231_InitialCreate.cs`) chạy **trước** khi `UseSnakeCaseNamingConvention()` được áp dụng cho cột PK.

Tất cả cột khác trong 2 bảng này đều snake_case bình thường (`citizen_id`, `report_id`, v.v.).

**Ảnh hưởng khi dùng Raw SQL:**
```sql
-- SAI (column không tìm thấy — PostgreSQL case-sensitive với quoted identifiers)
UPDATE complaints SET ... WHERE id = @id

-- ĐÚNG
UPDATE complaints SET ... WHERE "Id" = @id
UPDATE citizen_reports SET ... WHERE "Id" = @id
```

---

## Tables

### 1. `users`

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| `id` | uuid | NOT NULL PK | Generated |
| `email` | varchar | NOT NULL | UNIQUE index |
| `email_verified` | boolean | NOT NULL | Must be true để login |
| `google_id` | varchar | NULL | Index; null nếu local-only account |
| `provider` | varchar | NOT NULL | `"local"` hoặc `"google"` |
| `full_name` | varchar | NOT NULL | |
| `avatar_url` | varchar | NULL | Cloudinary URL |
| `password_hash` | varchar | NULL | BCrypt hash; null nếu Google-only |
| `is_banned` | boolean | NOT NULL | |
| `is_login` | boolean | NOT NULL | |
| `login_term` | int | NOT NULL | Version counter; tăng khi reuse detected |
| `role` | varchar | NOT NULL | Enum: `Citizen`, `Collector`, `Enterprise`, `Admin` |
| `address` | varchar(512) | NULL | |
| `work_area_id` | uuid | NULL FK → work_areas.id | Phường/quận đang sinh sống |
| `created_at` | timestamptz | NOT NULL | |
| `updated_at` | timestamptz | NOT NULL | |

---

### 2. `refresh_tokens`

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| `id` | uuid | NOT NULL PK | |
| `user_id` | uuid | NOT NULL FK → users.id | CASCADE delete |
| `token_hash` | varchar | NOT NULL | UNIQUE; SHA-256 hex lowercase của raw token |
| `email` | varchar | NOT NULL | Denormalized (dùng cho lookup) |
| `expires_at` | timestamptz | NOT NULL | TTL 7 ngày |
| `is_revoked` | boolean | NOT NULL | true sau khi dùng hoặc bị revoke |
| `created_at` | timestamptz | NOT NULL | |

---

### 3. `email_otps`

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| `id` | uuid | NOT NULL PK | |
| `email` | varchar | NOT NULL | Index |
| `otp_code` | varchar | NOT NULL | BCrypt hash của 6-digit OTP |
| `expires_at` | timestamptz | NOT NULL | TTL 5 phút |
| `is_used` | boolean | NOT NULL | |
| `count` | int | NOT NULL | Số lần gửi OTP |
| `created_at` | timestamptz | NOT NULL | |
| `updated_at` | timestamptz | NULL | |

---

### 4. `password_otp`

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| `email` | varchar | NOT NULL **PK** | Email là khóa chính (1 OTP per email) |
| `otp_code` | varchar | NOT NULL | BCrypt hash của OTP |
| `expires_at` | timestamptz | NOT NULL | TTL 10 phút |
| `is_used` | boolean | NOT NULL | |
| `created_at` | timestamptz | NOT NULL | |
| `updated_at` | timestamptz | NULL | |

---

### 5. `citizen_reports`

> **PK column là `"Id"` (PascalCase quoted)**

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| `"Id"` | uuid | NOT NULL PK | Quoted PascalCase! |
| `user_id` | uuid | NOT NULL FK → users.id | |
| `team_id` | uuid | NULL FK → teams.id | Set khi assigned |
| `point_category_id` | uuid | NULL FK → point_categories.id | |
| `assign_by` | uuid | NULL | Enterprise ID who assigned |
| `status` | varchar | NOT NULL | Enum string: `Pending`, `Queue`, `Assigned`, `Processing`, `Collected`, `Completed`, `Rejected`, `Failed`, `Cancel`, `OnTheWay` |
| `citizen_image_urls` | JSONB | NOT NULL | `["url1", "url2", ...]` — ảnh từ citizen |
| `collector_image_urls` | JSONB | NOT NULL | `["url1", ...]` — ảnh từ collector |
| `types` | JSONB | NOT NULL | `[1, 2, 3]` — WasteType enum values (max 4) |
| `capacity` | decimal | NULL | kg, ước tính từ citizen (0.01–10000) |
| `actual_capacity_kg` | decimal | NULL | kg, thực tế sau thu gom |
| `description` | varchar(500) | NULL | Ghi chú từ citizen |
| `report_note` | varchar | NULL | Lý do reject hoặc fail |
| `point` | int | NULL | Điểm thưởng |
| `assign_at` | timestamptz | NULL | Khi enterprise assign |
| `deadline` | timestamptz | NULL | Deadline thu gom |
| `start_collecting_at` | timestamptz | NULL | Khi collector bắt đầu |
| `collected_at` | timestamptz | NULL | Khi thu gom xong |
| `complete_at` | timestamptz | NULL | Khi enterprise confirm |
| `report_at` | timestamptz | NOT NULL | Khi citizen tạo báo cáo |
| `created_at` | timestamptz | NOT NULL | |
| `updated_at` | timestamptz | NULL | Set sau khi citizen update (1 lần duy nhất) |

---

### 6. `complaints`

> **PK column là `"Id"` (PascalCase quoted)**

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| `"Id"` | uuid | NOT NULL PK | Quoted PascalCase! |
| `citizen_id` | uuid | NOT NULL FK → users.id | Restrict delete |
| `report_id` | uuid | NOT NULL FK → citizen_reports."Id" | Cascade delete |
| `reason` | varchar(1000) | NOT NULL | Lý do khiếu nại |
| `admin_response` | varchar | NULL | Phản hồi từ admin khi resolve |
| `status` | varchar | NOT NULL | Enum: `Pending`, `Approved`, `Rejected` |
| `image_urls` | text | NOT NULL | JSON string `["url1", ...]` |
| `messages` | jsonb | NOT NULL | `[{sender, email, message, time}, ...]` |
| `request_at` | timestamptz | NOT NULL | Khi tạo khiếu nại |
| `response_at` | timestamptz | NULL | Khi admin resolve |

**ComplaintMessage structure (trong JSONB `messages`):**
```json
{
  "sender": "citizen",      // hoặc "admin"
  "email": "user@example.com",
  "message": "Nội dung tin nhắn",
  "time": "2026-04-26T10:00:00Z"
}
```

---

### 7. `enterprise_hub`

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| `id` | uuid | NOT NULL PK | |
| `name` | varchar(256) | NOT NULL | |
| `phone_number` | varchar | NOT NULL | 10–11 digits |
| `email` | varchar | NOT NULL | UNIQUE — dùng để identify enterprise user |
| `address` | varchar(512) | NOT NULL | |
| `latitude` | decimal | NULL | |
| `longitude` | decimal | NULL | |
| `work_area_id` | uuid | NULL FK → work_areas.id | District phụ trách |
| `created_at` | timestamptz | NOT NULL | |
| `updated_at` | timestamptz | NOT NULL | |

**Auth Note:** Enterprise user được identify qua `Enterprise.Email == User.Email`. Không có FK giữa `users` và `enterprise_hub`.

---

### 8. `collector_hub`

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| `id` | uuid | NOT NULL PK | |
| `enterprise_id` | uuid | NOT NULL FK → enterprise_hub.id | |
| `name` | varchar(256) | NOT NULL | |
| `phone_number` | varchar | NOT NULL | |
| `email` | varchar | NOT NULL | UNIQUE |
| `address` | varchar(512) | NOT NULL | |
| `latitude` | decimal | NULL | -90 đến 90 |
| `longitude` | decimal | NULL | -180 đến 180 |
| `work_area_id` | uuid | NULL FK → work_areas.id | Ward phu trach |
| `assigned_capacity` | int | NULL | Must be > 0 nếu có giá trị |
| `created_at` | timestamptz | NOT NULL | |
| `updated_at` | timestamptz | NOT NULL | |

---

### 9. `teams`

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| `id` | uuid | NOT NULL PK | |
| `collector_id` | uuid | NOT NULL FK → collector_hub.id | |
| `name` | varchar(256) | NOT NULL | |
| `total_capacity` | decimal | NOT NULL | Must be > 0 |
| `is_active` | boolean | NOT NULL | |
| `in_work` | boolean | NOT NULL | true khi đang trong ca |
| `dispatch_time` | varchar(50) | NULL | Format `"HH:MM"`, e.g. `"07:30"` |
| `route_optimized` | boolean | NOT NULL | |
| `work_area_id` | uuid | NULL FK → work_areas.id | Ward team hoat dong |
| `start_working_time` | timestamptz | NULL | |
| `last_finish_time` | timestamptz | NULL | |
| `created_at` | timestamptz | NOT NULL | |
| `updated_at` | timestamptz | NOT NULL | |

---

### 10. `staffs`

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| `user_id` | uuid | NOT NULL | Composite PK (user_id + enterprise_id) |
| `enterprise_id` | uuid | NOT NULL FK → enterprise_hub.id | Composite PK |
| `collector_id` | uuid | **NULL** FK → collector_hub.id | Set khi enterprise assign |
| `team_id` | uuid | **NULL** FK → teams.id | Restrict delete |
| `join_team_at` | timestamptz | NULL | Set khi assign vào team |

**Lifecycle:**
1. Admin tạo: `{user_id, enterprise_id, collector_id=null, team_id=null}`
2. Enterprise assign: `{..., collector_id=X, team_id=Y, join_team_at=now}`
3. Enterprise remove: `{..., collector_id=null, team_id=null, join_team_at=null}` (không xóa record)

---

### 11. `point_categories`

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| `id` | uuid | NOT NULL PK | |
| `enterprise_id` | uuid | NOT NULL FK → enterprise_hub.id | |
| `name` | varchar(256) | NOT NULL | |
| `is_active` | boolean | NOT NULL | |
| `mechanic` | jsonb | NOT NULL | PointMechanic object |
| `created_at` | timestamptz | NOT NULL | |
| `updated_at` | timestamptz | NOT NULL | |

**PointMechanic JSONB structure:**
```json
{
  "organic":        { "points": 10, "min_weight_grams": 500 },
  "recyclable":     { "points": 20, "min_weight_grams": 300 },
  "non_recyclable": { "points": 5,  "min_weight_grams": 1000 }
}
```

---

### 12. `team_sessions`

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| `id` | uuid | NOT NULL PK | |
| `team_id` | uuid | NOT NULL FK → teams.id | |
| `date` | date | NOT NULL | Ngày của ca (DateOnly) |
| `start_at` | timestamptz | NOT NULL | Khi start-shift |
| `end_at` | timestamptz | NULL | Khi end-shift (null = đang trong ca) |
| `total_reports` | int | NOT NULL | Tổng báo cáo trong ca |
| `total_capacity` | decimal | NOT NULL | Tổng kg đã thu |
| `created_at` | timestamptz | NOT NULL | |

---

### 13. `user_points`

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| `user_id` | uuid | NOT NULL **PK** FK → users.id | 1 row per user |
| `week_points` | decimal | NOT NULL | Điểm tuần hiện tại |
| `month_points` | decimal | NOT NULL | Điểm tháng hiện tại |
| `year_points` | decimal | NOT NULL | Điểm năm hiện tại |
| `total_points` | decimal | NOT NULL | Tổng điểm tích lũy |

Upsert pattern: `INSERT ... ON CONFLICT (user_id) DO UPDATE SET ...`

---

### 14. `work_areas`

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| `id` | uuid | NOT NULL PK | |
| `name` | varchar(256) | NOT NULL | e.g. "Quan 1", "Phuong Ben Nghe" |
| `type` | varchar(50) | NOT NULL | `"District"` hoac `"Ward"` |
| `parent_id` | uuid | NULL FK → work_areas.id | null neu la District; tro den District neu la Ward |
| `created_at` | timestamptz | NOT NULL | |

**Hierarchy:** District → Ward (2 cap). Ward co `parent_id` tro den District cha.

---

## Relationship Diagram

```
users ─────────────────< refresh_tokens
users ─────────────────< email_otps (by email, not FK)
users ─────────────────< staffs (user_id)
users ─────────────────< citizen_reports (user_id)
users ─────────────────< complaints (citizen_id)
users ─────────────────1 user_points (user_id PK)
users ──────────────────o work_areas (work_area_id, nullable)

citizen_reports ────────< complaints (report_id, CASCADE)

enterprise_hub ─────────< collector_hub (enterprise_id)
enterprise_hub ─────────< staffs (enterprise_id)
enterprise_hub ─────────< point_categories (enterprise_id)
enterprise_hub ──────────o work_areas (work_area_id, nullable)

collector_hub ──────────< teams (collector_id)
collector_hub ──────────< staffs (collector_id, nullable)
collector_hub ───────────o work_areas (work_area_id, nullable)

teams ──────────────────< staffs (team_id, nullable, RESTRICT delete)
teams ──────────────────< team_sessions (team_id)
teams ──────────────────< citizen_reports (team_id, nullable)
teams ───────────────────o work_areas (work_area_id, nullable)

work_areas ─────────────< work_areas (parent_id, self-ref: District → Ward)
```

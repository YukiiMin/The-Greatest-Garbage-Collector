# 07 — API Reference

**Base URL:** `https://collect-garbage-production.up.railway.app/api/v1`

**Auth:** HttpOnly cookie `accessToken` (set tự động khi login). Các endpoint có Auth=Yes yêu cầu cookie này.

**Response format:** Luôn là `ApiResponse<T>` — xem [06_BUSINESS_RULES.md](06_BUSINESS_RULES.md#5-error-response-format).

---

## Auth (`/api/v1/auth`)

---

### POST `/auth/google-auth/account`
**Auth:** No

**Request:**
```json
{ "data": { "google_token": "<Google ID token từ client SDK>" } }
```

**Response 200:**
```json
{
  "status": "success", "message": "login successful",
  "data": {
    "email": "user@gmail.com", "full_name": "Nguyen Van A",
    "has_password": false, "avatar_url": "https://...", "address": null, "role": "Citizen"
  }
}
```
Cookies set: `accessToken`, `refreshToken`

**Errors:** `404 GOOGLE_INVALID`, `403 USER_BANNED`

---

### POST `/auth/local-auth/account-registration`
**Auth:** No

**Request:**
```json
{
  "data": {
    "email": "user@example.com",
    "password": "Test@1234",
    "full_name": "Nguyen Van A"
  }
}
```

**Response 200:**
```json
{
  "status": "success", "message": "registration successful",
  "data": { "email": "user@example.com", "full_name": "Nguyen Van A", "has_password": true }
}
```
Cookies set: `accessToken`, `refreshToken`

**Errors:** `409 ACCOUNT_EXISTS`, `400` (validation: email/password format)

---

### POST `/auth/local-auth/login`
**Auth:** No

**Request:**
```json
{ "data": { "email": "user@example.com", "password": "Test@1234" } }
```

**Response 200:**
```json
{
  "status": "success", "message": "login successful",
  "data": {
    "email": "user@example.com", "full_name": "Nguyen Van A",
    "has_password": true, "avatar_url": null, "address": null, "role": "Citizen"
  }
}
```
Cookies set: `accessToken`, `refreshToken`

**Errors:** `409 INVALID_CREDENTIALS`, `409 EMAIL_NOT_VERIFIED`, `403 USER_BANNED`

---

### POST `/auth/local-auth/account-verification`
**Auth:** No

**Request:**
```json
{ "data": { "email": "user@example.com", "otp": "123456" } }
```

**Response 200:**
```json
{ "status": "success", "message": "email verified" }
```

**Errors:** `422 OTP_INVALID` (sai / hết hạn / đã dùng), `404 OTP_NOT_FOUND`

---

### POST `/auth/local-auth/email-otp`
**Auth:** No — Gửi lại OTP xác minh email

**Request:**
```json
{ "data": { "email": "user@example.com" } }
```

**Response 200:**
```json
{ "status": "success", "message": "OTP sent" }
```

---

### POST `/auth/local-auth/password-otp`
**Auth:** No — Tạo OTP để reset password

**Request:**
```json
{ "data": { "email": "user@example.com" } }
```

**Response 200:**
```json
{ "status": "success", "message": "OTP sent to email" }
```

---

### POST `/auth/reset-password`
**Auth:** No

**Request:**
```json
{
  "data": {
    "email": "user@example.com",
    "otp": "123456",
    "password": "NewPass@1234"
  }
}
```

**Response 200:**
```json
{ "status": "success", "message": "password has been reset", "data": { "email": "user@example.com" } }
```

**Errors:** `422 OTP_INVALID`, `404 ACCOUNT_NOT_FOUND`, `404 OTP_NOT_FOUND`

---

### GET `/auth/account-auth/license`
**Auth:** Cookie `refreshToken` (HttpOnly)

Không cần request body. Đọc `refreshToken` cookie → rotate → set cookies mới.

**Response 200:**
```json
{
  "status": "success", "message": "license issued",
  "data": { "email": "user@example.com", "full_name": "Nguyen Van A", "avatar_url": null, "address": null }
}
```
Cookies set: new `accessToken`, new `refreshToken`

**Errors:** `401 NO_REFRESH_TOKEN`, `401 INVALID_REFRESH_TOKEN`, `422 TOKEN_EXPIRED`, `409 TOKEN_REUSE`

---

### GET `/auth/account-auth/verification`
**Auth:** Yes

Verify access token + trả về thông tin account.

**Response 200:**
```json
{
  "status": "success", "message": "verified",
  "data": {
    "email": "user@example.com", "full_name": "Nguyen Van A",
    "avatar_url": null, "address": null, "role": "Citizen",
    "has_password": true, "email_verified": true
  }
}
```

---

## User & Profile (`/api/v1/users`)

---

### GET `/users/profile`
**Auth:** Yes

**Response 200:**
```json
{
  "status": "success",
  "data": {
    "email": "user@example.com",
    "full_name": "Nguyen Van A",
    "role": "Citizen",
    "address": null,
    "avatar_url": null,
    "work_area_id": null,
    "created_at": "2026-04-26T00:00:00Z",
    "updated_at": null
  }
}
```

---

### PUT `/users/profile`
**Auth:** Yes | Content-Type: `multipart/form-data`

**Form fields:**
- `fullname` (string, required) — ten moi
- `address` (string, optional)
- `work_area_id` (uuid, optional) — ID cua Ward dang sinh song
- `avatar` (file, optional) — anh dai dien, jpg/jpeg/png, toi da 5MB

**Response 200:**
```json
{ "status": "success", "message": "update user profile successfully", "data": { /* UserProfileDto */ } }
```

**Errors:** `422 INVALID_INPUT`, `422 INVALID_FILE_FORMAT`, `413 FILE_TOO_LARGE`, `404 NOT_FOUND`

---

### PUT `/users/profile/change-password`
**Auth:** Yes

**Request:**
```json
{
  "data": {
    "old_password": "OldPass@1234",
    "new_password": "NewPass@5678",
    "logout_all_devices": false
  }
}
```

**Response 200:**
```json
{ "status": "success", "message": "password updated successfully" }
```
Cookie `accessToken` moi duoc set sau khi doi mat khau thanh cong.

**Errors:** `400 INVALID_OLD_PASSWORD`, `400 SAME_PASSWORD`, `422 INVALID_PASSWORD_FORMAT`

---

### GET `/users/leaderboard`
**Auth:** Yes

**Query params:**
- `period`: `Week` | `Month` | `Year` (default: `Week`)
- `scope`: `Ward` | `Area` (default: `Ward`)
- `page`: int (default: 1)
- `limit`: int 1–50 (default: 10)

> `scope=Ward` — xếp hạng toàn hệ thống.  
> `scope=Area` — chỉ xếp hạng trong phường của user. Yêu cầu user đã set `work_area_id` trong profile.

**Response 200:**
```json
{
  "status": "success",
  "data": {
    "my_rank": { "rank": 5, "total_points": 80 },
    "leaderboard": [
      { "rank": 1, "full_name": "Nguyen Van A", "avatar_url": "...", "total_points": 150, "work_area_name": "Phuong Ben Nghe" },
      { "rank": 2, "full_name": "Tran Thi B",   "avatar_url": null,  "total_points": 120, "work_area_name": null }
    ],
    "pagination": { "page": 1, "limit": 10, "total": 50 }
  }
}
```

> `total_points` trong `leaderboard[]` phu thuoc vao `period`: Week → `week_points`, Month → `month_points`, Year → `year_points`.  
> `TotalPoints` trong `my_rank` cung map theo `period` tuong tu.

**Errors:**
- `422 INVALID_QUERY_PARAMS` — page < 1 hoac limit > 50
- `422 WORK_AREA_NOT_SET` — scope=Area nhung user chua chon phuong cu tru (chua set `work_area_id`)

---

## Citizen Reports (`/api/v1/users`)

---

### POST `/users/citizen-reports`
**Auth:** Yes | Content-Type: `multipart/form-data`

**Form fields:**
- `images` (files, 1–3 ảnh) — ảnh rác
- `data` (JSON string):
```json
{
  "types": [1, 2],
  "capacity": 5.5,
  "description": "Rác thải nhựa và rác hữu cơ"
}
```
WasteType: `1=Organic`, `2=Recyclable`, `3=NonRecyclable`

**Response 200:**
```json
{
  "status": "success", "message": "report created",
  "data": { "id": "uuid", "status": "Pending", "report_at": "2026-04-26T...", ... }
}
```

---

### GET `/users/citizen-reports`
**Auth:** Yes

**Query params:** `page=1`, `limit=10`

**Response 200:**
```json
{
  "status": "success",
  "data": {
    "items": [ { "id": "...", "status": "Pending", "types": [1], "report_at": "..." } ],
    "meta": { "page": 1, "limit": 10, "total": 5, "total_pages": 1 }
  }
}
```

---

### GET `/users/citizen-reports/{id}`
**Auth:** Yes

**Response 200:** Full report object với tất cả fields.

**Errors:** `404 NOT_FOUND`

---

### PATCH `/users/citizen-reports/{id}`
**Auth:** Yes

Chỉ update khi: Status == Pending VÀ chưa update lần nào (`updated_at == null`).

**Request:** `multipart/form-data` (tương tự Create — có thể gửi ảnh mới + data JSON)

**Response 200:**
```json
{ "status": "success", "message": "report updated", "data": { /* updated report */ } }
```

**Errors:** `429 TOO_MANY_REQUESTS` (update lần 2), `409 CONFLICT` (status != Pending)

---

### DELETE `/users/citizen-reports/{id}`
**Auth:** Yes

Chỉ cancel khi: Status == Pending VÀ trong 10 phút đầu.

**Response 200:**
```json
{ "status": "success", "message": "report cancelled" }
```

**Errors:** `409 CONFLICT` (quá 10 phút hoặc status != Pending)

---

## Complaints (`/api/v1/users`)

---

### POST `/users/citizen-reports/{reportId}/complaints`
**Auth:** Yes

**Request:**
```json
{
  "data": {
    "reason": "Báo cáo không được xử lý đúng hạn",
    "image_urls": ["https://cloudinary.com/..."]
  }
}
```

**Response 200:**
```json
{ "status": "success", "data": { "id": "uuid", "status": "Pending", "reason": "...", ... } }
```

---

### GET `/users/citizen-reports/{reportId}/complaints`
**Auth:** Yes

**Query:** `page=1`, `limit=10`

**Response 200:**
```json
{
  "data": {
    "items": [ { "id": "uuid", "status": "Pending", "reason": "...", "request_at": "..." } ],
    "meta": { "page": 1, "limit": 10, "total": 1 }
  }
}
```

---

### GET `/users/citizen-reports/{reportId}/complaints/{id}`
**Auth:** Yes

**Response 200:** Full complaint với messages thread, report details, citizen info.

---

### POST `/users/citizen-reports/{reportId}/complaints/{id}/messages`
**Auth:** Yes

**Request:**
```json
{ "data": { "message": "Tôi cần giải thích thêm về lý do từ chối" } }
```

**Response 200:**
```json
{ "status": "success", "message": "message sent" }
```

---

## Enterprise (`/api/v1/enterprise`) — Auth: Enterprise role

---

### GET `/enterprise/reports`
**Query:** `status=Pending`, `page=1`, `limit=10`

**Response 200:** Danh sách báo cáo với pagination meta.

### GET `/enterprise/reports/{id}`
Full report detail.

### PATCH `/enterprise/reports/{id}/queue`
Pending → Queue. Không cần body.

**Response 200:** Updated report.

### PATCH `/enterprise/reports/{id}/assign`
Queue → Assigned.

**Request:**
```json
{ "data": { "team_id": "uuid", "deadline": "2026-04-27T18:00:00Z" } }
```

### PATCH `/enterprise/reports/{id}/reject`
Pending/Queue → Rejected.

**Request:**
```json
{ "data": { "reason": "Địa chỉ nằm ngoài khu vực phục vụ" } }
```

### PATCH `/enterprise/reports/{id}/complete`
Collected → Completed. Không cần body.

---

### GET `/enterprise/collectors`
Danh sách collectors của enterprise.

### GET `/enterprise/collectors/{id}`
Chi tiết collector.

### POST `/enterprise/collectors`
**Request:**
```json
{
  "data": {
    "name": "Đội thu gom Quận 1",
    "phone_number": "0901234567",
    "email": "collector@example.com",
    "address": "123 Đường ABC, Q1",
    "work_area": "Quận 1, TP.HCM",
    "assigned_capacity": 500,
    "latitude": 10.7769,
    "longitude": 106.7009
  }
}
```

### PATCH `/enterprise/collectors/{id}`
Tương tự Create — body chứa fields cần update.

### DELETE `/enterprise/collectors/{id}`
Chỉ xóa được nếu không còn team nào.

---

### GET `/enterprise/teams`
Danh sách teams với member count.

### GET `/enterprise/teams/{id}`
Chi tiết team với danh sách staff.

### POST `/enterprise/teams`
**Request:**
```json
{
  "data": {
    "name": "Team A",
    "total_capacity": 200.0,
    "collector_id": "uuid",
    "dispatch_time": "07:00"
  }
}
```

### PATCH `/enterprise/teams/{id}`
### DELETE `/enterprise/teams/{id}`
Chỉ xóa được nếu không còn staff.

---

### GET `/enterprise/teams/{teamId}/staff`
Danh sách staff trong team.

### POST `/enterprise/teams/{teamId}/staff`
**Request:**
```json
{ "data": { "user_id": "uuid" } }
```
Staff phải thuộc enterprise và chưa được assign team khác.

### DELETE `/enterprise/teams/{teamId}/staff/{userId}`
Remove staff khỏi team (soft — chỉ clear assignment).

---

### GET `/enterprise/point-categories`
### POST `/enterprise/point-categories`
**Request:**
```json
{
  "data": {
    "name": "Chương trình tháng 4",
    "mechanic": {
      "organic": { "points": 10, "min_weight_grams": 500 },
      "recyclable": { "points": 20, "min_weight_grams": 300 },
      "non_recyclable": { "points": 5, "min_weight_grams": 1000 }
    }
  }
}
```

### PATCH `/enterprise/point-categories/{id}`
### DELETE `/enterprise/point-categories/{id}`

---

## Collector (`/api/v1/collector`) — Auth: Collector role

---

### GET `/collector/reports`
**Query:** `date=2026-04-26` (optional, default today)

Trả về báo cáo Assigned + Processing của team collector hôm nay.

### PATCH `/collector/reports/start-shift`
**Request:**
```json
{ "data": { "team_id": "uuid", "date": "2026-04-26" } }
```

Start shift: chuyển tất cả báo cáo Assigned → Processing, tạo TeamSession, set `team.in_work = true`.

### PATCH `/collector/reports/end-shift`
**Request:**
```json
{ "data": { "team_id": "uuid", "date": "2026-04-26" } }
```

End shift: tổng kết TeamSession, set `team.in_work = false`.

### PATCH `/collector/reports/{id}`
Submit kết quả báo cáo.

**Request (Collected):**
```json
{
  "status": "Collected",
  "images": ["https://cloudinary.com/collected1.jpg"],
  "weight": 12.5
}
```

**Request (Failed):**
```json
{
  "status": "Failed",
  "reason": "Không có rác tại địa điểm"
}
```

### GET `/collector/dashboard`
Thống kê: tổng báo cáo, tổng cân nặng, báo cáo hôm nay, v.v.

---

## Admin (`/api/v1/admin`) — Auth: Admin role

---

### GET `/admin/users`
**Query:** `page=1`, `limit=10`, `search=` (email/name), `role=` (Citizen/Collector/Enterprise/Admin), `is_banned=` (true/false)

**Response 200:**
```json
{
  "data": {
    "items": [ { "id": "uuid", "email": "...", "full_name": "...", "role": "Citizen", "is_banned": false } ],
    "meta": { "page": 1, "limit": 10, "total": 50 }
  }
}
```

### PATCH `/admin/users/{id}/role`
**Request:**
```json
{ "data": { "role": "Collector" } }
```
Valid roles: `Citizen`, `Collector`, `Enterprise`, `Admin`

### PATCH `/admin/users/{id}/ban`
**Request:**
```json
{ "data": { "is_banned": true } }
```

---

### GET `/admin/enterprises`
Danh sách tất cả enterprises.

### GET `/admin/enterprises/{id}`
Chi tiết enterprise.

### POST `/admin/enterprises`
**Request:**
```json
{
  "data": {
    "name": "Công ty TNHH Thu Gom ABC",
    "phone_number": "0281234567",
    "email": "enterprise@abc.com",
    "address": "456 Đường XYZ, Q3",
    "work_area": "Quận 3, Quận 4, TP.HCM"
  }
}
```

### PATCH `/admin/enterprises/{id}`
Tương tự Create — partial update.

### DELETE `/admin/enterprises/{id}`
Chỉ xóa nếu không còn staff nào.

---

### POST `/admin/setup/enterprise`
Setup một user có sẵn thành Enterprise manager.

Tạo Enterprise entity + Staff record + đổi User.Role = Enterprise.

**Request:**
```json
{
  "data": {
    "user_id": "uuid-của-user",
    "name": "Công ty ABC",
    "phone_number": "0901234567",
    "address": "123 Đường ABC",
    "work_area": "Quận 1"
  }
}
```

**Response 200:**
```json
{
  "data": {
    "user": { "id": "uuid", "email": "...", "role": "Enterprise" },
    "extra_data": { "id": "enterprise-uuid", "name": "Công ty ABC", ... }
  }
}
```

---

### POST `/admin/setup/collector`
Setup một user có sẵn thành Collector.

Tạo Staff record (unassigned) + đổi User.Role = Collector.

**Request:**
```json
{
  "data": {
    "user_id": "uuid-của-user",
    "enterprise_id": "uuid-của-enterprise"
  }
}
```

**Response 200:**
```json
{
  "data": {
    "user": { "id": "uuid", "email": "...", "role": "Collector" },
    "extra_data": { "user_id": "uuid", "enterprise_id": "uuid", "team_id": null }
  }
}
```

---

### GET `/admin/complaints`
**Query:** `status=Pending`, `page=1`, `limit=10`

**Response 200:**
```json
{
  "data": {
    "items": [
      {
        "id": "uuid", "status": "Pending", "reason": "...",
        "request_at": "2026-04-26T...", "citizen": { "full_name": "...", "email": "..." }
      }
    ],
    "meta": { "page": 1, "limit": 10, "total": 3 }
  }
}
```

### GET `/admin/complaints/{id}`
Full complaint detail với messages và report info.

### PATCH `/admin/complaints/{id}`
Resolve hoặc reject khiếu nại.

**Request:**
```json
{
  "data": {
    "status": "Approved",
    "admin_response": "Đã xem xét, khiếu nại hợp lệ. Sẽ xử lý lại báo cáo."
  }
}
```
Valid status: `Approved`, `Rejected`

---

### GET `/admin/work-areas`
**Query:** `type=District|Ward` (optional — filter theo cap)

**Response 200:**
```json
{
  "status": "success",
  "data": [
    {
      "id": "uuid", "name": "Quan 1", "type": "District", "parent_id": null, "parent_name": null,
      "children": [
        { "id": "uuid", "name": "Phuong Ben Nghe", "type": "Ward", "parent_id": "uuid-quan1", "children": [] }
      ],
      "created_at": "2026-04-26T..."
    }
  ]
}
```

### GET `/admin/work-areas/{id}`
Chi tiet mot work area kem children.

**Errors:** `404 NOT_FOUND`

### POST `/admin/work-areas`
Tao District hoac Ward.

**Request:**
```json
{
  "data": {
    "name": "Phuong Ben Nghe",
    "type": "Ward",
    "parent_id": "uuid-cua-district"
  }
}
```
- `type`: `"District"` hoac `"Ward"`
- `parent_id`: bat buoc neu `type = "Ward"`

**Response 201:** WorkAreaDto

**Errors:** `400 BAD_REQUEST` (Ward thieu parent_id), `404 NOT_FOUND` (parent khong ton tai)

### PATCH `/admin/work-areas/{id}`
Cap nhat ten/parent cua work area.

**Request:** Giong POST.

**Response 200:** WorkAreaDto updated.

### DELETE `/admin/work-areas/{id}`
Xoa work area. Chi xoa duoc neu khong co entity nao FK vao (users, enterprise, collector, team).

**Errors:** `404 NOT_FOUND`, `409 CONFLICT` (con entity FK vao)

---

## Image (`/api/v1/image`) — Auth: Yes

---

### POST `/image/upload`
**Auth:** Yes | Content-Type: `multipart/form-data`

**Form field:** `images` (1 hoặc nhiều files)

**Response 200:**
```json
{
  "status": "success",
  "data": [
    "https://res.cloudinary.com/demo/image/upload/waste-reports/abc123.jpg",
    "https://res.cloudinary.com/demo/image/upload/waste-reports/def456.jpg"
  ]
}
```

**Errors:** `400 VALIDATION_ERROR` (không có file)

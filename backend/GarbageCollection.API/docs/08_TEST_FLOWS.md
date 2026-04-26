# 08 — Test Flows

Các luồng test end-to-end cho từng actor. Mỗi luồng độc lập và chứa đủ request body để chạy không cần tham chiếu tài liệu khác.

**Base URL:** `https://collect-garbage-production.up.railway.app/api/v1`

---

## Flow 1 — Citizen: Đăng ký → Báo cáo → Khiếu nại

**Prerequisite:** Không cần account có sẵn.

---

### Bước 1 — Đăng ký tài khoản

```http
POST /auth/local-auth/account-registration
Content-Type: application/json

{
  "data": {
    "email": "citizen_test@example.com",
    "password": "Test@1234",
    "full_name": "Nguyen Van Test"
  }
}
```

**Expected:** 200, nhận `accessToken` + `refreshToken` cookie, `email_verified = false`

---

### Bước 2 — Xác minh email (OTP)

Kiểm tra email để lấy OTP 6 số.

```http
POST /auth/local-auth/account-verification
Content-Type: application/json

{
  "data": {
    "email": "citizen_test@example.com",
    "otp": "123456"
  }
}
```

**Expected:** 200, `"email verified"`

**Edge case:** OTP sai → `422 OTP_INVALID`. Gửi lại OTP:
```http
POST /auth/local-auth/email-otp
{ "data": { "email": "citizen_test@example.com" } }
```

---

### Bước 3 — Đăng nhập

```http
POST /auth/local-auth/login
Content-Type: application/json

{
  "data": {
    "email": "citizen_test@example.com",
    "password": "Test@1234"
  }
}
```

**Expected:** 200, role = `"Citizen"`, cookies set

---

### Bước 4 — Upload ảnh

```http
POST /image/upload
Content-Type: multipart/form-data
(Cần cookie accessToken)

images: [file1.jpg, file2.jpg]
```

**Expected:** 200, trả về array URLs:
```json
{ "data": ["https://res.cloudinary.com/...jpg", "https://res.cloudinary.com/...jpg"] }
```

---

### Bước 5 — Tạo báo cáo rác

```http
POST /users/citizen-reports
Content-Type: multipart/form-data

images: [file1.jpg]
data: {
  "types": [1, 2],
  "capacity": 5.5,
  "description": "Rác thải nhựa và rác hữu cơ tại góc đường"
}
```

WasteType: `1=Organic`, `2=Recyclable`, `3=NonRecyclable`

**Expected:** 200, `status = "Pending"`, nhận `id` để dùng các bước sau

---

### Bước 6 — Xem danh sách báo cáo

```http
GET /users/citizen-reports?page=1&limit=10
(Cookie accessToken)
```

**Expected:** 200, danh sách có báo cáo vừa tạo

---

### Bước 7 — Update báo cáo (1 lần duy nhất)

```http
PATCH /users/citizen-reports/{id}
Content-Type: multipart/form-data

data: {
  "capacity": 8.0,
  "description": "Cập nhật: rác nhiều hơn ban đầu"
}
```

**Expected:** 200, `updated_at` được set

**Edge case:** Thử update lần 2 → `429 TOO_MANY_REQUESTS`

---

### Bước 8 — Tạo khiếu nại

```http
POST /users/citizen-reports/{id}/complaints
Content-Type: application/json

{
  "data": {
    "reason": "Báo cáo đã quá hạn nhưng chưa được xử lý",
    "image_urls": []
  }
}
```

**Expected:** 200, nhận `complaint_id`

---

### Bước 9 — Gửi tin nhắn trong khiếu nại

```http
POST /users/citizen-reports/{id}/complaints/{complaint_id}/messages
Content-Type: application/json

{
  "data": {
    "message": "Tôi cần được giải thích tại sao báo cáo bị trễ hạn"
  }
}
```

**Expected:** 200, `"message sent"`

---

### Edge Cases

| Tình huống | Expected |
|-----------|---------|
| Login với email chưa verify | `409 EMAIL_NOT_VERIFIED` |
| Login với password sai | `409 INVALID_CREDENTIALS` |
| Đăng ký email đã dùng | `409 ACCOUNT_EXISTS` |
| Cancel báo cáo sau 10 phút | `409 CONFLICT` |
| Cancel báo cáo đang status Queue | `409 CONFLICT` |
| Upload mà không có cookie | `401 UNAUTHORIZED` |

---

---

## Flow 2 — Admin: Setup hệ thống

**Prerequisite:** Có account admin (liên hệ team để lấy credentials).

---

### Bước 1 — Đăng nhập admin

```http
POST /auth/local-auth/login
{
  "data": {
    "email": "admin@ecoconnect.com",
    "password": "Admin@1234"
  }
}
```

**Expected:** 200, `role = "Admin"`

---

### Bước 2 — Tạo Enterprise

```http
POST /admin/enterprises
{
  "data": {
    "name": "Công ty TNHH Thu Gom Xanh",
    "phone_number": "0281234567",
    "email": "xanh@enterprise.com",
    "address": "456 Đường Xanh, Quận 3, TP.HCM",
    "work_area": "Quận 3, Quận 4"
  }
}
```

**Expected:** 200, nhận `enterprise_id`

---

### Bước 3 — Tìm user cần setup

```http
GET /admin/users?search=xanh@enterprise.com
```

**Expected:** 200, nhận `user_id` của user có email `xanh@enterprise.com`

---

### Bước 4 — Setup Enterprise user (Enterprise manager)

User có email `xanh@enterprise.com` sẽ được liên kết với Enterprise vừa tạo.

```http
POST /admin/setup/enterprise
{
  "data": {
    "user_id": "{user_id}",
    "name": "Công ty TNHH Thu Gom Xanh",
    "phone_number": "0281234567",
    "address": "456 Đường Xanh, Quận 3",
    "work_area": "Quận 3, Quận 4"
  }
}
```

**Expected:** 200, `user.role = "Enterprise"`, nhận enterprise details

---

### Bước 5 — Tìm user collector để setup

```http
GET /admin/users?search=collector@example.com
```

---

### Bước 6 — Setup Collector user

```http
POST /admin/setup/collector
{
  "data": {
    "user_id": "{collector_user_id}",
    "enterprise_id": "{enterprise_id}"
  }
}
```

**Expected:** 200, `user.role = "Collector"`, `team_id = null` (chưa được assign team)

---

### Bước 7 — Xem danh sách khiếu nại

```http
GET /admin/complaints?status=Pending&page=1&limit=10
```

---

### Bước 8 — Resolve khiếu nại

```http
PATCH /admin/complaints/{complaint_id}
{
  "data": {
    "status": "Approved",
    "admin_response": "Đã xem xét và chấp nhận khiếu nại. Báo cáo sẽ được ưu tiên xử lý."
  }
}
```

**Expected:** 200, complaint status = `"Approved"`

---

### Edge Cases

| Tình huống | Expected |
|-----------|---------|
| Setup enterprise user không có enterprise email khớp | `404 NOT_FOUND` hoặc lỗi lookup |
| Setup collector với enterprise_id không tồn tại | `404 NOT_FOUND` |
| Đổi role không hợp lệ (vd. "SuperAdmin") | `400` validation |
| Ban account admin khác | 200 (admin có thể ban bất kỳ ai) |

---

---

## Flow 3 — Enterprise: Phân công báo cáo

**Prerequisite:** Đã setup qua Admin (có account role Enterprise và Enterprise entity tồn tại với email khớp).

---

### Bước 1 — Đăng nhập Enterprise user

```http
POST /auth/local-auth/login
{
  "data": {
    "email": "xanh@enterprise.com",
    "password": "Enterprise@1234"
  }
}
```

**Expected:** 200, `role = "Enterprise"`

---

### Bước 2 — Tạo Collector hub

```http
POST /enterprise/collectors
{
  "data": {
    "name": "Đội thu gom Quận 3",
    "phone_number": "0901234567",
    "email": "team_q3@xanh.com",
    "address": "123 Đường Thu Gom, Q3",
    "work_area": "Quận 3",
    "assigned_capacity": 500,
    "latitude": 10.7769,
    "longitude": 106.7009
  }
}
```

**Expected:** 200, nhận `collector_id`

---

### Bước 3 — Tạo Team dưới collector

```http
POST /enterprise/teams
{
  "data": {
    "name": "Team A - Sáng sớm",
    "total_capacity": 150.0,
    "collector_id": "{collector_id}",
    "dispatch_time": "06:00"
  }
}
```

**Expected:** 200, nhận `team_id`

---

### Bước 4 — Thêm staff collector vào team

```http
POST /enterprise/teams/{team_id}/staff
{
  "data": {
    "user_id": "{collector_user_id}"
  }
}
```

Staff phải thuộc enterprise này và `team_id == null`.

**Expected:** 200, staff được assign vào team

---

### Bước 5 — Tạo PointCategory

```http
POST /enterprise/point-categories
{
  "data": {
    "name": "Chương trình tháng 4/2026",
    "mechanic": {
      "organic":        { "points": 10, "min_weight_grams": 500 },
      "recyclable":     { "points": 20, "min_weight_grams": 300 },
      "non_recyclable": { "points": 5,  "min_weight_grams": 1000 }
    }
  }
}
```

---

### Bước 6 — Xem báo cáo Pending

```http
GET /enterprise/reports?status=Pending&page=1&limit=10
```

---

### Bước 7 — Queue báo cáo

```http
PATCH /enterprise/reports/{report_id}/queue
```

Không cần body. Chuyển `Pending → Queue`.

**Expected:** 200, `status = "Queue"`

---

### Bước 8 — Assign báo cáo cho team

```http
PATCH /enterprise/reports/{report_id}/assign
{
  "data": {
    "team_id": "{team_id}",
    "deadline": "2026-04-27T18:00:00Z"
  }
}
```

Deadline phải là tương lai.

**Expected:** 200, `status = "Assigned"`, `assign_at` được set

---

### Bước 9 — Reject báo cáo khác

```http
PATCH /enterprise/reports/{other_report_id}/reject
{
  "data": {
    "reason": "Địa chỉ nằm ngoài khu vực phục vụ của chúng tôi"
  }
}
```

**Expected:** 200, `status = "Rejected"`

---

### Bước 10 — Confirm Complete (sau khi Collector thu xong)

Đợi Collector Flow 4 hoàn thành bước Submit Collected trước.

```http
PATCH /enterprise/reports/{report_id}/complete
```

**Expected:** 200, `status = "Completed"`, `complete_at` được set

---

### Edge Cases

| Tình huống | Expected |
|-----------|---------|
| Assign team không thuộc enterprise | `403` hoặc `404` |
| Assign deadline trong quá khứ | `400` validation |
| Queue báo cáo đang Queue | `409 CONFLICT` |
| Xóa team đang có staff | `409 CONFLICT` |
| Remove staff đang có collector+team | 200, clear assignment |

---

---

## Flow 4 — Collector: Thực hiện ca thu gom

**Prerequisite:** Đã setup qua Admin (có account role Collector, được assign team qua Enterprise flow).

---

### Bước 1 — Đăng nhập Collector

```http
POST /auth/local-auth/login
{
  "data": {
    "email": "collector@example.com",
    "password": "Collector@1234"
  }
}
```

**Expected:** 200, `role = "Collector"`

---

### Bước 2 — Xem báo cáo được giao hôm nay

```http
GET /collector/reports
```

Trả về báo cáo `Assigned` + `Processing` của team hôm nay.

---

### Bước 3 — Bắt đầu ca làm

```http
PATCH /collector/reports/start-shift
{
  "data": {
    "team_id": "{team_id}",
    "date": "2026-04-26"
  }
}
```

Chuyển tất cả báo cáo `Assigned → Processing`. Tạo `TeamSession`.

**Expected:** 200, danh sách báo cáo Processing

---

### Bước 4 — Upload ảnh kết quả thu gom

```http
POST /image/upload
Content-Type: multipart/form-data

images: [after_collection.jpg]
```

**Expected:** 200, nhận URL ảnh

---

### Bước 5 — Submit báo cáo thành công (Collected)

```http
PATCH /collector/reports/{report_id}
Content-Type: application/json

{
  "status": "Collected",
  "images": ["https://res.cloudinary.com/.../after_collection.jpg"],
  "weight": 12.5
}
```

**Expected:** 200, `status = "Collected"`, `collected_at` được set

---

### Bước 6 — Submit báo cáo thất bại (Failed)

```http
PATCH /collector/reports/{other_report_id}
Content-Type: application/json

{
  "status": "Failed",
  "reason": "Không có rác tại địa điểm — có thể đã được dọn trước"
}
```

**Expected:** 200, `status = "Failed"`, `report_note` = reason

---

### Bước 7 — Kết thúc ca

```http
PATCH /collector/reports/end-shift
{
  "data": {
    "team_id": "{team_id}",
    "date": "2026-04-26"
  }
}
```

Cập nhật `TeamSession.EndAt`, tổng kết số báo cáo + cân nặng. Set `team.in_work = false`.

**Expected:** 200, session summary

---

### Bước 8 — Xem dashboard

```http
GET /collector/dashboard
```

**Expected:** 200, thống kê: tổng báo cáo, kg đã thu, báo cáo hôm nay, v.v.

---

### Edge Cases

| Tình huống | Expected |
|-----------|---------|
| Start shift khi team đang `in_work = true` | `409 CONFLICT` |
| Submit báo cáo không thuộc ca đang chạy | `403` hoặc `404` |
| Submit status không hợp lệ | `400` validation |
| End shift khi `in_work = false` | `409 CONFLICT` |

---

---

## Flow 5 — Token Lifecycle: Refresh & Reuse Detection

**Prerequisite:** Đã đăng nhập (có `accessToken` + `refreshToken` cookie).

---

### Bước 1 — Gọi API với access token hợp lệ

```http
GET /users/profile
(Cookie: accessToken=<valid token>)
```

**Expected:** 200, trả về profile

---

### Bước 2 — Access token hết hạn

Sau 15 phút (hoặc xóa cookie `accessToken`):

```http
GET /users/profile
(Không có cookie hoặc token hết hạn)
```

**Expected:** `401 UNAUTHORIZED`
```json
{ "status": "failed", "error": { "code": "UNAUTHORIZED" } }
```

---

### Bước 3 — Refresh token để lấy token mới

```http
GET /auth/account-auth/license
(Cookie: refreshToken=<valid refresh token>)
```

**Expected:** 200
- Cookie `accessToken` được set mới
- Cookie `refreshToken` được set mới (token cũ bị revoke)

```json
{
  "status": "success", "message": "license issued",
  "data": { "email": "...", "full_name": "..." }
}
```

---

### Bước 4 — Thử dùng lại refresh token cũ (đã bị revoke)

Gửi lại request với `refreshToken` cookie cũ (token đã bị revoke ở Bước 3):

```http
GET /auth/account-auth/license
(Cookie: refreshToken=<OLD revoked token>)
```

**Expected:** `409 TOKEN_REUSE`
```json
{
  "status": "failed",
  "message": "abnormal detection",
  "error": {
    "code": "TOKEN_REUSE",
    "description": "Refresh token reuse detected. All sessions have been terminated."
  }
}
```

**Effect:** Toàn bộ refresh tokens của user bị revoke + `login_term` tăng → user phải đăng nhập lại.

---

### Kịch bản: Token hết hạn (7 ngày)

```http
GET /auth/account-auth/license
(refreshToken hết hạn)
```

**Expected:** `422 TOKEN_EXPIRED`
```json
{
  "status": "failed",
  "error": { "code": "TOKEN_EXPIRED", "description": "Refresh token has expired. Please log in again." }
}
```

→ Client phải redirect đến trang login.

# 06 — Business Rules

## 1. Report State Machine

### Trạng thái (ReportStatus Enum)

| Value | Name | Mô tả |
|-------|------|--------|
| 1 | Pending | Citizen vừa tạo, chưa được xét duyệt |
| 2 | Queue | Enterprise đã queue, chưa assign team |
| 3 | Assigned | Đã assign cho team, chờ collector bắt đầu |
| 4 | Processing | Collector đang trong ca (start-shift) |
| 5 | Collected | Collector đã thu gom xong |
| 6 | Completed | Enterprise đã xác nhận hoàn thành |
| 7 | Rejected | Enterprise từ chối |
| 8 | Failed | Collector báo thu thất bại |
| 9 | Cancel | Citizen hủy |
| 10 | OnTheWay | Collector đang trên đường |

### Flow Diagram

```
[Citizen tạo báo cáo]
         │
         ▼
      [Pending] ─────────────────────────────→ [Rejected]
         │                                    (Enterprise)
         │ Enterprise queue
         ▼
       [Queue] ──────────────────────────────→ [Rejected]
         │                                    (Enterprise)
         │ Enterprise assign (TeamId + Deadline)
         ▼
     [Assigned]
         │
         │ Collector start-shift
         ▼
    [Processing]
       /       \
      /         \
Collector     Collector
collected      failed
     │             │
     ▼             ▼
[Collected]     [Failed]
     │
     │ Enterprise complete
     ▼
[Completed]

[Pending] → [Cancel]   (Citizen, trong 10 phút đầu)
```

### Transition Table

| From | To | Actor | Endpoint | Conditions | Side Effects |
|------|----|-------|----------|------------|--------------|
| Pending | Queue | Enterprise | `PATCH /enterprise/reports/{id}/queue` | Status == Pending | — |
| Pending / Queue | Rejected | Enterprise | `PATCH /enterprise/reports/{id}/reject` | Status == Pending hoặc Queue | `ReportNote` = reason |
| Queue | Assigned | Enterprise | `PATCH /enterprise/reports/{id}/assign` | Status == Queue; TeamId thuộc enterprise; Deadline > now | `TeamId`, `AssignAt`, `AssignBy`, `Deadline` |
| Assigned | Processing | Collector | `PATCH /reports/start-shift` | Status == Assigned | `TeamSession.StartAt` |
| Processing | Collected | Collector | `PATCH /collector/reports/{id}` | Status == Processing | `CollectedAt`, `ActualCapacityKg`, `CollectorImageUrls` |
| Processing | Failed | Collector | `PATCH /collector/reports/{id}` | Status == Processing | `ReportNote` = fail reason |
| Collected | Completed | Enterprise | `PATCH /enterprise/reports/{id}/complete` | Status == Collected | `CompleteAt` |
| Pending | Cancel | Citizen | `DELETE /users/citizen-reports/{id}` | Status == Pending; CreatedAt + 10 phút > now | Hard delete record |

---

## 2. Citizen Report Constraints

### Tạo báo cáo
- **Tối đa 4 WasteType** per report (`Types.Count ≤ 4`)
- Hình ảnh upload lên Cloudinary folder `"waste-reports"`
- Status khởi tạo luôn là `Pending`

### Update báo cáo (1 lần duy nhất)
- Chỉ update được khi `Status == Pending`
- Chỉ update được khi `UpdatedAt == null` (chưa update lần nào)
- Cố update lần 2 → `TooManyRequestsException` → HTTP 429

### Cancel báo cáo
- Chỉ cancel được khi `Status == Pending`
- Chỉ cancel được trong **10 phút** đầu từ khi tạo (`ReportAt + 10 min > UtcNow`)
- Cancel là **hard delete** (xóa hoàn toàn record khỏi DB)

---

## 3. Staff Assignment

### Luồng tổng quát

```
1. Admin tạo unassigned Staff:
   POST /admin/setup/collector { user_id, enterprise_id }
   → staffs: { user_id, enterprise_id, collector_id=NULL, team_id=NULL }

2. Enterprise assign staff vào team:
   POST /enterprise/teams/{teamId}/staff { user_id }
   → staffs: { ..., collector_id=X, team_id=teamId, join_team_at=now }

3. Enterprise remove staff khỏi team:
   DELETE /enterprise/teams/{teamId}/staff/{userId}
   → staffs: { ..., collector_id=NULL, team_id=NULL, join_team_at=NULL }
   (Không xóa record — staff vẫn thuộc enterprise)
```

### Constraints
- Staff chỉ được assign nếu `team_id == null` (chưa thuộc team nào)
- Remove staff chỉ clear assignment, không xóa staff record
- Team thuộc enterprise nào thì chỉ enterprise đó được assign staff vào

---

## 4. Validation Rules

### Regex Patterns (ValidatorConstants.cs)

| Pattern | Giá trị |
|---------|---------|
| Email | `^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$` |
| Password | `^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[!@#$%^&*()\-_=+\[\]{}|;':"",./<>?`~\\]).{8,16}$` |
| Phone | `^\d{10,11}$` |
| OTP | `^\d{6}$` |

**Password requirements:** 8–16 ký tự, có ít nhất 1 chữ hoa, 1 chữ thường, 1 chữ số, 1 ký tự đặc biệt từ tập `!@#$%^&*()-_=+[]{}|;':",./<>?~\`

### Field Limits

| Field | Rule | Validator |
|-------|------|-----------|
| Email | Matches EmailRegex | LocalRegisterRequestValidator |
| Password | Matches PasswordRegex (8-16 chars) | LocalRegisterRequestValidator, ResetPasswordRequestValidator |
| FullName | Required, MaxLength(256) | LocalRegisterRequestValidator, UpdateUserProfileRequestValidator |
| Address | MaxLength(512) | UpdateUserProfileRequestValidator |
| PhoneNumber | Matches PhoneRegex (10-11 digits) | SaveCollectorRequestValidator, SaveAdminEnterpriseRequestValidator |
| OTP | Matches OtpRegex (6 digits), Length(6) | VerifyEmailRequestValidator |
| Capacity | InclusiveBetween(0.01, 10000) | CreateCitizenReportValidator, UpdateCitizenReportValidator |
| Description | MaxLength(500) | CreateCitizenReportValidator |
| Reason (Complaint) | NotEmpty, MaxLength(1000) | CreateComplaintValidator |
| AdminResponse | NotEmpty, MaxLength(2000) | ResolveComplaintRequestValidator |
| Message | NotEmpty, MaxLength(1000) | SendComplaintMessageRequestValidator |
| Types | NotNull, Count > 0 | CreateCitizenReportValidator |
| Deadline | Must be > DateTime.UtcNow | AssignReportRequestValidator |
| Date (StartShift) | Must be ≤ DateOnly.FromDateTime(UtcNow) | StartShiftRequestValidator |

---

## 5. Error Response Format

Tất cả response đều dùng `ApiResponse<T>` envelope:

```json
// Success
{
  "status": "success",
  "message": "report queued",
  "data": { /* payload */ },
  "error": null
}

// Failure
{
  "status": "failed",
  "message": "mô tả ngắn cho user",
  "data": null,
  "error": {
    "code": "ERROR_CODE",
    "description": "mô tả kỹ thuật"
  }
}
```

### Common Error Codes

| Code | HTTP | Tình huống |
|------|------|-----------|
| `INVALID_CREDENTIALS` | 409 | Email hoặc password sai (login) |
| `ACCOUNT_EXISTS` | 409 | Email đã đăng ký |
| `EMAIL_NOT_VERIFIED` | 409 | Chưa xác minh email |
| `USER_BANNED` | 403 | Tài khoản bị ban |
| `OTP_INVALID` | 422 | OTP sai hoặc hết hạn |
| `OTP_NOT_FOUND` | 404 | Không có OTP cho email |
| `GOOGLE_INVALID` | 404 | Google token không hợp lệ |
| `NO_REFRESH_TOKEN` | 401 | Cookie refreshToken không có |
| `INVALID_REFRESH_TOKEN` | 401 | JWT refresh token malformed |
| `TOKEN_REUSE` | 409 | Refresh token đã bị revoke |
| `TOKEN_EXPIRED` | 422 | Refresh token hết hạn |
| `UNAUTHORIZED` | 401 | Access token không có/invalid |
| `FORBIDDEN` | 403 | Không đủ quyền |
| `NOT_FOUND` | 404 | Resource không tồn tại |
| `CONFLICT` | 409 | Conflict state (vd. status transition không hợp lệ) |
| `TOO_MANY_REQUESTS` | 429 | Update report lần 2 |
| `BAD_REQUEST` | 400 | Input không hợp lệ |
| `WORK_AREA_NOT_SET` | 422 | scope=Area nhung user chua set work_area_id |
| `INTERNAL_SERVER_ERROR` | 500 | Unexpected error |

---

## 6. Point Mechanic

Enterprise tạo `PointCategory` để định nghĩa điểm thưởng cho Citizen khi báo cáo rác:

```json
POST /enterprise/point-categories
{
  "data": {
    "name": "Chuong trinh thang 4",
    "mechanic": {
      "organic":        { "points": 10, "min_weight_grams": 500 },
      "recyclable":     { "points": 20, "min_weight_grams": 300 },
      "non_recyclable": { "points": 5,  "min_weight_grams": 1000 }
    }
  }
}
```

- `points`: So diem nhan duoc neu dat nguong can nang toi thieu
- `min_weight_grams`: Can nang toi thieu (grams) de duoc diem — **duoc enforce trong code**

### Cach tinh diem (`CalculatePoints`)

Khi collector submit `status=Collected` voi `actual_capacity_kg`:

```
Voi moi waste type trong report.Types:
  actualGrams = actual_capacity_kg * 1000
  if actualGrams < type.MinWeightGrams → loai do khong duoc diem
  else → cong them type.Points vao tong
```

**Vi du:**
- `actual_capacity_kg = 0.3` (300g), mechanic `organic.min_weight_grams = 500`
- 300g < 500g → Organic khong duoc diem
- Recyclable `min_weight_grams = 300` → 300g >= 300g → Recyclable duoc 20 diem

> **Note:** Endpoint legacy (`PATCH /collector/reports/{id}/collect`) khong co `actual_capacity_kg` → bo qua check MinWeightGrams, tinh diem day du.

### Thoi diem award diem

- Diem duoc ghi khi collector chuyen status → `Collected` (qua `CollectWithPointsAsync`)
- Khong award diem khi `Failed`
- Diem ghi vao bang `user_points` cua Citizen (nguoi da tao bao cao)

### Reset dinh ky (PointsResetBackgroundService)

Background service chay moi dem 00:00 UTC:

| Ngay | Reset |
|------|-------|
| Thu Hai | `WeekPoints = 0` cho tat ca users |
| Ngay 1 hang thang | `MonthPoints = 0` cho tat ca users |
| 1 thang 1 hang nam | `YearPoints = 0` cho tat ca users |
| Khong bao gio | `TotalPoints` — tong tich luy, khong reset |

**Leaderboard period mapping:**

| `period` query param | Cot su dung |
|---------------------|-------------|
| `Week` | `week_points` |
| `Month` | `month_points` |
| `Year` | `year_points` |

---

## 7. Collector Shift (TeamSession)

### Start Shift
```
POST /collector/reports/start-shift
{ data: { team_id, date } }

1. Kiểm tra collector thuộc team (via staffs table)
2. Kiểm tra team.InWork == false (chưa có ca đang chạy)
3. Query tất cả báo cáo Assigned của team hôm nay
4. Set status → Processing
5. Create TeamSession { TeamId, Date, StartAt=now }
6. Set Team.InWork = true
```

### End Shift
```
POST /collector/reports/end-shift
{ data: { team_id, date } }

1. Kiểm tra team.InWork == true
2. Tổng kết: count reports + sum capacity
3. Update TeamSession.EndAt = now, TotalReports, TotalCapacity
4. Set Team.InWork = false
```

### Submit Report
```
PATCH /collector/reports/{id}
{ status: "Collected", images: [...], weight: 12.5 }
// hoặc
{ status: "Failed", reason: "Địa chỉ không tìm thấy" }

Collected → set CollectedAt, ActualCapacityKg, CollectorImageUrls
Failed    → set ReportNote (reason)
```

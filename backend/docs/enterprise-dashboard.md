# Enterprise Dashboard API

## Endpoint

```
GET /api/v1/enterprise/dashboard
Authorization: Bearer <accessToken>   (role = Enterprise)
```

---

## Response Structure

```json
{
  "status": "success",
  "message": "get enterprise dashboard successfully",
  "data": {
    "today":    { ... },
    "summary":  { ... },
    "capacity": { ... },
    "monthly":  [ ... ],
    "teams":    [ ... ]
  }
}
```

---

## 1. `today` — Snapshot hiện tại (toàn bộ lifetime)

| Field         | Type | Mô tả |
|---------------|------|--------|
| `pending`     | int  | Số báo cáo đang chờ duyệt |
| `queue`       | int  | Số báo cáo đã duyệt, chờ assign |
| `assigned`    | int  | Số báo cáo đã assign cho tổ, chưa bắt đầu thu gom |
| `processing`  | int  | Số báo cáo đang được thu gom |
| `collected`   | int  | Số báo cáo đã thu gom xong, chờ xác nhận hoàn thành |
| `completed`   | int  | Số báo cáo hoàn thành **hôm nay** (theo `complete_at`) |
| `failed`      | int  | Số báo cáo thất bại **hôm nay** (theo `updated_at`) |
| `rejected`    | int  | Số báo cáo bị từ chối **hôm nay** (theo `updated_at`) |
| `active_teams`| int  | Số tổ đang hoạt động (`in_work = true`) |

> `pending`, `queue`, `assigned`, `processing`, `collected` là số liệu **toàn thời gian** (mọi báo cáo đang ở trạng thái đó).
> `completed`, `failed`, `rejected` trong `today` chỉ tính **báo cáo hoàn tất trong ngày hôm nay**.

---

## 2. `summary` — Tổng kết toàn thời gian

| Field                  | Type    | Mô tả |
|------------------------|---------|--------|
| `total`                | int     | Tổng số báo cáo liên quan đến enterprise |
| `completed`            | int     | Tổng số báo cáo đã hoàn thành (Completed) |
| `failed`               | int     | Tổng số báo cáo thất bại |
| `rejected`             | int     | Tổng số báo cáo bị từ chối |
| `completion_rate`      | decimal | Tỷ lệ hoàn thành (%) = completed / (completed + failed + rejected) × 100 |
| `avg_processing_hours` | double  | Thời gian xử lý trung bình (giờ) từ lúc báo cáo đến lúc hoàn thành |
| `total_kg`             | decimal | Tổng khối lượng rác đã thu gom (kg) — chỉ tính báo cáo Completed |

---

## 3. `capacity` — Năng lực thu gom theo loại rác

```json
{
  "total_kg": 150.5,
  "by_type": [
    { "type": "Organic",       "total_kg": 80.0 },
    { "type": "Recyclable",    "total_kg": 55.5 },
    { "type": "NonRecyclable", "total_kg": 15.0 }
  ]
}
```

| Field      | Type    | Mô tả |
|------------|---------|--------|
| `total_kg` | decimal | Tổng tất cả loại rác |
| `by_type`  | array   | Breakdown theo từng loại rác (Organic / Recyclable / NonRecyclable) |

> Chỉ tính báo cáo có trạng thái **Completed** và có `actual_capacity_kg`.

---

## 4. `monthly` — Thống kê theo tháng (12 tháng gần nhất)

```json
[
  {
    "month":     "2026-04",
    "total":     30,
    "completed": 20,
    "failed":    5,
    "rejected":  2,
    "total_kg":  85.3
  },
  ...
]
```

| Field       | Type    | Mô tả |
|-------------|---------|--------|
| `month`     | string  | Định dạng `yyyy-MM` |
| `total`     | int     | Tổng báo cáo tạo trong tháng |
| `completed` | int     | Số hoàn thành trong tháng |
| `failed`    | int     | Số thất bại trong tháng |
| `rejected`  | int     | Số bị từ chối trong tháng |
| `total_kg`  | decimal | Tổng kg thu gom trong tháng |

---

## 5. `teams` — Hiệu suất từng tổ

```json
[
  {
    "team_id":       "uuid",
    "team_name":     "Tổ 1",
    "collector_name":"Trạm Q1-A",
    "total":         25,
    "completed":     20,
    "failed":        3,
    "total_kg":      65.0,
    "session_count": 10
  },
  ...
]
```

| Field            | Type    | Mô tả |
|------------------|---------|--------|
| `team_id`        | uuid    | ID của tổ |
| `team_name`      | string  | Tên tổ |
| `collector_name` | string  | Tên trạm (collector hub) quản lý tổ |
| `total`          | int     | Tổng báo cáo được assign cho tổ |
| `completed`      | int     | Số báo cáo hoàn thành |
| `failed`         | int     | Số báo cáo thất bại |
| `total_kg`       | decimal | Tổng kg đã thu gom |
| `session_count`  | int     | Số ca làm việc đã thực hiện |

---

## Phạm vi dữ liệu

Enterprise chỉ thấy dữ liệu của **chính enterprise mình**:
- Các collector hub thuộc enterprise → các team thuộc collectors đó
- Báo cáo có `team_id` thuộc danh sách team của enterprise
- Báo cáo có `team_id = null` (Pending/Queue chưa assign) **cũng được tính** để enterprise thấy tổng workload

---

## Error responses

| HTTP | Code           | Khi nào |
|------|----------------|---------|
| 401  | UNAUTHORIZED   | Token không hợp lệ hoặc không tìm thấy enterprise |

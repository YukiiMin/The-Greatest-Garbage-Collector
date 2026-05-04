# Functional Requirements Audit
> Kiểm tra mức độ đáp ứng yêu cầu chức năng — cập nhật 2026-04-26

---

## Citizen

| # | Yêu cầu | Trạng thái | Endpoint |
|---|---------|-----------|---------|
| 1 | Báo cáo rác (ảnh + GPS + mô tả) | ✅ | `POST /api/v1/users/citizen-reports` |
| 2 | Theo dõi trạng thái báo cáo | ✅ | `GET /api/v1/users/citizen-reports` + `GET …/{id}` |
| 3 | Phân loại rác tại nguồn (WasteType) | ✅ | Enum: Organic / Recyclable / NonRecyclable |
| 4 | Nhận điểm thưởng khi báo cáo hợp lệ | ✅ | Tính điểm tự động trong `CollectorReportService` khi mark Collected |
| 5 | Xem lịch sử điểm cá nhân | ⚠️ MISSING | Chỉ có bảng xếp hạng — thiếu endpoint lịch sử điểm của cá nhân |
| 6 | Bảng xếp hạng theo khu vực | ✅ | `GET /api/v1/users/leaderboard?period=Week&scope=Ward` |
| 7 | Gửi khiếu nại | ✅ | `POST /api/v1/users/citizen-reports/{id}/complaints` |

---

## Recycling Enterprise

| # | Yêu cầu | Trạng thái | Endpoint |
|---|---------|-----------|---------|
| 1 | Quản lý năng lực (hubs / collectors / khu vực) | ✅ | `GET/POST/PATCH/DELETE /enterprise/hubs` + `/enterprise/collectors` |
| 2 | Tiếp nhận / từ chối yêu cầu | ✅ | `PATCH /enterprise/reports/{id}/queue` + `/reject` |
| 3 | Gán báo cáo cho Team | ✅ | `PATCH /enterprise/reports/{id}/assign` |
| 4 | Theo dõi tiến độ real-time | ✅ | `GET /enterprise/dashboard` + `GET /enterprise/reports?status=…` |
| 5 | Báo cáo khối lượng theo loại/khu vực/thời gian | ⚠️ MISSING | Dashboard có monthly breakdown nhưng thiếu dedicated analytics endpoint với filter tùy chỉnh |
| 6 | Cấu hình quy tắc tính điểm | ✅ | `GET/POST/PATCH/DELETE /enterprise/point-categories` với PointMechanic |

---

## Collector

| # | Yêu cầu | Trạng thái | Endpoint |
|---|---------|-----------|---------|
| 1 | Nhận yêu cầu được phân công | ✅ | `GET /api/v1/collector/reports` |
| 2 | Cập nhật trạng thái (Assigned → Processing → Collected) | ✅ | `PATCH /collector/reports/start-shift` + `PATCH /collector/reports/{id}` |
| 3 | Xác nhận hoàn tất bằng hình ảnh | ✅ | Upload ảnh bắt buộc khi status = Collected |
| 4 | Xem lịch sử + thống kê hoàn thành | ✅ | `GET /api/v1/collector/dashboard` |

---

## Administrator

| # | Yêu cầu | Trạng thái | Endpoint |
|---|---------|-----------|---------|
| 1 | Quản lý tài khoản & phân quyền | ✅ | `GET /admin/users`, `PATCH …/role`, `PATCH …/ban` |
| 2 | Giám sát hệ thống + quản lý Enterprise | ✅ | `GET/POST/PATCH/DELETE /admin/enterprises`, setup accounts |
| 3 | Tiếp nhận & giải quyết khiếu nại | ✅ | `GET /admin/complaints`, `PATCH /admin/complaints/{id}` |

---

## Tổng kết

| Actor | Tỷ lệ |
|-------|--------|
| Citizen | 6/7 (thiếu points history) |
| Enterprise | 5/6 (thiếu analytics endpoint) |
| Collector | 4/4 |
| Administrator | 3/3 |

---

## TODO — Gaps cần bổ sung

### 1. Lịch sử điểm của Citizen (Medium)

- **Vấn đề:** `UserPoints` chỉ lưu tổng điểm (week/month/year/total), không có bảng ghi lịch sử từng lần nhận điểm.
- **Phương án A (đơn giản):** Trả về danh sách CitizenReport của citizen kèm `points_earned`.
  ```
  GET /api/v1/users/points-history
  ```
- **Phương án B (đầy đủ):** Thêm bảng `PointTransaction` với `userId, reportId, points, reason, createdAt`.

### 2. Enterprise Analytics Endpoint (Medium)

- **Vấn đề:** Dashboard hiện tại có `monthly` breakdown và `capacity.by_type` nhưng không có filter theo khoảng thời gian tùy chỉnh hay theo khu vực cụ thể.
- **Đề xuất:**
  ```
  GET /api/v1/enterprise/analytics?from=2025-01-01&to=2025-03-31&waste_type=Organic&work_area_id=…
  ```
  Response: tổng khối lượng, breakdown theo loại rác, theo khu vực, theo tháng trong range.

### 3. Minor / Optional

- **Complaint thread history:** Có endpoint gửi tin nhắn nhưng cần verify có `GET /complaints/{id}/messages` không.
- **Gợi ý ưu tiên báo cáo cho Enterprise** (optional trong SRS) — chưa có, có thể bỏ qua.

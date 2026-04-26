# EcoConnect — Backend Documentation

Hệ thống quản lý thu gom rác thải đô thị. Kết nối Citizen báo cáo rác → Enterprise phân công → Collector thu gom.

---

## Table of Contents

| File | Nội dung |
|------|----------|
| [01_PROJECT_OVERVIEW.md](01_PROJECT_OVERVIEW.md) | Tổng quan dự án, actors, tech stack tóm tắt, folder structure |
| [02_TECHNOLOGIES.md](02_TECHNOLOGIES.md) | Tất cả packages, công nghệ, kỹ thuật đặc biệt được sử dụng |
| [03_ARCHITECTURE.md](03_ARCHITECTURE.md) | Clean Architecture 4 layers, middleware pipeline, DI, CORS, exception handling |
| [04_DATABASE.md](04_DATABASE.md) | 13 tables, schema, JSONB fields, relationships, lưu ý column name |
| [05_AUTH.md](05_AUTH.md) | Đăng ký, đăng nhập, JWT, cookie, refresh token rotation, reuse detection |
| [06_BUSINESS_RULES.md](06_BUSINESS_RULES.md) | Report state machine, business constraints, validation rules |
| [07_API_REFERENCE.md](07_API_REFERENCE.md) | 50+ endpoints với request/response JSON mẫu đầy đủ |
| [08_TEST_FLOWS.md](08_TEST_FLOWS.md) | 5 luồng test end-to-end: Citizen, Admin, Enterprise, Collector, Token |

---

## Quick Start

**Dev mới:** Đọc theo thứ tự `01 → 02 → 03 → 04 → 05 → 06`.

**Tester:** Đọc `07_API_REFERENCE.md` để biết endpoint, sau đó làm theo `08_TEST_FLOWS.md`.

**Tìm hiểu cụ thể:**
- "Tại sao dùng X?" → `02_TECHNOLOGIES.md`
- "Table này có gì?" → `04_DATABASE.md`
- "Token refresh hoạt động thế nào?" → `05_AUTH.md`
- "Báo cáo chuyển trạng thái thế nào?" → `06_BUSINESS_RULES.md`

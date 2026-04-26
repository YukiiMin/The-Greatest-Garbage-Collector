-- ============================================================
-- SEED DATA — EcoConnect / GarbageCollection
-- Chạy trong DBeaver hoặc bất kỳ PostgreSQL client nào
--
-- MẬT KHẨU (plain-text để test):
--   Tất cả tài khoản: Test@1234
--
-- TÀI KHOẢN:
--   admin@ecoconnect.vn           → Admin
--   enterprise1@ecoconnect.vn     → Enterprise (Quản lý DN)
--   enterprise2@ecoconnect.vn
--   enterprise3@ecoconnect.vn
--   enterprise4@ecoconnect.vn
--   enterprise5@ecoconnect.vn
--   collector1@ecoconnect.vn      → Collector (Nhân viên)
--   ...
--   collector10@ecoconnect.vn
--   citizen1@ecoconnect.vn        → Citizen (Người dân)
--   ...
--   citizen34@ecoconnect.vn
-- ============================================================

-- Đặt schema trước khi làm gì khác
-- public phải có trong path để pgcrypto functions (gen_salt, crypt) tìm thấy được
SET search_path TO minh, public;

-- Kích hoạt pgcrypto nếu chưa có
CREATE EXTENSION IF NOT EXISTS pgcrypto SCHEMA public;

-- Ghi chú: \set ON_ERROR_STOP on chỉ hoạt động với psql CLI, không có tác dụng trong DBeaver

-- Xoá data cũ theo thứ tự FK
TRUNCATE TABLE
  team_sessions,
  user_points,
  complaints,
  citizen_reports,
  staffs,
  point_categories,
  teams,
  collector_hub,
  enterprise_hub,
  users,
  work_areas
CASCADE;

-- ─────────────────────────────────────────────────────────────
-- 1. WORK AREAS  (5 District + 20 Ward = 25 rows)
-- ─────────────────────────────────────────────────────────────

INSERT INTO work_areas (id, name, type, parent_id, created_at) VALUES
  ('00000001-0000-0000-0001-000000000000', 'Quận 1',          'District', NULL, now()),
  ('00000002-0000-0000-0001-000000000000', 'Quận 3',          'District', NULL, now()),
  ('00000003-0000-0000-0001-000000000000', 'Quận Bình Thạnh', 'District', NULL, now()),
  ('00000004-0000-0000-0001-000000000000', 'Quận Tân Bình',   'District', NULL, now()),
  ('00000005-0000-0000-0001-000000000000', 'Quận Gò Vấp',     'District', NULL, now());

-- Wards Quận 1
INSERT INTO work_areas (id, name, type, parent_id, created_at) VALUES
  ('00000001-0000-0000-0002-000000000001', 'Phường Bến Nghé',         'Ward', '00000001-0000-0000-0001-000000000000', now()),
  ('00000001-0000-0000-0002-000000000002', 'Phường Cầu Kho',          'Ward', '00000001-0000-0000-0001-000000000000', now()),
  ('00000001-0000-0000-0002-000000000003', 'Phường Đa Kao',           'Ward', '00000001-0000-0000-0001-000000000000', now()),
  ('00000001-0000-0000-0002-000000000004', 'Phường Nguyễn Thái Bình', 'Ward', '00000001-0000-0000-0001-000000000000', now());

-- Wards Quận 3
INSERT INTO work_areas (id, name, type, parent_id, created_at) VALUES
  ('00000002-0000-0000-0002-000000000001', 'Phường 1', 'Ward', '00000002-0000-0000-0001-000000000000', now()),
  ('00000002-0000-0000-0002-000000000002', 'Phường 2', 'Ward', '00000002-0000-0000-0001-000000000000', now()),
  ('00000002-0000-0000-0002-000000000003', 'Phường 3', 'Ward', '00000002-0000-0000-0001-000000000000', now()),
  ('00000002-0000-0000-0002-000000000004', 'Phường 4', 'Ward', '00000002-0000-0000-0001-000000000000', now());

-- Wards Quận Bình Thạnh
INSERT INTO work_areas (id, name, type, parent_id, created_at) VALUES
  ('00000003-0000-0000-0002-000000000001', 'Phường 1', 'Ward', '00000003-0000-0000-0001-000000000000', now()),
  ('00000003-0000-0000-0002-000000000002', 'Phường 2', 'Ward', '00000003-0000-0000-0001-000000000000', now()),
  ('00000003-0000-0000-0002-000000000003', 'Phường 3', 'Ward', '00000003-0000-0000-0001-000000000000', now()),
  ('00000003-0000-0000-0002-000000000004', 'Phường 4', 'Ward', '00000003-0000-0000-0001-000000000000', now());

-- Wards Quận Tân Bình
INSERT INTO work_areas (id, name, type, parent_id, created_at) VALUES
  ('00000004-0000-0000-0002-000000000001', 'Phường 1', 'Ward', '00000004-0000-0000-0001-000000000000', now()),
  ('00000004-0000-0000-0002-000000000002', 'Phường 2', 'Ward', '00000004-0000-0000-0001-000000000000', now()),
  ('00000004-0000-0000-0002-000000000003', 'Phường 3', 'Ward', '00000004-0000-0000-0001-000000000000', now()),
  ('00000004-0000-0000-0002-000000000004', 'Phường 4', 'Ward', '00000004-0000-0000-0001-000000000000', now());

-- Wards Quận Gò Vấp
INSERT INTO work_areas (id, name, type, parent_id, created_at) VALUES
  ('00000005-0000-0000-0002-000000000001', 'Phường 1', 'Ward', '00000005-0000-0000-0001-000000000000', now()),
  ('00000005-0000-0000-0002-000000000002', 'Phường 2', 'Ward', '00000005-0000-0000-0001-000000000000', now()),
  ('00000005-0000-0000-0002-000000000003', 'Phường 3', 'Ward', '00000005-0000-0000-0001-000000000000', now()),
  ('00000005-0000-0000-0002-000000000004', 'Phường 4', 'Ward', '00000005-0000-0000-0001-000000000000', now());

-- ─────────────────────────────────────────────────────────────
-- 2. USERS  (50 rows)
-- role: 1=Citizen  2=Collector  3=Enterprise  4=Admin
-- crypt() tạo BCrypt hash tương thích với BCrypt.Net-Next
-- ─────────────────────────────────────────────────────────────

-- Admin (1)
INSERT INTO users (id, email, full_name, role, password_hash, provider, email_verified,
                   is_banned, is_login, login_term, work_area_id, created_at, updated_at)
VALUES (
  'a0000000-0000-0000-0000-000000000001',
  'admin@ecoconnect.vn', 'Quản Trị Viên', 4,
  crypt('Test@1234', gen_salt('bf', 11)),
  'local', true, false, false, 0, NULL, now(), now()
);

-- Enterprise users (5)
INSERT INTO users (id, email, full_name, role, password_hash, provider, email_verified,
                   is_banned, is_login, login_term, work_area_id, created_at, updated_at)
VALUES
  ('b0000000-0000-0000-0000-000000000001', 'enterprise1@ecoconnect.vn', 'Nguyễn Văn An',  3, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000001-0000-0000-0001-000000000000', now(), now()),
  ('b0000000-0000-0000-0000-000000000002', 'enterprise2@ecoconnect.vn', 'Trần Thị Bích',  3, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000002-0000-0000-0001-000000000000', now(), now()),
  ('b0000000-0000-0000-0000-000000000003', 'enterprise3@ecoconnect.vn', 'Lê Văn Cường',   3, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000003-0000-0000-0001-000000000000', now(), now()),
  ('b0000000-0000-0000-0000-000000000004', 'enterprise4@ecoconnect.vn', 'Phạm Thị Dung',  3, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000004-0000-0000-0001-000000000000', now(), now()),
  ('b0000000-0000-0000-0000-000000000005', 'enterprise5@ecoconnect.vn', 'Hoàng Văn Em',   3, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000005-0000-0000-0001-000000000000', now(), now());

-- Collector users (10)
INSERT INTO users (id, email, full_name, role, password_hash, provider, email_verified,
                   is_banned, is_login, login_term, work_area_id, created_at, updated_at)
VALUES
  ('c0000000-0000-0000-0000-000000000001', 'collector1@ecoconnect.vn',  'Nguyễn Thị Fang',  2, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000001-0000-0000-0002-000000000001', now(), now()),
  ('c0000000-0000-0000-0000-000000000002', 'collector2@ecoconnect.vn',  'Trần Văn Giang',   2, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000001-0000-0000-0002-000000000002', now(), now()),
  ('c0000000-0000-0000-0000-000000000003', 'collector3@ecoconnect.vn',  'Lê Thị Hoa',       2, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000001-0000-0000-0002-000000000003', now(), now()),
  ('c0000000-0000-0000-0000-000000000004', 'collector4@ecoconnect.vn',  'Phạm Văn Inh',     2, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000001-0000-0000-0002-000000000004', now(), now()),
  ('c0000000-0000-0000-0000-000000000005', 'collector5@ecoconnect.vn',  'Hoàng Thị Khánh',  2, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000002-0000-0000-0002-000000000001', now(), now()),
  ('c0000000-0000-0000-0000-000000000006', 'collector6@ecoconnect.vn',  'Vũ Văn Long',      2, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000002-0000-0000-0002-000000000002', now(), now()),
  ('c0000000-0000-0000-0000-000000000007', 'collector7@ecoconnect.vn',  'Đặng Thị Minh',    2, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000003-0000-0000-0002-000000000001', now(), now()),
  ('c0000000-0000-0000-0000-000000000008', 'collector8@ecoconnect.vn',  'Bùi Văn Nam',      2, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000003-0000-0000-0002-000000000002', now(), now()),
  ('c0000000-0000-0000-0000-000000000009', 'collector9@ecoconnect.vn',  'Đinh Thị Oanh',    2, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000004-0000-0000-0002-000000000001', now(), now()),
  ('c0000000-0000-0000-0000-000000000010', 'collector10@ecoconnect.vn', 'Đỗ Văn Phong',     2, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000004-0000-0000-0002-000000000002', now(), now());

-- Citizen users (34)
INSERT INTO users (id, email, full_name, role, password_hash, provider, email_verified,
                   is_banned, is_login, login_term, work_area_id, created_at, updated_at)
VALUES
  ('d0000000-0000-0000-0000-000000000001', 'citizen1@ecoconnect.vn',  'Phan Văn Quyết',  1, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000001-0000-0000-0002-000000000001', now(), now()),
  ('d0000000-0000-0000-0000-000000000002', 'citizen2@ecoconnect.vn',  'Ngô Thị Rồng',    1, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000001-0000-0000-0002-000000000002', now(), now()),
  ('d0000000-0000-0000-0000-000000000003', 'citizen3@ecoconnect.vn',  'Trịnh Văn Sơn',   1, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000001-0000-0000-0002-000000000003', now(), now()),
  ('d0000000-0000-0000-0000-000000000004', 'citizen4@ecoconnect.vn',  'Lý Thị Tùng',     1, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000001-0000-0000-0002-000000000004', now(), now()),
  ('d0000000-0000-0000-0000-000000000005', 'citizen5@ecoconnect.vn',  'Mai Văn Uy',       1, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000002-0000-0000-0002-000000000001', now(), now()),
  ('d0000000-0000-0000-0000-000000000006', 'citizen6@ecoconnect.vn',  'Cao Thị Vân',      1, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000002-0000-0000-0002-000000000002', now(), now()),
  ('d0000000-0000-0000-0000-000000000007', 'citizen7@ecoconnect.vn',  'Điền Văn Xanh',   1, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000002-0000-0000-0002-000000000003', now(), now()),
  ('d0000000-0000-0000-0000-000000000008', 'citizen8@ecoconnect.vn',  'Hoà Thị Yến',     1, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000002-0000-0000-0002-000000000004', now(), now()),
  ('d0000000-0000-0000-0000-000000000009', 'citizen9@ecoconnect.vn',  'Kha Văn Zịnh',    1, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000003-0000-0000-0002-000000000001', now(), now()),
  ('d0000000-0000-0000-0000-000000000010', 'citizen10@ecoconnect.vn', 'Lâm Thị An',      1, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000003-0000-0000-0002-000000000002', now(), now()),
  ('d0000000-0000-0000-0000-000000000011', 'citizen11@ecoconnect.vn', 'Lũng Văn Bình',   1, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000003-0000-0000-0002-000000000003', now(), now()),
  ('d0000000-0000-0000-0000-000000000012', 'citizen12@ecoconnect.vn', 'Môn Thị Chi',     1, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000003-0000-0000-0002-000000000004', now(), now()),
  ('d0000000-0000-0000-0000-000000000013', 'citizen13@ecoconnect.vn', 'Ngân Văn Đức',    1, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000004-0000-0000-0002-000000000001', now(), now()),
  ('d0000000-0000-0000-0000-000000000014', 'citizen14@ecoconnect.vn', 'Ổn Thị Em',       1, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000004-0000-0000-0002-000000000002', now(), now()),
  ('d0000000-0000-0000-0000-000000000015', 'citizen15@ecoconnect.vn', 'Phong Văn Phát',  1, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000004-0000-0000-0002-000000000003', now(), now()),
  ('d0000000-0000-0000-0000-000000000016', 'citizen16@ecoconnect.vn', 'Quang Thị Giao',  1, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000004-0000-0000-0002-000000000004', now(), now()),
  ('d0000000-0000-0000-0000-000000000017', 'citizen17@ecoconnect.vn', 'Sơn Văn Hải',     1, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000005-0000-0000-0002-000000000001', now(), now()),
  ('d0000000-0000-0000-0000-000000000018', 'citizen18@ecoconnect.vn', 'Tâm Thị Inh',     1, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000005-0000-0000-0002-000000000002', now(), now()),
  ('d0000000-0000-0000-0000-000000000019', 'citizen19@ecoconnect.vn', 'Ước Văn Kiên',    1, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000005-0000-0000-0002-000000000003', now(), now()),
  ('d0000000-0000-0000-0000-000000000020', 'citizen20@ecoconnect.vn', 'Vạn Thị Linh',    1, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000005-0000-0000-0002-000000000004', now(), now()),
  ('d0000000-0000-0000-0000-000000000021', 'citizen21@ecoconnect.vn', 'Xuân Văn Minh',   1, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000001-0000-0000-0002-000000000001', now(), now()),
  ('d0000000-0000-0000-0000-000000000022', 'citizen22@ecoconnect.vn', 'Yên Thị Nhi',     1, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000001-0000-0000-0002-000000000002', now(), now()),
  ('d0000000-0000-0000-0000-000000000023', 'citizen23@ecoconnect.vn', 'Ảnh Văn Oanh',    1, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000001-0000-0000-0002-000000000003', now(), now()),
  ('d0000000-0000-0000-0000-000000000024', 'citizen24@ecoconnect.vn', 'Bảo Thị Phụng',   1, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000002-0000-0000-0002-000000000001', now(), now()),
  ('d0000000-0000-0000-0000-000000000025', 'citizen25@ecoconnect.vn', 'Các Văn Quý',     1, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000002-0000-0000-0002-000000000002', now(), now()),
  ('d0000000-0000-0000-0000-000000000026', 'citizen26@ecoconnect.vn', 'Dạ Thị Rực',      1, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000002-0000-0000-0002-000000000003', now(), now()),
  ('d0000000-0000-0000-0000-000000000027', 'citizen27@ecoconnect.vn', 'Ê Văn Sáng',      1, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000003-0000-0000-0002-000000000001', now(), now()),
  ('d0000000-0000-0000-0000-000000000028', 'citizen28@ecoconnect.vn', 'Gia Thị Thủy',    1, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000003-0000-0000-0002-000000000002', now(), now()),
  ('d0000000-0000-0000-0000-000000000029', 'citizen29@ecoconnect.vn', 'Hà Văn Uyên',     1, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000003-0000-0000-0002-000000000003', now(), now()),
  ('d0000000-0000-0000-0000-000000000030', 'citizen30@ecoconnect.vn', 'Ích Thị Vui',     1, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000004-0000-0000-0002-000000000001', now(), now()),
  ('d0000000-0000-0000-0000-000000000031', 'citizen31@ecoconnect.vn', 'Kén Văn Xuân',    1, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000004-0000-0000-0002-000000000002', now(), now()),
  ('d0000000-0000-0000-0000-000000000032', 'citizen32@ecoconnect.vn', 'Lợi Thị Yêu',     1, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000004-0000-0000-0002-000000000003', now(), now()),
  ('d0000000-0000-0000-0000-000000000033', 'citizen33@ecoconnect.vn', 'Mỹ Văn Ẩn',      1, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000005-0000-0000-0002-000000000001', now(), now()),
  ('d0000000-0000-0000-0000-000000000034', 'citizen34@ecoconnect.vn', 'Nâu Thị Bậu',    1, crypt('Test@1234', gen_salt('bf', 11)), 'local', true, false, false, 0, '00000005-0000-0000-0002-000000000002', now(), now());

-- ─────────────────────────────────────────────────────────────
-- 3. ENTERPRISE HUB  (5 rows)
-- ─────────────────────────────────────────────────────────────

INSERT INTO enterprise_hub (id, name, phone_number, email, address, latitude, longitude, work_area_id, created_at, updated_at)
VALUES
  ('e0000000-0000-0000-0000-000000000001', 'Công ty TNHH Môi Trường Xanh Q1',   '0901234561', 'enterprise1@ecoconnect.vn', '123 Nguyễn Huệ, Q.1',               10.775158, 106.701916, '00000001-0000-0000-0001-000000000000', now(), now()),
  ('e0000000-0000-0000-0000-000000000002', 'Công ty TNHH Sạch Phố Q3',          '0901234562', 'enterprise2@ecoconnect.vn', '456 Nam Kỳ Khởi Nghĩa, Q.3',        10.769132, 106.687424, '00000002-0000-0000-0001-000000000000', now(), now()),
  ('e0000000-0000-0000-0000-000000000003', 'Công ty CP Thu Gom Bình Thạnh',      '0901234563', 'enterprise3@ecoconnect.vn', '789 Đinh Bộ Lĩnh, Q.Bình Thạnh',    10.814212, 106.709543, '00000003-0000-0000-0001-000000000000', now(), now()),
  ('e0000000-0000-0000-0000-000000000004', 'Công ty TNHH Rác Tái Chế Tân Bình', '0901234564', 'enterprise4@ecoconnect.vn', '321 Hoàng Văn Thụ, Q.Tân Bình',     10.799001, 106.655342, '00000004-0000-0000-0001-000000000000', now(), now()),
  ('e0000000-0000-0000-0000-000000000005', 'Công ty CP Xử Lý Rác Gò Vấp',       '0901234565', 'enterprise5@ecoconnect.vn', '654 Quang Trung, Q.Gò Vấp',          10.839872, 106.674123, '00000005-0000-0000-0001-000000000000', now(), now());

-- ─────────────────────────────────────────────────────────────
-- 4. COLLECTOR HUB  (5 rows)
-- ─────────────────────────────────────────────────────────────

INSERT INTO collector_hub (id, name, phone_number, email, address, latitude, longitude, enterprise_id, work_area_id, assigned_capacity, created_at, updated_at)
VALUES
  ('f0000000-0000-0000-0000-000000000001', 'Trạm Q1-A',         '0911234561', 'coll1@ecoconnect.vn', '10 Lê Lợi, Q.1',              10.773100, 106.700234, 'e0000000-0000-0000-0000-000000000001', '00000001-0000-0000-0002-000000000001', 500, now(), now()),
  ('f0000000-0000-0000-0000-000000000002', 'Trạm Q3-A',         '0911234562', 'coll2@ecoconnect.vn', '20 Võ Văn Tần, Q.3',          10.767450, 106.685012, 'e0000000-0000-0000-0000-000000000002', '00000002-0000-0000-0002-000000000001', 500, now(), now()),
  ('f0000000-0000-0000-0000-000000000003', 'Trạm Bình Thạnh-A', '0911234563', 'coll3@ecoconnect.vn', '30 Bạch Đằng, Q.Bình Thạnh',  10.812300, 106.707654, 'e0000000-0000-0000-0000-000000000003', '00000003-0000-0000-0002-000000000001', 500, now(), now()),
  ('f0000000-0000-0000-0000-000000000004', 'Trạm Tân Bình-A',   '0911234564', 'coll4@ecoconnect.vn', '40 Cộng Hòa, Q.Tân Bình',     10.797812, 106.653211, 'e0000000-0000-0000-0000-000000000004', '00000004-0000-0000-0002-000000000001', 500, now(), now()),
  ('f0000000-0000-0000-0000-000000000005', 'Trạm Gò Vấp-A',     '0911234565', 'coll5@ecoconnect.vn', '50 Lê Đức Thọ, Q.Gò Vấp',    10.837321, 106.672890, 'e0000000-0000-0000-0000-000000000005', '00000005-0000-0000-0002-000000000001', 500, now(), now());

-- ─────────────────────────────────────────────────────────────
-- 5. TEAMS  (10 rows — 2 per collector)
-- ─────────────────────────────────────────────────────────────

INSERT INTO teams (id, name, total_capacity, is_active, collector_id, work_area_id, dispatch_time, route_optimized, in_work, created_at, updated_at)
VALUES
  ('70000000-0000-0000-0000-000000000001', 'Tổ 1', 200, true, 'f0000000-0000-0000-0000-000000000001', '00000001-0000-0000-0002-000000000001', '07:00', false, false, now(), now()),
  ('70000000-0000-0000-0000-000000000002', 'Tổ 2', 200, true, 'f0000000-0000-0000-0000-000000000001', '00000001-0000-0000-0002-000000000002', '07:00', false, false, now(), now()),
  ('70000000-0000-0000-0000-000000000003', 'Tổ 3', 200, true, 'f0000000-0000-0000-0000-000000000002', '00000002-0000-0000-0002-000000000001', '07:00', false, false, now(), now()),
  ('70000000-0000-0000-0000-000000000004', 'Tổ 4', 200, true, 'f0000000-0000-0000-0000-000000000002', '00000002-0000-0000-0002-000000000002', '07:00', false, false, now(), now()),
  ('70000000-0000-0000-0000-000000000005', 'Tổ 5', 200, true, 'f0000000-0000-0000-0000-000000000003', '00000003-0000-0000-0002-000000000001', '07:00', false, false, now(), now()),
  ('70000000-0000-0000-0000-000000000006', 'Tổ 6', 200, true, 'f0000000-0000-0000-0000-000000000003', '00000003-0000-0000-0002-000000000002', '07:00', false, false, now(), now()),
  ('70000000-0000-0000-0000-000000000007', 'Tổ 7', 200, true, 'f0000000-0000-0000-0000-000000000004', '00000004-0000-0000-0002-000000000001', '07:00', false, false, now(), now()),
  ('70000000-0000-0000-0000-000000000008', 'Tổ 8', 200, true, 'f0000000-0000-0000-0000-000000000004', '00000004-0000-0000-0002-000000000002', '07:00', false, false, now(), now()),
  ('70000000-0000-0000-0000-000000000009', 'Tổ 9', 200, true, 'f0000000-0000-0000-0000-000000000005', '00000005-0000-0000-0002-000000000001', '07:00', false, false, now(), now()),
  ('70000000-0000-0000-0000-000000000010', 'Tổ 10',200, true, 'f0000000-0000-0000-0000-000000000005', '00000005-0000-0000-0002-000000000002', '07:00', false, false, now(), now());

-- ─────────────────────────────────────────────────────────────
-- 6. STAFFS  (10 rows — 2 collector users per enterprise)
-- ─────────────────────────────────────────────────────────────

INSERT INTO staffs (user_id, enterprise_id, collector_id, team_id, join_team_at)
VALUES
  ('c0000000-0000-0000-0000-000000000001', 'e0000000-0000-0000-0000-000000000001', 'f0000000-0000-0000-0000-000000000001', '70000000-0000-0000-0000-000000000001', now() - interval '30 days'),
  ('c0000000-0000-0000-0000-000000000002', 'e0000000-0000-0000-0000-000000000001', 'f0000000-0000-0000-0000-000000000001', '70000000-0000-0000-0000-000000000002', now() - interval '25 days'),
  ('c0000000-0000-0000-0000-000000000003', 'e0000000-0000-0000-0000-000000000002', 'f0000000-0000-0000-0000-000000000002', '70000000-0000-0000-0000-000000000003', now() - interval '30 days'),
  ('c0000000-0000-0000-0000-000000000004', 'e0000000-0000-0000-0000-000000000002', 'f0000000-0000-0000-0000-000000000002', '70000000-0000-0000-0000-000000000004', now() - interval '25 days'),
  ('c0000000-0000-0000-0000-000000000005', 'e0000000-0000-0000-0000-000000000003', 'f0000000-0000-0000-0000-000000000003', '70000000-0000-0000-0000-000000000005', now() - interval '30 days'),
  ('c0000000-0000-0000-0000-000000000006', 'e0000000-0000-0000-0000-000000000003', 'f0000000-0000-0000-0000-000000000003', '70000000-0000-0000-0000-000000000006', now() - interval '25 days'),
  ('c0000000-0000-0000-0000-000000000007', 'e0000000-0000-0000-0000-000000000004', 'f0000000-0000-0000-0000-000000000004', '70000000-0000-0000-0000-000000000007', now() - interval '30 days'),
  ('c0000000-0000-0000-0000-000000000008', 'e0000000-0000-0000-0000-000000000004', 'f0000000-0000-0000-0000-000000000004', '70000000-0000-0000-0000-000000000008', now() - interval '25 days'),
  ('c0000000-0000-0000-0000-000000000009', 'e0000000-0000-0000-0000-000000000005', 'f0000000-0000-0000-0000-000000000005', '70000000-0000-0000-0000-000000000009', now() - interval '30 days'),
  ('c0000000-0000-0000-0000-000000000010', 'e0000000-0000-0000-0000-000000000005', 'f0000000-0000-0000-0000-000000000005', '70000000-0000-0000-0000-000000000010', now() - interval '25 days');

-- ─────────────────────────────────────────────────────────────
-- 7. POINT CATEGORIES  (10 rows)
-- mechanic lưu dạng JSONB
-- ─────────────────────────────────────────────────────────────

INSERT INTO point_categories (id, name, enterprise_id, is_active, mechanic, created_at, updated_at)
VALUES
  ('80000000-0000-0000-0000-000000000001', 'Chương Trình Tháng 4 - E1', 'e0000000-0000-0000-0000-000000000001', true,  '{"Organic":{"Points":10,"MinWeightGrams":500},"Recyclable":{"Points":20,"MinWeightGrams":300},"NonRecyclable":{"Points":5,"MinWeightGrams":1000}}'::jsonb, now(), now()),
  ('80000000-0000-0000-0000-000000000002', 'Chương Trình Tháng 3 - E1', 'e0000000-0000-0000-0000-000000000001', false, '{"Organic":{"Points":8,"MinWeightGrams":500},"Recyclable":{"Points":15,"MinWeightGrams":300},"NonRecyclable":{"Points":4,"MinWeightGrams":1000}}'::jsonb, now() - interval '1 month', now() - interval '1 month'),
  ('80000000-0000-0000-0000-000000000003', 'Chương Trình Tháng 4 - E2', 'e0000000-0000-0000-0000-000000000002', true,  '{"Organic":{"Points":12,"MinWeightGrams":400},"Recyclable":{"Points":25,"MinWeightGrams":200},"NonRecyclable":{"Points":6,"MinWeightGrams":800}}'::jsonb, now(), now()),
  ('80000000-0000-0000-0000-000000000004', 'Chương Trình Tháng 3 - E2', 'e0000000-0000-0000-0000-000000000002', false, '{"Organic":{"Points":10,"MinWeightGrams":500},"Recyclable":{"Points":20,"MinWeightGrams":300},"NonRecyclable":{"Points":5,"MinWeightGrams":1000}}'::jsonb, now() - interval '1 month', now() - interval '1 month'),
  ('80000000-0000-0000-0000-000000000005', 'Chương Trình Tháng 4 - E3', 'e0000000-0000-0000-0000-000000000003', true,  '{"Organic":{"Points":10,"MinWeightGrams":500},"Recyclable":{"Points":20,"MinWeightGrams":300},"NonRecyclable":{"Points":5,"MinWeightGrams":1000}}'::jsonb, now(), now()),
  ('80000000-0000-0000-0000-000000000006', 'Chương Trình Tháng 3 - E3', 'e0000000-0000-0000-0000-000000000003', false, '{"Organic":{"Points":10,"MinWeightGrams":500},"Recyclable":{"Points":20,"MinWeightGrams":300},"NonRecyclable":{"Points":5,"MinWeightGrams":1000}}'::jsonb, now() - interval '1 month', now() - interval '1 month'),
  ('80000000-0000-0000-0000-000000000007', 'Chương Trình Tháng 4 - E4', 'e0000000-0000-0000-0000-000000000004', true,  '{"Organic":{"Points":10,"MinWeightGrams":500},"Recyclable":{"Points":20,"MinWeightGrams":300},"NonRecyclable":{"Points":5,"MinWeightGrams":1000}}'::jsonb, now(), now()),
  ('80000000-0000-0000-0000-000000000008', 'Chương Trình Tháng 3 - E4', 'e0000000-0000-0000-0000-000000000004', false, '{"Organic":{"Points":10,"MinWeightGrams":500},"Recyclable":{"Points":20,"MinWeightGrams":300},"NonRecyclable":{"Points":5,"MinWeightGrams":1000}}'::jsonb, now() - interval '1 month', now() - interval '1 month'),
  ('80000000-0000-0000-0000-000000000009', 'Chương Trình Tháng 4 - E5', 'e0000000-0000-0000-0000-000000000005', true,  '{"Organic":{"Points":10,"MinWeightGrams":500},"Recyclable":{"Points":20,"MinWeightGrams":300},"NonRecyclable":{"Points":5,"MinWeightGrams":1000}}'::jsonb, now(), now()),
  ('80000000-0000-0000-0000-000000000010', 'Chương Trình Tháng 3 - E5', 'e0000000-0000-0000-0000-000000000005', false, '{"Organic":{"Points":10,"MinWeightGrams":500},"Recyclable":{"Points":20,"MinWeightGrams":300},"NonRecyclable":{"Points":5,"MinWeightGrams":1000}}'::jsonb, now() - interval '1 month', now() - interval '1 month');

-- ─────────────────────────────────────────────────────────────
-- 8. CITIZEN REPORTS  (50 rows — 5 per citizen user)
-- status lưu dạng text (tên enum)
-- types lưu dạng JSON array số nguyên: 1=Organic 2=Recyclable 3=NonRecyclable
-- ─────────────────────────────────────────────────────────────

INSERT INTO citizen_reports
  ("Id", citizen_id, "Types", "Capacity", "Description", "Status", "CitizenImageUrls", "CollectorImageUrls",
   report_at, "CreatedAt", "PointCategoryId", team_id, assign_by, assign_at, deadline,
   start_collecting_at, collected_at, actual_capacity_kg, complete_at, "ReportNote", "Point")
VALUES
-- Pending (chưa xét duyệt)
  ('90000001-0000-0000-0000-000000000001', 'd0000000-0000-0000-0000-000000000001', '[1]',   2.0, 'Rác sinh hoạt trước nhà số 1',   'Pending',    '[]', '[]', now()-'5 days'::interval,  now()-'5 days'::interval,  NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('90000001-0000-0000-0000-000000000002', 'd0000000-0000-0000-0000-000000000002', '[1,2]', 3.5, 'Rác hỗn hợp khu vực số 2',      'Pending',    '[]', '[]', now()-'4 days'::interval,  now()-'4 days'::interval,  NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('90000001-0000-0000-0000-000000000003', 'd0000000-0000-0000-0000-000000000003', '[2]',   1.5, 'Chai nhựa tái chế số 3',         'Pending',    '[]', '[]', now()-'3 days'::interval,  now()-'3 days'::interval,  NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('90000001-0000-0000-0000-000000000004', 'd0000000-0000-0000-0000-000000000004', '[1,3]', 4.0, 'Rác không phân loại số 4',       'Pending',    '[]', '[]', now()-'2 days'::interval,  now()-'2 days'::interval,  NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('90000001-0000-0000-0000-000000000005', 'd0000000-0000-0000-0000-000000000005', '[1]',   2.5, 'Rác thực phẩm thừa số 5',        'Pending',    '[]', '[]', now()-'1 days'::interval,  now()-'1 days'::interval,  NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
-- Queue
  ('90000002-0000-0000-0000-000000000001', 'd0000000-0000-0000-0000-000000000006', '[1]',   3.0, 'Rác chờ được assign số 6',       'Queue',      '[]', '[]', now()-'10 days'::interval, now()-'10 days'::interval, '80000000-0000-0000-0000-000000000001', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('90000002-0000-0000-0000-000000000002', 'd0000000-0000-0000-0000-000000000007', '[2]',   2.0, 'Rác tái chế chờ xử lý số 7',    'Queue',      '[]', '[]', now()-'9 days'::interval,  now()-'9 days'::interval,  '80000000-0000-0000-0000-000000000003', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('90000002-0000-0000-0000-000000000003', 'd0000000-0000-0000-0000-000000000008', '[1,2]', 5.0, 'Rác lớn cần xe đặc biệt số 8',  'Queue',      '[]', '[]', now()-'8 days'::interval,  now()-'8 days'::interval,  '80000000-0000-0000-0000-000000000005', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('90000002-0000-0000-0000-000000000004', 'd0000000-0000-0000-0000-000000000009', '[3]',   1.0, 'Rác điện tử số 9',              'Queue',      '[]', '[]', now()-'7 days'::interval,  now()-'7 days'::interval,  '80000000-0000-0000-0000-000000000007', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('90000002-0000-0000-0000-000000000005', 'd0000000-0000-0000-0000-000000000010', '[1]',   2.5, 'Rác hữu cơ khu chợ số 10',      'Queue',      '[]', '[]', now()-'6 days'::interval,  now()-'6 days'::interval,  '80000000-0000-0000-0000-000000000009', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
-- Assigned
  ('90000003-0000-0000-0000-000000000001', 'd0000000-0000-0000-0000-000000000011', '[1]',   3.0, 'Đã assign cho tổ 1 số 11',       'Assigned',   '[]', '[]', now()-'15 days'::interval, now()-'15 days'::interval, '80000000-0000-0000-0000-000000000001', '70000000-0000-0000-0000-000000000001', 'b0000000-0000-0000-0000-000000000001', now()-'14 days'::interval, now()-'13 days'::interval, NULL, NULL, NULL, NULL, NULL, NULL),
  ('90000003-0000-0000-0000-000000000002', 'd0000000-0000-0000-0000-000000000012', '[1,2]', 4.0, 'Đã assign cho tổ 2 số 12',       'Assigned',   '[]', '[]', now()-'14 days'::interval, now()-'14 days'::interval, '80000000-0000-0000-0000-000000000003', '70000000-0000-0000-0000-000000000002', 'b0000000-0000-0000-0000-000000000002', now()-'13 days'::interval, now()-'12 days'::interval, NULL, NULL, NULL, NULL, NULL, NULL),
  ('90000003-0000-0000-0000-000000000003', 'd0000000-0000-0000-0000-000000000013', '[2]',   2.0, 'Đã assign cho tổ 3 số 13',       'Assigned',   '[]', '[]', now()-'13 days'::interval, now()-'13 days'::interval, '80000000-0000-0000-0000-000000000005', '70000000-0000-0000-0000-000000000003', 'b0000000-0000-0000-0000-000000000003', now()-'12 days'::interval, now()-'11 days'::interval, NULL, NULL, NULL, NULL, NULL, NULL),
  ('90000003-0000-0000-0000-000000000004', 'd0000000-0000-0000-0000-000000000014', '[1,3]', 3.5, 'Đã assign cho tổ 4 số 14',       'Assigned',   '[]', '[]', now()-'12 days'::interval, now()-'12 days'::interval, '80000000-0000-0000-0000-000000000007', '70000000-0000-0000-0000-000000000004', 'b0000000-0000-0000-0000-000000000004', now()-'11 days'::interval, now()-'10 days'::interval, NULL, NULL, NULL, NULL, NULL, NULL),
  ('90000003-0000-0000-0000-000000000005', 'd0000000-0000-0000-0000-000000000015', '[1]',   2.5, 'Đã assign cho tổ 5 số 15',       'Assigned',   '[]', '[]', now()-'11 days'::interval, now()-'11 days'::interval, '80000000-0000-0000-0000-000000000009', '70000000-0000-0000-0000-000000000005', 'b0000000-0000-0000-0000-000000000005', now()-'10 days'::interval, now()-'9 days'::interval,  NULL, NULL, NULL, NULL, NULL, NULL),
-- Processing
  ('90000004-0000-0000-0000-000000000001', 'd0000000-0000-0000-0000-000000000016', '[1]',   3.0, 'Đang thu gom số 16',             'Processing', '[]', '[]', now()-'5 days'::interval,  now()-'5 days'::interval,  '80000000-0000-0000-0000-000000000001', '70000000-0000-0000-0000-000000000001', 'b0000000-0000-0000-0000-000000000001', now()-'4 days'::interval, now()-'3 days'::interval, now()-'3 days'::interval+interval'7 hours', NULL, NULL, NULL, NULL, NULL),
  ('90000004-0000-0000-0000-000000000002', 'd0000000-0000-0000-0000-000000000017', '[2]',   2.0, 'Đang thu gom số 17',             'Processing', '[]', '[]', now()-'4 days'::interval,  now()-'4 days'::interval,  '80000000-0000-0000-0000-000000000003', '70000000-0000-0000-0000-000000000003', 'b0000000-0000-0000-0000-000000000002', now()-'3 days'::interval, now()-'2 days'::interval, now()-'2 days'::interval+interval'7 hours', NULL, NULL, NULL, NULL, NULL),
  ('90000004-0000-0000-0000-000000000003', 'd0000000-0000-0000-0000-000000000018', '[1,2]', 4.5, 'Đang thu gom số 18',             'Processing', '[]', '[]', now()-'3 days'::interval,  now()-'3 days'::interval,  '80000000-0000-0000-0000-000000000005', '70000000-0000-0000-0000-000000000005', 'b0000000-0000-0000-0000-000000000003', now()-'2 days'::interval, now()-'1 days'::interval, now()-'1 days'::interval+interval'7 hours', NULL, NULL, NULL, NULL, NULL),
  ('90000004-0000-0000-0000-000000000004', 'd0000000-0000-0000-0000-000000000019', '[3]',   1.5, 'Đang thu gom số 19',             'Processing', '[]', '[]', now()-'2 days'::interval,  now()-'2 days'::interval,  '80000000-0000-0000-0000-000000000007', '70000000-0000-0000-0000-000000000007', 'b0000000-0000-0000-0000-000000000004', now()-'1 days'::interval, now()+interval'1 day', now()+interval'1 hour', NULL, NULL, NULL, NULL, NULL),
  ('90000004-0000-0000-0000-000000000005', 'd0000000-0000-0000-0000-000000000020', '[1]',   2.0, 'Đang thu gom số 20',             'Processing', '[]', '[]', now()-'1 days'::interval,  now()-'1 days'::interval,  '80000000-0000-0000-0000-000000000009', '70000000-0000-0000-0000-000000000009', 'b0000000-0000-0000-0000-000000000005', now(),              now()+interval'1 day', now()+interval'2 hours', NULL, NULL, NULL, NULL, NULL),
-- Collected
  ('90000005-0000-0000-0000-000000000001', 'd0000000-0000-0000-0000-000000000021', '[1]',   3.0, 'Đã thu gom xong số 21',          'Collected',  '[]', '[]', now()-'20 days'::interval, now()-'20 days'::interval, '80000000-0000-0000-0000-000000000001', '70000000-0000-0000-0000-000000000001', 'b0000000-0000-0000-0000-000000000001', now()-'19 days'::interval, now()-'18 days'::interval, now()-'18 days'::interval+interval'7 hours', now()-'18 days'::interval+interval'10 hours', 2.7, NULL, NULL, 30),
  ('90000005-0000-0000-0000-000000000002', 'd0000000-0000-0000-0000-000000000022', '[1,2]', 4.0, 'Đã thu gom xong số 22',          'Collected',  '[]', '[]', now()-'19 days'::interval, now()-'19 days'::interval, '80000000-0000-0000-0000-000000000003', '70000000-0000-0000-0000-000000000002', 'b0000000-0000-0000-0000-000000000002', now()-'18 days'::interval, now()-'17 days'::interval, now()-'17 days'::interval+interval'7 hours', now()-'17 days'::interval+interval'10 hours', 3.6, NULL, NULL, 50),
  ('90000005-0000-0000-0000-000000000003', 'd0000000-0000-0000-0000-000000000023', '[2]',   2.5, 'Đã thu gom xong số 23',          'Collected',  '[]', '[]', now()-'18 days'::interval, now()-'18 days'::interval, '80000000-0000-0000-0000-000000000005', '70000000-0000-0000-0000-000000000004', 'b0000000-0000-0000-0000-000000000003', now()-'17 days'::interval, now()-'16 days'::interval, now()-'16 days'::interval+interval'7 hours', now()-'16 days'::interval+interval'10 hours', 2.3, NULL, NULL, 20),
  ('90000005-0000-0000-0000-000000000004', 'd0000000-0000-0000-0000-000000000024', '[1,3]', 3.5, 'Đã thu gom xong số 24',          'Collected',  '[]', '[]', now()-'17 days'::interval, now()-'17 days'::interval, '80000000-0000-0000-0000-000000000007', '70000000-0000-0000-0000-000000000006', 'b0000000-0000-0000-0000-000000000004', now()-'16 days'::interval, now()-'15 days'::interval, now()-'15 days'::interval+interval'7 hours', now()-'15 days'::interval+interval'10 hours', 3.2, NULL, NULL, 15),
  ('90000005-0000-0000-0000-000000000005', 'd0000000-0000-0000-0000-000000000025', '[1]',   2.0, 'Đã thu gom xong số 25',          'Collected',  '[]', '[]', now()-'16 days'::interval, now()-'16 days'::interval, '80000000-0000-0000-0000-000000000009', '70000000-0000-0000-0000-000000000008', 'b0000000-0000-0000-0000-000000000005', now()-'15 days'::interval, now()-'14 days'::interval, now()-'14 days'::interval+interval'7 hours', now()-'14 days'::interval+interval'10 hours', 1.8, NULL, NULL, 10),
-- Completed
  ('90000006-0000-0000-0000-000000000001', 'd0000000-0000-0000-0000-000000000026', '[1]',   3.0, 'Đã hoàn thành số 26',            'Completed',  '[]', '[]', now()-'25 days'::interval, now()-'25 days'::interval, '80000000-0000-0000-0000-000000000001', '70000000-0000-0000-0000-000000000001', 'b0000000-0000-0000-0000-000000000001', now()-'24 days'::interval, now()-'23 days'::interval, now()-'23 days'::interval+interval'7 hours', now()-'23 days'::interval+interval'10 hours', 2.8, now()-'23 days'::interval+interval'12 hours', NULL, 30),
  ('90000006-0000-0000-0000-000000000002', 'd0000000-0000-0000-0000-000000000027', '[1,2]', 4.5, 'Đã hoàn thành số 27',            'Completed',  '[]', '[]', now()-'24 days'::interval, now()-'24 days'::interval, '80000000-0000-0000-0000-000000000003', '70000000-0000-0000-0000-000000000003', 'b0000000-0000-0000-0000-000000000002', now()-'23 days'::interval, now()-'22 days'::interval, now()-'22 days'::interval+interval'7 hours', now()-'22 days'::interval+interval'10 hours', 4.1, now()-'22 days'::interval+interval'12 hours', NULL, 50),
  ('90000006-0000-0000-0000-000000000003', 'd0000000-0000-0000-0000-000000000028', '[2]',   2.0, 'Đã hoàn thành số 28',            'Completed',  '[]', '[]', now()-'23 days'::interval, now()-'23 days'::interval, '80000000-0000-0000-0000-000000000005', '70000000-0000-0000-0000-000000000005', 'b0000000-0000-0000-0000-000000000003', now()-'22 days'::interval, now()-'21 days'::interval, now()-'21 days'::interval+interval'7 hours', now()-'21 days'::interval+interval'10 hours', 1.9, now()-'21 days'::interval+interval'12 hours', NULL, 20),
  ('90000006-0000-0000-0000-000000000004', 'd0000000-0000-0000-0000-000000000029', '[1]',   3.5, 'Đã hoàn thành số 29',            'Completed',  '[]', '[]', now()-'22 days'::interval, now()-'22 days'::interval, '80000000-0000-0000-0000-000000000007', '70000000-0000-0000-0000-000000000007', 'b0000000-0000-0000-0000-000000000004', now()-'21 days'::interval, now()-'20 days'::interval, now()-'20 days'::interval+interval'7 hours', now()-'20 days'::interval+interval'10 hours', 3.3, now()-'20 days'::interval+interval'12 hours', NULL, 10),
  ('90000006-0000-0000-0000-000000000005', 'd0000000-0000-0000-0000-000000000030', '[1,2,3]',5.0,'Đã hoàn thành số 30',            'Completed',  '[]', '[]', now()-'21 days'::interval, now()-'21 days'::interval, '80000000-0000-0000-0000-000000000009', '70000000-0000-0000-0000-000000000009', 'b0000000-0000-0000-0000-000000000005', now()-'20 days'::interval, now()-'19 days'::interval, now()-'19 days'::interval+interval'7 hours', now()-'19 days'::interval+interval'10 hours', 4.7, now()-'19 days'::interval+interval'12 hours', NULL, 35),
-- Rejected
  ('90000007-0000-0000-0000-000000000001', 'd0000000-0000-0000-0000-000000000001', '[1]',   0.5, 'Không đủ điều kiện số 31',        'Rejected',   '[]', '[]', now()-'12 days'::interval, now()-'12 days'::interval, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Khối lượng rác quá nhỏ, không đủ điều kiện thu gom', NULL),
  ('90000007-0000-0000-0000-000000000002', 'd0000000-0000-0000-0000-000000000002', '[3]',   0.3, 'Rác nguy hiểm không xử lý số 32', 'Rejected',   '[]', '[]', now()-'11 days'::interval, now()-'11 days'::interval, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Loại rác này không thuộc phạm vi xử lý', NULL),
  ('90000007-0000-0000-0000-000000000003', 'd0000000-0000-0000-0000-000000000003', '[1,2]', 0.8, 'Địa chỉ không rõ số 33',          'Rejected',   '[]', '[]', now()-'10 days'::interval, now()-'10 days'::interval, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Địa chỉ thu gom không xác định được', NULL),
  ('90000007-0000-0000-0000-000000000004', 'd0000000-0000-0000-0000-000000000004', '[1]',   1.0, 'Trùng báo cáo số 34',             'Rejected',   '[]', '[]', now()-'9 days'::interval,  now()-'9 days'::interval,  NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Báo cáo trùng với yêu cầu đã có', NULL),
  ('90000007-0000-0000-0000-000000000005', 'd0000000-0000-0000-0000-000000000005', '[2]',   0.4, 'Hình ảnh không hợp lệ số 35',    'Rejected',   '[]', '[]', now()-'8 days'::interval,  now()-'8 days'::interval,  NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Hình ảnh đính kèm không rõ ràng', NULL),
-- Failed
  ('90000008-0000-0000-0000-000000000001', 'd0000000-0000-0000-0000-000000000006', '[1]',   2.5, 'Thu thất bại số 36',              'Failed',     '[]', '[]', now()-'7 days'::interval,  now()-'7 days'::interval,  '80000000-0000-0000-0000-000000000001', '70000000-0000-0000-0000-000000000001', 'b0000000-0000-0000-0000-000000000001', now()-'6 days'::interval, now()-'5 days'::interval, now()-'5 days'::interval+interval'7 hours', NULL, NULL, NULL, 'Không tìm thấy địa điểm thu gom', NULL),
  ('90000008-0000-0000-0000-000000000002', 'd0000000-0000-0000-0000-000000000007', '[2]',   3.0, 'Thu thất bại số 37',              'Failed',     '[]', '[]', now()-'6 days'::interval,  now()-'6 days'::interval,  '80000000-0000-0000-0000-000000000003', '70000000-0000-0000-0000-000000000003', 'b0000000-0000-0000-0000-000000000002', now()-'5 days'::interval, now()-'4 days'::interval, now()-'4 days'::interval+interval'7 hours', NULL, NULL, NULL, 'Cư dân từ chối cho vào', NULL),
  ('90000008-0000-0000-0000-000000000003', 'd0000000-0000-0000-0000-000000000008', '[1,2]', 4.0, 'Thu thất bại số 38',              'Failed',     '[]', '[]', now()-'5 days'::interval,  now()-'5 days'::interval,  '80000000-0000-0000-0000-000000000005', '70000000-0000-0000-0000-000000000005', 'b0000000-0000-0000-0000-000000000003', now()-'4 days'::interval, now()-'3 days'::interval, now()-'3 days'::interval+interval'7 hours', NULL, NULL, NULL, 'Đường bị ngập, xe không vào được', NULL),
  ('90000008-0000-0000-0000-000000000004', 'd0000000-0000-0000-0000-000000000009', '[1]',   2.0, 'Thu thất bại số 39',              'Failed',     '[]', '[]', now()-'4 days'::interval,  now()-'4 days'::interval,  '80000000-0000-0000-0000-000000000007', '70000000-0000-0000-0000-000000000007', 'b0000000-0000-0000-0000-000000000004', now()-'3 days'::interval, now()-'2 days'::interval, now()-'2 days'::interval+interval'7 hours', NULL, NULL, NULL, 'Rác đã được người khác thu trước', NULL),
  ('90000008-0000-0000-0000-000000000005', 'd0000000-0000-0000-0000-000000000010', '[3]',   1.5, 'Thu thất bại số 40',              'Failed',     '[]', '[]', now()-'3 days'::interval,  now()-'3 days'::interval,  '80000000-0000-0000-0000-000000000009', '70000000-0000-0000-0000-000000000009', 'b0000000-0000-0000-0000-000000000005', now()-'2 days'::interval, now()-'1 days'::interval, now()-'1 days'::interval+interval'7 hours', NULL, NULL, NULL, 'Hết giờ làm việc trong ngày', NULL),
-- Cancel
  ('90000009-0000-0000-0000-000000000001', 'd0000000-0000-0000-0000-000000000011', '[1]',   1.0, 'Đã hủy báo cáo số 41',           'Cancel',     '[]', '[]', now()-'2 days'::interval,  now()-'2 days'::interval,  NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('90000009-0000-0000-0000-000000000002', 'd0000000-0000-0000-0000-000000000012', '[2]',   1.5, 'Đã hủy báo cáo số 42',           'Cancel',     '[]', '[]', now()-'3 days'::interval,  now()-'3 days'::interval,  NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('90000009-0000-0000-0000-000000000003', 'd0000000-0000-0000-0000-000000000013', '[1,2]', 2.5, 'Đã hủy báo cáo số 43',           'Cancel',     '[]', '[]', now()-'4 days'::interval,  now()-'4 days'::interval,  NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('90000009-0000-0000-0000-000000000004', 'd0000000-0000-0000-0000-000000000014', '[1]',   3.0, 'Đã hủy báo cáo số 44',           'Cancel',     '[]', '[]', now()-'5 days'::interval,  now()-'5 days'::interval,  NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
  ('90000009-0000-0000-0000-000000000005', 'd0000000-0000-0000-0000-000000000015', '[3]',   0.8, 'Đã hủy báo cáo số 45',           'Cancel',     '[]', '[]', now()-'6 days'::interval,  now()-'6 days'::interval,  NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
-- Processing (thêm 5 nữa — OnTheWay đã bỏ)
  ('90000010-0000-0000-0000-000000000001', 'd0000000-0000-0000-0000-000000000016', '[1]',   2.5, 'Đang thu gom số 46',              'Processing', '[]', '[]', now()-'1 days'::interval,  now()-'1 days'::interval,  '80000000-0000-0000-0000-000000000001', '70000000-0000-0000-0000-000000000001', 'b0000000-0000-0000-0000-000000000001', now()-'1 days'::interval+interval'2 hours', now()+interval'4 hours', now()+interval'3 hours', NULL, NULL, NULL, NULL, NULL),
  ('90000010-0000-0000-0000-000000000002', 'd0000000-0000-0000-0000-000000000017', '[2]',   3.0, 'Đang thu gom số 47',              'Processing', '[]', '[]', now()-'1 days'::interval,  now()-'1 days'::interval,  '80000000-0000-0000-0000-000000000003', '70000000-0000-0000-0000-000000000003', 'b0000000-0000-0000-0000-000000000002', now()-'1 days'::interval+interval'2 hours', now()+interval'4 hours', now()+interval'3 hours', NULL, NULL, NULL, NULL, NULL),
  ('90000010-0000-0000-0000-000000000003', 'd0000000-0000-0000-0000-000000000018', '[1,2]', 4.0, 'Đang thu gom số 48',              'Processing', '[]', '[]', now(),                    now(),                    '80000000-0000-0000-0000-000000000005', '70000000-0000-0000-0000-000000000005', 'b0000000-0000-0000-0000-000000000003', now()+interval'30 minutes',           now()+interval'6 hours', now()+interval'1 hour',  NULL, NULL, NULL, NULL, NULL),
  ('90000010-0000-0000-0000-000000000004', 'd0000000-0000-0000-0000-000000000019', '[1]',   2.0, 'Đang thu gom số 49',              'Processing', '[]', '[]', now(),                    now(),                    '80000000-0000-0000-0000-000000000007', '70000000-0000-0000-0000-000000000007', 'b0000000-0000-0000-0000-000000000004', now()+interval'1 hour',               now()+interval'6 hours', now()+interval'2 hours', NULL, NULL, NULL, NULL, NULL),
  ('90000010-0000-0000-0000-000000000005', 'd0000000-0000-0000-0000-000000000020', '[3]',   1.5, 'Đang thu gom số 50',              'Processing', '[]', '[]', now(),                    now(),                    '80000000-0000-0000-0000-000000000009', '70000000-0000-0000-0000-000000000009', 'b0000000-0000-0000-0000-000000000005', now()+interval'1 hour',               now()+interval'6 hours', now()+interval'2 hours', NULL, NULL, NULL, NULL, NULL);

-- ─────────────────────────────────────────────────────────────
-- 9. COMPLAINTS  (15 rows — trên các report Completed/Rejected)
-- ─────────────────────────────────────────────────────────────

INSERT INTO complaints (id, citizen_id, report_id, reason, image_urls, status, admin_response, messages, request_at, response_at)
VALUES
  -- Trên Completed reports
  (gen_random_uuid(), 'd0000000-0000-0000-0000-000000000026', '90000006-0000-0000-0000-000000000001', 'Collector đến muộn 2 tiếng so với hẹn',           '[]', 'Approved', 'Đã ghi nhận và sẽ nhắc nhở collector.',    '[]', now()-'23 days'::interval+interval'13 hours', now()-'22 days'::interval),
  (gen_random_uuid(), 'd0000000-0000-0000-0000-000000000027', '90000006-0000-0000-0000-000000000002', 'Rác chưa được thu hết, còn sót nhiều túi',        '[]', 'Pending',  NULL,                                        '[]', now()-'22 days'::interval+interval'13 hours', NULL),
  (gen_random_uuid(), 'd0000000-0000-0000-0000-000000000028', '90000006-0000-0000-0000-000000000003', 'Collector thái độ không lịch sự',                  '[]', 'Rejected', 'Sau xác minh, collector đã làm đúng quy trình.','[]', now()-'21 days'::interval+interval'13 hours', now()-'20 days'::interval),
  (gen_random_uuid(), 'd0000000-0000-0000-0000-000000000029', '90000006-0000-0000-0000-000000000004', 'Điểm tích lũy không được cộng sau khi thu gom',   '[]', 'Approved', 'Đã kiểm tra và cộng thêm 10 điểm còn thiếu.','[]', now()-'20 days'::interval+interval'13 hours', now()-'19 days'::interval),
  (gen_random_uuid(), 'd0000000-0000-0000-0000-000000000030', '90000006-0000-0000-0000-000000000005', 'Ảnh xác nhận thu gom không đúng địa điểm',        '[]', 'Pending',  NULL,                                        '[]', now()-'19 days'::interval+interval'13 hours', NULL),
  -- Trên Rejected reports
  (gen_random_uuid(), 'd0000000-0000-0000-0000-000000000001', '90000007-0000-0000-0000-000000000001', 'Từ chối không có lý do chính đáng',               '[]', 'Approved', 'Báo cáo đã được duyệt lại và chuyển sang Queue.','[]', now()-'11 days'::interval, now()-'10 days'::interval),
  (gen_random_uuid(), 'd0000000-0000-0000-0000-000000000002', '90000007-0000-0000-0000-000000000002', 'Loại rác trong danh mục cho phép',                 '[]', 'Rejected', 'Rác điện tử cần xử lý riêng theo quy định.',  '[]', now()-'10 days'::interval, now()-'9 days'::interval),
  (gen_random_uuid(), 'd0000000-0000-0000-0000-000000000003', '90000007-0000-0000-0000-000000000003', 'Địa chỉ rõ ràng, có ảnh đính kèm',                '[]', 'Pending',  NULL,                                        '[]', now()-'9 days'::interval,  NULL),
  (gen_random_uuid(), 'd0000000-0000-0000-0000-000000000004', '90000007-0000-0000-0000-000000000004', 'Đây là báo cáo đầu tiên, không trùng lặp',        '[]', 'Approved', 'Xác nhận không trùng, đã chuyển lại Pending.', '[]', now()-'8 days'::interval,  now()-'7 days'::interval),
  (gen_random_uuid(), 'd0000000-0000-0000-0000-000000000005', '90000007-0000-0000-0000-000000000005', 'Hình ảnh rõ ràng, yêu cầu xem xét lại',           '[]', 'Rejected', 'Hình ảnh không đủ góc nhìn theo yêu cầu.',   '[]', now()-'7 days'::interval,  now()-'6 days'::interval),
  -- Trên Collected + Completed thêm 5 nữa
  (gen_random_uuid(), 'd0000000-0000-0000-0000-000000000021', '90000005-0000-0000-0000-000000000001', 'Khối lượng thực tế ghi sai',                       '[]', 'Pending',  NULL,                                        '[]', now()-'17 days'::interval, NULL),
  (gen_random_uuid(), 'd0000000-0000-0000-0000-000000000022', '90000005-0000-0000-0000-000000000002', 'Điểm không được tính đúng theo loại rác',          '[]', 'Approved', 'Đã kiểm tra, điểm đã được cập nhật chính xác.','[]', now()-'16 days'::interval, now()-'15 days'::interval),
  (gen_random_uuid(), 'd0000000-0000-0000-0000-000000000023', '90000005-0000-0000-0000-000000000003', 'Collector không để lại biên lai',                  '[]', 'Rejected', 'Biên lai điện tử đã gửi qua email.',          '[]', now()-'15 days'::interval, now()-'14 days'::interval),
  (gen_random_uuid(), 'd0000000-0000-0000-0000-000000000024', '90000005-0000-0000-0000-000000000004', 'Yêu cầu thu thêm rác còn sót',                    '[]', 'Pending',  NULL,                                        '[]', now()-'14 days'::interval, NULL),
  (gen_random_uuid(), 'd0000000-0000-0000-0000-000000000025', '90000005-0000-0000-0000-000000000005', 'Thông báo thu gom đến quá muộn',                   '[]', 'Approved', 'Đã điều chỉnh lịch thông báo trước 2 tiếng.', '[]', now()-'13 days'::interval, now()-'12 days'::interval);

-- ─────────────────────────────────────────────────────────────
-- 10. USER POINTS  (34 rows — một row mỗi citizen)
-- ─────────────────────────────────────────────────────────────

INSERT INTO user_points (user_id, week_points, month_points, year_points, total_points, leaderboard_opt_out, work_area_name, updated_at)
VALUES
  ('d0000000-0000-0000-0000-000000000001', 45,  180,  1200,  5400,  false, 'Phường Bến Nghé',         now()-'2 days'::interval),
  ('d0000000-0000-0000-0000-000000000002', 30,  150,  900,   4200,  false, 'Phường Cầu Kho',          now()-'3 days'::interval),
  ('d0000000-0000-0000-0000-000000000003', 60,  240,  1600,  7800,  false, 'Phường Đa Kao',           now()-'1 days'::interval),
  ('d0000000-0000-0000-0000-000000000004', 20,  80,   600,   2100,  false, 'Phường Nguyễn Thái Bình', now()-'4 days'::interval),
  ('d0000000-0000-0000-0000-000000000005', 75,  300,  2000,  9500,  false, 'Phường 1',                now()          ),
  ('d0000000-0000-0000-0000-000000000006', 50,  200,  1400,  6300,  false, 'Phường 2',                now()-'1 days'::interval),
  ('d0000000-0000-0000-0000-000000000007', 35,  140,  980,   4900,  false, 'Phường 3',                now()-'2 days'::interval),
  ('d0000000-0000-0000-0000-000000000008', 10,  40,   280,   1400,  false, 'Phường 4',                now()-'5 days'::interval),
  ('d0000000-0000-0000-0000-000000000009', 90,  360,  2400,  12000, false, 'Phường 1',                now()          ),
  ('d0000000-0000-0000-0000-000000000010', 25,  100,  700,   3500,  false, 'Phường 2',                now()-'3 days'::interval),
  ('d0000000-0000-0000-0000-000000000011', 55,  220,  1500,  7000,  false, 'Phường 3',                now()-'1 days'::interval),
  ('d0000000-0000-0000-0000-000000000012', 40,  160,  1100,  5500,  false, 'Phường 4',                now()-'2 days'::interval),
  ('d0000000-0000-0000-0000-000000000013', 80,  320,  2200,  10800, false, 'Phường 1',                now()          ),
  ('d0000000-0000-0000-0000-000000000014', 15,  60,   420,   2100,  false, 'Phường 2',                now()-'4 days'::interval),
  ('d0000000-0000-0000-0000-000000000015', 65,  260,  1800,  8700,  false, 'Phường 3',                now()-'1 days'::interval),
  ('d0000000-0000-0000-0000-000000000016', 48,  192,  1344,  6720,  false, 'Phường 4',                now()-'2 days'::interval),
  ('d0000000-0000-0000-0000-000000000017', 72,  288,  2016,  10080, false, 'Phường 1',                now()          ),
  ('d0000000-0000-0000-0000-000000000018', 36,  144,  1008,  5040,  false, 'Phường 2',                now()-'3 days'::interval),
  ('d0000000-0000-0000-0000-000000000019', 84,  336,  2352,  11760, false, 'Phường 3',                now()          ),
  ('d0000000-0000-0000-0000-000000000020', 24,  96,   672,   3360,  false, 'Phường 4',                now()-'4 days'::interval),
  ('d0000000-0000-0000-0000-000000000021', 96,  384,  2688,  13440, false, 'Phường Bến Nghé',         now()          ),
  ('d0000000-0000-0000-0000-000000000022', 12,  48,   336,   1680,  false, 'Phường Cầu Kho',          now()-'5 days'::interval),
  ('d0000000-0000-0000-0000-000000000023', 68,  272,  1904,  9520,  false, 'Phường Đa Kao',           now()-'1 days'::interval),
  ('d0000000-0000-0000-0000-000000000024', 52,  208,  1456,  7280,  false, 'Phường 1',                now()-'2 days'::interval),
  ('d0000000-0000-0000-0000-000000000025', 88,  352,  2464,  12320, false, 'Phường 2',                now()          ),
  ('d0000000-0000-0000-0000-000000000026', 16,  64,   448,   2240,  false, 'Phường 3',                now()-'6 days'::interval),
  ('d0000000-0000-0000-0000-000000000027', 76,  304,  2128,  10640, false, 'Phường 1',                now()          ),
  ('d0000000-0000-0000-0000-000000000028', 44,  176,  1232,  6160,  false, 'Phường 2',                now()-'1 days'::interval),
  ('d0000000-0000-0000-0000-000000000029', 92,  368,  2576,  12880, false, 'Phường 3',                now()          ),
  ('d0000000-0000-0000-0000-000000000030', 28,  112,  784,   3920,  false, 'Phường 1',                now()-'3 days'::interval),
  ('d0000000-0000-0000-0000-000000000031', 64,  256,  1792,  8960,  false, 'Phường 2',                now()-'1 days'::interval),
  ('d0000000-0000-0000-0000-000000000032', 56,  224,  1568,  7840,  false, 'Phường 3',                now()-'2 days'::interval),
  ('d0000000-0000-0000-0000-000000000033', 100, 400,  2800,  14000, false, 'Phường 1',                now()          ),
  ('d0000000-0000-0000-0000-000000000034', 8,   32,   224,   1120,  true,  'Phường 2',                now()-'7 days'::interval);

-- ─────────────────────────────────────────────────────────────
-- 11. TEAM SESSIONS  (15 rows — lịch sử các ca làm việc)
-- ─────────────────────────────────────────────────────────────

INSERT INTO team_sessions (id, team_id, date, start_at, end_at, total_reports, total_capacity, created_at)
VALUES
  (gen_random_uuid(), '70000000-0000-0000-0000-000000000001', (now()-'1 days'::interval)::date,  now()-'1 days'::interval+'7 hours'::interval, now()-'1 days'::interval+'15 hours'::interval, 8,  15.5, now()-'1 days'::interval+'7 hours'::interval),
  (gen_random_uuid(), '70000000-0000-0000-0000-000000000002', (now()-'1 days'::interval)::date,  now()-'1 days'::interval+'7 hours'::interval, now()-'1 days'::interval+'16 hours'::interval, 6,  12.0, now()-'1 days'::interval+'7 hours'::interval),
  (gen_random_uuid(), '70000000-0000-0000-0000-000000000003', (now()-'2 days'::interval)::date,  now()-'2 days'::interval+'7 hours'::interval, now()-'2 days'::interval+'14 hours'::interval, 10, 20.3, now()-'2 days'::interval+'7 hours'::interval),
  (gen_random_uuid(), '70000000-0000-0000-0000-000000000004', (now()-'2 days'::interval)::date,  now()-'2 days'::interval+'7 hours'::interval, now()-'2 days'::interval+'15 hours'::interval, 7,  14.8, now()-'2 days'::interval+'7 hours'::interval),
  (gen_random_uuid(), '70000000-0000-0000-0000-000000000005', (now()-'3 days'::interval)::date,  now()-'3 days'::interval+'7 hours'::interval, now()-'3 days'::interval+'15 hours'::interval, 9,  18.2, now()-'3 days'::interval+'7 hours'::interval),
  (gen_random_uuid(), '70000000-0000-0000-0000-000000000006', (now()-'3 days'::interval)::date,  now()-'3 days'::interval+'7 hours'::interval, now()-'3 days'::interval+'16 hours'::interval, 5,  10.5, now()-'3 days'::interval+'7 hours'::interval),
  (gen_random_uuid(), '70000000-0000-0000-0000-000000000007', (now()-'4 days'::interval)::date,  now()-'4 days'::interval+'7 hours'::interval, now()-'4 days'::interval+'14 hours'::interval, 11, 22.7, now()-'4 days'::interval+'7 hours'::interval),
  (gen_random_uuid(), '70000000-0000-0000-0000-000000000008', (now()-'4 days'::interval)::date,  now()-'4 days'::interval+'7 hours'::interval, now()-'4 days'::interval+'15 hours'::interval, 8,  16.4, now()-'4 days'::interval+'7 hours'::interval),
  (gen_random_uuid(), '70000000-0000-0000-0000-000000000009', (now()-'5 days'::interval)::date,  now()-'5 days'::interval+'7 hours'::interval, now()-'5 days'::interval+'13 hours'::interval, 4,  8.1,  now()-'5 days'::interval+'7 hours'::interval),
  (gen_random_uuid(), '70000000-0000-0000-0000-000000000010', (now()-'5 days'::interval)::date,  now()-'5 days'::interval+'7 hours'::interval, now()-'5 days'::interval+'16 hours'::interval, 12, 24.6, now()-'5 days'::interval+'7 hours'::interval),
  (gen_random_uuid(), '70000000-0000-0000-0000-000000000001', (now()-'6 days'::interval)::date,  now()-'6 days'::interval+'7 hours'::interval, now()-'6 days'::interval+'14 hours'::interval, 7,  14.0, now()-'6 days'::interval+'7 hours'::interval),
  (gen_random_uuid(), '70000000-0000-0000-0000-000000000003', (now()-'7 days'::interval)::date,  now()-'7 days'::interval+'7 hours'::interval, now()-'7 days'::interval+'15 hours'::interval, 9,  18.9, now()-'7 days'::interval+'7 hours'::interval),
  (gen_random_uuid(), '70000000-0000-0000-0000-000000000005', (now()-'8 days'::interval)::date,  now()-'8 days'::interval+'7 hours'::interval, now()-'8 days'::interval+'14 hours'::interval, 6,  12.3, now()-'8 days'::interval+'7 hours'::interval),
  (gen_random_uuid(), '70000000-0000-0000-0000-000000000007', (now()-'9 days'::interval)::date,  now()-'9 days'::interval+'7 hours'::interval, now()-'9 days'::interval+'16 hours'::interval, 10, 21.0, now()-'9 days'::interval+'7 hours'::interval),
  (gen_random_uuid(), '70000000-0000-0000-0000-000000000009', (now()-'10 days'::interval)::date, now()-'10 days'::interval+'7 hours'::interval,now()-'10 days'::interval+'15 hours'::interval, 8,  17.5, now()-'10 days'::interval+'7 hours'::interval);

-- ─────────────────────────────────────────────────────────────
-- XONG  — Kiểm tra nhanh số lượng row
-- ─────────────────────────────────────────────────────────────
SELECT 'work_areas'      AS "table", COUNT(*) AS rows FROM work_areas
UNION ALL SELECT 'users',            COUNT(*) FROM users
UNION ALL SELECT 'enterprise_hub',   COUNT(*) FROM enterprise_hub
UNION ALL SELECT 'collector_hub',    COUNT(*) FROM collector_hub
UNION ALL SELECT 'teams',            COUNT(*) FROM teams
UNION ALL SELECT 'staffs',           COUNT(*) FROM staffs
UNION ALL SELECT 'point_categories', COUNT(*) FROM point_categories
UNION ALL SELECT 'citizen_reports',  COUNT(*) FROM citizen_reports
UNION ALL SELECT 'complaints',       COUNT(*) FROM complaints
UNION ALL SELECT 'user_points',      COUNT(*) FROM user_points
UNION ALL SELECT 'team_sessions',    COUNT(*) FROM team_sessions
ORDER BY "table";

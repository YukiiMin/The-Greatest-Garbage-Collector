# 05 — Authentication & Authorization

## Tổng quan

- **Access token:** JWT, HttpOnly cookie `accessToken`, TTL 15 phút
- **Refresh token:** JWT bọc raw token, HttpOnly cookie `refreshToken`, TTL 7 ngày
- **Provider:** Local (email/password) hoặc Google OAuth
- **Session:** Single-session (đăng nhập mới revoke session cũ)

---

## 1. Đăng ký Local (`POST /auth/local-auth/account-registration`)

```
1. Validate: email format + password strength (via FluentValidation)
2. Normalize email: lowercase + trim
3. Check duplicate: query DB by email → 409 ACCOUNT_EXISTS nếu đã tồn tại
4. Create User:
   - provider = "local"
   - passwordHash = BCrypt.HashPassword(password)
   - emailVerified = false
   - role = Citizen
   - loginTerm = 0
5. Generate 6-digit OTP → BCrypt hash → persist to email_otps
6. Send email OTP (fire-and-forget — không block nếu mail fail)
7. Generate JWT pair → revoke old refresh tokens → persist new hash
8. Set cookies (accessToken + refreshToken)
9. Return: email, hasPassword, fullName, ...
```

---

## 2. Xác minh Email (`POST /auth/local-auth/account-verification`)

```
Request: { data: { email, otp } }

1. Query email_otps by email
2. Check: not IsUsed, not expired (ExpiresAt > UtcNow)
3. BCrypt.Verify(otp, stored_hash)
4. Set user.EmailVerified = true
5. Mark OTP as used
6. Return 200
```

---

## 3. Đăng nhập Local (`POST /auth/local-auth/login`)

```
Request: { data: { email, password } }

1. Validate: email not empty, password not empty
2. Normalize email
3. Query User by email
4. BCrypt.Verify(password, user.PasswordHash)
   → Nếu user not found HOẶC password sai → 409 INVALID_CREDENTIALS
   (cùng error để chống user enumeration)
5. Check user.EmailVerified == true → 409 EMAIL_NOT_VERIFIED nếu chưa xác minh
6. Check user.IsBanned → 403 USER_BANNED
7. Generate access token (JWT, claims: email, full_name, login_term, jti)
8. Generate refresh token (64 bytes base64 → JWT wrapper → SHA-256 hash)
9. RevokeAllForUserAsync (single-session enforcement)
10. Persist refresh token hash to DB
11. Set HttpOnly cookies: accessToken + refreshToken
12. Return: email, fullName, hasPassword, avatarUrl, address, role
```

---

## 4. Đăng nhập Google (`POST /auth/google-auth/account`)

```
Request: { data: { google_token: "<Google ID token>" } }

1. GoogleJsonWebSignature.ValidateAsync(token)
   → 404 GOOGLE_INVALID nếu token không hợp lệ
2. Lookup User: GetByGoogleIdAsync(subject) || GetByEmailAsync(email)
3a. User not found → CREATE new User (emailVerified=true, provider="google")
3b. User found + IsBanned → 403 USER_BANNED
3c. User found + GoogleId == null → patch GoogleId (link Google to local account)
4. Generate tokens → revoke old → persist
5. Set cookies
6. Return user profile
```

---

## 5. JWT Token Details

### Access Token Claims
```json
{
  "email": "user@example.com",
  "full_name": "Nguyen Van A",
  "login_term": "0",
  "jti": "550e8400-e29b-41d4-a716-446655440000"
}
```

### Refresh Token Claims
```json
{
  "refresh_token": "<64-byte base64 string>",
  "email": "user@example.com",
  "jti": "550e8400-e29b-41d4-a716-446655440001"
}
```

**Lưu ý:** DB chỉ lưu `SHA-256(raw_refresh_token)` dạng hex lowercase — không bao giờ lưu raw token hoặc JWT wrapper.

---

## 6. Cookie Configuration

| Cookie | Name | HttpOnly | Secure | SameSite | Path | TTL |
|--------|------|----------|--------|----------|------|-----|
| Access | `accessToken` | ✓ | ✓ | Strict | `/` | 15 phút |
| Refresh | `refreshToken` | ✓ | ✓ | Strict | `/` | 7 ngày |

---

## 7. Refresh Token Rotation (`GET /auth/account-auth/license`)

```
1. Đọc cookie "refreshToken"
   → 401 NO_REFRESH_TOKEN nếu không có

2. Validate JWT structure (ValidateLifetime = false)
   → 401 INVALID_REFRESH_TOKEN nếu JWT malformed

3. Extract claim "refresh_token" (raw base64) từ JWT payload
4. SHA-256(raw) → lookup DB by token_hash
   → 401 INVALID_REFRESH_TOKEN nếu không tìm thấy

5. Check IsRevoked == true
   → 409 TOKEN_REUSE
   → RevokeAllForUserAsync (toàn bộ session)
   → IncrementLoginTermAsync (invalidate tất cả access tokens)

6. Check DB ExpiresAt > UtcNow
   → 422 TOKEN_EXPIRED nếu hết hạn

7. Generate new token pair
8. RevokeByIdAsync(oldToken.Id)  ← revoke token vừa dùng
9. Persist new token hash
10. Set new cookies
11. Return: email, fullName, avatarUrl, address
```

---

## 8. Reuse Detection — Cơ chế bảo mật

**Tình huống:** Attacker đánh cắp refresh token và dùng trước client thật.

```
Timeline:
  T1: Client có token A (valid)
  T2: Attacker dùng token A → server rotate → tạo token B, revoke token A
  T3: Client dùng token A → IsRevoked = true → REUSE DETECTED
      → Server: revoke ALL tokens của user (kể cả token B)
      → Server: increment user.login_term
      → Response: 409 TOKEN_REUSE

Effect:
  - Toàn bộ session của user bị terminate
  - Mọi access token cũ (claim login_term < DB login_term) fail validation
  - User phải đăng nhập lại
```

---

## 9. Login Term Invalidation

`login_term` là một integer trên `User` và được nhúng vào JWT access token claim.

**Cơ chế:**
1. Khi reuse detected → `user.login_term += 1` (DB)
2. JWT access token cũ vẫn chưa hết hạn về mặt time, nhưng claim `login_term` cũ < DB `login_term`
3. `AccountVerificationService` check: `tokenLoginTerm == user.LoginTerm` → 403 nếu sai

Kết quả: toàn bộ access token cũ trở thành invalid ngay lập tức, không cần chờ hết TTL.

---

## 10. Password Reset

```
Step 1 — Tạo OTP:
POST /auth/local-auth/password-otp
{ data: { email } }
→ Generate OTP → email user → persist hash to password_otp table

Step 2 — Reset:
POST /auth/reset-password
{ data: { email, otp, password } }
1. Validate: email format + password strength
2. Lookup user by email
3. Lookup OTP record
4. Check not used, not expired
5. BCrypt.Verify(otp, hash)
6. BCrypt hash new password → save
7. Mark OTP used
8. Return 200
```

---

## 11. Enterprise Authentication

Enterprise không có FK trực tiếp đến `users`. Mechanism:

```
1. Admin tạo Enterprise: { name, email=X, phoneNumber, ... }
2. Admin setup enterprise user (user với email=X):
   POST /admin/setup/enterprise { user_id, name, email=X, ... }
   → Tạo Enterprise entity với Email = User.Email
   → Tạo Staff record { UserId=X, EnterpriseId=new }
   → Set User.Role = Enterprise

3. Khi Enterprise user gọi API:
   → JWT claim chứa email
   → EnterpriseService query: Enterprise WHERE Email = claimsEmail
   → Nếu không tìm thấy enterprise cho email đó → 403
```

---

## 12. Authorization

Controller dùng `[Authorize]` attribute. Một số endpoint dùng `[Authorize(Roles = "Admin")]` hoặc tương tự.

Mặc định: `[Authorize]` chỉ yêu cầu JWT hợp lệ + login_term hợp lệ.

Các endpoint không cần auth: đăng ký, đăng nhập, verify email, resend OTP, password OTP, reset password.

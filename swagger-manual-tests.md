# MyEStore Manual Swagger Test Guide

This guide mirrors the automated checks in run_tests.sh and lets you run them manually in Swagger.

## 1) Open Swagger

1. Start API.
2. Open: `http://localhost:5000/swagger`
3. Keep this page open.

## 2) Test Data To Reuse

Use a unique email on each run to avoid duplicate registration errors.

- Test Email: `testrun_20260421_01@myestore.com`
- Test Password: `Test@1234A`
- Wrong Password: `WrongPassword!!`

If you rerun later, change the email.

## 3) Capture JWT Token (for protected APIs)

After successful login (T2), copy the `token` value from response JSON.

In Swagger:

1. Click `Authorize` (top-right).
2. Paste token as: `Bearer <your_token>`
3. Click `Authorize` and then `Close`.

## 4) Manual Test Cases (T1-T15)

### T1 Register new user -> expected 201

- Endpoint: `POST /api/auth/register`
- Body:

```json
{
  "firstName": "Test",
  "lastName": "Runner",
  "email": "testrun_20260421_01@myestore.com",
  "password": "Test@1234A",
  "confirmPassword": "Test@1234A",
  "phoneNumber": "9111111111"
}
```

### T2 Login valid -> expected 200

- Endpoint: `POST /api/auth/login`
- Body:

```json
{
  "email": "testrun_20260421_01@myestore.com",
  "password": "Test@1234A"
}
```

- Save `token` from response for authorized calls.

### T3 Profile authenticated -> expected 200

- Endpoint: `GET /api/users/me`
- Pre-step: set Bearer token via `Authorize`.

### T4 Profile no token -> expected 401

- Endpoint: `GET /api/users/me`
- Pre-step: click `Authorize` and clear token first.

### T5 Products public list -> expected 200

- Endpoint: `GET /api/products`

### T6 Create product non-admin -> expected 403

- Endpoint: `POST /api/products`
- Pre-step: use normal customer token (not admin).
- Body:

```json
{
  "name": "X",
  "brand": "Y",
  "sku": "Z-001",
  "price": 100,
  "stockQuantity": 1,
  "categoryId": 1
}
```

### T7 Wrong password login -> expected 401

- Endpoint: `POST /api/auth/login`
- Body:

```json
{
  "email": "testrun_20260421_01@myestore.com",
  "password": "WrongPassword!!"
}
```

### T8 Duplicate email register -> expected 400

- Endpoint: `POST /api/auth/register`
- Body (same email as T1):

```json
{
  "firstName": "T",
  "lastName": "U",
  "email": "testrun_20260421_01@myestore.com",
  "password": "Test@1234A",
  "confirmPassword": "Test@1234A",
  "phoneNumber": "9876543210"
}
```

### T9 Get cart authenticated -> expected 200

- Endpoint: `GET /api/cart`
- Pre-step: valid Bearer token set.

### T10 Add cart item with invalid qty 0 -> expected 400

- Endpoint: `POST /api/cart`
- Pre-step: valid Bearer token set.
- Body:

```json
{
  "productId": 1,
  "quantity": 0
}
```

### T11 OTP send -> expected 200

- Endpoint: `POST /api/auth/send-otp`
- Query param: `email=testrun_20260421_01@myestore.com`

### T12 Search products -> expected 200

- Endpoint: `GET /api/products/search`
- Query param: `query=phone`

### T13 Products by category -> expected 200

- Endpoint: `GET /api/products/category/{categoryName}`
- Path value: `Electronics`

### T14 My orders authenticated -> expected 200

- Endpoint: `GET /api/orders/my`
- Pre-step: valid Bearer token set.

### T15 All orders with non-admin token -> expected 403

- Endpoint: `GET /api/orders`
- Pre-step: use customer token (not admin).

## 5) Quick Pass/Fail Checklist

Mark each after execution:

- [ ] T1 = 201
- [ ] T2 = 200
- [ ] T3 = 200
- [ ] T4 = 401
- [ ] T5 = 200
- [ ] T6 = 403
- [ ] T7 = 401
- [ ] T8 = 400
- [ ] T9 = 200
- [ ] T10 = 400
- [ ] T11 = 200
- [ ] T12 = 200
- [ ] T13 = 200
- [ ] T14 = 200
- [ ] T15 = 403

## 6) Common Issues

- `T1` fails with 400: email already used. Change to a new email.
- Protected endpoint returns 401: token missing/expired in `Authorize`.
- `T6` or `T15` not 403: you likely used admin token.
- `T11` fails: check SMTP configuration and app logs.

## 7) New Feature Tests (Email Verification, Refresh Token, Wishlist, Coupons)

Use these after T1/T2. Keep one customer token and, if available, one admin token.

### A) Email Verification Flow

#### A1 Verify email using token link -> expected 200

- Endpoint: `GET /api/auth/verify-email`
- Query param: `token=<token_from_email_link>`
- Expected response message: `Email verified successfully. You can now log in.`

#### A2 Resend verification email -> expected 200

- Endpoint: `POST /api/auth/resend-verification`
- Query param: `email=testrun_20260421_01@myestore.com`

Notes:

- If account already verified, expect 400 with message like `Email is already verified.`
- If no user exists, expect 404.

### B) Refresh Token / Logout Flow

#### B1 Login and copy both `token` and `refreshToken` -> expected 200

- Endpoint: `POST /api/auth/login`
- Body:

```json
{
  "email": "testrun_20260421_01@myestore.com",
  "password": "Test@1234A"
}
```

#### B2 Refresh access token -> expected 200

- Endpoint: `POST /api/auth/refresh`
- Body:

```json
{
  "refreshToken": "PASTE_REFRESH_TOKEN_FROM_LOGIN"
}
```

Expected:

- New `token`
- New `refreshToken` (rotation)

#### B3 Old refresh token should fail after rotation -> expected 401

- Re-run B2 using the old refresh token from B1.

#### B4 Logout using current refresh token -> expected 200

- Endpoint: `POST /api/auth/logout`
- Body:

```json
{
  "refreshToken": "LATEST_REFRESH_TOKEN"
}
```

#### B5 Refresh after logout should fail -> expected 401

- Re-run `POST /api/auth/refresh` with the logged-out token.

### C) Wishlist Flow

Requires customer Bearer token in `Authorize`.

#### C1 Add product to wishlist -> expected 201

- Endpoint: `POST /api/wishlist/{productId}`
- Example path value: `1`

#### C2 Check product exists in wishlist -> expected 200

- Endpoint: `GET /api/wishlist/{productId}/check`
- Example path value: `1`
- Expected JSON: `inWishlist: true`

#### C3 List wishlist -> expected 200

- Endpoint: `GET /api/wishlist`
- Expected: array with item(s), product details, and `addedAt`.

#### C4 Remove product from wishlist -> expected 204

- Endpoint: `DELETE /api/wishlist/{productId}`
- Example path value: `1`

#### C5 Check removed product -> expected 200

- Endpoint: `GET /api/wishlist/{productId}/check`
- Expected JSON: `inWishlist: false`

### D) Coupon Flow

#### D1 Create coupon (admin token required) -> expected 201

- Endpoint: `POST /api/coupons`
- Body:

```json
{
  "code": "NEWUSER10",
  "description": "10% off for new users",
  "discountType": "Percentage",
  "discountValue": 10,
  "minOrderAmount": 300,
  "maxDiscountAmount": 200,
  "maxUses": 100,
  "maxUsesPerUser": 1,
  "expiresAt": "2026-12-31T23:59:59Z"
}
```

#### D2 Validate coupon (customer token) -> expected 200

- Endpoint: `POST /api/coupons/validate`
- Query params:
  - `code=NEWUSER10`
  - `orderSubtotal=1200`

Expected JSON:

- `isValid: true`
- `discountAmount` > 0

#### D3 Validate with low subtotal -> expected 200 with invalid result

- Endpoint: `POST /api/coupons/validate`
- Query params:
  - `code=NEWUSER10`
  - `orderSubtotal=100`

Expected JSON:

- `isValid: false`
- Message indicates minimum order amount not met.

#### D4 Deactivate coupon (admin token) -> expected 204

- Endpoint: `DELETE /api/coupons/{id}`
- Use the `id` returned by D1.

#### D5 Validate deactivated coupon -> expected 200 with invalid result

- Endpoint: `POST /api/coupons/validate`
- Query params:
  - `code=NEWUSER10`
  - `orderSubtotal=1200`

Expected JSON:

- `isValid: false`
- Message indicates invalid or inactive coupon.

## 8) Suggested Execution Order for New Features

1. Run A2 (resend verification), then A1 (verify).
2. Run B1, B2, B3, B4, B5.
3. Run C1, C2, C3, C4, C5.
4. Run D1 (admin), D2, D3, D4 (admin), D5.

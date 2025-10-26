# 🔐 Hướng dẫn Phân quyền JWT Authorization

## ✅ Đã cài đặt

### 1. **JWT Authentication**
- ✅ Package: `Microsoft.AspNetCore.Authentication.JwtBearer 9.0.10`
- ✅ JWT Settings trong `appsettings.json`
- ✅ Middleware Authentication & Authorization trong `Program.cs`

### 2. **Authorization Policies**
```csharp
- AdminOnly: Chỉ admin
- CustomerOnly: Chỉ customer  
- ShipperOnly: Chỉ shipper
- AdminOrShipper: Admin hoặc shipper
```

### 3. **Roles**
- **admin** - Quản trị viên (Toàn quyền)
- **customer** - Khách hàng (Tạo đơn, xem đơn của mình)
- **shipper** - Nhân viên giao hàng (Cập nhật trạng thái, check-in)

## 🎯 Phân quyền theo Endpoint

### **Orders Controller** (`/api/Orders`)

| Method | Endpoint | Roles | Mô tả |
|--------|----------|-------|-------|
| GET | `/api/Orders` | All authenticated | Xem tất cả đơn hàng |
| GET | `/api/Orders/{id}` | All authenticated | Xem chi tiết đơn hàng |
| POST | `/api/Orders` | **admin, customer** | Tạo đơn hàng mới |
| PATCH | `/api/Orders/{id}/status` | **admin, shipper** | Cập nhật trạng thái |
| PATCH | `/api/Orders/{id}/assign-staff/{staffId}` | **admin** | Gán nhân viên |
| GET | `/api/Orders/status/{status}` | All authenticated | Lọc theo trạng thái |
| GET | `/api/Orders/staff/{staffId}` | All authenticated | Đơn hàng của nhân viên |
| DELETE | `/api/Orders/{id}` | **admin** | Xóa đơn hàng |

### **Delivery Staff Controller** (`/api/DeliveryStaff`)

| Method | Endpoint | Roles | Mô tả |
|--------|----------|-------|-------|
| GET | `/api/DeliveryStaff` | All authenticated | Danh sách nhân viên |
| GET | `/api/DeliveryStaff/{id}` | All authenticated | Chi tiết nhân viên |
| GET | `/api/DeliveryStaff/available` | All authenticated | Nhân viên rảnh |
| POST | `/api/DeliveryStaff` | **admin** | Thêm nhân viên |
| PUT | `/api/DeliveryStaff/{id}` | **admin** | Cập nhật nhân viên |
| PATCH | `/api/DeliveryStaff/{id}/availability` | **admin, shipper** | Cập nhật trạng thái |
| DELETE | `/api/DeliveryStaff/{id}` | **admin** | Xóa nhân viên |

### **Tracking Controller** (`/api/Tracking`)

| Method | Endpoint | Roles | Mô tả |
|--------|----------|-------|-------|
| GET | `/api/Tracking/order/{orderId}` | All authenticated | Lịch sử check-in |
| POST | `/api/Tracking/checkin` | **admin, shipper** | Check-in vị trí |
| GET | `/api/Tracking/track/{orderCode}` | **Public** | Tracking công khai |
| GET | `/api/Tracking/location/{orderId}` | All authenticated | Vị trí hiện tại |

### **Auth Controller** (`/api/Auth`)

| Method | Endpoint | Roles | Mô tả |
|--------|----------|-------|-------|
| POST | `/api/Auth/login` | **Public** | Đăng nhập |
| POST | `/api/Auth/register` | **Public** | Đăng ký |
| POST | `/api/Auth/forgot-password` | **Public** | Quên mật khẩu |
| POST | `/api/Auth/reset-password` | **Public** | Reset mật khẩu |

## 📝 Cách sử dụng

### 1. **Đăng nhập để lấy JWT Token**

```bash
POST http://localhost:5221/api/Auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "admin123"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "userId": 1,
    "username": "admin",
    "fullName": "Quản trị viên",
    "email": "admin@delivery.com",
    "role": "admin"
  }
}
```

### 2. **Sử dụng Token trong Request**

Thêm header `Authorization`:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Ví dụ với cURL:**
```bash
curl -X GET "http://localhost:5221/api/Orders" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

**Ví dụ với JavaScript:**
```javascript
fetch('http://localhost:5221/api/Orders', {
  method: 'GET',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
})
.then(response => response.json())
.then(data => console.log(data));
```

### 3. **Test trong Swagger**

1. Mở http://localhost:5221
2. Click nút **"Authorize"** ở góc trên bên phải
3. Nhập: `Bearer YOUR_TOKEN_HERE`
4. Click **"Authorize"**
5. Giờ có thể test các endpoints yêu cầu authorization

## 🧪 Test Accounts

### Admin Account
```json
{
  "username": "admin",
  "password": "admin123",
  "role": "admin"
}
```
**Quyền:** Toàn quyền - Tạo/sửa/xóa mọi thứ

### Customer Account  
```json
{
  "username": "customer1",
  "password": "customer123",
  "role": "customer"
}
```
**Quyền:** Tạo đơn hàng, xem đơn hàng

### Shipper Account
```json
{
  "username": "shipper1",
  "password": "shipper123",
  "role": "shipper"
}
```
**Quyền:** Cập nhật trạng thái đơn, check-in GPS

## ⚠️ Error Responses

### 401 Unauthorized
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401
}
```
**Nguyên nhân:** 
- Chưa đăng nhập
- Token không hợp lệ
- Token hết hạn (60 phút)

### 403 Forbidden
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403
}
```
**Nguyên nhân:**
- Không có quyền truy cập endpoint này
- Role không phù hợp

## 🔧 JWT Token Structure

**Header:**
```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

**Payload:**
```json
{
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier": "1",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": "admin",
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": "admin",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress": "admin@delivery.com",
  "FullName": "Quản trị viên",
  "exp": 1729434000,
  "iss": "DeliveryManagementAPI",
  "aud": "DeliveryManagementClients"
}
```

**Signature:**
- Algorithm: HS256
- Secret: Trong `appsettings.json`

## 📊 Token Settings

```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyForDeliveryManagementSystem2025!@#$%",
    "Issuer": "DeliveryManagementAPI",
    "Audience": "DeliveryManagementClients",
    "ExpiryMinutes": 60
  }
}
```

## 🎨 Frontend Integration

### Lưu token sau khi login
```javascript
// login.js
const response = await fetch('/api/Auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ username, password })
});

const data = await response.json();
if (data.token) {
  localStorage.setItem('authToken', data.token);
  localStorage.setItem('user', JSON.stringify(data.user));
}
```

### Gửi token với mọi request
```javascript
// api-service.js
async function apiRequest(endpoint, options = {}) {
  const token = localStorage.getItem('authToken');
  
  const headers = {
    'Content-Type': 'application/json',
    ...options.headers
  };
  
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }
  
  const response = await fetch(`${API_BASE_URL}${endpoint}`, {
    ...options,
    headers
  });
  
  if (response.status === 401) {
    // Token hết hạn, redirect về login
    localStorage.removeItem('authToken');
    window.location.href = '/login.html';
  }
  
  return response;
}
```

### Kiểm tra role
```javascript
function getCurrentUser() {
  const userJson = localStorage.getItem('user');
  return userJson ? JSON.parse(userJson) : null;
}

function isAdmin() {
  const user = getCurrentUser();
  return user && user.role === 'admin';
}

function isShipper() {
  const user = getCurrentUser();
  return user && user.role === 'shipper';
}

// Ẩn/hiện UI theo role
if (isAdmin()) {
  document.getElementById('admin-panel').style.display = 'block';
}
```

## ✨ Best Practices

1. **Luôn validate token ở backend** - Không tin tưởng frontend
2. **Refresh token trước khi hết hạn** - Tránh gián đoạn UX
3. **Logout = xóa token** - Clear localStorage
4. **HTTPS trong production** - Bảo mật token
5. **Token expiry hợp lý** - 60 phút là tốt cho demo
6. **Không lưu sensitive data trong JWT** - Chỉ lưu ID, role

---

🎉 **Phân quyền đã được cài đặt thành công!**

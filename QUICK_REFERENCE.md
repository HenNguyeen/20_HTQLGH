# 🚀 Quick Reference - JWT Authentication

## 🔑 Test Accounts
```
admin/admin123      → Full access
customer1/customer123  → Create orders, view orders
shipper1/shipper123    → Update status, GPS check-in
```

## 📝 Quick Start

### 1. Start API
```bash
cd DeliveryManagementAPI
dotnet run
# API: http://localhost:5221
# Swagger: http://localhost:5221
```

### 2. Test Login
```
Open: login.html
Username: admin
Password: admin123
```

### 3. Test with Swagger
```
1. Go to http://localhost:5221
2. POST /api/Auth/login → Get token
3. Click "Authorize" → Paste "Bearer {token}"
4. Test protected endpoints
```

## 🎯 Common Tasks

### Login Programmatically
```javascript
const result = await apiService.login({ 
    username: 'admin', 
    password: 'admin123' 
});

auth.setToken(result.token);
auth.setCurrentUser(result.user);
```

### Check User Role
```javascript
if (auth.isAdmin()) { /* Admin features */ }
if (auth.isCustomer()) { /* Customer features */ }
if (auth.isShipper()) { /* Shipper features */ }
```

### Get Current User
```javascript
const user = auth.getCurrentUser();
// { userId, username, fullName, role, email }
```

### Logout
```javascript
auth.logout(); // Auto-redirect to login
```

### API Call with Token
```javascript
// Token tự động được thêm
const orders = await apiService.getAllOrders();
```

## 📊 Role Permissions

| Action | Admin | Customer | Shipper |
|--------|-------|----------|---------|
| View orders | ✅ | ✅ | ✅ |
| Create order | ✅ | ✅ | ❌ |
| Update status | ✅ | ❌ | ✅ |
| Assign staff | ✅ | ❌ | ❌ |
| Delete order | ✅ | ❌ | ❌ |
| Manage staff | ✅ | ❌ | ❌ |
| GPS check-in | ✅ | ❌ | ✅ |
| Public tracking | ✅ | ✅ | ✅ |

## 🔧 Troubleshooting

### 401 Unauthorized
- Token hết hạn (60 phút)
- Token không hợp lệ
- Chưa đăng nhập
→ **Solution:** Login lại

### 403 Forbidden
- Không có quyền truy cập endpoint
- Role không phù hợp
→ **Solution:** Login với account có quyền

### Token không được gửi
- Check: F12 → Network → Headers
- Should see: `Authorization: Bearer ...`
→ **Solution:** Kiểm tra auth.getToken()

## 📱 Pages

| Page | Auth Required | Roles |
|------|---------------|-------|
| login.html | ❌ Public | All |
| register.html | ❌ Public | All |
| index.html | ✅ Required | All |
| orders.html | ✅ Required | All |
| staff.html | ✅ Required | **Admin only** |
| tracking.html | ⚡ Optional | Public + Auth |
| jwt-test.html | ⚡ Test | All |

## 💾 Storage

**LocalStorage (Remember Me = true)**
- `authToken` - JWT token
- `currentUser` - User info JSON

**SessionStorage (Remember Me = false)**
- `authToken` - JWT token
- `currentUser` - User info JSON

## 📚 Files

| File | Purpose |
|------|---------|
| AUTHORIZATION_GUIDE.md | Backend docs |
| JWT_INTEGRATION_GUIDE.md | Frontend docs |
| JWT_INTEGRATION_SUMMARY.md | Work summary |
| JWT_COMPLETE_SUMMARY.md | Complete overview |
| **THIS FILE** | Quick reference |

## 🎨 UI Elements

### User Dropdown
```html
<i class="fas fa-user-circle"></i>
<span class="user-name">John Doe</span>
<span class="user-role badge bg-danger">Admin</span>
```

### Role Badges
- Admin: `badge bg-danger` (Red)
- Customer: `badge bg-success` (Green)
- Shipper: `badge bg-info` (Blue)

## ⌨️ Console Commands

```javascript
// Get token
auth.getToken()

// Get user
auth.getCurrentUser()

// Check login
auth.isLoggedIn()

// Check role
auth.isAdmin()
auth.isCustomer()
auth.isShipper()

// Logout
auth.logout()
```

## 🔗 API Endpoints

**Auth**
- POST `/api/Auth/login`
- POST `/api/Auth/register`
- POST `/api/Auth/forgot-password`
- POST `/api/Auth/reset-password`

**Orders (Auth Required)**
- GET `/api/Orders`
- POST `/api/Orders` (Admin, Customer)
- PATCH `/api/Orders/{id}/status` (Admin, Shipper)

**Staff (Admin Only)**
- GET `/api/DeliveryStaff`
- POST `/api/DeliveryStaff`
- PUT `/api/DeliveryStaff/{id}`
- DELETE `/api/DeliveryStaff/{id}`

**Tracking**
- GET `/api/Tracking/track/{code}` (Public)
- POST `/api/Tracking/checkin` (Admin, Shipper)

---

**Last Updated:** October 20, 2025

# 🔐 Hướng Dẫn Tích Hợp JWT vào Frontend

## ✅ Đã hoàn thành

### 1. **API Service với JWT Authentication**
File: `js/api-service.js`

**Thêm Auth Helper:**
```javascript
const auth = {
    getToken()          // Lấy JWT token
    setToken()          // Lưu token
    removeToken()       // Xóa token
    getCurrentUser()    // Lấy thông tin user
    setCurrentUser()    // Lưu user info
    isLoggedIn()        // Kiểm tra đã login
    isAdmin()           // Kiểm tra role admin
    isCustomer()        // Kiểm tra role customer
    isShipper()         // Kiểm tra role shipper
    logout()            // Đăng xuất
    requireAuth()       // Yêu cầu login
}
```

**Auto-add JWT Token vào API requests:**
```javascript
async request(endpoint, options = {}) {
    const token = auth.getToken();
    const headers = {
        'Content-Type': 'application/json',
        ...options.headers
    };
    
    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
    }
    
    // Auto-redirect khi 401 Unauthorized
    if (response.status === 401) {
        auth.removeToken();
        window.location.href = 'login.html';
    }
    
    // Show error khi 403 Forbidden
    if (response.status === 403) {
        utils.showToast('Bạn không có quyền!', 'danger');
    }
}
```

### 2. **Login Page - Cải tiến**
File: `js/login.js`

**Tính năng mới:**
- ✅ Lưu JWT token vào localStorage/sessionStorage
- ✅ Lưu thông tin user (fullName, role, email)
- ✅ Remember Me - chọn lưu dài hạn hay session
- ✅ Redirect theo role:
  - Admin → Dashboard
  - Customer → Orders
  - Shipper → Dashboard
- ✅ Loading spinner khi đang login
- ✅ Error handling đầy đủ

**Flow:**
1. User nhập username/password
2. Click "Đăng nhập"
3. Call API `/api/Auth/login`
4. Nhận response: `{ token, user }`
5. Lưu token và user info
6. Redirect theo role

### 3. **Register Page - Cải tiến**
File: `js/register.js`

**Tính năng mới:**
- ✅ Validate password match
- ✅ Validate password length (min 6 chars)
- ✅ Loading spinner
- ✅ Auto-redirect sau 2s
- ✅ Reset form sau register thành công
- ✅ Error handling

### 4. **Forgot Password Page - Cải tiến**
File: `js/forgot-password.js`

**Tính năng mới:**
- ✅ Loading spinner
- ✅ Success/error messages
- ✅ Reset form sau submit
- ✅ Error handling

### 5. **Dashboard - Auth Check**
File: `js/dashboard.js`

**Tính năng mới:**
- ✅ Check authentication khi load page
- ✅ Auto-redirect nếu chưa login
- ✅ Hiển thị user info (name, role)
- ✅ Role badge với màu khác nhau:
  - Admin: `badge bg-danger`
  - Shipper: `badge bg-info`
  - Customer: `badge bg-success`

### 6. **Orders Page - Auth & Role-based UI**
File: `js/orders.js`

**Tính năng mới:**
- ✅ Check authentication
- ✅ Hide admin-only features for customers
- ✅ Hide customer-only features for shippers
- ✅ Role-based UI rendering

**CSS Classes:**
```html
<button class="admin-only">Chỉ Admin thấy</button>
<button class="customer-only">Chỉ Customer thấy</button>
<button class="shipper-only">Chỉ Shipper thấy</button>
```

### 7. **Staff Page - Admin Only**
File: `js/staff.js`

**Tính năng mới:**
- ✅ Check if user is admin
- ✅ Redirect non-admin users to dashboard
- ✅ Show error message cho non-admin

### 8. **Tracking Page - Public + Auth**
File: `js/tracking.js`

**Đặc biệt:**
- ✅ Cho phép anonymous access (tracking công khai)
- ✅ Nếu logged in → show user info
- ✅ Nếu chưa login → hide auth-only features
- ✅ Class `auth-only` để control visibility

### 9. **HTML Updates - User Dropdown**
Files: `index.html`, `orders.html`, `staff.html`, `tracking.html`

**Navbar mới:**
```html
<div class="dropdown">
    <button class="btn btn-link dropdown-toggle">
        <i class="fas fa-user-circle fa-lg me-2"></i>
        <span class="user-name">User</span>
        <span class="user-role badge bg-primary ms-2">Role</span>
    </button>
    <ul class="dropdown-menu dropdown-menu-end">
        <li><h6 class="dropdown-header"><span class="user-name">User</span></h6></li>
        <li><hr class="dropdown-divider"></li>
        <li><a class="dropdown-item" href="#"><i class="fas fa-user me-2"></i>Thông tin tài khoản</a></li>
        <li><a class="dropdown-item" href="#"><i class="fas fa-cog me-2"></i>Cài đặt</a></li>
        <li><hr class="dropdown-divider"></li>
        <li><a class="dropdown-item text-danger" href="#" onclick="auth.logout()">
            <i class="fas fa-sign-out-alt me-2"></i>Đăng xuất
        </a></li>
    </ul>
</div>
```

## 🎯 Cách Sử Dụng

### A. Login Flow

1. **Mở trang login:** `login.html`
2. **Nhập thông tin:**
   - Username: `admin` hoặc `customer1` hoặc `shipper1`
   - Password: `admin123`, `customer123`, `shipper123`
3. **Click "Đăng nhập"**
4. **Token được lưu vào:**
   - `localStorage.authToken` (nếu Remember Me)
   - `sessionStorage.authToken` (nếu không)
5. **User info được lưu:**
   - `localStorage.currentUser` hoặc `sessionStorage.currentUser`

### B. Gọi API với Token

**Tự động - không cần code gì thêm:**
```javascript
// Token tự động được thêm vào header
const orders = await apiService.getAllOrders();
```

**Kiểm tra trong Browser DevTools:**
```
Network → Headers → Request Headers
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### C. Kiểm tra Role trong JS

```javascript
// Check login status
if (auth.isLoggedIn()) {
    console.log('User đã login');
}

// Check role
if (auth.isAdmin()) {
    // Show admin features
    document.querySelector('.admin-panel').style.display = 'block';
}

if (auth.isCustomer()) {
    // Show customer features
    document.querySelector('.create-order-btn').style.display = 'block';
}

if (auth.isShipper()) {
    // Show shipper features
    document.querySelector('.checkin-btn').style.display = 'block';
}

// Get user info
const user = auth.getCurrentUser();
console.log(user.fullName); // "Quản trị viên"
console.log(user.role);     // "admin"
console.log(user.email);    // "admin@delivery.com"
```

### D. Logout

**Cách 1: Click vào dropdown menu**
```html
<a onclick="auth.logout()">Đăng xuất</a>
```

**Cách 2: Programmatically**
```javascript
auth.logout(); // Auto-redirect to login.html
```

### E. Require Auth trong Page

```javascript
document.addEventListener('DOMContentLoaded', function() {
    // Check authentication
    if (!auth.requireAuth()) {
        return; // Auto-redirect to login
    }
    
    // Load page data...
});
```

## 🔧 Token Management

### Token Storage

**LocalStorage (Remember Me):**
- Tồn tại vĩnh viễn cho đến khi logout
- Dùng khi user check "Ghi nhớ đăng nhập"

**SessionStorage (Default):**
- Xóa khi đóng browser
- An toàn hơn cho máy công cộng

### Token Expiry

- **Duration:** 60 phút (config trong backend)
- **Auto-refresh:** Chưa implement (TODO)
- **Expired behavior:**
  - API trả về 401 Unauthorized
  - Token bị xóa
  - Auto-redirect về login.html
  - Show message: "Phiên đăng nhập hết hạn"

### Token Structure

**Decoded JWT:**
```json
{
  "nameid": "1",
  "unique_name": "admin",
  "role": "admin",
  "email": "admin@delivery.com",
  "FullName": "Quản trị viên",
  "exp": 1729434000,
  "iss": "DeliveryManagementAPI",
  "aud": "DeliveryManagementClients"
}
```

## 🎨 UI/UX Features

### 1. **Loading Spinners**
```html
<button disabled>
    <span class="spinner-border spinner-border-sm me-2"></span>
    Đang đăng nhập...
</button>
```

### 2. **Error Messages với Icons**
```html
<div class="alert alert-danger">
    <i class="fas fa-exclamation-triangle me-2"></i>
    Đăng nhập thất bại!
</div>
```

### 3. **Success Messages**
```html
<div class="alert alert-success">
    <i class="fas fa-check-circle me-2"></i>
    Đăng nhập thành công!
</div>
```

### 4. **Role Badges**
```html
<span class="badge bg-danger">Quản trị viên</span>    <!-- Admin -->
<span class="badge bg-info">Nhân viên giao hàng</span> <!-- Shipper -->
<span class="badge bg-success">Khách hàng</span>       <!-- Customer -->
```

## 🔒 Security Best Practices

### ✅ Implemented

1. **Token in Authorization header** - Không qua URL
2. **Auto-redirect on 401** - Phát hiện token hết hạn
3. **Client-side role check** - Hide UI elements
4. **Server-side authorization** - Backend vẫn validate

### ⚠️ Important Notes

1. **Client-side checks chỉ là UX** - Không phải security
2. **Backend vẫn phải validate role** - [Authorize] attributes
3. **HTTPS trong production** - Bảo mật token khi truyền
4. **Không lưu password** - Chỉ lưu token
5. **Token expiry hợp lý** - 60 phút là okay

## 📊 Test Scenarios

### Scenario 1: Admin Login
1. Login với `admin/admin123`
2. Redirect to Dashboard
3. See: "Quản trị viên" badge (red)
4. All pages accessible
5. All buttons visible

### Scenario 2: Customer Login
1. Login với `customer1/customer123`
2. Redirect to Orders page
3. See: "Khách hàng" badge (green)
4. Can create orders
5. Cannot access Staff page

### Scenario 3: Shipper Login
1. Login với `shipper1/shipper123`
2. Redirect to Dashboard
3. See: "Nhân viên giao hàng" badge (blue)
4. Can update order status
5. Can check-in GPS

### Scenario 4: Token Expired
1. Login successful
2. Wait 60 minutes
3. Try to load orders
4. Get 401 from API
5. Auto-redirect to login
6. See message: "Phiên đăng nhập hết hạn"

### Scenario 5: Logout
1. Click user dropdown
2. Click "Đăng xuất"
3. Token removed
4. Redirect to login.html

## 🚀 Next Steps (TODO)

- [ ] Auto-refresh token trước khi hết hạn
- [ ] Remember last visited page
- [ ] Profile page để edit user info
- [ ] Change password functionality
- [ ] 2FA (Two-Factor Authentication)
- [ ] Session management (kill other sessions)
- [ ] Activity log (login history)

---

🎉 **JWT Integration hoàn thành!** Frontend giờ đã có authentication & authorization đầy đủ!

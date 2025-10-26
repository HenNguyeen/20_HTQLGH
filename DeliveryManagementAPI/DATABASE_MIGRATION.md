# 🚀 Delivery Management System - Cập nhật Database Migration

## ✅ Đã hoàn thành

### 1. **Chuyển đổi sang SQL Server Database**
- ✅ Tạo `DeliveryDbContext` với Entity Framework Core
- ✅ Cấu hình connection string: `Server=localhost\SQLEXPRESS`
- ✅ Migration database với tất cả các bảng:
  - **UserAccounts** - Quản lý tài khoản người dùng
  - **Orders** - Quản lý đơn hàng
  - **DeliveryStaffs** - Quản lý nhân viên giao hàng
  - **LocationCheckpoints** - Theo dõi GPS
  - **Customers** - Quản lý khách hàng

### 2. **Services sử dụng Entity Framework**
- ✅ **OrderService** - CRUD đơn hàng, thống kê, gán nhân viên
- ✅ **DeliveryStaffService** - CRUD nhân viên, cập nhật trạng thái
- ✅ **CheckpointService** - Quản lý điểm check-in GPS
- ✅ **UserAccountService** - Xác thực, đăng ký, reset mật khẩu

### 3. **Controllers đã cập nhật**
- ✅ **OrdersController** - Sử dụng OrderService + DeliveryStaffService
- ✅ **DeliveryStaffController** - Sử dụng DeliveryStaffService
- ✅ **TrackingController** - Sử dụng OrderService + CheckpointService
- ✅ **AuthController** - Sử dụng UserAccountService (async methods)

### 4. **Seed Data**
Đã tạo dữ liệu mẫu:
- **3 Users**: admin/admin123, customer1/customer123, shipper1/shipper123
- **3 Delivery Staff**: Trần Văn B, Lê Thị C, Phạm Văn D
- **2 Customers**: Hoàng Văn E, Võ Thị F
- **3 Orders**: DH001 (Đang giao), DH002 (Chưa nhận), DH003 (Đã giao)
- **3 Location Checkpoints**: Theo dõi vị trí đơn hàng

## 🌐 API Endpoints (http://localhost:5221)

### **Authentication** (`/api/Auth`)
```
POST /api/Auth/login                - Đăng nhập
POST /api/Auth/register             - Đăng ký
POST /api/Auth/forgot-password      - Quên mật khẩu
POST /api/Auth/reset-password       - Reset mật khẩu
```

### **Orders** (`/api/Orders`)
```
GET    /api/Orders                  - Lấy tất cả đơn hàng
GET    /api/Orders/{id}             - Lấy đơn hàng theo ID
POST   /api/Orders                  - Tạo đơn hàng mới
PATCH  /api/Orders/{id}/status      - Cập nhật trạng thái
PATCH  /api/Orders/{id}/assign-staff/{staffId} - Gán nhân viên
GET    /api/Orders/status/{status}  - Lấy đơn theo trạng thái
GET    /api/Orders/staff/{staffId}  - Lấy đơn theo nhân viên
DELETE /api/Orders/{id}             - Xóa đơn hàng
```

### **Delivery Staff** (`/api/DeliveryStaff`)
```
GET    /api/DeliveryStaff           - Lấy tất cả nhân viên
GET    /api/DeliveryStaff/{id}      - Lấy nhân viên theo ID
GET    /api/DeliveryStaff/available - Lấy nhân viên rảnh
POST   /api/DeliveryStaff           - Tạo nhân viên mới
PUT    /api/DeliveryStaff/{id}      - Cập nhật nhân viên
PATCH  /api/DeliveryStaff/{id}/availability - Cập nhật trạng thái
DELETE /api/DeliveryStaff/{id}      - Xóa nhân viên
```

### **Tracking** (`/api/Tracking`)
```
GET  /api/Tracking/order/{orderId}  - Lấy lịch sử check-in
POST /api/Tracking/checkin          - Check-in vị trí
GET  /api/Tracking/track/{orderCode} - Theo dõi đơn hàng
GET  /api/Tracking/location/{orderId} - Vị trí hiện tại
```

## 📊 Database Schema

### **UserAccounts**
- UserId (PK, IDENTITY)
- Username, PasswordHash, FullName
- Email, PhoneNumber, Role
- PasswordResetToken, ResetTokenExpiry

### **Orders**
- OrderId (PK, IDENTITY)
- OrderCode, CreatedDate
- CustomerId (FK → Customers)
- ProductCode, PackageType, Weight, Size
- ShippingFee (decimal 18,2), CollectionAmount (decimal 18,2)
- Status, AssignedStaffStaffId (FK → DeliveryStaffs)

### **DeliveryStaffs**
- StaffId (PK, IDENTITY)
- FullName, PhoneNumber
- VehicleType, VehiclePlate
- IsAvailable

### **Customers**
- CustomerId (PK, IDENTITY)
- FullName, PhoneNumber
- Address, Ward, District, City

### **LocationCheckpoints**
- CheckpointId (PK, IDENTITY)
- OrderId (FK → Orders)
- Latitude, Longitude
- LocationName, CheckInTime, Notes

## 🔧 Chạy Project

### 1. **Khởi động API**
```bash
cd DeliveryManagementAPI
dotnet run
```
API sẽ chạy tại: http://localhost:5221

### 2. **Test với Swagger**
Mở trình duyệt: http://localhost:5221

### 3. **Test đăng nhập**
```json
POST http://localhost:5221/api/Auth/login
{
  "username": "admin",
  "password": "admin123"
}
```

### 4. **Xem dữ liệu**
```
GET http://localhost:5221/api/Orders
GET http://localhost:5221/api/DeliveryStaff
GET http://localhost:5221/api/Tracking/track/DH001
```

## 📝 Migration Commands

### Tạo migration mới
```bash
dotnet ef migrations add MigrationName
```

### Áp dụng migration
```bash
dotnet ef database update
```

### Xóa database
```bash
dotnet ef database drop --force
```

### Xóa migration cuối
```bash
dotnet ef migrations remove
```

## 🎯 Tiếp theo

### Chức năng cần hoàn thiện:
1. ⏳ **Phân quyền** - JWT Authorization (Admin, Customer, Shipper)
2. ⏳ **Quản lý khách hàng** - CRUD customers, lịch sử đơn hàng
3. ⏳ **SignalR** - Thông báo realtime
4. ⏳ **Export** - PDF/Excel reports
5. ⏳ **Audit Log** - Ghi nhật ký hoạt động

### Test accounts:
- **Admin**: `admin` / `admin123`
- **Customer**: `customer1` / `customer123`
- **Shipper**: `shipper1` / `shipper123`

---

✨ **API đang chạy thành công với SQL Server Database!**

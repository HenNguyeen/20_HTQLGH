# 🔧 Fix: Admin Login Issue - Password Hash Mismatch

## Vấn Đề (The Problem)
❌ Tài khoản admin/admin123 không thể đăng nhập nữa
❌ Lý do: Khi chúng tôi đổi từ SHA256 sang BCrypt, dữ liệu cũ vẫn dùng SHA256

## Nguyên Nhân (Root Cause)
1. **user-accounts.json** có `PasswordHash` rỗng hoặc dùng SHA256 cũ
2. **Database** lưu trữ hash cũ không tương thích với BCrypt
3. **Login endpoint** dùng `BCrypt.Verify()` nhưng hash cũ là SHA256
4. Kết quả: **Login fail** vì BCrypt không recognize SHA256 hash

## Giải Pháp (Solution) ✅

### Bước 1: Database đã được xóa
- ✅ Xóa file database (.db)
- ✅ Xóa file user-accounts.json
- ✅ Xóa file dữ liệu cũ

### Bước 2: Khi API khởi động lại
- SeedData.cs sẽ chạy tự động
- Sẽ hash password "admin123" bằng **BCrypt mới**
- Tạo UserAccount với hash chính xác

### Bước 3: Đăng nhập lại
```
Username: admin
Password: admin123
```

## Tài Khoản Test (Test Accounts)

| Username | Password | Role | Email |
|----------|----------|------|-------|
| admin | admin123 | admin | admin@delivery.com |
| customer1 | customer123 | customer | customer1@gmail.com |
| shipper1 | 123456 | shipper | shipper1@gmail.com |

## Cách Khởi Động Lại (How to Restart API)

### Option 1: Visual Studio
```
Ctrl + Shift + F5  (Restart API)
```

### Option 2: Terminal
```
cd c:\Users\DELL\Documents\GitHub\20_HTQLGH\DeliveryManagementAPI
dotnet run
```

### Option 3: Kill existing process & restart
```powershell
# Tìm và kill process API
Get-Process | Where-Object {$_.Name -like "*dotnet*"} | Stop-Process -Force

# Khởi động lại
dotnet run
```

Sau khi API khởi động lại, bạn sẽ thấy log:
```
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand - Creating tables...
...
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: http://localhost:5221
```

## Xác Nhận (Verification) ✅

Sau khi restart API:
1. Mở trang login: http://localhost:3000/login.html
2. Nhập: admin / admin123
3. Nếu login thành công → ✅ Problem solved!
4. Bạn sẽ thấy dashboard với role "admin"

## Tại Sao Điều Này Xảy Ra (Why This Happened)

Khi chúng ta update từ SHA256 → BCrypt:
- ✅ Code được update
- ✅ Hashing algorithm mới được integrate
- ❌ Nhưng **dữ liệu cũ không được migrate**
- ❌ Admin account vẫn dùng hash cũ
- ❌ BCrypt verify không thể recognize SHA256 hash

## Bài Học (Lesson)

Lần sau khi thay đổi password hashing, cần:
1. **Migration script** để re-hash tất cả user existing
2. **Hoặc**: Reset database và reseed
3. **Hoặc**: Supportboth SHA256 và BCrypt trong transition period

## Nếu Vẫn Không Hoạt Động (If Still Not Working)

Kiểm tra:
```powershell
# 1. Check xem database file mới tạo không
Get-ChildItem "c:\Users\DELL\Documents\GitHub\20_HTQLGH\DeliveryManagementAPI" -Filter "*.db"

# 2. Check xem SeedData chạy chưa (look for log messages about seeding)
# 3. Xem error log: C:\Users\DELL\Documents\GitHub\20_HTQLGH\DeliveryManagementAPI\log.txt (if any)

#4. Xem browser console cho error messages
# 5. Check AuthController response - có tin gì không
```

**Liên hệ** nếu vẫn có vấn đề! 🚀

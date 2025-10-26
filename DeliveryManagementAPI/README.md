# 🚚 Hệ Thống Quản Lý Giao Hàng - Delivery Management API

## 📋 Mô tả dự án

API quản lý hệ thống giao hàng được xây dựng bằng ASP.NET Core, sử dụng dữ liệu JSON để lưu trữ thông tin. Hệ thống hỗ trợ đầy đủ quy trình từ nhận đơn hàng, phân loại, giao hàng và theo dõi vị trí.

## ✨ Tính năng chính

### 1. 📦 Quản lý đơn hàng
- Nhận mã đơn hàng từ website bán hàng
- Nhập thông tin khách hàng và hàng hóa
- Tính phí giao hàng tự động dựa trên nhiều yếu tố:
  - Khoảng cách
  - Trọng lượng
  - Loại hàng (dễ vỡ, trị giá, xe...)
  - Loại giao hàng (thường/nhanh)

### 2. 💰 Quản lý thanh toán
- Gửi thường: Nhận tiền khi giao hàng (COD)
- Gửi nhanh: Chuyển khoản/Thanh toán trực tuyến

### 3. 📊 Phân loại hàng hóa
Hỗ trợ các loại: Gói nhỏ, Bọc, Bao, Thùng, TiVi, Laptop, Máy tính, CPU, Xe...

### 4. 🚴 Quản lý nhân viên giao hàng
- Danh sách nhân viên và phương tiện
- Trạng thái sẵn sàng
- Gán nhân viên cho đơn hàng

### 5. 📍 Theo dõi vị trí (Tracking)
- Check-in tự động tại các điểm trên đường
- Theo dõi đơn hàng theo mã
- Xem vị trí hiện tại của đơn hàng

### 6. 🔄 Cập nhật trạng thái
- Chưa nhận
- Đã nhận - Chưa giao
- Đã nhận - Đang giao
- Đã giao

## 🛠️ Công nghệ sử dụng

- **Framework**: ASP.NET Core 9.0
- **Lưu trữ dữ liệu**: JSON Files
- **API Documentation**: Swagger/OpenAPI
- **Ngôn ngữ**: C# 12

## 📁 Cấu trúc dự án

```
DeliveryManagementAPI/
├── Controllers/
│   ├── OrdersController.cs          # API quản lý đơn hàng
│   ├── DeliveryStaffController.cs   # API quản lý nhân viên
│   └── TrackingController.cs        # API theo dõi vị trí
├── Models/
│   ├── Order.cs                     # Model đơn hàng
│   ├── Customer.cs                  # Model khách hàng
│   ├── DeliveryStaff.cs            # Model nhân viên
│   ├── LocationCheckpoint.cs       # Model điểm check-in
│   ├── OrderStatus.cs              # Enum trạng thái
│   ├── DeliveryType.cs             # Enum loại giao hàng
│   ├── PaymentMethod.cs            # Enum phương thức thanh toán
│   └── PackageType.cs              # Enum loại hàng
├── Services/
│   ├── JsonDataService.cs          # Service quản lý dữ liệu JSON
│   └── ShippingFeeService.cs       # Service tính phí
├── Data/
│   ├── orders.json                 # Dữ liệu đơn hàng
│   ├── delivery-staff.json         # Dữ liệu nhân viên
│   └── checkpoints.json            # Dữ liệu check-in
└── Program.cs                       # Entry point
```

## 🚀 Cách chạy ứng dụng

### Yêu cầu
- .NET SDK 9.0 trở lên
- Visual Studio 2022 hoặc VS Code

### Các bước chạy

1. **Build dự án**
```bash
dotnet build
```

2. **Chạy ứng dụng**
```bash
dotnet run
```

3. **Truy cập Swagger UI**
```
https://localhost:5001
hoặc
http://localhost:5000
```

## 📡 API Endpoints

### Orders API (`/api/orders`)

| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/api/orders` | Lấy tất cả đơn hàng |
| GET | `/api/orders/{id}` | Lấy đơn hàng theo ID |
| POST | `/api/orders` | Tạo đơn hàng mới |
| PATCH | `/api/orders/{id}/status` | Cập nhật trạng thái |
| PATCH | `/api/orders/{id}/assign-staff/{staffId}` | Gán nhân viên |
| GET | `/api/orders/status/{status}` | Lọc theo trạng thái |
| GET | `/api/orders/staff/{staffId}` | Lọc theo nhân viên |
| DELETE | `/api/orders/{id}` | Xóa đơn hàng |

### Delivery Staff API (`/api/deliverystaff`)

| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/api/deliverystaff` | Lấy tất cả nhân viên |
| GET | `/api/deliverystaff/{id}` | Lấy nhân viên theo ID |
| GET | `/api/deliverystaff/available` | Lấy nhân viên rảnh |
| POST | `/api/deliverystaff` | Thêm nhân viên mới |
| PATCH | `/api/deliverystaff/{id}/availability` | Cập nhật trạng thái |

### Tracking API (`/api/tracking`)

| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/api/tracking/order/{orderId}` | Lấy lịch sử check-in |
| POST | `/api/tracking/checkin` | Check-in vị trí mới |
| GET | `/api/tracking/track/{orderCode}` | Theo dõi theo mã đơn |
| GET | `/api/tracking/location/{orderId}` | Lấy vị trí hiện tại |

## 📝 Ví dụ sử dụng

### Tạo đơn hàng mới

```json
POST /api/orders
{
  "orderCode": "WEB20251019004",
  "customerName": "Nguyễn Văn A",
  "customerPhone": "0912345678",
  "deliveryAddress": "123 Nguyễn Huệ",
  "ward": "Bến Nghé",
  "district": "Quận 1",
  "city": "TP. Hồ Chí Minh",
  "productCode": "PROD001",
  "packageType": 8,
  "weight": 2.5,
  "size": "45x35x10 cm",
  "distance": 8.5,
  "isFragile": true,
  "isValuable": true,
  "isVehicle": false,
  "collectMoney": true,
  "collectionAmount": 5000000,
  "paymentMethod": 0,
  "deliveryType": 1,
  "notes": "Giao hàng nhanh"
}
```

### Cập nhật trạng thái đơn hàng

```json
PATCH /api/orders/{id}/status
{
  "orderId": "ORD001",
  "status": 2,
  "staffId": "STAFF001",
  "notes": "Đang trên đường giao"
}
```

### Check-in vị trí

```json
POST /api/tracking/checkin
{
  "orderId": "ORD001",
  "latitude": 10.7769,
  "longitude": 106.7009,
  "locationName": "Ngã tư Hàng Xanh",
  "notes": "Đang trên đường giao hàng"
}
```

## 🎯 Tính năng nổi bật

### Tính phí tự động
Hệ thống tự động tính phí dựa trên:
- Khoảng cách giao hàng
- Trọng lượng hàng hóa
- Loại hàng đặc biệt (dễ vỡ, trị giá, xe)
- Loại giao hàng (thường/nhanh)
- Loại đóng gói

### Theo dõi thời gian thực
- Check-in tự động tại các điểm
- Khách hàng có thể theo dõi đơn hàng
- Lịch sử di chuyển đầy đủ

### Quản lý trạng thái
- Cập nhật trạng thái tự động
- Lưu thời gian từng bước
- Ghi chú chi tiết

## 📊 Enum Values

### OrderStatus
- `0`: Chưa nhận
- `1`: Đã nhận - Chưa giao
- `2`: Đã nhận - Đang giao
- `3`: Đã giao

### DeliveryType
- `0`: Giao hàng thường
- `1`: Giao hàng nhanh

### PaymentMethod
- `0`: Gửi thường (COD)
- `1`: Gửi nhanh
- `2`: Chuyển khoản
- `3`: Thanh toán trực tuyến

### PackageType
- `0`: Gói nhỏ
- `1-6`: Các loại bọc/thùng khác
- `7`: TiVi
- `8`: Laptop
- `9`: Máy tính
- `10`: CPU
- `11`: Xe

## 🔐 CORS

API đã được cấu hình CORS cho phép tất cả origins, methods và headers để dễ dàng tích hợp.

## 📞 Liên hệ

Dự án được xây dựng theo Case Study 14 - Hệ Thống Quản Lý Giao Hàng

## 📄 License

MIT License

# 📖 Hướng Dẫn Sử Dụng API Quản Lý Giao Hàng

## 🎯 Mục lục
1. [Giới thiệu](#giới-thiệu)
2. [Các bước bắt đầu](#các-bước-bắt-đầu)
3. [Quy trình nghiệp vụ](#quy-trình-nghiệp-vụ)
4. [API Chi tiết](#api-chi-tiết)
5. [Ví dụ thực tế](#ví-dụ-thực-tế)

## 🌟 Giới thiệu

API này cung cấp đầy đủ chức năng để quản lý hệ thống giao hàng từ A-Z:
- ✅ Nhận và tạo đơn hàng
- ✅ Tính phí tự động
- ✅ Phân công nhân viên
- ✅ Theo dõi vị trí thời gian thực
- ✅ Cập nhật trạng thái đơn hàng

## 🚀 Các bước bắt đầu

### 1. Chạy ứng dụng
```bash
cd DeliveryManagementAPI
dotnet run
```

### 2. Truy cập Swagger UI
Mở trình duyệt tại: **http://localhost:5221**

### 3. Test API
- Sử dụng Swagger UI để test trực tiếp
- Hoặc sử dụng file `DeliveryManagementAPI.http` trong VS Code
- Hoặc dùng Postman/Insomnia

## 📋 Quy trình nghiệp vụ

### Quy trình 1: Nhận và xử lý đơn hàng mới

```
1. Website bán hàng gửi mã đơn hàng
   ↓
2. Nhân viên nhập thông tin hàng hóa và khách hàng
   POST /api/orders
   ↓
3. Hệ thống tự động tính phí giao hàng
   → Trả về thông tin đơn hàng + phí
   ↓
4. Nhận thanh toán (nếu gửi nhanh)
   → Cập nhật trạng thái đã thanh toán
```

### Quy trình 2: Giao hàng

```
1. Phân công nhân viên
   PATCH /api/orders/{id}/assign-staff/{staffId}
   ↓
2. Cập nhật: Đã nhận - Chưa giao (status=1)
   PATCH /api/orders/{id}/status
   ↓
3. Nhân viên bắt đầu giao: Đang giao (status=2)
   PATCH /api/orders/{id}/status
   ↓
4. Check-in vị trí theo đường đi
   POST /api/tracking/checkin
   ↓
5. Giao hàng thành công: Đã giao (status=3)
   PATCH /api/orders/{id}/status
```

### Quy trình 3: Khách hàng theo dõi đơn hàng

```
1. Khách nhập mã đơn hàng
   ↓
2. API trả về thông tin đầy đủ
   GET /api/tracking/track/{orderCode}
   ↓
3. Hiển thị:
   - Trạng thái hiện tại
   - Lịch sử di chuyển
   - Vị trí hiện tại
   - Thông tin nhân viên giao
```

## 📡 API Chi tiết

### 📦 ORDERS API

#### 1. Tạo đơn hàng mới
**Endpoint:** `POST /api/orders`

**Request Body:**
```json
{
  "orderCode": "WEB20251019004",
  "customerName": "Nguyễn Văn A",
  "customerPhone": "0912345678",
  "deliveryAddress": "123 Nguyễn Huệ",
  "ward": "Bến Nghé",
  "district": "Quận 1",
  "city": "TP. Hồ Chí Minh",
  "productCode": "PROD001",
  "packageType": 8,        // 8 = Laptop
  "weight": 2.5,
  "size": "45x35x10 cm",
  "distance": 8.5,
  "isFragile": true,
  "isValuable": true,
  "isVehicle": false,
  "collectMoney": true,
  "collectionAmount": 5000000,
  "paymentMethod": 0,      // 0 = COD
  "deliveryType": 1,       // 1 = Giao nhanh
  "notes": "Giao hàng nhanh"
}
```

**Response:**
```json
{
  "orderId": "guid-generated",
  "orderCode": "WEB20251019004",
  "shippingFee": 85000,    // Tự động tính
  "status": 0,             // Chưa nhận
  "customer": { ... },
  "createdDate": "2025-10-19T10:30:00"
}
```

**Cách tính phí giao hàng:**
- Phí cơ bản: 20,000đ
- Khoảng cách (0-5km): +10,000đ
- Khoảng cách (5-10km): +20,000đ
- Khoảng cách (10-20km): +40,000đ
- Khoảng cách (>20km): +60,000đ + 3,000đ/km
- Trọng lượng (>5kg): +2,000đ/kg
- Hàng dễ vỡ: +15,000đ
- Hàng trị giá: +30,000đ
- Hàng là xe: +100,000đ
- Giao nhanh: x1.5 tổng phí
- Laptop/CPU/Máy tính: +20,000đ
- TiVi: +30,000đ
- Xe: +150,000đ

#### 2. Cập nhật trạng thái
**Endpoint:** `PATCH /api/orders/{id}/status`

**Request Body:**
```json
{
  "orderId": "ORD001",
  "status": 2,            // 2 = Đang giao
  "staffId": "STAFF001",  // Optional
  "notes": "Đang trên đường"
}
```

**Các trạng thái:**
- `0`: Chưa nhận
- `1`: Đã nhận - Chưa giao
- `2`: Đã nhận - Đang giao
- `3`: Đã giao

#### 3. Lọc đơn hàng theo trạng thái
**Endpoint:** `GET /api/orders/status/{status}`

**Ví dụ:** `GET /api/orders/status/2` → Tất cả đơn đang giao

#### 4. Lọc đơn hàng theo nhân viên
**Endpoint:** `GET /api/orders/staff/{staffId}`

**Ví dụ:** `GET /api/orders/staff/STAFF001`

### 👷 DELIVERY STAFF API

#### 1. Lấy nhân viên đang rảnh
**Endpoint:** `GET /api/deliverystaff/available`

**Response:**
```json
[
  {
    "staffId": "STAFF003",
    "fullName": "Hoàng Thị Dung",
    "phoneNumber": "0923456789",
    "vehicleType": "Xe tải nhỏ",
    "vehiclePlate": "59C3-11111",
    "isAvailable": true
  }
]
```

#### 2. Thêm nhân viên mới
**Endpoint:** `POST /api/deliverystaff`

**Request Body:**
```json
{
  "fullName": "Lê Văn Giang",
  "phoneNumber": "0956789012",
  "vehicleType": "Xe máy",
  "vehiclePlate": "59F6-44444",
  "isAvailable": true
}
```

#### 3. Cập nhật trạng thái sẵn sàng
**Endpoint:** `PATCH /api/deliverystaff/{id}/availability`

**Request Body:** `true` hoặc `false`

### 📍 TRACKING API

#### 1. Check-in vị trí
**Endpoint:** `POST /api/tracking/checkin`

**Request Body:**
```json
{
  "orderId": "ORD001",
  "latitude": 10.7769,
  "longitude": 106.7009,
  "locationName": "Ngã tư Hàng Xanh",
  "notes": "Đang trên đường giao hàng"
}
```

#### 2. Theo dõi đơn hàng
**Endpoint:** `GET /api/tracking/track/{orderCode}`

**Response:**
```json
{
  "order": {
    "orderId": "ORD001",
    "orderCode": "WEB20251019001",
    "status": 2,
    "customer": { ... },
    "assignedStaff": { ... }
  },
  "checkpoints": [
    {
      "locationName": "Kho trung tâm",
      "checkInTime": "2025-10-19T09:30:00",
      "latitude": 10.7769,
      "longitude": 106.7009
    },
    {
      "locationName": "Ngã tư Hàng Xanh",
      "checkInTime": "2025-10-19T10:15:00",
      "latitude": 10.7820,
      "longitude": 106.6950
    }
  ],
  "currentStatus": "DaNhanDangGiao",
  "lastUpdate": "2025-10-19T10:15:00"
}
```

## 💡 Ví dụ thực tế

### Case 1: Giao laptop nhanh (trong ngày)

**Bước 1: Tạo đơn**
```http
POST /api/orders
{
  "orderCode": "WEB20251019100",
  "customerName": "Trần Văn B",
  "customerPhone": "0908123456",
  "deliveryAddress": "789 Lê Lợi, Q1, TPHCM",
  "packageType": 8,         // Laptop
  "weight": 2.0,
  "distance": 5.5,
  "isFragile": true,
  "isValuable": true,
  "collectMoney": true,
  "collectionAmount": 25000000,
  "paymentMethod": 0,       // COD
  "deliveryType": 1         // Nhanh
}
```
→ Phí: ~70,000đ

**Bước 2: Phân công nhân viên rảnh**
```http
GET /api/deliverystaff/available
→ Chọn STAFF003

PATCH /api/orders/{orderId}/assign-staff/STAFF003
```

**Bước 3: Nhân viên nhận hàng**
```http
PATCH /api/orders/{orderId}/status
{
  "status": 1,  // Đã nhận - Chưa giao
  "notes": "Đã lấy hàng từ kho"
}
```

**Bước 4: Bắt đầu giao**
```http
PATCH /api/orders/{orderId}/status
{
  "status": 2,  // Đang giao
  "notes": "Đang trên đường"
}
```

**Bước 5: Check-in theo đường**
```http
POST /api/tracking/checkin
{
  "orderId": "{orderId}",
  "latitude": 10.7769,
  "longitude": 106.7009,
  "locationName": "Đường Điện Biên Phủ"
}
```

**Bước 6: Giao thành công**
```http
PATCH /api/orders/{orderId}/status
{
  "status": 3,  // Đã giao
  "notes": "Đã giao thành công, thu tiền 25tr"
}
```

### Case 2: Giao xe máy (hàng đặc biệt)

```http
POST /api/orders
{
  "orderCode": "WEB20251019200",
  "customerName": "Nguyễn Văn C",
  "packageType": 11,        // Xe
  "weight": 120.0,
  "distance": 30.0,
  "isVehicle": true,
  "isValuable": true,
  "collectMoney": true,
  "collectionAmount": 50000000,
  "paymentMethod": 0,
  "deliveryType": 1
}
```
→ Phí: ~400,000đ (cần xe chuyên dụng)

### Case 3: Khách hàng theo dõi

**Web/App của khách gọi:**
```http
GET /api/tracking/track/WEB20251019100
```

**Hiển thị:**
- ✅ Đã nhận hàng - 09:00
- ✅ Đang giao - 10:00
- 📍 Vị trí hiện tại: Đường Điện Biên Phủ
- 👷 Nhân viên: Hoàng Thị Dung - 0923456789
- 🚚 Phương tiện: Xe tải nhỏ - 59C3-11111

## 🎨 Tips & Best Practices

### 1. Tối ưu phí giao hàng
- Giao hàng thường cho đơn không gấp → Rẻ hơn 50%
- Gộp nhiều đơn cùng tuyến
- Chọn loại đóng gói phù hợp

### 2. Quản lý nhân viên
- Check nhân viên rảnh trước khi phân công
- Cập nhật trạng thái available khi bận/rảnh
- Phân loại theo loại xe (máy/tải)

### 3. Check-in thường xuyên
- Check-in ít nhất mỗi 10-15 phút
- Check-in tại các điểm quan trọng
- Thêm notes chi tiết

### 4. Xử lý thanh toán
- PaymentMethod = 0 (COD): Thu tiền khi giao
- PaymentMethod = 1,2,3: Đã thanh toán trước

## ⚠️ Lưu ý quan trọng

1. **Dữ liệu JSON**: Tất cả dữ liệu lưu trong folder `Data/`
2. **OrderId**: Tự động generate bằng GUID
3. **Trạng thái**: Phải cập nhật theo thứ tự 0→1→2→3
4. **Phí**: Tự động tính, không cần truyền vào
5. **CORS**: Đã enable cho tất cả origins

## 🔧 Troubleshooting

### Lỗi không lưu được dữ liệu
→ Kiểm tra folder `Data/` có tồn tại không

### Lỗi 404 Not Found
→ Kiểm tra URL và OrderId có đúng không

### Swagger không hiện
→ Chỉ chạy trong Development mode

### Port bị chiếm
→ Đổi port trong `launchSettings.json`

## 📞 Support

Nếu cần hỗ trợ, vui lòng tham khảo:
- README.md
- Swagger UI tại http://localhost:5221
- File test: DeliveryManagementAPI.http

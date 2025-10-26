# 🚀 Quick Start - Hướng Dẫn Nhanh

## Chạy dự án trong 3 bước

### Bước 1: Mở Terminal
```bash
cd DeliveryManagementAPI
```

### Bước 2: Chạy ứng dụng
```bash
dotnet run
```

### Bước 3: Mở trình duyệt
```
http://localhost:5221
```

---

## 🎯 Test API nhanh

### 1️⃣ Tạo đơn hàng mới
**Endpoint:** `POST /api/orders`

Vào Swagger → Orders → POST /api/orders → Try it out

```json
{
  "orderCode": "TEST001",
  "customerName": "Nguyễn Văn A",
  "customerPhone": "0912345678",
  "deliveryAddress": "123 Nguyễn Huệ, Q1, TPHCM",
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
  "notes": "Test đơn hàng"
}
```

→ Nhận được OrderId và phí giao hàng tự động

### 2️⃣ Xem tất cả đơn hàng
**Endpoint:** `GET /api/orders`

→ Thấy đơn vừa tạo + 3 đơn mẫu

### 3️⃣ Xem nhân viên rảnh
**Endpoint:** `GET /api/deliverystaff/available`

→ Danh sách nhân viên sẵn sàng

### 4️⃣ Gán nhân viên cho đơn
**Endpoint:** `PATCH /api/orders/{orderId}/assign-staff/{staffId}`

Thay {orderId} và {staffId} bằng giá trị thực

### 5️⃣ Cập nhật trạng thái
**Endpoint:** `PATCH /api/orders/{orderId}/status`

```json
{
  "orderId": "your-order-id",
  "status": 2,
  "staffId": "STAFF001",
  "notes": "Đang giao hàng"
}
```

### 6️⃣ Check-in vị trí
**Endpoint:** `POST /api/tracking/checkin`

```json
{
  "orderId": "your-order-id",
  "latitude": 10.7769,
  "longitude": 106.7009,
  "locationName": "Đang ở Quận 1",
  "notes": "Trên đường giao"
}
```

### 7️⃣ Theo dõi đơn hàng
**Endpoint:** `GET /api/tracking/track/TEST001`

→ Xem đầy đủ thông tin tracking

---

## 📊 Enum Values (Copy để dùng)

### OrderStatus (Trạng thái)
```
0 = Chưa nhận
1 = Đã nhận - Chưa giao
2 = Đã nhận - Đang giao
3 = Đã giao
```

### DeliveryType (Loại giao hàng)
```
0 = Giao thường (rẻ)
1 = Giao nhanh (x1.5 phí)
```

### PaymentMethod (Thanh toán)
```
0 = COD (thu tiền khi giao)
1 = Gửi nhanh
2 = Chuyển khoản
3 = Thanh toán trực tuyến
```

### PackageType (Loại hàng)
```
0 = Gói nhỏ
1 = Gói bọc van
2 = Bọc
3 = Bao
4 = Thùng
5 = Bao PB
6 = Hộp thùng
7 = TiVi
8 = Laptop
9 = Máy tính
10 = CPU
11 = Xe
```

---

## 💰 Phí giao hàng ước tính

| Loại đơn | Ước tính phí |
|----------|--------------|
| Gói nhỏ 5km thường | ~30,000đ |
| Laptop 10km nhanh | ~85,000đ |
| TiVi 15km nhanh | ~150,000đ |
| Xe máy 25km nhanh | ~400,000đ |

---

## 🔥 Tips

### Test nhanh nhất:
1. Mở Swagger: http://localhost:5221
2. Chọn endpoint
3. Click "Try it out"
4. Điền data hoặc dùng mẫu
5. Click "Execute"

### Debug:
- Xem terminal để thấy logs
- F12 trong browser để xem network
- Kiểm tra file JSON trong folder Data/

### Dừng ứng dụng:
```
Ctrl + C trong terminal
```

---

## 📞 Cần giúp?

1. Đọc `README.md` - Tổng quan
2. Đọc `HUONG_DAN_SU_DUNG.md` - Chi tiết
3. Đọc `TOM_TAT_DU_AN.md` - Tóm tắt
4. Dùng Swagger UI - Documentation
5. Xem file `.http` - Ví dụ test

---

## ✅ Checklist test đầy đủ

- [ ] Tạo đơn hàng mới
- [ ] Xem danh sách đơn hàng
- [ ] Xem nhân viên rảnh
- [ ] Gán nhân viên cho đơn
- [ ] Cập nhật trạng thái: Đã nhận
- [ ] Cập nhật trạng thái: Đang giao
- [ ] Check-in vị trí
- [ ] Theo dõi đơn hàng
- [ ] Xem vị trí hiện tại
- [ ] Cập nhật trạng thái: Đã giao

**Hoàn thành 10/10 → Hệ thống hoạt động tốt! 🎉**

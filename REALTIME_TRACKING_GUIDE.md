# HƯỚNG DẪN SỬ DỤNG TÍNH NĂNG THEO DÕI VỊ TRÍ REAL-TIME

## 📱 Tổng quan tính năng

Hệ thống cho phép **Admin và Khách hàng theo dõi vị trí shipper real-time** khi đơn hàng đang được giao.

### Cơ chế hoạt động:
- **Shipper**: Bật "Chia sẻ vị trí" → App tự động gửi tọa độ GPS mỗi 30 giây
- **Admin/Customer**: Mở trang "Chi tiết đơn hàng" → Nhìn thấy vị trí shipper cập nhật mỗi 5 giây trên bản đồ
- **Độ trễ**: ~5-10 giây (gần như real-time, phù hợp cho giao hàng)

---

## 🚀 HƯỚNG DẪN SỬ DỤNG

### A. DÀNH CHO SHIPPER (Android App)

#### Bước 1: Mở đơn hàng
1. Đăng nhập app với tài khoản shipper
2. Chọn đơn hàng cần giao từ danh sách
3. Vào màn hình "Chi tiết đơn hàng"

#### Bước 2: Bật chia sẻ vị trí
1. Tìm mục **"Theo dõi vị trí Real-time"**
2. Bật switch **"🔴 Bật chia sẻ vị trí"**
3. Cho phép quyền truy cập vị trí khi được hỏi
4. Thấy toast thông báo: _"Đã bật chia sẻ vị trí real-time (30s/lần)"_

#### Bước 3: Giao hàng
- App sẽ **tự động gửi vị trí mỗi 30 giây** lên server
- Latitude/Longitude sẽ tự động điền vào ô nhập
- Bạn có thể để app chạy nền (nhưng nên giữ app mở để tracking ổn định hơn)

#### Bước 4: Tắt chia sẻ (khi hoàn thành)
- Tắt switch để dừng gửi vị trí
- Hoặc đóng app/thoát màn hình chi tiết đơn

#### ⚠️ Lưu ý:
- **Pin**: Tracking liên tục sẽ tốn pin, nên sạc điện thoại trong lúc giao hàng
- **GPS**: Đảm bảo GPS đã bật và có tín hiệu tốt (ở ngoài trời sẽ chính xác hơn)
- **Quyền**: App cần quyền `ACCESS_FINE_LOCATION` để lấy tọa độ

---

### B. DÀNH CHO ADMIN/CUSTOMER (Web UI)

#### Cách 1: Từ danh sách đơn hàng
1. Đăng nhập vào web: `http://localhost:5221`
2. Vào trang **"Đơn Hàng"** (Orders)
3. Tìm đơn cần theo dõi
4. Click nút **"Chi tiết"** (icon 👁️ hoặc nút Info)
5. → Chuyển sang trang `order-detail.html?id={orderId}`

#### Cách 2: Truy cập trực tiếp
- Mở URL: `http://localhost:5221/order-detail.html?id=123` (thay `123` = orderId thực tế)

#### Xem vị trí real-time
Sau khi mở trang chi tiết đơn hàng:

1. **Bản đồ** (bên phải):
   - Hiển thị marker **màu xanh dương** = vị trí shipper hiện tại
   - Bản đồ tự động cập nhật **mỗi 5 giây**
   - Click vào marker để xem chi tiết (thời gian, ghi chú)

2. **Thông tin trạng thái** (bên trái - card "Trạng Thái Vị Trí"):
   - 🟢 **"Shipper đang chia sẻ vị trí"** (xanh lá, nhấp nháy) = Vị trí cập nhật < 2 phút trước
   - 🟡 **"Vị trí chưa được cập nhật gần đây"** (vàng) = Vị trí cũ hơn 2 phút
   - ⚪ **"Chưa có dữ liệu vị trí"** (xám) = Shipper chưa bật tracking hoặc chưa có checkpoint

3. **Thời gian cập nhật**:
   - Hiển thị dưới status: _"Cập nhật lần cuối: vừa xong"_ hoặc _"3 phút trước"_

#### ⚠️ Lưu ý:
- Trang sẽ **tự động dừng polling** khi bạn chuyển sang tab khác (tiết kiệm tài nguyên)
- Khi quay lại tab, polling tự động bật lại
- Cần **đăng nhập** để xem (admin/customer role)

---

## 🛠️ KIỂM TRA & TROUBLESHOOTING

### Test end-to-end:

#### Bước 1: Khởi động backend
```powershell
cd DeliveryManagementAPI
dotnet run
```
→ Server chạy tại `http://localhost:5221`

#### Bước 2: Chạy Android app (Shipper)
1. Mở Android Studio
2. Build & Run app trên emulator/device
3. Login với tài khoản shipper:
   - Phone: `0923456789`
   - Password: `123456`
4. Vào đơn hàng bất kỳ → Bật switch "Chia sẻ vị trí"

#### Bước 3: Mở web (Admin)
1. Mở browser: `http://localhost:5221`
2. Login với admin:
   - Phone: `0912345678`
   - Password: `123456`
3. Vào "Đơn Hàng" → Click "Chi tiết" đơn mà shipper đang bật tracking
4. **Quan sát**: Sau ~30 giây, marker shipper xuất hiện trên map và di chuyển khi shipper di chuyển

---

### Các vấn đề thường gặp:

#### ❌ Shipper: "Không lấy được vị trí"
- Kiểm tra GPS đã bật chưa
- Thử ra ngoài trời (tín hiệu GPS tốt hơn)
- Cấp quyền location cho app: Settings → Apps → Shipper App → Permissions → Location → Allow

#### ❌ Admin: "Chưa có dữ liệu vị trí"
- Đảm bảo shipper đã bật switch tracking
- Đợi ít nhất 30 giây để shipper gửi checkpoint đầu tiên
- Kiểm tra Network tab trong DevTools → Request tới `/api/tracking/location/{orderId}` có response không
- Nếu response 404: Chưa có checkpoint trong DB → Shipper cần bật tracking

#### ❌ Web: "Vị trí không cập nhật"
- Hard refresh: `Ctrl + F5` (xóa cache)
- Mở DevTools → Console, xem có lỗi gì không
- Kiểm tra Network tab → Có request polling mỗi 5s không
- Đảm bảo trang không bị minimize/hide (polling sẽ tạm dừng)

#### ❌ Android: "Tracking dừng khi tắt màn hình"
- Hiện tại app chỉ tracking khi màn hình bật
- Để tracking liên tục khi màn hình tắt → Cần implement **Foreground Service** (tính năng nâng cao, chưa làm)

---

## 📊 KỸ THUẬT & KIẾN TRÚC

### Polling Architecture:

```
┌─────────────────┐     POST /api/tracking/checkin      ┌──────────────┐
│  Shipper App    │────────────(mỗi 30s)─────────────>  │   Backend    │
│  (Android)      │                                       │  ASP.NET API │
└─────────────────┘                                       └──────┬───────┘
                                                                 │
                                                                 │ Lưu DB
                                                                 ▼
                                                          LocationCheckpoints
                                                                 │
                                                                 │
┌─────────────────┐     GET /api/tracking/location/{id}  ┌──────┴───────┐
│  Admin/Customer │◄──────────(mỗi 5s)──────────────────┤   Backend    │
│   (Web UI)      │                                       │  ASP.NET API │
└─────────────────┘                                       └──────────────┘
     │
     │ Update marker on map
     ▼
  Leaflet.js Map
```

### Endpoints sử dụng:

1. **POST /api/tracking/checkin** (Shipper → Backend)
   - Body: `{ orderId, latitude, longitude, locationName, notes }`
   - Lưu checkpoint mới vào DB
   - Authorization: `admin` hoặc `shipper` role

2. **GET /api/tracking/location/{orderId}** (Web → Backend)
   - Response: Checkpoint mới nhất của đơn hàng
   - Authorization: Tất cả user đã đăng nhập

### Performance:

- **Shipper**: 30s/request = 120 requests/giờ (rất nhẹ)
- **Admin**: 5s/request = 720 requests/giờ/user (chấp nhận được với số user nhỏ)
- **Database**: Mỗi checkpoint = 1 row insert (~10-20KB/checkpoint)
- **Scalability**: Với <100 shipper, hệ thống này hoạt động tốt. Nếu scale lớn → cân nhắc SignalR

---

## 🔧 TÙY CHỈNH

### Thay đổi tần suất tracking:

#### Android App (shipper):
File: `OrderDetailActivity.java`
```java
private final long TRACK_INTERVAL_MS = 30 * 1000; // Đổi 30 thành số giây khác
```

#### Web UI (admin/customer):
File: `order-detail.js`
```javascript
const POLLING_INTERVAL_MS = 5000; // Đổi 5000 thành milliseconds khác
```

### Thay đổi ngưỡng "active":
File: `order-detail.js`
```javascript
const isActive = minutesAgo < 2; // Đổi 2 thành số phút khác
```

---

## 📝 CẤU TRÚC FILE MỚI

### Backend:
- ✅ Không cần thay đổi (endpoint đã có sẵn)

### Android App:
- 📝 `OrderDetailActivity.java`: Giảm interval 2min → 30s, cải thiện UX
- 📝 `activity_order_detail.xml`: UI rõ ràng hơn cho switch tracking

### Web UI:
- 🆕 `order-detail.html`: Trang mới hiển thị chi tiết đơn + map real-time
- 🆕 `js/order-detail.js`: Logic polling và cập nhật map mỗi 5s
- 📝 `js/orders.js`: Redirect tới trang order-detail thay vì modal

---

## ✅ TÍNH NĂNG ĐÃ HOÀN THÀNH

- ✅ Shipper bật/tắt chia sẻ vị trí dễ dàng
- ✅ Tracking tự động mỗi 30 giây
- ✅ Admin/Customer xem vị trí real-time trên map
- ✅ Polling tự động mỗi 5 giây
- ✅ Hiển thị trạng thái "đang chia sẻ" / "không hoạt động"
- ✅ Timestamp "cập nhật X phút trước"
- ✅ Tự động dừng polling khi chuyển tab (tiết kiệm tài nguyên)
- ✅ **Lịch sử đường đi**: Vẽ polyline từ tất cả checkpoints (đường nét đứt màu xanh)
- ✅ **Timeline chi tiết**: Hiển thị danh sách tất cả checkpoint với thời gian
- ✅ **Toggle lịch sử**: Bật/tắt hiển thị đường đi bằng nút

## 🎨 GIAO DIỆN LỊCH SỬ ĐƯỜNG ĐI

### Trên bản đồ:
- **Polyline xanh lá nét đứt**: Toàn bộ hành trình shipper đã đi
- **Marker ▶ xanh lá**: Điểm bắt đầu (checkpoint đầu tiên)
- **Chấm tròn xám**: Các checkpoint trung gian (click để xem chi tiết)
- **Marker xanh dương lớn**: Vị trí hiện tại của shipper (real-time)

### Card "Lịch Sử Check-in":
- Hiển thị danh sách tất cả checkpoint theo thứ tự thời gian
- Badge "Bắt đầu" cho checkpoint đầu, "Hiện tại" cho checkpoint cuối
- Thời gian, tọa độ, ghi chú của từng checkpoint
- Tự động scroll được nếu có nhiều checkpoint

### Nút "Ẩn/Hiện lịch sử":
- Ở góc trên bên phải bản đồ
- Click để bật/tắt hiển thị polyline và markers lịch sử
- Vị trí real-time luôn hiển thị

## 🚧 TÍNH NĂNG NÂNG CAO (Có thể làm thêm)

- ⏳ **Foreground Service** cho Android: Tracking liên tục khi màn hình tắt
- ⏳ **SignalR**: Real-time thực sự (<1s latency) thay vì polling
- ⏳ **Thông báo push**: Báo khách hàng khi shipper gần đến
- ⏳ **ETA dự đoán**: Tính thời gian giao dự kiến dựa trên vị trí
- ⏳ **Phát lại hành trình**: Animation theo thời gian thực

---

## 📞 HỖ TRỢ

Nếu gặp vấn đề:
1. Kiểm tra Console/Logcat để xem lỗi chi tiết
2. Đảm bảo backend đang chạy (`dotnet run`)
3. Kiểm tra quyền location của app
4. Hard refresh web UI (`Ctrl+F5`)

Chúc bạn triển khai thành công! 🎉

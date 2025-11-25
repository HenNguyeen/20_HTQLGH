# 📱 Shipper Android App - Realtime Tracking

## ✨ Tính năng mới đã thêm:

### 1. **🔴 Chia sẻ vị trí Realtime qua SignalR**
- Tự động gửi GPS mỗi 10 giây
- Khách hàng xem vị trí shipper realtime trên web
- Switch ON/OFF để bật/tắt tracking

### 2. **🗺️ Bản đồ lộ trình Google Maps**
- Hiển thị vị trí shipper (marker xanh)
- Hiển thị vị trí khách hàng (marker đỏ)
- Đường đi từ shipper → khách hàng
- Tính khoảng cách

### 3. **📍 Checkpoint tự động**
- Vẫn lưu checkpoint vào DB (lịch sử)
- SignalR cho realtime, checkpoint cho lưu trữ

---

## 🔧 Đã cập nhật:

### **Dependencies (build.gradle)**
```gradle
// SignalR
implementation 'com.microsoft.signalr:signalr:7.0.0'

// Google Maps
implementation 'com.google.android.gms:play-services-maps:18.1.0'
```

### **Files mới:**
1. **TrackingService.java** - SignalR client service
2. **OrderMapActivity.java** - Màn hình bản đồ
3. **activity_order_map.xml** - Layout bản đồ

### **Files đã sửa:**
1. **OrderDetailActivity.java**
   - Tích hợp SignalR TrackingService
   - Gửi vị trí qua SignalR mỗi 10s
   - Thêm nút "Xem bản đồ"
   
2. **activity_order_detail.xml**
   - Thêm button "Xem Bản Đồ Lộ Trình"
   
3. **AndroidManifest.xml**
   - Thêm OrderMapActivity
   - Thêm meta-data cho Google Maps API Key

---

## 🚀 Cách sử dụng:

### **1. Cài đặt Google Maps API Key**

#### Lấy API Key:
1. Vào https://console.cloud.google.com/
2. Tạo project mới hoặc chọn project
3. Enable **Maps SDK for Android**
4. Tạo API Key (Credentials → Create API Key)
5. Copy API Key

#### Cập nhật vào app:
Mở file `AndroidManifest.xml` và thay:
```xml
<meta-data
    android:name="com.google.android.geo.API_KEY"
    android:value="YOUR_API_KEY_HERE" />
```

**Lưu ý:** Để test local, có thể dùng **Debug API Key** từ SHA-1 certificate của máy:
```bash
# Windows
keytool -list -v -keystore "%USERPROFILE%\.android\debug.keystore" -alias androiddebugkey -storepass android -keypass android
```

### **2. Chạy app:**

```bash
cd ShipperAndroidApp
./gradlew clean
./gradlew assembleDebug
# Hoặc chạy từ Android Studio
```

### **3. Test tracking:**

1. **Login shipper** (username: shipper1 / pass: 123456)

2. **Chọn đơn hàng** → Vào chi tiết

3. **Bật switch "Chia sẻ vị trí"**
   - App sẽ kết nối SignalR
   - Tự động gửi GPS mỗi 10 giây
   - Thông báo: "✅ Đã kết nối tracking realtime"

4. **Click "Xem Bản Đồ Lộ Trình"**
   - Hiển thị bản đồ Google Maps
   - Marker xanh = shipper
   - Marker đỏ = khách hàng
   - Khoảng cách tính toán

5. **Khách hàng xem web tracking**
   - Mở: `http://localhost:5221/customer/tracking.html?order=DH001&orderId=1`
   - Sẽ thấy marker shipper di chuyển realtime!

---

## 🔄 Luồng hoạt động:

```
Shipper bật switch tracking
    ↓
App kết nối SignalR Hub
    ↓
Lấy GPS mỗi 10 giây
    ↓
Gửi qua SignalR:
  trackingService.sendShipperLocation(staffId, orderId, lat, lng)
    ↓
Server broadcast cho clients
    ↓
Web customer nhận:
  - ReceiveShipperLocation event
  - Cập nhật marker trên bản đồ
```

---

## 📝 Lưu ý:

### **Giới hạn hiện tại:**
- ⚠️ Vị trí khách hàng là **giả lập** (cách shipper ~3km)
- 💡 Cần tích hợp **Geocoding API** để convert địa chỉ → tọa độ chính xác
- 🔐 Google Maps API Key cần được protect (sử dụng API restrictions)

### **Tối ưu hóa:**
- Tracking interval: 10s (có thể giảm xuống 5s)
- SignalR tự động reconnect nếu mất mạng
- Background tracking: Cần implement **Foreground Service** để chạy nền

### **Production checklist:**
- [ ] Lấy Google Maps API Key production
- [ ] Thêm Geocoding để chuyển địa chỉ → GPS
- [ ] Implement Foreground Service cho background tracking
- [ ] Thêm battery optimization
- [ ] Xử lý network offline/online
- [ ] Permission runtime cho Android 11+

---

## 🎯 Kết quả:

✅ **App shipper** gửi vị trí realtime qua SignalR  
✅ **Web customer** nhận và hiển thị marker di chuyển  
✅ **Bản đồ** hiển thị lộ trình shipper → khách hàng  
✅ **Checkpoint** vẫn lưu lịch sử vào DB  

---

## 🆘 Troubleshooting:

### **Lỗi: "SignalR not connected"**
- Kiểm tra BASE_URL đúng: `http://10.0.2.2:5221/` (emulator) hoặc IP máy thật
- Server phải đang chạy: `dotnet run` trong DeliveryManagementAPI
- Kiểm tra firewall không block port 5221

### **Lỗi: "Google Maps not loading"**
- Kiểm tra API Key đã enable Maps SDK for Android
- Kiểm tra SHA-1 certificate đã add vào Google Console
- Xem logcat: `adb logcat | grep Maps`

### **Lỗi: "Location permission denied"**
- Vào Settings → Apps → ShipperApp → Permissions
- Enable Location (Allow all the time cho background)

---

**Hệ thống tracking realtime đã hoàn chỉnh! 🎉**

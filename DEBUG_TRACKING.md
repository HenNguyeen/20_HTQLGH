# DEBUG TRACKING - HƯỚNG DẪN KIỂM TRA LOGCAT

## 🔍 Cách xem logs chi tiết

### Phương pháp 1: Android Studio Logcat (Đơn giản nhất)

1. Mở Android Studio
2. Chạy app trên emulator/device
3. Mở tab **Logcat** (phía dưới IDE)
4. Trong ô filter, gõ: `OrderDetail`
5. Bật switch "Chia sẻ vị trí" trong app
6. Quan sát logs

### Phương pháp 2: Command line (ADB)

```powershell
# Xóa logs cũ
adb logcat -c

# Xem logs real-time, chỉ filter OrderDetail
adb logcat -s OrderDetail:D

# Hoặc lưu vào file
adb logcat -s OrderDetail:D > tracking-logs.txt
```

---

## 📋 CÁC LOG MESSAGE CẦN TÌM

### Khi BẬT switch tracking:

```
D/OrderDetail: *** START TRACKING called for orderId: 123
D/OrderDetail: Location permission OK
D/OrderDetail: trackRunnable posted to handler - will run immediately
D/OrderDetail: === trackRunnable RUNNING === orderId: 123
D/OrderDetail: Got location: lat=10.7622, lng=106.6602
D/OrderDetail: Calling postCheckInWithLocation...
D/OrderDetail: >>> Sending checkpoint: orderId=123, lat=10.7622, lng=106.6602
D/OrderDetail: <<< AUTO CHECK-IN SUCCESS: 200, checkpointId=456
```

### Mỗi 30 giây sau đó:

```
D/OrderDetail: === trackRunnable RUNNING === orderId: 123
D/OrderDetail: Got location: lat=10.7623, lng=106.6603
D/OrderDetail: Calling postCheckInWithLocation...
D/OrderDetail: >>> Sending checkpoint: orderId=123, lat=10.7623, lng=106.6603
D/OrderDetail: <<< AUTO CHECK-IN SUCCESS: 200, checkpointId=457
```

### Khi TẮT switch:

```
D/OrderDetail: *** STOP TRACKING called
```

---

## ❌ CÁC LỖI THƯỜNG GẶP

### 1. Không thấy log nào cả
**Nguyên nhân**: App không chạy hoặc filter sai  
**Giải pháp**:
- Kiểm tra app đang chạy: `adb shell ps | grep shipperapp`
- Thử filter rộng hơn: `adb logcat | grep -i "track\|location\|checkpoint"`

### 2. Thấy "Location permission not granted"
**Nguyên nhân**: Chưa cấp quyền location  
**Giải pháp**:
- Settings → Apps → Shipper App → Permissions → Location → **Allow**
- Hoặc gỡ cài đặt và cài lại app, chấp nhận quyền khi được hỏi

### 3. Thấy "trackRunnable: lastLocation null"
**Nguyên nhân**: GPS chưa có tín hiệu  
**Giải pháp**:
- **Emulator**: Mở Extended Controls (⋮) → Location → Gửi tọa độ test
- **Device thật**: Ra ngoài trời, đợi 30-60s để GPS lock

### 4. Thấy "AUTO CHECK-IN FAILED: 400"
**Nguyên nhân**: Backend từ chối request (dữ liệu sai hoặc auth)  
**Giải pháp**:
- Xem thêm log error body phía dưới
- Kiểm tra token còn hạn không (login lại)
- Xem backend logs xem lỗi gì

### 5. Thấy "AUTO CHECK-IN NETWORK ERROR"
**Nguyên nhân**: Không kết nối được backend  
**Giải pháp**:
- Kiểm tra backend đang chạy: `http://localhost:5221`
- Emulator dùng URL: `http://10.0.2.2:5221`
- Device thật cần dùng IP máy tính (không dùng localhost)

---

## 🧪 TEST CASE ĐẦY ĐỦ

### Test 1: Bật tracking lần đầu
1. Mở app → Login
2. Vào order detail
3. **BẬT** switch
4. Đợi 5 giây

**Kết quả mong đợi**:
```
D/OrderDetail: *** START TRACKING called
D/OrderDetail: === trackRunnable RUNNING ===
D/OrderDetail: >>> Sending checkpoint
D/OrderDetail: <<< AUTO CHECK-IN SUCCESS: 200
```

Toast hiển thị: ✓ Vị trí đã gửi

---

### Test 2: Tracking liên tục
1. Giữ switch BẬT
2. Đợi 30 giây
3. Kiểm tra log

**Kết quả mong đợi**:
- Thấy log "=== trackRunnable RUNNING ===" **mỗi 30 giây**
- Thấy "<<< AUTO CHECK-IN SUCCESS" **mỗi 30 giây**
- Toast "✓ Vị trí đã gửi" hiện mỗi 30 giây

---

### Test 3: Tắt và bật lại
1. **TẮT** switch
2. Đợi 1 phút (không thấy log nào)
3. **BẬT** lại switch
4. Kiểm tra tracking tiếp tục

**Kết quả mong đợi**:
```
D/OrderDetail: *** STOP TRACKING called
(... không có log trong 1 phút ...)
D/OrderDetail: *** START TRACKING called
D/OrderDetail: === trackRunnable RUNNING ===
```

---

### Test 4: Di chuyển vị trí (Emulator)
1. Bật tracking
2. Mở Extended Controls → Location
3. Thay đổi lat/lng (ví dụ: di chuyển 0.001 độ)
4. Click "Send"
5. Đợi đến chu kỳ 30s tiếp theo

**Kết quả mong đợi**:
- Log hiển thị **lat/lng mới**
- Web UI (admin) thấy marker di chuyển

---

## 🔧 EMULATOR: GỬI TỌA ĐỘ TEST

### Cách 1: Extended Controls (GUI)
1. Emulator đang chạy
2. Click icon **⋮** (More) bên phải emulator
3. Chọn **Location**
4. Nhập tọa độ (VD: HCM = `10.762622, 106.660172`)
5. Click **Send**

### Cách 2: ADB Command
```powershell
# Gửi tọa độ HCM
adb emu geo fix 106.660172 10.762622

# Di chuyển một chút
adb emu geo fix 106.661172 10.763622

# Di chuyển liên tục (mô phỏng shipper đi)
adb emu geo fix 106.662 10.764
Start-Sleep 5
adb emu geo fix 106.663 10.765
Start-Sleep 5
adb emu geo fix 106.664 10.766
```

---

## 📱 DEVICE THẬT: THAY ĐỔI BASE_URL

Nếu test trên device thật (không phải emulator):

### Cách 1: Dùng IP máy tính
1. Xem IP máy tính:
```powershell
ipconfig
# Tìm IPv4 Address (VD: 192.168.1.100)
```

2. Sửa file `OrderDetailActivity.java`:
```java
// Thay đổi từ:
private final String BASE_URL = "http://10.0.2.2:5221/";

// Thành:
private final String BASE_URL = "http://192.168.1.100:5221/";
```

3. Rebuild app

### Cách 2: Dùng ngrok (nếu cần)
```powershell
ngrok http 5221
# Lấy URL public (VD: https://abc123.ngrok.io)
# Cập nhật BASE_URL trong app
```

---

## ✅ CHECKLIST HOÀN CHỈNH

Trước khi báo lỗi, hãy kiểm tra:

- [ ] Backend đang chạy: `dotnet run` trong DeliveryManagementAPI
- [ ] App đã rebuild sau khi sửa code
- [ ] Đã login vào app
- [ ] Đã cấp quyền Location cho app
- [ ] GPS đã bật (Settings → Location)
- [ ] Emulator: Đã gửi tọa độ test qua Extended Controls
- [ ] Device thật: Đã sửa BASE_URL sang IP máy tính
- [ ] Logcat filter đang chạy: `adb logcat -s OrderDetail:D`
- [ ] Switch "Chia sẻ vị trí" đã BẬT

---

## 🆘 NẾU VẪN KHÔNG HOẠT ĐỘNG

### Gửi cho tôi thông tin sau:

1. **Full Logcat** (từ khi bật switch đến 1 phút sau):
```powershell
adb logcat -d > full-log.txt
```

2. **Backend logs** (console output của `dotnet run`)

3. **Screenshots**:
   - Màn hình app khi bật switch
   - Toast messages hiển thị

4. **Môi trường**:
   - Emulator hay device thật?
   - Android version?
   - Đã thử trên ngoài trời chưa (nếu device thật)?

Dán logs vào đây và tôi sẽ phân tích ngay! 🚀

# Tài Liệu Kiểm Thử Hệ Thống Quản Lý Giao Hàng
# Testing Documentation - Delivery Management System

**Dự án:** Hệ Thống Quản Lý Giao Hàng (Delivery Management System)  
**Phiên bản:** 1.0  
**Ngày tạo:** 9 tháng 3, 2026  
**Môn học:** Bảo Đảm Chất Lượng Phần Mềm

---

## Mục Lục

1. [System Modules (Các Module Hệ Thống)](#1-system-modules)
2. [Requirement Analysis (Phân Tích Yêu Cầu)](#2-requirement-analysis)
   - [Functional Requirements](#21-functional-requirements-fr)
   - [Non-Functional Requirements](#22-non-functional-requirements-nfr)
3. [Test Scenarios](#3-test-scenarios)

---

## 1. System Modules

Hệ thống Quản Lý Giao Hàng được chia thành 13 module chính:

### 1.1. Authentication & Security Module (Module Xác Thực & Bảo Mật)

**Mô tả:** Quản lý đăng nhập, đăng ký, xác thực hai yếu tố (2FA), đăng nhập Google OAuth, và quản lý phiên làm việc của người dùng.

**Chức năng chính:**
- Đăng nhập với username/password
- Đăng ký tài khoản mới
- Đăng nhập qua Google OAuth2
- Xác thực hai yếu tố (2FA) qua email
- Quên mật khẩu và đặt lại mật khẩu
- Quản lý JWT token
- Phân quyền theo vai trò (Admin, Customer, Shipper)

---

### 1.2. Order Management Module (Module Quản Lý Đơn Hàng)

**Mô tả:** Quản lý toàn bộ vòng đời của đơn hàng từ lúc tạo đến khi hoàn thành hoặc hủy.

**Chức năng chính:**
- Tạo đơn hàng mới (đơn lẻ hoặc nhập từ Excel)
- Xem danh sách đơn hàng (tất cả, của tôi, theo trạng thái)
- Xem chi tiết đơn hàng
- Cập nhật thông tin đơn hàng
- Cập nhật trạng thái đơn hàng (Chưa nhận → Đã nhận chưa giao → Đang giao → Đã giao)
- Phân công đơn hàng cho shipper
- Xác nhận thanh toán
- Xác nhận đã nhận hàng (bởi khách hàng)
- Xóa đơn hàng
- Tính phí giao hàng tự động

---

### 1.3. Customer Management Module (Module Quản Lý Khách Hàng)

**Mô tả:** Quản lý thông tin khách hàng sử dụng dịch vụ giao hàng.

**Chức năng chính:**
- Thêm khách hàng mới
- Xem danh sách khách hàng
- Xem chi tiết thông tin khách hàng
- Cập nhật thông tin khách hàng
- Xóa khách hàng
- Lưu trữ lịch sử giao dịch của khách hàng

---

### 1.4. Delivery Staff Management Module (Module Quản Lý Nhân Viên Giao Hàng)

**Mô tả:** Quản lý thông tin nhân viên giao hàng (shipper) và theo dõi trạng thái làm việc của họ.

**Chức năng chính:**
- Thêm nhân viên giao hàng mới
- Xem danh sách nhân viên giao hàng
- Xem danh sách nhân viên đang rảnh/có sẵn
- Xem chi tiết thông tin nhân viên
- Cập nhật thông tin nhân viên
- Xóa nhân viên giao hàng
- Xem thông tin nhân viên hiện tại (cho shipper đăng nhập)
- Theo dõi số đơn đã giao và đánh giá của shipper

---

### 1.5. User Account Management Module (Module Quản Lý Tài Khoản Người Dùng)

**Mô tả:** Quản lý tài khoản người dùng hệ thống với các vai trò khác nhau.

**Chức năng chính:**
- Tạo tài khoản người dùng mới
- Xem danh sách tài khoản
- Xem chi tiết tài khoản
- Cập nhật thông tin tài khoản
- Phân quyền vai trò (Admin, Customer, Shipper)
- Xóa tài khoản
- Kích hoạt/Vô hiệu hóa tài khoản

---

### 1.6. Notification Module (Module Thông Báo)

**Mô tả:** Quản lý thông báo realtime cho người dùng về các hoạt động liên quan đến đơn hàng và hệ thống.

**Chức năng chính:**
- Gửi thông báo realtime qua SignalR
- Xem danh sách thông báo
- Đếm số thông báo chưa đọc
- Đánh dấu thông báo đã đọc (đơn lẻ hoặc tất cả)
- Xóa thông báo
- Cài đặt tùy chọn thông báo (Email, SMS, Push, In-App)
- Phân loại thông báo (Order, Payment, Delivery, Chat, System, Feedback, Promotion)

---

### 1.7. Chat Module (Module Trò Chuyện)

**Mô tả:** Hỗ trợ giao tiếp realtime giữa khách hàng, admin và shipper.

**Chức năng chính:**
- Gửi tin nhắn text
- Gửi hình ảnh (tối đa 5MB)
- Chat theo đơn hàng cụ thể
- Chat hỗ trợ chung (General Support)
- Xem danh sách cuộc hội thoại (cho Admin)
- Đếm tin nhắn chưa đọc
- Broadcasting tin nhắn realtime qua SignalR

---

### 1.8. Chatbot Module (Module Chatbot Tự Động)

**Mô tả:** Tích hợp Dialogflow để cung cấp hỗ trợ tự động cho khách hàng.

**Chức năng chính:**
- Tra cứu đơn hàng qua chatbot
- Kiểm tra trạng thái đơn hàng
- Kiểm tra thông tin và tình trạng shipper
- Webhook tích hợp với Dialogflow
- Trả lời tự động với emoji và nội dung thân thiện

---

### 1.9. Tracking & Location Module (Module Theo Dõi Vị Trí)

**Mô tả:** Theo dõi vị trí shipper và trạng thái giao hàng realtime.

**Chức năng chính:**
- Theo dõi vị trí shipper realtime
- Lưu lịch sử checkpoint vị trí
- Xem lịch sử di chuyển của đơn hàng
- Tracking công khai không cần đăng nhập (qua mã đơn hàng)
- Cập nhật vị trí tự động từ ứng dụng mobile
- Broadcasting vị trí realtime qua SignalR

---

### 1.10. Feedback & Rating Module (Module Đánh Giá & Phản Hồi)

**Mô tả:** Cho phép khách hàng đánh giá dịch vụ giao hàng và shipper.

**Chức năng chính:**
- Gửi feedback cho đơn hàng
- Đánh giá shipper (1-5 sao)
- Xem danh sách feedback của đơn hàng
- Xem feedback của tôi
- Xem tất cả feedback (Admin)
- Xem đánh giá trung bình của shipper
- Kiểm tra quyền đánh giá (chỉ người tạo đơn)

---

### 1.11. Report & Analytics Module (Module Báo Cáo & Thống Kê)

**Mô tả:** Cung cấp báo cáo thống kê về hoạt động kinh doanh và hiệu suất giao hàng.

**Chức năng chính:**
- Dashboard tổng quan (đơn hàng hôm nay, doanh thu, tỷ lệ thành công)
- Thống kê đơn hàng theo trạng thái
- Thống kê đơn hàng theo ngày (7-30 ngày)
- Thống kê theo nhân viên giao hàng
- Thống kê theo loại giao hàng (Thường/Nhanh)
- Thống kê theo loại hàng hóa
- Thống kê theo địa điểm
- Xuất báo cáo CSV
- Biểu đồ xu hướng đơn hàng

---

### 1.12. Payment Module (Module Thanh Toán)

**Mô tả:** Quản lý thanh toán và xác nhận giao dịch cho đơn hàng.

**Chức năng chính:**
- Hỗ trợ nhiều phương thức thanh toán (Gửi thường, Gửi nhanh, Chuyển khoản, Thanh toán trực tuyến)
- Xác nhận thanh toán đơn hàng
- Theo dõi trạng thái thanh toán
- Lưu thời gian thanh toán
- Tính phí dựa trên khoảng cách, trọng lượng, loại hàng
- Thu tiền hộ (COD)

---

### 1.13. Profile Management Module (Module Quản Lý Thông Tin Cá Nhân)

**Mô tả:** Cho phép người dùng quản lý thông tin cá nhân của họ.

**Chức năng chính:**
- Xem thông tin cá nhân
- Cập nhật thông tin cá nhân
- Đổi mật khẩu
- Cài đặt xác thực hai yếu tố (2FA)
- Bật/tắt 2FA

---

## 2. Requirement Analysis

### 2.1. Functional Requirements (FR)

#### **FR01 - Đăng nhập hệ thống**
Người dùng có thể đăng nhập vào hệ thống bằng username và password. Hệ thống phải xác thực thông tin và trả về JWT token nếu hợp lệ.

#### **FR02 - Đăng ký tài khoản**
Người dùng mới có thể đăng ký tài khoản với vai trò Customer. Hệ thống phải kiểm tra username và email không bị trùng lặp.

#### **FR03 - Đăng nhập Google OAuth**
Người dùng có thể đăng nhập bằng tài khoản Google. Hệ thống tự động tạo tài khoản mới nếu lần đầu đăng nhập.

#### **FR04 - Xác thực hai yếu tố (2FA)**
Người dùng có thể bật 2FA và nhận mã OTP qua email. Hệ thống phải gửi mã OTP 6 chữ số có thời hạn 5 phút.

#### **FR05 - Quên mật khẩu**
Người dùng có thể yêu cầu đặt lại mật khẩu qua email. Hệ thống gửi token reset có thời hạn 15 phút.

#### **FR06 - Đặt lại mật khẩu**
Người dùng có thể đặt lại mật khẩu bằng token nhận được qua email.

#### **FR07 - Tạo đơn hàng**
Admin và Customer có thể tạo đơn hàng mới. Hệ thống tự động tính phí giao hàng dựa trên khoảng cách, trọng lượng, và loại hàng.

#### **FR08 - Xem danh sách đơn hàng**
Người dùng có thể xem danh sách đơn hàng theo quyền: Admin xem tất cả, Customer xem đơn của mình, Shipper xem đơn được phân công.

#### **FR09 - Xem chi tiết đơn hàng**
Người dùng có thể xem thông tin chi tiết của một đơn hàng bao gồm: khách hàng, hàng hóa, phí, trạng thái, shipper được phân công.

#### **FR10 - Cập nhật trạng thái đơn hàng**
Admin và Shipper có thể cập nhật trạng thái đơn hàng (Chưa nhận → Đã nhận chưa giao → Đang giao → Đã giao).

#### **FR11 - Phân công đơn hàng cho shipper**
Admin có thể phân công đơn hàng cho shipper có sẵn. Hệ thống gửi thông báo cho shipper khi được phân công.

#### **FR12 - Nhập đơn hàng từ Excel**
Admin có thể nhập hàng loạt đơn hàng từ file Excel.

#### **FR13 - Xóa đơn hàng**
Admin có thể xóa đơn hàng khỏi hệ thống.

#### **FR14 - Xác nhận thanh toán**
Admin có thể xác nhận đơn hàng đã thanh toán. Hệ thống cập nhật trạng thái IsPaid và thời gian thanh toán.

#### **FR15 - Xác nhận đã nhận hàng**
Khách hàng có thể xác nhận đã nhận được hàng. Hệ thống lưu thời gian xác nhận.

#### **FR16 - Quản lý khách hàng (CRUD)**
Admin có thể thêm, xem, sửa, xóa thông tin khách hàng.

#### **FR17 - Quản lý nhân viên giao hàng (CRUD)**
Admin có thể thêm, xem, sửa, xóa thông tin nhân viên giao hàng.

#### **FR18 - Xem danh sách shipper có sẵn**
Hệ thống hiển thị danh sách shipper đang rảnh để phân công đơn hàng.

#### **FR19 - Quản lý tài khoản người dùng**
Admin có thể tạo, xem, sửa, xóa tài khoản người dùng và phân quyền vai trò.

#### **FR20 - Gửi và nhận tin nhắn chat**
Người dùng có thể gửi và nhận tin nhắn realtime. Hỗ trợ text và hình ảnh (tối đa 5MB).

#### **FR21 - Chat theo đơn hàng**
Người dùng có thể chat trong ngữ cảnh của một đơn hàng cụ thể để thảo luận về giao hàng.

#### **FR22 - Sử dụng chatbot**
Khách hàng có thể sử dụng chatbot để tra cứu đơn hàng, kiểm tra trạng thái, và kiểm tra thông tin shipper.

#### **FR23 - Theo dõi vị trí shipper realtime**
Khách hàng và Admin có thể xem vị trí shipper realtime trên bản đồ khi đơn hàng đang được giao.

#### **FR24 - Xem lịch sử checkpoint**
Người dùng có thể xem lịch sử các điểm check-in của shipper trong quá trình giao hàng.

#### **FR25 - Tracking công khai bằng mã đơn hàng**
Bất kỳ ai có mã đơn hàng có thể tra cứu trạng thái giao hàng mà không cần đăng nhập.

#### **FR26 - Đánh giá đơn hàng và shipper**
Khách hàng (người tạo đơn) có thể đánh giá đơn hàng và shipper từ 1-5 sao kèm nhận xét.

#### **FR27 - Xem thông báo**
Người dùng có thể xem danh sách thông báo của họ, bao gồm thông báo về đơn hàng, thanh toán, giao hàng, chat.

#### **FR28 - Đánh dấu thông báo đã đọc**
Người dùng có thể đánh dấu một hoặc tất cả thông báo là đã đọc.

#### **FR29 - Cài đặt thông báo**
Người dùng có thể cài đặt cách nhận thông báo (Email, SMS, Push, In-App) cho từng loại thông báo.

#### **FR30 - Xem báo cáo và thống kê**
Admin có thể xem dashboard và các báo cáo về đơn hàng, doanh thu, hiệu suất shipper, xu hướng đơn hàng theo thời gian.

#### **FR31 - Xuất báo cáo CSV**
Admin có thể xuất báo cáo dưới dạng file CSV với các bộ lọc tùy chỉnh.

#### **FR32 - Quản lý thông tin cá nhân**
Người dùng có thể xem và cập nhật thông tin cá nhân của họ (tên, email, số điện thoại).

#### **FR33 - Đổi mật khẩu**
Người dùng đã đăng nhập có thể đổi mật khẩu bằng cách nhập mật khẩu cũ và mật khẩu mới.

---

### 2.2. Non-Functional Requirements (NFR)

#### **NFR01 - Bảo mật (Security)**
- Mật khẩu phải được hash bằng SHA256 trước khi lưu vào database
- JWT token có thời hạn 60 phút
- API endpoints phải có phân quyền theo vai trò (Role-based Authorization)
- Hỗ trợ xác thực hai yếu tố (2FA) để tăng cường bảo mật
- HTTPS phải được sử dụng cho tất cả giao tiếp giữa client và server
- Chống Cross-Site Scripting (XSS) và SQL Injection

#### **NFR02 - Hiệu năng (Performance)**
- Trang web phải tải trong vòng 3 giây với kết nối internet trung bình
- API response time không quá 500ms cho các query đơn giản
- Hỗ trợ pagination cho danh sách lớn (mặc định 20 items/page)
- Realtime updates qua SignalR không gây delay > 1 giây
- Hệ thống phải xử lý được ít nhất 100 concurrent users

#### **NFR03 - Khả năng sử dụng (Usability)**
- Giao diện thân thiện, dễ sử dụng cho người dùng không có kinh nghiệm kỹ thuật
- Hỗ trợ responsive design cho mobile, tablet, desktop
- Thông báo lỗi phải rõ ràng và hướng dẫn người dùng khắc phục
- Hỗ trợ dark mode và light mode
- Form validation realtime với thông báo lỗi tức thì

#### **NFR04 - Tương thích (Compatibility)**
- Hỗ trợ các trình duyệt: Chrome, Firefox, Edge, Safari (phiên bản mới nhất và 2 phiên bản trước đó)
- Responsive design hoạt động tốt trên màn hình từ 320px đến 2560px
- API tuân theo chuẩn RESTful
- Tương thích với Android và iOS cho tính năng tracking

#### **NFR05 - Độ tin cậy (Reliability)**
- Uptime > 99% trong giờ làm việc
- Dữ liệu phải được backup định kỳ (hàng ngày)
- Hệ thống phải có logging để theo dõi lỗi và debug
- Graceful degradation khi một số service không hoạt động (ví dụ: SignalR, chatbot)

#### **NFR06 - Khả năng mở rộng (Scalability)**
- Kiến trúc phải hỗ trợ horizontal scaling (thêm server)
- Database phải được thiết kế để xử lý tăng trưởng dữ liệu
- SignalR có thể mở rộng với Redis backplane nếu cần
- Microservices-ready architecture

#### **NFR07 - Khả năng bảo trì (Maintainability)**
- Code phải tuân theo design patterns (Repository, Service Layer, DI, etc.)
- Có documentation đầy đủ cho API (Swagger/OpenAPI)
- Code phải có comments và naming conventions rõ ràng
- Unit tests coverage ít nhất 60% cho business logic

#### **NFR08 - Tính khả dụng (Availability)**
- Thông báo realtime phải hoạt động liên tục
- Hệ thống tracking phải hoạt động 24/7
- Chatbot phải có fallback khi Dialogflow không khả dụng

---

## 3. Test Scenarios

### 3.1. Authentication & Security Module

#### **SC001 - Đăng nhập thành công với tài khoản hợp lệ**
**Module:** Authentication  
**Mô tả:** Người dùng nhập username và password đúng, hệ thống cho phép đăng nhập và chuyển hướng đến trang dashboard.

#### **SC002 - Đăng nhập thất bại với mật khẩu sai**
**Module:** Authentication  
**Mô tả:** Người dùng nhập username đúng nhưng password sai, hệ thống hiển thị thông báo lỗi "Sai tài khoản hoặc mật khẩu".

#### **SC003 - Đăng nhập thất bại với username không tồn tại**
**Module:** Authentication  
**Mô tả:** Người dùng nhập username không tồn tại trong hệ thống, hệ thống hiển thị thông báo lỗi.

#### **SC004 - Đăng ký tài khoản mới thành công**
**Module:** Authentication  
**Mô tả:** Người dùng điền đầy đủ thông tin hợp lệ (username, email, password, họ tên, số điện thoại) và đăng ký thành công.

#### **SC005 - Đăng ký thất bại với username đã tồn tại**
**Module:** Authentication  
**Mô tả:** Người dùng nhập username đã có trong hệ thống, hệ thống hiển thị thông báo "Tên đăng nhập đã tồn tại".

#### **SC006 - Đăng ký thất bại với email đã tồn tại**
**Module:** Authentication  
**Mô tả:** Người dùng nhập email đã được sử dụng, hệ thống hiển thị thông báo "Email đã tồn tại".

#### **SC007 - Đăng nhập qua Google OAuth thành công**
**Module:** Authentication  
**Mô tả:** Người dùng click nút "Sign in with Google", chọn tài khoản Google và đăng nhập thành công vào hệ thống.

#### **SC008 - Đăng nhập với 2FA được bật**
**Module:** Authentication  
**Mô tả:** Người dùng có 2FA nhập username/password đúng, hệ thống yêu cầu nhập mã OTP được gửi qua email.

#### **SC009 - Xác thực 2FA thành công**
**Module:** Authentication  
**Mô tả:** Người dùng nhập mã OTP đúng trong vòng 5 phút, hệ thống cho phép đăng nhập.

#### **SC010 - Xác thực 2FA thất bại với mã OTP sai**
**Module:** Authentication  
**Mô tả:** Người dùng nhập mã OTP sai, hệ thống hiển thị thông báo lỗi "Mã xác thực không đúng".

#### **SC011 - Xác thực 2FA thất bại với mã OTP hết hạn**
**Module:** Authentication  
**Mô tả:** Người dùng nhập mã OTP sau 5 phút, hệ thống hiển thị thông báo "Mã xác thực đã hết hạn".

#### **SC012 - Yêu cầu đặt lại mật khẩu**
**Module:** Authentication  
**Mô tả:** Người dùng quên mật khẩu, nhập email, hệ thống gửi token reset qua email.

#### **SC013 - Đặt lại mật khẩu thành công**
**Module:** Authentication  
**Mô tả:** Người dùng click link trong email, nhập mật khẩu mới và xác nhận, hệ thống cập nhật mật khẩu.

#### **SC014 - Đặt lại mật khẩu thất bại với token hết hạn**
**Module:** Authentication  
**Mô tả:** Người dùng sử dụng token reset sau 15 phút, hệ thống hiển thị "Token không hợp lệ hoặc đã hết hạn".

#### **SC015 - Đăng xuất khỏi hệ thống**
**Module:** Authentication  
**Mô tả:** Người dùng đã đăng nhập click nút Logout, hệ thống xóa session và chuyển về trang login.

---

### 3.2. Order Management Module

#### **SC016 - Tạo đơn hàng mới thành công**
**Module:** Order Management  
**Mô tả:** Admin/Customer điền đầy đủ thông tin đơn hàng (khách hàng, hàng hóa, địa chỉ, trọng lượng, khoảng cách), hệ thống tự động tính phí và tạo đơn hàng.

#### **SC017 - Tạo đơn hàng thất bại với thông tin thiếu**
**Module:** Order Management  
**Mô tả:** Người dùng bỏ trống các trường bắt buộc, hệ thống hiển thị thông báo validation errors.

#### **SC018 - Xem danh sách tất cả đơn hàng (Admin)**
**Module:** Order Management  
**Mô tả:** Admin truy cập trang Orders, hệ thống hiển thị danh sách tất cả đơn hàng với pagination.

#### **SC019 - Xem danh sách đơn hàng của tôi (Customer)**
**Module:** Order Management  
**Mô tả:** Customer đăng nhập, xem danh sách chỉ những đơn hàng do mình tạo.

#### **SC020 - Xem danh sách đơn hàng được phân công (Shipper)**
**Module:** Order Management  
**Mô tả:** Shipper đăng nhập, xem danh sách đơn hàng được phân công cho mình.

#### **SC021 - Xem chi tiết đơn hàng**
**Module:** Order Management  
**Mô tả:** Người dùng click vào một đơn hàng, hệ thống hiển thị thông tin chi tiết bao gồm: khách hàng, hàng hóa, phí, trạng thái, shipper, timeline.

#### **SC022 - Lọc đơn hàng theo trạng thái**
**Module:** Order Management  
**Mô tả:** Người dùng chọn trạng thái (Chưa nhận, Đang giao, Đã giao), hệ thống hiển thị danh sách đơn hàng theo trạng thái đó.

#### **SC023 - Tìm kiếm đơn hàng theo mã đơn hàng**
**Module:** Order Management  
**Mô tả:** Người dùng nhập mã đơn hàng vào ô tìm kiếm, hệ thống hiển thị đơn hàng tương ứng.

#### **SC024 - Cập nhật trạng thái đơn hàng thành "Đã nhận chưa giao"**
**Module:** Order Management  
**Mô tả:** Admin/Shipper chọn đơn hàng, cập nhật trạng thái thành "Đã nhận chưa giao", hệ thống lưu thời gian nhận hàng.

#### **SC025 - Cập nhật trạng thái đơn hàng thành "Đang giao"**
**Module:** Order Management  
**Mô tả:** Shipper cập nhật trạng thái thành "Đang giao", hệ thống lưu thời gian bắt đầu giao và gửi thông báo cho khách hàng.

#### **SC026 - Cập nhật trạng thái đơn hàng thành "Đã giao"**
**Module:** Order Management  
**Mô tả:** Shipper cập nhật trạng thái thành "Đã giao", hệ thống lưu thời gian giao thành công.

#### **SC027 - Phân công đơn hàng cho shipper có sẵn**
**Module:** Order Management  
**Mô tả:** Admin chọn đơn hàng, chọn shipper từ danh sách shipper rảnh, hệ thống phân công và gửi thông báo cho shipper.

#### **SC028 - Phân công đơn hàng thất bại khi không có shipper rảnh**
**Module:** Order Management  
**Mô tả:** Admin cố gắng phân công đơn hàng nhưng không có shipper nào rảnh, hệ thống hiển thị thông báo.

#### **SC029 - Xác nhận thanh toán đơn hàng**
**Module:** Order Management  
**Mô tả:** Admin click nút "Xác nhận thanh toán", hệ thống cập nhật trạng thái IsPaid=true và lưu thời gian thanh toán.

#### **SC030 - Xác nhận đã nhận hàng bởi khách hàng**
**Module:** Order Management  
**Mô tả:** Khách hàng (người tạo đơn) click nút "Xác nhận đã nhận hàng", hệ thống lưu thời gian xác nhận.

#### **SC031 - Nhập đơn hàng từ file Excel**
**Module:** Order Management  
**Mô tả:** Admin upload file Excel chứa nhiều đơn hàng, hệ thống đọc và tạo tất cả đơn hàng trong file.

#### **SC032 - Nhập đơn hàng thất bại với file Excel sai định dạng**
**Module:** Order Management  
**Mô tả:** Admin upload file Excel không đúng format, hệ thống hiển thị thông báo lỗi và danh sách các dòng bị lỗi.

#### **SC033 - Xóa đơn hàng**
**Module:** Order Management  
**Mô tả:** Admin chọn đơn hàng và click nút Delete, hệ thống hiển thị confirm dialog, sau khi xác nhận đơn hàng bị xóa.

#### **SC034 - Tính phí giao hàng tự động**
**Module:** Order Management  
**Mô tả:** Khi nhập thông tin đơn hàng (khoảng cách, trọng lượng, loại hàng), hệ thống tự động tính và hiển thị phí giao hàng.

---

### 3.3. Customer Management Module

#### **SC035 - Thêm khách hàng mới thành công**
**Module:** Customer Management  
**Mô tả:** Admin điền thông tin khách hàng (tên, SĐT, địa chỉ, email) và click Save, hệ thống tạo khách hàng mới.

#### **SC036 - Thêm khách hàng thất bại với số điện thoại trùng**
**Module:** Customer Management  
**Mô tả:** Admin nhập số điện thoại đã tồn tại, hệ thống hiển thị thông báo lỗi.

#### **SC037 - Xem danh sách khách hàng**
**Module:** Customer Management  
**Mô tả:** Admin truy cập trang Customers, hệ thống hiển thị danh sách tất cả khách hàng với pagination.

#### **SC038 - Xem chi tiết thông tin khách hàng**
**Module:** Customer Management  
**Mô tả:** Admin click vào một khách hàng, hệ thống hiển thị thông tin chi tiết và lịch sử đơn hàng.

#### **SC039 - Cập nhật thông tin khách hàng**
**Module:** Customer Management  
**Mô tả:** Admin sửa thông tin khách hàng và click Save, hệ thống cập nhật thông tin.

#### **SC040 - Xóa khách hàng**
**Module:** Customer Management  
**Mô tả:** Admin chọn khách hàng và click Delete, sau khi xác nhận khách hàng bị xóa khỏi hệ thống.

#### **SC041 - Tìm kiếm khách hàng theo tên**
**Module:** Customer Management  
**Mô tả:** Admin nhập tên khách hàng vào ô tìm kiếm, hệ thống hiển thị danh sách khách hàng phù hợp.

#### **SC042 - Tìm kiếm khách hàng theo số điện thoại**
**Module:** Customer Management  
**Mô tả:** Admin nhập số điện thoại, hệ thống tìm và hiển thị khách hàng có số điện thoại đó.

---

### 3.4. Delivery Staff Management Module

#### **SC043 - Thêm nhân viên giao hàng mới**
**Module:** Delivery Staff Management  
**Mô tả:** Admin điền thông tin shipper (tên, SĐT, email, khu vực hoạt động) và click Save, hệ thống tạo shipper và tài khoản tương ứng.

#### **SC044 - Xem danh sách tất cả nhân viên giao hàng**
**Module:** Delivery Staff Management  
**Mô tả:** Admin truy cập trang Staff, hệ thống hiển thị danh sách tất cả shipper.

#### **SC045 - Xem danh sách shipper đang rảnh**
**Module:** Delivery Staff Management  
**Mô tả:** Admin click tab "Available", hệ thống hiển thị danh sách shipper đang không có đơn hàng hoặc đã giao xong.

#### **SC046 - Xem chi tiết thông tin nhân viên giao hàng**
**Module:** Delivery Staff Management  
**Mô tả:** Admin click vào một shipper, hệ thống hiển thị thông tin chi tiết, số đơn đã giao, đánh giá trung bình.

#### **SC047 - Cập nhật thông tin nhân viên giao hàng**
**Module:** Delivery Staff Management  
**Mô tả:** Admin sửa thông tin shipper và click Save, hệ thống cập nhật thông tin.

#### **SC048 - Xóa nhân viên giao hàng**
**Module:** Delivery Staff Management  
**Mô tả:** Admin chọn shipper và click Delete, sau khi xác nhận shipper bị xóa.

#### **SC049 - Xem thông tin cá nhân của shipper đăng nhập**
**Module:** Delivery Staff Management  
**Mô tả:** Shipper đăng nhập và truy cập trang Profile, hệ thống hiển thị thông tin cá nhân, đánh giá, số đơn đã giao.

---

### 3.5. User Account Management Module

#### **SC050 - Tạo tài khoản người dùng mới (Admin)**
**Module:** User Account Management  
**Mô tả:** Admin tạo tài khoản với username, password, role, hệ thống tạo tài khoản mới.

#### **SC051 - Xem danh sách tài khoản người dùng**
**Module:** User Account Management  
**Mô tả:** Admin truy cập trang Accounts, hệ thống hiển thị danh sách tất cả tài khoản.

#### **SC052 - Phân quyền vai trò cho tài khoản**
**Module:** User Account Management  
**Mô tả:** Admin chọn tài khoản, thay đổi role (Admin/Customer/Shipper) và lưu, hệ thống cập nhật vai trò.

#### **SC053 - Xóa tài khoản người dùng**
**Module:** User Account Management  
**Mô tả:** Admin chọn tài khoản và click Delete, sau khi xác nhận tài khoản bị xóa.

---

### 3.6. Notification Module

#### **SC054 - Nhận thông báo realtime khi có đơn hàng mới**
**Module:** Notification  
**Mô tả:** Khi admin tạo đơn hàng mới, shipper nhận được thông báo realtime ngay lập tức mà không cần refresh trang.

#### **SC055 - Nhận thông báo khi đơn hàng được phân công**
**Module:** Notification  
**Mô tả:** Khi admin phân công đơn hàng cho shipper, shipper nhận thông báo realtime về đơn hàng mới.

#### **SC056 - Nhận thông báo khi trạng thái đơn hàng thay đổi**
**Module:** Notification  
**Mô tả:** Khi shipper cập nhật trạng thái đơn hàng, khách hàng nhận thông báo realtime về thay đổi.

#### **SC057 - Xem danh sách thông báo**
**Module:** Notification  
**Mô tả:** Người dùng click vào icon thông báo, hệ thống hiển thị danh sách thông báo với pagination.

#### **SC058 - Xem số lượng thông báo chưa đọc**
**Module:** Notification  
**Mô tả:** Hệ thống hiển thị badge số lượng thông báo chưa đọc trên icon thông báo.

#### **SC059 - Đánh dấu một thông báo đã đọc**
**Module:** Notification  
**Mô tả:** Người dùng click vào một thông báo, hệ thống đánh dấu thông báo đó là đã đọc.

#### **SC060 - Đánh dấu tất cả thông báo đã đọc**
**Module:** Notification  
**Mô tả:** Người dùng click nút "Mark all as read", hệ thống đánh dấu tất cả thông báo là đã đọc.

#### **SC061 - Xóa thông báo**
**Module:** Notification  
**Mô tả:** Người dùng click nút Delete trên một thông báo, thông báo bị xóa khỏi danh sách.

#### **SC062 - Cài đặt tùy chọn nhận thông báo**
**Module:** Notification  
**Mô tả:** Người dùng truy cập Settings > Notifications, chọn cách nhận thông báo (Email, SMS, Push, In-App) cho từng loại thông báo.

---

### 3.7. Chat Module

#### **SC063 - Gửi tin nhắn text trong chat**
**Module:** Chat  
**Mô tả:** Người dùng nhập tin nhắn và click Send, tin nhắn được gửi và hiển thị realtime cho người nhận.

#### **SC064 - Gửi hình ảnh trong chat**
**Module:** Chat  
**Mô tả:** Người dùng chọn hình ảnh (dưới 5MB), upload và gửi, hình ảnh hiển thị trong chat.

#### **SC065 - Gửi hình ảnh thất bại với file quá lớn**
**Module:** Chat  
**Mô tả:** Người dùng chọn hình ảnh trên 5MB, hệ thống hiển thị thông báo lỗi "File quá lớn".

#### **SC066 - Chat theo đơn hàng cụ thể**
**Module:** Chat  
**Mô tả:** Người dùng mở chi tiết đơn hàng và click tab Chat, hệ thống hiển thị chat trong ngữ cảnh đơn hàng đó.

#### **SC067 - Chat hỗ trợ chung (General Support)**
**Module:** Chat  
**Mô tả:** Khách hàng click vào "Support Chat", hệ thống mở chat với admin để hỗ trợ chung.

#### **SC068 - Xem danh sách cuộc hội thoại (Admin)**
**Module:** Chat  
**Mô tả:** Admin truy cập trang Messages, hệ thống hiển thị danh sách người dùng đã chat với số tin nhắn chưa đọc.

#### **SC069 - Xem số tin nhắn chưa đọc**
**Module:** Chat  
**Mô tả:** Hệ thống hiển thị badge số tin nhắn chưa đọc trên icon chat.

#### **SC070 - Nhận tin nhắn realtime**
**Module:** Chat  
**Mô tả:** Khi có tin nhắn mới, người nhận nhận được tin nhắn realtime mà không cần refresh trang.

---

### 3.8. Chatbot Module

#### **SC071 - Sử dụng chatbot để tra cứu đơn hàng**
**Module:** Chatbot  
**Mô tả:** Người dùng chat với bot "Tôi muốn tra cứu đơn hàng DH20260309001", bot trả về thông tin đơn hàng.

#### **SC072 - Sử dụng chatbot để kiểm tra trạng thái đơn hàng**
**Module:** Chatbot  
**Mô tả:** Người dùng chat với bot "Kiểm tra trạng thái đơn hàng của tôi", bot trả về trạng thái hiện tại của đơn hàng.

#### **SC073 - Sử dụng chatbot để kiểm tra shipper**
**Module:** Chatbot  
**Mô tả:** Người dùng chat với bot "Kiểm tra shipper của đơn hàng", bot trả về thông tin shipper được phân công.

#### **SC074 - Chatbot trả lời khi không hiểu câu hỏi**
**Module:** Chatbot  
**Mô tả:** Người dùng hỏi câu hỏi ngoài phạm vi, bot trả lời "Xin lỗi, tôi chưa hiểu câu hỏi của bạn".

---

### 3.9. Tracking & Location Module

#### **SC075 - Xem vị trí shipper realtime trên bản đồ**
**Module:** Tracking  
**Mô tả:** Khách hàng mở trang Tracking, hệ thống hiển thị vị trí shipper realtime trên bản đồ (Google Maps).

#### **SC076 - Shipper cập nhật vị trí**
**Module:** Tracking  
**Mô tả:** Shipper di chuyển, ứng dụng mobile tự động gửi vị trí mới, vị trí cập nhật realtime trên bản đồ.

#### **SC077 - Xem lịch sử checkpoint của đơn hàng**
**Module:** Tracking  
**Mô tả:** Người dùng xem chi tiết đơn hàng, hệ thống hiển thị timeline các checkpoint (thời gian, vị trí, ghi chú).

#### **SC078 - Tracking công khai bằng mã đơn hàng**
**Module:** Tracking  
**Mô tả:** Người dùng chưa đăng nhập nhập mã đơn hàng vào trang Tracking, hệ thống hiển thị thông tin tracking.

#### **SC079 - Tracking thất bại với mã đơn hàng không tồn tại**
**Module:** Tracking  
**Mô tả:** Người dùng nhập mã đơn hàng không tồn tại, hệ thống hiển thị "Không tìm thấy đơn hàng".

---

### 3.10. Feedback & Rating Module

#### **SC080 - Đánh giá đơn hàng sau khi giao thành công**
**Module:** Feedback & Rating  
**Mô tả:** Khách hàng (người tạo đơn) chọn số sao (1-5), nhập nhận xét và click Submit, hệ thống lưu feedback.

#### **SC081 - Đánh giá thất bại khi không phải người tạo đơn**
**Module:** Feedback & Rating  
**Mô tả:** Người dùng khác cố đánh giá đơn hàng không phải của mình, hệ thống hiển thị "Bạn chỉ có thể đánh giá đơn của mình".

#### **SC082 - Xem đánh giá trung bình của shipper**
**Module:** Feedback & Rating  
**Mô tả:** Admin/Customer xem thông tin shipper, hệ thống hiển thị đánh giá trung bình (ví dụ: 4.5/5 sao).

#### **SC083 - Xem danh sách feedback của đơn hàng**
**Module:** Feedback & Rating  
**Mô tả:** Người dùng xem chi tiết đơn hàng, hệ thống hiển thị tất cả feedback của đơn hàng đó.

---

### 3.11. Report & Analytics Module

#### **SC084 - Xem dashboard tổng quan**
**Module:** Report & Analytics  
**Mô tả:** Admin truy cập trang Reports, hệ thống hiển thị KPI: tổng đơn hàng hôm nay, doanh thu, tỷ lệ thành công.

#### **SC085 - Xem thống kê đơn hàng theo trạng thái**
**Module:** Report & Analytics  
**Mô tả:** Admin xem biểu đồ phân bố đơn hàng theo trạng thái (Chưa nhận, Đang giao, Đã giao, Hủy).

#### **SC086 - Xem xu hướng đơn hàng 7 ngày**
**Module:** Report & Analytics  
**Mô tả:** Admin chọn "Last 7 days", hệ thống hiển thị biểu đồ đường số lượng đơn hàng theo ngày.

#### **SC087 - Xem xu hướng đơn hàng 30 ngày**
**Module:** Report & Analytics  
**Mô tả:** Admin chọn "Last 30 days", hệ thống hiển thị biểu đồ đường số lượng đơn hàng và doanh thu theo ngày.

#### **SC088 - Xem thống kê theo nhân viên giao hàng**
**Module:** Report & Analytics  
**Mô tả:** Admin xem báo cáo "By Staff", hệ thống hiển thị số đơn và doanh thu của từng shipper.

#### **SC089 - Xem thống kê theo loại giao hàng**
**Module:** Report & Analytics  
**Mô tả:** Admin xem thống kê theo loại giao hàng (Thường/Nhanh), hệ thống hiển thị biểu đồ tròn.

#### **SC090 - Xem thống kê theo loại hàng hóa**
**Module:** Report & Analytics  
**Mô tả:** Admin xem thống kê theo loại hàng (Gói nhỏ, Bao, Thùng, Laptop, TV, etc.), hệ thống hiển thị biểu đồ cột.

#### **SC091 - Xuất báo cáo CSV**
**Module:** Report & Analytics  
**Mô tả:** Admin click nút "Export CSV", hệ thống tạo file CSV và tự động download về máy.

#### **SC092 - Lọc báo cáo theo khoảng thời gian**
**Module:** Report & Analytics  
**Mô tả:** Admin chọn từ ngày đến ngày, hệ thống hiển thị báo cáo theo khoảng thời gian đã chọn.

---

### 3.12. Profile Management Module

#### **SC093 - Xem thông tin cá nhân**
**Module:** Profile Management  
**Mô tả:** Người dùng đã đăng nhập click vào avatar > Profile, hệ thống hiển thị thông tin cá nhân.

#### **SC094 - Cập nhật thông tin cá nhân**
**Module:** Profile Management  
**Mô tả:** Người dùng sửa họ tên, email, số điện thoại và click Save, hệ thống cập nhật thông tin.

#### **SC095 - Đổi mật khẩu thành công**
**Module:** Profile Management  
**Mô tả:** Người dùng nhập mật khẩu cũ đúng, nhập mật khẩu mới và xác nhận, hệ thống cập nhật mật khẩu.

#### **SC096 - Đổi mật khẩu thất bại với mật khẩu cũ sai**
**Module:** Profile Management  
**Mô tả:** Người dùng nhập sai mật khẩu cũ, hệ thống hiển thị "Mật khẩu cũ không đúng".

#### **SC097 - Đổi mật khẩu thất bại với mật khẩu mới không khớp**
**Module:** Profile Management  
**Mô tả:** Người dùng nhập mật khẩu mới và xác nhận không khớp nhau, hệ thống hiển thị thông báo lỗi.

#### **SC098 - Bật xác thực hai yếu tố (2FA)**
**Module:** Profile Management  
**Mô tả:** Người dùng vào Settings > 2FA và toggle ON, hệ thống cập nhật TwoFactorEnabled=true.

#### **SC099 - Tắt xác thực hai yếu tố (2FA)**
**Module:** Profile Management  
**Mô tả:** Người dùng toggle OFF 2FA, hệ thống cập nhật TwoFactorEnabled=false.

---

### 3.13. Additional Scenarios (Cross-Module & Edge Cases)

#### **SC100 - Session timeout**
**Module:** Authentication  
**Mô tả:** Người dùng đăng nhập và không hoạt động trong 60 phút, JWT token hết hạn, hệ thống tự động logout và chuyển về trang login.

#### **SC101 - Truy cập trang không có quyền**
**Module:** Authentication  
**Mô tả:** Customer cố truy cập trang Admin (ví dụ: /staff.html), hệ thống hiển thị "403 Forbidden" hoặc chuyển về dashboard.

#### **SC102 - Upload file không phải hình ảnh**
**Module:** Chat  
**Mô tả:** Người dùng cố upload file .pdf vào chat, hệ thống hiển thị "Chỉ chấp nhận file hình ảnh (.jpg, .png, .gif)".

#### **SC103 - Mất kết nối internet**
**Module:** All  
**Mô tả:** Người dùng mất kết nối internet, hệ thống hiển thị thông báo "Mất kết nối. Vui lòng kiểm tra internet".

#### **SC104 - Kết nối lại SignalR sau khi mất kết nối**
**Module:** Notification, Chat, Tracking  
**Mô tả:** Sau khi mất kết nối internet và kết nối lại, SignalR tự động reconnect và tiếp tục nhận updates realtime.

---

## Tổng Kết

### Thống Kê Tài Liệu

- **System Modules:** 13 modules chính
- **Functional Requirements:** 33 yêu cầu chức năng (FR01 - FR33)
- **Non-Functional Requirements:** 8 yêu cầu phi chức năng (NFR01 - NFR08)
- **Test Scenarios:** 104 scenarios (SC001 - SC104)

### Lưu Ý Cho Việc Tạo Test Cases & Automation Tests

**Test Cases** sẽ được tạo từ các Scenarios trên với các thông tin chi tiết:
- **Test Case ID** (TC001, TC002, ...)
- **Scenario liên quan** (SC001, SC002, ...)
- **Preconditions** (điều kiện tiên quyết)
- **Test Steps** (các bước thực hiện chi tiết)
- **Test Data** (dữ liệu test cụ thể)
- **Expected Result** (kết quả mong đợi)
- **Priority** (High, Medium, Low)

**Automation Tests** có thể được tạo từ các test cases bằng **Katalon Studio** cho:
- Các luồng chính (happy path): Login, Create Order, Update Status, etc.
- Các tính năng CRUD: Orders, Customers, Staff, Users
- Các form validation: Register, Create Order, Update Profile
- End-to-end scenarios: Tạo đơn hàng → Phân công → Giao hàng → Đánh giá

**Katalon Recording Suggestions:**
- Sử dụng Web Recorder để record các scenarios phức tạp
- Tạo Test Objects cho các elements quan trọng
- Sử dụng Data-Driven Testing cho các scenarios với nhiều test data
- Tạo Custom Keywords cho các actions lặp lại (login, logout, navigate)
- Sử dụng TestNG/JUnit assertions cho verification

---

---

## 4. Test Cases (Chi Tiết)

Dưới đây là các test cases chi tiết được thiết kế theo format chuẩn, phù hợp để nhập vào Excel hoặc công cụ quản lý test case.

### 4.1. Authentication Module Test Cases

#### **TC001 - Đăng nhập thành công với tài khoản hợp lệ**

| Thuộc tính | Nội dung |
|------------|----------|
| **Test Case ID** | TC001 |
| **Scenario** | SC001 |
| **Function** | Đăng nhập hệ thống |
| **Big Item** | Authentication |
| **Medium Item** | Login |
| **Small Item** | Valid login |
| **Pre-condition** | 1. Hệ thống đang chạy<br>2. Người dùng chưa đăng nhập<br>3. Tài khoản test đã tồn tại trong database |
| **Priority** | High |

**Steps:**

| Step | Step Action | Test Data | Expected Result |
|------|-------------|-----------|-----------------|
| 1 | Truy cập trang login | URL: /login.html | Hiển thị form đăng nhập |
| 2 | Nhập username | Username: admin | Username được điền vào ô input |
| 3 | Nhập password | Password: admin123 | Password được ẩn dưới dạng *** |
| 4 | Click nút "Đăng nhập" | - | Hệ thống xác thực thành công, chuyển hướng đến trang dashboard (/index.html), hiển thị tên người dùng trên header |

---

#### **TC002 - Đăng nhập thất bại với mật khẩu sai**

| Thuộc tính | Nội dung |
|------------|----------|
| **Test Case ID** | TC002 |
| **Scenario** | SC002 |
| **Function** | Đăng nhập hệ thống |
| **Big Item** | Authentication |
| **Medium Item** | Login |
| **Small Item** | Invalid password |
| **Pre-condition** | 1. Hệ thống đang chạy<br>2. Người dùng chưa đăng nhập<br>3. Tài khoản admin tồn tại |
| **Priority** | High |

**Steps:**

| Step | Step Action | Test Data | Expected Result |
|------|-------------|-----------|-----------------|
| 1 | Truy cập trang login | URL: /login.html | Hiển thị form đăng nhập |
| 2 | Nhập username đúng | Username: admin | Username được điền vào ô input |
| 3 | Nhập password sai | Password: wrongpass123 | Password được ẩn dưới dạng *** |
| 4 | Click nút "Đăng nhập" | - | Hiển thị thông báo lỗi: "Sai tài khoản hoặc mật khẩu". Người dùng vẫn ở trang login |

---

#### **TC003 - Đăng nhập thất bại với username không tồn tại**

| Thuộc tính | Nội dung |
|------------|----------|
| **Test Case ID** | TC003 |
| **Scenario** | SC003 |
| **Function** | Đăng nhập hệ thống |
| **Big Item** | Authentication |
| **Medium Item** | Login |
| **Small Item** | Invalid username |
| **Pre-condition** | 1. Hệ thống đang chạy<br>2. Người dùng chưa đăng nhập |
| **Priority** | Medium |

**Steps:**

| Step | Step Action | Test Data | Expected Result |
|------|-------------|-----------|-----------------|
| 1 | Truy cập trang login | URL: /login.html | Hiển thị form đăng nhập |
| 2 | Nhập username không tồn tại | Username: notexistuser | Username được điền vào ô input |
| 3 | Nhập password | Password: anypassword | Password được ẩn dưới dạng *** |
| 4 | Click nút "Đăng nhập" | - | Hiển thị thông báo lỗi: "Sai tài khoản hoặc mật khẩu" |

---

#### **TC004 - Đăng nhập với trường bỏ trống**

| Thuộc tính | Nội dung |
|------------|----------|
| **Test Case ID** | TC004 |
| **Scenario** | SC002 (extended) |
| **Function** | Đăng nhập hệ thống |
| **Big Item** | Authentication |
| **Medium Item** | Login |
| **Small Item** | Empty fields validation |
| **Pre-condition** | 1. Hệ thống đang chạy<br>2. Người dùng chưa đăng nhập |
| **Priority** | High |

**Steps:**

| Step | Step Action | Test Data | Expected Result |
|------|-------------|-----------|-----------------|
| 1 | Truy cập trang login | URL: /login.html | Hiển thị form đăng nhập |
| 2 | Để trống username | Username: (empty) | Ô username trống |
| 3 | Để trống password | Password: (empty) | Ô password trống |
| 4 | Click nút "Đăng nhập" | - | Hiển thị thông báo validation: "Vui lòng nhập tên đăng nhập" hoặc "Username is required" |

---

#### **TC005 - Đăng ký tài khoản mới thành công**

| Thuộc tính | Nội dung |
|------------|----------|
| **Test Case ID** | TC005 |
| **Scenario** | SC004 |
| **Function** | Đăng ký tài khoản |
| **Big Item** | Authentication |
| **Medium Item** | Register |
| **Small Item** | Valid registration |
| **Pre-condition** | 1. Hệ thống đang chạy<br>2. Username "newuser123" chưa tồn tại<br>3. Email "newuser@test.com" chưa được sử dụng |
| **Priority** | High |

**Steps:**

| Step | Step Action | Test Data | Expected Result |
|------|-------------|-----------|-----------------|
| 1 | Truy cập trang đăng ký | URL: /register.html | Hiển thị form đăng ký |
| 2 | Nhập username | Username: newuser123 | Username được điền vào ô input |
| 3 | Nhập password | Password: Pass@123 | Password được ẩn dưới dạng *** |
| 4 | Nhập họ tên | Full Name: Nguyễn Văn A | Họ tên được điền vào ô input |
| 5 | Nhập email | Email: newuser@test.com | Email được điền vào ô input |
| 6 | Nhập số điện thoại | Phone: 0912345678 | Số điện thoại được điền vào ô input |
| 7 | Click nút "Đăng ký" | - | Hiển thị thông báo "Đăng ký thành công", chuyển hướng đến trang login |

---

#### **TC006 - Đăng ký thất bại với username đã tồn tại**

| Thuộc tính | Nội dung |
|------------|----------|
| **Test Case ID** | TC006 |
| **Scenario** | SC005 |
| **Function** | Đăng ký tài khoản |
| **Big Item** | Authentication |
| **Medium Item** | Register |
| **Small Item** | Duplicate username |
| **Pre-condition** | 1. Hệ thống đang chạy<br>2. Username "admin" đã tồn tại trong database |
| **Priority** | High |

**Steps:**

| Step | Step Action | Test Data | Expected Result |
|------|-------------|-----------|-----------------|
| 1 | Truy cập trang đăng ký | URL: /register.html | Hiển thị form đăng ký |
| 2 | Nhập username đã tồn tại | Username: admin | Username được điền vào ô input |
| 3 | Nhập password | Password: Pass@123 | Password được ẩn dưới dạng *** |
| 4 | Nhập họ tên | Full Name: Người Dùng Mới | Họ tên được điền vào ô input |
| 5 | Nhập email | Email: newemail@test.com | Email được điền vào ô input |
| 6 | Nhập số điện thoại | Phone: 0987654321 | Số điện thoại được điền vào ô input |
| 7 | Click nút "Đăng ký" | - | Hiển thị thông báo lỗi: "Tên đăng nhập đã tồn tại" |

---

#### **TC007 - Đăng ký thất bại với email đã tồn tại**

| Thuộc tính | Nội dung |
|------------|----------|
| **Test Case ID** | TC007 |
| **Scenario** | SC006 |
| **Function** | Đăng ký tài khoản |
| **Big Item** | Authentication |
| **Medium Item** | Register |
| **Small Item** | Duplicate email |
| **Pre-condition** | 1. Hệ thống đang chạy<br>2. Email "admin@delivery.com" đã được sử dụng |
| **Priority** | High |

**Steps:**

| Step | Step Action | Test Data | Expected Result |
|------|-------------|-----------|-----------------|
| 1 | Truy cập trang đăng ký | URL: /register.html | Hiển thị form đăng ký |
| 2 | Nhập username mới | Username: newuser456 | Username được điền vào ô input |
| 3 | Nhập password | Password: Pass@123 | Password được ẩn dưới dạng *** |
| 4 | Nhập họ tên | Full Name: Người Dùng Mới | Họ tên được điền vào ô input |
| 5 | Nhập email đã tồn tại | Email: admin@delivery.com | Email được điền vào ô input |
| 6 | Nhập số điện thoại | Phone: 0987654321 | Số điện thoại được điền vào ô input |
| 7 | Click nút "Đăng ký" | - | Hiển thị thông báo lỗi: "Email đã tồn tại" |

---

### 4.2. Order Management Test Cases

#### **TC008 - Tạo đơn hàng mới thành công**

| Thuộc tính | Nội dung |
|------------|----------|
| **Test Case ID** | TC008 |
| **Scenario** | SC016 |
| **Function** | Quản lý đơn hàng |
| **Big Item** | Order Management |
| **Medium Item** | Create Order |
| **Small Item** | Valid order creation |
| **Pre-condition** | 1. Đã đăng nhập với tài khoản admin<br>2. Có ít nhất 1 khách hàng trong hệ thống |
| **Priority** | High |

**Steps:**

| Step | Step Action | Test Data | Expected Result |
|------|-------------|-----------|-----------------|
| 1 | Đăng nhập hệ thống | Username: admin, Password: admin123 | Đăng nhập thành công |
| 2 | Truy cập trang Orders | Click menu "Orders" | Hiển thị trang quản lý đơn hàng |
| 3 | Click nút "Tạo đơn hàng mới" | - | Hiển thị form tạo đơn hàng |
| 4 | Nhập tên khách hàng | Customer Name: Nguyễn Văn A | Tên được điền vào ô input |
| 5 | Nhập SĐT khách hàng | Phone: 0912345678 | SĐT được điền vào ô input |
| 6 | Nhập địa chỉ giao hàng | Address: 123 Đường ABC, Phường XYZ | Địa chỉ được điền |
| 7 | Chọn phường/xã | Ward: Phường Bến Nghé | Phường được chọn |
| 8 | Chọn quận/huyện | District: Quận 1 | Quận được chọn |
| 9 | Chọn tỉnh/thành | City: TP.Hồ Chí Minh | Thành phố được chọn |
| 10 | Nhập mã hàng hóa | Product Code: SP001 | Mã hàng được điền |
| 11 | Chọn loại hàng | Package Type: Gói nhỏ | Loại hàng được chọn |
| 12 | Nhập trọng lượng | Weight: 2.5 kg | Trọng lượng được điền |
| 13 | Nhập kích thước | Size: 30x20x10 cm | Kích thước được điền |
| 14 | Nhập khoảng cách | Distance: 15 km | Khoảng cách được điền |
| 15 | Chọn loại giao hàng | Delivery Type: Giao hàng nhanh | Loại giao hàng được chọn |
| 16 | Kiểm tra phí tự động tính | - | Hệ thống hiển thị phí: 75,000 VNĐ (tự động tính) |
| 17 | Click nút "Tạo đơn hàng" | - | Hiển thị thông báo "Tạo đơn hàng thành công", đơn hàng mới xuất hiện trong danh sách với mã đơn tự động (DH...) |

---

#### **TC009 - Tạo đơn hàng thất bại với thông tin thiếu**

| Thuộc tính | Nội dung |
|------------|----------|
| **Test Case ID** | TC009 |
| **Scenario** | SC017 |
| **Function** | Quản lý đơn hàng |
| **Big Item** | Order Management |
| **Medium Item** | Create Order |
| **Small Item** | Missing required fields |
| **Pre-condition** | 1. Đã đăng nhập với tài khoản admin |
| **Priority** | High |

**Steps:**

| Step | Step Action | Test Data | Expected Result |
|------|-------------|-----------|-----------------|
| 1 | Đăng nhập hệ thống | Username: admin, Password: admin123 | Đăng nhập thành công |
| 2 | Truy cập trang Orders | Click menu "Orders" | Hiển thị trang quản lý đơn hàng |
| 3 | Click nút "Tạo đơn hàng mới" | - | Hiển thị form tạo đơn hàng |
| 4 | Bỏ trống tên khách hàng | Customer Name: (empty) | Ô tên khách hàng trống |
| 5 | Nhập SĐT khách hàng | Phone: 0912345678 | SĐT được điền |
| 6 | Bỏ trống địa chỉ | Address: (empty) | Ô địa chỉ trống |
| 7 | Nhập trọng lượng | Weight: 2 kg | Trọng lượng được điền |
| 8 | Click nút "Tạo đơn hàng" | - | Hiển thị thông báo validation: "Vui lòng điền đầy đủ thông tin bắt buộc" hoặc highlight các ô bị thiếu màu đỏ |

---

#### **TC010 - Tạo đơn hàng với trọng lượng âm (invalid)**

| Thuộc tính | Nội dung |
|------------|----------|
| **Test Case ID** | TC010 |
| **Scenario** | SC017 (extended) |
| **Function** | Quản lý đơn hàng |
| **Big Item** | Order Management |
| **Medium Item** | Create Order |
| **Small Item** | Invalid weight value |
| **Pre-condition** | 1. Đã đăng nhập với tài khoản admin |
| **Priority** | Medium |

**Steps:**

| Step | Step Action | Test Data | Expected Result |
|------|-------------|-----------|-----------------|
| 1 | Đăng nhập hệ thống | Username: admin, Password: admin123 | Đăng nhập thành công |
| 2 | Truy cập form tạo đơn hàng | - | Hiển thị form tạo đơn hàng |
| 3 | Điền đầy đủ thông tin hợp lệ | Customer Name: Nguyễn Văn B, Phone: 0923456789, Address: 456 Đường DEF | Thông tin được điền |
| 4 | Nhập trọng lượng âm | Weight: -5 kg | Trọng lượng -5 được điền |
| 5 | Nhập khoảng cách | Distance: 10 km | Khoảng cách được điền |
| 6 | Click nút "Tạo đơn hàng" | - | Hiển thị thông báo lỗi: "Trọng lượng phải lớn hơn 0" hoặc "Trọng lượng không hợp lệ" |

---

#### **TC011 - Xem danh sách đơn hàng (Admin)**

| Thuộc tính | Nội dung |
|------------|----------|
| **Test Case ID** | TC011 |
| **Scenario** | SC018 |
| **Function** | Quản lý đơn hàng |
| **Big Item** | Order Management |
| **Medium Item** | View Orders |
| **Small Item** | List all orders |
| **Pre-condition** | 1. Đã đăng nhập với tài khoản admin<br>2. Có ít nhất 5 đơn hàng trong hệ thống |
| **Priority** | High |

**Steps:**

| Step | Step Action | Test Data | Expected Result |
|------|-------------|-----------|-----------------|
| 1 | Đăng nhập hệ thống | Username: admin, Password: admin123 | Đăng nhập thành công |
| 2 | Truy cập trang Orders | Click menu "Orders" | Hiển thị trang danh sách đơn hàng |
| 3 | Kiểm tra danh sách | - | Hiển thị danh sách tất cả đơn hàng với các cột: Mã đơn, Khách hàng, Trạng thái, Phí, Ngày tạo. Có pagination nếu nhiều hơn 20 đơn |
| 4 | Kiểm tra số lượng hiển thị | - | Mặc định hiển thị 20 đơn hàng/trang |

---

#### **TC012 - Lọc đơn hàng theo trạng thái**

| Thuộc tính | Nội dung |
|------------|----------|
| **Test Case ID** | TC012 |
| **Scenario** | SC022 |
| **Function** | Quản lý đơn hàng |
| **Big Item** | Order Management |
| **Medium Item** | Filter Orders |
| **Small Item** | Filter by status |
| **Pre-condition** | 1. Đã đăng nhập với tài khoản admin<br>2. Có đơn hàng ở nhiều trạng thái khác nhau |
| **Priority** | Medium |

**Steps:**

| Step | Step Action | Test Data | Expected Result |
|------|-------------|-----------|-----------------|
| 1 | Đăng nhập hệ thống | Username: admin, Password: admin123 | Đăng nhập thành công |
| 2 | Truy cập trang Orders | Click menu "Orders" | Hiển thị danh sách đơn hàng |
| 3 | Click dropdown "Lọc theo trạng thái" | - | Hiển thị danh sách trạng thái: Tất cả, Chưa nhận, Đã nhận chưa giao, Đang giao, Đã giao |
| 4 | Chọn "Đang giao" | Status: Đang giao | Dropdown hiển thị "Đang giao" được chọn |
| 5 | Kiểm tra kết quả lọc | - | Chỉ hiển thị các đơn hàng có trạng thái "Đang giao" |

---

#### **TC013 - Cập nhật trạng thái đơn hàng thành "Đang giao"**

| Thuộc tính | Nội dung |
|------------|----------|
| **Test Case ID** | TC013 |
| **Scenario** | SC025 |
| **Function** | Quản lý đơn hàng |
| **Big Item** | Order Management |
| **Medium Item** | Update Order Status |
| **Small Item** | Change to "In Delivery" |
| **Pre-condition** | 1. Đã đăng nhập với tài khoản shipper<br>2. Có đơn hàng ở trạng thái "Đã nhận chưa giao" được phân công cho shipper đăng nhập |
| **Priority** | High |

**Steps:**

| Step | Step Action | Test Data | Expected Result |
|------|-------------|-----------|-----------------|
| 1 | Đăng nhập với tài khoản shipper | Username: shipper1, Password: shipper123 | Đăng nhập thành công |
| 2 | Truy cập trang "Đơn của tôi" | Click menu "My Orders" | Hiển thị danh sách đơn được phân công |
| 3 | Click vào đơn hàng cần cập nhật | Order Code: DH20260309001 | Hiển thị chi tiết đơn hàng |
| 4 | Click nút "Bắt đầu giao hàng" | - | Hiển thị confirm dialog |
| 5 | Xác nhận thay đổi trạng thái | Click "Xác nhận" | Trạng thái đơn hàng chuyển thành "Đang giao", lưu thời gian bắt đầu giao, gửi thông báo cho khách hàng |

---

#### **TC014 - Phân công đơn hàng cho shipper**

| Thuộc tính | Nội dung |
|------------|----------|
| **Test Case ID** | TC014 |
| **Scenario** | SC027 |
| **Function** | Quản lý đơn hàng |
| **Big Item** | Order Management |
| **Medium Item** | Assign Order |
| **Small Item** | Assign to available shipper |
| **Pre-condition** | 1. Đã đăng nhập với tài khoản admin<br>2. Có đơn hàng ở trạng thái "Chưa nhận"<br>3. Có ít nhất 1 shipper rảnh |
| **Priority** | High |

**Steps:**

| Step | Step Action | Test Data | Expected Result |
|------|-------------|-----------|-----------------|
| 1 | Đăng nhập với tài khoản admin | Username: admin, Password: admin123 | Đăng nhập thành công |
| 2 | Truy cập trang Orders | Click menu "Orders" | Hiển thị danh sách đơn hàng |
| 3 | Click vào đơn hàng chưa phân công | Order Code: DH20260309002 | Hiển thị chi tiết đơn hàng |
| 4 | Click nút "Phân công shipper" | - | Hiển thị danh sách shipper đang rảnh |
| 5 | Chọn shipper từ danh sách | Shipper: Trần Văn B | Shipper được chọn |
| 6 | Click nút "Xác nhận phân công" | - | Đơn hàng được phân công cho shipper, trạng thái chuyển thành "Đã nhận chưa giao", gửi thông báo cho shipper |

---

### 4.3. Customer Management Test Cases

#### **TC015 - Thêm khách hàng mới thành công**

| Thuộc tính | Nội dung |
|------------|----------|
| **Test Case ID** | TC015 |
| **Scenario** | SC035 |
| **Function** | Quản lý khách hàng |
| **Big Item** | Customer Management |
| **Medium Item** | Add Customer |
| **Small Item** | Valid customer creation |
| **Pre-condition** | 1. Đã đăng nhập với tài khoản admin<br>2. Số điện thoại "0945678912" chưa tồn tại |
| **Priority** | High |

**Steps:**

| Step | Step Action | Test Data | Expected Result |
|------|-------------|-----------|-----------------|
| 1 | Đăng nhập hệ thống | Username: admin, Password: admin123 | Đăng nhập thành công |
| 2 | Truy cập trang Customers | Click menu "Customers" | Hiển thị trang quản lý khách hàng |
| 3 | Click nút "Thêm khách hàng" | - | Hiển thị form thêm khách hàng |
| 4 | Nhập họ tên | Full Name: Lê Thị C | Họ tên được điền |
| 5 | Nhập số điện thoại | Phone: 0945678912 | SĐT được điền |
| 6 | Nhập email | Email: lethic@example.com | Email được điền |
| 7 | Nhập địa chỉ | Address: 789 Đường GHI, Quận 3 | Địa chỉ được điền |
| 8 | Click nút "Lưu" | - | Hiển thị thông báo "Thêm khách hàng thành công", khách hàng mới xuất hiện trong danh sách |

---

#### **TC016 - Thêm khách hàng thất bại với SĐT trùng**

| Thuộc tính | Nội dung |
|------------|----------|
| **Test Case ID** | TC016 |
| **Scenario** | SC036 |
| **Function** | Quản lý khách hàng |
| **Big Item** | Customer Management |
| **Medium Item** | Add Customer |
| **Small Item** | Duplicate phone number |
| **Pre-condition** | 1. Đã đăng nhập với tài khoản admin<br>2. Số điện thoại "0912345678" đã tồn tại |
| **Priority** | Medium |

**Steps:**

| Step | Step Action | Test Data | Expected Result |
|------|-------------|-----------|-----------------|
| 1 | Đăng nhập hệ thống | Username: admin, Password: admin123 | Đăng nhập thành công |
| 2 | Truy cập form thêm khách hàng | - | Hiển thị form thêm khách hàng |
| 3 | Điền thông tin khách hàng | Full Name: Phạm Văn D | Họ tên được điền |
| 4 | Nhập SĐT đã tồn tại | Phone: 0912345678 | SĐT được điền |
| 5 | Nhập email | Email: phamvand@example.com | Email được điền |
| 6 | Nhập địa chỉ | Address: 321 Đường KLM | Địa chỉ được điền |
| 7 | Click nút "Lưu" | - | Hiển thị thông báo lỗi: "Số điện thoại đã tồn tại" hoặc "Phone number already exists" |

---

#### **TC017 - Xem chi tiết khách hàng**

| Thuộc tính | Nội dung |
|------------|----------|
| **Test Case ID** | TC017 |
| **Scenario** | SC038 |
| **Function** | Quản lý khách hàng |
| **Big Item** | Customer Management |
| **Medium Item** | View Customer |
| **Small Item** | Customer details |
| **Pre-condition** | 1. Đã đăng nhập với tài khoản admin<br>2. Có khách hàng ID = 1 trong hệ thống |
| **Priority** | Medium |

**Steps:**

| Step | Step Action | Test Data | Expected Result |
|------|-------------|-----------|-----------------|
| 1 | Đăng nhập hệ thống | Username: admin, Password: admin123 | Đăng nhập thành công |
| 2 | Truy cập trang Customers | Click menu "Customers" | Hiển thị danh sách khách hàng |
| 3 | Click vào khách hàng | Customer: Nguyễn Văn A (ID=1) | Hiển thị chi tiết khách hàng |
| 4 | Kiểm tra thông tin hiển thị | - | Hiển thị: Họ tên, SĐT, Email, Địa chỉ, Lịch sử đơn hàng của khách hàng này |

---

### 4.4. Notification Test Cases

#### **TC018 - Nhận thông báo realtime khi có đơn hàng mới**

| Thuộc tính | Nội dung |
|------------|----------|
| **Test Case ID** | TC018 |
| **Scenario** | SC054 |
| **Function** | Thông báo |
| **Big Item** | Notification |
| **Medium Item** | Real-time Notification |
| **Small Item** | New order notification |
| **Pre-condition** | 1. Đã đăng nhập với tài khoản shipper trên trình duyệt 1<br>2. Có tài khoản admin đăng nhập trên trình duyệt 2<br>3. SignalR đang hoạt động |
| **Priority** | High |

**Steps:**

| Step | Step Action | Test Data | Expected Result |
|------|-------------|-----------|-----------------|
| 1 | Trình duyệt 1: Đăng nhập shipper | Username: shipper1, Password: shipper123 | Shipper đăng nhập thành công |
| 2 | Trình duyệt 1: Ở lại trang dashboard | - | Trang dashboard hiển thị icon thông báo với badge = 0 |
| 3 | Trình duyệt 2: Đăng nhập admin | Username: admin, Password: admin123 | Admin đăng nhập thành công |
| 4 | Trình duyệt 2: Tạo đơn hàng mới | Tạo đơn hàng với thông tin hợp lệ | Đơn hàng được tạo thành công |
| 5 | Trình duyệt 1: Kiểm tra thông báo | - | Badge thông báo tăng lên (0→1), hiển thị notification popup "Có đơn hàng mới", không cần refresh trang |

---

#### **TC019 - Đánh dấu thông báo đã đọc**

| Thuộc tính | Nội dung |
|------------|----------|
| **Test Case ID** | TC019 |
| **Scenario** | SC059 |
| **Function** | Thông báo |
| **Big Item** | Notification |
| **Medium Item** | Mark as Read |
| **Small Item** | Single notification |
| **Pre-condition** | 1. Đã đăng nhập<br>2. Có ít nhất 1 thông báo chưa đọc |
| **Priority** | Medium |

**Steps:**

| Step | Step Action | Test Data | Expected Result |
|------|-------------|-----------|-----------------|
| 1 | Đăng nhập hệ thống | Username: customer1, Password: customer123 | Đăng nhập thành công |
| 2 | Click vào icon thông báo | - | Hiển thị danh sách thông báo, badge hiển thị số 3 |
| 3 | Click vào 1 thông báo chưa đọc | Notification: "Đơn hàng DH001 đang được giao" | Thông báo chuyển sang trạng thái đã đọc (màu xám hoặc không in đậm), badge giảm xuống (3→2) |

---

### 4.5. Chat Test Cases

#### **TC020 - Gửi tin nhắn text thành công**

| Thuộc tính | Nội dung |
|------------|----------|
| **Test Case ID** | TC020 |
| **Scenario** | SC063 |
| **Function** | Chat |
| **Big Item** | Chat |
| **Medium Item** | Send Message |
| **Small Item** | Text message |
| **Pre-condition** | 1. Đã đăng nhập với tài khoản customer<br>2. SignalR đang hoạt động |
| **Priority** | High |

**Steps:**

| Step | Step Action | Test Data | Expected Result |
|------|-------------|-----------|-----------------|
| 1 | Đăng nhập hệ thống | Username: customer1, Password: customer123 | Đăng nhập thành công |
| 2 | Truy cập trang Chat | Click menu "Chat" hoặc "Support" | Hiển thị giao diện chat |
| 3 | Nhập tin nhắn vào ô input | Message: "Xin chào, tôi cần hỗ trợ về đơn hàng" | Tin nhắn được nhập vào ô input |
| 4 | Click nút "Gửi" hoặc Enter | - | Tin nhắn được gửi và hiển thị trong chat box với timestamp, avatar người gửi |
| 5 | Kiểm tra tin nhắn ở phía admin | - | Admin nhận được tin nhắn realtime (nếu admin đang online) |

---

#### **TC021 - Gửi hình ảnh thất bại với file quá lớn**

| Thuộc tính | Nội dung |
|------------|----------|
| **Test Case ID** | TC021 |
| **Scenario** | SC065 |
| **Function** | Chat |
| **Big Item** | Chat |
| **Medium Item** | Send Image |
| **Small Item** | Oversized file |
| **Pre-condition** | 1. Đã đăng nhập<br>2. Có file ảnh > 5MB |
| **Priority** | Medium |

**Steps:**

| Step | Step Action | Test Data | Expected Result |
|------|-------------|-----------|-----------------|
| 1 | Đăng nhập hệ thống | Username: customer1, Password: customer123 | Đăng nhập thành công |
| 2 | Truy cập trang Chat | Click menu "Chat" | Hiển thị giao diện chat |
| 3 | Click nút "Đính kèm hình ảnh" | - | Mở file picker |
| 4 | Chọn file ảnh lớn | File: image_10mb.jpg (10MB) | File được chọn |
| 5 | Xác nhận upload | Click "Open" | Hiển thị thông báo lỗi: "Kích thước file vượt quá 5MB" hoặc "File quá lớn. Tối đa 5MB" |

---

### 4.6. Tracking Test Cases

#### **TC022 - Tracking công khai bằng mã đơn hàng**

| Thuộc tính | Nội dung |
|------------|----------|
| **Test Case ID** | TC022 |
| **Scenario** | SC078 |
| **Function** | Theo dõi đơn hàng |
| **Big Item** | Tracking |
| **Medium Item** | Public Tracking |
| **Small Item** | Track by order code |
| **Pre-condition** | 1. Không cần đăng nhập<br>2. Có đơn hàng với mã DH20260309001 |
| **Priority** | High |

**Steps:**

| Step | Step Action | Test Data | Expected Result |
|------|-------------|-----------|-----------------|
| 1 | Truy cập trang Tracking | URL: /tracking.html | Hiển thị trang tracking công khai |
| 2 | Nhập mã đơn hàng | Order Code: DH20260309001 | Mã đơn hàng được điền vào ô input |
| 3 | Click nút "Tra cứu" | - | Hiển thị thông tin đơn hàng: Trạng thái hiện tại, Timeline các checkpoint, Thông tin shipper (nếu có), Vị trí hiện tại trên bản đồ (nếu đang giao) |

---

#### **TC023 - Tracking thất bại với mã không tồn tại**

| Thuộc tính | Nội dung |
|------------|----------|
| **Test Case ID** | TC023 |
| **Scenario** | SC079 |
| **Function** | Theo dõi đơn hàng |
| **Big Item** | Tracking |
| **Medium Item** | Public Tracking |
| **Small Item** | Invalid order code |
| **Pre-condition** | 1. Không cần đăng nhập |
| **Priority** | Medium |

**Steps:**

| Step | Step Action | Test Data | Expected Result |
|------|-------------|-----------|-----------------|
| 1 | Truy cập trang Tracking | URL: /tracking.html | Hiển thị trang tracking |
| 2 | Nhập mã đơn hàng không tồn tại | Order Code: DH99999999999 | Mã đơn hàng được điền |
| 3 | Click nút "Tra cứu" | - | Hiển thị thông báo: "Không tìm thấy đơn hàng với mã này" hoặc "Order not found" |

---

### 4.7. Report Test Cases

#### **TC024 - Xem dashboard tổng quan**

| Thuộc tính | Nội dung |
|------------|----------|
| **Test Case ID** | TC024 |
| **Scenario** | SC084 |
| **Function** | Báo cáo thống kê |
| **Big Item** | Report & Analytics |
| **Medium Item** | Dashboard |
| **Small Item** | Overview KPIs |
| **Pre-condition** | 1. Đã đăng nhập với tài khoản admin<br>2. Có dữ liệu đơn hàng trong hệ thống |
| **Priority** | High |

**Steps:**

| Step | Step Action | Test Data | Expected Result |
|------|-------------|-----------|-----------------|
| 1 | Đăng nhập hệ thống | Username: admin, Password: admin123 | Đăng nhập thành công |
| 2 | Truy cập trang Reports | Click menu "Reports" | Hiển thị trang báo cáo |
| 3 | Kiểm tra KPIs hiển thị | - | Hiển thị: Tổng đơn hàng hôm nay, Doanh thu hôm nay, Tỷ lệ giao hàng thành công (%), Số đơn đang giao |
| 4 | Kiểm tra biểu đồ | - | Hiển thị: Biểu đồ tròn phân bố trạng thái đơn hàng, Biểu đồ đường xu hướng 7 ngày |

---

#### **TC025 - Xuất báo cáo CSV**

| Thuộc tính | Nội dung |
|------------|----------|
| **Test Case ID** | TC025 |
| **Scenario** | SC091 |
| **Function** | Báo cáo thống kê |
| **Big Item** | Report & Analytics |
| **Medium Item** | Export Report |
| **Small Item** | CSV export |
| **Pre-condition** | 1. Đã đăng nhập với tài khoản admin<br>2. Có dữ liệu đơn hàng |
| **Priority** | Medium |

**Steps:**

| Step | Step Action | Test Data | Expected Result |
|------|-------------|-----------|-----------------|
| 1 | Đăng nhập hệ thống | Username: admin, Password: admin123 | Đăng nhập thành công |
| 2 | Truy cập trang Reports | Click menu "Reports" | Hiển thị trang báo cáo |
| 3 | Chọn khoảng thời gian | From: 01/03/2026, To: 09/03/2026 | Khoảng thời gian được chọn |
| 4 | Click nút "Export CSV" | - | File CSV được download về máy với tên dạng "orders_report_20260309.csv" |
| 5 | Mở file CSV | - | File chứa dữ liệu đơn hàng trong khoảng thời gian đã chọn với các cột: Order Code, Customer, Status, Fee, Date |

---

### 4.8. Profile Management Test Cases

#### **TC026 - Đổi mật khẩu thành công**

| Thuộc tính | Nội dung |
|------------|----------|
| **Test Case ID** | TC026 |
| **Scenario** | SC095 |
| **Function** | Quản lý thông tin cá nhân |
| **Big Item** | Profile Management |
| **Medium Item** | Change Password |
| **Small Item** | Valid password change |
| **Pre-condition** | 1. Đã đăng nhập với tài khoản customer1<br>2. Mật khẩu hiện tại: customer123 |
| **Priority** | High |

**Steps:**

| Step | Step Action | Test Data | Expected Result |
|------|-------------|-----------|-----------------|
| 1 | Đăng nhập hệ thống | Username: customer1, Password: customer123 | Đăng nhập thành công |
| 2 | Truy cập trang Profile | Click avatar → "Profile" | Hiển thị trang thông tin cá nhân |
| 3 | Click tab "Đổi mật khẩu" | - | Hiển thị form đổi mật khẩu |
| 4 | Nhập mật khẩu cũ | Old Password: customer123 | Mật khẩu cũ được ẩn dưới dạng *** |
| 5 | Nhập mật khẩu mới | New Password: NewPass@456 | Mật khẩu mới được ẩn |
| 6 | Xác nhận mật khẩu mới | Confirm Password: NewPass@456 | Mật khẩu xác nhận được ẩn |
| 7 | Click nút "Lưu thay đổi" | - | Hiển thị thông báo "Đổi mật khẩu thành công", có thể đăng nhập lại với mật khẩu mới |

---

#### **TC027 - Đổi mật khẩu thất bại với mật khẩu cũ sai**

| Thuộc tính | Nội dung |
|------------|----------|
| **Test Case ID** | TC027 |
| **Scenario** | SC096 |
| **Function** | Quản lý thông tin cá nhân |
| **Big Item** | Profile Management |
| **Medium Item** | Change Password |
| **Small Item** | Wrong old password |
| **Pre-condition** | 1. Đã đăng nhập với tài khoản customer1 |
| **Priority** | Medium |

**Steps:**

| Step | Step Action | Test Data | Expected Result |
|------|-------------|-----------|-----------------|
| 1 | Đăng nhập hệ thống | Username: customer1, Password: customer123 | Đăng nhập thành công |
| 2 | Truy cập form đổi mật khẩu | - | Hiển thị form đổi mật khẩu |
| 3 | Nhập mật khẩu cũ sai | Old Password: wrongpassword | Mật khẩu được ẩn |
| 4 | Nhập mật khẩu mới | New Password: NewPass@789 | Mật khẩu được ẩn |
| 5 | Xác nhận mật khẩu mới | Confirm Password: NewPass@789 | Mật khẩu được ẩn |
| 6 | Click nút "Lưu thay đổi" | - | Hiển thị thông báo lỗi: "Mật khẩu cũ không đúng" hoặc "Current password is incorrect" |

---

### Tổng Kết Test Cases

**Tổng số Test Cases chi tiết:** 27 test cases (TC001 - TC027)

**Phân bố theo module:**
- Authentication: 7 test cases (TC001 - TC007)
- Order Management: 7 test cases (TC008 - TC014)
- Customer Management: 3 test cases (TC015 - TC017)
- Notification: 2 test cases (TC018 - TC019)
- Chat: 2 test cases (TC020 - TC021)
- Tracking: 2 test cases (TC022 - TC023)
- Report: 2 test cases (TC024 - TC025)
- Profile Management: 2 test cases (TC026 - TC027)

**Lưu ý:**
- Các test cases trên có thể được mở rộng thêm tùy theo yêu cầu
- Format này hoàn toàn tương thích với việc nhập vào Excel
- Mỗi test case có đầy đủ thông tin để thực hiện manual testing hoặc automation testing
- Có thể dễ dàng chuyển đổi sang Katalon Studio test scripts

---

**Người tạo:** GitHub Copilot  
**Phiên bản:** 1.0  
**Cập nhật lần cuối:** 9 tháng 3, 2026

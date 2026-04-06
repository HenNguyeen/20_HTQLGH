## 2.2 CÁC CHUẨN CHẤT LƯỢNG

### 2.2.1 Functional (Chức năng)

• **Đăng ký và Đăng nhập:** Hệ thống phải cho phép khách hàng đăng ký tài khoản mới với email và mật khẩu hợp lệ, đồng thời cho phép đăng nhập thành công với thông tin chính xác. Hệ thống phải ghi nhận thông tin người dùng, xác thực quyền truy cập, và hỗ trợ chức năng quên mật khẩu.

• **Quản lý thông tin cá nhân:** Khách hàng có thể chính sửa và cập nhật thông tin cá nhân như họ tên, địa chỉ, số điện thoại, email, mật khẩu. Hệ thống phải đảm bảo bảo mật thông tin tài khoản và che giấu mật khẩu khi hiển thị.

• **Quản lý đơn hàng (User):** Hệ thống phải cho phép khách hàng theo dõi các thông tin của đơn hàng (đang chờ xác nhận, đã xác nhận, đã gán nhân viên giao hàng, đang giao, đã giao). Khách hàng có thể duyệt danh sách đơn hàng, xem chi tiết đơn hàng, hủy đơn hàng (nếu chưa được gán).

• **Quản lý sản phẩm (Admin):** Admin quản lý sản phẩm (thêm, sửa, xóa) và kiểm tra trên trang User sản phẩm mới tạo đã được cập nhật hay chưa. Admin có thể tạo mới sản phẩm dự trên loại hàng sẵn có, chép sản phẩm hoặc xóa sản phẩm.

• **Quản lý nhân viên giao hàng:** Admin có thể tạo tài khoản nhân viên giao hàng mới, sửa thông tin nhân viên (họ tên, số điện thoại, địa chỉ, vùng phụ trách), xóa nhân viên. Hệ thống phải cập nhật danh sách nhân viên và hiển thị trạng thái hoạt động.

• **Quản lý đơn hàng (Admin):** Admin có thể xem tất cả đơn hàng, lọc theo trạng thái (chờ gán, đã gán, đang giao, đã giao), tìm kiếm theo mã đơn hàng hoặc tên khách hàng, gán nhân viên cho đơn hàng. Hệ thống phải ghi lại lịch sử thay đổi trạng thái.

• **Thanh toán:** Hệ thống hỗ trợ hai phương thức thanh toán: COD (thanh toán khi nhận hàng) và Momo (thanh toán trực tuyến). Khi khách hàng chọn thanh toán qua Momo, hệ thống chuyển hướng sang cổng thanh toán Momo. Hệ thống phải ghi nhận và xác nhận khoản thanh toán.

• **Theo dõi đơn hàng thực thời:** Khách hàng có thể theo dõi vị trí hiện tại của nhân viên giao hàng trên bản đồ, xem thời gian giao hàng dự kiến, và lịch sử các điểm dừng của đơn hàng.

• **Giao diện người dùng:** Giao diện phải thân thiện, dễ sử dụng, và hỗ trợ trên nhiều thiết bị (máy tính, tablet, điện thoại di động). Các thông báo lỗi phải rõ ràng và hữu ích, giúp người dùng hiểu vấn đề và cách khắc phục.

---

### 2.2.2 Hiệu suất (Performance)

• **Thời gian tải trang:** Trang web phải tải xong và có thể tương tác trong vòng 2 giây trên kết nối internet thông thường. Trang danh sách đơn hàng phải load đủ nhanh khi có dữ liệu lớn (1000+ đơn hàng).

• **Phản hồi API:** Các yêu cầu API phải nhận được phản hồi từ server trong vòng 500ms (95% trường hợp). Các thao tác tạo đơn hàng, cập nhật trạng thái, thanh toán phải xử lý nhanh chóng.

• **Database:** Các truy vấn cơ sở dữ liệu phải hoàn thành trong vòng 1 giây. Tối ưu hóa truy vấn, lập chỉ mục, và tránh N+1 query problems để đảm bảo hiệu suất.

• **Bộ nhớ và tài nguyên:** Ứng dụng phải sử dụng bộ nhớ ổn định, không có rò rỉ bộ nhớ khi chạy lâu dài. Hình ảnh phải được tối ưu hóa (kích thước nhỏ hơn 100KB) và lazy load.

---

### 2.2.3 Bảo mật (Security)

• **Xác thực và phân quyền:** Hệ thống phải xác thực người dùng thông qua tài khoản và mật khẩu, mã hóa mật khẩu bằng bcrypt. Quyền truy cập phải được kiểm soát theo vai trò (Customer, Shipper, Admin) để ngăn chặn truy cập trái phép.

• **Kết nối an toàn:** Tất cả giao tiếp giữa client và server phải sử dụng HTTPS (SSL/TLS). Hệ thống phải yêu cầu CSRF token cho các thao tác thay đổi dữ liệu.

• **Bảo vệ dữ liệu:** Thông tin cá nhân khách hàng (tên, địa chỉ, số điện thoại) phải được mã hóa khi lưu trữ. Dữ liệu thanh toán phải tuân thủ tiêu chuẩn PCI DSS, không lưu trữ số thẻ tín dụng.

• **Kiểm duyệt đầu vào:** Ứng dụng phải kiểm tra và xác thực tất cả dữ liệu đầu vào để ngăn chặn SQL Injection, XSS (Cross-Site Scripting), và các cuộc tấn công khác.

• **Giới hạn tốc độ:** Hệ thống phải giới hạn số lượng yêu cầu từ một người dùng (tối đa 10 yêu cầu/phút) để ngăn chặn brute force attacks.

---

### 2.2.4 Khả dụng (Usability)

• **Giao diện trực quan:** Các biểu mẫu phải rõ ràng, dễ điền, với hướng dẫn cần thiết. Danh sách, menu phải được sắp xếp hợp lý để người dùng dễ tìm thấy thông tin cần thiết.

• **Thông báo và phản hồi:** Ứng dụng phải cung cấp thông báo rõ ràng khi thao tác thành công (ví dụ: "Đơn hàng đã được tạo - Mã: ORD123") hoặc thất bại (ví dụ: "Khách hàng không tồn tại"). Các hộp xác nhận phải xuất hiện trước khi xóa hoặc hủy đơn.

• **Phản ứng nhanh:** Ứng dụng phải có chỉ báo tải (loading spinner) khi đang xử lý yêu cầu, tránh cho người dùng cảm thấy ứng dụng đã dừng lại.

• **Accessibility:** Các phần tử giao diện phải có kích thước đủ lớn (cỡ font tối thiểu 12px), độ tương phản màu tốt (4.5:1 trở lên), và hỗ trợ điều hướng bàn phím.

• **Hỗ trợ đa nền tảng:** Ứng dụng phải hoạt động tốt trên desktop, tablet, và mobile. Giao diện phải responsive, tự điều chỉnh kích thước theo màn hình.

---

### 2.2.5 Độ tin cậy (Reliability)

• **Sẵn sàng hệ thống:** Hệ thống phải hoạt động liên tục với thời gian ngắn nhất đối với downtime (tối đa 99.5% uptime, tương đương 3.6 giờ/tháng). Khi xảy ra sự cố, hệ thống nên khôi phục trong vòng 30 phút tối đa.

• **Tính toàn vẹn dữ liệu:** Các giao dịch phải tuân theo nguyên tắc ACID (Atomicity, Consistency, Isolation, Durability) để đảm bảo không mất dữ liệu hoặc dữ liệu không bị hỏng. Hệ thống phải sao lưu dữ liệu định kỳ (mỗi 6 giờ) và kiểm tra quy trình khôi phục hàng tháng.

• **Xử lý lỗi:** Khi xảy ra lỗi bất ngờ, hệ thống phải ghi lại lỗi chi tiết để kiểm tra sau, nhưng không lộ thông tin nhạy cảm cho người dùng. Hệ thống phải tự động thử lại kết nối database khi gặp sự cố tạm thời.

• **Ghi chép nhật ký:** Tất cả hành động quan trọng (đăng nhập, tạo đơn hàng, thanh toán, cập nhật trạng thái) phải được ghi lại với thông tin: người dùng, thời gian, thao tác, giá trị cũ/mới. Nhật ký phải được bảo vệ và không thể xóa.

• **Kiểm tra đồng thời:** Khi nhiều người dùng thực hiện thao tác cùng lúc (ví dụ: cập nhật trạng thái đơn hàng cùng lúc), hệ thống phải xử lý chính xác mà không gây ra xung đột hay mất dữ liệu.

---

## 2.3 PHẠM VI KIỂM THỬ CỦA HỆ THỐNG (Scope)

Các chức năng và phạm vi kiểm thử được xác định như sau:

• Đăng ký tài khoản
• Đăng nhập hệ thống
• Đăng xuất hệ thống
• Chính sửa thông tin cá nhân
• Đặt sản phẩm (Order Management)
• Tra cứu đơn hàng
• Xem chi tiết đơn hàng
• Quản lý sản phẩm (Admin)
• Quản lý nhân viên giao hàng (Admin)
• Thanh toán (COD, Momo)
• Theo dõi đơn hàng thực thời
• Thông báo status đơn hàng
• Báo cáo & Thống kê

### Bảng Phạm vi Kiểm thử Chi tiết

| Chức năng | Phạm vi Kiểm thử |
|-----------|------------------|
| **Đăng ký tài khoản** | • Kiểm tra email hợp lệ (format, không trùng lặp) <br> • Kiểm tra password strength (min 8 chars, complexity) <br> • Kiểm tra validate dữ liệu (họ tên, số điện thoại, địa chỉ) <br> • Kiểm tra xác nhận email<br> • Kiểm tra lỗi nhập liệu |
| **Đăng nhập** | • Kiểm tra đăng nhập thành công với credentials đúng <br> • Kiểm tra đăng nhập thất bại với credentials sai <br> • Kiểm tra lock account sau 5 lần thất bại <br> • Kiểm tra password reset functionality <br> • Kiểm tra session timeout (30 min inactivity) <br> • Kiểm tra 2FA (Two-Factor Authentication) |
| **Đăng xuất** | • Kiểm tra logout hoạt động đúng <br> • Kiểm tra session terminated sau logout <br> • Kiểm tra redirect tới login page |
| **Chính sửa thông tin cá nhân** | • Kiểm tra cập nhật thông tin (họ tên, số điện thoại, địa chỉ) <br> • Kiểm tra validate dữ liệu <br> • Kiểm tra mật khẩu được mã hóa <br> • Kiểm tra audit trail là ghi nhận thay đổi <br> • Kiểm tra error handling khi cập nhật thất bại |
| **Đặt sản phẩm** | • Kiểm tra tạo order mới với tất cả required fields <br> • Kiểm tra validate dữ liệu (customer ID, product, quantity, address) <br> • Kiểm tra order ID được gen automatically <br> • Kiểm tra trạng thái order = Pending <br> • Kiểm tra ngày tạo order được ghi nhận <br> • Kiểm tra user có thể thêm multiple items <br> • Kiểm tra validate số lượng sản phẩm (min 1, max stock) <br> • Kiểm tra lỗi khi customer không tồn tại |
| **Tra cứu đơn hàng** | • Kiểm tra hiển thị danh sách đơn hàng của user <br> • Kiểm tra filter theo trạng thái (Pending, Assigned, InTransit, Delivered) <br> • Kiểm tra search theo mã đơn hàng <br> • Kiểm tra sort theo ngày tạo, trạng thái <br> • Kiểm tra pagination (10 items/page) <br> • Kiểm tra hiển thị đúng số lượng kết quả |
| **Xem chi tiết đơn hàng** | • Kiểm tra hiển thị đúng thông tin order <br> • Kiểm tra hiển thị customer info, products, quantity, price <br> • Kiểm tra hiển thị trạng thái order & ngày tạo <br> • Kiểm tra hiển thị shipper info nếu đã gán <br> • Kiểm tra hiển thị estimated delivery time <br> • Kiểm tra button action phù hợp với trạng thái (Cancel, Track) |
| **Quản lý sản phẩm (Admin)** | • Kiểm tra thêm sản phẩm mới (name, description, price, stock) <br> • Kiểm tra sửa thông tin sản phẩm <br> • Kiểm tra xóa sản phẩm (soft delete) <br> • Kiểm tra validate dữ liệu (price > 0, stock >= 0) <br> • Kiểm tra hiển thị danh sách sản phẩm cho user <br> • Kiểm tra hình ảnh sản phẩm upload & hiển thị |
| **Quản lý nhân viên giao hàng** | • Kiểm tra Admin tạo tài khoản shipper mới <br> • Kiểm tra sửa thông tin shipper (họ tên, số điện thoại, địa chỉ, vùng phụ trách) <br> • Kiểm tra xóa shipper <br> • Kiểm tra assign order cho shipper <br> • Kiểm tra hiển thị danh sách shipper available <br> • Kiểm tra kiểm tra shipper status (active, inactive) |
| **Thanh toán (COD)** | • Kiểm tra user chọn COD option <br> • Kiểm tra order status = Pending Payment <br> • Kiểm tra shipper nhận được thông báo <br> • Kiểm tra payment được ghi nhận khi shipper confirm <br> • Kiểm tra invoice được generate <br> • Kiểm tra email confirmation được gửi |
| **Thanh toán (Momo)** | • Kiểm tra user chọn Momo payment <br> • Kiểm tra redirect tới Momo gateway <br> • Kiểm tra payment success: order status = Paid <br> • Kiểm tra payment cancelled: order status = Pending Payment <br> • Kiểm tra transaction ID được lưu <br> • Kiểm tra email confirmation sau payment success <br> • Kiểm tra refund functionality (Admin) |
| **Theo dõi đơn hàng thực thời** | • Kiểm tra customer xem vị trí shipper trên map <br> • Kiểm tra location update mỗi 30 giây <br> • Kiểm tra estimated delivery time hiển thị & update <br> • Kiểm tra checkpoint được record với timestamp <br> • Kiểm tra GPS accuracy >= 50m <br> • Kiểm tra handle lost GPS signal |
| **Thông báo Status Đơn hàng** | • Kiểm tra notification gửi khi order created <br> • Kiểm tra notification gửi khi assigned shipper <br> • Kiểm tra notification gửi khi InTransit <br> • Kiểm tra notification gửi khi Delivered <br> • Kiểm tra notification gửi trong 5 giây <br> • Kiểm tra retry nếu send failed (3 times) <br> • Kiểm tra multiple channels (in-app, email, SMS) |
| **Báo cáo & Thống kê (Admin)** | • Kiểm tra generate báo cáo theo ngày/tháng <br> • Kiểm tra thống kê số đơn hàng, doanh thu <br> • Kiểm tra thống kê theo shipper performance <br> • Kiểm tra export báo cáo (Excel, PDF) <br> • Kiểm tra filter báo cáo (date range, status) <br> • Kiểm tra dữ liệu báo cáo chính xác |

---

## 2.4 PHƯƠNG PHÁP KIỂM THỬ

### 2.4.1 Loại Kiểm thử

• **Happy Path Testing:** Kiểm thử các tình huống bình thường, thành công
• **Negative Testing:** Kiểm thử với invalid input, error handling
• **Boundary Testing:** Kiểm thử giới hạn (min, max values)
• **Performance Testing:** Kiểm thử tốc độ, load, memory leak
• **Security Testing:** SQL Injection, XSS, CSRF, brute force
• **Usability Testing:** Kiểm thử giao diện, user experience
• **Regression Testing:** Đảm bảo bug cũ không tái diễn

### 2.4.2 Công cụ Kiểm thử

**Automated Testing:**
• Selenium: E2E testing trên web
• JUnit / MSTest: Unit testing
• Postman: API testing
• JMeter: Load & Performance testing
• OWASP ZAP: Security testing

**Manual Testing:**
• Test case execution
• Exploratory testing
• Usability testing
• User Acceptance Testing (UAT)

---

## 2.5 TIÊU CHÍ CHẤP NHẬN (Acceptance Criteria)

| Tiêu chí | Yêu cầu | Ghi chú |
|----------|---------|--------|
| **Functional Pass Rate** | >= 95% | Tất cả test cases phải pass |
| **Performance** | 95% page loads <= 2s | Desktop với internet thông thường |
| **Security** | 0 Critical/High vulnerabilities | OWASP Top 10 compliant |
| **Usability** | SUS score >= 70 | User testing feedback |
| **Reliability** | 99.5% uptime | Tối đa 3.6 giờ/tháng downtime |
| **Data Integrity** | 0 data loss | ACID compliance |
| **Response Time** | 95% API calls <= 500ms | 95th percentile |
| **Coverage** | >= 80% code coverage | Unit test coverage |

---

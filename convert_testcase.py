#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Script convert Test Case từ Scenario New → TestCase Chi tiết (IMPROVED V2)
Input: Scenario New - Test Scenario.csv
Output: TestCase Chi tiết - TestCase.csv (filled)
Uses scenario mapping to properly inherit LV1/LV2 for all test cases
"""

import csv
import os

# ============= KNOWLEDGE BASE (từ code analysis) =============
# Validation Rules từ code
AUTH_RULES = {
    'username_min': 5,
    'username_pattern': '^[a-zA-Z0-9_]{5,}$',
    'password_min': 8,
    'password_requires': ['uppercase', 'lowercase', 'digit', 'special_char'],
    'password_special_chars': '!@#$%^&*()_+-=[]{};\':"|,.<>/?',
    '2fa_expiry': '5 phút',
    'jwt_expiry': '60 phút',
    'forgot_password_token_expiry': '15 phút'
}

ORDER_RULES = {
    'order_status': {
        0: 'ChuaNhan',
        1: 'DaNhanChuaGiao', 
        2: 'DaNhanDangGiao',
        3: 'DaGiao'
    },
    'payment_methods': {
        0: 'GuiThuong (COD)',
        1: 'GuiNhanh',
        2: 'ChuyenKhoan',
        3: 'ThanhToanTrucTuyen'
    },
    'package_types': {
        0: 'Gói nhỏ',
        1: 'Gói tiêu chuẩn',
        11: 'Xe'
    },
    'order_code_format': 'DH{yyyyMMddHHmmssfff}{nnn}'
}

PRIORITY_MAP = {
    'Aut_DangNhap': 'High',      # Đăng nhập là chức năng quan trọng
    'Aut_DangKy': 'High',         # Đăng ký cũng quan trọng
    'Ord_ThêmDH': 'High',         # Tạo đơn hàng là chính
    'Ord_LocDon': 'High',
    'Ord_ChiTietDH': 'High',
    'Cus_': 'Medium',             # Customer management
    'Pro_': 'Medium',             # Profile
    'Noti_': 'Medium',            # Notifications
    'CBot_': 'Medium',            # Chatbot
    'Chat_': 'Medium',            # Chat
    'Re_': 'Low',                 # Reports
    'Sta_': 'Low'                 # Staff
}

# ============= STEP TEMPLATES =============
STEP_TEMPLATES = {
    'Login': [
        ('1', 'Mở trang đăng nhập', 'URL: /login'),
        ('2', 'Nhập email/username vào field "Tên đăng nhập"', 'Email/Username: {test_data}'),
        ('3', 'Nhập mật khẩu vào field "Mật khẩu"', 'Password: {test_data}'),
        ('4', 'Click nút "Đăng nhập"', 'Button ID: btn-login')
    ],
    'Register': [
        ('1', 'Mở trang đăng ký', 'URL: /register'),
        ('2', 'Nhập First Name', 'First Name: {test_data}'),
        ('3', 'Nhập Last Name', 'Last Name: {test_data}'),
        ('4', 'Nhập Email', 'Email: {test_data}'),
        ('5', 'Nhập Password', 'Password: {test_data}'),
        ('6', 'Nhập Confirm Password', 'Confirm Pass: {test_data}'),
        ('7', 'Chọn Gender (Radio Button)', 'Gender: {test_data}'),
        ('8', 'Click nút "Register"', 'Button ID: btn-register')
    ],
    'CreateOrder': [
        ('1', 'Mở trang tạo đơn hàng', 'URL: /orders/create'),
        ('2', 'Nhập tên khách hàng', 'Customer Name: {test_data}'),
        ('3', 'Nhập số điện thoại', 'Phone: {test_data}'),
        ('4', 'Nhập địa chỉ giao', 'Address: {test_data}'),
        ('5', 'Chọn Tỉnh/Thành phố', 'Province: {test_data}'),
        ('6', 'Chọn Quận/Huyện', 'District: {test_data}'),
        ('7', 'Chọn Phường/Xã', 'Ward: {test_data}'),
        ('8', 'Chọn loại bao bì', 'Package Type: {test_data}'),
        ('9', 'Nhập trọng lượng', 'Weight (kg): {test_data}'),
        ('10', 'Nhập khoảng cách', 'Distance (km): {test_data}'),
        ('11', 'Chọn phương thức thanh toán', 'Payment: {test_data}'),
        ('12', 'Click nút "Tạo đơn"', 'Button ID: btn-create-order'),
    ]
}

# ============= EXPECTED RESULT TEMPLATES =============
EXPECTED_RESULTS = {
    'Login_Success': 'Đăng nhập thành công. Hệ thống chuyển hướng tới trang Dashboard. JWT Token được lưu trong LocalStorage/HTTPOnly Cookie.',
    'Login_Fail_WrongPassword': 'Hiển thị thông báo lỗi màu đỏ: "Sai mật khẩu. Vui lòng thử lại." Form không được submit.',
    'Login_Fail_UserNotFound': 'Hiển thị thông báo lỗi: "Email/username không tồn tại trong hệ thống."',
    'Login_Fail_EmptyField': 'Form không được submit. Hiển thị thông báo validation: "Vui lòng nhập email/username và mật khẩu."',
    'Logout_Success': 'Đăng xuất thành công. JWT Token được xóa. Chuyển hướng tới trang /login.',
    'Register_Success': 'Tài khoản được tạo thành công. Gửi email verification. Chuyển hướng tới trang xác nhận email.',
    'Register_Fail_EmailExists': 'Hiển thị thông báo lỗi: "Email này đã tồn tại trong hệ thống."',
    'Register_Fail_PasswordMismatch': 'Hiển thị thông báo lỗi: "Mật khẩu xác nhận không khớp."',
    'Order_Created_Success': 'Đơn hàng được tạo thành công. OrderCode tự động sinh: DH{yyyyMMddHHmmssfff}{nnn}. Gửi thông báo cho admin và khách. Chuyển hướng tới trang chi tiết đơn.',
    'Order_Fail_MissingField': 'Form không được submit. Hiển thị validation lỗi trên các trường trống/sai.',
}

# ============= HELPER FUNCTIONS =============

def get_priority(big_item):
    """Xác định Priority dựa trên Big Item"""
    for key, priority in PRIORITY_MAP.items():
        if big_item.startswith(key):
            return priority
    return 'Medium'

def get_precondition(scenario_id_lv2):
    """Tạo Pre-condition cho từng loại test case"""
    preconditions = {
        'Aut_DangNhap': 'User chưa đăng nhập. Trình duyệt có Internet. Tài khoản test khả dụng.',
        'Aut_DangKy': 'User chưa có tài khoản. Trình duyệt có Internet.',
        'Aut_QuenMK': 'User có tài khoản. Email khác dụng. Trình duyệt có Internet.',
        'Ord_ThêmDH': 'User đã đăng nhập. JWT Token hợp lệ. Truy cập từ Admin hoặc Customer role.',
        'Ord_LocDon': 'User đã đăng nhập. Đã có ít nhất 1 đơn hàng trong hệ thống.',
        'Ord_ChiTietDH': 'User đã đăng nhập. Đã có ít nhất 1 đơn hàng được tạo.',
        'Cus_ThemKH': 'User là Admin. JWT Token hợp lệ. Truy cập được trang quản lý khách hàng.',
        'Pro_CapNhatThongTin': 'User đã đăng nhập. JWT Token hợp lệ.',
        'Chat_TC': 'User đã đăng nhập. Có ít nhất 2 user đang online.',
        'Noti_RealTime': 'User đã đăng nhập. Kết nối SignalR thành công.',
    }
    
    for key in preconditions:
        if scenario_id_lv2.startswith(key if key.endswith('_') else key.split('_')[0]):
            return preconditions[key]
    
    return 'User đã đăng nhập. Hệ thống sẵn sàng.'

def generate_steps_and_data(test_case_id, description):
    """Tạo chi tiết Step/Action/TestData từ description"""
    description_lower = description.lower()
    
    steps = []
    expected = ''
    
    # ===== AUTHENTICATION =====
    if 'Aut_DN' in test_case_id:  # Login
        if 'thành công' in description_lower and 'hợp lệ' in description_lower:
            return (
                [('1', 'Mở trang /login', 'URL: http://localhost:3000/login'),
                 ('2', 'Nhập email/username hợp lệ', 'Email: test@example.com hoặc Username: testuser1'),
                 ('3', 'Nhập mật khẩu hợp lệ', 'Password: Pass@123456'),
                 ('4', 'Click nút Đăng nhập', 'Button ID: btn-login')],
                EXPECTED_RESULTS['Login_Success']
            )
        elif 'sai mật khẩu' in description_lower:
            return (
                [('1', 'Mở trang /login', 'URL: http://localhost:3000/login'),
                 ('2', 'Nhập email hợp lệ', 'Email: test@example.com'),
                 ('3', 'Nhập mật khẩu sai', 'Password: WrongPass123'),
                 ('4', 'Click nút Đăng nhập', 'Button ID: btn-login')],
                EXPECTED_RESULTS['Login_Fail_WrongPassword']
            )
        elif 'email chưa tồn tại' in description_lower or 'email không tồn tại' in description_lower:
            return (
                [('1', 'Mở trang /login', 'URL: http://localhost:3000/login'),
                 ('2', 'Nhập email không tồn tại', 'Email: nonexistent@example.com'),
                 ('3', 'Nhập bất kỳ mật khẩu', 'Password: Pass@123456'),
                 ('4', 'Click nút Đăng nhập', 'Button ID: btn-login')],
                EXPECTED_RESULTS['Login_Fail_UserNotFound']
            )
        elif 'để trống tất cả' in description_lower:
            return (
                [('1', 'Mở trang /login', 'URL: http://localhost:3000/login'),
                 ('2', 'Để trống trường Email', 'Email: (rỗng)'),
                 ('3', 'Để trống trường Password', 'Password: (rỗng)'),
                 ('4', 'Click nút Đăng nhập', 'Button ID: btn-login')],
                EXPECTED_RESULTS['Login_Fail_EmptyField']
            )
        elif 'để trống' in description_lower and 'password' in description_lower:
            return (
                [('1', 'Mở trang /login', 'URL: http://localhost:3000/login'),
                 ('2', 'Nhập email hợp lệ', 'Email: test@example.com'),
                 ('3', 'Để trống trường Password', 'Password: (rỗng)'),
                 ('4', 'Click nút Đăng nhập', 'Button ID: btn-login')],
                EXPECTED_RESULTS['Login_Fail_EmptyField']
            )
        elif 'email sai định dạng' in description_lower:
            return (
                [('1', 'Mở trang /login', 'URL: http://localhost:3000/login'),
                 ('2', 'Nhập email sai định dạng', 'Email: invalidemail (không có @)'),
                 ('3', 'Nhập mật khẩu', 'Password: Pass@123456'),
                 ('4', 'Click nút Đăng nhập hoặc rời khỏi field', 'Button/Field blur')],
                'Form không được submit hoặc hiển thị lỗi validation: "Email không hợp lệ."'
            )
        elif 'đăng xuất' in description_lower:
            return (
                [('1', 'Đã đăng nhập sẵn', 'JWT Token hợp lệ'),
                 ('2', 'Tìm nút Đăng xuất (Logout)', 'Button ID: btn-logout'),
                 ('3', 'Click nút Đăng xuất', 'Button click')],
                EXPECTED_RESULTS['Logout_Success']
            )
        elif 'remember me' in description_lower:
            return (
                [('1', 'Mở trang /login', 'URL: http://localhost:3000/login'),
                 ('2', 'Nhập email', 'Email: test@example.com'),
                 ('3', 'Nhập mật khẩu', 'Password: Pass@123456'),
                 ('4', 'Check checkbox "Remember me"', 'Checkbox ID: remember-me'),
                 ('5', 'Click Đăng nhập', 'Button click')],
                'Đăng nhập thành công. Cookie được lưu để session duy trì.'
            )
        elif 'phím enter' in description_lower:
            return (
                [('1', 'Mở trang /login', 'URL: http://localhost:3000/login'),
                 ('2', 'Nhập email', 'Email: test@example.com'),
                 ('3', 'Nhập mật khẩu', 'Password: Pass@123456'),
                 ('4', 'Nhấn phím Enter', 'Key: Enter')],
                'Đăng nhập thành công như click nút. Chuyển hướng tới Dashboard.'
            )
        elif 'ẩn ký tự' in description_lower:
            return (
                [('1', 'Mở trang /login', 'URL: http://localhost:3000/login'),
                 ('2', 'Nhập mật khẩu vào field', 'Password: Pass@123456')],
                'Field password hiển thị dấu * thay vì chữ thực. Nút "Hiển thị/Ẩn" (eye icon) khả dụng.'
            )
        elif 'tài khoản đã bị khóa' in description_lower:
            return (
                [('1', 'Mở trang /login', 'URL: http://localhost:3000/login'),
                 ('2', 'Nhập email tài khoản bị khóa', 'Email: locked@example.com'),
                 ('3', 'Nhập mật khẩu', 'Password: Pass@123456'),
                 ('4', 'Click Đăng nhập', 'Button click')],
                'Hiển thị thông báo: "Tài khoản của bạn đã bị khóa. Liên hệ admin."'
            )
    
    # ===== ORDER MANAGEMENT =====
    elif 'Ord_ThemDH' in test_case_id:  # Create Order
        if 'thành công' in description_lower and 'hợp lệ' in description_lower:
            return (
                [('1', 'Mở trang /orders/create', 'URL: http://localhost:3000/orders/create'),
                 ('2', 'Nhập tên khách hàng', 'Name: Nguyễn Văn A'),
                 ('3', 'Nhập số điện thoại', 'Phone: 0912345678'),
                 ('4', 'Nhập địa chỉ', 'Address: 123 Đường ABC, TP.HCM'),
                 ('5', 'Chọn Tỉnh/Thành phố', 'Province: TP. Hồ Chí Minh'),
                 ('6', 'Chọn Quận/Huyện', 'District: Quận 1'),
                 ('7', 'Chọn Phường/Xã', 'Ward: Phường Bến Nghé'),
                 ('8', 'Chọn loại bao bì', 'Package: Gói tiêu chuẩn'),
                 ('9', 'Nhập trọng lượng', 'Weight: 5 kg'),
                 ('10', 'Chọn phương thức thanh toán', 'Payment: COD (Gửi thường)'),
                 ('11', 'Click nút Tạo đơn', 'Button click')],
                EXPECTED_RESULTS['Order_Created_Success']
            )
        elif 'để trống tên khách' in description_lower:
            return (
                [('1', 'Mở trang /orders/create', 'URL: http://localhost:3000/orders/create'),
                 ('2', 'Để trống tên khách hàng', 'Name: (rỗng)'),
                 ('3', 'Điền các trường khác hợp lệ', 'Fill other fields...'),
                 ('4', 'Click Tạo đơn', 'Button click')],
                EXPECTED_RESULTS['Order_Fail_MissingField']
            )
    
    # ===== AUTHENTICATION - REGISTER =====
    elif 'Aut_DK' in test_case_id:  # Register
        if 'thành công' in description_lower and 'hợp lệ' in description_lower:
            return (
                [('1', 'Mở trang /register', 'URL: http://localhost:3000/register'),
                 ('2', 'Nhập First Name', 'First Name: Nguyễn'),
                 ('3', 'Nhập Last Name', 'Last Name: Văn A'),
                 ('4', 'Nhập Email', 'Email: nguyenvana@example.com'),
                 ('5', 'Nhập Password', 'Password: Pass@123456 (min 8 chars, upper, lower, digit, special)'),
                 ('6', 'Nhập Confirm Password', 'Confirm Pass: Pass@123456'),
                 ('7', 'Chọn Gender', 'Gender: Male'),
                 ('8', 'Click nút Register', 'Button ID: btn-register')],
                'Tài khoản tạo thành công. Email verification được gửi. Chuyển tới trang xác nhận email.'
            )
        elif 'để trống toàn bộ' in description_lower:
            return (
                [('1', 'Mở trang /register', 'URL: http://localhost:3000/register'),
                 ('2', 'Để trống First Name', 'First Name: (rỗng)'),
                 ('3', 'Để trống Last Name', 'Last Name: (rỗng)'),
                 ('4', 'Để trống Email', 'Email: (rỗng)'),
                 ('5', 'Để trống Password', 'Password: (rỗng)'),
                 ('6', 'Để trống Confirm Password', 'Confirm Pass: (rỗng)'),
                 ('7', 'Click nút Register', 'Button ID: btn-register')],
                'Form không được submit. Validation errors hiển thị cho tất cả trường.'
            )
        elif 'email đã tồn tại' in description_lower:
            return (
                [('1', 'Mở trang /register', 'URL: http://localhost:3000/register'),
                 ('2', 'Nhập First Name', 'First Name: Nguyễn'),
                 ('3', 'Nhập Last Name', 'Last Name: Văn A'),
                 ('4', 'Nhập Email đã tồn tại', 'Email: admin@example.com'),
                 ('5', 'Nhập Password hợp lệ', 'Password: Pass@123456'),
                 ('6', 'Nhập Confirm Password', 'Confirm Pass: Pass@123456'),
                 ('7', 'Chọn Gender', 'Gender: Male'),
                 ('8', 'Click Register', 'Button ID: btn-register')],
                'Thông báo lỗi: "Email này đã tồn tại trong hệ thống."'
            )
        elif 'mật khẩu xác nhận không khớp' in description_lower:
            return (
                [('1', 'Mở trang /register', 'URL: http://localhost:3000/register'),
                 ('2', 'Nhập First Name', 'First Name: Nguyễn'),
                 ('3', 'Nhập Last Name', 'Last Name: Văn A'),
                 ('4', 'Nhập Email', 'Email: nguyenvana2@example.com'),
                 ('5', 'Nhập Password', 'Password: Pass@123456'),
                 ('6', 'Nhập Confirm Password khác', 'Confirm Pass: DifferentPass@123'),
                 ('7', 'Chọn Gender', 'Gender: Female'),
                 ('8', 'Click Register', 'Button ID: btn-register')],
                'Thông báo lỗi: "Mật khẩu xác nhận không khớp với mật khẩu."'
            )
        elif 'email sai định dạng' in description_lower or 'thiếu @' in description_lower:
            return (
                [('1', 'Mở trang /register', 'URL: http://localhost:3000/register'),
                 ('2', 'Nhập First Name', 'First Name: Nguyễn'),
                 ('3', 'Nhập Last Name', 'Last Name: Văn A'),
                 ('4', 'Nhập Email sai định dạng', 'Email: invalidemail'),
                 ('5', 'Nhập Password', 'Password: Pass@123456'),
                 ('6', 'Nhập Confirm Password', 'Confirm Pass: Pass@123456'),
                 ('7', 'Click Register hoặc rời field', 'Button/Blur event')],
                'Thông báo lỗi: "Email không hợp lệ. Vui lòng nhập email đúng định dạng."'
            )
        elif 'để trống' in description_lower and 'first name' in description_lower:
            return (
                [('1', 'Mở trang /register', 'URL: http://localhost:3000/register'),
                 ('2', 'Để trống First Name', 'First Name: (rỗng)'),
                 ('3', 'Nhập Last Name', 'Last Name: Văn A'),
                 ('4', 'Nhập Email', 'Email: test@example.com'),
                 ('5', 'Nhập Password', 'Password: Pass@123456'),
                 ('6', 'Nhập Confirm Password', 'Confirm Pass: Pass@123456'),
                 ('7', 'Click Register', 'Button ID: btn-register')],
                'Thông báo lỗi: "Vui lòng nhập first name."'
            )
        elif 'để trống' in description_lower and 'last name' in description_lower:
            return (
                [('1', 'Mở trang /register', 'URL: http://localhost:3000/register'),
                 ('2', 'Nhập First Name', 'First Name: Nguyễn'),
                 ('3', 'Để trống Last Name', 'Last Name: (rỗng)'),
                 ('4', 'Nhập Email', 'Email: test@example.com'),
                 ('5', 'Nhập Password', 'Password: Pass@123456'),
                 ('6', 'Nhập Confirm Password', 'Confirm Pass: Pass@123456'),
                 ('7', 'Click Register', 'Button ID: btn-register')],
                'Thông báo lỗi: "Vui lòng nhập last name."'
            )
        elif 'để trống' in description_lower and 'email' in description_lower and 'trường' in description_lower:
            return (
                [('1', 'Mở trang /register', 'URL: http://localhost:3000/register'),
                 ('2', 'Nhập First Name', 'First Name: Nguyễn'),
                 ('3', 'Nhập Last Name', 'Last Name: Văn A'),
                 ('4', 'Để trống Email', 'Email: (rỗng)'),
                 ('5', 'Nhập Password', 'Password: Pass@123456'),
                 ('6', 'Nhập Confirm Password', 'Confirm Pass: Pass@123456'),
                 ('7', 'Click Register', 'Button ID: btn-register')],
                'Thông báo lỗi: "Vui lòng nhập email."'
            )
        elif 'để trống' in description_lower and 'password' in description_lower and 'confirm' not in description_lower:
            return (
                [('1', 'Mở trang /register', 'URL: http://localhost:3000/register'),
                 ('2', 'Nhập First Name', 'First Name: Nguyễn'),
                 ('3', 'Nhập Last Name', 'Last Name: Văn A'),
                 ('4', 'Nhập Email', 'Email: test@example.com'),
                 ('5', 'Để trống Password', 'Password: (rỗng)'),
                 ('6', 'Nhập Confirm Password', 'Confirm Pass: Pass@123456'),
                 ('7', 'Click Register', 'Button ID: btn-register')],
                'Thông báo lỗi: "Vui lòng nhập mật khẩu."'
            )
        elif 'để trống' in description_lower and 'confirm' in description_lower:
            return (
                [('1', 'Mở trang /register', 'URL: http://localhost:3000/register'),
                 ('2', 'Nhập First Name', 'First Name: Nguyễn'),
                 ('3', 'Nhập Last Name', 'Last Name: Văn A'),
                 ('4', 'Nhập Email', 'Email: test@example.com'),
                 ('5', 'Nhập Password', 'Password: Pass@123456'),
                 ('6', 'Để trống Confirm Password', 'Confirm Pass: (rỗng)'),
                 ('7', 'Click Register', 'Button ID: btn-register')],
                'Thông báo lỗi: "Vui lòng xác nhận mật khẩu."'
            )
        elif 'độ dài password tối thiểu' in description_lower or '< 6' in description_lower:
            return (
                [('1', 'Mở trang /register', 'URL: http://localhost:3000/register'),
                 ('2', 'Nhập First Name', 'First Name: Nguyễn'),
                 ('3', 'Nhập Last Name', 'Last Name: Văn A'),
                 ('4', 'Nhập Email', 'Email: test@example.com'),
                 ('5', 'Nhập Password quá ngắn', 'Password: Pass1 (5 chars < 8 required)'),
                 ('6', 'Nhập Confirm Password', 'Confirm Pass: Pass1'),
                 ('7', 'Click Register', 'Button ID: btn-register')],
                'Thông báo lỗi: "Mật khẩu phải có ít nhất 8 ký tự."'
            )
        elif 'radio button' in description_lower and 'gender' in description_lower:
            return (
                [('1', 'Mở trang /register', 'URL: http://localhost:3000/register'),
                 ('2', 'Nhập các field khác hợp lệ', 'First/Last/Email/Password filled'),
                 ('3', 'Click Radio Button Male', 'Radio: Male checked'),
                 ('4', 'Click Radio Button Female', 'Radio: Female checked'),
                 ('5', 'Click Radio Button Other', 'Radio: Other checked'),
                 ('6', 'Verify chỉ 1 được chọn', 'Only 1 radio selected at a time')],
                'Radio buttons hoạt động đúng: chỉ 1 option được chọn.'
            )
        elif 'responsive' in description_lower and 'mobile' in description_lower:
            return (
                [('1', 'Mở trang /register trên Mobile', 'Device: Mobile (375x667)'),
                 ('2', 'Kiểm tra layout form', 'Form fields theo chiều dọc'),
                 ('3', 'Kiểm tra input fields responsive', 'Width: 100% của container'),
                 ('4', 'Kiểm tra buttons', 'Register button: full-width'),
                 ('5', 'Kiểm tra spacing', 'Margin/padding hợp lệ')],
                'Giao diện responsive đúng trên Mobile: các element rõ ràng, không bị cắt.'
            )
        elif 'quay lại' in description_lower or 'back' in description_lower:
            return (
                [('1', 'Ở trang /register', 'Current page: /register'),
                 ('2', 'Tìm liên kết "Quay lại / Đăng nhập"', 'Link text: "Đã có tài khoản?"'),
                 ('3', 'Click liên kết', 'Link click')],
                'Chuyển hướng tới trang /login thành công.'
            )
    
    # ===== ORDER MANAGEMENT - ADD ORDER =====
    elif 'Ord_ThemDH' in test_case_id:
        if 'thành công' in description_lower and 'hợp lệ' in description_lower:
            return (
                [('1', 'Mở trang /orders/create', 'URL: http://localhost:3000/orders/create'),
                 ('2', 'Nhập tên khách hàng', 'Customer Name: Nguyễn Văn A'),
                 ('3', 'Nhập số điện thoại', 'Phone: 0912345678'),
                 ('4', 'Nhập địa chỉ giao hàng', 'Address: 123 Đường ABC'),
                 ('5', 'Chọn Tỉnh/Thành phố', 'Province: TP. Hồ Chí Minh'),
                 ('6', 'Chọn Quận/Huyện', 'District: Quận 1'),
                 ('7', 'Chọn Phường/Xã', 'Ward: Phường Bến Nghé'),
                 ('8', 'Chọn loại bao bì', 'Package: Gói tiêu chuẩn'),
                 ('9', 'Nhập trọng lượng', 'Weight: 5 kg'),
                 ('10', 'Nhập khoảng cách', 'Distance: 10 km'),
                 ('11', 'Chọn phương thức thanh toán', 'Payment: Gửi thường (COD)'),
                 ('12', 'Click nút Tạo đơn', 'Button ID: btn-create-order')],
                'Đơn tạo thành công. OrderCode tự động sinh: DH{yyyyMMddHHmmssfff}{nnn}. Gửi thông báo. Chuyển tới chi tiết đơn.'
            )
        elif 'để trống tên khách' in description_lower:
            return (
                [('1', 'Mở trang /orders/create', 'URL: http://localhost:3000/orders/create'),
                 ('2', 'Để trống Customer Name', 'Customer Name: (rỗng)'),
                 ('3', 'Điền các trường khác hợp lệ', 'Fill: Phone, Address, Province, District, Ward, Package, Weight, Distance, Payment'),
                 ('4', 'Click Tạo đơn', 'Button click')],
                'Form không submit. Validation error: "Vui lòng nhập tên khách hàng."'
            )
        elif 'để trống số điện thoại' in description_lower:
            return (
                [('1', 'Mở trang /orders/create', 'URL'),
                 ('2', 'Nhập Customer Name', 'Customer Name: Nguyễn Văn A'),
                 ('3', 'Để trống Phone', 'Phone: (rỗng)'),
                 ('4', 'Điền các trường khác', 'Fill: Address, Location, Package, Weight, Distance, Payment'),
                 ('5', 'Click Tạo đơn', 'Button click')],
                'Validation error: "Vui lòng nhập số điện thoại."'
            )
        elif 'để trống địa chỉ' in description_lower and 'giao' in description_lower:
            return (
                [('1', 'Mở trang /orders/create', 'URL'),
                 ('2', 'Nhập Customer Name', 'Customer Name: Nguyễn Văn A'),
                 ('3', 'Nhập Phone', 'Phone: 0912345678'),
                 ('4', 'Để trống Address', 'Address: (rỗng)'),
                 ('5', 'Điền các trường khác', 'Fill other fields'),
                 ('6', 'Click Tạo đơn', 'Button click')],
                'Validation error: "Vui lòng nhập địa chỉ giao hàng."'
            )
        elif 'để trống phường' in description_lower:
            return (
                [('1', 'Mở trang /orders/create', 'URL'),
                 ('2', 'Nhập Customer Name, Phone, Address', 'Filled'),
                 ('3', 'Chọn Province/District', 'Filled'),
                 ('4', 'Để trống Ward', 'Ward: (rỗng)'),
                 ('5', 'Điền các trường khác', 'Package, Weight, Distance, Payment'),
                 ('6', 'Click Tạo đơn', 'Button click')],
                'Validation error: "Vui lòng chọn phường/xã."'
            )
        elif 'để trống quận' in description_lower or 'để trống huyện' in description_lower:
            return (
                [('1', 'Mở trang /orders/create', 'URL'),
                 ('2', 'Nhập Customer Name, Phone, Address', 'Filled'),
                 ('3', 'Chọn Province', 'Province selected'),
                 ('4', 'Để trống District', 'District: (rỗng)'),
                 ('5', 'Điền các trường khác', 'Filled'),
                 ('6', 'Click Tạo đơn', 'Button click')],
                'Validation error: "Vui lòng chọn quận/huyện."'
            )
        elif 'để trống tỉnh' in description_lower or 'để trống thành phố' in description_lower:
            return (
                [('1', 'Mở trang /orders/create', 'URL'),
                 ('2', 'Nhập Customer Name, Phone, Address', 'Filled'),
                 ('3', 'Để trống Province', 'Province: (rỗng)'),
                 ('4', 'Điền các trường khác', 'Filled'),
                 ('5', 'Click Tạo đơn', 'Button click')],
                'Validation error: "Vui lòng chọn tỉnh/thành phố."'
            )
        elif 'để trống loại bao bì' in description_lower:
            return (
                [('1', 'Mở trang /orders/create', 'URL'),
                 ('2', 'Nhập Customer Name, Phone, Address, Location', 'Filled'),
                 ('3', 'Để trống Package Type', 'Package: (rỗng)'),
                 ('4', 'Điền các trường khác', 'Weight, Distance, Payment'),
                 ('5', 'Click Tạo đơn', 'Button click')],
                'Validation error: "Vui lòng chọn loại bao bì."'
            )
        elif 'loại bao bì = gói nhỏ' in description_lower or '= 0' in description_lower:
            return (
                [('1', 'Mở trang /orders/create', 'URL'),
                 ('2', 'Nhập các thông tin hợp lệ', 'Customer, Phone, Address, Location'),
                 ('3', 'Chọn Package Type = Gói nhỏ', 'Package: 0 (Gói nhỏ)'),
                 ('4', 'Nhập Weight, Distance, Payment', 'Filled'),
                 ('5', 'Click Tạo đơn', 'Button click')],
                'Đơn được tạo thành công với Package Type = 0.'
            )
        elif 'trọng lượng = 0' in description_lower:
            return (
                [('1', 'Mở trang /orders/create', 'URL'),
                 ('2', 'Nhập Customer, Phone, Address, Location', 'Filled'),
                 ('3', 'Nhập Weight = 0', 'Weight: 0 kg'),
                 ('4', 'Nhập Distance, Payment', 'Filled'),
                 ('5', 'Click Tạo đơn', 'Button click')],
                'Validation error: "Trọng lượng phải lớn hơn 0."'
            )
        elif 'trọng lượng = -5' in description_lower or 'âm' in description_lower:
            return (
                [('1', 'Mở trang /orders/create', 'URL'),
                 ('2', 'Nhập các thông tin trước Weight', 'Customer, Phone, Address, Location, Package'),
                 ('3', 'Nhập Weight = -5', 'Weight: -5 kg'),
                 ('4', 'Nhập Distance, Payment', 'Filled'),
                 ('5', 'Click Tạo đơn', 'Button click')],
                'Validation error: "Trọng lượng không thể âm."'
            )
        elif 'để trống phương thức' in description_lower:
            return (
                [('1', 'Mở trang /orders/create', 'URL'),
                 ('2', 'Nhập tất cả thông tin', 'Customer, Phone, Address, Location, Package, Weight, Distance'),
                 ('3', 'Để trống Payment Method', 'Payment: (rỗng)'),
                 ('4', 'Click Tạo đơn', 'Button click')],
                'Validation error: "Vui lòng chọn phương thức thanh toán."'
            )
        elif 'gửi thường' in description_lower and 'cod' in description_lower:
            return (
                [('1', 'Mở trang /orders/create', 'URL'),
                 ('2', 'Nhập tất cả thông tin hợp lệ', 'Customer, Phone, Address, Location, Package, Weight, Distance'),
                 ('3', 'Chọn Payment = Gửi thường (COD)', 'Payment: 0 (GuiThuong)'),
                 ('4', 'Click Tạo đơn', 'Button click')],
                'Đơn tạo thành công với Payment Method = COD (Gửi thường).'
            )
        elif 'vnpay' in description_lower or 'momo' in description_lower:
            return (
                [('1', 'Mở trang /orders/create', 'URL'),
                 ('2', 'Nhập tất cả thông tin hợp lệ', 'Customer, Phone, Address, Location, Package, Weight, Distance'),
                 ('3', 'Chọn Payment = Thanh toán trực tuyến', 'Payment: 3 (ThanhToanTrucTuyen)'),
                 ('4', 'Chọn Payment Gateway', f'Gateway: {"VNPay" if "vnpay" in description_lower else "Momo"}'),
                 ('5', 'Click Tạo đơn', 'Button click')],
                'Chuyển hướng tới trang thanh toán của VNPay/Momo. Sau thanh toán, đơn được tạo.'
            )
    
    # ===== CUSTOMER MANAGEMENT - ADD CUSTOMER =====
    elif 'Cus_ThemKH' in test_case_id:
        if 'thành công' in description_lower and 'hợp lệ' in description_lower:
            return (
                [('1', 'Mở trang /customers/add', 'URL: http://localhost:3000/customers/add'),
                 ('2', 'Nhập tên khách hàng', 'Customer Name: Nguyễn Văn A'),
                 ('3', 'Nhập email', 'Email: nguyenvana@example.com'),
                 ('4', 'Nhập số điện thoại', 'Phone: 0912345678'),
                 ('5', 'Nhập địa chỉ', 'Address: 123 Đường ABC, TP.HCM'),
                 ('6', 'Chọn Tỉnh/Thành phố', 'Province: TP. Hồ Chí Minh'),
                 ('7', 'Chọn Quận/Huyện', 'District: Quận 1'),
                 ('8', 'Chọn Phường/Xã', 'Ward: Phường Bến Nghé'),
                 ('9', 'Nhập ghi chú (nếu có)', 'Notes: Khách hàng thường xuyên'),
                 ('10', 'Click nút Thêm khách hàng', 'Button ID: btn-add-customer')],
                'Khách hàng được tạo thành công. Chuyển tới trang danh sách khách hàng.'
            )
        elif 'để trống tên' in description_lower:
            return (
                [('1', 'Mở trang /customers/add', 'URL'),
                 ('2', 'Để trống Customer Name', 'Name: (rỗng)'),
                 ('3', 'Nhập Email, Phone, Address, Location', 'Filled'),
                 ('4', 'Click Thêm khách hàng', 'Button click')],
                'Validation error: "Vui lòng nhập tên khách hàng."'
            )
        elif 'để trống email' in description_lower or 'để trống.*email' in description_lower.lower():
            return (
                [('1', 'Mở trang /customers/add', 'URL'),
                 ('2', 'Nhập Customer Name', 'Name: Nguyễn Văn A'),
                 ('3', 'Để trống Email', 'Email: (rỗng)'),
                 ('4', 'Nhập Phone, Address, Location', 'Filled'),
                 ('5', 'Click Thêm khách hàng', 'Button click')],
                'Validation error: "Vui lòng nhập email."'
            )
        elif 'để trống số điện thoại' in description_lower:
            return (
                [('1', 'Mở trang /customers/add', 'URL'),
                 ('2', 'Nhập Customer Name, Email, Address, Location', 'Filled'),
                 ('3', 'Để trống Phone', 'Phone: (rỗng)'),
                 ('4', 'Click Thêm khách hàng', 'Button click')],
                'Validation error: "Vui lòng nhập số điện thoại."'
            )
        elif 'để trống địa chỉ' in description_lower:
            return (
                [('1', 'Mở trang /customers/add', 'URL'),
                 ('2', 'Nhập Customer Name, Email, Phone, Location', 'Filled'),
                 ('3', 'Để trống Address', 'Address: (rỗng)'),
                 ('4', 'Click Thêm khách hàng', 'Button click')],
                'Validation error: "Vui lòng nhập địa chỉ."'
            )
        elif 'email sai định dạng' in description_lower:
            return (
                [('1', 'Mở trang /customers/add', 'URL'),
                 ('2', 'Nhập Customer Name', 'Name: Nguyễn Văn A'),
                 ('3', 'Nhập Email sai định dạng', 'Email: invalidemail'),
                 ('4', 'Nhập Phone, Address, Location', 'Filled'),
                 ('5', 'Click Thêm khách hàng hoặc rời field', 'Button/Blur')],
                'Validation error: "Email không hợp lệ. Vui lòng nhập email đúng định dạng."'
            )
        elif 'email đã tồn tại' in description_lower:
            return (
                [('1', 'Mở trang /customers/add', 'URL'),
                 ('2', 'Nhập Customer Name khác', 'Name: Nguyễn Văn B'),
                 ('3', 'Nhập Email đã tồn tại', 'Email: existing@example.com'),
                 ('4', 'Nhập Phone, Address, Location', 'Filled'),
                 ('5', 'Click Thêm khách hàng', 'Button click')],
                'Validation error: "Email này đã tồn tại trong hệ thống."'
            )
        elif 'số điện thoại sai định dạng' in description_lower:
            return (
                [('1', 'Mở trang /customers/add', 'URL'),
                 ('2', 'Nhập Customer Name, Email, Address, Location', 'Filled'),
                 ('3', 'Nhập Phone sai định dạng', 'Phone: 123 (quá ngắn)'),
                 ('4', 'Click Thêm khách hàng', 'Button click')],
                'Validation error: "Số điện thoại không hợp lệ."'
            )
    
    # ===== STAFF MANAGEMENT - ADD STAFF =====
    elif 'Sta_ThemNV' in test_case_id:
        if ('thành công' in description_lower and 'hợp lệ' in description_lower) or ('thành công' in description_lower and 'để trống' not in description_lower and 'sai' not in description_lower and 'trùng' not in description_lower):
            return (
                [('1', 'Mở trang /staff/add', 'URL: http://localhost:3000/staff/add'),
                 ('2', 'Nhập tên nhân viên', 'Staff Name: Trần Văn C'),
                 ('3', 'Nhập email', 'Email: tranvanc@example.com'),
                 ('4', 'Nhập số điện thoại', 'Phone: 0909876543'),
                 ('5', 'Chọn vị trí công tác', 'Location: TP. Hồ Chí Minh'),
                 ('6', 'Chọn phòng ban', 'Department: Delivery'),
                 ('7', 'Chọn chức vụ', 'Position: Shipper'),
                 ('8', 'Chọn quyền truy cập', 'Role: Delivery Staff'),
                 ('9', 'Nhập lương/hoa hồng', 'Salary: 5000000 VND'),
                 ('10', 'Upload ảnh nhân viên', 'Image file: avatar.jpg'),
                 ('11', 'Click nút Thêm nhân viên', 'Button ID: btn-add-staff')],
                'Nhân viên được tạo thành công. Email mời được gửi. Chuyển tới danh sách nhân viên.'
            )
        elif 'để trống tên' in description_lower:
            return (
                [('1', 'Mở trang /staff/add', 'URL'),
                 ('2', 'Để trống Staff Name', 'Name: (rỗng)'),
                 ('3', 'Nhập Email, Phone, Location, Department, Position, Role', 'Filled'),
                 ('4', 'Click Thêm nhân viên', 'Button click')],
                'Validation error: "Vui lòng nhập tên nhân viên."'
            )
        elif 'để trống email' in description_lower:
            return (
                [('1', 'Mở trang /staff/add', 'URL'),
                 ('2', 'Nhập Staff Name', 'Name: Trần Văn C'),
                 ('3', 'Để trống Email', 'Email: (rỗng)'),
                 ('4', 'Nhập Phone, Location, Department, Position, Role', 'Filled'),
                 ('5', 'Click Thêm nhân viên', 'Button click')],
                'Validation error: "Vui lòng nhập email."'
            )
        elif 'để trống số điện thoại' in description_lower:
            return (
                [('1', 'Mở trang /staff/add', 'URL'),
                 ('2', 'Nhập Staff Name, Email, Location, Department, Position, Role', 'Filled'),
                 ('3', 'Để trống Phone', 'Phone: (rỗng)'),
                 ('4', 'Click Thêm nhân viên', 'Button click')],
                'Validation error: "Vui lòng nhập số điện thoại."'
            )
        elif 'email sai định dạng' in description_lower:
            return (
                [('1', 'Mở trang /staff/add', 'URL'),
                 ('2', 'Nhập Staff Name', 'Name: Trần Văn C'),
                 ('3', 'Nhập Email sai định dạng', 'Email: invalidemail'),
                 ('4', 'Nhập Phone, Location, Department, Position, Role', 'Filled'),
                 ('5', 'Click Thêm nhân viên', 'Button click')],
                'Validation error: "Email không hợp lệ."'
            )
        elif 'email trùng' in description_lower:
            return (
                [('1', 'Mở trang /staff/add', 'URL'),
                 ('2', 'Nhập Staff Name khác', 'Name: Trần Văn D'),
                 ('3', 'Nhập Email đã tồn tại', 'Email: existing@example.com'),
                 ('4', 'Nhập Phone, Location, Department, Position, Role', 'Filled'),
                 ('5', 'Click Thêm nhân viên', 'Button click')],
                'Validation error: "Email này đã được sử dụng."'
            )
        elif 'chọn vị trí' in description_lower:
            return (
                [('1', 'Mở trang /staff/add', 'URL'),
                 ('2', 'Nhập Staff Name, Email, Phone', 'Filled'),
                 ('3', 'Chọn vị trí công tác', 'Location dropdown: Mở menu'),
                 ('4', 'Chọn một location', 'Option: TP. Hồ Chí Minh'),
                 ('5', 'Nhập các trường khác', 'Department, Position, Role'),
                 ('6', 'Click Thêm nhân viên', 'Button click')],
                'Location được chọn thành công. Danh sách Department cập nhật dựa trên location.'
            )
        elif 'chọn phòng ban' in description_lower:
            return (
                [('1', 'Mở trang /staff/add', 'URL'),
                 ('2', 'Nhập Staff Name, Email, Phone', 'Filled'),
                 ('3', 'Chọn Location', 'Location: TP. Hồ Chí Minh'),
                 ('4', 'Chọn phòng ban', 'Department dropdown: Delivery'),
                 ('5', 'Nhập các trường khác', 'Position, Role, Salary'),
                 ('6', 'Click Thêm nhân viên', 'Button click')],
                'Department được chọn. Danh sách Position cập nhật dựa trên department.'
            )
        elif 'chọn chức vụ' in description_lower:
            return (
                [('1', 'Mở trang /staff/add', 'URL'),
                 ('2', 'Nhập Staff Name, Email, Phone', 'Filled'),
                 ('3', 'Chọn Location, Department', 'Filled'),
                 ('4', 'Chọn chức vụ', 'Position dropdown: Shipper'),
                 ('5', 'Nhập các trường khác', 'Role, Salary, Image'),
                 ('6', 'Click Thêm nhân viên', 'Button click')],
                'Position được chọn. Role tự động cập nhật theo chức vụ.'
            )
    
    # ===== DEFAULT =====
    return (
        [('1', description, 'Test Data TBD')],
        'Expected Result TBD'
    )

def read_scenarios(input_file):
    """Đọc file CSV source"""
    scenarios = []
    with open(input_file, 'r', encoding='utf-8-sig') as f:
        reader = csv.DictReader(f)
        for row in reader:
            if row.get('Test Case ID', '').strip():  # Chỉ lấy có Test Case ID
                scenarios.append(row)
    return scenarios

def build_scenario_map(scenarios):
    """Tạo map để biết scenario nào cho mỗi test case - pre-scan file"""
    scenario_map = {}
    current_section = {}
    
    for scenario in scenarios:
        test_id = scenario.get('Test Case ID', '').strip()
        lv1 = scenario.get('Scenario LV1', '').strip()
        lv2 = scenario.get('Scenario ID LV2', '').strip()
        lv3 = scenario.get('Scenario ID LV3', '').strip()
        desc = scenario.get('Scenario Description', '').strip()
        
        # Khi LV1 mới, update current_section (ngay cả khi LV2 trống)
        if lv1:
            current_section['lv1'] = lv1
        
        if lv2:
            current_section['lv2'] = lv2
        elif lv1:  # Nếu LV1 mới nhưng LV2 trống, lấy từ test case ID
            # Extract big item từ test case ID (e.g., "Sta_ThemNV_TC_01" → "Sta_ThemNV")
            if '_TC_' in test_id:
                big_item = test_id.split('_TC_')[0]
                current_section['lv2'] = big_item
        
        if lv3:
            current_section['lv3'] = lv3
        if desc:
            current_section['desc'] = desc
        
        if test_id:
            scenario_map[test_id] = dict(current_section)  # Lưu copy
    
    return scenario_map

def create_detailed_testcases(scenarios):
    """Tạo danh sách detailed test cases"""
    detailed = []
    no = 1
    
    # Pre-scan để build scenario map
    scenario_map = build_scenario_map(scenarios)
    
    for scenario in scenarios:
        test_case_id = scenario.get('Test Case ID', '').strip()
        if not test_case_id:
            continue
        
        # Get từ map
        section = scenario_map.get(test_case_id, {})
        function = section.get('lv1', 'N/A')
        big_item = section.get('lv2', 'N/A')
        medium_item = section.get('desc', big_item)
        small_item = scenario.get('Description Testcase', '').strip()
        
        # Generate Steps, Action, TestData, ExpectedResult
        steps_list, expected_result = generate_steps_and_data(test_case_id, small_item)
        
        # Pre-condition
        precondition = get_precondition(big_item)
        
        # Priority
        priority = get_priority(big_item)
        
        # Tạo một hàng cho mỗi step
        for step_num, (step_id, step_action, test_data) in enumerate(steps_list, 1):
            detailed.append({
                'NO': no,
                'Test Case ID': test_case_id if step_num == 1 else '',
                'Function': function if step_num == 1 else '',
                'Big Item': big_item if step_num == 1 else '',
                'Medium Item': medium_item if step_num == 1 else '',
                'Small Item': small_item if step_num == 1 else '',
                'Pre-condition': precondition if step_num == 1 else '',
                'Step': step_id,
                'Step action': step_action,
                'Test Data': test_data,
                'Expected Result': expected_result if step_num == len(steps_list) else '',
                'Actual Result': '',
                'Status': '',
                'Notes': '',
                'Test Priority': priority if step_num == 1 else ''
            })
            no += 1
    
    return detailed

def write_output(output_file, detailed_testcases):
    """Ghi vào file CSV output"""
    # Header từ template
    header_rows = [
        ['', '', '', '', '', '', '', '', '', '', '', '', '', '', ''],
        ['', '', '', '', '', '', '', '', '', '', '', '', '', '', ''],
        ['Tên dự án:', 'Delivery Management System', 'Số test case:', len([x for x in detailed_testcases if x['Test Case ID']]), '', '', '', '', '', '', '', '', '', '', ''],
        ['', '', 'Số test case PASS:', '?', '', '', '', '', '', '', '', '', '', '', ''],
        ['', '', 'Số testcase FAIL:', '?', '', '', '', '', '', '', '', '', '', '', ''],
        ['', '', '', '', '', '', '', '', '', '', '', '', '', '', ''],
    ]
    
    with open(output_file, 'w', newline='', encoding='utf-8') as f:
        writer = csv.writer(f)
        
        # Ghi header rows
        for row in header_rows:
            writer.writerow(row)
        
        # Ghi column headers
        writer.writerow(['NO.', 'Test Case ID', 'Function', 'Big Item', 'Medium Item', 'Small Item', 
                        'Pre-condition', 'Step', 'Step action', 'Test Data', 'Expected Result', 
                        'Actual Result', 'Status', 'Notes', 'Test Priority'])
        
        # Ghi dữ liệu
        for tc in detailed_testcases:
            writer.writerow([
                tc['NO'],
                tc['Test Case ID'],
                tc['Function'],
                tc['Big Item'],
                tc['Medium Item'],
                tc['Small Item'],
                tc['Pre-condition'],
                tc['Step'],
                tc['Step action'],
                tc['Test Data'],
                tc['Expected Result'],
                tc['Actual Result'],
                tc['Status'],
                tc['Notes'],
                tc['Test Priority']
            ])

# ============= MAIN =============
if __name__ == '__main__':
    input_file = r'c:\Users\DELL\Documents\GitHub\20_HTQLGH\Scenario New - Test Scenario.csv'
    output_file = r'c:\Users\DELL\Documents\GitHub\20_HTQLGH\TestCase Chi tiết - TestCase.csv'
    
    print('🔄 Đang đọc file scenario...')
    scenarios = read_scenarios(input_file)
    print(f'   ✓ Đã đọc {len(scenarios)} test case')
    
    print('🔄 Đang build scenario map...')
    scenario_map = build_scenario_map(scenarios)
    print(f'   ✓ Mapped {len(scenario_map)} test cases')
    
    print('🔄 Đang tạo detailed test case...')
    detailed = create_detailed_testcases(scenarios)
    print(f'   ✓ Đã tạo {len(detailed)} hàng (gồm cả nhiều step)')
    
    print(f'💾 Đang ghi vào {output_file}...')
    write_output(output_file, detailed)
    print(f'   ✓ Hoàn thành!')
    
    total_tcs = len([x for x in detailed if x['Test Case ID']])
    print(f'\n📊 Thống kê:')
    print(f'   - Total test cases: {total_tcs}')
    print(f'   - Total rows (steps): {len(detailed)}')

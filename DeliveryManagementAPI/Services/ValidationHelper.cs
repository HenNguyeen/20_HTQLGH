using System.Text.RegularExpressions;

namespace DeliveryManagementAPI.Services
{
    /// <summary>
    /// Helper class để xác định tính hợp lệ của các trường form đăng ký
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Xác định tính hợp lệ của Họ và tên
        /// - Bắt buộc (không được trống)
        /// - Độ dài: 2 đến 100 ký tự
        /// - Chỉ chứa chữ cái (a-z, A-Z) và khoảng trắng
        /// - Không chứa số hoặc ký tự đặc biệt
        /// - Trim khoảng trắng đầu/cuối
        /// - Không được chỉ chứa khoảng trắng
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateFullName(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return (false, "Họ và tên không được để trống");
            
            // Trim leading and trailing spaces
            fullName = fullName.Trim();
            
            // Check if it's only spaces
            if (string.IsNullOrWhiteSpace(fullName))
                return (false, "Họ và tên không được chỉ chứa khoảng trắng");
            
            // Check length (2-100 characters)
            if (fullName.Length < 2 || fullName.Length > 100)
                return (false, "Họ và tên phải từ 2 đến 100 ký tự");
            
            // Check if contains only letters and spaces (Vietnamese + English letters)
            if (!Regex.IsMatch(fullName, @"^[a-zA-Z\s\u0100-\u0177\u01A0-\u01A1\u1EA0-\u1EFF]+$"))
                return (false, "Họ và tên chỉ được chứa chữ cái và khoảng trắng (không có số hoặc ký tự đặc biệt)");
            
            return (true, string.Empty);
        }

        /// <summary>
        /// Xác định tính hợp lệ của Email
        /// - Bắt buộc
        /// - Định dạng email hợp lệ
        /// - Max độ dài: 255 ký tự
        /// - Không có khoảng trắng
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return (false, "Email không được để trống");
            
            email = email.Trim();
            
            // Check for spaces
            if (email.Contains(" "))
                return (false, "Email không được chứa khoảng trắng");
            
            // Check max length
            if (email.Length > 255)
                return (false, "Email không được vượt quá 255 ký tự");
            
            // Check valid email format (RFC 5322 simplified)
            if (!Regex.IsMatch(email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
                return (false, "Định dạng email không hợp lệ (ví dụ: example@gmail.com)");
            
            return (true, string.Empty);
        }

        /// <summary>
        /// Xác định tính hợp lệ của Số điện thoại
        /// - Bắt buộc
        /// - Chỉ chứa chữ số (0-9)
        /// - Độ dài: 10 đến 11 chữ số
        /// - Phải bắt đầu với 0
        /// - Không có khoảng trắng hoặc ký tự đặc biệt
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidatePhoneNumber(string? phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return (false, "Số điện thoại không được để trống");
            
            phoneNumber = phoneNumber.Trim();
            
            // Check for spaces or special characters
            if (!Regex.IsMatch(phoneNumber, @"^\d+$"))
                return (false, "Số điện thoại chỉ được chứa chữ số (không có khoảng trắng hoặc ký tự đặc biệt)");
            
            // Check length (10-11 digits)
            if (phoneNumber.Length < 10 || phoneNumber.Length > 11)
                return (false, "Số điện thoại phải từ 10 đến 11 chữ số");
            
            // Check if starts with 0
            if (!phoneNumber.StartsWith("0"))
                return (false, "Số điện thoại phải bắt đầu với số 0");
            
            return (true, string.Empty);
        }

        /// <summary>
        /// Xác định tính hợp lệ của Tên đăng nhập (Username)
        /// - Bắt buộc
        /// - Độ dài: 5 đến 50 ký tự
        /// - Chỉ chứa chữ cái và số (a-z, A-Z, 0-9)
        /// - Không có khoảng trắng hoặc ký tự đặc biệt
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateUsername(string? username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return (false, "Tên đăng nhập không được để trống");
            
            username = username.Trim();
            
            // Check length (5-50 characters)
            if (username.Length < 5 || username.Length > 50)
                return (false, "Tên đăng nhập phải từ 5 đến 50 ký tự");
            
            // Check if contains only letters and numbers (no spaces, no special characters)
            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9]+$"))
                return (false, "Tên đăng nhập chỉ được chứa chữ cái và số (không có khoảng trắng hoặc ký tự đặc biệt)");
            
            return (true, string.Empty);
        }

        /// <summary>
        /// Xác định tính hợp lệ của Mật khẩu
        /// - Bắt buộc
        /// - Độ dài tối thiểu: 8 ký tự
        /// - Phải chứa ít nhất:
        ///   + 1 chữ hoa (A-Z)
        ///   + 1 chữ thường (a-z)
        ///   + 1 chữ số (0-9)
        ///   + 1 ký tự đặc biệt (!@#$%^&*)
        /// - Không có khoảng trắng
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidatePassword(string? password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return (false, "Mật khẩu không được để trống");
            
            // Check for spaces
            if (password.Contains(" "))
                return (false, "Mật khẩu không được chứa khoảng trắng");
            
            // Check minimum length
            if (password.Length < 8)
                return (false, "Mật khẩu phải có ít nhất 8 ký tự");
            
            // Check for uppercase letters
            if (!Regex.IsMatch(password, @"[A-Z]"))
                return (false, "Mật khẩu phải chứa ít nhất 1 chữ hoa (A-Z)");
            
            // Check for lowercase letters
            if (!Regex.IsMatch(password, @"[a-z]"))
                return (false, "Mật khẩu phải chứa ít nhất 1 chữ thường (a-z)");
            
            // Check for digits
            if (!Regex.IsMatch(password, @"[0-9]"))
                return (false, "Mật khẩu phải chứa ít nhất 1 chữ số (0-9)");
            
            // Check for special characters (!@#$%^&*)
            if (!Regex.IsMatch(password, @"[!@#$%^&*]"))
                return (false, "Mật khẩu phải chứa ít nhất 1 ký tự đặc biệt (!@#$%^&*)");
            
            return (true, string.Empty);
        }

        /// <summary>
        /// Xác định tính hợp lệ của xác nhận mật khẩu
        /// - Bắt buộc
        /// - Phải khớp chính xác với mật khẩu
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateConfirmPassword(string? password, string? confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(confirmPassword))
                return (false, "Xác nhận mật khẩu không được để trống");
            
            if (password != confirmPassword)
                return (false, "Xác nhận mật khẩu không khớp với mật khẩu");
            
            return (true, string.Empty);
        }

        /// <summary>
        /// Xác định mức độ mạnh của mật khẩu
        /// </summary>
        public static string GetPasswordStrength(string? password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return "Chưa nhập";
            
            int strength = 0;
            
            if (password.Length >= 8) strength++;
            if (Regex.IsMatch(password, @"[A-Z]")) strength++;
            if (Regex.IsMatch(password, @"[a-z]")) strength++;
            if (Regex.IsMatch(password, @"[0-9]")) strength++;
            if (Regex.IsMatch(password, @"[!@#$%^&*]")) strength++;
            
            if (strength < 3) return "Yếu";
            if (strength < 5) return "Trung bình";
            return "Mạnh";
        }

        /// <summary>
        /// Kiểm tra checkbox điều khoản sử dụng
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateTermsCheckbox(bool? acceptTerms)
        {
            if (acceptTerms != true)
                return (false, "Bạn phải đồng ý với Điều khoản dịch vụ và Chính sách bảo mật");
            
            return (true, string.Empty);
        }

        /// <summary>
        /// Sanitize input để không bị XSS
        /// </summary>
        public static string SanitizeInput(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
            
            // HTML encode dangerous characters
            return System.Text.Encodings.Web.HtmlEncoder.Default.Encode(input);
        }

        /// <summary>
        /// Trim input và remove extra spaces
        /// </summary>
        public static string NormalizeInput(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;
            
            // Trim and normalize multiple spaces to single space
            return Regex.Replace(input.Trim(), @"\s+", " ");
        }
    }
}

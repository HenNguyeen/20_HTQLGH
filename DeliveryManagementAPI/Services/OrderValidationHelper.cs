using System.Text.RegularExpressions;

namespace DeliveryManagementAPI.Services
{
    /// <summary>
    /// Helper class để xác định tính hợp lệ của các trường form tạo đơn hàng
    /// </summary>
    public static class OrderValidationHelper
    {
        /// <summary>
        /// Xác định tính hợp lệ của Mã Đơn Hàng
        /// - Bắt buộc
        /// - Độ dài: tối đa 50 ký tự
        /// - Chỉ chứa chữ, số, dấu gạch dưới (_)
        /// - Định dạng: DH + timestamp hoặc system-generated
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateOrderCode(string? orderCode)
        {
            if (string.IsNullOrWhiteSpace(orderCode))
                return (false, "Mã đơn hàng không được để trống");
            
            orderCode = orderCode.Trim();
            
            if (orderCode.Length > 50)
                return (false, "Mã đơn hàng không được vượt quá 50 ký tự");
            
            // Allow letters, numbers, underscore
            if (!Regex.IsMatch(orderCode, @"^[a-zA-Z0-9_]+$"))
                return (false, "Mã đơn hàng chỉ được chứa chữ cái, số và dấu gạch dưới (_)");
            
            return (true, string.Empty);
        }

        /// <summary>
        /// Xác định tính hợp lệ của Tên Khách Hàng
        /// - Bắt buộc
        /// - Độ dài: 2 đến 100 ký tự
        /// - Chỉ chứa chữ cái và khoảng trắng
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateCustomerName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return (false, "Tên khách hàng không được để trống");
            
            name = name.Trim();
            
            if (name.Length < 2 || name.Length > 100)
                return (false, "Tên khách hàng phải từ 2 đến 100 ký tự");
            
            // Allow letters (including Vietnamese) and spaces
            if (!Regex.IsMatch(name, @"^[a-zA-Z\s\u0100-\u0177\u01A0-\u01A1\u1EA0-\u1EFF]+$"))
                return (false, "Tên khách hàng chỉ được chứa chữ cái và khoảng trắng");
            
            return (true, string.Empty);
        }

        /// <summary>
        /// Xác định tính hợp lệ của Số Điện Thoại
        /// - Bắt buộc
        /// - Chỉ chứa chữ số (0-9)
        /// - Độ dài: 10 đến 11 chữ số
        /// - Phải bắt đầu với 0
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidatePhoneNumber(string? phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return (false, "Số điện thoại không được để trống");
            
            phoneNumber = phoneNumber.Trim();
            
            if (!Regex.IsMatch(phoneNumber, @"^\d+$"))
                return (false, "Số điện thoại chỉ được chứa chữ số");
            
            if (phoneNumber.Length < 10 || phoneNumber.Length > 11)
                return (false, "Số điện thoại phải từ 10 đến 11 chữ số");
            
            if (!phoneNumber.StartsWith("0"))
                return (false, "Số điện thoại phải bắt đầu với số 0");
            
            return (true, string.Empty);
        }

        /// <summary>
        /// Xác định tính hợp lệ của Địa Chỉ Giao Hàng
        /// - Bắt buộc
        /// - Độ dài: 5 đến 255 ký tự
        /// - Cho phép chữ, số, dấu phẩy, dấu chấm, khoảng trắng
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateDeliveryAddress(string? address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return (false, "Địa chỉ giao hàng không được để trống");
            
            address = address.Trim();
            
            if (address.Length < 5)
                return (false, "Địa chỉ giao hàng phải ít nhất 5 ký tự");
            
            if (address.Length > 255)
                return (false, "Địa chỉ giao hàng không được vượt quá 255 ký tự");
            
            return (true, string.Empty);
        }

        /// <summary>
        /// Xác định tính hợp lệ của Phường/Xã, Quận/Huyện, Thành Phố
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateLocation(string? ward, string? district, string? city)
        {
            if (string.IsNullOrWhiteSpace(ward))
                return (false, "Phường/Xã không được để trống");
            
            if (string.IsNullOrWhiteSpace(district))
                return (false, "Quận/Huyện không được để trống");
            
            if (string.IsNullOrWhiteSpace(city))
                return (false, "Thành Phố không được để trống");
            
            return (true, string.Empty);
        }

        /// <summary>
        /// Xác định tính hợp lệ của Mã Sản Phẩm
        /// - Bắt buộc
        /// - Chỉ chứa chữ, số, dấu gạch dưới
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateProductCode(string? productCode)
        {
            if (string.IsNullOrWhiteSpace(productCode))
                return (false, "Mã sản phẩm không được để trống");
            
            productCode = productCode.Trim();
            
            if (!Regex.IsMatch(productCode, @"^[a-zA-Z0-9_]+$"))
                return (false, "Mã sản phẩm chỉ được chứa chữ cái, số và dấu gạch dưới");
            
            return (true, string.Empty);
        }

        /// <summary>
        /// Xác định tính hợp lệ của Loại Hàng (Product Type)
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateProductType(string? productType)
        {
            if (string.IsNullOrWhiteSpace(productType))
                return (false, "Loại hàng phải được chọn");
            
            // Valid types: "Gói nhỏ", "Gói lớn", "Gói chuẩn", etc.
            var validTypes = new[] { "Gói nhỏ", "Gói lớn", "Gói chuẩn", "Khác" };
            if (!validTypes.Contains(productType.Trim()))
                return (false, "Loại hàng không hợp lệ");
            
            return (true, string.Empty);
        }

        /// <summary>
        /// Xác định tính hợp lệ của Trọng Lượng (kg)
        /// - Bắt buộc
        /// - Phải là số > 0
        /// - Max: 1000 kg
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateWeight(string? weightStr)
        {
            if (string.IsNullOrWhiteSpace(weightStr))
                return (false, "Trọng lượng không được để trống");
            
            if (!decimal.TryParse(weightStr, out decimal weight))
                return (false, "Trọng lượng phải là số hợp lệ");
            
            if (weight <= 0)
                return (false, "Trọng lượng phải lớn hơn 0");
            
            if (weight > 1000)
                return (false, "Trọng lượng không được vượt quá 1000 kg");
            
            return (true, string.Empty);
        }

        /// <summary>
        /// Xác định tính hợp lệ của Kích Thước (LxWxH cm)
        /// - Định dạng: số x số x số
        /// - Mỗi giá trị > 0
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateDimensions(string? length, string? width, string? height)
        {
            if (string.IsNullOrWhiteSpace(length) || string.IsNullOrWhiteSpace(width) || string.IsNullOrWhiteSpace(height))
                return (false, "Kích thước chiều dài, chiều rộng, chiều cao không được để trống");
            
            if (!decimal.TryParse(length, out decimal lengthVal) || 
                !decimal.TryParse(width, out decimal widthVal) || 
                !decimal.TryParse(height, out decimal heightVal))
                return (false, "Kích thước phải là số hợp lệ");
            
            if (lengthVal <= 0 || widthVal <= 0 || heightVal <= 0)
                return (false, "Kích thước chiều dài, rộng, cao phải lớn hơn 0");
            
            return (true, string.Empty);
        }

        /// <summary>
        /// Xác định tính hợp lệ của Khoảng Cách (km)
        /// - Bắt buộc
        /// - Phải là số >= 0
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateDistance(string? distanceStr)
        {
            if (string.IsNullOrWhiteSpace(distanceStr))
                return (false, "Khoảng cách không được để trống");
            
            if (!decimal.TryParse(distanceStr, out decimal distance))
                return (false, "Khoảng cách phải là số hợp lệ");
            
            if (distance < 0)
                return (false, "Khoảng cách không được âm");
            
            return (true, string.Empty);
        }

        /// <summary>
        /// Xác định tính hợp lệ của Phương Thức Thanh Toán
        /// - Bắt buộc
        /// - Options: COD, Online
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidatePaymentMethod(string? paymentMethod)
        {
            if (string.IsNullOrWhiteSpace(paymentMethod))
                return (false, "Phương thức thanh toán phải được chọn");
            
            var validMethods = new[] { "COD", "Online" };
            if (!validMethods.Contains(paymentMethod.Trim()))
                return (false, "Phương thức thanh toán không hợp lệ");
            
            return (true, string.Empty);
        }

        /// <summary>
        /// Xác định tính hợp lệ của Loại Giao Hàng
        /// - Bắt buộc
        /// - Options: Standard, Express
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateDeliveryType(string? deliveryType)
        {
            if (string.IsNullOrWhiteSpace(deliveryType))
                return (false, "Loại giao hàng phải được chọn");
            
            var validTypes = new[] { "Standard", "Express", "Chuẩn", "Nhanh" };
            if (!validTypes.Contains(deliveryType.Trim()))
                return (false, "Loại giao hàng không hợp lệ");
            
            return (true, string.Empty);
        }

        /// <summary>
        /// Xác định tính hợp lệ của Tiền COD (nếu được chọn)
        /// - Phải là số > 0
        /// - Max: 50,000,000 VND
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateCODAmount(string? codAmountStr, bool isCODSelected)
        {
            if (!isCODSelected)
                return (true, string.Empty);
            
            if (string.IsNullOrWhiteSpace(codAmountStr))
                return (false, "Tiền COD không được để trống khi chọn hình thức COD");
            
            if (!decimal.TryParse(codAmountStr, out decimal codAmount))
                return (false, "Tiền COD phải là số hợp lệ");
            
            if (codAmount <= 0)
                return (false, "Tiền COD phải lớn hơn 0");
            
            if (codAmount > 50000000)
                return (false, "Tiền COD không được vượt quá 50,000,000 VND");
            
            return (true, string.Empty);
        }

        /// <summary>
        /// Xác định tính hợp lệ của Ghi Chú
        /// - Optional
        /// - Max: 500 ký tự
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateNote(string? note)
        {
            if (string.IsNullOrWhiteSpace(note))
                return (true, string.Empty);
            
            if (note.Length > 500)
                return (false, "Ghi chú không được vượt quá 500 ký tự");
            
            return (true, string.Empty);
        }

        /// <summary>
        /// Tính phí giao hàng dự kiến
        /// Công thức: Base Fee + Distance Fee + Weight Fee + Extra Fees
        /// </summary>
        public static decimal CalculateShippingFee(
            decimal distance,
            decimal weight,
            string deliveryType,
            bool isFragile = false,
            bool isHighValue = false,
            bool isBulky = false)
        {
            decimal fee = 0;
            
            // Base fee theo loại giao hàng
            if (deliveryType == "Express" || deliveryType == "Nhanh")
            {
                fee = 25000; // 25,000 VND cho giao hàng nhanh
            }
            else
            {
                fee = 15000; // 15,000 VND cho giao hàng chuẩn
            }
            
            // Distance fee: 5,000 VND per km với tối thiểu 5 km
            decimal distanceCharge = Math.Max(distance, 5) * 5000;
            fee += distanceCharge;
            
            // Weight fee: 2,000 VND per kg với tối thiểu 1 kg
            decimal weightCharge = Math.Max(weight, 1) * 2000;
            fee += weightCharge;
            
            // Extra fees cho các tính chất đặc biệt
            if (isFragile)
                fee += 10000; // 10,000 VND cho hàng dễ vỡ
            
            if (isHighValue)
                fee += 15000; // 15,000 VND cho hàng trị giá cao
            
            if (isBulky)
                fee += 20000; // 20,000 VND cho hàng cồng kềnh
            
            return fee;
        }

        /// <summary>
        /// Sanitize input để tránh XSS
        /// </summary>
        public static string SanitizeInput(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
            
            return System.Text.Encodings.Web.HtmlEncoder.Default.Encode(input);
        }

        /// <summary>
        /// Normalize input - trim và remove extra spaces
        /// </summary>
        public static string NormalizeInput(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;
            
            return Regex.Replace(input.Trim(), @"\s+", " ");
        }
    }
}

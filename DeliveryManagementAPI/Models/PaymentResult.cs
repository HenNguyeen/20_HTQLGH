namespace DeliveryManagementAPI.Models
{
    /// <summary>
    /// Kết quả xử lý thanh toán - Common interface cho tất cả payment gateways
    /// Dùng cho Adapter Pattern (Pattern #15)
    /// </summary>
    public class PaymentResult
    {
        /// <summary>
        /// Thanh toán có thành công không
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Mã giao dịch từ cổng thanh toán
        /// </summary>
        public string? TransactionId { get; set; }

        /// <summary>
        /// URL để redirect user đến trang thanh toán (nếu cần)
        /// </summary>
        public string? PaymentUrl { get; set; }

        /// <summary>
        /// Thông báo lỗi (nếu có)
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Mã lỗi từ payment gateway
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Dữ liệu bổ sung từ payment gateway
        /// </summary>
        public Dictionary<string, string>? AdditionalData { get; set; }

        /// <summary>
        /// Thời gian giao dịch
        /// </summary>
        public DateTime TransactionTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Kết quả hoàn tiền
    /// </summary>
    public class RefundResult
    {
        public bool Success { get; set; }
        public string? RefundId { get; set; }
        public decimal RefundedAmount { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime RefundTime { get; set; } = DateTime.Now;
    }
}

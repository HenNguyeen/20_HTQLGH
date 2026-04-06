namespace DeliveryManagementAPI.Models.VNPay
{
    /// <summary>
    /// Request model cho VNPay API (Adaptee trong Adapter Pattern #15)
    /// Đây là interface của bên thứ 3 VNPay, khác với interface chung của hệ thống
    /// </summary>
    public class VNPayRequest
    {
        public string OrderId { get; set; } = string.Empty;
        public long Amount { get; set; } // VNPay dùng đơn vị xu (VND * 100)
        public string OrderDescription { get; set; } = string.Empty;
        public string OrderType { get; set; } = "billpayment";
        public string ReturnUrl { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public DateTime CreateDate { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Response model từ VNPay API
    /// </summary>
    public class VNPayResponse
    {
        public string ResponseCode { get; set; } = string.Empty; // "00" = Success
        public string TransactionId { get; set; } = string.Empty;
        public string PaymentUrl { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, string> RawData { get; set; } = new();
    }

    /// <summary>
    /// Kết quả xác minh payment từ VNPay callback
    /// </summary>
    public class VNPayVerifyResult
    {
        public bool IsValid { get; set; }
        public string ResponseCode { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public long Amount { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string BankCode { get; set; } = string.Empty;
        public string CardType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request hoàn tiền VNPay
    /// </summary>
    public class VNPayRefundRequest
    {
        public string TransactionId { get; set; } = string.Empty;
        public long RefundAmount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime RefundDate { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Response hoàn tiền từ VNPay
    /// </summary>
    public class VNPayRefundResponse
    {
        public string ResponseCode { get; set; } = string.Empty;
        public string RefundId { get; set; } = string.Empty;
        public long RefundAmount { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}

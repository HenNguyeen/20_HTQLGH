namespace DeliveryManagementAPI.Models.Momo
{
    /// <summary>
    /// Request model cho Momo API (Adaptee thứ 2 trong Adapter Pattern #15)
    /// Interface của Momo khác với cả VNPay và interface chung của hệ thống
    /// </summary>
    public class MomoPaymentRequest
    {
        public string PartnerCode { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
        public string OrderInfo { get; set; } = string.Empty;
        public string RedirectUrl { get; set; } = string.Empty;
        public string IpnUrl { get; set; } = string.Empty; // Callback URL
        public long Amount { get; set; } // Momo dùng đơn vị VND (không nhân 100 như VNPay)
        public string OrderId { get; set; } = string.Empty;
        public string RequestType { get; set; } = "captureWallet";
        public string ExtraData { get; set; } = string.Empty;
        public string Lang { get; set; } = "vi";
        public string Signature { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response từ Momo API (format khác với VNPay)
    /// </summary>
    public class MomoPaymentResponse
    {
        public string PartnerCode { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public long Amount { get; set; }
        public long ResultCode { get; set; } // 0 = Success (khác với VNPay dùng "00")
        public string Message { get; set; } = string.Empty;
        public string PayUrl { get; set; } = string.Empty; // Tên field khác với VNPay
        public string QrCodeUrl { get; set; } = string.Empty;
        public string Deeplink { get; set; } = string.Empty;
    }

    /// <summary>
    /// Momo IPN notification (callback)
    /// </summary>
    public class MomoIpnRequest
    {
        public string PartnerCode { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
        public long Amount { get; set; }
        public long ResultCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public long TransId { get; set; } // Transaction ID từ Momo
        public string TransactionType { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
    }

    /// <summary>
    /// Momo refund request
    /// </summary>
    public class MomoRefundRequest
    {
        public string PartnerCode { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
        public long TransId { get; set; } // Transaction ID cần hoàn tiền
        public long Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
    }

    /// <summary>
    /// Momo refund response
    /// </summary>
    public class MomoRefundResponse
    {
        public string PartnerCode { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
        public long TransId { get; set; }
        public long ResultCode { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}

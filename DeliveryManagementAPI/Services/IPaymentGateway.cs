using DeliveryManagementAPI.Models;

namespace DeliveryManagementAPI.Services
{
    /// <summary>
    /// Target Interface cho Adapter Pattern (Pattern #15)
    /// Định nghĩa interface chung mà hệ thống mong đợi từ các payment gateways
    /// Các bên thứ 3 (VNPay, Momo, etc.) sẽ được adapt vào interface này
    /// </summary>
    public interface IPaymentGateway
    {
        /// <summary>
        /// Tên của payment gateway
        /// </summary>
        string GatewayName { get; }

        /// <summary>
        /// Xử lý thanh toán
        /// </summary>
        /// <param name="amount">Số tiền cần thanh toán (VND)</param>
        /// <param name="orderCode">Mã đơn hàng</param>
        /// <param name="description">Mô tả giao dịch</param>
        /// <param name="returnUrl">URL trả về sau khi thanh toán</param>
        /// <returns>Kết quả thanh toán với URL redirect nếu cần</returns>
        Task<PaymentResult> ProcessPaymentAsync(
            decimal amount, 
            string orderCode, 
            string description,
            string returnUrl);

        /// <summary>
        /// Xác minh kết quả thanh toán từ callback/return URL
        /// </summary>
        /// <param name="queryParams">Query parameters từ callback</param>
        /// <returns>Kết quả xác minh</returns>
        Task<PaymentResult> VerifyPaymentAsync(Dictionary<string, string> queryParams);

        /// <summary>
        /// Hoàn tiền cho giao dịch
        /// </summary>
        /// <param name="transactionId">Mã giao dịch cần hoàn</param>
        /// <param name="amount">Số tiền cần hoàn (VND)</param>
        /// <param name="reason">Lý do hoàn tiền</param>
        /// <returns>Kết quả hoàn tiền</returns>
        Task<RefundResult> RefundAsync(string transactionId, decimal amount, string reason);

        /// <summary>
        /// Kiểm tra trạng thái giao dịch
        /// </summary>
        /// <param name="transactionId">Mã giao dịch</param>
        /// <returns>Trạng thái giao dịch</returns>
        Task<PaymentResult> CheckTransactionStatusAsync(string transactionId);
    }
}

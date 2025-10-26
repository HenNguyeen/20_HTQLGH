namespace DeliveryManagementAPI.Models
{
    /// <summary>
    /// Phương thức thanh toán
    /// </summary>
    public enum PaymentMethod
    {
        GuiThuong = 0,          // Gửi thường (nhận tiền khi giao hàng)
        GuiNhanh = 1,           // Gửi nhanh (chuyển khoản/thanh toán trực tuyến)
        ChuyenKhoan = 2,        // Chuyển khoản
        ThanhToanTrucTuyen = 3  // Thanh toán trực tuyến
    }
}

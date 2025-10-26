namespace DeliveryManagementAPI.Models
{
    /// <summary>
    /// Đơn hàng giao hàng
    /// </summary>
    public class Order
    {
        [System.ComponentModel.DataAnnotations.Key]
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty; // Mã đơn hàng từ website
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // Người tạo đơn (UserAccount.UserId) - dùng để lọc "đơn của tôi"
        public int? CreatedByUserId { get; set; }
        
        // Thông tin khách hàng (Foreign Key)
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = new Customer();
        
        // Thông tin hàng hóa
        public string ProductCode { get; set; } = string.Empty; // Mã hàng
        public PackageType PackageType { get; set; }
        public double Weight { get; set; } // Cân nặng (kg)
        public string Size { get; set; } = string.Empty; // Kích thước
        public double Distance { get; set; } // Khoảng cách (km)
        
        // Các thông số đặc biệt
        public bool IsFragile { get; set; } // Hàng dễ vỡ
        public bool IsValuable { get; set; } // Hàng trị giá
        public bool IsVehicle { get; set; } // Hàng hóa là xe
        public bool CollectMoney { get; set; } // Thu tiền
        public decimal CollectionAmount { get; set; } // Số tiền thu
        
        // Thông tin thanh toán
        public PaymentMethod PaymentMethod { get; set; }
        public decimal ShippingFee { get; set; } // Phí giao hàng
        public bool IsPaid { get; set; } // Đã thanh toán

    // Thông tin thanh toán chi tiết
    public decimal? PaidAmount { get; set; } // Số tiền đã thanh toán
    public DateTime? PaymentTime { get; set; } // Thời điểm thanh toán
        
        // Thông tin giao hàng
        public DeliveryType DeliveryType { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.ChuaNhan;
        public string? AssignedStaffId { get; set; } // Nhân viên được gán
        public DeliveryStaff? AssignedStaff { get; set; }
        
        // Thông tin giao hàng thực tế
        public DateTime? ReceivedDate { get; set; } // Ngày nhận hàng
        public DateTime? DeliveryStartDate { get; set; } // Ngày bắt đầu giao
        public DateTime? DeliveredDate { get; set; } // Ngày giao thành công
        
        // Ghi chú
        public string Notes { get; set; } = string.Empty;
        
        // Vị trí check in
        public List<LocationCheckpoint> Checkpoints { get; set; } = new List<LocationCheckpoint>();

        // Xác nhận đã nhận hàng bởi khách
        public bool ConfirmedReceived { get; set; } // Đã xác nhận nhận hàng
        public DateTime? ConfirmedAt { get; set; } // Thời điểm xác nhận
    }
}

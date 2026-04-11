namespace DeliveryManagementAPI.Models
{
    /// <summary>
    /// Builder Pattern để tạo đối tượng Order phức tạp
    /// Giúp code dễ đọc, dễ maintain và đảm bảo các thuộc tính được thiết lập đúng
    /// </summary>
    public class OrderBuilder
    {
        private readonly Order _order;

        public OrderBuilder()
        {
            _order = new Order
            {
                CreatedDate = DateTime.Now,
                Status = OrderStatus.ChuaNhan
            };
        }

        /// <summary>
        /// Thiết lập mã đơn hàng (OrderCode)
        /// </summary>
        public OrderBuilder WithOrderCode(string orderCode)
        {
            if (string.IsNullOrWhiteSpace(orderCode))
            {
                // Tự động generate OrderCode nếu không có
                _order.OrderCode = GenerateOrderCode();
            }
            else
            {
                _order.OrderCode = orderCode;
            }
            return this;
        }

        /// <summary>
        /// Thiết lập người tạo đơn hàng
        /// </summary>
        public OrderBuilder CreatedBy(int? userId)
        {
            _order.CreatedByUserId = userId;
            return this;
        }

        /// <summary>
        /// Thiết lập khách hàng
        /// </summary>
        public OrderBuilder ForCustomer(Customer customer)
        {
            _order.CustomerId = customer.CustomerId;
            _order.Customer = customer;
            return this;
        }

        /// <summary>
        /// Thiết lập khách hàng bằng ID
        /// </summary>
        public OrderBuilder ForCustomer(int customerId)
        {
            _order.CustomerId = customerId;
            return this;
        }

        /// <summary>
        /// Thiết lập thông tin hàng hóa
        /// </summary>
        public OrderBuilder WithPackageDetails(
            string productCode,
            PackageType packageType,
            double weight,
            string size,
            double distance)
        {
            _order.ProductCode = productCode;
            _order.PackageType = packageType;
            _order.Weight = weight;
            _order.Size = size;
            _order.Distance = distance;
            return this;
        }

        /// <summary>
        /// Thiết lập ID sản phẩm (ProductId)
        /// </summary>
        public OrderBuilder WithProductId(int productId)
        {
            _order.ProductId = productId;
            return this;
        }

        /// <summary>
        /// Thiết lập các đặc điểm đặc biệt của hàng hóa
        /// </summary>
        public OrderBuilder WithSpecialCharacteristics(
            bool isFragile = false,
            bool isValuable = false,
            bool isVehicle = false)
        {
            _order.IsFragile = isFragile;
            _order.IsValuable = isValuable;
            _order.IsVehicle = isVehicle;
            return this;
        }

        /// <summary>
        /// Đánh dấu hàng dễ vỡ
        /// </summary>
        public OrderBuilder IsFragile(bool value = true)
        {
            _order.IsFragile = value;
            return this;
        }

        /// <summary>
        /// Đánh dấu hàng có giá trị cao
        /// </summary>
        public OrderBuilder IsValuable(bool value = true)
        {
            _order.IsValuable = value;
            return this;
        }

        /// <summary>
        /// Đánh dấu hàng là phương tiện
        /// </summary>
        public OrderBuilder IsVehicle(bool value = true)
        {
            _order.IsVehicle = value;
            return this;
        }

        /// <summary>
        /// Thiết lập thu tiền hộ
        /// </summary>
        public OrderBuilder WithCollectionAmount(decimal amount)
        {
            _order.CollectMoney = amount > 0;
            _order.CollectionAmount = amount;
            return this;
        }

        /// <summary>
        /// Thiết lập phương thức thanh toán và phí giao hàng
        /// </summary>
        public OrderBuilder WithPayment(
            PaymentMethod paymentMethod,
            decimal shippingFee)
        {
            _order.PaymentMethod = paymentMethod;
            _order.ShippingFee = shippingFee;
            
            // Tự động đánh dấu đã thanh toán cho Momo
            _order.IsPaid = paymentMethod == PaymentMethod.Momo;
            
            return this;
        }

        /// <summary>
        /// Thiết lập loại giao hàng
        /// </summary>
        public OrderBuilder WithDeliveryType(DeliveryType deliveryType)
        {
            _order.DeliveryType = deliveryType;
            return this;
        }

        /// <summary>
        /// Thiết lập trạng thái đơn hàng
        /// </summary>
        public OrderBuilder WithStatus(OrderStatus status)
        {
            _order.Status = status;
            return this;
        }

        /// <summary>
        /// Gán nhân viên giao hàng
        /// </summary>
        public OrderBuilder AssignToStaff(int staffId)
        {
            _order.AssignedStaffId = staffId.ToString();
            return this;
        }

        /// <summary>
        /// Gán nhân viên giao hàng
        /// </summary>
        public OrderBuilder AssignToStaff(DeliveryStaff staff)
        {
            _order.AssignedStaffId = staff.StaffId.ToString();
            _order.AssignedStaff = staff;
            return this;
        }

        /// <summary>
        /// Thêm ghi chú
        /// </summary>
        public OrderBuilder WithNotes(string notes)
        {
            _order.Notes = notes ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Thiết lập thời gian tạo đơn
        /// </summary>
        public OrderBuilder WithCreatedDate(DateTime createdDate)
        {
            _order.CreatedDate = createdDate;
            return this;
        }

        /// <summary>
        /// Tạo Order từ CreateOrderDto
        /// </summary>
        public OrderBuilder FromDto(CreateOrderDto dto)
        {
            WithOrderCode(dto.OrderCode);
            
            WithProductId(dto.ProductId);
            
            _order.PackageType = dto.PackageType;
            _order.Weight = dto.Weight;
            _order.Size = dto.Size;
            _order.Distance = dto.Distance;

            WithSpecialCharacteristics(
                dto.IsFragile,
                dto.IsValuable,
                dto.IsVehicle
            );

            if (dto.CollectMoney)
            {
                WithCollectionAmount(dto.CollectionAmount);
            }

            WithDeliveryType(dto.DeliveryType);
            WithNotes(dto.Notes);

            return this;
        }

        /// <summary>
        /// Validate và build Order object
        /// </summary>
        public Order Build()
        {
            // Validation
            if (string.IsNullOrWhiteSpace(_order.OrderCode))
            {
                _order.OrderCode = GenerateOrderCode();
            }

            if (_order.CustomerId <= 0 && _order.Customer == null)
            {
                throw new InvalidOperationException("Order phải có thông tin khách hàng. Sử dụng ForCustomer() để thiết lập.");
            }

            if (_order.Weight <= 0)
            {
                throw new InvalidOperationException("Trọng lượng phải lớn hơn 0");
            }

            if (_order.Distance <= 0)
            {
                throw new InvalidOperationException("Khoảng cách phải lớn hơn 0");
            }

            if (_order.ShippingFee <= 0)
            {
                throw new InvalidOperationException("Phí giao hàng phải lớn hơn 0. Sử dụng WithPayment() để thiết lập.");
            }

            return _order;
        }

        /// <summary>
        /// Tạo mã đơn hàng tự động
        /// Format: DHyyyyMMddHHmmssfffRRR (DH + timestamp + random 3 digits)
        /// </summary>
        private static string GenerateOrderCode()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            var random = new Random().Next(100, 999);
            return $"DH{timestamp}{random}";
        }
    }
}

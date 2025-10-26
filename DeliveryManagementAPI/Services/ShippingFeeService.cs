using DeliveryManagementAPI.Models;

namespace DeliveryManagementAPI.Services
{
    /// <summary>
    /// Service tính phí giao hàng
    /// </summary>
    public class ShippingFeeService
    {
        public decimal CalculateShippingFee(CreateOrderDto orderDto)
        {
            decimal baseFee = 20000; // Phí cơ bản 20,000 VNĐ
            decimal totalFee = baseFee;

            // Phí theo khoảng cách
            if (orderDto.Distance <= 5)
            {
                totalFee += 10000;
            }
            else if (orderDto.Distance <= 10)
            {
                totalFee += 20000;
            }
            else if (orderDto.Distance <= 20)
            {
                totalFee += 40000;
            }
            else
            {
                totalFee += 60000 + ((decimal)orderDto.Distance - 20) * 3000;
            }

            // Phí theo trọng lượng
            if (orderDto.Weight > 5)
            {
                totalFee += ((decimal)orderDto.Weight - 5) * 2000;
            }

            // Phí hàng đặc biệt
            if (orderDto.IsFragile)
            {
                totalFee += 15000;
            }

            if (orderDto.IsValuable)
            {
                totalFee += 30000;
            }

            if (orderDto.IsVehicle)
            {
                totalFee += 100000;
            }

            // Phí theo loại giao hàng
            if (orderDto.DeliveryType == DeliveryType.GiaoHangNhanh)
            {
                totalFee *= 1.5m; // Tăng 50% cho giao hàng nhanh
            }

            // Phí theo loại hàng
            switch (orderDto.PackageType)
            {
                case PackageType.TiVi:
                case PackageType.MayTinh:
                    totalFee += 30000;
                    break;
                case PackageType.Laptop:
                case PackageType.CPU:
                    totalFee += 20000;
                    break;
                case PackageType.Xe:
                    totalFee += 150000;
                    break;
            }

            return totalFee;
        }
    }
}

using DeliveryManagementAPI.Models;
using DeliveryManagementAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace DeliveryManagementAPI.Data
{
    /// <summary>
    /// Class để seed dữ liệu mẫu vào database
    /// </summary>
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var context = new DeliveryDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<DeliveryDbContext>>());

            // Kiểm tra nếu đã có dữ liệu thì không seed nữa
            if (context.UserAccounts.Any())
            {
                return; // Database đã được seed
            }

            // Seed User Accounts
            var users = new[]
            {
                new UserAccount
                {
                    Username = "admin",
                    PasswordHash = UserAccountService.HashPassword("admin123"),
                    FullName = "Quản trị viên",
                    Email = "admin@delivery.com",
                    PhoneNumber = "0901234567",
                    Role = "admin"
                },
                new UserAccount
                {
                    Username = "customer1",
                    PasswordHash = UserAccountService.HashPassword("customer123"),
                    FullName = "Nguyễn Văn A",
                    Email = "customer1@gmail.com",
                    PhoneNumber = "0912345678",
                    Role = "customer"
                },
                new UserAccount
                {
                    Username = "shipper1",
                    PasswordHash = UserAccountService.HashPassword("shipper123"),
                    FullName = "Trần Văn B",
                    Email = "shipper1@gmail.com",
                    PhoneNumber = "0923456789",
                    Role = "shipper"
                }
            };
            context.UserAccounts.AddRange(users);
            await context.SaveChangesAsync();

            // Seed Delivery Staff
            var staff = new[]
            {
                new DeliveryStaff
                {
                    FullName = "Trần Văn B",
                    PhoneNumber = "0923456789",
                    VehicleType = "Xe máy",
                    VehiclePlate = "29A-12345",
                    IsAvailable = true
                },
                new DeliveryStaff
                {
                    FullName = "Lê Thị C",
                    PhoneNumber = "0934567890",
                    VehicleType = "Xe máy",
                    VehiclePlate = "29B-67890",
                    IsAvailable = true
                },
                new DeliveryStaff
                {
                    FullName = "Phạm Văn D",
                    PhoneNumber = "0945678901",
                    VehicleType = "Xe tải nhỏ",
                    VehiclePlate = "29C-11111",
                    IsAvailable = false
                }
            };
            context.DeliveryStaffs.AddRange(staff);
            await context.SaveChangesAsync();

            // Seed Customers
            var customers = new[]
            {
                new Customer
                {
                    FullName = "Hoàng Văn E",
                    PhoneNumber = "0956789012",
                    Address = "123 Nguyễn Huệ",
                    Ward = "Bến Nghé",
                    District = "Quận 1",
                    City = "TP.HCM"
                },
                new Customer
                {
                    FullName = "Võ Thị F",
                    PhoneNumber = "0967890123",
                    Address = "456 Lê Lợi",
                    Ward = "Bến Thành",
                    District = "Quận 1",
                    City = "TP.HCM"
                }
            };
            context.Customers.AddRange(customers);
            await context.SaveChangesAsync();

            // Seed Orders
            var orders = new[]
            {
                new Order
                {
                    OrderCode = "DH001",
                    CreatedDate = DateTime.Now.AddDays(-3),
                    CustomerId = customers[0].CustomerId,
                    ProductCode = "SP001",
                    PackageType = PackageType.Thung,
                    Weight = 2.5,
                    Size = "30x20x10",
                    Distance = 15.5,
                    IsFragile = false,
                    IsValuable = false,
                    IsVehicle = false,
                    CollectMoney = true,
                    CollectionAmount = 500000,
                    PaymentMethod = PaymentMethod.GuiThuong,
                    ShippingFee = 45000,
                    IsPaid = false,
                    DeliveryType = DeliveryType.GiaoHangThuong,
                    Status = OrderStatus.DaNhanDangGiao,
                    AssignedStaffId = staff[0].StaffId.ToString(),
                    ReceivedDate = DateTime.Now.AddDays(-2),
                    DeliveryStartDate = DateTime.Now.AddDays(-1),
                    Notes = "Giao trong giờ hành chính"
                },
                new Order
                {
                    OrderCode = "DH002",
                    CreatedDate = DateTime.Now.AddDays(-1),
                    CustomerId = customers[1].CustomerId,
                    ProductCode = "SP002",
                    PackageType = PackageType.GoiNho,
                    Weight = 0.5,
                    Size = "25x15x5",
                    Distance = 8.2,
                    IsFragile = false,
                    IsValuable = true,
                    IsVehicle = false,
                    CollectMoney = false,
                    CollectionAmount = 0,
                    PaymentMethod = PaymentMethod.GuiNhanh,
                    ShippingFee = 30000,
                    IsPaid = true,
                    DeliveryType = DeliveryType.GiaoHangNhanh,
                    Status = OrderStatus.ChuaNhan,
                    Notes = "Hàng cần bảo mật"
                },
                new Order
                {
                    OrderCode = "DH003",
                    CreatedDate = DateTime.Now.AddDays(-5),
                    CustomerId = customers[0].CustomerId,
                    ProductCode = "SP003",
                    PackageType = PackageType.Thung,
                    Weight = 5.0,
                    Size = "40x30x20",
                    Distance = 25.0,
                    IsFragile = true,
                    IsValuable = true,
                    IsVehicle = false,
                    CollectMoney = false,
                    CollectionAmount = 0,
                    PaymentMethod = PaymentMethod.ChuyenKhoan,
                    ShippingFee = 85000,
                    IsPaid = true,
                    DeliveryType = DeliveryType.GiaoHangThuong,
                    Status = OrderStatus.DaGiao,
                    AssignedStaffId = staff[1].StaffId.ToString(),
                    ReceivedDate = DateTime.Now.AddDays(-4),
                    DeliveryStartDate = DateTime.Now.AddDays(-3),
                    DeliveredDate = DateTime.Now.AddDays(-2),
                    Notes = "Đã giao thành công"
                }
            };
            context.Orders.AddRange(orders);
            await context.SaveChangesAsync();

            // Seed Location Checkpoints
            var checkpoints = new[]
            {
                new LocationCheckpoint
                {
                    OrderId = orders[0].OrderId,
                    Latitude = 10.7626,
                    Longitude = 106.6823,
                    LocationName = "Bưu cục Quận 1",
                    CheckInTime = DateTime.Now.AddDays(-1),
                    Notes = "Đã nhận hàng tại bưu cục"
                },
                new LocationCheckpoint
                {
                    OrderId = orders[0].OrderId,
                    Latitude = 10.7726,
                    Longitude = 106.6923,
                    LocationName = "Trên đường giao hàng",
                    CheckInTime = DateTime.Now.AddHours(-2),
                    Notes = "Đang trên đường đến địa chỉ giao hàng"
                },
                new LocationCheckpoint
                {
                    OrderId = orders[2].OrderId,
                    Latitude = 10.7726,
                    Longitude = 106.7023,
                    LocationName = "Địa chỉ khách hàng",
                    CheckInTime = DateTime.Now.AddDays(-2),
                    Notes = "Đã giao hàng thành công"
                }
            };
            context.LocationCheckpoints.AddRange(checkpoints);
            await context.SaveChangesAsync();

            Console.WriteLine("✅ Seed data completed successfully!");
        }
    }
}

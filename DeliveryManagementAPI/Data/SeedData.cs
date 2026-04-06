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

            // ❌ DISABLED: Seed data không được tải - database sẽ trống
            /*
            // FORCE: Clear ALL users and reseed fresh for development
            var allUsers = context.UserAccounts.ToList();
            if (allUsers.Any())
            {
                context.UserAccounts.RemoveRange(allUsers);
                await context.SaveChangesAsync();
            }

            // Seed User Accounts fresh
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
                    PasswordHash = UserAccountService.HashPassword("123456"),
                    FullName = "Trần Văn B",
                    Email = "shipper1@gmail.com",
                    PhoneNumber = "0923456789",
                    Role = "shipper"
                },
                new UserAccount
                {
                    Username = "shipper2",
                    PasswordHash = UserAccountService.HashPassword("123456"),
                    FullName = "Lê Thị C",
                    Email = "shipper2@gmail.com",
                    PhoneNumber = "0934567890",
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
                    IdCardNumber = "001201012345",
                    Hometown = "Hà Nam, Việt Nam",
                    DateOfBirth = new DateTime(1990, 5, 15),
                    WorkingArea = "Quận 1, Quận 2",
                    VehicleType = "Xe máy",
                    VehiclePlate = "29A-12345",
                    IsAvailable = true
                },
                new DeliveryStaff
                {
                    FullName = "Lê Thị C",
                    PhoneNumber = "0934567890",
                    IdCardNumber = "001201012346",
                    Hometown = "Thanh Hóa, Việt Nam",
                    DateOfBirth = new DateTime(1992, 8, 20),
                    WorkingArea = "Quận 3, Quận 4, Quận 5",
                    VehicleType = "Xe máy",
                    VehiclePlate = "29B-67890",
                    IsAvailable = true
                },
                new DeliveryStaff
                {
                    FullName = "Phạm Văn D",
                    PhoneNumber = "0945678901",
                    IdCardNumber = "001201012347",
                    Hometown = "Nghệ An, Việt Nam",
                    DateOfBirth = new DateTime(1988, 3, 10),
                    WorkingArea = "Quận 6, Quận 7, Quận 8",
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

            // Seed Orders - Sử dụng Builder Pattern để tạo test data
            var orders = new[]
            {
                new OrderBuilder()
                    .WithOrderCode("DH001")
                    .WithCreatedDate(DateTime.Now.AddDays(-3))
                    .ForCustomer(customers[0].CustomerId)
                    .WithPackageDetails("SP001", PackageType.Thung, 2.5, "30x20x10", 15.5)
                    .WithCollectionAmount(500000)
                    .WithPayment(PaymentMethod.COD, 45000)
                    .WithDeliveryType(DeliveryType.GiaoHangThuong)
                    .WithStatus(OrderStatus.DaNhanDangGiao)
                    .AssignToStaff(staff[0].StaffId)
                    .WithNotes("Giao trong giờ hành chính")
                    .Build(),

                new OrderBuilder()
                    .WithOrderCode("DH002")
                    .WithCreatedDate(DateTime.Now.AddDays(-1))
                    .ForCustomer(customers[1].CustomerId)
                    .WithPackageDetails("SP002", PackageType.GoiNho, 0.5, "25x15x5", 8.2)
                    .IsValuable()
                    .WithPayment(PaymentMethod.Momo, 30000)
                    .WithDeliveryType(DeliveryType.GiaoHangNhanh)
                    .WithStatus(OrderStatus.ChuaNhan)
                    .WithNotes("Hàng cần bảo mật")
                    .Build(),

                new OrderBuilder()
                    .WithOrderCode("DH003")
                    .WithCreatedDate(DateTime.Now.AddDays(-5))
                    .ForCustomer(customers[0].CustomerId)
                    .WithPackageDetails("SP003", PackageType.Thung, 5.0, "40x30x20", 25.0)
                    .IsFragile()
                    .IsValuable()
                    .WithPayment(PaymentMethod.COD, 85000)
                    .WithDeliveryType(DeliveryType.GiaoHangThuong)
                    .WithStatus(OrderStatus.DaGiao)
                    .AssignToStaff(staff[1].StaffId)
                    .WithNotes("Đã giao thành công")
                    .Build()
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
            */

            Console.WriteLine("⚠️ Seed data bị DISABLED - database trống");
        }
    }
}

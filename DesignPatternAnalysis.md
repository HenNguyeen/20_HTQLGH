# Phân Tích Design Pattern (Mẫu Thiết Kế) Trong Dự Án

Tài liệu này xác định và giải thích các mẫu thiết kế phần mềm (Design Pattern) đã được triển khai trong dự án **Hệ Thống Quản Lý Giao Hàng** (Delivery Management System) ASP.NET Core, có thể là có chủ đích hoặc không chủ đích.

---

## Mục Lục

1. [Mẫu Dependency Injection (Tiêm Phụ Thuộc)](#1-mẫu-dependency-injection-tiêm-phụ-thuộc)
2. [Mẫu Repository (Kho Dữ Liệu)](#2-mẫu-repository-kho-dữ-liệu)
3. [Mẫu Service Layer (Tầng Dịch Vụ)](#3-mẫu-service-layer-tầng-dịch-vụ)
4. [Mẫu Singleton (Đơn Thể)](#4-mẫu-singleton-đơn-thể)
5. [Mẫu Strategy (Chiến Lược)](#5-mẫu-strategy-chiến-lược)
6. [Mẫu Observer (Quan Sát)](#6-mẫu-observer-quan-sát)
7. [Mẫu Facade (Mặt Tiền)](#7-mẫu-facade-mặt-tiền)
8. [Mẫu Template Method (Phương Thức Khuôn Mẫu)](#8-mẫu-template-method-phương-thức-khuôn-mẫu)
9. [Mẫu Factory Method (Phương Thức Nhà Máy)](#9-mẫu-factory-method-phương-thức-nhà-máy)
10. [Đề Xuất Các Mẫu Bổ Sung](#đề-xuất-các-mẫu-bổ-sung)

---

## 1. Mẫu Dependency Injection (Tiêm Phụ Thuộc)

### Phân Loại
**Mẫu Khởi Tạo (Creational Pattern)** (có khía cạnh của mẫu hành vi - Behavioral pattern)

### Vị trí xuất hiện trong dự án

Mẫu này được sử dụng xuyên suốt toàn bộ dự án, chủ yếu trong:
- [Program.cs](DeliveryManagementAPI/Program.cs) - Đăng ký services
- Tất cả Controllers ([OrdersController.cs](DeliveryManagementAPI/Controllers/OrdersController.cs), [AuthController.cs](DeliveryManagementAPI/Controllers/AuthController.cs), v.v.)
- Tất cả Services ([OrderService.cs](DeliveryManagementAPI/Services/OrderService.cs), [NotificationService.cs](DeliveryManagementAPI/Services/NotificationService.cs), v.v.)

### Ví Dụ Code

**Đăng ký Service trong [Program.cs](DeliveryManagementAPI/Program.cs#L22-L29):**
```csharp
// Đăng ký các services
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<DeliveryStaffService>();
builder.Services.AddScoped<CheckpointService>();
builder.Services.AddScoped<UserAccountService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddSingleton<ShippingFeeService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITwoFactorService, TwoFactorService>();
```

**Dependency Injection trong [OrdersController.cs](DeliveryManagementAPI/Controllers/OrdersController.cs#L19-L33):**
```csharp
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;
    private readonly DeliveryStaffService _staffService;
    private readonly ShippingFeeService _feeService;
    private readonly DeliveryDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        OrderService orderService,
        DeliveryStaffService staffService,
        ShippingFeeService feeService,
        DeliveryDbContext context,
        INotificationService notificationService,
        ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _staffService = staffService;
        _feeService = feeService;
        _context = context;
        _notificationService = notificationService;
        _logger = logger;
    }
}
```

### Giải Thích

Dependency Injection là một mẫu thiết kế cơ bản trong đó các phụ thuộc (các đối tượng mà một class cần) được cung cấp từ bên ngoài thay vì được tạo bên trong class. Điều này tuân theo nguyên tắc **Đảo Ngược Điều Khiển (Inversion of Control - IoC)**.

### Cách Hoạt Động Trong Hệ Thống

1. **Đăng ký Service**: Trong [Program.cs](DeliveryManagementAPI/Program.cs), các service được đăng ký với DI container sử dụng các vòng đời khác nhau:
   - `AddScoped`: Instance mới cho mỗi HTTP request (ví dụ: `OrderService`, `DeliveryStaffService`)
   - `AddSingleton`: Instance duy nhất cho toàn bộ vòng đời ứng dụng (ví dụ: `ShippingFeeService`)
   - `AddTransient`: Instance mới mỗi khi được yêu cầu

2. **Constructor Injection**: Controllers và services nhận các phụ thuộc thông qua constructor. DI container của ASP.NET Core tự động phân giải và tiêm các phụ thuộc này.

3. **Lợi Ích**:
   - Giảm sự liên kết chặt chẽ giữa các class
   - Dễ dàng test (có thể mock các phụ thuộc)
   - Dễ dàng thay đổi implementation (ví dụ: `INotificationService` có thể có nhiều implementation khác nhau)
   - Bảo trì code tốt hơn

---

## 2. Mẫu Repository (Kho Dữ Liệu)

### Phân Loại
**Mẫu Cấu Trúc (Structural Pattern)**

### Vị trí xuất hiện trong dự án

- [DeliveryDbContext.cs](DeliveryManagementAPI/DeliveryDbContext.cs) - Lớp trừu tượng truy xuất dữ liệu
- Các class Service như [OrderService.cs](DeliveryManagementAPI/Services/OrderService.cs), [UserAccountService.cs](DeliveryManagementAPI/Services/UserAccountService.cs), và [DeliveryStaffService.cs](DeliveryManagementAPI/Services/DeliveryStaffService.cs)

### Ví Dụ Code

**DbContext đóng vai trò Repository ([DeliveryDbContext.cs](DeliveryManagementAPI/DeliveryDbContext.cs#L6-L24)):**
```csharp
public class DeliveryDbContext : DbContext
{
    public DeliveryDbContext(DbContextOptions<DeliveryDbContext> options) : base(options) { }

    public DbSet<UserAccount> UserAccounts { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<DeliveryStaff> DeliveryStaffs { get; set; }
    public DbSet<LocationCheckpoint> LocationCheckpoints { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<NotificationSetting> NotificationSettings { get; set; }
}
```

**Service đóng vai trò Repository ([OrderService.cs](DeliveryManagementAPI/Services/OrderService.cs#L9-L34)):**
```csharp
public class OrderService
{
    private readonly DeliveryDbContext _context;

    public OrderService(DeliveryDbContext context)
    {
        _context = context;
    }

    // Lấy tất cả đơn hàng
    public async Task<List<Order>> GetAllOrdersAsync()
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.AssignedStaff)
            .OrderByDescending(o => o.CreatedDate)
            .ToListAsync();
    }

    // Lấy đơn hàng theo ID
    public async Task<Order?> GetOrderByIdAsync(int orderId)
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.AssignedStaff)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);
    }
}
```

### Giải Thích

Mẫu Repository cung cấp một lớp trừu tượng giữa logic nghiệp vụ và logic truy xuất dữ liệu. Nó đóng gói logic cần thiết để truy cập nguồn dữ liệu và cung cấp một giao diện giống collection để truy cập các đối tượng domain.

### Cách Hoạt Động Trong Hệ Thống

1. **DbContext như Generic Repository**: Entity Framework's `DbContext` đóng vai trò như Unit of Work và các thuộc tính `DbSet<T>` đóng vai trò như repository cho mỗi loại entity.

2. **Service Layer như Specialized Repositories**: Các service như `OrderService`, `UserAccountService`, và `DeliveryStaffService` bao bọc `DbContext` và cung cấp các phương thức cụ thể cho các thao tác truy xuất dữ liệu (CRUD operations).

3. **Lợi Ích**:
   - Logic truy xuất dữ liệu được tập trung
   - Dễ dàng mock để unit testing
   - Có thể dễ dàng chuyển đổi nguồn dữ liệu (ví dụ: từ SQL Server sang database khác)
   - Controllers không tương tác trực tiếp với database

---

## 3. Mẫu Service Layer (Tầng Dịch Vụ)

### Phân Loại
**Mẫu Cấu Trúc (Structural Pattern)**

### Vị trí xuất hiện trong dự án

Tất cả các class service trong thư mục [Services](DeliveryManagementAPI/Services/):
- [OrderService.cs](DeliveryManagementAPI/Services/OrderService.cs)
- [NotificationService.cs](DeliveryManagementAPI/Services/NotificationService.cs)
- [UserAccountService.cs](DeliveryManagementAPI/Services/UserAccountService.cs)
- [ShippingFeeService.cs](DeliveryManagementAPI/Services/ShippingFeeService.cs)
- [EmailService.cs](DeliveryManagementAPI/Services/EmailService.cs)
- [TwoFactorService.cs](DeliveryManagementAPI/Services/TwoFactorService.cs)

### Ví Dụ Code

**Logic Nghiệp Vụ trong [ShippingFeeService.cs](DeliveryManagementAPI/Services/ShippingFeeService.cs#L9-L73):**
```csharp
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

        // Phí theo loại giao hàng
        if (orderDto.DeliveryType == DeliveryType.GiaoHangNhanh)
        {
            totalFee *= 1.5m; // Tăng 50% cho giao hàng nhanh
        }

        return totalFee;
    }
}
```

**Sử dụng trong Controller ([OrdersController.cs](DeliveryManagementAPI/Controllers/OrdersController.cs)):**
```csharp
public class OrdersController : ControllerBase
{
    private readonly ShippingFeeService _feeService;
    
    // Service được tiêm vào và sử dụng cho logic nghiệp vụ
    public OrdersController(ShippingFeeService feeService)
    {
        _feeService = feeService;
    }
    
    // Controllers ủy quyền logic nghiệp vụ cho services
}
```

### Giải Thích

Mẫu Service Layer định nghĩa ranh giới của ứng dụng với một lớp service thiết lập tập hợp các thao tác có sẵn và điều phối phản hồi của ứng dụng trong mỗi thao tác. Logic nghiệp vụ được đóng gói trong các class service.

### Cách Hoạt Động Trong Hệ Thống

1. **Tách Biệt Các Mối Quan Tâm**: Logic nghiệp vụ được tách riêng khỏi controllers (tầng trình bày) và truy xuất dữ liệu (tầng repository).

2. **Các Class Service**: Mỗi service xử lý một domain cụ thể (đơn hàng, thông báo, người dùng, phí giao hàng, v.v.).

3. **Controllers như Tầng Mỏng**: Controllers mỏng và ủy quyền tất cả logic nghiệp vụ cho services.

4. **Lợi Ích**:
   - Logic nghiệp vụ có thể tái sử dụng trên nhiều controller
   - Dễ dàng test logic nghiệp vụ độc lập
   - Tách biệt rõ ràng giữa các tầng
   - Nguyên tắc trách nhiệm đơn nhất (Single Responsibility Principle)

---

## 4. Mẫu Singleton (Đơn Thể)

### Phân Loại
**Mẫu Khởi Tạo (Creational Pattern)**

### Vị trí xuất hiện trong dự án

- [ShippingFeeService](DeliveryManagementAPI/Services/ShippingFeeService.cs) - Đăng ký như Singleton trong [Program.cs](DeliveryManagementAPI/Program.cs#L27)
- [JsonDataService](DeliveryManagementAPI/Services/JsonDataService.cs) - Đăng ký như Singleton trong [Program.cs](DeliveryManagementAPI/Program.cs#L32)

### Ví Dụ Code

**Đăng ký Singleton trong [Program.cs](DeliveryManagementAPI/Program.cs#L27):**
```csharp
// Đăng ký services với Singleton lifetime
builder.Services.AddSingleton<ShippingFeeService>();
builder.Services.AddSingleton<JsonDataService>();
```

**Service Không Có State ([ShippingFeeService.cs](DeliveryManagementAPI/Services/ShippingFeeService.cs)):**
```csharp
/// <summary>
/// Service tính phí giao hàng
/// </summary>
public class ShippingFeeService
{
    // Không có state - an toàn khi dùng như Singleton
    
    public decimal CalculateShippingFee(CreateOrderDto orderDto)
    {
        decimal baseFee = 20000;
        decimal totalFee = baseFee;
        
        // Tính toán chỉ dựa trên tham số đầu vào
        // Không có state dùng chung được thay đổi
        
        return totalFee;
    }
}
```

### Giải Thích

Mẫu Singleton đảm bảo một class chỉ có một instance duy nhất trong suốt vòng đời ứng dụng và cung cấp một điểm truy cập toàn cục đến instance đó.

### Cách Hoạt Động Trong Hệ Thống

1. **Quản Lý bởi DI Container**: DI container của ASP.NET Core quản lý vòng đời singleton khi bạn đăng ký một service với `AddSingleton<T>()`.

2. **Services Không Có State**: Các service được đăng ký như singleton trong dự án này (`ShippingFeeService`, `JsonDataService`) không có state, nghĩa là chúng không duy trì bất kỳ trạng thái có thể thay đổi nào giữa các request.

3. **Thread Safety**: Vì các service này không thay đổi state dùng chung, chúng là thread-safe và có thể được chia sẻ giữa tất cả các request.

4. **Lợi Ích**:
   - Tiết kiệm bộ nhớ (chỉ có một instance)
   - Tốt cho các service không có state như calculators hoặc utilities
   - Nhanh hơn việc tạo instance mới cho mỗi request

5. **Khi Nào Sử Dụng**:
   - Services không giữ state cụ thể cho từng request
   - Services tính toán (như `ShippingFeeService`)
   - Services cấu hình
   - Services caching

---

## 5. Mẫu Strategy (Chiến Lược)

### Phân Loại
**Mẫu Hành Vi (Behavioral Pattern)**

### Vị trí xuất hiện trong dự án

- Interface [INotificationService](DeliveryManagementAPI/Services/INotificationService.cs) với implementation [NotificationService](DeliveryManagementAPI/Services/NotificationService.cs)
- Interface [IEmailService](DeliveryManagementAPI/Services/EmailService.cs) với implementation [EmailService](DeliveryManagementAPI/Services/EmailService.cs)
- Interface [ITwoFactorService](DeliveryManagementAPI/Services/TwoFactorService.cs) với implementation [TwoFactorService](DeliveryManagementAPI/Services/TwoFactorService.cs)

### Ví Dụ Code

**Interface Strategy ([IEmailService](DeliveryManagementAPI/Services/EmailService.cs#L6-L9)):**
```csharp
public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
}
```

**Concrete Strategy ([EmailService](DeliveryManagementAPI/Services/EmailService.cs#L11-L58)):**
```csharp
public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            var smtpHost = _config["Email:SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_config["Email:SmtpPort"] ?? "587");
            // ... Logic gửi email qua SMTP
            
            using var client = new SmtpClient(smtpHost, smtpPort);
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
            
            await client.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Gửi email thất bại: {ex.Message}");
            throw;
        }
    }
}
```

**Context sử dụng Strategy ([TwoFactorService](DeliveryManagementAPI/Services/TwoFactorService.cs#L12-L18)):**
```csharp
public class TwoFactorService : ITwoFactorService
{
    private readonly IEmailService _emailService;

    public TwoFactorService(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task<bool> SendOTPEmailAsync(string email, string fullName, string otp)
    {
        // Sử dụng email strategy để gửi OTP
        await _emailService.SendEmailAsync(email, subject, body);
        return true;
    }
}
```

**Đăng ký Service trong [Program.cs](DeliveryManagementAPI/Program.cs#L26-L28):**
```csharp
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITwoFactorService, TwoFactorService>();
```

### Giải Thích

Mẫu Strategy định nghĩa một họ các thuật toán, đóng gói từng thuật toán, và làm cho chúng có thể hoán đổi cho nhau. Strategy cho phép thuật toán thay đổi độc lập với các client sử dụng nó.

### Cách Hoạt Động Trong Hệ Thống

1. **Định Nghĩa Interface**: Các interface như `IEmailService`, `INotificationService`, và `ITwoFactorService` định nghĩa hợp đồng cho các strategy khác nhau.

2. **Nhiều Implementation**: Bạn có thể tạo các implementation khác nhau của cùng một interface:
   - `EmailService` có thể sử dụng SMTP
   - Bạn có thể tạo `SmsService` hoặc `PushNotificationService` implement cùng interface
   - `NotificationService` có thể được thay thế bằng một notification strategy khác

3. **Dependency Injection**: DI container tiêm implementation phù hợp dựa trên đăng ký.

4. **Lợi Ích**:
   - Dễ dàng thêm notification/email strategy mới mà không thay đổi code hiện có
   - Có thể hoán đổi implementation tại runtime hoặc configuration time
   - Nguyên tắc Đóng/Mở (Open/Closed Principle): mở cho mở rộng, đóng cho thay đổi
   - Dễ dàng test với mock implementation

5. **Ví Dụ Use Case**: 
   - Hiện tại, hệ thống sử dụng SMTP cho email
   - Bạn có thể dễ dàng tạo `AzureEmailService` hoặc `SendGridEmailService` implement `IEmailService`
   - Chỉ cần thay đổi đăng ký trong `Program.cs` mà không cần sửa code của consumer

---

## 6. Mẫu Observer (Quan Sát)

### Phân Loại
**Mẫu Hành Vi (Behavioral Pattern)**

### Vị trí xuất hiện trong dự án

- [SignalR Hubs](DeliveryManagementAPI/Hubs/):
  - [NotificationHub.cs](DeliveryManagementAPI/Hubs/NotificationHub.cs)
  - [TrackingHub.cs](DeliveryManagementAPI/Hubs/TrackingHub.cs)
  - [ChatHub.cs](DeliveryManagementAPI/Hubs/ChatHub.cs)
- [NotificationService.cs](DeliveryManagementAPI/Services/NotificationService.cs) - Broadcasting thông báo

### Ví Dụ Code

**Subject (Observable) - [NotificationService](DeliveryManagementAPI/Services/NotificationService.cs#L11-L24):**
```csharp
public class NotificationService : INotificationService
{
    private readonly DeliveryDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        DeliveryDbContext context,
        IHubContext<NotificationHub> hubContext,
        ILogger<NotificationService> logger)
    {
        _context = context;
        _hubContext = hubContext;
        _logger = logger;
    }
    
    public async Task<Notification> CreateNotificationAsync(
        int userId,
        string title,
        string message,
        NotificationType type,
        int? relatedEntityId = null,
        string? actionUrl = null)
    {
        var notification = new Notification { /* ... */ };
        
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        
        // Thông báo cho tất cả observers (các client đã kết nối)
        // Đây là cơ chế thông báo observer
        
        return notification;
    }
}
```

**Observer Hub - [NotificationHub](DeliveryManagementAPI/Hubs/NotificationHub.cs#L12-L44):**
```csharp
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Khi user kết nối (Observer đăng ký)
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            // Thêm connection vào group của user (subscription)
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            
            _logger.LogInformation($"User {userId} đã kết nối NotificationHub");
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Khi user ngắt kết nối (Observer hủy đăng ký)
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        await base.OnDisconnectedAsync(exception);
    }
}
```

**Real-time Tracking Observer - [TrackingHub](DeliveryManagementAPI/Hubs/TrackingHub.cs#L12-L25):**
```csharp
public class TrackingHub : Hub
{
    /// <summary>
    /// Shipper cập nhật vị trí (Subject thông báo observers)
    /// </summary>
    public async Task UpdateShipperLocation(int staffId, int orderId, double latitude, double longitude)
    {
        // Broadcast đến tất cả observers đang xem đơn hàng này
        await Clients.Group($"order_{orderId}").SendAsync("ReceiveShipperLocation", new
        {
            staffId,
            orderId,
            latitude,
            longitude,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Client đăng ký theo dõi đơn hàng
    /// </summary>
    public async Task JoinOrderTracking(string orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"order_{orderId}");
    }
}
```

### Giải Thích

Mẫu Observer định nghĩa một quan hệ phụ thuộc một-nhiều giữa các đối tượng sao cho khi một đối tượng (Subject) thay đổi trạng thái, tất cả các đối tượng phụ thuộc (Observers) được thông báo và cập nhật tự động.

### Cách Hoạt Động Trong Hệ Thống

1. **SignalR như Implementation của Observer**: SignalR cung cấp một implementation sẵn có của mẫu Observer cho giao tiếp real-time.

2. **Subject**: Các service phía server (`NotificationService`, `TrackingHub`, `ChatHub`) đóng vai trò subject duy trì danh sách các observer.

3. **Observers**: Các client đã kết nối (trình duyệt web, ứng dụng mobile) đóng vai trò observer đăng ký các sự kiện hoặc nhóm cụ thể.

4. **Cơ Chế Đăng Ký**:
   - Clients kết nối đến SignalR hubs
   - Họ tham gia các nhóm cụ thể (ví dụ: `user_{userId}`, `order_{orderId}`)
   - Khi sự kiện xảy ra, hub thông báo cho tất cả clients trong nhóm liên quan

5. **Ví Dụ Thực Tế Trong Hệ Thống**:
   - **Thông báo**: Khi tạo thông báo mới, tất cả clients đã kết nối của user đó được thông báo
   - **Tracking**: Khi shipper cập nhật vị trí, tất cả clients đang theo dõi đơn hàng đó nhận được cập nhật
   - **Chat**: Khi gửi tin nhắn, tất cả người tham gia chat đơn hàng nhận được tin nhắn

6. **Lợi Ích**:
   - Cập nhật real-time mà không cần polling
   - Giảm sự liên kết giữa subject và observers
   - Đăng ký/hủy đăng ký động
   - Có thể mở rộng cho nhiều observers

---

## 7. Mẫu Facade (Mặt Tiền)

### Phân Loại
**Mẫu Cấu Trúc (Structural Pattern)**

### Vị trí xuất hiện trong dự án

- [OrdersController.cs](DeliveryManagementAPI/Controllers/OrdersController.cs) - Cung cấp giao diện đơn giản hóa cho các subsystem phức tạp
- [AuthController.cs](DeliveryManagementAPI/Controllers/AuthController.cs) - Facade cho phức tạp của authentication
- Tất cả các class Controller đóng vai trò facade cho tầng service

### Ví Dụ Code

**Facade Controller ([OrdersController](DeliveryManagementAPI/Controllers/OrdersController.cs#L11-L33)):**
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    // Nhiều subsystem phức tạp
    private readonly OrderService _orderService;
    private readonly DeliveryStaffService _staffService;
    private readonly ShippingFeeService _feeService;
    private readonly DeliveryDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        OrderService orderService,
        DeliveryStaffService staffService,
        ShippingFeeService feeService,
        DeliveryDbContext context,
        INotificationService notificationService,
        ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _staffService = staffService;
        _feeService = feeService;
        _context = context;
        _notificationService = notificationService;
        _logger = logger;
    }
    
    // Giao diện đơn giản cho thao tác phức tạp
    [HttpGet]
    public async Task<ActionResult<List<Order>>> GetAllOrders()
    {
        // Client chỉ gọi một phương thức, controller điều phối nhiều service
        var orders = await _orderService.GetAllOrdersAsync();
        return Ok(orders);
    }
}
```

**Thao Tác Phức Tạp Được Đơn Giản Hóa ([AuthController](DeliveryManagementAPI/Controllers/AuthController.cs#L30-L56)):**
```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest req)
{
    // Facade ẩn đi sự phức tạp của:
    // 1. Xác thực người dùng
    var user = await _userService.AuthenticateAsync(req.Username!, req.Password!);
    if (user == null)
        return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu" });

    // 2. Kiểm tra xác thực hai yếu tố
    if (user.TwoFactorEnabled)
    {
        // 3. Tạo mã OTP
        var otp = _twoFactorService.GenerateOTP();
        
        // 4. Cập nhật database
        await _userService.SetTwoFactorCodeAsync(user.UserId, otp, DateTime.UtcNow.AddMinutes(5));
        
        // 5. Gửi email
        await _twoFactorService.SendOTPEmailAsync(user.Email, user.FullName, otp);

        return Ok(new { requiresTwoFactor = true });
    }

    // 6. Tạo JWT token
    var token = GenerateJwtToken(user);
    
    // Client nhận được response đơn giản mặc dù xử lý backend phức tạp
    return Ok(new { token, user = new { user.UserId, user.Username } });
}
```

### Giải Thích

Mẫu Facade cung cấp một giao diện thống nhất, đơn giản hóa cho một tập hợp các giao diện trong một subsystem. Nó định nghĩa một giao diện cấp cao hơn làm cho subsystem dễ sử dụng hơn.

### Cách Hoạt Động Trong Hệ Thống

1. **Controllers như Facades**: Tất cả controllers đóng vai trò facade cung cấp một giao diện HTTP API đơn giản cho các thao tác backend phức tạp.

2. **Ẩn Sự Phức Tạp**: Controller điều phối nhiều service, xử lý lỗi, quản lý transaction, và định dạng response, nhưng client chỉ cần thực hiện các HTTP call đơn giản.

3. **Ví Dụ Luồng Xử Lý**:
   - Client gọi `POST /api/auth/login`
   - AuthController (Facade) điều phối:
     - UserAccountService (xác thực)
     - TwoFactorService (tạo OTP)
     - EmailService (gửi email)
     - Tạo token
   - Client nhận response JSON đơn giản

4. **Lợi Ích**:
   - Đơn giản hóa API cho clients
   - Giảm sự liên kết giữa client và subsystem
   - Làm cho hệ thống dễ sử dụng và hiểu
   - Có thể phát triển backend mà không thay đổi giao diện client

5. **So Sánh Thực Tế**: Giống như lễ tân khách sạn điều phối dọn phòng, room service, bảo trì, v.v., controllers điều phối nhiều service để đáp ứng yêu cầu của client.

---

## 8. Mẫu Template Method (Phương Thức Khuôn Mẫu)

### Phân Loại
**Mẫu Hành Vi (Behavioral Pattern)**

### Vị trí xuất hiện trong dự án

- Sử dụng class base SignalR Hub:
  - [NotificationHub.cs](DeliveryManagementAPI/Hubs/NotificationHub.cs) override `OnConnectedAsync` và `OnDisconnectedAsync`
  - [TrackingHub.cs](DeliveryManagementAPI/Hubs/TrackingHub.cs) override các phương thức lifecycle của hub
  - [ChatHub.cs](DeliveryManagementAPI/Hubs/ChatHub.cs) override các phương thức lifecycle của hub

### Ví Dụ Code

**Template Method trong Base Class (SignalR's `Hub`):**
```csharp
// Class Hub của SignalR định nghĩa template
public abstract class Hub
{
    // Template method - định nghĩa khung xử lý kết nối
    public virtual Task OnConnectedAsync() 
    { 
        return Task.CompletedTask; 
    }
    
    public virtual Task OnDisconnectedAsync(Exception? exception) 
    { 
        return Task.CompletedTask; 
    }
}
```

**Concrete Implementation ([NotificationHub](DeliveryManagementAPI/Hubs/NotificationHub.cs#L19-L54)):**
```csharp
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Override template method để tùy chỉnh hành vi kết nối
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            // Logic tùy chỉnh: Thêm vào group của user
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            
            _logger.LogInformation($"User {userId} đã kết nối NotificationHub");
        }
        else
        {
            _logger.LogWarning($"User ẩn danh đã kết nối");
        }

        // Gọi implementation của base (template)
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Override template method để tùy chỉnh hành vi ngắt kết nối
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            // Logic tùy chỉnh: Xóa khỏi group
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            
            _logger.LogInformation($"User {userId} đã ngắt kết nối");
        }

        if (exception != null)
        {
            _logger.LogError(exception, "Lỗi khi ngắt kết nối");
        }

        // Gọi implementation của base (template)
        await base.OnDisconnectedAsync(exception);
    }
}
```

**Implementation Khác ([ChatHub](DeliveryManagementAPI/Hubs/ChatHub.cs#L46-L55)):**
```csharp
public class ChatHub : Hub
{
    // Override template methods với logic tùy chỉnh khác
    public override async Task OnConnectedAsync()
    {
        // Logic tùy chỉnh khác cho chat connections
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Logic tùy chỉnh khác cho chat disconnections
        await base.OnDisconnectedAsync(exception);
    }
}
```

### Giải Thích

Mẫu Template Method định nghĩa khung của một thuật toán trong class base, cho phép các subclass override các bước cụ thể mà không thay đổi cấu trúc của thuật toán.

### Cách Hoạt Động Trong Hệ Thống

1. **Base Class Template**: Class `Hub` của SignalR định nghĩa các phương thức virtual (`OnConnectedAsync`, `OnDisconnectedAsync`) đóng vai trò như template methods.

2. **Concrete Implementations**: Mỗi hub (`NotificationHub`, `ChatHub`, `TrackingHub`) override các phương thức này để cung cấp hành vi cụ thể trong khi vẫn duy trì cấu trúc vòng đời kết nối tổng thể.

3. **Cấu Trúc Thuật Toán**:
   - Kết nối được thiết lập (framework xử lý)
   - `OnConnectedAsync()` được gọi (có thể tùy chỉnh bởi subclass)
   - Các phương thức hub được thực thi
   - `OnDisconnectedAsync()` được gọi (có thể tùy chỉnh bởi subclass)
   - Kết nối được dọn dẹp (framework xử lý)

4. **Lợi Ích**:
   - Tái sử dụng code: Logic xử lý kết nối chung nằm trong base class
   - Linh hoạt: Mỗi hub có thể tùy chỉnh hành vi kết nối/ngắt kết nối
   - Nhất quán: Tất cả hubs tuân theo cùng một cấu trúc vòng đời
   - Nguyên tắc Đóng/Mở: Có thể mở rộng mà không cần sửa base class

5. **So Sánh Thực Tế**: Giống như một công thức nấu ăn định nghĩa các bước (template), nhưng cho phép bạn tùy chỉnh nguyên liệu (override methods) trong khi vẫn giữ nguyên quy trình nấu.

---

## 9. Mẫu Factory Method (Phương Thức Nhà Máy)

### Phân Loại
**Mẫu Khởi Tạo (Creational Pattern)**

### Vị trí xuất hiện trong dự án

- [SeedData.cs](DeliveryManagementAPI/Data/SeedData.cs) - Factory để tạo dữ liệu test ban đầu
- [UserAccountService.cs](DeliveryManagementAPI/Services/UserAccountService.cs) - Factory methods để tạo user

### Ví Dụ Code

**Factory Method để Tạo User ([UserAccountService](DeliveryManagementAPI/Services/UserAccountService.cs#L75-L82)):**
```csharp
public class UserAccountService
{
    private readonly DeliveryDbContext _context;

    // Factory method: Tạo user với password hashing
    public async Task<UserAccount> RegisterAsync(UserAccount user, string password)
    {
        // Logic tạo đối tượng được đóng gói
        user.PasswordHash = HashPassword(password);
        _context.UserAccounts.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    // Factory method: Tạo user không có password (cho OAuth)
    public async Task<UserAccount> CreateUserAsync(UserAccount user)
    {
        _context.UserAccounts.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }
    
    // Helper factory method
    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
```

**Data Factory ([SeedData.cs](DeliveryManagementAPI/Data/SeedData.cs#L12-L65)):**
```csharp
public static class SeedData
{
    // Factory method: Tạo trạng thái database ban đầu
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using var context = new DeliveryDbContext(
            serviceProvider.GetRequiredService<DbContextOptions<DeliveryDbContext>>());

        if (context.UserAccounts.Any())
        {
            return; // Database đã được seed
        }

        // Factory pattern: Tạo các đối tượng chuẩn hóa
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
            // Thêm users...
        };
        
        context.UserAccounts.AddRange(users);
        await context.SaveChangesAsync();
    }
}
```

**Sử dụng trong Controller ([AuthController](DeliveryManagementAPI/Controllers/AuthController.cs#L61-L77)):**
```csharp
[HttpPost("register")]
public async Task<IActionResult> Register([FromBody] RegisterRequest req)
{
    // Validation
    if (await _userService.UsernameExistsAsync(req.Username!))
        return BadRequest(new { message = "Tên đăng nhập đã tồn tại" });
    
    // Factory method tạo đối tượng user
    var user = new UserAccount
    {
        Username = req.Username!,
        FullName = req.FullName!,
        Email = req.Email!,
        PhoneNumber = req.PhoneNumber!,
        Role = "customer"
    };
    
    // Factory method xử lý logic tạo (password hashing, database insertion)
    await _userService.RegisterAsync(user, req.Password!);
    
    return Ok(new { success = true });
}
```

### Giải Thích

Mẫu Factory Method định nghĩa một interface để tạo đối tượng, nhưng để subclass hoặc phương thức quyết định class nào sẽ được khởi tạo. Nó đóng gói logic tạo đối tượng.

### Cách Hoạt Động Trong Hệ Thống

1. **UserAccountService như Factory**: Các phương thức `RegisterAsync` và `CreateUserAsync` đóng vai trò factory methods đóng gói logic tạo tài khoản người dùng.

2. **Logic Tạo Được Đóng Gói**:
   - Password hashing
   - Database insertion
   - Validation
   - Thiết lập giá trị mặc định

3. **SeedData như Factory**: Phương thức `Initialize` là một factory tạo tập dữ liệu test ban đầu với cấu trúc và validation nhất quán.

4. **Lợi Ích**:
   - Tạo đối tượng tập trung
   - Khởi tạo đối tượng nhất quán
   - Dễ dàng sửa logic tạo ở một nơi
   - Có thể thêm validation rules mà không thay đổi code client
   - Thúc đẩy nguyên tắc DRY (Don't Repeat Yourself)

5. **Ví Dụ Thực Tế**: Giống như nhà máy ô tô biết tất cả các bước để chế tạo xe - clients không cần biết về lắp ráp động cơ, sơn, v.v. Họ chỉ cần yêu cầu một chiếc xe.

---

## Đề Xuất Các Mẫu Bổ Sung

Mặc dù dự án đã triển khai nhiều design pattern một cách hiệu quả, dưới đây là một số pattern có thể hữu ích để thêm vào hoặc làm rõ hơn:

### 1. **Mẫu Builder** (Creational)

**Nơi có thể áp dụng**: Tạo đối tượng `Order` phức tạp

**Thách Thức Hiện Tại**: Class `Order` có nhiều thuộc tính (weight, distance, special flags, delivery type, payment method, v.v.). Tạo orders yêu cầu thiết lập nhiều thuộc tính.

**Đề Xuất Implementation**:
```csharp
public class OrderBuilder
{
    private Order _order = new Order();
    
    public OrderBuilder WithCustomer(Customer customer)
    {
        _order.Customer = customer;
        _order.CustomerId = customer.CustomerId;
        return this;
    }
    
    public OrderBuilder WithPackageDetails(PackageType type, double weight, string size)
    {
        _order.PackageType = type;
        _order.Weight = weight;
        _order.Size = size;
        return this;
    }
    
    public OrderBuilder WithDeliveryType(DeliveryType type)
    {
        _order.DeliveryType = type;
        return this;
    }
    
    public OrderBuilder IsFragile(bool isFragile = true)
    {
        _order.IsFragile = isFragile;
        return this;
    }
    
    public OrderBuilder WithShippingFee(decimal fee)
    {
        _order.ShippingFee = fee;
        return this;
    }
    
    public Order Build()
    {
        // Validation trước khi tạo
        if (string.IsNullOrEmpty(_order.OrderCode))
            _order.OrderCode = GenerateOrderCode();
        
        _order.CreatedDate = DateTime.Now;
        return _order;
    }
    
    private string GenerateOrderCode()
    {
        return $"ORD{DateTime.Now:yyyyMMddHHmmss}";
    }
}

// Sử dụng:
var order = new OrderBuilder()
    .WithCustomer(customer)
    .WithPackageDetails(PackageType.Laptop, 2.5, "30x40x10")
    .WithDeliveryType(DeliveryType.GiaoHangNhanh)
    .IsFragile()
    .WithShippingFee(50000)
    .Build();
```

**Lợi Ích**:
- Code dễ đọc hơn khi tạo orders phức tạp
- Đảm bảo các trường bắt buộc được thiết lập
- Cho phép xây dựng từng bước
- Dễ dàng thêm validation

---

### 2. **Mẫu Decorator** (Structural)

**Nơi có thể áp dụng**: Mở rộng chức năng notification

**Thách Thức Hiện Tại**: `NotificationService` có thể cần nhiều cách khác nhau để gửi thông báo (email, SMS, push, in-app) với các tính năng tùy chọn (logging, retry logic, rate limiting).

**Đề Xuất Implementation**:
```csharp
// Base interface
public interface INotificationSender
{
    Task SendAsync(Notification notification);
}

// Implementation cơ bản
public class BasicNotificationSender : INotificationSender
{
    private readonly IHubContext<NotificationHub> _hubContext;
    
    public async Task SendAsync(Notification notification)
    {
        await _hubContext.Clients.Group($"user_{notification.UserId}")
            .SendAsync("ReceiveNotification", notification);
    }
}

// Decorator: Thêm logging
public class LoggingNotificationDecorator : INotificationSender
{
    private readonly INotificationSender _inner;
    private readonly ILogger _logger;
    
    public async Task SendAsync(Notification notification)
    {
        _logger.LogInformation($"Đang gửi thông báo {notification.Id}");
        await _inner.SendAsync(notification);
        _logger.LogInformation($"Đã gửi thông báo {notification.Id}");
    }
}

// Decorator: Thêm retry logic
public class RetryNotificationDecorator : INotificationSender
{
    private readonly INotificationSender _inner;
    
    public async Task SendAsync(Notification notification)
    {
        int retries = 3;
        while (retries > 0)
        {
            try
            {
                await _inner.SendAsync(notification);
                return;
            }
            catch
            {
                retries--;
                if (retries == 0) throw;
                await Task.Delay(1000);
            }
        }
    }
}

// Sử dụng:
INotificationSender sender = new BasicNotificationSender(hubContext);
sender = new LoggingNotificationDecorator(sender, logger);
sender = new RetryNotificationDecorator(sender);

await sender.SendAsync(notification); // Sẽ log và retry nếu fail
```

**Lợi Ích**:
- Thêm tính năng mà không sửa code hiện có
- Kết hợp các tính năng linh hoạt
- Nguyên tắc trách nhiệm đơn nhất (Single Responsibility Principle)

---

### 3. **Mẫu Command** (Behavioral)

**Nơi có thể áp dụng**: Cập nhật trạng thái đơn hàng và các hành động

**Thách Thức Hiện Tại**: Các hành động khác nhau trên đơn hàng (accept, assign, deliver, complete) bị phân tán trong các phương thức controller.

**Đề Xuất Implementation**:
```csharp
// Command interface
public interface IOrderCommand
{
    Task ExecuteAsync();
    Task UndoAsync();
}

// Concrete command: Gán đơn hàng cho shipper
public class AssignOrderCommand : IOrderCommand
{
    private readonly Order _order;
    private readonly DeliveryStaff _staff;
    private readonly DeliveryDbContext _context;
    private readonly INotificationService _notificationService;
    private string? _previousStaffId;
    
    public AssignOrderCommand(Order order, DeliveryStaff staff, 
        DeliveryDbContext context, INotificationService notificationService)
    {
        _order = order;
        _staff = staff;
        _context = context;
        _notificationService = notificationService;
    }
    
    public async Task ExecuteAsync()
    {
        _previousStaffId = _order.AssignedStaffId;
        _order.AssignedStaffId = _staff.StaffId.ToString();
        _order.Status = OrderStatus.DaNhanChuaGiao;
        
        await _context.SaveChangesAsync();
        
        await _notificationService.CreateNotificationAsync(
            _staff.StaffId,
            "Đơn hàng mới",
            $"Bạn được giao đơn hàng {_order.OrderCode}",
            NotificationType.OrderAssigned,
            _order.OrderId
        );
    }
    
    public async Task UndoAsync()
    {
        _order.AssignedStaffId = _previousStaffId;
        _order.Status = OrderStatus.ChuaNhan;
        await _context.SaveChangesAsync();
    }
}

// Invoker
public class OrderCommandInvoker
{
    private Stack<IOrderCommand> _commandHistory = new Stack<IOrderCommand>();
    
    public async Task ExecuteCommandAsync(IOrderCommand command)
    {
        await command.ExecuteAsync();
        _commandHistory.Push(command);
    }
    
    public async Task UndoLastCommandAsync()
    {
        if (_commandHistory.Any())
        {
            var command = _commandHistory.Pop();
            await command.UndoAsync();
        }
    }
}

// Sử dụng:
var command = new AssignOrderCommand(order, shipper, context, notificationService);
await invoker.ExecuteCommandAsync(command);

// Có thể undo sau này
await invoker.UndoLastCommandAsync();
```

**Lợi Ích**:
- Đóng gói request như các đối tượng
- Hỗ trợ chức năng undo/redo
- Có thể queue commands
- Có thể log lịch sử command

---

### 4. **Mẫu Chain of Responsibility** (Behavioral)

**Nơi có thể áp dụng**: Validation đơn hàng và authorization

**Thách Thức Hiện Tại**: Tạo đơn hàng liên quan đến nhiều bước validation (customer validation, payment validation, address validation, v.v.).

**Đề Xuất Implementation**:
```csharp
// Handler interface
public interface IOrderValidationHandler
{
    IOrderValidationHandler SetNext(IOrderValidationHandler handler);
    Task<ValidationResult> HandleAsync(CreateOrderDto order);
}

// Base handler
public abstract class OrderValidationHandler : IOrderValidationHandler
{
    private IOrderValidationHandler? _nextHandler;
    
    public IOrderValidationHandler SetNext(IOrderValidationHandler handler)
    {
        _nextHandler = handler;
        return handler;
    }
    
    public virtual async Task<ValidationResult> HandleAsync(CreateOrderDto order)
    {
        if (_nextHandler != null)
            return await _nextHandler.HandleAsync(order);
        
        return ValidationResult.Success();
    }
}

// Concrete handler: Customer validation
public class CustomerValidationHandler : OrderValidationHandler
{
    private readonly DeliveryDbContext _context;
    
    public override async Task<ValidationResult> HandleAsync(CreateOrderDto order)
    {
        var customer = await _context.Customers.FindAsync(order.CustomerId);
        if (customer == null)
            return ValidationResult.Failure("Khách hàng không tồn tại");
        
        if (string.IsNullOrEmpty(customer.PhoneNumber))
            return ValidationResult.Failure("Khách hàng chưa có số điện thoại");
        
        // Chuyển đến handler tiếp theo
        return await base.HandleAsync(order);
    }
}

// Concrete handler: Distance validation
public class DistanceValidationHandler : OrderValidationHandler
{
    public override async Task<ValidationResult> HandleAsync(CreateOrderDto order)
    {
        if (order.Distance <= 0)
            return ValidationResult.Failure("Khoảng cách phải lớn hơn 0");
        
        if (order.Distance > 100)
            return ValidationResult.Failure("Khoảng cách tối đa là 100km");
        
        return await base.HandleAsync(order);
    }
}

// Concrete handler: Weight validation
public class WeightValidationHandler : OrderValidationHandler
{
    public override async Task<ValidationResult> HandleAsync(CreateOrderDto order)
    {
        if (order.Weight <= 0)
            return ValidationResult.Failure("Trọng lượng phải lớn hơn 0");
        
        if (order.Weight > 50)
            return ValidationResult.Failure("Trọng lượng tối đa là 50kg");
        
        return await base.HandleAsync(order);
    }
}

// Sử dụng:
var validator = new CustomerValidationHandler(context);
validator
    .SetNext(new DistanceValidationHandler())
    .SetNext(new WeightValidationHandler());

var result = await validator.HandleAsync(orderDto);
if (!result.IsValid)
    return BadRequest(result.ErrorMessage);
```

**Lợi Ích**:
- Tách biệt sender và receiver
- Dễ dàng thêm/bỏ các bước validation
- Mỗi validator có trách nhiệm đơn nhất
- Chuỗi validation linh hoạt

---

### 5. **Mẫu State** (Behavioral)

**Nơi có thể áp dụng**: Quản lý trạng thái đơn hàng

**Thách Thức Hiện Tại**: Thay đổi trạng thái đơn hàng được xử lý bằng switch statements và logic điều kiện phân tán trong code.

**Đề Xuất Implementation**:
```csharp
// State interface
public interface IOrderState
{
    OrderStatus Status { get; }
    Task<bool> CanTransitionTo(OrderStatus newStatus);
    Task OnEnterAsync(Order order);
    Task OnExitAsync(Order order);
}

// Concrete state: Pending
public class PendingOrderState : IOrderState
{
    private readonly INotificationService _notificationService;
    
    public OrderStatus Status => OrderStatus.ChuaNhan;
    
    public async Task<bool> CanTransitionTo(OrderStatus newStatus)
    {
        // Chỉ có thể chuyển sang Accepted hoặc Cancelled
        return newStatus == OrderStatus.DaNhanChuaGiao 
            || newStatus == OrderStatus.DaHuy;
    }
    
    public async Task OnEnterAsync(Order order)
    {
        // Thông báo cho admin về đơn hàng mới
        await _notificationService.CreateNotificationAsync(
            adminId,
            "Đơn hàng mới",
            $"Có đơn hàng mới {order.OrderCode}",
            NotificationType.NewOrder,
            order.OrderId
        );
    }
    
    public Task OnExitAsync(Order order) => Task.CompletedTask;
}

// Concrete state: In Delivery
public class InDeliveryOrderState : IOrderState
{
    public OrderStatus Status => OrderStatus.DaNhanDangGiao;
    
    public async Task<bool> CanTransitionTo(OrderStatus newStatus)
    {
        // Chỉ có thể chuyển sang Completed hoặc Failed
        return newStatus == OrderStatus.DaGiao 
            || newStatus == OrderStatus.GiaoKhongThanhCong;
    }
    
    public async Task OnEnterAsync(Order order)
    {
        order.DeliveryStartDate = DateTime.Now;
        
        // Thông báo khách hàng
        await _notificationService.CreateNotificationAsync(
            order.CreatedByUserId.Value,
            "Đang giao hàng",
            $"Đơn hàng {order.OrderCode} đang được giao",
            NotificationType.OrderInDelivery,
            order.OrderId
        );
    }
    
    public Task OnExitAsync(Order order) => Task.CompletedTask;
}

// Order với state machine
public class OrderStateMachine
{
    private readonly Order _order;
    private IOrderState _currentState;
    private readonly Dictionary<OrderStatus, IOrderState> _states;
    
    public async Task TransitionToAsync(OrderStatus newStatus)
    {
        if (!await _currentState.CanTransitionTo(newStatus))
            throw new InvalidOperationException(
                $"Không thể chuyển từ {_currentState.Status} sang {newStatus}");
        
        await _currentState.OnExitAsync(_order);
        _currentState = _states[newStatus];
        _order.Status = newStatus;
        await _currentState.OnEnterAsync(_order);
    }
}
```

**Lợi Ích**:
- Đóng gói hành vi cụ thể của state
- Dễ dàng thêm state mới
- Chuyển đổi state rõ ràng và được validate
- Loại bỏ logic điều kiện phức tạp

---

### 6. **Mẫu Adapter** (Structural)

**Nơi có thể áp dụng**: Tích hợp cổng thanh toán hoặc shipping API của bên thứ 3

**Đề Xuất Implementation**:
```csharp
// Target interface (interface mà hệ thống mong đợi)
public interface IPaymentGateway
{
    Task<PaymentResult> ProcessPaymentAsync(decimal amount, string orderCode);
    Task<RefundResult> RefundAsync(string transactionId, decimal amount);
}

// Adaptee: VNPay (bên thứ 3 với interface khác)
public class VNPayService
{
    public VNPayResponse CreatePaymentUrl(VNPayRequest request) { /* ... */ }
    public VNPayVerifyResult VerifyPayment(string queryString) { /* ... */ }
}

// Adapter: Làm cho VNPay tương thích với interface của chúng ta
public class VNPayAdapter : IPaymentGateway
{
    private readonly VNPayService _vnPayService;
    private readonly IConfiguration _config;
    
    public VNPayAdapter(VNPayService vnPayService, IConfiguration config)
    {
        _vnPayService = vnPayService;
        _config = config;
    }
    
    public async Task<PaymentResult> ProcessPaymentAsync(decimal amount, string orderCode)
    {
        // Chuyển đổi interface của chúng ta sang interface của VNPay
        var request = new VNPayRequest
        {
            OrderId = orderCode,
            Amount = (long)amount * 100, // VNPay dùng đơn vị xu
            OrderDescription = $"Thanh toán đơn hàng {orderCode}",
            ReturnUrl = _config["VNPay:ReturnUrl"]
        };
        
        var response = _vnPayService.CreatePaymentUrl(request);
        
        // Chuyển đổi response của VNPay sang format chung của chúng ta
        return new PaymentResult
        {
            Success = response.ResponseCode == "00",
            TransactionId = response.TransactionId,
            PaymentUrl = response.PaymentUrl
        };
    }
    
    public async Task<RefundResult> RefundAsync(string transactionId, decimal amount)
    {
        // Implement refund logic chuyển đổi sang interface VNPay
        throw new NotImplementedException();
    }
}

// Adapter khác cho Momo
public class MomoAdapter : IPaymentGateway
{
    private readonly MomoService _momoService;
    
    public async Task<PaymentResult> ProcessPaymentAsync(decimal amount, string orderCode)
    {
        // Chuyển đổi interface của chúng ta sang interface của Momo
        // ...
    }
    
    public async Task<RefundResult> RefundAsync(string transactionId, decimal amount)
    {
        // ...
    }
}

// Sử dụng trong service:
public class PaymentService
{
    private readonly Dictionary<PaymentMethod, IPaymentGateway> _gateways;
    
    public PaymentService(
        VNPayAdapter vnPayAdapter,
        MomoAdapter momoAdapter)
    {
        _gateways = new Dictionary<PaymentMethod, IPaymentGateway>
        {
            { PaymentMethod.VNPay, vnPayAdapter },
            { PaymentMethod.Momo, momoAdapter }
        };
    }
    
    public async Task<PaymentResult> ProcessPaymentAsync(
        PaymentMethod method,
        decimal amount,
        string orderCode)
    {
        var gateway = _gateways[method];
        return await gateway.ProcessPaymentAsync(amount, orderCode);
    }
}
```

**Lợi Ích**:
- Có thể tích hợp nhiều service bên thứ 3 với interface khác nhau
- Dễ dàng thêm cổng thanh toán mới
- Hệ thống không phụ thuộc vào API cụ thể của bên thứ 3
- Có thể chuyển đổi nhà cung cấp thanh toán mà không thay đổi logic nghiệp vụ

---

## Tổng Kết

Hệ Thống Quản Lý Giao Hàng này đã triển khai một số design pattern quan trọng:

### Các Pattern Đã Triển Khai:
1. ✅ **Dependency Injection** - Xuyên suốt toàn bộ ứng dụng
2. ✅ **Repository Pattern** - Qua DbContext và Service layer
3. ✅ **Service Layer** - Tất cả services tách biệt logic nghiệp vụ
4. ✅ **Singleton** - ShippingFeeService, JsonDataService
5. ✅ **Strategy** - IEmailService, INotificationService, ITwoFactorService
6. ✅ **Observer** - SignalR Hubs cho cập nhật real-time
7. ✅ **Facade** - Controllers đơn giản hóa các thao tác phức tạp
8. ✅ **Template Method** - SignalR Hub lifecycle methods
9. ✅ **Factory Method** - UserAccountService, SeedData

### Các Pattern Đề Xuất:
- 🔲 **Builder** - Cho việc tạo Order phức tạp
- 🔲 **Decorator** - Để mở rộng tính năng notification
- 🔲 **Command** - Cho các hành động trên đơn hàng với hỗ trợ undo
- 🔲 **Chain of Responsibility** - Cho validation pipelines
- 🔲 **State** - Cho quản lý trạng thái đơn hàng
- 🔲 **Adapter** - Cho tích hợp bên thứ 3

Dự án thể hiện kiến trúc phần mềm tốt với sự tách biệt rõ ràng các mối quan tâm, quản lý phụ thuộc, và khả năng mở rộng. Các pattern hiện có cung cấp nền tảng vững chắc cho tính bảo trì và khả năng mở rộng.

---

## Tài Nguyên Học Tập

Để tìm hiểu thêm về các design pattern này:

1. **Sách**:
   - "Design Patterns: Elements of Reusable Object-Oriented Software" của Gang of Four
   - "Head First Design Patterns" của Freeman & Freeman
   - "Patterns of Enterprise Application Architecture" của Martin Fowler

2. **Trực Tuyến**:
   - [Refactoring Guru](https://refactoring.guru/design-patterns) - Có phiên bản tiếng Việt
   - [Microsoft Documentation - Cloud Design Patterns](https://docs.microsoft.com/en-us/azure/architecture/patterns/)
   - [SourceMaking](https://sourcemaking.com/design_patterns)

3. **Thực Hành**:
   - Xem lại codebase này và xác định cách các pattern được sử dụng
   - Thử triển khai các pattern được đề xuất như bài tập
   - Refactor code hiện có để sử dụng pattern ở những nơi phù hợp

---

**Phiên Bản Tài Liệu**: 1.0  
**Cập Nhật Lần Cuối**: 8 tháng 3, 2026  
**Dự Án**: Hệ Thống Quản Lý Giao Hàng (Case Study 14)  
**Ngôn Ngữ**: Tiếng Việt

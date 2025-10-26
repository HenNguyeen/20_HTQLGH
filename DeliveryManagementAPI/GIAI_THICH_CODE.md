# 📚 Giải Thích Cấu Trúc Code

## 🏗️ Kiến trúc tổng quan

```
┌─────────────────────────────────────────┐
│         Client (Browser/App)            │
│      Swagger UI / HTTP Client           │
└────────────┬────────────────────────────┘
             │ HTTP Request
             ▼
┌─────────────────────────────────────────┐
│          Controllers Layer              │
│  ┌──────────────────────────────────┐   │
│  │ OrdersController                 │   │
│  │ DeliveryStaffController          │   │
│  │ TrackingController               │   │
│  └──────────────────────────────────┘   │
└────────────┬────────────────────────────┘
             │ Business Logic
             ▼
┌─────────────────────────────────────────┐
│           Services Layer                │
│  ┌──────────────────────────────────┐   │
│  │ JsonDataService                  │   │
│  │ ShippingFeeService               │   │
│  └──────────────────────────────────┘   │
└────────────┬────────────────────────────┘
             │ Data Access
             ▼
┌─────────────────────────────────────────┐
│            Data Layer                   │
│  ┌──────────────────────────────────┐   │
│  │ orders.json                      │   │
│  │ delivery-staff.json              │   │
│  │ checkpoints.json                 │   │
│  └──────────────────────────────────┘   │
└─────────────────────────────────────────┘
```

---

## 📁 Giải thích từng thành phần

### 1. 🎯 Controllers (API Endpoints)

Controllers xử lý HTTP requests và trả về responses.

#### OrdersController.cs
```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly JsonDataService _dataService;
    private readonly ShippingFeeService _feeService;
    
    // Dependency Injection trong constructor
    public OrdersController(JsonDataService dataService, ShippingFeeService feeService)
    {
        _dataService = dataService;
        _feeService = feeService;
    }
}
```

**Nhiệm vụ:**
- Nhận HTTP request từ client
- Validate dữ liệu đầu vào
- Gọi services để xử lý logic
- Trả về HTTP response (JSON)

**Các HTTP Methods:**
- `GET` - Lấy dữ liệu
- `POST` - Tạo mới
- `PATCH` - Cập nhật một phần
- `DELETE` - Xóa

**Ví dụ endpoint:**
```csharp
[HttpGet("{id}")]  // Route: GET /api/orders/{id}
public async Task<ActionResult<Order>> GetOrderById(string id)
{
    var order = await _dataService.GetOrderByIdAsync(id);
    if (order == null)
        return NotFound();
    return Ok(order);
}
```

---

### 2. 🔧 Services (Business Logic)

#### JsonDataService.cs
**Chức năng:** Quản lý đọc/ghi dữ liệu JSON

```csharp
public class JsonDataService
{
    private readonly string _dataPath;  // Path đến folder Data/
    
    // Lấy tất cả đơn hàng
    public async Task<List<Order>> GetOrdersAsync()
    {
        var filePath = Path.Combine(_dataPath, "orders.json");
        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<List<Order>>(json);
    }
    
    // Lưu đơn hàng
    public async Task SaveOrdersAsync(List<Order> orders)
    {
        var filePath = Path.Combine(_dataPath, "orders.json");
        var json = JsonSerializer.Serialize(orders, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }
}
```

**Tại sao cần Service?**
- Tách biệt logic khỏi Controller
- Dễ test
- Tái sử dụng được
- Dễ bảo trì

#### ShippingFeeService.cs
**Chức năng:** Tính phí giao hàng tự động

```csharp
public decimal CalculateShippingFee(CreateOrderDto orderDto)
{
    decimal baseFee = 20000;
    decimal totalFee = baseFee;
    
    // Tính theo khoảng cách
    if (orderDto.Distance <= 5)
        totalFee += 10000;
    else if (orderDto.Distance <= 10)
        totalFee += 20000;
    // ...
    
    // Tính theo trọng lượng
    if (orderDto.Weight > 5)
        totalFee += ((decimal)orderDto.Weight - 5) * 2000;
    
    // Phụ phí đặc biệt
    if (orderDto.IsFragile) totalFee += 15000;
    if (orderDto.IsValuable) totalFee += 30000;
    
    // Giao nhanh
    if (orderDto.DeliveryType == DeliveryType.GiaoHangNhanh)
        totalFee *= 1.5m;
    
    return totalFee;
}
```

**Logic tính phí:**
1. Phí cơ bản: 20,000đ
2. + Phí khoảng cách
3. + Phí trọng lượng
4. + Phí đặc biệt (vỡ, trị giá, xe)
5. × 1.5 nếu giao nhanh

---

### 3. 📦 Models (Data Structure)

Models định nghĩa cấu trúc dữ liệu.

#### Order.cs (Model chính)
```csharp
public class Order
{
    // Thông tin cơ bản
    public string OrderId { get; set; }
    public string OrderCode { get; set; }
    public DateTime CreatedDate { get; set; }
    
    // Quan hệ với Customer
    public Customer Customer { get; set; }
    
    // Thông tin hàng hóa
    public PackageType PackageType { get; set; }
    public double Weight { get; set; }
    public double Distance { get; set; }
    
    // Flags (boolean)
    public bool IsFragile { get; set; }
    public bool IsValuable { get; set; }
    public bool IsVehicle { get; set; }
    
    // Thanh toán
    public PaymentMethod PaymentMethod { get; set; }
    public decimal ShippingFee { get; set; }
    
    // Trạng thái
    public OrderStatus Status { get; set; }
    public DeliveryStaff? AssignedStaff { get; set; }
    
    // Timestamps
    public DateTime? ReceivedDate { get; set; }
    public DateTime? DeliveryStartDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    
    // Collection
    public List<LocationCheckpoint> Checkpoints { get; set; }
}
```

**Giải thích:**
- `?` = Nullable (có thể null)
- `List<T>` = Danh sách
- `enum` = Giá trị cố định (0, 1, 2...)
- `decimal` = Số thập phân cho tiền

#### Enums (Giá trị cố định)
```csharp
public enum OrderStatus
{
    ChuaNhan = 0,
    DaNhanChuaGiao = 1,
    DaNhanDangGiao = 2,
    DaGiao = 3
}
```

**Tại sao dùng enum?**
- Type-safe (không bị sai giá trị)
- Dễ đọc code
- Dễ maintain
- IntelliSense support

#### DTOs (Data Transfer Objects)
```csharp
public class CreateOrderDto
{
    // Chỉ chứa data cần thiết để tạo order
    // KHÔNG có OrderId (auto generate)
    // KHÔNG có ShippingFee (auto calculate)
    // KHÔNG có Timestamps (auto set)
}
```

**Tại sao cần DTO?**
- Tách biệt input/output
- Validation riêng
- Giảm data truyền tải
- Bảo mật (không expose hết fields)

---

### 4. ⚙️ Program.cs (Configuration)

```csharp
var builder = WebApplication.CreateBuilder(args);

// 1. Đăng ký Controllers
builder.Services.AddControllers();

// 2. Đăng ký Services (Dependency Injection)
builder.Services.AddSingleton<JsonDataService>();
builder.Services.AddSingleton<ShippingFeeService>();

// 3. Cấu hình Swagger
builder.Services.AddSwaggerGen(options => {
    options.SwaggerDoc("v1", new OpenApiInfo {
        Title = "Delivery Management API",
        Version = "v1"
    });
});

// 4. CORS
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policy => {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// 5. Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();      // API docs
    app.UseSwaggerUI();    // UI
}

app.UseHttpsRedirection();  // Force HTTPS
app.UseCors("AllowAll");    // Enable CORS
app.UseAuthorization();     // Auth (chưa dùng)
app.MapControllers();       // Map routes

app.Run();  // Start server
```

**Giải thích:**
- `AddSingleton` = 1 instance cho cả app
- `AddScoped` = 1 instance per request
- `AddTransient` = Instance mới mỗi lần inject
- Middleware = Xử lý request theo pipeline

---

## 🔄 Flow xử lý request

### Ví dụ: Tạo đơn hàng mới

```
1. Client gửi POST request
   ↓
POST /api/orders
{
  "orderCode": "WEB001",
  "customerName": "Nguyễn Văn A",
  ...
}

2. ASP.NET Core routing
   ↓
Tìm Controller: OrdersController
Tìm Method: CreateOrder

3. Model Binding
   ↓
Bind JSON → CreateOrderDto object

4. Controller nhận request
   ↓
[HttpPost]
public async Task<ActionResult<Order>> CreateOrder(CreateOrderDto dto)

5. Tính phí
   ↓
var fee = _feeService.CalculateShippingFee(dto);

6. Tạo Order object
   ↓
var order = new Order {
    OrderId = Guid.NewGuid().ToString(),
    ShippingFee = fee,
    ...
};

7. Lưu vào JSON
   ↓
await _dataService.AddOrderAsync(order);

8. Trả về response
   ↓
return CreatedAtAction(nameof(GetOrderById), 
    new { id = order.OrderId }, 
    order);

9. Client nhận response
   ↓
HTTP 201 Created
{
  "orderId": "abc-123",
  "shippingFee": 65000,
  ...
}
```

---

## 🎓 Design Patterns sử dụng

### 1. Repository Pattern (đơn giản hóa)
```csharp
JsonDataService = Repository
- GetOrdersAsync()
- AddOrderAsync()
- UpdateOrderAsync()
- DeleteOrderAsync()
```

### 2. Service Layer Pattern
```csharp
Controllers → Services → Data
Tách biệt rõ ràng từng layer
```

### 3. Dependency Injection
```csharp
// Đăng ký
builder.Services.AddSingleton<JsonDataService>();

// Inject vào constructor
public OrdersController(JsonDataService dataService)
{
    _dataService = dataService;
}
```

### 4. DTO Pattern
```csharp
CreateOrderDto    → Input
UpdateOrderStatusDto → Input
Order             → Output
```

### 5. RESTful API Pattern
```
GET    /api/orders        → List
GET    /api/orders/{id}   → Detail
POST   /api/orders        → Create
PATCH  /api/orders/{id}   → Update
DELETE /api/orders/{id}   → Delete
```

---

## 💡 Best Practices được áp dụng

### 1. ✅ Async/Await
```csharp
public async Task<List<Order>> GetOrdersAsync()
{
    // Non-blocking I/O
    var json = await File.ReadAllTextAsync(filePath);
    return JsonSerializer.Deserialize<List<Order>>(json);
}
```

**Tại sao?** Tăng performance, không block thread

### 2. ✅ Try-Catch Error Handling
```csharp
try
{
    var orders = await _dataService.GetOrdersAsync();
    return Ok(orders);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error getting orders");
    return StatusCode(500, "Lỗi server");
}
```

### 3. ✅ Nullable Reference Types
```csharp
public DeliveryStaff? AssignedStaff { get; set; }
//                   ^ Có thể null
```

### 4. ✅ XML Documentation
```csharp
/// <summary>
/// Lấy tất cả đơn hàng
/// </summary>
[HttpGet]
public async Task<ActionResult<List<Order>>> GetAllOrders()
```

→ Hiện trong Swagger UI

### 5. ✅ HTTP Status Codes
```csharp
return Ok(order);              // 200
return CreatedAtAction(...);   // 201
return NotFound();             // 404
return StatusCode(500, ...);   // 500
```

---

## 🔍 Debug & Testing

### Kiểm tra dữ liệu JSON
```bash
# Xem file orders.json
cat DeliveryManagementAPI/Data/orders.json
```

### Logs trong terminal
```
info: OrdersController[0]
      Getting all orders
```

### Test trong Swagger
```
1. Mở http://localhost:5221
2. Chọn endpoint
3. Try it out
4. Execute
5. Xem Response
```

### Test bằng .http file
```http
### Test 1: Get all orders
GET http://localhost:5221/api/orders

### Test 2: Create order
POST http://localhost:5221/api/orders
Content-Type: application/json

{ ... }
```

---

## 📊 Tóm tắt luồng dữ liệu

```
JSON File
   ↕
JsonDataService (Read/Write)
   ↕
Controllers (API Logic)
   ↕
HTTP JSON (Request/Response)
   ↕
Client (Browser/App)
```

**Đơn giản:** File JSON ↔ C# Objects ↔ JSON API

---

## 🎯 Điểm cần nhớ

1. **Controllers** = Xử lý HTTP, gọi Services
2. **Services** = Business logic, tính toán
3. **Models** = Cấu trúc dữ liệu
4. **DTOs** = Input/Output riêng biệt
5. **Dependency Injection** = Tự động inject services
6. **Async/Await** = Performance tốt hơn
7. **RESTful** = Chuẩn API design

---

**Code sạch, dễ hiểu, dễ maintain! ✨**

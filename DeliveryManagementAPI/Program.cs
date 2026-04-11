using DeliveryManagementAPI.Services;
using DeliveryManagementAPI.Services.Commands;
using DeliveryManagementAPI.Middleware;
using DeliveryManagementAPI.Hubs;
using Microsoft.Extensions.FileProviders;
using System.IO;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký DbContext với SQL Server
builder.Services.AddDbContext<DeliveryManagementAPI.DeliveryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container
builder.Services.AddControllers();

// Add SignalR
builder.Services.AddSignalR();

// Đăng ký các services
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<OrderCommandHandler>(); // Command Pattern Handler (Pattern #12)
builder.Services.AddScoped<AuditLogService>(); // Command Pattern - Audit Logging (Pattern #12)
builder.Services.AddScoped<OrderStateService>(); // State Pattern (Pattern #15)
builder.Services.AddScoped<DeliveryStaffService>();
builder.Services.AddScoped<CheckpointService>();
builder.Services.AddScoped<UserAccountService>();

// Đăng ký Notification Services với Decorator Pattern (Pattern #11)
// Stack decorators: Logging -> Retry -> Basic
builder.Services.AddScoped<INotificationSender>(sp =>
{
    var hubContext = sp.GetRequiredService<IHubContext<NotificationHub>>();
    var loggerBasic = sp.GetRequiredService<ILogger<BasicNotificationSender>>();
    var loggerRetry = sp.GetRequiredService<ILogger<RetryNotificationDecorator>>();
    var loggerLogging = sp.GetRequiredService<ILogger<LoggingNotificationDecorator>>();

    // Tạo basic sender
    INotificationSender sender = new BasicNotificationSender(hubContext);
    
    // Wrap với retry logic (3 lần thử, delay 1 giây)
    sender = new RetryNotificationDecorator(sender, loggerRetry, maxRetries: 3, delayMs: 1000);
    
    // Wrap với logging
    sender = new LoggingNotificationDecorator(sender, loggerLogging);
    
    return sender;
});

builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddSingleton<ShippingFeeService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITwoFactorService, TwoFactorService>();

// Đăng ký Payment Gateway Services với Adapter Pattern (Pattern #15)
// Adaptees: VNPayService, MomoService (các service bên thứ 3 với interface riêng)
builder.Services.AddScoped<DeliveryManagementAPI.Services.VNPay.VNPayService>();
builder.Services.AddScoped<DeliveryManagementAPI.Services.Momo.MomoService>();

// Adapters: chuyển đổi interface của bên thứ 3 sang interface chung IPaymentGateway
builder.Services.AddScoped<IPaymentGateway, VNPayAdapter>();
builder.Services.AddScoped<IPaymentGateway, MomoAdapter>();

// Client: sử dụng các gateways thông qua interface chung
builder.Services.AddScoped<PaymentGatewayService>();

// Giữ lại JsonDataService cho việc migration dữ liệu (có thể xóa sau)
builder.Services.AddSingleton<JsonDataService>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyForJWT12345678901234567890"; // Tối thiểu 32 ký tự

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "DeliveryManagementAPI",
        ValidAudience = jwtSettings["Audience"] ?? "DeliveryManagementClients",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// Configure Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
    options.AddPolicy("CustomerOnly", policy => policy.RequireRole("customer"));
    options.AddPolicy("ShipperOnly", policy => policy.RequireRole("shipper"));
    options.AddPolicy("AdminOrShipper", policy => policy.RequireRole("admin", "shipper"));
});

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Delivery Management API",
        Version = "v1",
        Description = "API quản lý hệ thống giao hàng - Case Study 14",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Delivery Management System"
        }
    });
    
    // Thêm JWT Authentication vào Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n" +
                      "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                      "Example: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    
    // Thêm comment XML vào Swagger
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Seed data khi chạy lần đầu
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // ✅ ENABLED: Seed data được tải
        await DeliveryManagementAPI.Data.SeedData.Initialize(services);
        Console.WriteLine("✅ Seed data đã được tải thành công");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Delivery Management API v1");
        c.RoutePrefix = "swagger"; // move Swagger UI to /swagger to avoid root conflicts with the static UI
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

// Add Rate Limiting Middleware
app.UseRateLimiting();

app.UseAuthentication();
app.UseAuthorization();

// Serve the DeliveryManagementUI static files from the repository root (sibling folder) so the UI and API share the same origin.
// The DeliveryManagementUI folder sits next to DeliveryManagementAPI, so go up one level from the API content root.
var uiPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "DeliveryManagementUI"));
// Fallback: if the sibling path doesn't exist, also check a DeliveryManagementUI folder inside the content root (for alternate layouts).
if (!Directory.Exists(uiPath))
{
    uiPath = Path.Combine(builder.Environment.ContentRootPath, "DeliveryManagementUI");
}

if (Directory.Exists(uiPath))
{
    // If root requested, redirect to /Home/home.html so visiting '/' opens the Home page explicitly
    app.Use(async (context, next) =>
    {
        var p = context.Request.Path.Value;
        if (string.Equals(p, "/", StringComparison.OrdinalIgnoreCase) || string.Equals(p, "/index.html", StringComparison.OrdinalIgnoreCase))
        {
            // Ensure redirect responses are not cached by the browser
            context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            context.Response.Headers["Pragma"] = "no-cache";
            context.Response.Headers["Expires"] = "0";
            context.Response.Redirect("/Home/home.html", false);
            return;
        }
        await next();
    });

    // Enable default wwwroot static files (for uploaded images)
    app.UseStaticFiles();

    // Serve default files: prefer Home/home.html as the default root document, then index.html
    app.UseDefaultFiles(new DefaultFilesOptions
    {
        FileProvider = new PhysicalFileProvider(uiPath),
        RequestPath = "",
        DefaultFileNames = new List<string> { "Home/home.html", "index.html" }
    });

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uiPath),
        RequestPath = "",
        OnPrepareResponse = ctx =>
        {
            // Prevent caching of static files in development so clients always fetch latest changes
            ctx.Context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            ctx.Context.Response.Headers["Pragma"] = "no-cache";
            ctx.Context.Response.Headers["Expires"] = "0";
        }
    });
}

app.MapControllers();

// Map SignalR Hubs
app.MapHub<DeliveryManagementAPI.Hubs.ChatHub>("/chatHub");
app.MapHub<DeliveryManagementAPI.Hubs.TrackingHub>("/trackingHub");
app.MapHub<DeliveryManagementAPI.Hubs.NotificationHub>("/notificationHub");

app.Run();

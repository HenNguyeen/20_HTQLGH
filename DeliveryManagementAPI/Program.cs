using DeliveryManagementAPI.Services;
using Microsoft.Extensions.FileProviders;
using System.IO;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký DbContext với SQL Server
builder.Services.AddDbContext<DeliveryManagementAPI.DeliveryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container
builder.Services.AddControllers();

// Đăng ký các services
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<DeliveryStaffService>();
builder.Services.AddScoped<CheckpointService>();
builder.Services.AddScoped<UserAccountService>();
builder.Services.AddSingleton<ShippingFeeService>();

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
        await DeliveryManagementAPI.Data.SeedData.Initialize(services);
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

app.Run();

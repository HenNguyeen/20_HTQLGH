using System.Collections.Concurrent;

namespace DeliveryManagementAPI.Middleware
{
    /// <summary>
    /// Middleware để giới hạn tỉ lệ yêu cầu (Rate Limiting) cho các API endpoints
    /// Đặc biệt là để bảo vệ các endpoint như registration, login, password reset
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        
        // Dictionary để theo dõi số yêu cầu từ mỗi IP address
        // Key: IP address, Value: tuple của (RequestCount, ResetTime)
        private static readonly ConcurrentDictionary<string, (int count, DateTime resetTime)> RequestCounts = 
            new();

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip rate limiting for health check and swagger endpoints
            if (context.Request.Path.StartsWithSegments("/health") ||
                context.Request.Path.StartsWithSegments("/swagger") ||
                context.Request.Path.StartsWithSegments("/api-docs"))
            {
                await _next(context);
                return;
            }

            var endpoint = context.Request.Path.ToString();
            var method = context.Request.Method;
            
            // Apply rate limiting to specific sensitive endpoints
            if (IsRateLimitedEndpoint(endpoint, method))
            {
                if (!CheckRateLimit(context))
                {
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.Response.ContentType = "application/json";
                    
                    var errorResponse = new 
                    { 
                        message = "Quá nhiều yêu cầu. Vui lòng thử lại sau 5 phút.",
                        retryAfter = 300
                    };
                    
                    await context.Response.WriteAsJsonAsync(errorResponse);
                    _logger.LogWarning($"Rate limit exceeded for IP: {GetClientIp(context)} on endpoint: {endpoint}");
                    return;
                }
            }

            await _next(context);
        }

        /// <summary>
        /// Kiểm tra xem endpoint có cần áp dụng rate limiting hay không
        /// </summary>
        private bool IsRateLimitedEndpoint(string endpoint, string method)
        {
            // Các endpoint nhạy cảm cần rate limiting
            var rateLimitedEndpoints = new[]
            {
                "/api/auth/register",
                "/api/auth/login",
                "/api/auth/forgot-password",
                "/api/auth/reset-password",
                "/api/auth/verify-2fa"
            };

            if (rateLimitedEndpoints.Any(e => endpoint.StartsWith(e, StringComparison.OrdinalIgnoreCase)))
                return true;

            // Chỉ rate limit POST /api/orders (tạo đơn), không rate limit GET
            if (endpoint.StartsWith("/api/orders", StringComparison.OrdinalIgnoreCase))
            {
                return method.Equals("POST", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        /// <summary>
        /// Kiểm tra xem request có vượt quá giới hạn hay không
        /// </summary>
        private bool CheckRateLimit(HttpContext context)
        {
            var clientIp = GetClientIp(context);
            var endpoint = context.Request.Path.ToString();
            
            // Tạo key duy nhất từ IP và endpoint
            var key = $"{clientIp}@{endpoint.ToLower()}";

            // Clean up old entries (nếu đã hết thời gian reset)
            if (RequestCounts.TryGetValue(key, out var currentData))
            {
                if (DateTime.UtcNow > currentData.resetTime)
                {
                    // Thời gian reset đã qua, xóa entry này
                    RequestCounts.TryRemove(key, out _);
                }
            }

            // Cập nhật số yêu cầu
            var now = DateTime.UtcNow;
            var resetTime = now.AddMinutes(5); // Reset sau 5 phút

            var result = RequestCounts.AddOrUpdate(key, 
                (1, resetTime), 
                (k, v) => (v.resetTime > now ? v.count + 1 : 1, v.resetTime > now ? v.resetTime : resetTime)
            );

            // Define rate limits for different endpoints
            var limit = GetRateLimitForEndpoint(endpoint);
            
            if (result.count > limit)
            {
                return false; // Exceeded limit
            }

            return true; // Within limit
        }

        /// <summary>
        /// Lấy giới hạn rate limit cho endpoint cụ thể
        /// </summary>
        private int GetRateLimitForEndpoint(string endpoint)
        {
            endpoint = endpoint.ToLower();

            // Các endpoint khác nhau có các giới hạn khác nhau
            if (endpoint == "/api/orders" || endpoint.StartsWith("/api/orders"))
            {
                // Chi kiểm tra POST /api/orders (tạo đơn)
                // GET requests không cần rate limiting
                if (endpoint == "/api/orders")
                    return 20; // 20 đơn hàng mỗi 15 phút
            }
            
            if (endpoint.StartsWith("/api/auth/register"))
                return 5; // 5 yêu cầu mỗi 5 phút
            
            if (endpoint.StartsWith("/api/auth/login"))
                return 30; // 30 yêu cầu mỗi 5 phút
            
            if (endpoint.StartsWith("/api/auth/forgot-password") || 
                endpoint.StartsWith("/api/auth/reset-password"))
                return 5; // 5 yêu cầu mỗi 5 phút
            
            if (endpoint.StartsWith("/api/auth/verify-2fa"))
                return 20; // 20 yêu cầu mỗi 5 phút

            return 100; // Default limit
        }

        /// <summary>
        /// Lấy địa chỉ IP của client
        /// </summary>
        private string GetClientIp(HttpContext context)
        {
            // Kiểm tra X-Forwarded-For header (nếu đằng sau proxy)
            if (context.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                var forwardedFor = context.Request.Headers["X-Forwarded-For"].ToString().Split(',').First().Trim();
                return forwardedFor;
            }

            // Kiểm tra X-Real-IP header
            if (context.Request.Headers.ContainsKey("X-Real-IP"))
            {
                return context.Request.Headers["X-Real-IP"].ToString();
            }

            // Lấy remote IP address
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }

    /// <summary>
    /// Extension method để dễ dàng thêm middleware vào pipeline
    /// </summary>
    public static class RateLimitingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitingMiddleware>();
        }
    }
}

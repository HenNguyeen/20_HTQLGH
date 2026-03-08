using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace DeliveryManagementAPI.Hubs
{
    /// <summary>
    /// SignalR Hub cho real-time notifications
    /// </summary>
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Khi user kết nối
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (!string.IsNullOrEmpty(userId))
            {
                // Thêm connection vào group của user
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                
                _logger.LogInformation($"User {userId} connected to NotificationHub with connection {Context.ConnectionId}");
            }
            else
            {
                _logger.LogWarning($"Anonymous user connected to NotificationHub with connection {Context.ConnectionId}");
            }

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Khi user ngắt kết nối
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
                
                _logger.LogInformation($"User {userId} disconnected from NotificationHub");
            }

            if (exception != null)
            {
                _logger.LogError(exception, "Error during NotificationHub disconnection");
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Client yêu cầu số lượng thông báo chưa đọc
        /// </summary>
        public async Task RequestUnreadCount()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (!string.IsNullOrEmpty(userId))
            {
                // Gọi lại client với unread count
                // Service sẽ gửi qua Clients.User() từ bên ngoài
                await Clients.Caller.SendAsync("UnreadCountRequested");
            }
        }

        /// <summary>
        /// Test connection
        /// </summary>
        public async Task Ping()
        {
            await Clients.Caller.SendAsync("Pong", DateTime.UtcNow);
        }
    }
}

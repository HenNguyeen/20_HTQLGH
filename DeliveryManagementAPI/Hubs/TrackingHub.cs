using Microsoft.AspNetCore.SignalR;

namespace DeliveryManagementAPI.Hubs
{
    /// <summary>
    /// SignalR Hub để tracking vị trí shipper realtime
    /// </summary>
    public class TrackingHub : Hub
    {
        /// <summary>
        /// Shipper gửi vị trí của mình lên server
        /// Server sẽ broadcast cho các client đang theo dõi đơn hàng
        /// </summary>
        public async Task UpdateShipperLocation(int staffId, int orderId, double latitude, double longitude)
        {
            // Broadcast vị trí mới của shipper cho tất cả client đang theo dõi đơn hàng này
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
        /// Client tham gia nhóm theo dõi một đơn hàng cụ thể
        /// </summary>
        public async Task JoinOrderTracking(string orderId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"order_{orderId}");
        }

        /// <summary>
        /// Client rời khỏi nhóm theo dõi đơn hàng
        /// </summary>
        public async Task LeaveOrderTracking(string orderId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order_{orderId}");
        }

        /// <summary>
        /// Khi client kết nối
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Khi client ngắt kết nối
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}

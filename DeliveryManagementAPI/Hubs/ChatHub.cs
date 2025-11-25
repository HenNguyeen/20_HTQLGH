using Microsoft.AspNetCore.SignalR;
using DeliveryManagementAPI.Models;

namespace DeliveryManagementAPI.Hubs
{
    public class ChatHub : Hub
    {
        // Gửi tin nhắn đến tất cả người dùng trong nhóm đơn hàng
        public async Task SendMessageToOrder(int orderId, string senderName, string senderRole, string message, string? imageUrl = null)
        {
            var chatMessage = new
            {
                OrderId = orderId,
                SenderName = senderName,
                SenderRole = senderRole,
                Content = message,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.Now
            };

            // Gửi tin nhắn đến tất cả clients trong nhóm đơn hàng này
            await Clients.Group($"Order_{orderId}").SendAsync("ReceiveMessage", chatMessage);
        }

        // Join vào nhóm chat của đơn hàng
        public async Task JoinOrderChat(int orderId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Order_{orderId}");
            await Clients.Group($"Order_{orderId}").SendAsync("UserJoined", $"User joined order {orderId} chat");
        }

        // Leave nhóm chat
        public async Task LeaveOrderChat(int orderId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Order_{orderId}");
            await Clients.Group($"Order_{orderId}").SendAsync("UserLeft", $"User left order {orderId} chat");
        }

        // Thông báo đang gõ
        public async Task NotifyTyping(int orderId, string userName)
        {
            await Clients.OthersInGroup($"Order_{orderId}").SendAsync("UserTyping", userName);
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using DeliveryManagementAPI.Models;
using Microsoft.AspNetCore.SignalR;
using DeliveryManagementAPI.Hubs;
using System.Security.Claims;

namespace DeliveryManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly DeliveryDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IWebHostEnvironment _env;

        public ChatController(DeliveryDbContext context, IHubContext<ChatHub> hubContext, IWebHostEnvironment env)
        {
            _context = context;
            _hubContext = hubContext;
            _env = env;
        }

        // GET: api/chat/conversations - Lấy danh sách người đã chat (cho admin)
        [HttpGet("conversations")]
        public async Task<ActionResult<IEnumerable<object>>> GetConversations()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("User ID not found in token");
            }

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "customer";

            // Chỉ admin mới có thể xem tất cả conversations
            if (userRole != "admin")
            {
                return Forbid();
            }

            // Lấy tất cả user (không phải admin) đã tham gia chat
            var userIds = await _context.ChatMessages
                .Where(m => m.OrderId == null && m.SenderRole != "admin")
                .Select(m => m.SenderId)
                .Distinct()
                .ToListAsync();

            var conversations = new List<object>();

            foreach (var targetUserId in userIds)
            {
                var lastMessage = await _context.ChatMessages
                    .Where(m => m.OrderId == null && 
                               (m.SenderId == targetUserId || 
                                (m.SenderId == userId && m.SenderRole == "admin")))
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => new
                    {
                        Content = m.Content,
                        ImageUrl = m.ImageUrl,
                        CreatedAt = m.CreatedAt
                    })
                    .FirstOrDefaultAsync();

                var user = await _context.UserAccounts.FindAsync(targetUserId);

                if (user != null && lastMessage != null)
                {
                    var unreadCount = await _context.ChatMessages
                        .CountAsync(m => m.OrderId == null && 
                                        m.SenderId == targetUserId && 
                                        m.SenderRole != "admin");

                    conversations.Add(new
                    {
                        UserId = targetUserId,
                        UserName = user.FullName,
                        LastMessage = lastMessage,
                        UnreadCount = unreadCount
                    });
                }
            }

            return Ok(conversations.OrderByDescending(c => ((dynamic)c).LastMessage.CreatedAt));
        }

        // GET: api/chat/my-messages - Lấy tin nhắn general support của user hiện tại
        [HttpGet("my-messages")]
        public async Task<ActionResult<IEnumerable<object>>> GetMyMessages()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("User ID not found in token");
            }

            // Lấy tất cả tin nhắn general support
            var allMessages = await _context.ChatMessages
                .Where(m => m.OrderId == null)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new
                {
                    m.Id,
                    m.OrderId,
                    m.SenderId,
                    m.SenderRole,
                    SenderName = m.Sender != null ? m.Sender.FullName : "Unknown",
                    m.Content,
                    m.ImageUrl,
                    m.CreatedAt
                })
                .ToListAsync();

            // Filter client-side: chỉ lấy tin nhắn của user này và admin reply cho user này
            var userHasSentMessage = allMessages.Any(m => m.SenderId == userId);
            
            var messages = allMessages.Where(m => 
                m.SenderId == userId || // Tin nhắn của chính user
                (m.SenderRole == "admin" && userHasSentMessage) // Admin reply (nếu user đã gửi tin nhắn)
            ).ToList();

            return Ok(messages);
        }

        // GET: api/chat/user/{userId} - Lấy tin nhắn với user cụ thể (cho admin)
        [HttpGet("user/{targetUserId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetMessagesByUser(int targetUserId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("User ID not found in token");
            }

            var messages = await _context.ChatMessages
                .Where(m => m.OrderId == null && 
                           (m.SenderId == userId || m.SenderId == targetUserId))
                .OrderBy(m => m.CreatedAt)
                .Select(m => new
                {
                    m.Id,
                    m.OrderId,
                    m.SenderId,
                    m.SenderRole,
                    SenderName = m.Sender != null ? m.Sender.FullName : "Unknown",
                    m.Content,
                    m.ImageUrl,
                    m.CreatedAt
                })
                .ToListAsync();

            return Ok(messages);
        }

        // GET: api/chat/order/{orderId}
        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetMessagesByOrder(int orderId)
        {
            var messages = await _context.ChatMessages
                .Where(m => orderId == 0 ? m.OrderId == null : m.OrderId == orderId)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new
                {
                    m.Id,
                    m.OrderId,
                    m.SenderId,
                    m.SenderRole,
                    SenderName = m.Sender != null ? m.Sender.FullName : "Unknown",
                    m.Content,
                    m.ImageUrl,
                    m.CreatedAt
                })
                .ToListAsync();

            return Ok(messages);
        }

        // POST: api/chat/send
        [HttpPost("send")]
        public async Task<ActionResult<ChatMessage>> SendMessage([FromBody] SendMessageDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("User ID not found in token");
            }

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "customer";
            var user = await _context.UserAccounts.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var message = new ChatMessage
            {
                OrderId = dto.OrderId,
                SenderId = userId,
                SenderRole = userRole,
                Content = dto.Content,
                ImageUrl = dto.ImageUrl,
                CreatedAt = DateTime.Now
            };

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            // Gửi tin nhắn real-time qua SignalR
            var groupName = dto.OrderId.HasValue ? $"Order_{dto.OrderId.Value}" : "Order_0";
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveMessage", new
            {
                message.Id,
                message.OrderId,
                message.SenderId,
                message.SenderRole,
                SenderName = user.FullName,
                message.Content,
                message.ImageUrl,
                message.CreatedAt
            });

            return Ok(message);
        }

        // POST: api/chat/upload
        [HttpPost("upload")]
        public async Task<ActionResult<string>> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            // Kiểm tra loại file
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest("Invalid file type. Only images are allowed.");
            }

            // Kiểm tra kích thước (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest("File size exceeds 5MB limit");
            }

            try
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "chat");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                var fileUrl = $"/uploads/chat/{uniqueFileName}";
                return Ok(new { url = fileUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error uploading file: {ex.Message}");
            }
        }

        // GET: api/chat/orders - Lấy danh sách đơn hàng có quyền chat
        [HttpGet("orders")]
        public async Task<ActionResult<IEnumerable<object>>> GetChatableOrders()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("User ID not found in token");
            }

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "customer";

            IQueryable<Order> ordersQuery = _context.Orders;

            // Filter theo role
            if (userRole == "customer")
            {
                ordersQuery = ordersQuery.Where(o => o.CreatedByUserId == userId);
            }
            else if (userRole == "shipper")
            {
                ordersQuery = ordersQuery.Where(o => o.AssignedStaffId == userId.ToString());
            }
            // Admin có thể xem tất cả

            var orders = await ordersQuery
                .OrderByDescending(o => o.CreatedDate)
                .Select(o => new
                {
                    Id = o.OrderId,
                    o.OrderCode,
                    o.Status,
                    CustomerName = o.Customer.FullName,
                    CustomerPhone = o.Customer.PhoneNumber,
                    LastMessage = _context.ChatMessages
                        .Where(m => m.OrderId == o.OrderId)
                        .OrderByDescending(m => m.CreatedAt)
                        .Select(m => new { m.Content, m.CreatedAt })
                        .FirstOrDefault(),
                    UnreadCount = _context.ChatMessages
                        .Count(m => m.OrderId == o.OrderId && m.SenderId != userId)
                })
                .ToListAsync();

            return Ok(orders);
        }
    }

    public class SendMessageDto
    {
        public int? OrderId { get; set; }
        public string? Content { get; set; }
        public string? ImageUrl { get; set; }
    }
}

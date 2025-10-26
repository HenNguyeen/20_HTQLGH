using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DeliveryManagementAPI.Models;
using DeliveryManagementAPI;
using System.Security.Claims;

namespace DeliveryManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FeedbackController : ControllerBase
    {
        private readonly DeliveryDbContext _context;
        private readonly ILogger<FeedbackController> _logger;

        public FeedbackController(DeliveryDbContext context, ILogger<FeedbackController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Gửi feedback cho đơn hàng (chỉ owner)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult<Feedback>> PostFeedback([FromBody] Feedback feedback)
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                    return Unauthorized(new { message = "Không xác định được người dùng từ token" });

                // Kiểm tra đơn hàng thuộc về user
                var order = await _context.Orders.FindAsync(feedback.OrderId);
                if (order == null || order.CreatedByUserId != userId)
                    return Forbid("Bạn chỉ có thể đánh giá đơn của mình");

                feedback.UserId = userId;
                feedback.CreatedAt = DateTime.Now;
                _context.Feedbacks.Add(feedback);
                await _context.SaveChangesAsync();
                return Ok(feedback);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error posting feedback");
                return StatusCode(500, "Lỗi khi gửi phản hồi");
            }
        }

        /// <summary>
        /// Lấy feedback của 1 đơn hàng
        /// </summary>
        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<List<Feedback>>> GetFeedbacksByOrder(int orderId)
        {
            try
            {
                var list = _context.Feedbacks.Where(f => f.OrderId == orderId).ToList();
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting feedbacks by order");
                return StatusCode(500, "Lỗi khi lấy phản hồi");
            }
        }

        /// <summary>
        /// Lấy tất cả feedback của user hiện tại
        /// </summary>
        [HttpGet("my")]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult<List<Feedback>>> GetMyFeedbacks()
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                    return Unauthorized(new { message = "Không xác định được người dùng từ token" });
                var list = _context.Feedbacks.Where(f => f.UserId == userId).ToList();
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting my feedbacks");
                return StatusCode(500, "Lỗi khi lấy phản hồi của tôi");
            }
        }
    }
}

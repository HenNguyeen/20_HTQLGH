using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        /// <summary>
        /// Lấy rating trung bình của shipper theo staffId
        /// </summary>
        [HttpGet("staff/{staffId}/rating")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetStaffRating(int staffId)
        {
            try
            {
                // Lấy tất cả đơn hàng của shipper này
                var orderIds = _context.Orders
                    .Where(o => o.AssignedStaffId == staffId.ToString())
                    .Select(o => o.OrderId)
                    .ToList();

                if (!orderIds.Any())
                {
                    return Ok(new { staffId, averageRating = 0.0, totalFeedbacks = 0, feedbacks = new List<Feedback>() });
                }

                // Lấy tất cả feedback của các đơn này
                var feedbacks = _context.Feedbacks
                    .Where(f => orderIds.Contains(f.OrderId))
                    .ToList();

                if (!feedbacks.Any())
                {
                    return Ok(new { staffId, averageRating = 0.0, totalFeedbacks = 0, feedbacks = new List<Feedback>() });
                }

                var averageRating = feedbacks.Average(f => f.Rating);
                var totalFeedbacks = feedbacks.Count;

                return Ok(new 
                { 
                    staffId, 
                    averageRating = Math.Round(averageRating, 2), 
                    totalFeedbacks,
                    feedbacks = feedbacks.OrderByDescending(f => f.CreatedAt).Take(10)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting staff rating");
                return StatusCode(500, "Lỗi khi lấy đánh giá nhân viên");
            }
        }

        /// <summary>
        /// Lấy tất cả feedback (Admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<List<object>>> GetAllFeedbacks()
        {
            try
            {
                var feedbacks = _context.Feedbacks
                    .OrderByDescending(f => f.CreatedAt)
                    .Select(f => new
                    {
                        f.FeedbackId,
                        f.OrderId,
                        f.UserId,
                        f.Rating,
                        f.Comment,
                        f.CreatedAt,
                        Order = _context.Orders
                            .Where(o => o.OrderId == f.OrderId)
                            .Select(o => new { o.OrderCode, o.AssignedStaffId })
                            .FirstOrDefault()
                    })
                    .ToList();

                return Ok(feedbacks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all feedbacks");
                return StatusCode(500, "Lỗi khi lấy tất cả phản hồi");
            }
        }

        /// <summary>
        /// Shipper lấy feedback của chính mình
        /// </summary>
        [HttpGet("my-ratings")]
        [Authorize(Roles = "shipper,admin")]
        public async Task<ActionResult<object>> GetMyRatings()
        {
            try
            {
                // Lấy thông tin staff từ token
                var fullName = User.Claims.FirstOrDefault(c => c.Type == "FullName")?.Value
                               ?? User.Claims.FirstOrDefault(c => c.Type.Contains("/name"))?.Value;

                if (string.IsNullOrEmpty(fullName))
                {
                    return Unauthorized(new { message = "Không xác định được thông tin shipper" });
                }

                // Tìm staff record
                var staff = await _context.DeliveryStaffs
                    .FirstOrDefaultAsync(s => s.FullName == fullName);

                if (staff == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin nhân viên" });
                }

                // Lấy tất cả đơn hàng của shipper
                var orderIds = await _context.Orders
                    .Where(o => o.AssignedStaffId == staff.StaffId.ToString())
                    .Select(o => o.OrderId)
                    .ToListAsync();

                if (!orderIds.Any())
                {
                    return Ok(new 
                    { 
                        staffId = staff.StaffId,
                        staffName = staff.FullName,
                        averageRating = 0.0, 
                        totalFeedbacks = 0, 
                        feedbacks = new List<object>() 
                    });
                }

                // Lấy tất cả feedback
                var feedbacks = await _context.Feedbacks
                    .Where(f => orderIds.Contains(f.OrderId))
                    .OrderByDescending(f => f.CreatedAt)
                    .Select(f => new
                    {
                        f.FeedbackId,
                        f.OrderId,
                        f.Rating,
                        f.Comment,
                        f.CreatedAt,
                        OrderCode = _context.Orders
                            .Where(o => o.OrderId == f.OrderId)
                            .Select(o => o.OrderCode)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                var averageRating = feedbacks.Any() ? feedbacks.Average(f => f.Rating) : 0;

                return Ok(new
                {
                    staffId = staff.StaffId,
                    staffName = staff.FullName,
                    averageRating = Math.Round(averageRating, 2),
                    totalFeedbacks = feedbacks.Count,
                    feedbacks
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting my ratings");
                return StatusCode(500, "Lỗi khi lấy đánh giá của tôi");
            }
        }
    }
}

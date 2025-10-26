using Microsoft.AspNetCore.Mvc;
using DeliveryManagementAPI.Models;
using DeliveryManagementAPI.Services;
using Microsoft.AspNetCore.Authorization;

namespace DeliveryManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrackingController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly CheckpointService _checkpointService;
        private readonly ILogger<TrackingController> _logger;

        public TrackingController(
            OrderService orderService,
            CheckpointService checkpointService,
            ILogger<TrackingController> logger)
        {
            _orderService = orderService;
            _checkpointService = checkpointService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy lịch sử check-in của đơn hàng
        /// </summary>
        [HttpGet("order/{orderId}")]
        [Authorize] // Tất cả user đã đăng nhập có thể xem
        public async Task<ActionResult<List<LocationCheckpoint>>> GetOrderCheckpoints(int orderId)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);
                
                if (order == null)
                {
                    return NotFound($"Không tìm thấy đơn hàng với ID: {orderId}");
                }

                var checkpoints = await _checkpointService.GetCheckpointsByOrderIdAsync(orderId);
                return Ok(checkpoints);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting checkpoints for order: {OrderId}", orderId);
                return StatusCode(500, "Lỗi khi lấy thông tin theo dõi");
            }
        }

        /// <summary>
        /// Check-in vị trí khi giao hàng
        /// </summary>
        [HttpPost("checkin")]
        [Authorize(Roles = "admin,shipper")] // Chỉ admin và shipper được check-in
        public async Task<ActionResult<LocationCheckpoint>> CheckIn(
            [FromBody] LocationCheckpoint checkpoint)
        {
            try
            {
                // Kiểm tra đơn hàng tồn tại
                var order = await _orderService.GetOrderByIdAsync(checkpoint.OrderId);
                
                if (order == null)
                {
                    return NotFound($"Không tìm thấy đơn hàng với ID: {checkpoint.OrderId}");
                }

                var savedCheckpoint = await _checkpointService.AddCheckpointAsync(checkpoint);

                return Ok(savedCheckpoint);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating checkpoint");
                return StatusCode(500, "Lỗi khi check-in vị trí");
            }
        }

        /// <summary>
        /// Theo dõi đơn hàng theo mã đơn hàng
        /// </summary>
        [HttpGet("track/{orderCode}")]
        [AllowAnonymous] // Cho phép tracking không cần đăng nhập
        public async Task<ActionResult<object>> TrackByOrderCode(string orderCode)
        {
            try
            {
                var order = await _orderService.GetOrderByCodeAsync(orderCode);
                
                if (order == null)
                {
                    return NotFound($"Không tìm thấy đơn hàng với mã: {orderCode}");
                }

                var checkpoints = await _checkpointService.GetCheckpointsByOrderIdAsync(order.OrderId);

                var trackingInfo = new
                {
                    Order = order,
                    Checkpoints = checkpoints.OrderBy(c => c.CheckInTime).ToList(),
                    CurrentStatus = order.Status.ToString(),
                    LastUpdate = checkpoints.Any() 
                        ? checkpoints.Max(c => c.CheckInTime)
                        : order.CreatedDate
                };

                return Ok(trackingInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking order: {OrderCode}", orderCode);
                return StatusCode(500, "Lỗi khi theo dõi đơn hàng");
            }
        }

        /// <summary>
        /// Lấy vị trí hiện tại của đơn hàng
        /// </summary>
        [HttpGet("location/{orderId}")]
        [Authorize] // Tất cả user đã đăng nhập có thể xem
        public async Task<ActionResult<LocationCheckpoint>> GetCurrentLocation(int orderId)
        {
            try
            {
                var latestCheckpoint = await _checkpointService.GetLatestCheckpointAsync(orderId);
                
                if (latestCheckpoint == null)
                {
                    return NotFound("Chưa có thông tin vị trí cho đơn hàng này");
                }

                return Ok(latestCheckpoint);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current location: {OrderId}", orderId);
                return StatusCode(500, "Lỗi khi lấy vị trí hiện tại");
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeliveryManagementAPI.Models;
using System.Text.Json;

namespace DeliveryManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatbotController : ControllerBase
    {
        private readonly DeliveryDbContext _context;
        private readonly ILogger<ChatbotController> _logger;

        public ChatbotController(DeliveryDbContext context, ILogger<ChatbotController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook([FromBody] DialogflowRequest request)
        {
            try
            {
                _logger.LogInformation($"Received intent: {request.QueryResult?.Intent?.DisplayName}");

                var intentName = request.QueryResult?.Intent?.DisplayName;
                var parameters = request.QueryResult?.Parameters;

                string responseText;

                switch (intentName)
                {
                    case "TraCuuDonHang":
                        responseText = await HandleTraCuuDonHang(parameters);
                        break;

                    case "KiemTraTrangThaiGiaoHang":
                        responseText = await HandleKiemTraTrangThai(parameters);
                        break;

                    case "KiemTraShipper":
                        responseText = await HandleKiemTraShipper(parameters);
                        break;

                    default:
                        responseText = "Xin lỗi, tôi chưa hiểu yêu cầu của bạn. Bạn có thể thử hỏi lại không?";
                        break;
                }

                var response = new DialogflowResponse
                {
                    FulfillmentText = responseText
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Webhook error: {ex.Message}");
                return Ok(new DialogflowResponse
                {
                    FulfillmentText = "Xin lỗi, đã có lỗi xảy ra. Vui lòng thử lại sau."
                });
            }
        }

        private async Task<string> HandleTraCuuDonHang(Dictionary<string, JsonElement>? parameters)
        {
            try
            {
                if (parameters == null || !parameters.ContainsKey("order-id"))
                {
                    return "Vui lòng cung cấp mã đơn hàng để tôi tra cứu cho bạn. Ví dụ: 'tra cứu đơn 1' hoặc 'xem đơn DH001'.";
                }

                // Lấy giá trị order-id và xử lý
                var orderParam = parameters["order-id"];
                string orderCodeOrId = string.Empty;

                // Xử lý nhiều kiểu dữ liệu từ Dialogflow
                if (orderParam.ValueKind == JsonValueKind.Number)
                {
                    orderCodeOrId = orderParam.GetInt32().ToString();
                }
                else if (orderParam.ValueKind == JsonValueKind.String)
                {
                    orderCodeOrId = orderParam.GetString()?.Trim() ?? string.Empty;
                }
                else
                {
                    orderCodeOrId = orderParam.ToString().Trim();
                }

                if (string.IsNullOrWhiteSpace(orderCodeOrId))
                {
                    return "Không thể nhận diện mã đơn hàng. Vui lòng thử lại!";
                }

                _logger.LogInformation($"Searching for order: {orderCodeOrId}");

                // Thử tìm theo OrderCode trước (DH001, DH002...)
                var order = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.AssignedStaff)
                    .FirstOrDefaultAsync(o => o.OrderCode == orderCodeOrId);
                
                // Nếu không tìm thấy, thử tìm theo OrderId (số)
                if (order == null && int.TryParse(orderCodeOrId, out int orderId))
                {
                    order = await _context.Orders
                        .Include(o => o.Customer)
                        .Include(o => o.AssignedStaff)
                        .FirstOrDefaultAsync(o => o.OrderId == orderId);
                }

                if (order == null)
                {
                    return $"❌ Không tìm thấy đơn hàng với mã '{orderCodeOrId}'.\n\n" +
                           $"💡 Gợi ý: Vui lòng kiểm tra lại mã đơn hàng hoặc thử tra cứu bằng số (ví dụ: 1, 2, 3...).";
                }

                var statusText = GetStatusText(order.Status);
                var statusIcon = GetStatusIcon(order.Status);
                
                var response = $"✅ Đã tìm thấy đơn hàng!\n\n";
                response += $"📦 MÃ ĐƠN: #{order.OrderId} ({order.OrderCode})\n";
                response += $"━━━━━━━━━━━━━━━━━━━━\n\n";
                response += $"👤 Khách hàng: {order.Customer?.FullName ?? "N/A"}\n";
                response += $"📍 Địa chỉ: {order.Customer?.Address ?? "N/A"}\n";
                response += $"📞 SĐT: {order.Customer?.PhoneNumber ?? "N/A"}\n";
                response += $"💰 Phí ship: {order.ShippingFee:N0}đ\n\n";
                response += $"{statusIcon} Trạng thái: {statusText}\n";

                if (order.AssignedStaff != null)
                {
                    response += $"\n🚚 Shipper phụ trách:\n";
                    response += $"   • Tên: {order.AssignedStaff.FullName}\n";
                    response += $"   • SĐT: {order.AssignedStaff.PhoneNumber}";
                }
                else
                {
                    response += $"\n⏳ Đơn hàng chưa được giao cho shipper.";
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HandleTraCuuDonHang");
                return "❌ Đã xảy ra lỗi khi tra cứu đơn hàng. Vui lòng thử lại sau!";
            }
        }

        private async Task<string> HandleKiemTraTrangThai(Dictionary<string, JsonElement>? parameters)
        {
            if (parameters == null || !parameters.ContainsKey("order-id"))
            {
                return "Vui lòng cung cấp mã đơn hàng để tôi kiểm tra trạng thái.";
            }

            var orderCodeOrId = parameters["order-id"].ToString().Trim();
            
            // Thử tìm theo OrderCode trước
            var order = await _context.Orders
                .Include(o => o.AssignedStaff)
                .FirstOrDefaultAsync(o => o.OrderCode == orderCodeOrId);
            
            // Nếu không tìm thấy, thử tìm theo OrderId
            if (order == null && int.TryParse(orderCodeOrId, out int orderId))
            {
                order = await _context.Orders
                    .Include(o => o.AssignedStaff)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);
            }

            if (order == null)
            {
                return $"❌ Không tìm thấy đơn hàng '{orderCodeOrId}'.";
            }

            var statusText = GetStatusText(order.Status);
            var response = $"📦 Đơn hàng #{order.OrderId}\n\n";
            response += $"📊 Trạng thái hiện tại: **{statusText}**\n\n";

            switch (order.Status)
            {
                case OrderStatus.ChuaNhan:
                    response += "⏳ Đơn hàng đang chờ xử lý. Chúng tôi sẽ sớm giao cho shipper.";
                    break;
                case OrderStatus.DaNhanChuaGiao:
                    response += "✅ Đơn hàng đã được xác nhận và đang chờ shipper lấy hàng.";
                    break;
                case OrderStatus.DaNhanDangGiao:
                    response += $"🚚 Đơn hàng đang được giao bởi shipper {order.AssignedStaff?.FullName ?? "N/A"}.\n";
                    response += $"📱 SĐT shipper: {order.AssignedStaff?.PhoneNumber ?? "N/A"}";
                    break;
                case OrderStatus.DaGiao:
                    response += "✅ Đơn hàng đã được giao thành công!";
                    break;
                default:
                    response += "Trạng thái đơn hàng đang được cập nhật.";
                    break;
            }

            return response;
        }

        private async Task<string> HandleKiemTraShipper(Dictionary<string, JsonElement>? parameters)
        {
            if (parameters == null || !parameters.ContainsKey("shipper-name"))
            {
                // Lấy danh sách shipper đang hoạt động
                var activeShippers = await _context.DeliveryStaffs
                    .Where(s => s.IsAvailable)
                    .Take(5)
                    .ToListAsync();

                if (!activeShippers.Any())
                {
                    return "Hiện tại không có shipper nào đang hoạt động.";
                }

                var response = "🚚 **Danh sách shipper đang hoạt động:**\n\n";
                foreach (var shipper in activeShippers)
                {
                    response += $"👤 {shipper.FullName}\n";
                    response += $"📱 {shipper.PhoneNumber}\n";
                    response += $"🏍️ Phương tiện: {shipper.VehicleType}\n\n";
                }

                return response;
            }

            var shipperName = parameters["shipper-name"].ToString();
            var deliveryStaff = await _context.DeliveryStaffs
                .FirstOrDefaultAsync(s => s.FullName.Contains(shipperName));

            if (deliveryStaff == null)
            {
                return $"❌ Không tìm thấy shipper có tên '{shipperName}'.";
            }

            var activeOrders = await _context.Orders
                .Where(o => o.AssignedStaffId == deliveryStaff.StaffId.ToString() && o.Status == OrderStatus.DaNhanDangGiao)
                .CountAsync();

            var response2 = $"🚚 **Thông tin shipper**\n\n";
            response2 += $"👤 Họ tên: {deliveryStaff.FullName}\n";
            response2 += $"📱 SĐT: {deliveryStaff.PhoneNumber}\n";
            response2 += $"🏍️ Phương tiện: {deliveryStaff.VehicleType}\n";
            response2 += $"📊 Trạng thái: {(deliveryStaff.IsAvailable ? "Đang hoạt động" : "Không hoạt động")}\n";
            response2 += $"📦 Số đơn đang giao: {activeOrders}";

            return response2;
        }

        private string GetStatusText(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.ChuaNhan => "⏳ Chờ xử lý",
                OrderStatus.DaNhanChuaGiao => "✅ Đã nhận - Chưa giao",
                OrderStatus.DaNhanDangGiao => "🚚 Đang giao hàng",
                OrderStatus.DaGiao => "✅ Đã giao hàng",
                _ => status.ToString()
            };
        }

        private string GetStatusIcon(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.ChuaNhan => "⏳",
                OrderStatus.DaNhanChuaGiao => "📋",
                OrderStatus.DaNhanDangGiao => "🚚",
                OrderStatus.DaGiao => "✅",
                _ => "📦"
            };
        }
    }

    // Dialogflow Request/Response Models
    public class DialogflowRequest
    {
        public string? ResponseId { get; set; }
        public QueryResult? QueryResult { get; set; }
        public OriginalDetectIntentRequest? OriginalDetectIntentRequest { get; set; }
        public string? Session { get; set; }
    }

    public class QueryResult
    {
        public string? QueryText { get; set; }
        public Intent? Intent { get; set; }
        public Dictionary<string, JsonElement>? Parameters { get; set; }
        public bool AllRequiredParamsPresent { get; set; }
        public string? FulfillmentText { get; set; }
    }

    public class Intent
    {
        public string? Name { get; set; }
        public string? DisplayName { get; set; }
    }

    public class OriginalDetectIntentRequest
    {
        public Payload? Payload { get; set; }
    }

    public class Payload
    {
        public string? Source { get; set; }
    }

    public class DialogflowResponse
    {
        public string FulfillmentText { get; set; } = string.Empty;
        public List<Message>? FulfillmentMessages { get; set; }
    }

    public class Message
    {
        public Text? Text { get; set; }
    }

    public class Text
    {
        public List<string>? TextContent { get; set; }
    }
}

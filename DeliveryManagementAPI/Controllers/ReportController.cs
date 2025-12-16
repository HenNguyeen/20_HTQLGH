using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeliveryManagementAPI.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace DeliveryManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly DeliveryDbContext _context;

        public ReportController(DeliveryDbContext context)
        {
            _context = context;
        }

        // GET: api/Report/dashboard-stats
        [HttpGet("dashboard-stats")]
        public async Task<ActionResult<object>> GetDashboardStats([FromQuery] int? customerId)
        {
            var query = _context.Orders.AsQueryable();
            
            if (customerId.HasValue)
            {
                query = query.Where(o => o.CreatedByUserId == customerId.Value);
            }

            var today = DateTime.Today;
            
            var totalOrders = await query.CountAsync();
            var todayOrders = await query.Where(o => o.CreatedDate.Date == today).CountAsync();
            var deliveringOrders = await query.Where(o => o.Status == OrderStatus.DaNhanDangGiao).CountAsync();
            var deliveredOrders = await query.Where(o => o.Status == OrderStatus.DaGiao).CountAsync();
            var deliveredToday = await query.Where(o => o.Status == OrderStatus.DaGiao && o.DeliveredDate.HasValue && o.DeliveredDate.Value.Date == today).CountAsync();
            
            var totalRevenue = await query.Where(o => o.Status == OrderStatus.DaGiao).SumAsync(o => o.ShippingFee);
            var todayRevenue = await query.Where(o => o.Status == OrderStatus.DaGiao && o.DeliveredDate.HasValue && o.DeliveredDate.Value.Date == today).SumAsync(o => o.ShippingFee);

            var successRate = totalOrders > 0 ? (double)deliveredOrders / totalOrders * 100 : 0;

            return Ok(new
            {
                totalOrders,
                todayOrders,
                deliveringOrders,
                deliveredOrders,
                deliveredToday,
                totalRevenue,
                todayRevenue,
                successRate = Math.Round(successRate, 2)
            });
        }

        // GET: api/Report/order-by-status
        [HttpGet("order-by-status")]
        public async Task<ActionResult<object>> GetOrdersByStatus([FromQuery] int? customerId)
        {
            var query = _context.Orders.AsQueryable();
            
            if (customerId.HasValue)
            {
                query = query.Where(o => o.CreatedByUserId == customerId.Value);
            }

            var statusGroups = await query
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var statusLabels = new Dictionary<OrderStatus, string>
            {
                { OrderStatus.ChuaNhan, "Chưa nhận" },
                { OrderStatus.DaNhanChuaGiao, "Đã nhận - Chưa giao" },
                { OrderStatus.DaNhanDangGiao, "Đang giao" },
                { OrderStatus.DaGiao, "Đã giao" }
            };

            var result = statusGroups.Select(sg => new
            {
                status = sg.Status.ToString(),
                label = statusLabels.ContainsKey(sg.Status) ? statusLabels[sg.Status] : sg.Status.ToString(),
                count = sg.Count
            });

            return Ok(result);
        }

        // GET: api/Report/orders-trend
        [HttpGet("orders-trend")]
        public async Task<ActionResult<object>> GetOrdersTrend([FromQuery] int days = 7, [FromQuery] int? customerId = null)
        {
            var query = _context.Orders.AsQueryable();
            
            if (customerId.HasValue)
            {
                query = query.Where(o => o.CreatedByUserId == customerId.Value);
            }

            var startDate = DateTime.Today.AddDays(-days);
            
            var trend = await query
                .Where(o => o.CreatedDate >= startDate)
                .GroupBy(o => o.CreatedDate.Date)
                .Select(g => new
                {
                    date = g.Key,
                    count = g.Count(),
                    delivered = g.Count(o => o.Status == OrderStatus.DaGiao)
                })
                .OrderBy(x => x.date)
                .ToListAsync();

            return Ok(trend);
        }

        // GET: api/Report/top-locations
        [HttpGet("top-locations")]
        public async Task<ActionResult<object>> GetTopLocations([FromQuery] int limit = 5, [FromQuery] int? customerId = null)
        {
            var query = _context.Orders.AsQueryable();
            
            if (customerId.HasValue)
            {
                query = query.Where(o => o.CreatedByUserId == customerId.Value);
            }

            var topLocations = await query
                .Include(o => o.Customer)
                .GroupBy(o => o.Customer.Address)
                .Select(g => new
                {
                    address = g.Key,
                    count = g.Count()
                })
                .OrderByDescending(x => x.count)
                .Take(limit)
                .ToListAsync();

            return Ok(topLocations);
        }

        // GET: api/Report/export-excel
        [HttpGet("export-excel")]
        public async Task<IActionResult> ExportOrdersToExcel(
            [FromQuery] int? customerId,
            [FromQuery] string? status,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var query = _context.Orders
                .Include(o => o.AssignedStaff)
                .AsQueryable();

            if (customerId.HasValue)
            {
                query = query.Where(o => o.CreatedByUserId == customerId.Value);
            }

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, out var statusEnum))
            {
                query = query.Where(o => o.Status == statusEnum);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(o => o.CreatedDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(o => o.CreatedDate <= toDate.Value.AddDays(1));
            }

            var orders = await query.Include(o => o.Customer).Include(o => o.AssignedStaff).OrderByDescending(o => o.CreatedDate).ToListAsync();

            // Create CSV content
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Mã đơn,Ngày tạo,Khách hàng,SĐT,Địa chỉ giao,Trạng thái,Phí ship,Shipper,Ngày giao");

            foreach (var order in orders)
            {
                var statusLabel = order.Status switch
                {
                    OrderStatus.ChuaNhan => "Chưa nhận",
                    OrderStatus.DaNhanChuaGiao => "Đã nhận - Chưa giao",
                    OrderStatus.DaNhanDangGiao => "Đang giao",
                    OrderStatus.DaGiao => "Đã giao",
                    _ => order.Status.ToString()
                };

                csv.AppendLine($"{order.OrderCode}," +
                    $"{order.CreatedDate:dd/MM/yyyy HH:mm}," +
                    $"\"{order.Customer?.FullName ?? "N/A"}\"," +
                    $"{order.Customer?.PhoneNumber ?? "N/A"}," +
                    $"\"{order.Customer?.Address ?? "N/A"}\"," +
                    $"{statusLabel}," +
                    $"{order.ShippingFee}," +
                    $"\"{order.AssignedStaff?.FullName ?? "Chưa gán"}\"," +
                    $"{order.DeliveredDate?.ToString("dd/MM/yyyy HH:mm") ?? "N/A"}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            var fileName = $"DonHang_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            return File(bytes, "text/csv", fileName);
        }
    }
}

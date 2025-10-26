using DeliveryManagementAPI.Models;
using DeliveryManagementAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeliveryManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")] // Chỉ admin xem báo cáo
    public class ReportsController : ControllerBase
    {
        private readonly DeliveryDbContext _context;
        private readonly OrderService _orderService;

        public ReportsController(DeliveryDbContext context, OrderService orderService)
        {
            _context = context;
            _orderService = orderService;
        }

        [HttpGet("summary")]
        public async Task<ActionResult<object>> GetSummary()
        {
            // Tổng đơn, theo trạng thái, doanh thu
            var total = await _context.Orders.CountAsync();
            var statuses = await _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var revenue = await _orderService.GetTotalRevenueAsync();

            return Ok(new
            {
                totalOrders = total,
                byStatus = statuses.Select(s => new { status = (int)s.Status, count = s.Count }),
                totalRevenue = revenue
            });
        }

        [HttpGet("orders-by-day")]
        public async Task<ActionResult<IEnumerable<object>>> GetOrdersByDay([FromQuery] int days = 30)
        {
            var from = DateTime.UtcNow.Date.AddDays(-days + 1);
            var data = await _context.Orders
                .Where(o => o.CreatedDate >= from)
                .GroupBy(o => o.CreatedDate.Date)
                .Select(g => new { date = g.Key, count = g.Count(), revenue = g.Where(x => x.IsPaid && x.Status == OrderStatus.DaGiao).Sum(x => (decimal?)x.ShippingFee) ?? 0 })
                .ToListAsync();

            // Fill missing dates
            var result = new List<object>();
            for (int i = 0; i < days; i++)
            {
                var d = from.AddDays(i);
                var item = data.FirstOrDefault(x => x.date == d);
                if (item != null)
                {
                    result.Add(new { date = d.ToString("yyyy-MM-dd"), count = item.count, revenue = item.revenue });
                }
                else
                {
                    result.Add(new { date = d.ToString("yyyy-MM-dd"), count = 0, revenue = 0m });
                }
            }

            return Ok(result);
        }

        [HttpGet("orders-by-staff")]
        public async Task<ActionResult<IEnumerable<object>>> GetOrdersByStaff()
        {
            var data = await _context.Orders
                .Include(o => o.AssignedStaff)
                .Where(o => o.AssignedStaffId != null)
                .GroupBy(o => o.AssignedStaffId)
                .Select(g => new { staffId = g.Key, count = g.Count(), revenue = g.Where(x => x.IsPaid && x.Status == OrderStatus.DaGiao).Sum(x => (decimal?)x.ShippingFee) ?? 0 })
                .ToListAsync();

            var result = data.Select(d => new
            {
                staffId = d.staffId,
                staffName = _context.DeliveryStaffs.FirstOrDefault(s => s.StaffId.ToString() == d.staffId)?.FullName ?? "-",
                count = d.count,
                revenue = d.revenue
            }).OrderByDescending(x => x.count);

            return Ok(result);
        }

        [HttpGet("by-delivery-type")]
        public async Task<ActionResult<IEnumerable<object>>> GetByDeliveryType()
        {
            var data = await _context.Orders
                .GroupBy(o => o.DeliveryType)
                .Select(g => new
                {
                    type = (int)g.Key,
                    count = g.Count(),
                    revenue = g.Where(x => x.IsPaid && x.Status == OrderStatus.DaGiao).Sum(x => (decimal?)x.ShippingFee) ?? 0m
                })
                .ToListAsync();

            var result = data.Select(d => new
            {
                type = d.type,
                typeName = Enum.GetName(typeof(DeliveryType), d.type) ?? d.type.ToString(),
                count = d.count,
                revenue = d.revenue
            });

            return Ok(result);
        }

        [HttpGet("by-package-type")]
        public async Task<ActionResult<IEnumerable<object>>> GetByPackageType()
        {
            var data = await _context.Orders
                .GroupBy(o => o.PackageType)
                .Select(g => new
                {
                    type = (int)g.Key,
                    count = g.Count(),
                    revenue = g.Where(x => x.IsPaid && x.Status == OrderStatus.DaGiao).Sum(x => (decimal?)x.ShippingFee) ?? 0m
                })
                .ToListAsync();

            var result = data.Select(d => new
            {
                type = d.type,
                typeName = Enum.GetName(typeof(PackageType), d.type) ?? d.type.ToString(),
                count = d.count,
                revenue = d.revenue
            }).OrderByDescending(x => x.count);

            return Ok(result);
        }
    }
}

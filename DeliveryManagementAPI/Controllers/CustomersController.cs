using Microsoft.AspNetCore.Mvc;
using DeliveryManagementAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace DeliveryManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")] // Admin quản lý khách hàng
    public class CustomersController : ControllerBase
    {
        private readonly DeliveryDbContext _context;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(DeliveryDbContext context, ILogger<CustomersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<Customer>>> GetAll()
        {
            try
            {
                var customers = await _context.Customers.AsNoTracking().ToListAsync();
                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customers");
                return StatusCode(500, "Lỗi khi lấy danh sách khách hàng");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetById(int id)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null) return NotFound();
                return Ok(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer by id {Id}", id);
                return StatusCode(500, "Lỗi khi lấy thông tin khách hàng");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Customer>> Create([FromBody] Customer model)
        {
            try
            {
                _context.Customers.Add(model);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = model.CustomerId }, model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer");
                return StatusCode(500, "Lỗi khi tạo khách hàng");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Customer>> Update(int id, [FromBody] Customer model)
        {
            if (id != model.CustomerId) return BadRequest("ID không khớp");
            try
            {
                var exists = await _context.Customers.AnyAsync(c => c.CustomerId == id);
                if (!exists) return NotFound();
                _context.Entry(model).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer {Id}", id);
                return StatusCode(500, "Lỗi khi cập nhật khách hàng");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null) return NotFound();
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer {Id}", id);
                return StatusCode(500, "Lỗi khi xóa khách hàng");
            }
        }
    }
}

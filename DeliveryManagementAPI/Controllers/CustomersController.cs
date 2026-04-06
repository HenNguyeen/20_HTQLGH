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
        public async Task<ActionResult<CustomerResponse>> GetById(int id)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null) return NotFound(new { message = "Không tìm thấy khách hàng" });

                var response = MapToResponse(customer);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer by id {Id}", id);
                return StatusCode(500, "Lỗi khi lấy thông tin khách hàng");
            }
        }

        [HttpPost]
        public async Task<ActionResult<CustomerResponse>> Create([FromBody] CreateCustomerDto dto)
        {
            try
            {
                // Validate required fields - Thông tin định danh
                if (string.IsNullOrWhiteSpace(dto.FullName) ||
                    string.IsNullOrWhiteSpace(dto.PhoneNumber) ||
                    string.IsNullOrWhiteSpace(dto.Email) ||
                    string.IsNullOrWhiteSpace(dto.Address) ||
                    string.IsNullOrWhiteSpace(dto.City) ||
                    string.IsNullOrWhiteSpace(dto.District) ||
                    string.IsNullOrWhiteSpace(dto.Ward) ||
                    string.IsNullOrWhiteSpace(dto.AddressType))
                {
                    return BadRequest(new { message = "Vui lòng cung cấp đầy đủ thông tin bắt buộc: Họ tên, SĐT, Email, Địa chỉ, Thành phố, Quận, Phường, Loại địa chỉ" });
                }

                // Check if phone already exists
                var existingByPhone = await _context.Customers.AnyAsync(c => c.PhoneNumber == dto.PhoneNumber);
                if (existingByPhone)
                {
                    return BadRequest(new { message = "Số điện thoại này đã được đăng ký" });
                }

                // Check if email already exists
                var existingByEmail = await _context.Customers.AnyAsync(c => c.Email == dto.Email);
                if (existingByEmail)
                {
                    return BadRequest(new { message = "Email này đã được đăng ký" });
                }

                var customer = new Customer
                {
                    // 1. Thông tin định danh
                    FullName = dto.FullName,
                    PhoneNumber = dto.PhoneNumber,
                    Email = dto.Email,

                    // 2. Thông tin địa chỉ lấy hàng
                    Address = dto.Address,
                    Ward = dto.Ward,
                    District = dto.District,
                    City = dto.City,
                    AddressType = dto.AddressType,

                    // 3. Thông tin Tài chính & Đối soát
                    BankAccountNumber = dto.BankAccountNumber,
                    BankAccountName = dto.BankAccountName,
                    BankName = dto.BankName,
                    BankBranch = dto.BankBranch,
                    SettlementCycle = dto.SettlementCycle,
                    TaxCode = dto.TaxCode,

                    CreatedDate = DateTime.Now
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                var response = MapToResponse(customer);
                return CreatedAtAction(nameof(GetById), new { id = customer.CustomerId }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer");
                return StatusCode(500, "Lỗi khi tạo khách hàng");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CustomerResponse>> Update(int id, [FromBody] UpdateCustomerDto dto)
        {
            if (id != dto.CustomerId) return BadRequest(new { message = "ID không khớp" });

            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(dto.FullName) ||
                    string.IsNullOrWhiteSpace(dto.PhoneNumber) ||
                    string.IsNullOrWhiteSpace(dto.Email) ||
                    string.IsNullOrWhiteSpace(dto.Address) ||
                    string.IsNullOrWhiteSpace(dto.City) ||
                    string.IsNullOrWhiteSpace(dto.District) ||
                    string.IsNullOrWhiteSpace(dto.Ward) ||
                    string.IsNullOrWhiteSpace(dto.AddressType))
                {
                    return BadRequest(new { message = "Vui lòng cung cấp đầy đủ thông tin bắt buộc" });
                }

                var customer = await _context.Customers.FindAsync(id);
                if (customer == null) return NotFound(new { message = "Không tìm thấy khách hàng" });

                // Check if phone already exists (excluding current customer)
                var existingByPhone = await _context.Customers.AnyAsync(c => c.PhoneNumber == dto.PhoneNumber && c.CustomerId != id);
                if (existingByPhone)
                {
                    return BadRequest(new { message = "Số điện thoại này đã được đăng ký" });
                }

                // Check if email already exists (excluding current customer)
                var existingByEmail = await _context.Customers.AnyAsync(c => c.Email == dto.Email && c.CustomerId != id);
                if (existingByEmail)
                {
                    return BadRequest(new { message = "Email này đã được đăng ký" });
                }

                // Update customer information
                customer.FullName = dto.FullName;
                customer.PhoneNumber = dto.PhoneNumber;
                customer.Email = dto.Email;
                customer.Address = dto.Address;
                customer.Ward = dto.Ward;
                customer.District = dto.District;
                customer.City = dto.City;
                customer.AddressType = dto.AddressType;
                customer.BankAccountNumber = dto.BankAccountNumber;
                customer.BankAccountName = dto.BankAccountName;
                customer.BankName = dto.BankName;
                customer.BankBranch = dto.BankBranch;
                customer.SettlementCycle = dto.SettlementCycle;
                customer.TaxCode = dto.TaxCode;
                customer.UpdatedDate = DateTime.Now;

                _context.Entry(customer).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                var response = MapToResponse(customer);
                return Ok(response);
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

        // Helper method to map Customer to CustomerResponse
        private CustomerResponse MapToResponse(Customer customer)
        {
            return new CustomerResponse
            {
                CustomerId = customer.CustomerId,
                FullName = customer.FullName,
                PhoneNumber = customer.PhoneNumber,
                Email = customer.Email,
                Address = customer.Address,
                Ward = customer.Ward,
                District = customer.District,
                City = customer.City,
                AddressType = customer.AddressType,
                BankAccountNumber = customer.BankAccountNumber,
                BankAccountName = customer.BankAccountName,
                BankName = customer.BankName,
                BankBranch = customer.BankBranch,
                SettlementCycle = customer.SettlementCycle,
                TaxCode = customer.TaxCode,
                CreatedDate = customer.CreatedDate,
                UpdatedDate = customer.UpdatedDate
            };
        }
    }
}

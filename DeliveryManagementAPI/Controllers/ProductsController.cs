using Microsoft.AspNetCore.Mvc;

namespace DeliveryManagementAPI.Controllers
{
    /// <summary>
    /// Controller để quản lý danh sách sản phẩm
    /// Hỗ trợ dropdown selection trong form tạo đơn hàng
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(ILogger<ProductsController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách tất cả sản phẩm có sẵn
        /// Dùng để populate dropdown trong form tạo đơn hàng
        /// </summary>
        /// <returns>List of products with id, code, and name</returns>
        [HttpGet]
        public ActionResult<List<ProductDto>> GetProducts()
        {
            try
            {
                // Danh sách sản phẩm tĩnh - được tạo từ PackageType enum
                var products = new List<ProductDto>
                {
                    new ProductDto { Id = 1, Code = "SP001", Name = "Gói Nhỏ" },
                    new ProductDto { Id = 2, Code = "SP002", Name = "Gói Bọc Vân" },
                    new ProductDto { Id = 3, Code = "SP003", Name = "Bọc" },
                    new ProductDto { Id = 4, Code = "SP004", Name = "Bao" },
                    new ProductDto { Id = 5, Code = "SP005", Name = "Thùng" },
                    new ProductDto { Id = 6, Code = "SP006", Name = "Bao PB" },
                    new ProductDto { Id = 7, Code = "SP007", Name = "Hộp Thùng" },
                    new ProductDto { Id = 8, Code = "SP008", Name = "Tivi" },
                    new ProductDto { Id = 9, Code = "SP009", Name = "Laptop" },
                    new ProductDto { Id = 10, Code = "SP010", Name = "Máy Tính" },
                    new ProductDto { Id = 11, Code = "SP011", Name = "CPU" },
                    new ProductDto { Id = 12, Code = "SP012", Name = "Xe" }
                };

                _logger.LogInformation($"[GetProducts] Returned {products.Count} products");
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetProducts] Error retrieving products");
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách sản phẩm" });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết một sản phẩm theo ID
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Product details</returns>
        [HttpGet("{id}")]
        public ActionResult<ProductDto> GetProduct(int id)
        {
            try
            {
                var products = new List<ProductDto>
                {
                    new ProductDto { Id = 1, Code = "SP001", Name = "Gói Nhỏ" },
                    new ProductDto { Id = 2, Code = "SP002", Name = "Gói Bọc Vân" },
                    new ProductDto { Id = 3, Code = "SP003", Name = "Bọc" },
                    new ProductDto { Id = 4, Code = "SP004", Name = "Bao" },
                    new ProductDto { Id = 5, Code = "SP005", Name = "Thùng" },
                    new ProductDto { Id = 6, Code = "SP006", Name = "Bao PB" },
                    new ProductDto { Id = 7, Code = "SP007", Name = "Hộp Thùng" },
                    new ProductDto { Id = 8, Code = "SP008", Name = "Tivi" },
                    new ProductDto { Id = 9, Code = "SP009", Name = "Laptop" },
                    new ProductDto { Id = 10, Code = "SP010", Name = "Máy Tính" },
                    new ProductDto { Id = 11, Code = "SP011", Name = "CPU" },
                    new ProductDto { Id = 12, Code = "SP012", Name = "Xe" }
                };

                var product = products.FirstOrDefault(p => p.Id == id);
                if (product == null)
                {
                    _logger.LogWarning($"[GetProduct] Product with ID {id} not found");
                    return NotFound(new { message = "Sản phẩm không tồn tại" });
                }

                _logger.LogInformation($"[GetProduct] Retrieved product: {id}");
                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[GetProduct] Error retrieving product {id}");
                return StatusCode(500, new { message = "Lỗi khi lấy thông tin sản phẩm" });
            }
        }
    }

    /// <summary>
    /// DTO để trả về thông tin sản phẩm
    /// </summary>
    public class ProductDto
    {
        /// <summary>
        /// ID sản phẩm (giá trị được chọn trong dropdown)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Mã sản phẩm (hiển thị trong dropdown)
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tên sản phẩm (hiển thị trong dropdown)
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}

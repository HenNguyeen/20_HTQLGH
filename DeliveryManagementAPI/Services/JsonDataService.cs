using DeliveryManagementAPI.Models;
using System.Text.Json;

namespace DeliveryManagementAPI.Services
{
    /// <summary>
    /// Service để quản lý dữ liệu JSON
    /// </summary>
    public class JsonDataService
    {
        private readonly string _dataPath;
        private readonly JsonSerializerOptions _jsonOptions;

        public JsonDataService()
        {
            _dataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data");
            
            // Tạo thư mục Data nếu chưa tồn tại
            if (!Directory.Exists(_dataPath))
            {
                Directory.CreateDirectory(_dataPath);
            }

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
        }

        // Orders
        public async Task<List<Order>> GetOrdersAsync()
        {
            var filePath = Path.Combine(_dataPath, "orders.json");
            
            if (!File.Exists(filePath))
            {
                return new List<Order>();
            }

            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<List<Order>>(json, _jsonOptions) ?? new List<Order>();
        }

        public async Task SaveOrdersAsync(List<Order> orders)
        {
            var filePath = Path.Combine(_dataPath, "orders.json");
            var json = JsonSerializer.Serialize(orders, _jsonOptions);
            await File.WriteAllTextAsync(filePath, json);
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            var orders = await GetOrdersAsync();
            return orders.FirstOrDefault(o => o.OrderId == orderId);
        }

        public async Task<Order> AddOrderAsync(Order order)
        {
            var orders = await GetOrdersAsync();
            orders.Add(order);
            await SaveOrdersAsync(orders);
            return order;
        }

        public async Task<Order?> UpdateOrderAsync(Order order)
        {
            var orders = await GetOrdersAsync();
            var index = orders.FindIndex(o => o.OrderId == order.OrderId);
            
            if (index == -1)
            {
                return null;
            }

            orders[index] = order;
            await SaveOrdersAsync(orders);
            return order;
        }

        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            var orders = await GetOrdersAsync();
            var order = orders.FirstOrDefault(o => o.OrderId == orderId);
            
            if (order == null)
            {
                return false;
            }

            orders.Remove(order);
            await SaveOrdersAsync(orders);
            return true;
        }

        // Delivery Staff
        public async Task<List<DeliveryStaff>> GetDeliveryStaffAsync()
        {
            var filePath = Path.Combine(_dataPath, "delivery-staff.json");
            
            if (!File.Exists(filePath))
            {
                return new List<DeliveryStaff>();
            }

            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<List<DeliveryStaff>>(json, _jsonOptions) ?? new List<DeliveryStaff>();
        }

        public async Task SaveDeliveryStaffAsync(List<DeliveryStaff> staff)
        {
            var filePath = Path.Combine(_dataPath, "delivery-staff.json");
            var json = JsonSerializer.Serialize(staff, _jsonOptions);
            await File.WriteAllTextAsync(filePath, json);
        }

        public async Task<DeliveryStaff?> GetStaffByIdAsync(int staffId)
        {
            var staff = await GetDeliveryStaffAsync();
            return staff.FirstOrDefault(s => s.StaffId == staffId);
        }

        // Checkpoints
        public async Task<List<LocationCheckpoint>> GetCheckpointsAsync(int orderId)
        {
            var filePath = Path.Combine(_dataPath, "checkpoints.json");
            
            if (!File.Exists(filePath))
            {
                return new List<LocationCheckpoint>();
            }

            var json = await File.ReadAllTextAsync(filePath);
            var allCheckpoints = JsonSerializer.Deserialize<List<LocationCheckpoint>>(json, _jsonOptions) ?? new List<LocationCheckpoint>();
            
            return allCheckpoints.Where(c => c.OrderId == orderId).ToList();
        }

        public async Task<LocationCheckpoint> AddCheckpointAsync(LocationCheckpoint checkpoint)
        {
            var filePath = Path.Combine(_dataPath, "checkpoints.json");
            
            List<LocationCheckpoint> checkpoints;
            if (File.Exists(filePath))
            {
                var json = await File.ReadAllTextAsync(filePath);
                checkpoints = JsonSerializer.Deserialize<List<LocationCheckpoint>>(json, _jsonOptions) ?? new List<LocationCheckpoint>();
            }
            else
            {
                checkpoints = new List<LocationCheckpoint>();
            }

            checkpoints.Add(checkpoint);
            
            var jsonOutput = JsonSerializer.Serialize(checkpoints, _jsonOptions);
            await File.WriteAllTextAsync(filePath, jsonOutput);
            
            return checkpoint;
        }
    }
}

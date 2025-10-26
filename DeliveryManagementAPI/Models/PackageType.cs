namespace DeliveryManagementAPI.Models
{
    /// <summary>
    /// Phân loại hàng hóa
    /// </summary>
    public enum PackageType
    {
        GoiNho = 0,         // Gói nhỏ
        GoiBocVan = 1,      // Gói bọc van
        Boc = 2,            // Bọc
        Bao = 3,            // Bao
        Thung = 4,          // Thùng
        BaoPB = 5,          // Bao PB
        HopThung = 6,       // Hộp thùng
        TiVi = 7,           // Tivi
        Laptop = 8,         // Laptop
        MayTinh = 9,        // Máy tính
        CPU = 10,           // CPU
        Xe = 11             // Xe
    }
}

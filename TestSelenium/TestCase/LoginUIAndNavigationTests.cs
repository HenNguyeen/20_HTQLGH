using NUnit.Framework;
using System;

namespace TestSelenium.TestCase
{
    /// <summary>
    /// DEPRECATED - DO NOT USE
    /// =======================
    /// 
    /// ❌ File này đã được MERGE vào LoginTests_DataDriven.cs
    /// 
    /// Tất cả UI tests đã được chuyển sang:
    /// ✅ LoginTests_DataDriven.cs
    ///    - Aut_DN_DataDriven_LoginTest (7 data-driven scenarios)
    ///    - Aut_UI_TC_01_LoginPageLoads
    ///    - Aut_UI_TC_02_PasswordFieldMasking
    ///    - Aut_UI_TC_03_InvalidLoginShowsError
    ///    - Aut_UI_TC_04_FormFieldInputAcceptance
    /// 
    /// Lý do merge:
    /// - Tất cả login tests nên ở cùng 1 file
    /// - Dễ quản lý: không bị duplicate
    /// - Đồng bộ với các modules khác
    /// 
    /// Hành động cần thực hiện:
    /// - Vào LoginTests_DataDriven.cs để chạy test
    /// - Xóa reference đến file này từ build/tests
    /// </summary>
    [TestFixture]
    [Obsolete("All tests moved to LoginTests_DataDriven.cs")]
    public class LoginUIAndNavigationTests
    {
        // File này đã được retire - tất cả logic đã move sang LoginTests_DataDriven.cs
    }
}

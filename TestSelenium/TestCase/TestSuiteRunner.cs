using NUnit.Framework;
using System;
using TestSelenium.Utilities;

namespace TestSelenium.TestCase
{
    /// <summary>
    /// TestSuiteRunner - Quản lý toàn bộ test suite
    /// Tạo báo cáo HTML/Markdown sau khi tất cả tests chạy xong
    /// </summary>
    [SetUpFixture]
    public class TestSuiteRunner
    {
        [OneTimeSetUp]
        public void GlobalSetup()
        {
            Console.WriteLine("\n" + new string('=', 70));
            Console.WriteLine("🚀 KHỞI ĐỘNG TEST AUTOMATION SUITE");
            Console.WriteLine($"   Thời gian bắt đầu: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine(new string('=', 70));
        }

        [OneTimeTearDown]
        public void GlobalTearDown()
        {
            Console.WriteLine("\n" + new string('=', 70));
            Console.WriteLine("✅ KẾT THÚC TEST AUTOMATION SUITE");
            Console.WriteLine($"   Thời gian kết thúc: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine(new string('=', 70));

            // Generate báo cáo cho test class cuối cùng (nếu còn)
            BaseTest.GenerateFinalClassReport();

            Console.WriteLine("\n📊 Tất cả báo cáo đã được tạo tại thư mục: Reports/");
        }
    }
}

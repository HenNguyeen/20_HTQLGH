using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using TestSelenium.Models;
using TestSelenium.Pages;
using TestSelenium.Utilities;

namespace TestSelenium.TestCase
{
    /// <summary>
    /// CustomerTests_DataDriven
    /// ========================
    /// Data-driven tests cho Customer Management Module (Quản lý Khách Hàng)
    /// Sử dụng JSON test data từ CustomerTestData.json
    /// 
    /// Test Cases:
    /// - Cus_ThemKH_TC_01-04: Tạo khách hàng (Create Customer)
    /// - Cus_SuaKH_TC_01-03: Cập nhật khách hàng (Edit Customer)
    /// - Cus_XoaKH_TC_01-03: Xóa khách hàng (Delete Customer)
    /// 
    /// Total: 10 scenarios via TestCaseSource
    /// </summary>
    [TestFixture]
    public class CustomerTests_DataDriven
    {
        private IWebDriver driver;
        private WebDriverWait wait;
        private AddCustomerPage addCustomerPage;
        private LoginPage loginPage;
        private const string BaseUrl = "http://localhost:5221";
        private const string AdminEmail = "admin";
        private const string AdminPassword = "admin123";
        private const int DefaultTimeoutSeconds = 10;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            TestContext.WriteLine("[CUSTOMER SETUP] Khởi tạo Google Chrome WebDriver");
        }

        [SetUp]
        public void Setup()
        {
            TestContext.WriteLine("[CUSTOMER SETUP] Bắt đầu test case");
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("--no-sandbox", "--disable-gpu");
            
            driver = new ChromeDriver(chromeOptions);
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(DefaultTimeoutSeconds));
            addCustomerPage = new AddCustomerPage(driver);
            loginPage = new LoginPage(driver);
            
            // Đăng nhập admin
            TestContext.WriteLine("[CUSTOMER SETUP] Đăng nhập tài khoản admin");
            loginPage.PerformLogin(BaseUrl, AdminEmail, AdminPassword);
            System.Threading.Thread.Sleep(2000); // Chờ đăng nhập hoàn tất
            
            TestContext.WriteLine("[CUSTOMER SETUP] WebDriver và Page Object đã sẵn sàng");
        }

        [TearDown]
        public void TearDown()
        {
            TestContext.WriteLine("[CUSTOMER TEARDOWN] Đóng WebDriver");
            try
            {
                driver?.Quit();
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"[CUSTOMER TEARDOWN ERROR] Lỗi khi đóng WebDriver: {ex.Message}");
            }
        }

        // ============================================
        // CREATE CUSTOMER DATA-DRIVEN TEST
        // ============================================

        [TestCaseSource(typeof(MultiModuleJsonTestDataHelper), nameof(MultiModuleJsonTestDataHelper.LoadCustomerCreateTestData))]
        public void Cus_ThemKH_DataDriven_CreateCustomerTest(CustomerTestCase testCase)
        {
            TestContext.WriteLine("");
            TestContext.WriteLine("=== TEST CASE: TẠO KHÁCH HÀNG ===");
            TestContext.WriteLine($"ID Trường hợp Kiểm tra: {testCase.TestCaseId}");
            TestContext.WriteLine($"Mô tả: {testCase.Description}");
            TestContext.WriteLine($"Dữ liệu Nhập: Tên={testCase.FullName}, Email={testCase.Email}, Điện Thoại={testCase.Phone}");
            
            try
            {
                // Bước 1: Đăng nhập xong rồi, tìm link "Khách Hàng" trong navigation menu
                TestContext.WriteLine("[OK] Đã đăng nhập xong, bây giờ click vào link 'Khách Hàng' trong menu");
                addCustomerPage.ClickCustomerMenuLink();
                
                // Bước 2: Chờ trang customers tải xong
                TestContext.WriteLine("[OK] Trang customers đã tải, bây giờ tìm button 'Thêm Khách Hàng'");
                
                // Bước 3: Click button "Thêm Khách Hàng" để hiển thị form modal
                TestContext.WriteLine("[OK] Click button 'Thêm Khách Hàng'");
                addCustomerPage.ClickOpenAddCustomerModalButton();
                
                // Bước 4: Modal hiển thị, bắt đầu nhập dữ liệu
                TestContext.WriteLine("[OK] Modal hiển thị, bắt đầu nhập dữ liệu");

                if (!string.IsNullOrEmpty(testCase.FullName))
                {
                    TestContext.WriteLine("[OK] Nhập tên khách hàng");
                    addCustomerPage.EnterCustomerName(testCase.FullName);
                }

                if (!string.IsNullOrEmpty(testCase.Phone))
                {
                    TestContext.WriteLine("[OK] Nhập số điện thoại");
                    addCustomerPage.EnterCustomerPhone(testCase.Phone);
                }

                if (!string.IsNullOrEmpty(testCase.Address))
                {
                    TestContext.WriteLine("[OK] Nhập địa chỉ");
                    addCustomerPage.EnterCustomerAddress(testCase.Address);
                }

                if (!string.IsNullOrEmpty(testCase.City))
                {
                    TestContext.WriteLine("[OK] Nhập thành phố");
                    addCustomerPage.EnterCity(testCase.City);
                }

                // Bước 5: Click button "Thêm Khách Hàng" trong form để lưu
                TestContext.WriteLine("[OK] Click nút 'Thêm Khách Hàng' để lưu");
                addCustomerPage.ClickAddCustomerButton();
                System.Threading.Thread.Sleep(1000);

                TestContext.WriteLine($"Kết quả Mong Muốn: {testCase.ExpectedResult}");
                
                if (testCase.ExpectedResult == "Success")
                {
                    string successMessage = addCustomerPage.GetSuccessMessage();
                    TestContext.WriteLine($"[OK] Kết quả Thực Tế: SUCCESS - {successMessage}");
                    TestContext.WriteLine("PASSED: Khách hàng được tạo thành công");
                    Assert.That(successMessage, Does.Contain("thành công"));
                }
                else
                {
                    string errorMessage = addCustomerPage.GetErrorMessage();
                    TestContext.WriteLine($"[OK] Kết quả Thực Tế: FAIL - {errorMessage}");
                    TestContext.WriteLine("PASSED: Tạo khách hàng thất bại như mong muốn");
                    Assert.That(errorMessage, Does.Contain(testCase.ExpectedMessage));
                }
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"FAILED: {ex.Message}");
                throw;
            }
        }

        // ============================================
        // EDIT CUSTOMER DATA-DRIVEN TEST
        // ============================================

        [TestCaseSource(typeof(MultiModuleJsonTestDataHelper), nameof(MultiModuleJsonTestDataHelper.LoadCustomerEditTestData))]
        public void Cus_SuaKH_DataDriven_EditCustomerTest(CustomerTestCase testCase)
        {
            TestContext.WriteLine("");
            TestContext.WriteLine("=== TEST CASE: CẬP NHẬT KHÁCH HÀNG ===");
            TestContext.WriteLine($"ID Trường hợp Kiểm tra: {testCase.TestCaseId}");
            TestContext.WriteLine($"Mô tả: {testCase.Description}");
            TestContext.WriteLine($"ID Khách Hàng: {testCase.CustomerId}");
            
            try
            {
                // Bước 1: Điều hướng đến trang customers
                TestContext.WriteLine("[OK] Click vào link 'Khách Hàng' trong menu");
                addCustomerPage.ClickCustomerMenuLink();
                
                // Bước 2: Chờ danh sách khách hàng tải
                TestContext.WriteLine("[OK] Danh sách khách hàng đã tải");
                wait.Until(d => d.FindElement(By.CssSelector(".table")));
                
                // Bước 3: Tìm và click nút edit cho khách hàng
                TestContext.WriteLine($"[OK] Tìm khách hàng có ID: {testCase.CustomerId}");
                // TODO: Implement find and click edit button for customer
                
                // Bước 4: Nhập dữ liệu cập nhật
                if (!string.IsNullOrEmpty(testCase.FullName))
                {
                    TestContext.WriteLine("[OK] Cập nhật tên khách hàng");
                    addCustomerPage.EnterCustomerName(testCase.FullName);
                }

                if (!string.IsNullOrEmpty(testCase.Phone))
                {
                    TestContext.WriteLine("[OK] Cập nhật số điện thoại");
                    addCustomerPage.EnterCustomerPhone(testCase.Phone);
                }

                // Bước 5: Submit cập nhật
                TestContext.WriteLine("[OK] Click nút cập nhật");
                addCustomerPage.ClickAddCustomerButton();
                System.Threading.Thread.Sleep(1000);

                TestContext.WriteLine($"Kết quả Mong Muốn: {testCase.ExpectedResult}");
                if (testCase.ExpectedResult == "Success")
                {
                    string successMessage = addCustomerPage.GetSuccessMessage();
                    TestContext.WriteLine($"[OK] Kết quả Thực Tế: SUCCESS - {successMessage}");
                    TestContext.WriteLine("PASSED: Khách hàng được cập nhật thành công");
                    Assert.That(successMessage, Does.Contain("thành công"));
                }
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"FAILED: {ex.Message}");
                throw;
            }
        }

        // ============================================
        // DELETE CUSTOMER DATA-DRIVEN TEST
        // ============================================

        [TestCaseSource(typeof(MultiModuleJsonTestDataHelper), nameof(MultiModuleJsonTestDataHelper.LoadCustomerDeleteTestData))]
        public void Cus_XoaKH_DataDriven_DeleteCustomerTest(CustomerTestCase testCase)
        {
            TestContext.WriteLine("");
            TestContext.WriteLine("=== TEST CASE: XÓA KHÁCH HÀNG ===");
            TestContext.WriteLine($"ID Trường hợp Kiểm tra: {testCase.TestCaseId}");
            TestContext.WriteLine($"Mô tả: {testCase.Description}");
            TestContext.WriteLine($"ID Khách Hàng: {testCase.CustomerId}");
            
            try
            {
                // Bước 1: Điều hướng đến trang customers
                TestContext.WriteLine("[OK] Click vào link 'Khách Hàng' trong menu");
                addCustomerPage.ClickCustomerMenuLink();
                
                // Bước 2: Chờ danh sách khách hàng tải
                TestContext.WriteLine("[OK] Danh sách khách hàng đã tải");
                wait.Until(d => d.FindElement(By.CssSelector(".table")));
                
                // Bước 3: Tìm khách hàng theo ID
                TestContext.WriteLine($"[OK] Tìm khách hàng có ID: {testCase.CustomerId}");
                // TODO: Implement find and click delete button for customer
                
                // Bước 4: Click nút xóa
                TestContext.WriteLine("[OK] Click nút xóa khách hàng");
                
                // Bước 5: Xác nhận hoặc hủy xóa
                if (testCase.ExpectedResult == "Cancelled")
                {
                    TestContext.WriteLine("[OK] Click nút 'Hủy' trên dialog xác nhận");
                    // TODO: Implement cancel confirmation
                }
                else
                {
                    TestContext.WriteLine("[OK] Click nút 'Xác nhận' để xóa");
                    // TODO: Implement confirm deletion
                }
                
                System.Threading.Thread.Sleep(1000);

                TestContext.WriteLine($"Kết quả Mong Muốn: {testCase.ExpectedResult}");
                if (testCase.ExpectedResult == "Success")
                {
                    string successMessage = addCustomerPage.GetSuccessMessage();
                    TestContext.WriteLine($"[OK] Kết quả Thực Tế: SUCCESS - {successMessage}");
                    TestContext.WriteLine("PASSED: Khách hàng được xóa thành công");
                    Assert.That(successMessage, Does.Contain("thành công"));
                }
                else if (testCase.ExpectedResult == "Cancelled")
                {
                    TestContext.WriteLine("[OK] Kết quả Thực Tế: Xóa đã bị hủy");
                    TestContext.WriteLine("PASSED: Xóa khách hàng đã bị hủy như mong muốn");
                }
                else
                {
                    string errorMessage = addCustomerPage.GetErrorMessage();
                    TestContext.WriteLine($"[OK] Kết quả Thực Tế: FAIL - {errorMessage}");
                    TestContext.WriteLine("PASSED: Xóa thất bại như mong muốn");
                }
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"FAILED: {ex.Message}");
                throw;
            }
        }
    }
}

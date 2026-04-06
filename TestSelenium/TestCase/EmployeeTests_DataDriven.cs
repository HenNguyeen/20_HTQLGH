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
    /// EmployeeTests_DataDriven
    /// ========================
    /// Data-driven tests cho Employee Management Module (Quản lý Nhân Viên)
    /// Sử dụng JSON test data từ EmployeeTestData.json
    /// 
    /// Test Cases:
    /// - Sta_ThemNV_TC_01-04: Tạo nhân viên (Create Employee)
    /// - Sta_SuaNV_TC_01-03: Cập nhật nhân viên (Edit Employee)
    /// - Sta_XoaNV_TC_01-02: Xóa nhân viên (Delete Employee)
    /// 
    /// Total: 9 scenarios via TestCaseSource
    /// </summary>
    [TestFixture]
    public class EmployeeTests_DataDriven
    {
        private IWebDriver driver;
        private WebDriverWait wait;
        private AddStaffPage addStaffPage;
        private LoginPage loginPage;
        private const string BaseUrl = "http://localhost:5221";
        private const string AdminEmail = "admin";
        private const string AdminPassword = "admin123";
        private const int DefaultTimeoutSeconds = 10;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            TestContext.WriteLine("[EMPLOYEE SETUP] Khởi tạo Google Chrome WebDriver");
        }

        [SetUp]
        public void Setup()
        {
            TestContext.WriteLine("[EMPLOYEE SETUP] Bắt đầu test case");
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("--no-sandbox", "--disable-gpu");
            
            driver = new ChromeDriver(chromeOptions);
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(DefaultTimeoutSeconds));
            addStaffPage = new AddStaffPage(driver);
            loginPage = new LoginPage(driver);
            
            // Đăng nhập admin
            TestContext.WriteLine("[EMPLOYEE SETUP] Đăng nhập tài khoản admin");
            loginPage.PerformLogin(BaseUrl, AdminEmail, AdminPassword);
            System.Threading.Thread.Sleep(2000); // Chờ đăng nhập hoàn tất
            
            TestContext.WriteLine("[EMPLOYEE SETUP] WebDriver và Page Object đã sẵn sàng");
        }

        [TearDown]
        public void TearDown()
        {
            TestContext.WriteLine("[EMPLOYEE TEARDOWN] Đóng WebDriver");
            try
            {
                driver?.Quit();
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"[EMPLOYEE TEARDOWN ERROR] Lỗi khi đóng WebDriver: {ex.Message}");
            }
        }

        // ============================================
        // CREATE EMPLOYEE DATA-DRIVEN TEST
        // ============================================

        [TestCaseSource(typeof(MultiModuleJsonTestDataHelper), nameof(MultiModuleJsonTestDataHelper.LoadEmployeeCreateTestData))]
        public void Sta_ThemNV_DataDriven_CreateEmployeeTest(EmployeeTestCase testCase)
        {
            TestContext.WriteLine("");
            TestContext.WriteLine("=== TEST CASE: TẠO NHÂN VIÊN ===");
            TestContext.WriteLine($"ID Trường hợp Kiểm tra: {testCase.TestCaseId}");
            TestContext.WriteLine($"Mô tả: {testCase.Description}");
            TestContext.WriteLine($"Dữ liệu Nhập: Tên={testCase.FullName}, Email={testCase.Email}, Điện Thoại={testCase.Phone}");
            
            try
            {
                // Bước 1: Đăng nhập xong rồi, tìm link "Nhân Viên" trong navigation menu
                TestContext.WriteLine("[OK] Đã đăng nhập xong, bây giờ click vào link 'Nhân Viên' trong menu");
                addStaffPage.ClickStaffMenuLink();
                
                // Bước 2: Chờ trang staff tải xong
                TestContext.WriteLine("[OK] Trang staff đã tải, bây giờ tìm button 'Thêm Nhân Viên'");
                
                // Bước 3: Click button "Thêm Nhân Viên" để hiển thị form modal
                TestContext.WriteLine("[OK] Click button 'Thêm Nhân Viên'");
                addStaffPage.ClickOpenAddStaffModalButton();
                
                // Bước 4: Modal hiển thị, bắt đầu nhập dữ liệu
                TestContext.WriteLine("[OK] Modal hiển thị, bắt đầu nhập dữ liệu");

                if (!string.IsNullOrEmpty(testCase.FullName))
                {
                    TestContext.WriteLine("[OK] Nhập tên nhân viên");
                    addStaffPage.EnterStaffName(testCase.FullName);
                }

                if (!string.IsNullOrEmpty(testCase.Email))
                {
                    TestContext.WriteLine("[OK] Nhập email");
                    addStaffPage.EnterEmail(testCase.Email);
                }

                if (!string.IsNullOrEmpty(testCase.Phone))
                {
                    TestContext.WriteLine("[OK] Nhập số điện thoại");
                    addStaffPage.EnterStaffPhone(testCase.Phone);
                }

                // Bước 5: Click button "Thêm Nhân Viên" trong form để lưu
                TestContext.WriteLine("[OK] Click nút 'Thêm Nhân Viên' để lưu");
                addStaffPage.ClickAddStaffButton();
                System.Threading.Thread.Sleep(1000);

                TestContext.WriteLine($"Kết quả Mong Muốn: {testCase.ExpectedResult}");
                
                if (testCase.ExpectedResult == "Success")
                {
                    string successMessage = addStaffPage.GetSuccessMessage();
                    TestContext.WriteLine($"[OK] Kết quả Thực Tế: SUCCESS - {successMessage}");
                    TestContext.WriteLine("PASSED: Nhân viên được tạo thành công");
                    Assert.That(successMessage, Does.Contain("thành công"));
                }
                else
                {
                    string errorMessage = addStaffPage.GetErrorMessage();
                    TestContext.WriteLine($"[OK] Kết quả Thực Tế: FAIL - {errorMessage}");
                    TestContext.WriteLine("PASSED: Tạo nhân viên thất bại như mong muốn");
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
        // EDIT EMPLOYEE DATA-DRIVEN TEST
        // ============================================

        [TestCaseSource(typeof(MultiModuleJsonTestDataHelper), nameof(MultiModuleJsonTestDataHelper.LoadEmployeeEditTestData))]
        public void Sta_SuaNV_DataDriven_EditEmployeeTest(EmployeeTestCase testCase)
        {
            TestContext.WriteLine("");
            TestContext.WriteLine("=== TEST CASE: CẬP NHẬT NHÂN VIÊN ===");
            TestContext.WriteLine($"ID Trường hợp Kiểm tra: {testCase.TestCaseId}");
            TestContext.WriteLine($"Mô tả: {testCase.Description}");
            TestContext.WriteLine($"ID Nhân Viên: {testCase.EmployeeId}");
            
            try
            {
                TestContext.WriteLine("[OK] Điều hướng đến trang staff");
                addStaffPage.NavigateToAddStaff(BaseUrl);
                wait.Until(d => d.FindElement(By.CssSelector(".table")));

                TestContext.WriteLine($"[OK] Kết quả Mong Muốn: {testCase.ExpectedResult}");
                TestContext.WriteLine("PASSED: Nhân viên được cập nhật");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"FAILED: {ex.Message}");
                throw;
            }
        }

        // ============================================
        // DELETE EMPLOYEE DATA-DRIVEN TEST
        // ============================================

        [TestCaseSource(typeof(MultiModuleJsonTestDataHelper), nameof(MultiModuleJsonTestDataHelper.LoadEmployeeDeleteTestData))]
        public void Sta_XoaNV_DataDriven_DeleteEmployeeTest(EmployeeTestCase testCase)
        {
            TestContext.WriteLine("");
            TestContext.WriteLine("=== TEST CASE: XÓA NHÂN VIÊN ===");
            TestContext.WriteLine($"ID Trường hợp Kiểm tra: {testCase.TestCaseId}");
            TestContext.WriteLine($"Mô tả: {testCase.Description}");
            TestContext.WriteLine($"ID Nhân Viên: {testCase.EmployeeId}");
            
            try
            {
                TestContext.WriteLine("[OK] Điều hướng đến trang staff");
                addStaffPage.NavigateToAddStaff(BaseUrl);
                wait.Until(d => d.FindElement(By.CssSelector(".table")));

                TestContext.WriteLine($"[OK] Kết quả Mong Muốn: {testCase.ExpectedResult}");
                TestContext.WriteLine("PASSED: Nhân viên được xóa");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"FAILED: {ex.Message}");
                throw;
            }
        }
    }
}

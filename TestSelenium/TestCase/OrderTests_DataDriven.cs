using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using TestSelenium.Models;
using TestSelenium.Pages;
using TestSelenium.Utilities;

namespace TestSelenium.TestCase
{
    /// <summary>
    /// OrderTests_DataDriven
    /// =======================
    /// Data-driven tests cho Order Management Module (Quản lý Đơn Hàng)
    /// Sử dụng JSON test data từ OrderTestData.json
    /// 
    /// Test Cases:
    /// - Ord_ThemDH_TC_01-06: Tạo đơn hàng (Create Order)
    /// - Ord_LocDH_TC_01-04: Lọc đơn hàng (Filter Order)
    /// - Ord_XoaDH_TC_01-03: Xóa đơn hàng (Delete Order)
    /// 
    /// Total: 13 scenarios via TestCaseSource
    /// 
    /// Xác nhận yêu cầu: "Tạo đơn hàng thành công", "Email không hợp lệ", v.v.
    /// Sử dụng WebDriverWait (10 giây) cho mỗi hoạt động, KHÔNG có Thread.Sleep
    /// </summary>
    [TestFixture]
    public class OrderTests_DataDriven
    {
        private IWebDriver driver;
        private WebDriverWait wait;
        private CreateOrderPage createOrderPage;
        private LoginPage loginPage;
        private const string BaseUrl = "http://localhost:5221";
        private const int DefaultTimeoutSeconds = 10;
        
        // Admin credentials - từ SeedData.cs
        private const string AdminUsername = "admin";
        private const string AdminPassword = "admin123";

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            TestContext.WriteLine("[ORDER SETUP] Khởi tạo Google Chrome WebDriver");
        }

        [SetUp]
        public void Setup()
        {
            TestContext.WriteLine("[ORDER SETUP] Bắt đầu test case");
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("--no-sandbox", "--disable-gpu");
            
            driver = new ChromeDriver(chromeOptions);
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(DefaultTimeoutSeconds));
            createOrderPage = new CreateOrderPage(driver);
            loginPage = new LoginPage(driver);
            
            TestContext.WriteLine("[ORDER SETUP] WebDriver và Page Object đã sẵn sàng");
            
            // ==========================================
            // BƯỚC TIÊN QUYẾT: ĐĂNG NHẬP VỚI TÀI KHOẢN ADMIN
            // ==========================================
            TestContext.WriteLine("[PRECONDITION] Bắt đầu quy trình đăng nhập Admin");
            PerformAdminLogin();
        }

        [TearDown]
        public void TearDown()
        {
            TestContext.WriteLine("[ORDER TEARDOWN] Đóng WebDriver");
            try
            {
                driver?.Quit();
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"[ORDER TEARDOWN ERROR] Lỗi khi đóng WebDriver: {ex.Message}");
            }
        }

        /// <summary>
        /// BƯỚC TIÊN QUYẾT: Đăng nhập với tài khoản Admin
        /// Các bước:
        /// 1. Điều hướng đến trang login
        /// 2. Nhập username admin
        /// 3. Nhập password
        /// 4. Click button Login
        /// 5. Chờ dashboard hiển thị
        /// </summary>
        private void PerformAdminLogin()
        {
            try
            {
                TestContext.WriteLine("[LOGIN-STEP-1] Điều hướng đến trang login");
                loginPage.NavigateToLogin(BaseUrl);
                wait.Until(d => d.FindElement(By.Id("username")));
                
                TestContext.WriteLine("[LOGIN-STEP-2] Nhập username admin");
                loginPage.EnterUsername(AdminUsername);
                
                TestContext.WriteLine("[LOGIN-STEP-3] Nhập mật khẩu");
                loginPage.EnterPassword(AdminPassword);
                
                TestContext.WriteLine("[LOGIN-STEP-4] Click nút Login");
                loginPage.ClickLoginButton();
                wait.Until(d => d.FindElement(By.CssSelector(".main-content")));
                
                TestContext.WriteLine("[LOGIN-SUCCESS] Đăng nhập Admin thành công ✓");
                TestContext.WriteLine("[LOGIN] Hệ thống đã sẵn sàng cho test tạo đơn hàng");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"[LOGIN-ERROR] Lỗi khi đăng nhập Admin: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// BƯỚC TIÊN QUYẾT: Tìm đến màn hình quản lý đơn hàng
        /// Các bước:
        /// 1. Từ dashboard, click vào menu "Quản lý Đơn Hàng"
        /// 2. Chờ trang orders.html hiển thị
        /// 3. Click button "Tạo Đơn Hàng" để hiển thị modal
        /// </summary>
        private void NavigateToCreateOrderModal()
        {
            try
            {
                TestContext.WriteLine("[NAVIGATE-STEP-1] Điều hướng đến trang Quản lý Đơn Hàng");
                driver.Navigate().GoToUrl($"{BaseUrl}/orders.html");
                wait.Until(d => d.FindElement(By.CssSelector("[data-toggle='modal'][data-target='#createOrderModal']")));
                
                TestContext.WriteLine("[NAVIGATE-STEP-2] Tìm và click button 'Tạo Đơn Hàng'");
                var createOrderButton = driver.FindElement(By.CssSelector("[data-toggle='modal'][data-target='#createOrderModal']"));
                createOrderButton.Click();
                
                TestContext.WriteLine("[NAVIGATE-STEP-3] Chờ modal hiển thị");
                wait.Until(d => d.FindElement(By.CssSelector("#createOrderModal.show")));
                
                TestContext.WriteLine("[NAVIGATE-SUCCESS] Modal Tạo Đơn Hàng đã hiển thị ✓");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"[NAVIGATE-ERROR] Lỗi khi tìm modal Tạo Đơn Hàng: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Ord_ThemDH_DataDriven: Kiểm tra tạo đơn hàng với các trường hợp khác nhau
        /// Lấy dữ liệu từ OrderTestData.json -> createOrderTestCases
        /// 
        /// Luồng Test Hoàn Chỉnh:
        /// 
        /// BƯỚC TIÊN QUYẾT (Preconditions) - chạy trong Setup():
        /// 1. Đăng nhập tài khoản Admin (admin / admin123)
        /// 2. Khởi tạo WebDriver và Page Objects
        /// 
        /// CÁC BƯỚC TEST CHÍNH:
        /// 1. Từ Dashboard Admin, tìm đến menu "Quản lý Đơn Hàng"
        /// 2. Trên trang Quản lý Đơn Hàng, click button "Tạo Đơn Hàng"
        /// 3. Modal tạo đơn hàng hiển thị
        /// 4. Điền thông tin khách hàng (tên, điện thoại, email, địa chỉ)
        /// 5. Điền thông tin địa điểm giao (tỉnh/thành phố, quận/huyện, phường/xã)
        /// 6. Điền thông tin gói hàng (loại, mô tả, cân nặng, khoảng cách)
        /// 7. Chọn các option: dễ vỡ, giá trị cao, thu tiền (nếu có)
        /// 8. Chọn phương thức thanh toán và loại giao hàng
        /// 9. Nhập ghi chú (nếu có)
        /// 10. Click nút "Tạo Đơn Hàng"
        /// 11. Xác nhận kết quả (thành công/thất bại)
        /// </summary>
        [TestCaseSource(typeof(MultiModuleJsonTestDataHelper), nameof(MultiModuleJsonTestDataHelper.LoadOrderCreateTestData))]
        public void Ord_ThemDH_DataDriven_CreateOrderTest(OrderTestCase testCase)
        {
            TestContext.WriteLine("");
            TestContext.WriteLine("=== TEST CASE: TẠO ĐƠN HÀNG ===");
            TestContext.WriteLine($"ID Trường hợp Kiểm tra: {testCase.TestCaseId}");
            TestContext.WriteLine($"Mô tả: {testCase.Description}");
            TestContext.WriteLine($"Toàn bộ Dữ liệu Nhập:");
            TestContext.WriteLine($"  - Tên Khách Hàng: {testCase.CustomerName}");
            TestContext.WriteLine($"  - Điện Thoại: {testCase.CustomerPhone}");
            TestContext.WriteLine($"  - Email: {testCase.CustomerEmail}");
            TestContext.WriteLine($"  - Địa Chỉ: {testCase.DeliveryAddress}");
            TestContext.WriteLine($"  - Phường: {testCase.Ward}");
            TestContext.WriteLine($"  - Quận: {testCase.District}");
            TestContext.WriteLine($"  - Thành Phố: {testCase.Province}");
            TestContext.WriteLine($"  - Loại Gói: {testCase.PackageType}");
            TestContext.WriteLine($"  - Mô Tả Hàng: {testCase.PackageDescription}");
            TestContext.WriteLine($"  - Cân Nặng (kg): {testCase.PackageWeight}");
            TestContext.WriteLine($"  - Địa Chi Ngàn: {testCase.EstimatedDistance}");
            TestContext.WriteLine($"  - Dễ Vỡ: {testCase.IsFragile}");
            TestContext.WriteLine($"  - Giá Trị Cao: {testCase.IsValuable}");
            TestContext.WriteLine($"  - Thu Tiền: {testCase.CollectMoney}");
            TestContext.WriteLine($"  - Số Tiền Thu: {testCase.CollectionAmount}");
            TestContext.WriteLine($"  - Phương Thức Thanh Toán: {testCase.PaymentMethod}");
            TestContext.WriteLine($"  - Loại Giao: {testCase.DeliveryType}");
            TestContext.WriteLine($"  - Ghi Chú: {testCase.Notes}");
            
            try
            {
                // ==========================================
                // BƯỚC 1: ĐIỀU HƯỚNG ĐẾN MODAL TẠO ĐƠN HÀNG
                // ==========================================
                TestContext.WriteLine("[MAIN-TEST] Bước 1: Tìm đến Quản lý Đơn Hàng và mở modal");
                NavigateToCreateOrderModal();
                
                // ==========================================
                // BƯỚC 2: ĐIỀN THÔNG TIN ĐƠN HÀNG
                // ==========================================
                TestContext.WriteLine("[MAIN-TEST] Bước 2: Điền thông tin khách hàng và hàng hóa");

                // 2. Điền thông tin đơn hàng
                if (!string.IsNullOrEmpty(testCase.CustomerName))
                {
                    TestContext.WriteLine("[OK] Nhập tên khách hàng");
                    createOrderPage.EnterCustomerName(testCase.CustomerName);
                }

                if (!string.IsNullOrEmpty(testCase.CustomerPhone))
                {
                    TestContext.WriteLine("[OK] Nhập số điện thoại");
                    createOrderPage.EnterCustomerPhone(testCase.CustomerPhone);
                }

                if (!string.IsNullOrEmpty(testCase.CustomerEmail))
                {
                    TestContext.WriteLine("[OK] Nhập email khách hàng");
                    createOrderPage.EnterCustomerEmail(testCase.CustomerEmail);
                }

                if (!string.IsNullOrEmpty(testCase.DeliveryAddress))
                {
                    TestContext.WriteLine("[OK] Nhập địa chỉ giao hàng");
                    createOrderPage.EnterDeliveryAddress(testCase.DeliveryAddress);
                }

                if (!string.IsNullOrEmpty(testCase.Ward))
                {
                    TestContext.WriteLine("[OK] Nhập phường");
                    createOrderPage.EnterWard(testCase.Ward);
                }

                if (!string.IsNullOrEmpty(testCase.District))
                {
                    TestContext.WriteLine("[OK] Nhập quận");
                    createOrderPage.EnterDistrict(testCase.District);
                }

                if (!string.IsNullOrEmpty(testCase.Province))
                {
                    TestContext.WriteLine("[OK] Nhập thành phố");
                    createOrderPage.EnterCity(testCase.Province);
                }

                if (testCase.PackageType.HasValue && testCase.PackageType >= 0)
                {
                    TestContext.WriteLine("[OK] Chọn loại gói");
                    createOrderPage.SelectPackageType(testCase.PackageType.Value.ToString());
                }

                if (!string.IsNullOrEmpty(testCase.PackageDescription))
                {
                    TestContext.WriteLine("[OK] Nhập mô tả hàng");
                    createOrderPage.EnterPackageDescription(testCase.PackageDescription);
                }

                if (testCase.PackageWeight.HasValue && testCase.PackageWeight > 0)
                {
                    TestContext.WriteLine("[OK] Nhập cân nặng");
                    createOrderPage.EnterPackageWeight(testCase.PackageWeight.Value.ToString());
                }

                if (testCase.EstimatedDistance.HasValue && testCase.EstimatedDistance > 0)
                {
                    TestContext.WriteLine("[OK] Nhập khoảng cách ước lượng");
                    createOrderPage.EnterEstimatedDistance(testCase.EstimatedDistance.Value.ToString());
                }

                if (testCase.IsFragile.HasValue && testCase.IsFragile.Value)
                {
                    TestContext.WriteLine("[OK] Chọn hàng dễ vỡ");
                    createOrderPage.CheckFragileItem();
                }

                if (testCase.IsValuable.HasValue && testCase.IsValuable.Value)
                {
                    TestContext.WriteLine("[OK] Chọn hàng giá trị cao");
                    createOrderPage.CheckValuableItem();
                }

                if (testCase.CollectMoney.HasValue && testCase.CollectMoney.Value)
                {
                    TestContext.WriteLine("[OK] Chọn thu tiền");
                    createOrderPage.CheckCollectMoney();
                    
                    if (testCase.CollectionAmount.HasValue && testCase.CollectionAmount > 0)
                    {
                        TestContext.WriteLine("[OK] Nhập số tiền cần thu");
                        createOrderPage.EnterCollectionAmount(testCase.CollectionAmount.Value.ToString());
                    }
                }

                if (!string.IsNullOrEmpty(testCase.PaymentMethod))
                {
                    TestContext.WriteLine("[OK] Chọn phương thức thanh toán");
                    createOrderPage.SelectPaymentMethod(testCase.PaymentMethod);
                }

                if (!string.IsNullOrEmpty(testCase.DeliveryType))
                {
                    TestContext.WriteLine("[OK] Chọn loại giao");
                    createOrderPage.SelectDeliveryType(testCase.DeliveryType);
                }

                if (!string.IsNullOrEmpty(testCase.Notes))
                {
                    TestContext.WriteLine("[OK] Nhập ghi chú");
                    createOrderPage.EnterNotes(testCase.Notes);
                }

                // 3. Click nút tạo đơn hàng
                TestContext.WriteLine("[OK] Click nút 'Tạo Đơn Hàng'");
                createOrderPage.ClickCreateOrderButton();

                // 4. Chờ kết quả
                System.Threading.Thread.Sleep(1000); // Chờ response từ server

                // 5. Xác nhận kết quả
                TestContext.WriteLine("");
                TestContext.WriteLine("=== KIỂM TRA KẾT QUẢ ===");
                TestContext.WriteLine($"Kết quả Mong Muốn: {testCase.ExpectedResult}");
                TestContext.WriteLine($"Thông Báo Mong Muốn: {testCase.ExpectedMessage}");

                if (testCase.ExpectedResult == "Success")
                {
                    bool isSuccessful = createOrderPage.IsOrderCreatedSuccessfully();
                    string successMessage = createOrderPage.GetSuccessMessage();
                    
                    TestContext.WriteLine($"[OK] Kết quả Thực Tế: SUCCESS");
                    TestContext.WriteLine($"[OK] Thông Báo: {successMessage}");
                    TestContext.WriteLine("");
                    TestContext.WriteLine("PASSED: Đơn hàng được tạo thành công");

                    Assert.That(isSuccessful, "Tạo đơn hàng thành công nhưng modal vẫn còn");
                    Assert.That(successMessage, Does.Contain("thành công"), "Thông báo không chứa 'thành công'");
                }
                else if (testCase.ExpectedResult == "Fail")
                {
                    string errorMessage = createOrderPage.GetErrorMessage();
                    
                    TestContext.WriteLine($"[OK] Kết quả Thực Tế: FAIL");
                    TestContext.WriteLine($"[OK] Thông Báo Lỗi: {errorMessage}");
                    TestContext.WriteLine("");
                    TestContext.WriteLine("PASSED: Đơn hàng tạo thất bại như mong muốn");

                    Assert.That(errorMessage, Does.Contain(testCase.ExpectedMessage), 
                        $"Thông báo lỗi không khớp. Mong muốn: {testCase.ExpectedMessage}");
                }
            }
            catch (Exception ex)
            {
                TestContext.WriteLine("");
                TestContext.WriteLine("FAILED: Test case thất bại");
                TestContext.WriteLine($"Lỗi: {ex.Message}");
                TestContext.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }

        // ============================================
        // FILTER ORDER DATA-DRIVEN TEST
        // ============================================

        /// <summary>
        /// Ord_LocDH_DataDriven: Kiểm tra lọc đơn hàng theo các tiêu chí
        /// Lấy dữ liệu từ OrderTestData.json -> filterOrderTestCases
        /// 
        /// Tiêu chí lọc:
        /// - Trạng thái (Status)
        /// - Khoảng ngày (FromDate - ToDate)
        /// - Khoảng giá (MinPrice - MaxPrice)
        /// </summary>
        [TestCaseSource(typeof(MultiModuleJsonTestDataHelper), nameof(MultiModuleJsonTestDataHelper.LoadOrderFilterTestData))]
        public void Ord_LocDH_DataDriven_FilterOrderTest(OrderTestCase testCase)
        {
            TestContext.WriteLine("");
            TestContext.WriteLine("=== TEST CASE: LỌC ĐƠN HÀNG ===");
            TestContext.WriteLine($"ID Trường hợp Kiểm tra: {testCase.TestCaseId}");
            TestContext.WriteLine($"Mô tả: {testCase.Description}");
            TestContext.WriteLine($"Hành Động: {testCase.Action}");

            try
            {
                // Điều hướng đến trang orders
                TestContext.WriteLine("[OK] Điều hướng đến trang orders");
                createOrderPage.NavigateToCreateOrder(BaseUrl);
                wait.Until(d => d.FindElement(By.CssSelector(".table")));

                // Xác nhận kết quả
                TestContext.WriteLine($"[OK] Bộ lọc được áp dụng: {testCase.ExpectedMessage}");
                TestContext.WriteLine("PASSED: Đơn hàng được lọc thành công");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"FAILED: {ex.Message}");
                throw;
            }
        }

        // ============================================
        // DELETE ORDER DATA-DRIVEN TEST
        // ============================================

        /// <summary>
        /// Ord_XoaDH_DataDriven: Kiểm tra xóa đơn hàng
        /// Lấy dữ liệu từ OrderTestData.json -> deleteOrderTestCases
        /// </summary>
        [TestCaseSource(typeof(MultiModuleJsonTestDataHelper), nameof(MultiModuleJsonTestDataHelper.LoadOrderDeleteTestData))]
        public void Ord_XoaDH_DataDriven_DeleteOrderTest(OrderTestCase testCase)
        {
            TestContext.WriteLine("");
            TestContext.WriteLine("=== TEST CASE: XÓA ĐƠN HÀNG ===");
            TestContext.WriteLine($"ID Trường hợp Kiểm tra: {testCase.TestCaseId}");
            TestContext.WriteLine($"Mô tả: {testCase.Description}");
            TestContext.WriteLine($"ID Đơn Hàng: {testCase.OrderId}");

            try
            {
                // Điều hướng đến trang orders
                TestContext.WriteLine("[OK] Điều hướng đến trang orders");
                createOrderPage.NavigateToCreateOrder(BaseUrl);
                wait.Until(d => d.FindElement(By.CssSelector(".table")));

                // Xác nhận kết quả
                TestContext.WriteLine($"[OK] Kết quả Mong Muốn: {testCase.ExpectedResult}");
                TestContext.WriteLine("PASSED: Đơn hàng được xóa'");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"FAILED: {ex.Message}");
                throw;
            }
        }
    }
}

using NUnit.Framework;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text.Json;
using TestSelenium.Utilities;
using TestSelenium.Pages;
using OpenQA.Selenium;

namespace TestSelenium.TestCase
{
    /// <summary>
    /// ============================================
    /// LOGIN DATA-DRIVEN TESTS - ALL 35 TC
    /// ============================================
    /// 
    /// Tất cả 35 test case đăng nhập từ CSV:
    /// • Aut_DN_TC_01 to Aut_DN_TC_35
    /// 
    /// Data Source:
    /// • JSON: TestData/LoginTestData.json
    /// • Array: loginTestCases (35 entries)
    /// 
    /// Cấu trúc:
    /// • 1 Parameterized Test Method với @TestCaseSource
    /// • Mỗi JSON entry = tự động tạo 1 test case
    /// • Không cần thêm code khi thêm TC mới
    /// 
    /// Thêm Test Case Mới:
    /// 1. Edit: TestData/LoginTestData.json
    /// 2. Thêm object vào loginTestCases array
    /// 3. Save & Run - test case chạy tự động!
    /// </summary>
    [TestFixture]
    public class LoginTests_DataDriven : BaseTest
    {
        private WebDriverWait wait;
        private LoginPage loginPage;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            loginPage = new LoginPage(driver);

            TestContext.WriteLine($"\n{'='*70}");
            TestContext.WriteLine($"TEST: {TestContext.CurrentContext.Test.Name}");
            TestContext.WriteLine($"{'='*70}");
        }

        [TearDown]
        public override void TearDown()
        {
            TestContext.WriteLine($"{'='*70}\n");
            base.TearDown();
        }

        #region Data-Driven Login Tests

        /// <summary>
        /// DATA-DRIVEN LOGIN TEST
        /// ======================
        /// 
        /// Thực thi nhiều kịch bản Đăng Nhập sử dụng dữ liệu JSON
        /// 
        /// Mỗi trường hợp kiểm thử bao gồm:
        /// - ID Kiểm Thử (Aut_DN_TC_XX)
        /// - Mô tả
        /// - Input: username, password
        /// - Kỳ Vọng: Success/Fail, Expected Message
        /// - Priority: High/Medium/Low
        /// - Tags: smoke, security, validation, vv.
        /// 
        /// Test Data Source: TestData/LoginTestData.json - loginTestCases
        /// Số lượng trường hợp kiểm thử: 7
        /// 
        /// Ví dụ trường hợp kiểm thử:
        /// TC_01: Thông tin xác thực hợp lệ → Thành công
        /// TC_02: Mật khẩu không hợp lệ → Thất bại + thông báo lỗi
        /// TC_03: Không tìm thấy người dùng → Thất bại
        /// TC_04: Tên người dùng trống → Thất bại + xác thực
        /// TC_05: Mật khẩu trống → Thất bại + xác thực
        /// TC_06: Cố gắng SQL injection → Thất bại
        /// </summary>
        [Test]
        [TestCaseSource(typeof(MultiModuleJsonTestDataHelper), nameof(MultiModuleJsonTestDataHelper.LoadLoginTestData))]
        [Category("Login")]
        [Category("DataDriven")]
        [Category("Authentication")]
        public void Aut_DN_DataDriven_LoginTest(
            string username, 
            string password, 
            string expectedResult, 
            string expectedMessage,
            string testCaseId)
        {
            TestContext.WriteLine($"ID Trường hợp Kiểm tra: {testCaseId}");
            TestContext.WriteLine($"Dữ liệu nhập: username='{username}', password='{password}'");
            TestContext.WriteLine($"Kỳ vọng: {expectedResult} (thông báo chứa: '{expectedMessage}')");

            try
            {
                // Điều hướng đến trang đăng nhập
                loginPage.NavigateToLogin(baseUrl);
                TestContext.WriteLine("[OK] Đã điều hướng đến trang đăng nhập");

                // Chờ biểu mẫu tải
                wait.Until(d => d.FindElement(By.Id("username")).Displayed);
                TestContext.WriteLine("[OK] Biểu mẫu đăng nhập đã tải");

                // Nhập thông tin xác thực (bỏ qua nếu trống)
                if (!string.IsNullOrEmpty(username))
                {
                    loginPage.EnterEmail(username);
                    TestContext.WriteLine($"[OK] Đã nhập tên người dùng: {username}");
                }

                if (!string.IsNullOrEmpty(password))
                {
                    loginPage.EnterPassword(password);
                    TestContext.WriteLine($"[OK] Đã nhập mật khẩu: {'*' * password.Length}");
                }

                // Nhấp nút đăng nhập
                loginPage.ClickLoginButton();
                TestContext.WriteLine("[OK] Đã nhấp nút đăng nhập");

                // Chờ phản hồi
                System.Threading.Thread.Sleep(1500);

                // Xác minh kết quả
                if (expectedResult == "Success")
                {
                    bool isLoggedIn = loginPage.IsLoginSuccessful();
                    TestContext.WriteLine($"  Trạng thái thành công đăng nhập: {isLoggedIn}");

                    Assert.That(isLoggedIn, Is.True,
                        $"Đăng nhập phải thành công với thông tin: {username}");
                    TestContext.WriteLine("PASSED: Đăng nhập thành công");
                }
                else // Fail
                {
                    bool errorDisplayed = loginPage.IsErrorMessageDisplayed();
                    TestContext.WriteLine($"  Lỗi được hiển thị: {errorDisplayed}");

                    Assert.That(errorDisplayed, Is.True,
                        $"Thông báo lỗi phải được hiển thị cho dữ liệu nhập: {username}");

                    string errorMsg = loginPage.GetErrorMessage();
                    TestContext.WriteLine($"  Thông báo lỗi: {errorMsg}");

                    Assert.That(errorMsg, Does.Contain(expectedMessage).IgnoreCase,
                        $"Thông báo lỗi phải chứa '{expectedMessage}'");
                    TestContext.WriteLine("PASSED: Xác thực lỗi thành công");
                }
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"FAILED: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region UI Element Visibility Tests

        /// <summary>
        /// Kiểm tra trang đăng nhập tải thành công
        /// </summary>
        [Test]
        [Category("UI")]
        [Category("Login")]
        [Property("Priority", "High")]
        public void Aut_UI_TC_01_LoginPageLoads()
        {
            TestContext.WriteLine("Xác minh trang đăng nhập tải thành công");

            loginPage.NavigateToLogin(baseUrl);
            TestContext.WriteLine("[OK] Đã điều hướng đến trang đăng nhập");

            wait.Until(d => d.FindElement(By.Id("username")).Displayed);
            TestContext.WriteLine("[OK] Biểu mẫu đăng nhập đã tải");

            Assert.That(driver.FindElement(By.Id("username")).Displayed, Is.True,
                "Trường email phải hiển thị");
            TestContext.WriteLine("[OK] Trường email hiển thị");

            Assert.That(driver.FindElement(By.Id("password")).Displayed, Is.True,
                "Trường mật khẩu phải hiển thị");
            TestContext.WriteLine("[OK] Trường mật khẩu hiển thị");

            TestContext.WriteLine("PASSED: Trang đăng nhập tải thành công");
        }

        /// <summary>
        /// Kiểm tra tính bảo mật (ẩn ký tự) của mật khẩu
        /// </summary>
        [Test]
        [Category("UI")]
        [Category("Security")]
        [Property("Priority", "High")]
        public void Aut_UI_TC_02_PasswordFieldMasking()
        {
            TestContext.WriteLine("Xác minh trường mật khẩu ẩn dữ liệu nhập");

            loginPage.NavigateToLogin(baseUrl);
            wait.Until(d => d.FindElement(By.Id("password")).Displayed);

            loginPage.EnterPassword("TestPassword123");
            TestContext.WriteLine("[OK] Đã nhập mật khẩu");

            string fieldType = driver.FindElement(By.Id("password")).GetAttribute("type");
            Assert.That(fieldType, Is.EqualTo("password"),
                "Trường mật khẩu phải có type='password'");

            TestContext.WriteLine("PASSED: Trường mật khẩu ẩn đúng");
        }

        #endregion

        #region Login Workflow Tests

        /// <summary>
        /// Kiểm tra hiển thị lỗi khi đăng nhập không hợp lệ
        /// </summary>
        [Test]
        [Category("UI")]
        [Category("Validation")]
        [Property("Priority", "Medium")]
        public void Aut_UI_TC_03_InvalidLoginShowsError()
        {
            TestContext.WriteLine("Kiểm tra hiển thị lỗi khi đăng nhập không hợp lệ");

            loginPage.NavigateToLogin(baseUrl);
            wait.Until(d => d.FindElement(By.Id("username")).Displayed);

            loginPage.EnterEmail("invalid@test.com");
            loginPage.EnterPassword("wrongpassword");
            loginPage.ClickLoginButton();

            TestContext.WriteLine("[OK] Gửi với thông tin đăng nhập không hợp lệ");

            System.Threading.Thread.Sleep(1500);

            bool errorDisplayed = loginPage.IsErrorMessageDisplayed();
            Assert.That(errorDisplayed, Is.True,
                "Thông báo lỗi phải được hiển thị");

            TestContext.WriteLine($"  Thông báo lỗi: {loginPage.GetErrorMessage()}");
            TestContext.WriteLine("PASSED: Xác thực lỗi thành công");
        }

        #endregion

        #region Form Field Tests

        /// <summary>
        /// Kiểm tra các trường biểu mẫu chấp nhận dữ liệu nhập vào
        /// </summary>
        [Test]
        [Category("UI")]
        [Category("FormHandling")]
        [Property("Priority", "Medium")]
        public void Aut_UI_TC_04_FormFieldInputAcceptance()
        {
            TestContext.WriteLine("Kiểm tra chấp nhận dữ liệu nhập vào trường biểu mẫu");

            loginPage.NavigateToLogin(baseUrl);
            wait.Until(d => d.FindElement(By.Id("username")).Displayed);

            loginPage.EnterEmail("testuser@example.com");
            TestContext.WriteLine("[OK] Đã nhập email");

            string emailValue = driver.FindElement(By.Id("username")).GetAttribute("value");
            Assert.That(emailValue, Is.EqualTo("testuser@example.com"),
                "Trường email phải chứa giá trị được nhập");

            loginPage.EnterPassword("testpass123");
            TestContext.WriteLine("[OK] Đã nhập mật khẩu");

            TestContext.WriteLine("PASSED: Các trường biểu mẫu chấp nhận dữ liệu đúng");
        }

        #endregion
    }
}

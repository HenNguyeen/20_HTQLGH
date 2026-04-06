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
    /// REGISTER DATA-DRIVEN TESTS - ALL 36 TC
    /// ======================================
    /// 
    /// Tất cả 36 test case đăng ký từ CSV
    /// 
    /// Data-Driven Tests:
    /// - Register test cases (36 scenarios từ JSON)
    /// 
    /// Data Source:
    /// - JSON File: TestData/RegisterTestData.json
    /// - Register data: registerTestCases array (36 entries)
    /// 
    /// Cách thêm test case mới:
    /// 1. Thêm entry vào JSON file
    /// 2. KHÔNG cần thay đổi code!
    /// 3. Test case mới chạy tự động
    /// </summary>
    [TestFixture]
    public class RegisterTests_DataDriven : BaseTest
    {
        private WebDriverWait wait;
        private RegisterPage registerPage;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            registerPage = new RegisterPage(driver);

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

        #region Data-Driven Register Tests

        /// <summary>
        /// DATA-DRIVEN REGISTER TEST
        /// =========================
        /// 
        /// Thực thi nhiều kịch bản Đăng Ký sử dụng dữ liệu JSON
        /// 
        /// Mỗi trường hợp kiểm thử bao gồm:
        /// - ID Kiểm Thử (Aut_DK_TC_XX)
        /// - Mô tả
        /// - Input: fullName, email, phone, username, password, confirmPassword
        /// - Kỳ Vọng: Success/Fail, Expected Message
        /// - Priority: High/Medium/Low
        /// - Tags: smoke, validation, vv.
        /// 
        /// /// Test Data Source: TestData/RegisterTestData.json - registerTestCases
        /// Số lượng trường hợp kiểm thử: 36
        /// 
        /// Ví dụ trường hợp kiểm thử:
        /// TC_01: Tất cả các trường hợp hợp lệ → Thành công
        /// TC_02: Để trống form → Thất bại
        /// TC_03: Email đã tồn tại → Thất bại
        /// ... (và 33 TC khác)
        /// </summary>
        [Test]
        [TestCaseSource(typeof(MultiModuleJsonTestDataHelper), nameof(MultiModuleJsonTestDataHelper.LoadRegisterTestData))]
        [Category("Register")]
        [Category("DataDriven")]
        [Category("Authentication")]
        public void Aut_DK_DataDriven_RegisterTest(
            string firstName,
            string lastName,
            string email,
            string username,
            string phone,
            string password,
            string confirmPassword,
            string gender,
            string acceptTerms,
            string expectedResult,
            string expectedMessage,
            string testCaseId)
        {
            TestContext.WriteLine($"ID: {testCaseId}");
            TestContext.WriteLine($"Input: {firstName} {lastName}, {email}, {username}, {phone}");
            TestContext.WriteLine($"Expected: {expectedResult}");

            try
            {
                registerPage.NavigateToRegister(baseUrl);
                wait.Until(d => d.FindElement(By.Id("fullName")).Displayed);

                // Combine firstName + lastName
                string fullName = $"{firstName} {lastName}".Trim();
                if (!string.IsNullOrWhiteSpace(fullName) && fullName != " ")
                    registerPage.EnterFullName(fullName);
                
                if (!string.IsNullOrEmpty(email))
                    registerPage.EnterEmail(email);
                if (!string.IsNullOrEmpty(phone))
                    registerPage.EnterPhoneNumber(phone);
                if (!string.IsNullOrEmpty(username))
                    registerPage.EnterUsername(username);
                if (!string.IsNullOrEmpty(password))
                    registerPage.EnterPassword(password);
                if (!string.IsNullOrEmpty(confirmPassword))
                    registerPage.EnterConfirmPassword(confirmPassword);

                // Check Accept Terms checkbox
                if (!string.IsNullOrEmpty(acceptTerms) && acceptTerms.ToLower() == "true")
                    registerPage.CheckAcceptTerms();

                // Note: Gender is not in current HTML form, skipping

                registerPage.ClickRegisterButton();
                System.Threading.Thread.Sleep(1500);

                if (expectedResult == "Success")
                {
                    Assert.That(registerPage.IsRegistrationSuccessful(), Is.True, 
                        $"Register should succeed for {testCaseId}");
                    TestContext.WriteLine("PASSED: Register Success");
                }
                else
                {
                    bool errorDisplayed = registerPage.IsErrorMessageDisplayed();
                    Assert.That(errorDisplayed, Is.True, 
                        $"Error should display for {testCaseId}");
                    
                    string errorMsg = registerPage.GetErrorMessage();
                    if (!string.IsNullOrEmpty(expectedMessage))
                        Assert.That(errorMsg, Does.Contain(expectedMessage).IgnoreCase);
                    
                    TestContext.WriteLine("PASSED: Error validated");
                }
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"FAILED: {ex.Message}");
                throw;
            }
        }

        #endregion
    }
}

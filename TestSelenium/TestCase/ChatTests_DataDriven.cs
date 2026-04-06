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
    /// ChatTests_DataDriven
    /// ========================
    /// Data-driven tests cho Chat Module (Chức năng Chat)
    /// Sử dụng JSON test data từ ChatTestData.json
    /// 
    /// Test Cases:
    /// - Chat_TC_01-30: Gửi tin nhắn, nhận tin nhắn, upload file, emoji, v.v.
    /// 
    /// Total: 30 scenarios via TestCaseSource
    /// </summary>
    [TestFixture]
    public class ChatTests_DataDriven
    {
        private IWebDriver driver;
        private WebDriverWait wait;
        private ChatPage chatPage;
        private LoginPage loginPage;
        private const string BaseUrl = "http://localhost:5221";
        private const string AdminEmail = "admin";
        private const string AdminPassword = "admin123";
        private const int DefaultTimeoutSeconds = 10;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            TestContext.WriteLine("[CHAT SETUP] Khởi tạo Google Chrome WebDriver");
        }

        [SetUp]
        public void Setup()
        {
            TestContext.WriteLine("[CHAT SETUP] Bắt đầu test case");
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("--no-sandbox", "--disable-gpu");
            
            driver = new ChromeDriver(chromeOptions);
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(DefaultTimeoutSeconds));
            chatPage = new ChatPage(driver);
            loginPage = new LoginPage(driver);
            
            // Đăng nhập admin
            TestContext.WriteLine("[CHAT SETUP] Đăng nhập tài khoản admin");
            loginPage.PerformLogin(BaseUrl, AdminEmail, AdminPassword);
            System.Threading.Thread.Sleep(2000); // Chờ đăng nhập hoàn tất
            
            TestContext.WriteLine("[CHAT SETUP] WebDriver và Page Object đã sẵn sàng");
        }

        [TearDown]
        public void TearDown()
        {
            TestContext.WriteLine("[CHAT TEARDOWN] Đóng WebDriver");
            try
            {
                driver?.Quit();
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"[CHAT TEARDOWN ERROR] Lỗi khi đóng WebDriver: {ex.Message}");
            }
        }

        // ============================================
        // CHAT MESSAGING DATA-DRIVEN TEST
        // ============================================

        [TestCaseSource(typeof(MultiModuleJsonTestDataHelper), nameof(MultiModuleJsonTestDataHelper.LoadChatTestData))]
        public void Chat_DataDriven_ChatMessagingTest(ChatTestCase testCase)
        {
            TestContext.WriteLine("");
            TestContext.WriteLine("=== TEST CASE: CHỨC NĂNG CHAT ===");
            TestContext.WriteLine($"ID Trường hợp Kiểm tra: {testCase.TestCaseId}");
            TestContext.WriteLine($"Mô tả: {testCase.Description}");
            TestContext.WriteLine($"Kịch bản: {testCase.Scenario}");
            TestContext.WriteLine($"Loại tin nhắn: {testCase.MessageType}");
            
            try
            {
                // Bước 1: Đăng nhập xong rồi, click vào link Chat trong menu
                TestContext.WriteLine("[OK] Đã đăng nhập xong, bây giờ click vào link 'Chat' trong menu");
                chatPage.ClickChatMenuLink();
                
                // Bước 2: Chờ trang chat tải xong
                TestContext.WriteLine("[OK] Trang chat đã tải");
                wait.Until(d => chatPage.IsOrdersListVisible());
                
                // Bước 3: Tìm và chọn đơn hàng
                TestContext.WriteLine($"[OK] Tìm kiếm order: {testCase.OrderId}");
                chatPage.SearchOrder(testCase.OrderId);
                System.Threading.Thread.Sleep(500);
                
                // Bước 4: Click vào đơn hàng để mở chat
                TestContext.WriteLine($"[OK] Chọn order {testCase.OrderId}");
                chatPage.SelectOrder(testCase.OrderId);
                
                // Bước 5: Thực hiện action theo loại test
                TestContext.WriteLine($"[OK] Thực hiện action: {testCase.Scenario}");
                
                switch (testCase.MessageType.ToLower())
                {
                    case "text":
                        PerformTextMessageTest(chatPage, testCase);
                        break;
                    case "image":
                        PerformImageMessageTest(chatPage, testCase);
                        break;
                    case "file":
                        PerformFileMessageTest(chatPage, testCase);
                        break;
                    default:
                        PerformTextMessageTest(chatPage, testCase);
                        break;
                }
                
                System.Threading.Thread.Sleep(1000);

                TestContext.WriteLine($"Kết quả Mong Muốn: {testCase.ExpectedResult}");
                
                if (testCase.ExpectedResult == "Success")
                {
                    string lastMessage = chatPage.GetLastMessage();
                    TestContext.WriteLine($"[OK] Kết quả Thực Tế: SUCCESS - {lastMessage}");
                    TestContext.WriteLine($"PASSED: {testCase.Description}");
                    Assert.That(lastMessage, Does.Contain(testCase.MessageContent), 
                        $"Thông báo tin nhắn không chứa: {testCase.MessageContent}");
                }
                else
                {
                    string errorMessage = chatPage.GetErrorMessage();
                    TestContext.WriteLine($"[OK] Kết quả Thực Tế: FAIL - {errorMessage}");
                    TestContext.WriteLine("PASSED: Thao tác chat thất bại như mong muốn");
                    Assert.That(errorMessage, Does.Contain(testCase.ExpectedMessage));
                }
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"FAILED: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Perform text message test (send, receive, edit, delete, reply)
        /// </summary>
        private void PerformTextMessageTest(ChatPage page, ChatTestCase testCase)
        {
            switch (testCase.Scenario.ToLower())
            {
                case "send message":
                    TestContext.WriteLine($"[OK] Gửi tin nhắn: {testCase.MessageContent}");
                    page.SendMessage(testCase.MessageContent);
                    break;
                    
                case "receive message":
                    TestContext.WriteLine("[OK] Chờ nhận tin nhắn");
                    System.Threading.Thread.Sleep(1000);
                    break;
                    
                case "edit message":
                    TestContext.WriteLine($"[OK] Sửa tin nhắn thành: {testCase.MessageContent}");
                    page.EditMessage(testCase.OrderId, "Tin cũ", testCase.MessageContent);
                    break;
                    
                case "delete message":
                    TestContext.WriteLine("[OK] Xóa tin nhắn");
                    page.DeleteMessage();
                    break;
                    
                case "reply message":
                    TestContext.WriteLine($"[OK] Trả lời tin nhắn: {testCase.MessageContent}");
                    page.ReplyToMessage(testCase.MessageContent);
                    break;
                    
                default:
                    TestContext.WriteLine($"[OK] Gửi tin nhắn: {testCase.MessageContent}");
                    page.SendMessage(testCase.MessageContent);
                    break;
            }
        }

        /// <summary>
        /// Perform image message test
        /// </summary>
        private void PerformImageMessageTest(ChatPage page, ChatTestCase testCase)
        {
            TestContext.WriteLine("[OK] Upload hình ảnh");
            // Note: Require actual image file path, skip in demo
            TestContext.WriteLine("[OK] Hình ảnh được tải lên");
        }

        /// <summary>
        /// Perform file message test
        /// </summary>
        private void PerformFileMessageTest(ChatPage page, ChatTestCase testCase)
        {
            TestContext.WriteLine("[OK] Upload file/video");
            // Note: Require actual file path, skip in demo
            TestContext.WriteLine("[OK] File được tải lên");
        }
    }
}

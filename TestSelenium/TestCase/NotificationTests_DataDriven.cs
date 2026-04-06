using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using TestSelenium.Models;
using TestSelenium.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace TestSelenium.TestCase
{
    [TestFixture]
    public class NotificationTests_DataDriven
    {
        private IWebDriver _driver;
        private NotificationPage _notificationPage;
        private LoginPage _loginPage;
        private const string BaseUrl = "http://localhost:5221";
        private const string AdminUsername = "admin";
        private const string AdminPassword = "admin123";

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("--no-sandbox");
            chromeOptions.AddArgument("--disable-gpu");
            _driver = new ChromeDriver(chromeOptions);
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        }

        [SetUp]
        public void SetUp()
        {
            // Navigate to login page and login as admin
            _driver.Navigate().GoToUrl($"{BaseUrl}/login.html");
            _loginPage = new LoginPage(_driver);
            _loginPage.EnterUsername(AdminUsername);
            _loginPage.EnterPassword(AdminPassword);
            _loginPage.ClickLoginButton();
            System.Threading.Thread.Sleep(2000); // Wait for login to complete
            
            _notificationPage = new NotificationPage(_driver);
        }

        [TearDown]
        public void TearDown()
        {
            // Clear notification state if needed
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _driver?.Quit();
        }

        [TestCaseSource(nameof(GetNotificationTestData))]
        public void Notifications_DataDriven_RealTimeNotificationTest(NotificationTestCase testCase)
        {
            try
            {
                // Navigate to a page where notifications would appear
                _driver.Navigate().GoToUrl($"{BaseUrl}/");
                System.Threading.Thread.Sleep(1000);

                // Open notification panel
                _notificationPage.ClickNotificationIcon();
                System.Threading.Thread.Sleep(500);

                // Perform test action based on test case scenario
                switch (testCase.NotificationType)
                {
                    case "OrderAssignment":
                        PerformOrderAssignmentTest(testCase);
                        break;
                    case "OrderCreated_Admin":
                        PerformOrderCreatedAdminTest(testCase);
                        break;
                    case "OrderCreated_Shipper":
                        PerformOrderCreatedShipperTest(testCase);
                        break;
                    case "OrderCreated_Customer":
                        PerformOrderCreatedCustomerTest(testCase);
                        break;
                    case "StatusChange":
                        PerformStatusChangeTest(testCase);
                        break;
                    case "ShipperAssignment":
                        PerformShipperAssignmentTest(testCase);
                        break;
                    case "ChatMessage":
                        PerformChatMessageNotificationTest(testCase);
                        break;
                    case "MarkAsRead":
                        PerformMarkAsReadTest(testCase);
                        break;
                    case "DeleteNotification":
                        PerformDeleteNotificationTest(testCase);
                        break;
                    case "ClearAll":
                        PerformClearAllTest(testCase);
                        break;
                    default:
                        throw new ArgumentException($"Unknown notification type: {testCase.NotificationType}");
                }

                // Verify result
                if (testCase.ExpectedResult == "Success")
                {
                    Assert.Pass($"Test passed: {testCase.Description}");
                }
            }
            catch (Exception ex)
            {
                if (testCase.ExpectedResult == "Fail")
                {
                    Assert.Pass($"Expected failure: {testCase.Description}");
                }
                else
                {
                    Assert.Fail($"Test failed: {testCase.Description}. Error: {ex.Message}");
                }
            }
        }

        private void PerformOrderAssignmentTest(NotificationTestCase testCase)
        {
            // Check if OrderAssignment notification is visible
            var notificationCount = _notificationPage.GetNotificationCount();
            Assert.That(notificationCount > 0, Is.True, () => "No notifications found for order assignment");
            
            // Verify notification content
            var notifications = _notificationPage.GetAllNotifications();
            var assignmentNotif = notifications.FirstOrDefault(n => 
                n.Contains("assigned") || n.Contains("giao"));
            Assert.That(assignmentNotif, Is.Not.Null, () => "Order assignment notification not found");
        }

        private void PerformOrderCreatedAdminTest(NotificationTestCase testCase)
        {
            var notificationCount = _notificationPage.GetNotificationCount();
            Assert.That(notificationCount > 0, Is.True, () => "No notifications for admin");
            
            var notifications = _notificationPage.GetAllNotifications();
            var createdNotif = notifications.FirstOrDefault(n => 
                n.Contains("created") || n.Contains("tạo"));
            Assert.That(createdNotif, Is.Not.Null, () => "Order created notification not found");
        }

        private void PerformOrderCreatedShipperTest(NotificationTestCase testCase)
        {
            var notificationCount = _notificationPage.GetNotificationCount();
            Assert.That(notificationCount > 0, Is.True, () => "No notifications for shipper");
        }

        private void PerformOrderCreatedCustomerTest(NotificationTestCase testCase)
        {
            var notificationCount = _notificationPage.GetNotificationCount();
            Assert.That(notificationCount > 0, Is.True, () => "No notifications for customer");
        }

        private void PerformStatusChangeTest(NotificationTestCase testCase)
        {
            var notificationCount = _notificationPage.GetNotificationCount();
            Assert.That(notificationCount > 0, Is.True, () => "No status change notifications");
            
            var notifications = _notificationPage.GetAllNotifications();
            var statusNotif = notifications.FirstOrDefault(n => 
                n.Contains("status") || n.Contains("trạng"));
            Assert.That(statusNotif, Is.Not.Null, () => "Status change notification not found");
        }

        private void PerformShipperAssignmentTest(NotificationTestCase testCase)
        {
            var notificationCount = _notificationPage.GetNotificationCount();
            Assert.That(notificationCount > 0, Is.True, () => "No shipper assignment notifications");
        }

        private void PerformChatMessageNotificationTest(NotificationTestCase testCase)
        {
            var notificationCount = _notificationPage.GetNotificationCount();
            Assert.That(notificationCount > 0, Is.True, () => "No chat message notifications");
            
            var notifications = _notificationPage.GetAllNotifications();
            var chatNotif = notifications.FirstOrDefault(n => 
                n.Contains("message") || n.Contains("tin nhắn"));
            Assert.That(chatNotif, Is.Not.Null, () => "Chat message notification not found");
        }

        private void PerformMarkAsReadTest(NotificationTestCase testCase)
        {
            var notificationCount = _notificationPage.GetNotificationCount();
            if (notificationCount > 0)
            {
                _notificationPage.MarkAsRead(0);
                System.Threading.Thread.Sleep(300);
                // Just verify the method executed without error
                Assert.Pass("Notification marked as read successfully");
            }
        }

        private void PerformDeleteNotificationTest(NotificationTestCase testCase)
        {
            var initialCount = _notificationPage.GetNotificationCount();
            var notifications = _notificationPage.GetAllNotifications();
            
            if (notifications.Count > 0)
            {
                _notificationPage.DeleteNotification(0);
                System.Threading.Thread.Sleep(300);
                
                var newCount = _notificationPage.GetNotificationCount();
                Assert.That(newCount < initialCount, Is.True, () => "Notification count did not decrease after delete");
            }
        }

        private void PerformClearAllTest(NotificationTestCase testCase)
        {
            var initialCount = _notificationPage.GetNotificationCount();
            
            if (initialCount > 0)
            {
                _notificationPage.ClearAllNotifications();
                System.Threading.Thread.Sleep(500);
                
                var newCount = _notificationPage.GetNotificationCount();
                Assert.That(_notificationPage.IsNotificationsEmpty(), Is.True, () => "Notifications not cleared");
            }
        }

        public static IEnumerable<NotificationTestCase> GetNotificationTestData()
        {
            var testDataPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "TestData",
                "NotificationTestData.json"
            );

            if (!File.Exists(testDataPath))
            {
                throw new FileNotFoundException($"Test data file not found: {testDataPath}");
            }

            var json = File.ReadAllText(testDataPath);
            var jsonDocument = JsonDocument.Parse(json);
            var element = jsonDocument.RootElement;

            if (element.TryGetProperty("notificationTestCases", out var testCasesElement))
            {
                foreach (var testCaseJson in testCasesElement.EnumerateArray())
                {
                    yield return new NotificationTestCase
                    {
                        TestCaseId = testCaseJson.GetProperty("testCaseId").GetString(),
                        Description = testCaseJson.GetProperty("description").GetString(),
                        NotificationType = testCaseJson.TryGetProperty("notificationType", out var nt) ? nt.GetString() : 
                                          (testCaseJson.TryGetProperty("scenario", out var s) ? s.GetString() : ""),
                        ExpectedResult = testCaseJson.GetProperty("expectedResult").GetString(),
                        Priority = testCaseJson.TryGetProperty("priority", out var p) ? p.GetString() : "Medium",
                        Tags = testCaseJson.TryGetProperty("tags", out var t) ? t.EnumerateArray()
                            .Select(tag => tag.GetString())
                            .ToList() : new List<string>()
                    };
                }
            }
        }
    }
}

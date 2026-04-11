using NUnit.Framework;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TestSelenium.Utilities;

namespace TestSelenium.TestCase
{
    /// <summary>
    /// BaseTest - Base class cho tất cả test classes
    /// Chứa Setup/Teardown, test data initialization, configuration
    /// </summary>
    [TimeoutAttribute(300000)] // 5 minutes timeout
    public class BaseTest
    {
        protected IWebDriver driver;
        protected string baseUrl;
        protected string testDataPath;
        
        // Static để track results của mỗi test class
        private static string _currentTestClass = null;
        private static List<ReportHelper.TestResult> _currentClassResults = new List<ReportHelper.TestResult>();
        private static Stopwatch _testTimer;

        [SetUp]
        public virtual void Setup()
        {
            // Start test timer
            _testTimer = Stopwatch.StartNew();

            // Initialize WebDriver
            driver = DriverFactory.CreateDriver();
            
            // Set base URL (có thể cấu hình từ config file)
            baseUrl = "http://localhost:5221";
            
            // Set test data path
            testDataPath = System.IO.Path.Combine(
                System.IO.Directory.GetCurrentDirectory(),
                "TestData"
            );

            // Maximize window
            driver.Manage().Window.Maximize();

            // Set implicit wait (10 seconds default)
            driver.Manage().Timeouts().ImplicitWait = System.TimeSpan.FromSeconds(10);
        }

        [TearDown]
        public virtual void TearDown()
        {
            // Stop timer
            _testTimer.Stop();
            
            // Cleanup after each test
            DriverFactory.DeleteAllCookies();
            
            string screenshotPath = null;
            if (TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed)
            {
                // Capture screenshot on failure
                screenshotPath = CaptureScreenshot(TestContext.CurrentContext.Test.Name);
            }

            // Ghi kết quả vào report
            RecordTestResult(screenshotPath);

            // Quit driver
            DriverFactory.QuitDriver();
        }

        [OneTimeTearDown]
        public virtual void OneTimeCleanUp()
        {
            // Generate báo cáo khi test class xong
            GenerateFinalClassReport();
        }

        /// <summary>
        /// Capture screenshot on failure
        /// Lưu vào folder Screenshots
        /// </summary>
        protected string CaptureScreenshot(string testName)
        {
            try
            {
                var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                // Navigate từ bin/Debug/net8.0 lên project root
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string projectRoot = System.IO.Path.Combine(baseDir, "..", "..", "..");
                projectRoot = System.IO.Path.GetFullPath(projectRoot);
                
                string screenshotDir = System.IO.Path.Combine(projectRoot, "Screenshots");
                string screenshotPath = System.IO.Path.Combine(
                    screenshotDir,
                    $"{testName}_{System.DateTime.Now:yyyyMMdd_HHmmss}.png"
                );

                // Create directory if not exists
                if (!System.IO.Directory.Exists(screenshotDir))
                    System.IO.Directory.CreateDirectory(screenshotDir);

                screenshot.SaveAsFile(screenshotPath);
                TestContext.WriteLine($"Screenshot saved to: {screenshotPath}");
                return screenshotPath;
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Error capturing screenshot: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Ghi kết quả test vào report
        /// Auto-generate báo cáo khi test class thay đổi
        /// </summary>
        private void RecordTestResult(string screenshotPath)
        {
            try
            {
                var testContext = TestContext.CurrentContext;
                var status = testContext.Result.Outcome.Status.ToString();
                var testName = testContext.Test.Name;
                string currentClass = this.GetType().Name;  // Lấy tên class hiện tại

                // Nếu class đã thay đổi, generate báo cáo cho class trước (WIP)
                if (_currentTestClass != null && _currentTestClass != currentClass)
                {
                    // Generate report cho class trước
                    try
                    {
                        var reportHelper = new ReportHelper();
                        reportHelper.GenerateClassReport(_currentTestClass, new List<ReportHelper.TestResult>(_currentClassResults));
                    }
                    catch (Exception ex)
                    {
                        TestContext.WriteLine($"[REPORT ERROR] Lỗi generate báo cáo {_currentTestClass}: {ex.Message}");
                    }
                    _currentClassResults.Clear();
                }

                _currentTestClass = currentClass;

                var result = new ReportHelper.TestResult
                {
                    TestCaseId = testContext.Test.MethodName,
                    TestName = testName,
                    Status = status,
                    DurationMs = _testTimer.ElapsedMilliseconds,
                    ErrorMessage = testContext.Result.Message,
                    ScreenshotPath = screenshotPath,
                    Timestamp = DateTime.Now
                };

                _currentClassResults.Add(result);
                TestContext.WriteLine($"[REPORT] Kết quả test recorded: {status} ({_testTimer.ElapsedMilliseconds}ms)");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"[REPORT ERROR] Lỗi ghi report: {ex.Message}");
            }
        }

        /// <summary>
        /// Generate báo cáo cho class cuối cùng (gọi từ OneTimeTearDown)
        /// </summary>
        public static void GenerateFinalClassReport()
        {
            if (_currentTestClass != null && _currentClassResults.Count > 0)
            {
                try
                {
                    var reportHelper = new ReportHelper();
                    string reportPath = reportHelper.GenerateClassReport(_currentTestClass, new List<ReportHelper.TestResult>(_currentClassResults));
                    if (!string.IsNullOrEmpty(reportPath))
                    {
                        Console.WriteLine($"✅ Báo cáo cuối cùng {_currentTestClass} đã lưu: {reportPath}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Lỗi generate báo cáo {_currentTestClass}: {ex.Message}");
                }
                finally
                {
                    _currentClassResults.Clear();
                    _currentTestClass = null;
                }
            }
        }

        /// <summary>
        /// Wait for element to be visible
        /// </summary>
        protected void WaitForElement(By locator, int timeoutSeconds = 10)
        {
            int attempts = 0;
            int maxAttempts = timeoutSeconds * 2; // 5 attempts per second
            while (attempts < maxAttempts)
            {
                try
                {
                    var element = driver.FindElement(locator);
                    if (element.Displayed)
                        return;
                }
                catch
                {
                }
                System.Threading.Thread.Sleep(500);
                attempts++;
            }
        }

        /// <summary>
        /// Delay (for demo/debugging purposes)
        /// </summary>
        protected void Delay(int milliseconds)
        {
            System.Threading.Thread.Sleep(milliseconds);
        }

        /// <summary>
        /// Assert element is displayed
        /// </summary>
        protected void AssertElementDisplayed(By locator, string message = "Element should be displayed")
        {
            bool isDisplayed = false;
            try
            {
                isDisplayed = driver.FindElement(locator).Displayed;
            }
            catch (NoSuchElementException)
            {
                isDisplayed = false;
            }

            Assert.That(isDisplayed, Is.True, message);
        }

        /// <summary>
        /// Assert element text contains
        /// </summary>
        protected void AssertElementTextContains(By locator, string expectedText)
        {
            string actualText = driver.FindElement(locator).Text;
            Assert.That(actualText, Does.Contain(expectedText), 
                $"Element text should contain '{expectedText}', but got '{actualText}'");
        }

        /// <summary>
        /// Assert current URL contains
        /// </summary>
        protected void AssertUrlContains(string expectedUrl)
        {
            string currentUrl = driver.Url;
            Assert.That(currentUrl, Does.Contain(expectedUrl),
                $"URL should contain '{expectedUrl}', but got '{currentUrl}'");
        }

        /// <summary>
        /// Assert page title
        /// </summary>
        protected void AssertPageTitle(string expectedTitle)
        {
            Assert.That(driver.Title, Does.Contain(expectedTitle),
                $"Page title should contain '{expectedTitle}'");
        }

        /// <summary>
        /// Clear cookies and navigate to base URL
        /// Use for pre-condition: logged out state
        /// </summary>
        protected void ClearStateAndNavigateToBase()
        {
            DriverFactory.DeleteAllCookies();
            driver.Navigate().GoToUrl(baseUrl);
            Delay(500);
        }

        /// <summary>
        /// Login user before test
        /// Pre-condition helper
        /// </summary>
        protected void LoginBefore(string email, string password)
        {
            var loginPage = new Pages.LoginPage(driver);
            loginPage.PerformLogin(baseUrl, email, password);
            Delay(1000);
        }

        /// <summary>
        /// Logout user after test
        /// </summary>
        protected void LogoutAfter()
        {
            var loginPage = new Pages.LoginPage(driver);
            loginPage.Logout();
            Delay(500);
        }
    }
}

using OpenQA.Selenium;
using System;
using System.IO;

namespace TestSelenium.Utilities
{
    /// <summary>
    /// ScreenshotHelper - Hỗ trợ chụp ảnh lỗi và lưu trữ bằng chứng
    /// </summary>
    public class ScreenshotHelper
    {
        private readonly IWebDriver _driver;
        private readonly string _screenshotPath;

        public ScreenshotHelper(IWebDriver driver)
        {
            _driver = driver;
            _screenshotPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Screenshots"
            );

            // Tạo thư mục nếu chưa tồn tại
            if (!Directory.Exists(_screenshotPath))
            {
                Directory.CreateDirectory(_screenshotPath);
            }
        }

        /// <summary>
        /// Chụp ảnh lỗi và lưu vào Screenshots folder
        /// </summary>
        /// <param name="testName">Tên test case</param>
        /// <param name="reason">Lý do chụp ảnh (lỗi, xác nhận, v.v.)</param>
        /// <returns>Đường dẫn file ảnh</returns>
        public string CaptureScreenshot(string testName, string reason = "Error")
        {
            try
            {
                var screenshot = ((ITakesScreenshot)_driver).GetScreenshot();
                string fileName = $"{testName}_{reason}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                string filePath = Path.Combine(_screenshotPath, fileName);

                screenshot.SaveAsFile(filePath);
                Console.WriteLine($"[SCREENSHOT] Lưu ảnh: {filePath}");
                return filePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SCREENSHOT ERROR] Không thể chụp ảnh: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Lưu HTML page source khi test fail
        /// </summary>
        /// <param name="testName">Tên test case</param>
        /// <returns>Đường dẫn file HTML</returns>
        public string SavePageSource(string testName)
        {
            try
            {
                string fileName = $"{testName}_Source_{DateTime.Now:yyyyMMdd_HHmmss}.html";
                string filePath = Path.Combine(_screenshotPath, fileName);
                string pageSource = _driver.PageSource;

                File.WriteAllText(filePath, pageSource);
                Console.WriteLine($"[PAGE SOURCE] Lưu HTML: {filePath}");
                return filePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PAGE SOURCE ERROR] Không thể lưu HTML: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Lưu browser console logs
        /// </summary>
        /// <param name="testName">Tên test case</param>
        /// <returns>Đường dẫn file logs</returns>
        public string SaveBrowserLogs(string testName)
        {
            try
            {
                var logEntries = _driver.Manage().Logs.GetLog(LogType.Browser);
                string fileName = $"{testName}_BrowserLogs_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                string filePath = Path.Combine(_screenshotPath, fileName);

                using (var writer = new StreamWriter(filePath))
                {
                    foreach (var entry in logEntries)
                    {
                        writer.WriteLine($"[{entry.Level}] {entry.Timestamp}: {entry.Message}");
                    }
                }

                Console.WriteLine($"[BROWSER LOGS] Lưu logs: {filePath}");
                return filePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BROWSER LOGS ERROR] Không thể lưu logs: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Lưu toàn bộ bằng chứng khi test fail
        /// Bao gồm: ảnh chụp, HTML source, browser logs
        /// </summary>
        /// <param name="testName">Tên test case</param>
        /// <param name="exception">Exception nếu có</param>
        public void CaptureEvidenceOnFailure(string testName, Exception exception = null)
        {
            Console.WriteLine($"\n[EVIDENCE CAPTURE] Chụp bằng chứng cho: {testName}");
            
            // Chụp ảnh lỗi
            CaptureScreenshot(testName, "Failure");
            
            // Lưu HTML source
            SavePageSource(testName);
            
            // Lưu browser logs
            SaveBrowserLogs(testName);
            
            // Lưu exception nếu có
            if (exception != null)
            {
                SaveExceptionLog(testName, exception);
            }

            Console.WriteLine($"[EVIDENCE CAPTURE] Hoàn thành\n");
        }

        /// <summary>
        /// Lưu exception log
        /// </summary>
        private void SaveExceptionLog(string testName, Exception exception)
        {
            try
            {
                string fileName = $"{testName}_Exception_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                string filePath = Path.Combine(_screenshotPath, fileName);

                using (var writer = new StreamWriter(filePath))
                {
                    writer.WriteLine($"Test: {testName}");
                    writer.WriteLine($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    writer.WriteLine($"Exception: {exception.GetType().Name}");
                    writer.WriteLine($"Message: {exception.Message}");
                    writer.WriteLine($"\nStack Trace:\n{exception.StackTrace}");
                }

                Console.WriteLine($"[EXCEPTION LOG] Lưu exception: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EXCEPTION LOG ERROR] {ex.Message}");
            }
        }

        /// <summary>
        /// Tạo thư mục con theo ngày
        /// </summary>
        public string GetDailyScreenshotFolder()
        {
            string folderName = DateTime.Now.ToString("yyyy-MM-dd");
            string folderPath = Path.Combine(_screenshotPath, folderName);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            return folderPath;
        }

        /// <summary>
        /// Xóa các ảnh cũ hơn số ngày quy định
        /// </summary>
        /// <param name="daysToKeep">Số ngày muốn giữ lại (mặc định 7 ngày)</param>
        public void CleanOldScreenshots(int daysToKeep = 7)
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
                var files = Directory.GetFiles(_screenshotPath, "*.png");

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        File.Delete(file);
                        Console.WriteLine($"[CLEANUP] Xóa ảnh cũ: {file}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CLEANUP ERROR] {ex.Message}");
            }
        }
    }
}

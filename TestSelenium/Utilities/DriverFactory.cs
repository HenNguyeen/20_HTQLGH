using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;

namespace TestSelenium.Utilities
{
    /// <summary>
    /// DriverFactory - Quản lý WebDriver instance
    /// Khởi tạo và đóng ChromeDriver
    /// </summary>
    public static class DriverFactory
    {
        private static IWebDriver driver;

        /// <summary>
        /// Tạo WebDriver instance (ChromeDriver)
        /// </summary>
        public static IWebDriver CreateDriver()
        {
            if (driver == null)
            {
                // Cũ lại: Chrome driver từ C:\WebDriver\chromedriver.exe
                // Hoặc dùng default system PATH
                var options = new ChromeOptions();
                options.AddArgument("--start-maximized");
                options.AddArgument("--disable-blink-features=AutomationControlled");
                options.AddArgument("disable-blink-features=AutomationControlled");
                options.AddExcludedArgument("enable-automation");
                
                // Uncomment để chạy headless (không mở GUI)
                // options.AddArgument("--headless");

                driver = new ChromeDriver(options);
            }
            return driver;
        }

        /// <summary>
        /// Lấy driver instance hiện tại
        /// </summary>
        public static IWebDriver GetDriver()
        {
            if (driver == null)
                throw new Exception("Driver chưa được khởi tạo. Gọi CreateDriver() trước.");
            return driver;
        }

        /// <summary>
        /// Đóng driver
        /// </summary>
        public static void QuitDriver()
        {
            if (driver != null)
            {
                driver.Quit();
                driver = null;
            }
        }

        /// <summary>
        /// Xoá cookies
        /// </summary>
        public static void DeleteAllCookies()
        {
            if (driver != null)
            {
                driver.Manage().Cookies.DeleteAllCookies();
            }
        }

        /// <summary>
        /// Refresh page
        /// </summary>
        public static void RefreshPage()
        {
            if (driver != null)
            {
                driver.Navigate().Refresh();
            }
        }
    }
}

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
                var options = new ChromeOptions();
                options.AddArgument("--start-maximized");
                options.AddArgument("--disable-blink-features=AutomationControlled");
                options.AddExcludedArgument("enable-automation");
                
                // Disable loading external resources to prevent hanging
                options.AddArgument("--disable-extensions");
                options.AddArgument("--no-first-run");
                options.AddArgument("--no-default-browser-check");
                options.AddArgument("--disable-popup-blocking");
                
                // Better performance for Selenium
                options.AddArgument("--disable-component-extensions-with-background-pages");
                options.AddArgument("--disable-default-apps");
                options.AddArgument("--disable-plugins");
                
                // Uncomment để chạy headless (không mở GUI)
                // options.AddArgument("--headless");

                driver = new ChromeDriver(options);
                
                // Set page load timeout to prevent hanging
                driver.Manage().Timeouts().PageLoad = System.TimeSpan.FromSeconds(30);
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

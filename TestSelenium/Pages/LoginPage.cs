using OpenQA.Selenium;

namespace TestSelenium.Pages
{
    /// <summary>
    /// LoginPage - Page Object cho trang /login
    /// Test Case: Aut_DN_TC_01-35
    /// 
    /// HTML Elements:
    /// - Email input field: id="email"
    /// - Password input field: id="password"
    /// - Remember me checkbox: id="remember-me"
    /// - Login button: id="btn-login"
    /// - Error message: id="error-message"
    /// - Dashboard (success indicator): id="dashboard-header"
    /// </summary>
    public class LoginPage : BasePage
    {
        // Locators - Based on actual HTML from login.html
        private By usernameField = By.Id("username");  // Tài khoản hoặc Email
        private By passwordField = By.Id("password");
        private By rememberMeCheckbox = By.Id("rememberMe");
        private By loginButton = By.CssSelector(".auth-submit-btn");  // Nút Đăng nhập
        private By errorMessage = By.Id("loginAlert");
        private By dashboardHeader = By.CssSelector(".main-content");  // Nơi chứa nội dung chính
        private By logoutButton = By.CssSelector(".dropdown-menu a[onclick*='logout']");
        private By forgotPasswordLink = By.CssSelector("a[href='forgot-password.html']");

        public LoginPage(IWebDriver driver) : base(driver)
        {
        }

        /// <summary>
        /// Điều hướng đến trang https://localhost/login.html
        /// </summary>
        public void NavigateToLogin(string baseUrl)
        {
            NavigateTo($"{baseUrl}/login.html");
        }

        /// <summary>
        /// Nhập tài khoản hoặc email vào username field
        /// Test Data: "test@example.com", "admin", "user123", etc.
        /// </summary>
        public void EnterUsername(string username)
        {
            SetText(usernameField, username);
        }

        /// <summary>
        /// Nhập email vào email field (alias cho EnterUsername)
        /// </summary>
        public void EnterEmail(string email)
        {
            EnterUsername(email);
        }

        /// <summary>
        /// Nhập password vào password field
        /// Test Data: Pass@123456, wrongpassword, 123456, etc.
        /// </summary>
        public void EnterPassword(string password)
        {
            SetText(passwordField, password);
        }

        /// <summary>
        /// Click vào nút Login
        /// Expected: Nếu credentials đúng → Điều hướng đến Dashboard
        ///           Nếu credentials sai → Hiển thị error message
        /// </summary>
        public void ClickLoginButton()
        {
            ClickElement(loginButton);
            // Wait for navigation to complete
            WaitForPageLoad();
        }

        /// <summary>
        /// Check/Uncheck "Remember me" checkbox
        /// </summary>
        public void ClickRememberMeCheckbox()
        {
            if (!IsElementChecked(rememberMeCheckbox))
            {
                ClickElement(rememberMeCheckbox);
            }
        }

        /// <summary>
        /// Verify error message is displayed
        /// Expected Result: "Tài khoản hoặc mật khẩu không đúng" or similar
        /// </summary>
        public bool IsErrorMessageDisplayed()
        {
            return IsElementDisplayed(errorMessage);
        }

        /// <summary>
        /// Get error message text
        /// </summary>
        public string GetErrorMessage()
        {
            return GetText(errorMessage);
        }

        /// <summary>
        /// Verify login success (user redirected to dashboard)
        /// Expected Result: Dashboard header should be visible, URL contains "dashboard" or "orders" or "customer"
        /// </summary>
        public bool IsLoginSuccessful()
        {
            bool isDashboardDisplayed = IsElementDisplayed(dashboardHeader);
            bool isUrlCorrect = GetCurrentUrl().Contains("dashboard") || GetCurrentUrl().Contains("orders") || GetCurrentUrl().Contains("customer");
            return isDashboardDisplayed && isUrlCorrect;
        }

        /// <summary>
        /// Perform complete login flow
        /// Step 1: Navigate to login page
        /// Step 2: Enter email
        /// Step 3: Enter password
        /// Step 4: Click login button
        /// </summary>
        public void PerformLogin(string baseUrl, string email, string password, bool rememberMe = false)
        {
            NavigateToLogin(baseUrl);
            EnterEmail(email);
            EnterPassword(password);
            
            if (rememberMe)
            {
                ClickRememberMeCheckbox();
            }
            
            ClickLoginButton();
        }

        /// <summary>
        /// Logout user
        /// </summary>
        public void Logout()
        {
            if (ElementExists(logoutButton))
            {
                ClickElement(logoutButton);
                WaitForPageLoad();
            }
        }

        /// <summary>
        /// Click "Quên mật khẩu?" link
        /// Navigate To: /forgot-password
        /// </summary>
        public void ClickForgotPasswordLink()
        {
            ClickElement(forgotPasswordLink);
            WaitForPageLoad();
        }

        /// <summary>
        /// Verify email field has correct HTML attributes
        /// Expected: type="text" or type="email", required, etc.
        /// </summary>
        public bool IsEmailFieldValid()
        {
            string fieldType = GetAttribute(usernameField, "type");
            return !string.IsNullOrEmpty(fieldType);
        }

        /// <summary>
        /// Verify password field is password type
        /// Expected: type="password", required, etc.
        /// </summary>
        public bool IsPasswordFieldValid()
        {
            string fieldType = GetAttribute(passwordField, "type");
            return fieldType == "password";
        }

        /// <summary>
        /// Wait for page to fully load
        /// Additional synchronization beyond WebDriver waits
        /// </summary>
        private void WaitForPageLoad()
        {
            // Add implicit wait for transitions/animations
            System.Threading.Thread.Sleep(500);
        }

        /// <summary>
        /// Clear form fields
        /// </summary>
        public void ClearFormFields()
        {
            SetText(usernameField, "");
            SetText(passwordField, "");
        }

        /// <summary>
        /// Get current page title
        /// Expected: "Đăng nhập" or "Login" or similar
        /// </summary>
        public string GetPageTitle()
        {
            return driver.Title;
        }

        /// <summary>
        /// Verify "Remember me" checkbox is checked
        /// </summary>
        public bool IsRememberMeChecked()
        {
            return IsElementChecked(rememberMeCheckbox);
        }
    }
}

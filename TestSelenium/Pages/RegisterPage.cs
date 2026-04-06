using OpenQA.Selenium;

namespace TestSelenium.Pages
{
    /// <summary>
    /// RegisterPage - Page Object cho trang /register
    /// Test Case: Aut_DK_TC_01-36
    /// 
    /// HTML Elements:
    /// - Full Name: id="fullName"
    /// - Email: id="email"
    /// - Phone Number: id="phoneNumber"
    /// - Username: id="username"
    /// - Password: id="password"
    /// - Confirm Password: id="confirmPassword"
    /// - Accept Terms: id="acceptTerms"
    /// </summary>
    public class RegisterPage : BasePage
    {
        // Locators - Based on actual HTML from register.html
        private By fullNameField = By.Id("fullName");
        private By emailField = By.Id("email");
       private By phoneNumberField = By.Id("phoneNumber");
        private By usernameField = By.Id("username");
        private By passwordField = By.Id("password");
        private By confirmPasswordField = By.Id("confirmPassword");
        private By acceptTermsCheckbox = By.Id("acceptTerms");
        private By registerButton = By.CssSelector(".auth-submit-btn");
        private By successMessage = By.Id("registerAlert");
        private By errorMessage = By.Id("registerAlert");
        private By loginLink = By.CssSelector("a[href='login.html']");
        private By usernameErrorMessage = By.Id("usernameError");
        private By emailErrorMessage = By.Id("emailError");
        private By passwordErrorMessage = By.Id("passwordError");

        public RegisterPage(IWebDriver driver) : base(driver)
        {
        }

        /// <summary>
        /// Điều hướng đến trang https://localhost/register.html
        /// </summary>
        public void NavigateToRegister(string baseUrl)
        {
            NavigateTo($"{baseUrl}/register.html");
        }

        /// <summary>
        /// Nhập username (tên đăng nhập)
        /// Validation: Min 5 chars, pattern ^[a-zA-Z0-9_]{5,}$
        /// Test Data: "nguyenvana", "user_123", "testuser01", etc.
        /// </summary>
        public void EnterUsername(string username)
        {
            SetText(usernameField, username);
        }

        /// <summary>
        /// Nhập email
        /// Validation: Valid email format
        /// Test Data: "nguyenvana@example.com", "test@gmail.com", "invalid.email", etc.
        /// </summary>
        public void EnterEmail(string email)
        {
            SetText(emailField, email);
        }

        /// <summary>
        /// Nhập phone number
        /// </summary>
        public void EnterPhoneNumber(string phone)
        {
            SetText(phoneNumberField, phone);
        }

        /// <summary>
        /// Nhập full name
        /// </summary>
        public void EnterFullName(string fullName)
        {
            SetText(fullNameField, fullName);
        }

        /// <summary>
        /// Nhập password
        /// Validation: Min 8 chars, uppercase + lowercase + digit + special char
        /// Pattern: ^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$
        /// Test Data: "Pass@123456", "MyPass@1", "123456", "password", etc.
        /// </summary>
        public void EnterPassword(string password)
        {
            SetText(passwordField, password);
        }

        /// <summary>
        /// Nhập password confirm
        /// Validation: Phải match với password field
        /// </summary>
        public void EnterConfirmPassword(string password)
        {
            SetText(confirmPasswordField, password);
        }

        /// <summary>
        /// Check "Tôi đồng ý với điều khoản" checkbox
        /// </summary>
        public void CheckAcceptTerms()
        {
            if (!IsElementChecked(acceptTermsCheckbox))
            {
                ClickElement(acceptTermsCheckbox);
            }
        }

        /// <summary>
        /// Uncheck "Tôi đồng ý với điều khoản" checkbox
        /// </summary>
        public void UncheckAcceptTerms()
        {
            if (IsElementChecked(acceptTermsCheckbox))
            {
                ClickElement(acceptTermsCheckbox);
            }
        }

        /// <summary>
        /// Click "Đăng ký" button
        /// Expected: 
        ///   - Success: Redirect to login page, show success message
        ///   - Failure: Show error message at top or below fields
        /// </summary>
        public void ClickRegisterButton()
        {
            ClickElement(registerButton);
            WaitForPageLoad();
        }

        /// <summary>
        /// Perform complete registration flow
        /// Step 1: Navigate to register page
        /// Step 2: Enter full name
        /// Step 3: Enter email
        /// Step 4: Enter phone number
        /// Step 5: Enter username
        /// Step 6: Enter password
        /// Step 7: Enter confirm password
        /// Step 8: Check accept terms
        /// Step 9: Click register button
        /// </summary>
        public void PerformRegistration(
            string baseUrl,
            string fullName,
            string email,
            string phoneNumber,
            string username,
            string password,
            string confirmPassword
        )
        {
            NavigateToRegister(baseUrl);
            EnterFullName(fullName);
            EnterEmail(email);
            EnterPhoneNumber(phoneNumber);
            EnterUsername(username);
            EnterPassword(password);
            EnterConfirmPassword(confirmPassword);
            CheckAcceptTerms();
            ClickRegisterButton();
        }

        /// <summary>
        /// Check if registration was successful
        /// Expected Result: Redirect to login page or show success message
        /// </summary>
        public bool IsRegistrationSuccessful()
        {
            bool isSuccessMessageDisplayed = IsElementDisplayed(successMessage);
            bool isRedirectedToLogin = GetCurrentUrl().Contains("login");
            
            return isSuccessMessageDisplayed || isRedirectedToLogin;
        }

        /// <summary>
        /// Get success message text
        /// Expected: "Đăng ký thành công! Vui lòng đăng nhập." or similar
        /// </summary>
        public string GetSuccessMessage()
        {
            return GetText(successMessage);
        }

        /// <summary>
        /// Check if error message is displayed
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
        /// Get specific field error message
        /// Field: "username", "email", "password"
        /// </summary>
        public string GetFieldErrorMessage(string fieldName)
        {
            By errorLocator = fieldName.ToLower() switch
            {
                "username" => usernameErrorMessage,
                "email" => emailErrorMessage,
                "password" => passwordErrorMessage,
                _ => errorMessage
            };

            if (IsElementDisplayed(errorLocator))
                return GetText(errorLocator);
            
            return string.Empty;
        }

        /// <summary>
        /// Click "Đăng nhập" link to go back to login page
        /// </summary>
        public void ClickLoginLink()
        {
            ClickElement(loginLink);
            WaitForPageLoad();
        }

        /// <summary>
        /// Verify username field validation
        /// Expected: Min length = 5, only alphanumeric and underscore
        /// </summary>
        public bool IsUsernameFieldValid()
        {
            string minLength = GetAttribute(usernameField, "minlength");
            string pattern = GetAttribute(usernameField, "pattern");
            
            return !string.IsNullOrEmpty(minLength) && minLength == "5" && !string.IsNullOrEmpty(pattern);
        }

        /// <summary>
        /// Verify email field validation
        /// </summary>
        public bool IsEmailFieldValid()
        {
            string fieldType = GetAttribute(emailField, "type");
            return fieldType == "email";
        }

        /// <summary>
        /// Verify password field validation
        /// </summary>
        public bool IsPasswordFieldValid()
        {
            string fieldType = GetAttribute(passwordField, "type");
            return fieldType == "password";
        }

        /// <summary>
        /// Verify confirm password field validation
        /// </summary>
        public bool IsConfirmPasswordFieldValid()
        {
            string fieldType = GetAttribute(confirmPasswordField, "type");
            return fieldType == "password";
        }

        /// <summary>
        /// Clear all form fields
        /// </summary>
        public void ClearAllFields()
        {
            SetText(usernameField, "");
            SetText(emailField, "");
            SetText(passwordField, "");
            SetText(confirmPasswordField, "");
        }

        /// <summary>
        /// Wait for page to fully load
        /// </summary>
        private void WaitForPageLoad()
        {
            System.Threading.Thread.Sleep(500);
        }

        /// <summary>
        /// Get current page title
        /// </summary>
        public string GetPageTitle()
        {
            return driver.Title;
        }

        /// <summary>
        /// Check if "Tôi đồng ý" checkbox is checked
        /// </summary>
        public bool IsAcceptTermsChecked()
        {
            return IsElementChecked(acceptTermsCheckbox);
        }
    }
}

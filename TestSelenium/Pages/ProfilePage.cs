using OpenQA.Selenium;
using System.Collections.Generic;

namespace TestSelenium.Pages
{
    /// <summary>
    /// ProfilePage - Page Object cho Profile Management
    /// </summary>
    public class ProfilePage : BasePage
    {
        // Navigation
        private By profileMenuLink = By.XPath("//a[@href='profile.html']");
        
        // Profile info
        private By userNameField = By.Id("userName");
        private By emailField = By.Id("email");
        private By phoneField = By.Id("phone");
        private By addressField = By.Id("address");
        private By avatarUpload = By.Id("avatarUpload");
        private By updateProfileButton = By.Id("updateProfileBtn");
        
        // Change password
        private By oldPasswordField = By.Id("oldPassword");
        private By newPasswordField = By.Id("newPassword");
        private By confirmPasswordField = By.Id("confirmPassword");
        private By changePasswordButton = By.Id("changePasswordBtn");
        
        // Language settings
        private By languageSelect = By.Id("languageSelect");
        
        // Messages
        private By successAlert = By.CssSelector(".alert-success");
        private By errorAlert = By.CssSelector(".alert-danger");

        public ProfilePage(IWebDriver driver) : base(driver)
        {
        }

        public void ClickProfileMenuLink()
        {
            ClickElement(profileMenuLink);
            System.Threading.Thread.Sleep(1000);
            WaitForPageLoad();
        }

        public void UpdateUserName(string userName)
        {
            SetText(userNameField, userName);
        }

        public void UpdateEmail(string email)
        {
            SetText(emailField, email);
        }

        public void UpdatePhone(string phone)
        {
            SetText(phoneField, phone);
        }

        public void UpdateAddress(string address)
        {
            SetText(addressField, address);
        }

        public void UpdateProfile()
        {
            ClickElement(updateProfileButton);
            System.Threading.Thread.Sleep(500);
        }

        public void ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            SetText(oldPasswordField, oldPassword);
            SetText(newPasswordField, newPassword);
            SetText(confirmPasswordField, confirmPassword);
            ClickElement(changePasswordButton);
            System.Threading.Thread.Sleep(500);
        }

        public void SelectLanguage(string languageCode)
        {
            SelectDropdownByText(languageSelect, languageCode);
            System.Threading.Thread.Sleep(500);
        }

        public void UploadAvatar(string filePath)
        {
            var fileInput = driver.FindElement(avatarUpload);
            fileInput.SendKeys(filePath);
            System.Threading.Thread.Sleep(1000);
        }

        public string GetSuccessMessage()
        {
            try
            {
                return GetText(successAlert);
            }
            catch
            {
                return "";
            }
        }

        public string GetErrorMessage()
        {
            try
            {
                return GetText(errorAlert);
            }
            catch
            {
                return "";
            }
        }

        public string GetUserName()
        {
            return GetText(userNameField);
        }

        public string GetEmail()
        {
            return GetText(emailField);
        }

        public string GetPhone()
        {
            return GetText(phoneField);
        }

        public string GetAddress()
        {
            return GetText(addressField);
        }
    }
}

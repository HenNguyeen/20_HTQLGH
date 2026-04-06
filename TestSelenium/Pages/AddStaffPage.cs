using OpenQA.Selenium;

namespace TestSelenium.Pages
{
    /// <summary>
    /// AddStaffPage - Page Object cho modal thêm nhân viên
    /// Test Case: Sta_ThemNV_TC_01-10+
    /// 
    /// HTML Elements (based on staff.html modal):
    /// - Staff Name: name="fullName"
    /// - Staff Phone: name="phoneNumber"
    /// - Staff Username: name="username"
    /// - Staff Email: name="email"
    /// - Vehicle Type: name="vehicleType" (select)
    /// - Vehicle Plate: name="vehiclePlate"
    /// - Is Available: name="isAvailable" (checkbox)
    /// - Add Staff button: .modal-footer .btn-primary
    /// - Cancel button: .modal-footer .btn-secondary
    /// - Success message: .alert-success
    /// - Error message: .alert-danger
    /// 
    /// NOTE: No address, province/district/ward dropdowns, department, position, role, salary, or profile image in real HTML
    /// </summary>
    public class AddStaffPage : BasePage
    {
        // Navigation locators
        private By staffMenuLink = By.XPath("//a[@href='staff.html']");
        private By openAddStaffModalButton = By.XPath("//button[@data-bs-target='#addStaffModal']");
        private By staffModal = By.CssSelector("#addStaffModal");

        // Form field locators - Based on actual HTML
        private By staffNameField = By.Name("fullName");
        private By staffPhoneField = By.Name("phoneNumber");
        private By usernameField = By.Name("username");
        private By emailField = By.Name("email");
        private By vehicleTypeDropdown = By.Name("vehicleType");
        private By vehiclePlateField = By.Name("vehiclePlate");
        private By isAvailableCheckbox = By.Name("isAvailable");
        private By addStaffButton = By.CssSelector("#addStaffModal .modal-footer .btn-primary");
        private By cancelButton = By.CssSelector("#addStaffModal .modal-footer .btn-secondary");
        private By successMessage = By.CssSelector(".alert-success");
        private By errorMessage = By.CssSelector(".alert-danger");

        public AddStaffPage(IWebDriver driver) : base(driver)
        {
        }

        /// <summary>
        /// Click Staff menu link from navigation to navigate to staff page
        /// Expected: Navigate to staff.html and display staff list
        /// </summary>
        public void ClickStaffMenuLink()
        {
            ClickElement(staffMenuLink);
            System.Threading.Thread.Sleep(1000); // Wait for page load
            WaitForPageLoad();
        }

        /// <summary>
        /// Click "Thêm Nhân Viên" button to open add staff modal
        /// Expected: Modal #addStaffModal is displayed
        /// </summary>
        public void ClickOpenAddStaffModalButton()
        {
            ClickElement(openAddStaffModalButton);
            WaitForElementVisibility(staffModal);
            System.Threading.Thread.Sleep(500); // Wait for modal animation
        }

        /// <summary>
        /// Navigate to staff page (modal is triggered from there)
        /// </summary>
        public void NavigateToAddStaff(string baseUrl)
        {
            NavigateTo($"{baseUrl}/staff.html");
        }

        /// <summary>
        /// Enter staff full name
        /// Test Data: "Nguyễn Văn A", "Trần Thị B"
        /// </summary>
        public void EnterStaffName(string name)
        {
            SetText(staffNameField, name);
        }

        /// <summary>
        /// Enter staff phone number
        /// Test Data: "0912345678", "0987654321"
        /// </summary>
        public void EnterStaffPhone(string phone)
        {
            SetText(staffPhoneField, phone);
        }

        /// <summary>
        /// Enter staff username
        /// Test Data: "user_nguyen_a", "user_tran_b"
        /// </summary>
        public void EnterUsername(string username)
        {
            SetText(usernameField, username);
        }

        /// <summary>
        /// Enter staff email
        /// Test Data: "nguyen.a@shipping.com", "tran.b@shipping.com"
        /// </summary>
        public void EnterEmail(string email)
        {
            SetText(emailField, email);
        }

        /// <summary>
        /// Select vehicle type from dropdown
        /// Test Data: "Xe máy", "Ô tô", "Xe tải"
        /// </summary>
        public void SelectVehicleType(string type)
        {
            SelectDropdownByText(vehicleTypeDropdown, type);
        }

        /// <summary>
        /// Enter vehicle plate/license
        /// Test Data: "TP50A-123.45", "TP50B-678.90"
        /// </summary>
        public void EnterVehiclePlate(string plate)
        {
            SetText(vehiclePlateField, plate);
        }

        /// <summary>
        /// Check is available checkbox
        /// </summary>
        public void CheckIsAvailable()
        {
            var checkbox = WaitForElementVisibility(isAvailableCheckbox);
            if (!checkbox.Selected)
            {
                ClickElement(isAvailableCheckbox);
            }
        }

        /// <summary>
        /// Uncheck is available checkbox
        /// </summary>
        public void UncheckIsAvailable()
        {
            var checkbox = WaitForElementVisibility(isAvailableCheckbox);
            if (checkbox.Selected)
            {
                ClickElement(isAvailableCheckbox);
            }
        }

        /// <summary>
        /// Click Add Staff button
        /// Expected: Close modal and add staff to list
        /// </summary>
        public void ClickAddStaffButton()
        {
            ClickElement(addStaffButton);
            WaitForPageLoad();
        }

        /// <summary>
        /// Click Cancel button
        /// Expected: Close modal without saving
        /// </summary>
        public void ClickCancelButton()
        {
            ClickElement(cancelButton);
            WaitForPageLoad();
        }

        /// <summary>
        /// Perform complete add staff flow
        /// Step 1: Navigate to staff page
        /// Step 2-5: Enter personal info (name, phone, username, email)
        /// Step 6-7: Select vehicle info (type, plate)
        /// Step 8: Check is available checkbox
        /// Step 9: Click add staff button
        /// </summary>
        public void PerformAddStaff(
            string baseUrl,
            string staffName,
            string staffPhone,
            string username,
            string email,
            string vehicleType,
            string vehiclePlate,
            bool isAvailable = true
        )
        {
            NavigateToAddStaff(baseUrl);
            EnterStaffName(staffName);
            EnterStaffPhone(staffPhone);
            EnterUsername(username);
            EnterEmail(email);
            SelectVehicleType(vehicleType);
            EnterVehiclePlate(vehiclePlate);
            if (isAvailable)
                CheckIsAvailable();
            ClickAddStaffButton();
        }

        /// <summary>
        /// Check if staff was added successfully
        /// Expected Result: Success message displayed
        /// </summary>
        public bool IsStaffAddedSuccessfully()
        {
            return IsElementDisplayed(successMessage);
        }

        /// <summary>
        /// Get success message
        /// </summary>
        public string GetSuccessMessage()
        {
            return GetText(successMessage);
        }

        /// <summary>
        /// Get error message
        /// </summary>
        public string GetErrorMessage()
        {
            return GetText(errorMessage);
        }

        /// <summary>
        /// Wait for page to fully load
        /// </summary>
        private void WaitForPageLoad()
        {
            System.Threading.Thread.Sleep(500);
        }
    }
}

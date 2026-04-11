using OpenQA.Selenium;

namespace TestSelenium.Pages
{
    /// <summary>
    /// AddCustomerPage - Page Object cho modal thêm khách hàng
    /// Test Case: Cus_ThemKH_TC_01-36
    /// 
    /// HTML Elements (based on customers.html modal):
    /// - Customer Name: id="fullName"
    /// - Customer Phone: id="phoneNumber"
    /// - Customer Address: id="address"
    /// - Ward: id="ward" (text input)
    /// - District: id="district" (text input)
    /// - City: id="city" (text input)
    /// - Add Customer button: .modal-footer .btn-primary
    /// - Cancel button: .modal-footer .btn-secondary
    /// - Success message: .alert-success
    /// - Error message: .alert-danger
    /// 
    /// NOTE: No email field, no province dropdown, no customer type dropdown in real HTML
    /// </summary>
    public class AddCustomerPage : BasePage
    {
        // Navigation locators
        private By customerMenuLink = By.XPath("//a[@href='customers.html']");
        private By openAddCustomerModalButton = By.Id("btnAddCustomer");
        private By customerModal = By.CssSelector("#customerModal");

        // Form field locators - Based on actual HTML
        private By customerNameField = By.Id("fullName");
        private By customerPhoneField = By.Id("phoneNumber");
        private By emailField = By.Id("email");
        private By customerAddressField = By.Id("address");
        private By wardField = By.Id("ward");
        private By districtField = By.Id("district");
        private By cityField = By.Id("city");
        private By addressTypeField = By.Id("addressType");
        private By bankAccountNumberField = By.Id("bankAccountNumber");
        private By bankAccountNameField = By.Id("bankAccountName");
        private By bankNameField = By.Id("bankName");
        private By bankBranchField = By.Id("bankBranch");
        private By settlementCycleField = By.Id("settlementCycle");
        private By taxCodeField = By.Id("taxCode");
        private By addCustomerButton = By.CssSelector("#customerModal .modal-footer .btn-primary");
        private By cancelButton = By.CssSelector("#customerModal .modal-footer .btn-secondary");
        private By successMessage = By.CssSelector(".alert-success");
        private By errorMessage = By.CssSelector(".alert-danger");

        public AddCustomerPage(IWebDriver driver) : base(driver)
        {
        }

        /// <summary>
        /// Click Customer menu link from navigation to navigate to customers page
        /// Expected: Navigate to customers.html and display customer list
        /// </summary>
        public void ClickCustomerMenuLink()
        {
            ClickElement(customerMenuLink);
            System.Threading.Thread.Sleep(1000); // Wait for page load
            WaitForPageLoad();
        }

        /// <summary>
        /// Click "Thêm Khách Hàng" button to open add customer modal
        /// Expected: Modal #customerModal is displayed
        /// </summary>
        public void ClickOpenAddCustomerModalButton()
        {
            ClickElement(openAddCustomerModalButton);
            WaitForElementVisibility(customerModal);
            System.Threading.Thread.Sleep(500); // Wait for modal animation
        }

        /// <summary>
        /// Navigate to customers page (modal is triggered from there)
        /// </summary>
        public void NavigateToAddCustomer(string baseUrl)
        {
            NavigateTo($"{baseUrl}/customers.html");
        }

        /// <summary>
        /// Enter customer name
        /// Test Data: "Nguyễn Văn An", "Trần Thị B", "Phạm Văn C"
        /// </summary>
        public void EnterCustomerName(string name)
        {
            SetText(customerNameField, name);
        }

        /// <summary>
        /// Enter customer phone
        /// Validation: Vietnamese phone format (10 digits, starts with 0)
        /// Test Data: "0912345678", "0987654321"
        /// </summary>
        public void EnterCustomerPhone(string phone)
        {
            SetText(customerPhoneField, phone);
        }

        /// <summary>
        /// Enter customer address
        /// Test Data: "123 Đường Nguyễn Huệ"
        /// </summary>
        public void EnterCustomerAddress(string address)
        {
            SetText(customerAddressField, address);
        }

        /// <summary>
        /// Enter ward/phường
        /// Test Data: "Phường 1", "Phường 2"
        /// </summary>
        public void EnterWard(string ward)
        {
            SetText(wardField, ward);
        }

        /// <summary>
        /// Enter district/quận
        /// Test Data: "Quận 1", "Quận 2"
        /// </summary>
        public void EnterDistrict(string district)
        {
            SetText(districtField, district);
        }

        /// <summary>
        /// Enter city/thành phố
        /// Test Data: "Hồ Chí Minh", "Hà Nội"
        /// </summary>
        public void EnterCity(string city)
        {
            SetText(cityField, city);
        }

        /// <summary>
        /// Click Add Customer button
        /// </summary>
        public void ClickAddCustomerButton()
        {
            ClickElement(addCustomerButton);
            WaitForPageLoad();
        }

        /// <summary>
        /// Click Cancel button
        /// </summary>
        public void ClickCancelButton()
        {
            ClickElement(cancelButton);
            WaitForPageLoad();
        }

        /// <summary>
        /// Perform complete add customer flow
        /// Step 1: Navigate to customers page
        /// Step 2-7: Enter customer info (name, phone, address, ward, district, city)
        /// Step 8: Click add customer button
        /// </summary>
        public void PerformAddCustomer(
            string baseUrl,
            string customerName,
            string customerPhone,
            string address,
            string ward,
            string district,
            string city
        )
        {
            NavigateToAddCustomer(baseUrl);
            EnterCustomerName(customerName);
            EnterCustomerPhone(customerPhone);
            EnterCustomerAddress(address);
            EnterWard(ward);
            EnterDistrict(district);
            EnterCity(city);
            ClickAddCustomerButton();
        }

        /// <summary>
        /// Check if customer was added successfully
        /// </summary>
        public bool IsCustomerAddedSuccessfully()
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

        /// <summary>
        /// Check if error message is displayed
        /// </summary>
        public bool IsErrorMessageDisplayed()
        {
            return IsElementDisplayed(errorMessage);
        }

        /// <summary>
        /// Select province (convenience method - enters text)
        /// </summary>
        public void SelectProvince(string province)
        {
            EnterCity(province);
        }

        /// <summary>
        /// Select district (convenience method - enters text)
        /// </summary>
        public void SelectDistrict(string district)
        {
            EnterDistrict(district);
        }

        /// <summary>
        /// Select ward (convenience method - enters text)
        /// </summary>
        public void SelectWard(string ward)
        {
            EnterWard(ward);
        }

        /// <summary>
        /// Enter email address
        /// Validation: Format must be valid email (contains @)
        /// Test Data: "tran.huong@email.com", "invalid-email"
        /// </summary>
        public void EnterEmail(string email)
        {
            SetText(emailField, email);
        }

        /// <summary>
        /// Select address type from dropdown
        /// Options: "Kho hàng", "Nhà riêng", "Văn phòng"
        /// </summary>
        public void SelectAddressType(string addressType)
        {
            if (string.IsNullOrEmpty(addressType)) return;
            try
            {
                var selectElement = new OpenQA.Selenium.Support.UI.SelectElement(
                    driver.FindElement(addressTypeField)
                );
                selectElement.SelectByValue(addressType);
            }
            catch
            {
                // Fallback: try by text
                var selectElement = new OpenQA.Selenium.Support.UI.SelectElement(
                    driver.FindElement(addressTypeField)
                );
                selectElement.SelectByText(addressType);
            }
        }

        /// <summary>
        /// Enter bank account number
        /// Test Data: "0123456789012", "0111111111111"
        /// </summary>
        public void EnterBankAccountNumber(string accountNumber)
        {
            SetText(bankAccountNumberField, accountNumber);
        }

        /// <summary>
        /// Enter bank account holder name
        /// Test Data: "Trần Thị Hương", "Công Ty ABC"
        /// </summary>
        public void EnterBankAccountName(string accountName)
        {
            SetText(bankAccountNameField, accountName);
        }

        /// <summary>
        /// Enter bank name
        /// Test Data: "Vietcombank", "VPBank"
        /// </summary>
        public void EnterBankName(string bankName)
        {
            SetText(bankNameField, bankName);
        }

        /// <summary>
        /// Enter bank branch
        /// Test Data: "Chi nhánh Hà Nội", "Chi nhánh TPHCM"
        /// </summary>
        public void EnterBankBranch(string branch)
        {
            SetText(bankBranchField, branch);
        }

        /// <summary>
        /// Select settlement cycle from dropdown
        /// Options: "Daily", "Weekly", "Monthly", "OnDemand", "MinimumBalance"
        /// </summary>
        public void SelectSettlementCycle(string cycle)
        {
            if (string.IsNullOrEmpty(cycle)) return;
            try
            {
                var selectElement = new OpenQA.Selenium.Support.UI.SelectElement(
                    driver.FindElement(settlementCycleField)
                );
                selectElement.SelectByValue(cycle);
            }
            catch
            {
                // Fallback: try by text
                var selectElement = new OpenQA.Selenium.Support.UI.SelectElement(
                    driver.FindElement(settlementCycleField)
                );
                selectElement.SelectByText(cycle);
            }
        }

        /// <summary>
        /// Enter tax code
        /// Test Data: "0123456789", "0111111111"
        /// </summary>
        public void EnterTaxCode(string taxCode)
        {
            SetText(taxCodeField, taxCode);
        }

        /// <summary>
        /// Click Edit button for a specific customer by ID
        /// Parameters: customerId - ID của khách hàng
        /// Expected: Modal mở với dữ liệu khách hàng được tải
        /// </summary>
        public void ClickEditCustomerButton(string customerId)
        {
            By editButton = By.CssSelector($"button[data-action='edit'][data-id='{customerId}']");
            ClickElement(editButton);
            WaitForElementVisibility(customerModal);
            System.Threading.Thread.Sleep(500); // Wait for modal animation
        }

        /// <summary>
        /// Click Delete button for a specific customer by ID
        /// Parameters: customerId - ID của khách hàng
        /// Expected: Browser alert/confirm dialog appears
        /// </summary>
        public void ClickDeleteCustomerButton(string customerId)
        {
            By deleteButton = By.CssSelector($"button[data-action='delete'][data-id='{customerId}']");
            ClickElement(deleteButton);
            System.Threading.Thread.Sleep(500); // Wait for delete confirmation dialog
        }

        /// <summary>
        /// Confirm the delete action by clicking OK on the browser confirm dialog
        /// </summary>
        public void ConfirmDelete()
        {
            try
            {
                var alert = driver.SwitchTo().Alert();
                alert.Accept();
                System.Threading.Thread.Sleep(1000); // Wait for deletion to complete
            }
            catch (NoAlertPresentException)
            {
                // No alert present, document may have already been deleted
            }
        }

        /// <summary>
        /// Cancel the delete action by clicking Cancel on the browser confirm dialog
        /// </summary>
        public void CancelDelete()
        {
            try
            {
                var alert = driver.SwitchTo().Alert();
                alert.Dismiss();
                System.Threading.Thread.Sleep(500); // Wait for dialog to close
            }
            catch (NoAlertPresentException)
            {
                // No alert present
            }
        }

        /// <summary>
        /// Wait for success or error message to appear
        /// Returns true if success message appears, false if error message appears
        /// </summary>
        private bool WaitForMessage()
        {
            int attempts = 0;
            while (attempts < 10)
            {
                if (IsElementDisplayed(successMessage))
                    return true;
                if (IsElementDisplayed(errorMessage))
                    return false;
                System.Threading.Thread.Sleep(500);
                attempts++;
            }
            return false; // Timeout
        }
    }
}

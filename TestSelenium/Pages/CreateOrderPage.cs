using OpenQA.Selenium;

namespace TestSelenium.Pages
{
    /// <summary>
    /// CreateOrderPage - Page Object cho modal tạo đơn hàng
    /// Test Case: Ord_ThemDH_TC_01-40
    /// 
    /// HTML Elements (based on orders.html):
    /// - Customer Name: name="customerName"
    /// - Customer Phone: name="customerPhone"
    /// - Delivery Address: name="deliveryAddress"
    /// - Ward: name="ward"
    /// - District: name="district"
    /// - City: name="city"
    /// - Product Code: name="productCode"
    /// - Package Type: name="packageType"
    /// - Weight: name="weight"
    /// - Distance: name="distance"
    /// - Payment Method: name="paymentMethod"
    /// - Create Order button: click in modal footer
    /// </summary>
    public class CreateOrderPage : BasePage
    {
        // Form field locators - Based on actual HTML
        private By orderCodeField = By.Name("orderCode");
        private By customerNameField = By.Name("customerName");
        private By customerPhoneField = By.Name("customerPhone");
        private By customerEmailField = By.Name("customerEmail");
        private By deliveryAddressField = By.Name("deliveryAddress");
        private By wardField = By.Name("ward");
        private By districtField = By.Name("district");
        private By cityField = By.Name("city");
        private By productCodeField = By.Name("productCode");
        private By packageTypeDropdown = By.Name("packageType");
        private By packageDescriptionField = By.Name("packageDescription");
        private By weightField = By.Name("weight");
        private By distanceField = By.Name("distance");
        private By isFragileCheckbox = By.Name("isFragile");
        private By isValuableCheckbox = By.Name("isValuable");
        private By collectMoneyCheckbox = By.Name("collectMoney");
        private By collectionAmountField = By.Name("collectionAmount");
        private By paymentMethodDropdown = By.Name("paymentMethod");
        private By deliveryTypeDropdown = By.Name("deliveryType");
        private By notesField = By.Name("notes");
        private By createOrderButton = By.CssSelector("#createOrderModal .modal-footer .btn-primary");
        private By cancelButton = By.CssSelector("#createOrderModal .modal-footer .btn-secondary");
        private By successMessage = By.CssSelector(".alert-success");
        private By errorMessage = By.CssSelector(".alert-danger");

        public CreateOrderPage(IWebDriver driver) : base(driver)
        {
        }

        /// <summary>
        /// Navigate to create order modal (should be already on page, just click button)
        /// </summary>
        public void NavigateToCreateOrder(string baseUrl)
        {
            // Modal is on orders page, so navigate there first
            NavigateTo($"{baseUrl}/orders.html");
            // Then trigger modal via button (this is handled in test)
        }

        /// <summary>
        /// Enter customer name
        /// Test Data: "Nguyễn Văn A", "Trần Thị B", "Phạm Văn C"
        /// </summary>
        public void EnterCustomerName(string name)
        {
            SetText(customerNameField, name);
        }

        /// <summary>
        /// Enter customer phone
        /// Validation: Vietnamese phone format (10 digits, starts with 0)
        /// Test Data: "0912345678", "0987654321", invalid: "123456789" (too short)
        /// </summary>
        public void EnterCustomerPhone(string phone)
        {
            SetText(customerPhoneField, phone);
        }

        /// <summary>
        /// Enter delivery address
        /// Test Data: "123 Đường Nguyễn Huệ, Quận 1"
        /// </summary>
        public void EnterDeliveryAddress(string address)
        {
            SetText(deliveryAddressField, address);
        }

        /// <summary>
        /// Enter ward/phường
        /// </summary>
        public void EnterWard(string ward)
        {
            SetText(wardField, ward);
        }

        /// <summary>
        /// Enter district/quận
        /// </summary>
        public void EnterDistrict(string district)
        {
            SetText(districtField, district);
        }

        /// <summary>
        /// Enter city/thành phố
        /// </summary>
        public void EnterCity(string city)
        {
            SetText(cityField, city);
        }

        /// <summary>
        /// Enter product code
        /// </summary>
        public void EnterProductCode(string code)
        {
            SetText(productCodeField, code);
        }

        /// <summary>
        /// Select package type from dropdown
        /// Test Data: "Gói Nhỏ", "Bọc", "Thùng", etc.
        /// </summary>
        public void SelectPackageType(string type)
        {
            SelectDropdownByText(packageTypeDropdown, type);
        }

        /// <summary>
        /// Enter package weight (in kg)
        /// Test Data: "0.5", "1.2", "5", "10"
        /// </summary>
        public void EnterPackageWeight(string weight)
        {
            SetText(weightField, weight);
        }

        /// <summary>
        /// Enter distance (in km)
        /// Test Data: "5", "10", "20"
        /// </summary>
        public void EnterDistance(string distance)
        {
            SetText(distanceField, distance);
        }

        /// <summary>
        /// Select payment method from dropdown
        /// Test Data: "Thanh toán khi nhận" (0), "Chuyển khoản" (1), "Online" (2)
        /// </summary>
        public void SelectPaymentMethod(string method)
        {
            SelectDropdownByText(paymentMethodDropdown, method);
        }

        /// <summary>
        /// Select delivery type from dropdown
        /// Test Data: "Thường", "Nhanh"
        /// </summary>
        public void SelectDeliveryType(string type)
        {
            SelectDropdownByText(deliveryTypeDropdown, type);
        }

        /// <summary>
        /// Click Create Order button
        /// Expected: 
        ///   - Success: Show order code, redirect to order detail page
        ///   - Failure: Show error message
        /// </summary>
        public void ClickCreateOrderButton()
        {
            ClickElement(createOrderButton);
            WaitForPageLoad();
        }

        /// <summary>
        /// Click Cancel button
        /// Expected: Return to orders list page
        /// </summary>
        public void ClickCancelButton()
        {
            ClickElement(cancelButton);
            WaitForPageLoad();
        }

        /// <summary>
        /// Perform complete create order flow
        /// Step 1: Navigate to orders page with create order modal
        /// Step 2-4: Enter customer info (name, phone, address)
        /// Step 5-7: Enter location (ward, district, city)
        /// Step 8-10: Enter package info (product code, type, weight, distance)
        /// Step 11: Select payment method and create order
        /// </summary>
        public void PerformCreateOrder(
            string baseUrl,
            string customerName,
            string customerPhone,
            string address,
            string ward,
            string district,
            string city,
            string productCode,
            string packageType,
            string weight,
            string distance,
            string paymentMethod = "COD",
            string deliveryType = "Normal"
        )
        {
            NavigateToCreateOrder(baseUrl);
            EnterCustomerName(customerName);
            EnterCustomerPhone(customerPhone);
            EnterDeliveryAddress(address);
            EnterWard(ward);
            EnterDistrict(district);
            EnterCity(city);
            EnterProductCode(productCode);
            SelectPackageType(packageType);
            EnterPackageWeight(weight);
            EnterDistance(distance);
            SelectPaymentMethod(paymentMethod);
            SelectDeliveryType(deliveryType);
            ClickCreateOrderButton();
        }

        /// <summary>
        /// Check if order was created successfully
        /// Expected Result: Success message displayed
        /// </summary>
        public bool IsOrderCreatedSuccessfully()
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
        /// Get generated order code
        /// Expected format: DH{yyyyMMddHHmmssfff}{nnn}
        /// </summary>
        public string GetOrderCode()
        {
            return GetText(orderCodeField);
        }

        /// <summary>
        /// Check if error message is displayed
        /// </summary>
        public bool IsErrorMessageDisplayed()
        {
            return IsElementDisplayed(errorMessage);
        }

        /// <summary>
        /// Verify all required fields are present
        /// </summary>
        public bool AreAllFieldsPresent()
        {
            return ElementExists(customerNameField) &&
                   ElementExists(customerPhoneField) &&
                   ElementExists(deliveryAddressField) &&
                   ElementExists(wardField) &&
                   ElementExists(districtField) &&
                   ElementExists(cityField) &&
                   ElementExists(packageTypeDropdown) &&
                   ElementExists(weightField) &&
                   ElementExists(paymentMethodDropdown) &&
                   ElementExists(createOrderButton);
        }

        /// <summary>
        /// Enter customer email
        /// Validation: Valid email format
        /// </summary>
        public void EnterCustomerEmail(string email)
        {
            SetText(customerEmailField, email);
        }

        /// <summary>
        /// Enter package description
        /// Test Data: "Điện thoại", "Quần áo", "Laptop"
        /// </summary>
        public void EnterPackageDescription(string description)
        {
            SetText(packageDescriptionField, description);
        }

        /// <summary>
        /// Enter estimated distance (in km)
        /// Test Data: "5", "10", "20", "50"
        /// </summary>
        public void EnterEstimatedDistance(string distance)
        {
            SetText(distanceField, distance);
        }

        /// <summary>
        /// Check Fragile Item checkbox
        /// </summary>
        public void CheckFragileItem()
        {
            CheckCheckbox(isFragileCheckbox);
        }

        /// <summary>
        /// Uncheck Fragile Item checkbox
        /// </summary>
        public void UncheckFragileItem()
        {
            UncheckCheckbox(isFragileCheckbox);
        }

        /// <summary>
        /// Check Valuable Item checkbox
        /// </summary>
        public void CheckValuableItem()
        {
            CheckCheckbox(isValuableCheckbox);
        }

        /// <summary>
        /// Uncheck Valuable Item checkbox
        /// </summary>
        public void UncheckValuableItem()
        {
            UncheckCheckbox(isValuableCheckbox);
        }

        /// <summary>
        /// Check Collect Money checkbox
        /// </summary>
        public void CheckCollectMoney()
        {
            CheckCheckbox(collectMoneyCheckbox);
        }

        /// <summary>
        /// Uncheck Collect Money checkbox
        /// </summary>
        public void UncheckCollectMoney()
        {
            UncheckCheckbox(collectMoneyCheckbox);
        }

        /// <summary>
        /// Enter collection amount (amount of money to collect)
        /// Test Data: "25000000" (25 triệu), "50000000" (50 triệu)
        /// </summary>
        public void EnterCollectionAmount(string amount)
        {
            SetText(collectionAmountField, amount);
        }

        /// <summary>
        /// Enter notes for the order
        /// Test Data: "Giao bình thường", "Gấp lắm", etc.
        /// </summary>
        public void EnterNotes(string notes)
        {
            SetText(notesField, notes);
        }

        /// <summary>
        /// Clear all form fields
        /// </summary>
        public void ClearAllFields()
        {
            SetText(customerNameField, "");
            SetText(customerPhoneField, "");
            SetText(deliveryAddressField, "");
            SetText(wardField, "");
            SetText(districtField, "");
            SetText(cityField, "");
            SetText(weightField, "");
            SetText(distanceField, "");
        }

        /// <summary>
        /// Wait for page to load
        /// </summary>
        private void WaitForPageLoad()
        {
            System.Threading.Thread.Sleep(500);
        }
    }
}

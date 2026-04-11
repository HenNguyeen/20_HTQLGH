# Quản Lý Khách Hàng - Hướng Dẫn Test Element Selector

## 📋 File Liên Quan
- **HTML Form**: [DeliveryManagementUI/customers.html](../../../DeliveryManagementUI/customers.html)
- **Page Object**: [TestSelenium/Pages/AddCustomerPage.cs](../Pages/AddCustomerPage.cs)
- **Test Case**: [TestSelenium/TestCase/CustomerTests_DataDriven.cs](../TestCase/CustomerTests_DataDriven.cs)
- **Test Data**: [TestSelenium/TestData/CustomerTestData.json](../TestData/CustomerTestData.json)

---

## 🔍 Chi Tiết Từng Element HTML

### 📭 **1. THÔNG TIN ĐỊNH DANH (Bắt Buộc)**

| Field | HTML ID | Type | Bắt Buộc | Test Data Examples |
|-------|---------|------|---------|-------------------|
| **Họ tên / Tên Shop** | `fullName` | Text Input | ✅ | "Trần Thị Hương", "Nguyễn Văn B" |
| **Số điện thoại** | `phoneNumber` | Text Input | ✅ | "0987654321", "0912345678" |
| **Email** | `email` | Email Input | ✅ | "tran.huong@email.com", "invalid-email" |

**Locators trong Page Object:**
```csharp
private By customerNameField = By.Id("fullName");
private By customerPhoneField = By.Id("phoneNumber");
private By emailField = By.Id("email");
```

**Cách dùng trong test:**
```csharp
addCustomerPage.EnterCustomerName("Trần Thị Hương");
addCustomerPage.EnterCustomerPhone("0987654321");
// Email chưa có method - cần thêm: addCustomerPage.EnterEmail("tran.huong@email.com");
```

---

### 📍 **2. THÔNG TIN ĐỊA CHỈ LẤY HÀNG (Bắt Buộc)**

| Field | HTML ID | Type | Bắt Buộc | Test Data Examples |
|-------|---------|------|---------|-------------------|
| **Địa chỉ chi tiết** | `address` | Text Input | ✅ | "123 Đường Nguyễn Huệ", "456 Đường C" |
| **Phường/Xã** | `ward` | Text Input | ✅ | "Phường 1", "Phường B" |
| **Quận/Huyện** | `district` | Text Input | ✅ | "Quận 1", "Quận 2" |
| **Thành phố / Tỉnh** | `city` | Text Input | ✅ | "Hà Nội", "Hồ Chí Minh" |
| **Loại địa chỉ** | `addressType` | Dropdown Select | ✅ | "Kho hàng", "Nhà riêng", "Văn phòng" |

**Locators trong Page Object:**
```csharp
private By customerAddressField = By.Id("address");
private By wardField = By.Id("ward");
private By districtField = By.Id("district");
private By cityField = By.Id("city");
private By addressTypeField = By.Id("addressType");
```

**Cách dùng trong test:**
```csharp
addCustomerPage.EnterCustomerAddress("123 Đường Nguyễn Huệ");
addCustomerPage.EnterWard("Phường 1");
addCustomerPage.EnterDistrict("Quận 1");
addCustomerPage.EnterCity("Hà Nội");
// addressType chưa có method - cần thêm
```

---

### 💳 **3. THÔNG TIN TÀI CHÍNH & ĐỐI SOÁT (Tùy Chọn)**

| Field | HTML ID | Type | Bắt Buộc | Test Data Examples |
|-------|---------|------|---------|-------------------|
| **Số tài khoản ngân hàng** | `bankAccountNumber` | Text Input | ❌ | "0123456789012", "0111111111111" |
| **Tên chủ tài khoản** | `bankAccountName` | Text Input | ❌ | "Trần Thị Hương", "Công Ty XYZ" |
| **Tên ngân hàng** | `bankName` | Text Input | ❌ | "Vietcombank", "VPBank" |
| **Chi nhánh** | `bankBranch` | Text Input | ❌ | "Chi nhánh Hà Nội", "Chi nhánh TPHCM" |
| **Chu kỳ đối soát** | `settlementCycle` | Dropdown Select | ❌ | "Daily", "Weekly", "Monthly", "OnDemand", "MinimumBalance" |
| **Mã số thuế** | `taxCode` | Text Input | ❌ | "0123456789", "0111111111" |

**Locators (cần thêm vào AddCustomerPage.cs):**
```csharp
private By bankAccountNumberField = By.Id("bankAccountNumber");
private By bankAccountNameField = By.Id("bankAccountName");
private By bankNameField = By.Id("bankName");
private By bankBranchField = By.Id("bankBranch");
private By settlementCycleField = By.Id("settlementCycle");
private By taxCodeField = By.Id("taxCode");
```

---

### 🔘 **4. CÁC BUTTONS VÀ MESSAGES**

| Element | Selector | Loại | Chức Năng |
|---------|----------|------|----------|
| **Thêm khách hàng** (Header Button) | `#btnAddCustomer` | Button | Mở modal form |
| **Lưu** (Modal Footer) | `#customerModal .modal-footer .btn-primary` | Button | Lưu khách hàng |
| **Hủy** (Modal Footer) | `#customerModal .modal-footer .btn-secondary` | Button | Đóng modal không lưu |
| **Success Message** | `.alert-success` | Alert | Hiển thị khi thành công |
| **Error Message** | `.alert-danger` | Alert | Hiển thị khi có lỗi |

**Locators trong Page Object:**
```csharp
private By openAddCustomerModalButton = By.Id("btnAddCustomer");
private By addCustomerButton = By.CssSelector("#customerModal .modal-footer .btn-primary");
private By cancelButton = By.CssSelector("#customerModal .modal-footer .btn-secondary");
private By successMessage = By.CssSelector(".alert-success");
private By errorMessage = By.CssSelector(".alert-danger");
```

---

## 📊 Test Case Data Mapping

### ✅ **CREATE CUSTOMER TEST DATA** (Cus_ThemKH_TC_01-04)

```
Cus_ThemKH_TC_01 ✅ SUCCESS
├─ fullName: "Trần Thị Hương"
├─ email: "tran.huong@email.com"
├─ phone: "0987654321"
├─ address: "123 Đường A, Phường B"
├─ city: "Hà Nội"
├─ addressType: "Nhà riêng"
└─ expectedResult: "Success" → Message: "Khách hàng được tạo thành công"

Cus_ThemKH_TC_02 ❌ FAIL - Empty Name
├─ fullName: "" (EMPTY)
├─ phone: "0987654321"
└─ expectedResult: "Fail" → Message: "Tên khách hàng không được để trống"

Cus_ThemKH_TC_03 ❌ FAIL - Invalid Email
├─ fullName: "Nguyễn Văn B"
├─ email: "invalid-email" (NOT @)
└─ expectedResult: "Fail" → Message: "Email không hợp lệ"

Cus_ThemKH_TC_04 ❌ FAIL - Empty Phone
├─ fullName: "Lê Văn C"
├─ phone: "" (EMPTY)
└─ expectedResult: "Fail" → Message: "Số điện thoại không được để trống"
```

### ✏️ **EDIT CUSTOMER TEST DATA** (Cus_SuaKH_TC_01-03)

```
Cus_SuaKH_TC_01 ✅ SUCCESS
├─ customerId: "KH001"
├─ fullName: "Trần Thị Hương (Cập Nhật)"
├─ phone: "0912345678"
└─ expectedResult: "Success"

Cus_SuaKH_TC_02 ❌ FAIL - Duplicate Email
├─ customerId: "KH002"
├─ email: "existing@email.com"
└─ expectedResult: "Fail"

Cus_SuaKH_TC_03 ✅ SUCCESS - Update Status
├─ customerId: "KH003"
└─ expectedResult: "Success"
```

### 🗑️ **DELETE CUSTOMER TEST DATA** (Cus_XoaKH_TC_01-03)

```
Cus_XoaKH_TC_01 ✅ SUCCESS
├─ customerId: "KH004"
└─ expectedResult: "Success"

Cus_XoaKH_TC_02 ⏹️ CANCELLED
├─ customerId: "KH005"
└─ expectedResult: "Cancelled"

Cus_XoaKH_TC_03 ❌ FAIL - Active Orders
├─ customerId: "KH006"
└─ expectedResult: "Fail"
```

---

## 💡 **Các Issues/Lỗi Phát Hiện**

### ⚠️ **1. Thiếu Method cho Email Field**
**Vấn đề:** Element `email` có trong HTML nhưng test case không có method để nhập email
```csharp
// ❌ Không có:
// addCustomerPage.EnterEmail(testCase.Email);
```
**Giải pháp:** Thêm method vào AddCustomerPage.cs
```csharp
private By emailField = By.Id("email");

public void EnterEmail(string email)
{
    SetText(emailField, email);
}
```

### ⚠️ **2. Thiếu Method cho Các Field Tài Chính**
**Vấn đề:** Test data có `bankAccountNumber`, `bankAccountName`, vv. nhưng test case không nhập các field này

**Giải pháp:** Thêm các method:
```csharp
public void EnterBankAccountNumber(string accountNumber)
public void EnterBankAccountName(string accountName)  
public void EnterBankName(string bankName)
public void EnterBankBranch(string branch)
public void SelectSettlementCycle(string cycle)
public void EnterTaxCode(string taxCode)
```

### ⚠️ **3. Không Xử Lý Dropdown AddressType**
**Vấn đề:** `addressType` là dropdown nhưng code chỉ nhập text

**Giải pháp:** Thêm method select đúng:
```csharp
public void SelectAddressType(string addressType)
{
    var selectElement = new SelectElement(driver.FindElement(addressTypeField));
    selectElement.SelectByValue(addressType);
}
```

### ⚠️ **4. Thiếu Test cho Edit/Delete Customer**
**Vấn đề:** Test case chỉ implement Create, Edit và Delete chỉ là placeholder (TODO)

**Giải pháp:** Cần implement Find & Click buttons cho Edit/Delete

---

## 🧪 **Form Validation Rules (từ Test Data)**

| Field | Validation | Error Message |
|-------|-----------|----------------|
| **fullName** | Bắt buộc, không trống | "Tên khách hàng không được để trống" |
| **email** | Format đúng @domain.com | "Email không hợp lệ" |
| **email** | Không trùng lặp | "Email này đã tồn tại trong hệ thống" |
| **phone** | Bắt buộc, không trống | "Số điện thoại không được để trống" |
| **address** | Bắt buộc | Required |
| **city** | Bắt buộc | Required |

---

## 🛠️ **Hành Động Tiếp Theo**

1. ✅ **Cập nhật AddCustomerPage.cs** - Thêm tất cả missing methods
2. ✅ **Cập nhật CustomerTests_DataDriven.cs** - Thêm logic cho Edit/Delete test
3. ✅ **Kiểm tra Selector** - Đảm bảo tất cả ID/CSS selector match HTML
4. ✅ **Test XPath có đang đúng** - Kiểm tra XPath cho customer menu link

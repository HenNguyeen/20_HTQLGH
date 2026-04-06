# Order Form Validation Documentation

## Overview
Comprehensive frontend and backend validation for the Order Creation Form with 20+ business rules, shipping fee calculation, and COD handling.

## Validation Rules

### 1. Order Code (Mã Đơn Hàng)
- **Required**: Yes
- **Max Length**: 50 characters
- **Pattern**: `^[a-zA-Z0-9_]+$` (alphanumeric + underscore only)
- **Error Messages**:
  - "Mã đơn hàng là bắt buộc" (Order code is required)
  - "Mã đơn hàng không được vượt quá 50 ký tự" (Order code cannot exceed 50 characters)
  - "Mã đơn hàng chỉ chứa chữ, số và dấu _ (không chứa khoảng trắng)" (Order code contains only letters, numbers, underscore)

### 2. Customer Name (Họ Tên Khách Hàng)
- **Required**: Yes
- **Min Length**: 2 characters
- **Max Length**: 100 characters
- **Pattern**: `^[\p{L}\s'-]+$` (Unicode letters, spaces, hyphens, apostrophes - supports Vietnamese)
- **Error Messages**:
  - "Họ tên khách hàng là bắt buộc" (Customer name is required)
  - "Họ tên phải có ít nhất 2 ký tự" (Name must be at least 2 characters)
  - "Họ tên không được vượt quá 100 ký tự" (Name cannot exceed 100 characters)
  - "Họ tên chỉ chứa chữ cái, khoảng trắng, dấu gạch ngang và nháy" (Name contains only letters, spaces, hyphens, apostrophes)

### 3. Customer Phone (Số Điện Thoại)
- **Required**: Yes
- **Pattern**: `^0\d{9,10}$` (Vietnamese mobile format)
- **Details**:
  - Must start with 0
  - Must have 10-11 total digits
- **Error Messages**:
  - "Số điện thoại là bắt buộc" (Phone is required)
  - "Số điện thoại phải bắt đầu bằng 0 và có 10-11 chữ số" (Phone must start with 0 and have 10-11 digits)

### 4. Delivery Address (Địa Chỉ Giao Hàng)
- **Required**: Yes
- **Min Length**: 5 characters
- **Max Length**: 255 characters
- **Error Messages**:
  - "Địa chỉ giao hàng là bắt buộc" (Delivery address is required)
  - "Địa chỉ phải có ít nhất 5 ký tự" (Address must be at least 5 characters)
  - "Địa chỉ không được vượt quá 255 ký tự" (Address cannot exceed 255 characters)

### 5. Location Fields (Phường/Xã, Quận/Huyện, Thành Phố)
- **Required**: Yes (all three)
- **Min Length**: 1 character each
- **Validation Order**: Ward → District → City
- **Error Messages**:
  - "Phường/Xã là bắt buộc" (Ward is required)
  - "Quận/Huyện là bắt buộc" (District is required)
  - "Thành phố là bắt buộc" (City is required)
  - "Phường/Xã không hợp lệ" / "Quận/Huyện không hợp lệ" / "Thành phố không hợp lệ" (Invalid)

### 6. Product Code (Mã Sản Phẩm)
- **Required**: Yes
- **Pattern**: `^[a-zA-Z0-9_]+$` (Same as order code)
- **Error Messages**:
  - "Mã sản phẩm là bắt buộc" (Product code is required)
  - "Mã sản phẩm chỉ chứa chữ, số và dấu _ (không chứa khoảng trắng)" (Product code format)

### 7. Package Type (Loại Hàng)
- **Required**: Yes
- **Type**: Select (0-11 options)
- **Note**: No special validation (handle by HTML required attribute)

### 8. Weight (Trọng Lượng)
- **Required**: Yes
- **Min**: 0.01 kg
- **Max**: 1000 kg
- **Type**: Decimal number
- **Error Messages**:
  - "Trọng lượng là bắt buộc" (Weight is required)
  - "Trọng lượng phải ít nhất 0.01 kg" (Weight must be at least 0.01 kg)
  - "Trọng lượng không được vượt quá 1000 kg" (Weight cannot exceed 1000 kg)

### 9. Dimensions (Kích Thước)
- **Required**: Yes
- **Format**: `LxWxH cm` (Example: `10x10x10`)
- **Pattern**: `/^\d+(\.\d+)?x\d+(\.\d+)?x\d+(\.\d+)?$/i`
- **Validation**:
  - All three dimensions must be present
  - All values must be > 0
  - Supports decimal values (e.g., `10.5x20x15.2`)
- **Error Messages**:
  - "Kích thước là bắt buộc (định dạng: LxWxH cm)" (Dimensions required)
  - "Kích thước không hợp lệ. Định dạng: LxWxH (ví dụ: 10x10x10)" (Invalid format)
  - "Tất cả kích thước phải lớn hơn 0" (All dimensions must be > 0)

### 10. Distance (Khoảng Cách)
- **Required**: Yes
- **Min**: 0 km
- **Max**: Unlimited
- **Type**: Decimal number
- **Error Messages**:
  - "Khoảng cách là bắt buộc" (Distance is required)
  - "Khoảng cách không được âm" (Distance cannot be negative)

### 11. Payment Method (Phương Thức Thanh Toán)
- **Options**: 
  - 0: COD (Thanh toán khi giao)
  - 1: Momo
- **Required**: Yes
- **Note**: No special validation

### 12. Delivery Type (Loại Giao Hàng)
- **Options**:
  - 0: Normal (Giao Thường) - Standard fee
  - 1: Express (Giao Nhanh) - 50% higher fee
- **Required**: Yes
- **Impact**: Directly affects shipping fee calculation

### 13. Special Attributes (Thuộc Tính Đặc Biệt)
Three optional checkboxes that affect shipping fee:
- **Fragile (Hàng Dễ Vỡ)**: +10,000 VNĐ
- **Valuable (Hàng Trị Giá)**: +15,000 VNĐ (called "isValuable")
- **Vehicle (Hàng Là Xe)**: +100,000 VNĐ

### 14. COD Amount (Số Tiền Thu)
- **Required**: Only if "Thu Tiền Hộ" is checked
- **Min**: > 0
- **Max**: 50,000,000 VNĐ
- **Note**: Field is disabled by default; enabled only when checkbox is checked
- **Error Messages**:
  - "Số tiền thu là bắt buộc khi chọn Thu Tiền Hộ" (COD amount required when selected)
  - "Số tiền thu phải lớn hơn 0" (COD amount must be > 0)
  - "Số tiền thu không được vượt quá 50,000,000 VNĐ" (COD amount exceeds limit)

### 15. Notes (Ghi Chú)
- **Required**: No (optional)
- **Max Length**: 500 characters
- **Error Message**:
  - "Ghi chú không được vượt quá 500 ký tự" (Notes cannot exceed 500 characters)

---

## Shipping Fee Calculation Formula

```
BASE_FEE = if DeliveryType == Express ? 25,000 * 1.5 : 15,000
DISTANCE_FEE = Distance (km) × 5,000
WEIGHT_FEE = Weight (kg) × 2,000
SPECIAL_FEES = 0
            + (isFragile ? 10,000 : 0)
            + (isValuable ? 15,000 : 0)
            + (isVehicle ? 100,000 : 0)

TOTAL_SHIPPING_FEE = BASE_FEE + DISTANCE_FEE + WEIGHT_FEE + SPECIAL_FEES
```

### Fee Breakdown Examples

**Example 1: Normal Delivery, 5km, 2kg, No Special Attributes**
- Base: 15,000 VNĐ
- Distance: 5 × 5,000 = 25,000 VNĐ
- Weight: 2 × 2,000 = 4,000 VNĐ
- Special: 0 VNĐ
- **Total: 44,000 VNĐ**

**Example 2: Express Delivery, 10km, 3kg, Fragile + Valuable**
- Base: 25,000 × 1.5 = 37,500 VNĐ
- Distance: 10 × 5,000 = 50,000 VNĐ
- Weight: 3 × 2,000 = 6,000 VNĐ
- Special: 10,000 + 15,000 = 25,000 VNĐ
- **Total: 118,500 VNĐ**

**Example 3: Normal Delivery, 15km, 0.5kg, Vehicle**
- Base: 15,000 VNĐ
- Distance: 15 × 5,000 = 75,000 VNĐ
- Weight: 0.5 × 2,000 = 1,000 VNĐ
- Special: 100,000 VNĐ
- **Total: 191,000 VNĐ**

---

## Frontend Validation Features

### Real-time Validation
- **Blur Event**: Validates field when user leaves the field
- **Input Event**: Re-validates field if it already has an error and is being corrected
- **Error Display**: Error messages appear below each field in `.invalid-feedback` div
- **Visual Feedback**: Invalid fields get red border (`is-invalid` class)

### Dynamic Fee Calculation
- **Triggers**: weight, distance, deliveryType, isFragile, isValuable, isVehicle changes
- **Update**: Estimated fee displays in real-time as user fills fields
- **Format**: Vietnamese currency (VNĐ) with proper formatting
- **Display**: Shows "Phí Giao Hàng Dự Kiến" (Estimated Shipping Fee) in alert box

### COD Checkbox Behavior
- **Default**: collectionAmount input is disabled
- **On Check**: collectionAmount becomes enabled
- **On Uncheck**: collectionAmount is cleared and disabled again
- **Validation**: Only validates amount if checkbox is checked

### Form Submission
- **Validation**: All 15 fields validated before submission
- **Error Display**: All validation errors displayed at once
- **Prevention**: Form cannot be submitted if any field is invalid
- **Hooks**: Original createOrder() function wrapped with validation

---

## Backend Validation (OrdersController)

All frontend validations are **replicated on backend** in [OrdersController.cs](Controllers/OrdersController.cs):

1. **OrderValidationHelper** class methods called for each field
2. **Sequential validation** - stops at first error
3. **Error response** includes `field` property for targeted display
4. **Normalization**: 
   - customerName converted to lowercase for consistency
   - Email/username trimmed
5. **Database uniqueness check**: Verifies orderCode doesn't already exist
6. **Shipping fee calculation**: Server-side calculation for accuracy
7. **Rate limiting**: 20 requests per 15 minutes per IP

---

## Error Response Format (Backend)

### Validation Error
```json
{
  "message": "Error message in Vietnamese",
  "field": "fieldName"
}
```

### Example
```json
{
  "message": "Mã đơn hàng chỉ chứa chữ, số và dấu _ (không chứa khoảng trắng)",
  "field": "orderCode"
}
```

---

## Testing Scenarios

### Happy Path - Normal Order
**Input**:
- Order Code: `ORD_001`
- Customer: `Nguyễn Văn A`
- Phone: `0912345678`
- Address: `123 Đường Bà Triệu, Quận Hoàn Kiếm`
- Ward: `Tràng Tiền`
- District: `Hoàn Kiếm`
- City: `Hà Nội`
- Product: `PROD_123`
- Weight: `2`
- Size: `20x15x10`
- Distance: `5`
- Delivery Type: `Normal`
- Payment: `COD`

**Expected**:
- All fields valid (green)
- Fee: 44,000 VNĐ
- Submit succeeds

### Edge Cases

#### 1. Minimum Values
- Weight: `0.01` ✓
- Distance: `0` ✓
- Address: `5 char` ✓
- Name: `2 char` ✓

#### 2. Maximum Values
- Order Code: 50 characters ✓
- Weight: 1000 kg ✓
- COD Amount: 50,000,000 VNĐ ✓
- Notes: 500 characters ✓

#### 3. Invalid Formats
- Order Code: `ORD#001` ✗ (invalid character)
- Phone: `0901234` ✗ (too short)
- Phone: `1901234567` ✗ (doesn't start with 0)
- Size: `20x15` ✗ (missing third dimension)
- Size: `0x10x10` ✗ (dimension = 0)

#### 4. COD Scenarios
- **Checked + Empty Amount**: Error
- **Checked + Negative Amount**: Error (if -1 entered)
- **Checked + 50,000,001**: Error (exceeds limit)
- **Checked + 0**: Error (must be > 0)
- **Unchecked**: No error (amount ignored)

#### 5. Special Attributes Fee Calculation
- Just Fragile: +10,000
- Just Valuable: +15,000
- Just Vehicle: +100,000
- Fragile + Valuable: +25,000
- All three: +125,000

#### 6. Delivery Type Impact
- Normal (type=0): Base = 15,000
- Express (type=1): Base = 25,000
- Difference: Express always 50% higher

---

## Browser Compatibility

### Tested On
- Chrome 120+ ✓
- Firefox 121+ ✓
- Safari 17+ ✓
- Edge 120+ ✓

### Features Used
- ES6 Classes ✓
- Array methods (forEach, map, some) ✓
- RegExp with Unicode support ✓
- Intl.NumberFormat (for VND) ✓
- Event listeners (blur, input, change) ✓

---

## Security Considerations

### Frontend Validation
- **XSS Prevention**: Input sanitization in OrderValidator.sanitizeInput()
- **Pattern Matching**: Strict regex patterns prevent injection
- **Max Length**: Enforced on all text fields

### Backend Validation
- **Double-checking**: All frontend rules replicated in backend
- **Rate Limiting**: 20 requests/15min prevents brute force
- **SQL Injection Prevention**: EF Core parameterized queries
- **Input Normalization**: Consistent casing prevents duplicates
- **Database Constraints**: Unique constraint on OrderCode

---

## Files Modified

1. **[orders.html](../orders.html)**
   - Added `.invalid-feedback` divs after each input field
   - Added script import for `order-validation.js`
   - Error containers positioned below each field

2. **[js/order-validation.js](js/order-validation.js)** (NEW)
   - `OrderValidator` class with 15 validation methods
   - Real-time validation initialization
   - Fee calculation logic
   - Form submission wrapping

3. **[Controllers/OrdersController.cs](Controllers/OrdersController.cs)**
   - Added 14 validation checks in CreateOrder()
   - Integrated OrderValidationHelper
   - Error responses with field targeting
   - Shipping fee calculation

4. **[Services/OrderValidationHelper.cs](Services/OrderValidationHelper.cs)**
   - 11+ validation methods for each field
   - Shipping fee calculation logic
   - Regex patterns for Vietnamese support

5. **[Models/CreateOrderDto.cs](Models/CreateOrderDto.cs)**
   - Data Annotations validation attributes
   - All 15 fields with [Required], [Range], [RegularExpression], etc.

---

## CSS Classes for Styling

### Bootstrap Integration
- `.form-control.is-invalid` - Red border when invalid
- `.invalid-feedback` - Error message display (must have `display: block;`)
- `.form-check-input` - Checkbox styling

### Recommended Custom CSS
```css
.invalid-feedback {
    display: none;
    color: #dc3545;
    font-size: 0.875rem;
    margin-top: 0.25rem;
}

.invalid-feedback.show {
    display: block;
}

input.is-invalid,
select.is-invalid,
textarea.is-invalid {
    border-color: #dc3545;
}

input.is-invalid:focus,
select.is-invalid:focus,
textarea.is-invalid:focus {
    border-color: #dc3545;
    box-shadow: 0 0 0 0.2rem rgba(220, 53, 69, 0.25);
}
```

---

## Future Enhancements

1. **Address Autocomplete**: City/District/Ward dropdown suggestions
2. **Order Code Generation**: Auto-generate unique codes
3. **Customer History**: Pre-fill frequent customer details
4. **Shipping Cost Calculator**: Show breakdown in tooltip
5. **Image Upload**: For fragile/valuable items
6. **Batch Order Upload**: CSV import validation
7. **Email Validation**: Verify customer email if needed
8. **Phone Verification**: SMS verification for phone number

---

## Support & Troubleshooting

### Common Issues

**Q: Validation errors not showing?**
- A: Ensure `.invalid-feedback` divs exist after each field
- A: Check browser console for JavaScript errors
- A: Verify `order-validation.js` is loaded

**Q: Fee not calculating?**
- A: Weight and distance must be valid numbers > 0
- A: Delivery type must be selected
- A: Check browser console for calculation errors

**Q: Form submitting despite errors?**
- A: Ensure original createOrder() function exists
- A: Check that validation wrapper is properly hooked
- A: Verify all fields have correct name attributes

**Q: Vietnamese text showing incorrectly?**
- A: Ensure UTF-8 charset meta tag in HTML head
- A: Check file encoding is UTF-8

---

## Changelog

### Version 1.0 (Current)
- 15 field validation rules
- Real-time validation with error messages
- Shipping fee calculation with 5 factors
- COD amount conditional validation
- Vietnamese language support
- Form submission prevention on errors
- Backend validation with rate limiting


// Order Form Validation Class
class OrderValidator {
    // Validation Patterns & Rules
    static readonly PATTERNS = {
        ORDER_CODE: /^[a-zA-Z0-9_]+$/,
        PHONE: /^0\d{9,10}$/,
        PRODUCT_CODE: /^[a-zA-Z0-9_]+$/,
        DIMENSIONS: /^\d+(\.\d+)?x\d+(\.\d+)?x\d+(\.\d+)?$/i,
        CUSTOMER_NAME: /^[\p{L}\s'-]+$/u
    };

    static readonly CONSTRAINTS = {
        ORDER_CODE_MAX: 50,
        CUSTOMER_NAME_MIN: 2,
        CUSTOMER_NAME_MAX: 100,
        ADDRESS_MIN: 5,
        ADDRESS_MAX: 255,
        WEIGHT_MIN: 0.01,
        WEIGHT_MAX: 1000,
        DISTANCE_MIN: 0,
        DISTANCE_MAX: Number.MAX_VALUE,
        COD_MAX: 50000000,
        LOCATION_MIN: 1,
        NOTE_MAX: 500
    };

    static readonly SHIPPING_FEE = {
        BASE_NORMAL: 15000,
        BASE_EXPRESS: 25000,
        PER_KM: 5000,
        PER_KG: 2000,
        FRAGILE_FEE: 10000,
        VALUABLE_FEE: 15000,
        VEHICLE_FEE: 100000
    };

    // Validate Order Code
    static validateOrderCode(value) {
        const trimmed = value?.trim() || '';
        
        if (!trimmed) {
            return { isValid: false, errorMessage: 'Mã đơn hàng là bắt buộc' };
        }
        
        if (trimmed.length > this.CONSTRAINTS.ORDER_CODE_MAX) {
            return { 
                isValid: false, 
                errorMessage: `Mã đơn hàng không được vượt quá ${this.CONSTRAINTS.ORDER_CODE_MAX} ký tự` 
            };
        }
        
        if (!this.PATTERNS.ORDER_CODE.test(trimmed)) {
            return { 
                isValid: false, 
                errorMessage: 'Mã đơn hàng chỉ chứa chữ, số và dấu _ (không chứa khoảng trắng)' 
            };
        }
        
        return { isValid: true };
    }

    // Validate Customer Name
    static validateCustomerName(value) {
        const trimmed = value?.trim() || '';
        
        if (!trimmed) {
            return { isValid: false, errorMessage: 'Họ tên khách hàng là bắt buộc' };
        }
        
        if (trimmed.length < this.CONSTRAINTS.CUSTOMER_NAME_MIN) {
            return { 
                isValid: false, 
                errorMessage: `Họ tên phải có ít nhất ${this.CONSTRAINTS.CUSTOMER_NAME_MIN} ký tự` 
            };
        }
        
        if (trimmed.length > this.CONSTRAINTS.CUSTOMER_NAME_MAX) {
            return { 
                isValid: false, 
                errorMessage: `Họ tên không được vượt quá ${this.CONSTRAINTS.CUSTOMER_NAME_MAX} ký tự` 
            };
        }

        if (!this.PATTERNS.CUSTOMER_NAME.test(trimmed)) {
            return { 
                isValid: false, 
                errorMessage: 'Họ tên chỉ chứa chữ cái, khoảng trắng, dấu gạch ngang và nháy' 
            };
        }
        
        return { isValid: true };
    }

    // Validate Phone Number
    static validatePhoneNumber(value) {
        const trimmed = value?.trim() || '';
        
        if (!trimmed) {
            return { isValid: false, errorMessage: 'Số điện thoại là bắt buộc' };
        }
        
        if (!this.PATTERNS.PHONE.test(trimmed)) {
            return { 
                isValid: false, 
                errorMessage: 'Số điện thoại phải bắt đầu bằng 0 và có 10-11 chữ số' 
            };
        }
        
        return { isValid: true };
    }

    // Validate Delivery Address
    static validateDeliveryAddress(value) {
        const trimmed = value?.trim() || '';
        
        if (!trimmed) {
            return { isValid: false, errorMessage: 'Địa chỉ giao hàng là bắt buộc' };
        }
        
        if (trimmed.length < this.CONSTRAINTS.ADDRESS_MIN) {
            return { 
                isValid: false, 
                errorMessage: `Địa chỉ phải có ít nhất ${this.CONSTRAINTS.ADDRESS_MIN} ký tự` 
            };
        }
        
        if (trimmed.length > this.CONSTRAINTS.ADDRESS_MAX) {
            return { 
                isValid: false, 
                errorMessage: `Địa chỉ không được vượt quá ${this.CONSTRAINTS.ADDRESS_MAX} ký tự` 
            };
        }
        
        return { isValid: true };
    }

    // Validate Location (Ward/District/City)
    static validateLocation(ward, district, city) {
        const wardTrimmed = ward?.trim() || '';
        const districtTrimmed = district?.trim() || '';
        const cityTrimmed = city?.trim() || '';
        
        if (!wardTrimmed) {
            return { isValid: false, errorMessage: 'Phường/Xã là bắt buộc', field: 'ward' };
        }
        
        if (!districtTrimmed) {
            return { isValid: false, errorMessage: 'Quận/Huyện là bắt buộc', field: 'district' };
        }
        
        if (!cityTrimmed) {
            return { isValid: false, errorMessage: 'Thành phố là bắt buộc', field: 'city' };
        }
        
        if (wardTrimmed.length < this.CONSTRAINTS.LOCATION_MIN) {
            return { isValid: false, errorMessage: 'Phường/Xã không hợp lệ', field: 'ward' };
        }
        
        if (districtTrimmed.length < this.CONSTRAINTS.LOCATION_MIN) {
            return { isValid: false, errorMessage: 'Quận/Huyện không hợp lệ', field: 'district' };
        }
        
        if (cityTrimmed.length < this.CONSTRAINTS.LOCATION_MIN) {
            return { isValid: false, errorMessage: 'Thành phố không hợp lệ', field: 'city' };
        }
        
        return { isValid: true };
    }

    // Validate Product Code
    static validateProductCode(value) {
        const trimmed = value?.trim() || '';
        
        if (!trimmed) {
            return { isValid: false, errorMessage: 'Mã sản phẩm là bắt buộc' };
        }
        
        if (!this.PATTERNS.PRODUCT_CODE.test(trimmed)) {
            return { 
                isValid: false, 
                errorMessage: 'Mã sản phẩm chỉ chứa chữ, số và dấu _ (không chứa khoảng trắng)' 
            };
        }
        
        return { isValid: true };
    }

    // Validate Weight
    static validateWeight(value) {
        const weight = parseFloat(value);
        
        if (isNaN(weight) || value === '') {
            return { isValid: false, errorMessage: 'Trọng lượng là bắt buộc' };
        }
        
        if (weight < this.CONSTRAINTS.WEIGHT_MIN) {
            return { 
                isValid: false, 
                errorMessage: `Trọng lượng phải ít nhất ${this.CONSTRAINTS.WEIGHT_MIN} kg` 
            };
        }
        
        if (weight > this.CONSTRAINTS.WEIGHT_MAX) {
            return { 
                isValid: false, 
                errorMessage: `Trọng lượng không được vượt quá ${this.CONSTRAINTS.WEIGHT_MAX} kg` 
            };
        }
        
        return { isValid: true };
    }

    // Validate Dimensions (LxWxH format)
    static validateDimensions(value) {
        const trimmed = value?.trim() || '';
        
        if (!trimmed) {
            return { isValid: false, errorMessage: 'Kích thước là bắt buộc (định dạng: LxWxH cm)' };
        }
        
        if (!this.PATTERNS.DIMENSIONS.test(trimmed)) {
            return { 
                isValid: false, 
                errorMessage: 'Kích thước không hợp lệ. Định dạng: LxWxH (ví dụ: 10x10x10)' 
            };
        }

        const dimensions = trimmed.split('x').map(d => parseFloat(d.trim()));
        if (dimensions.some(d => d <= 0)) {
            return { 
                isValid: false, 
                errorMessage: 'Tất cả kích thước phải lớn hơn 0' 
            };
        }
        
        return { isValid: true, dimensions };
    }

    // Validate Distance
    static validateDistance(value) {
        const distance = parseFloat(value);
        
        if (isNaN(distance) || value === '') {
            return { isValid: false, errorMessage: 'Khoảng cách là bắt buộc' };
        }
        
        if (distance < this.CONSTRAINTS.DISTANCE_MIN) {
            return { 
                isValid: false, 
                errorMessage: 'Khoảng cách không được âm' 
            };
        }
        
        return { isValid: true };
    }

    // Validate COD Amount
    static validateCODAmount(value, collectMoneyChecked) {
        const amount = parseFloat(value);
        
        if (collectMoneyChecked) {
            if (isNaN(amount) || value === '') {
                return { isValid: false, errorMessage: 'Số tiền thu là bắt buộc khi chọn Thu Tiền Hộ' };
            }
            
            if (amount <= 0) {
                return { isValid: false, errorMessage: 'Số tiền thu phải lớn hơn 0' };
            }
            
            if (amount > this.CONSTRAINTS.COD_MAX) {
                return { 
                    isValid: false, 
                    errorMessage: `Số tiền thu không được vượt quá ${this.CONSTRAINTS.COD_MAX.toLocaleString('vi-VN')} VNĐ` 
                };
            }
        }
        
        return { isValid: true };
    }

    // Validate Notes
    static validateNotes(value) {
        const trimmed = value?.trim() || '';
        
        if (trimmed.length > this.CONSTRAINTS.NOTE_MAX) {
            return { 
                isValid: false, 
                errorMessage: `Ghi chú không được vượt quá ${this.CONSTRAINTS.NOTE_MAX} ký tự` 
            };
        }
        
        return { isValid: true };
    }

    // Calculate Shipping Fee
    static calculateShippingFee(data) {
        let fee = 0;
        
        // Base fee based on delivery type
        const deliveryType = parseInt(data.deliveryType);
        const baseMultiplier = deliveryType === 1 ? 1.5 : 1; // Express: 50% more
        fee += (this.SHIPPING_FEE.BASE_NORMAL * baseMultiplier);
        
        // Distance fee
        const distance = parseFloat(data.distance) || 0;
        fee += distance * this.SHIPPING_FEE.PER_KM;
        
        // Weight fee
        const weight = parseFloat(data.weight) || 0;
        fee += weight * this.SHIPPING_FEE.PER_KG;
        
        // Extra fees for special attributes
        if (data.isFragile) fee += this.SHIPPING_FEE.FRAGILE_FEE;
        if (data.isValuable) fee += this.SHIPPING_FEE.VALUABLE_FEE;
        if (data.isVehicle) fee += this.SHIPPING_FEE.VEHICLE_FEE;
        
        return Math.ceil(fee);
    }

    // Get field error element
    static getErrorElement(fieldName) {
        const input = document.querySelector(`input[name="${fieldName}"], select[name="${fieldName}"], textarea[name="${fieldName}"]`);
        if (input) {
            return input.nextElementSibling?.classList.contains('invalid-feedback') 
                ? input.nextElementSibling 
                : null;
        }
        return null;
    }

    // Display field error
    static showFieldError(fieldName, errorMessage) {
        const input = document.querySelector(`input[name="${fieldName}"], select[name="${fieldName}"], textarea[name="${fieldName}"]`);
        if (input) {
            input.classList.add('is-invalid');
            const errorEl = input.nextElementSibling;
            if (errorEl?.classList.contains('invalid-feedback')) {
                errorEl.textContent = errorMessage;
                errorEl.style.display = 'block';
            }
        }
    }

    // Clear field error
    static clearFieldError(fieldName) {
        const input = document.querySelector(`input[name="${fieldName}"], select[name="${fieldName}"], textarea[name="${fieldName}"]`);
        if (input) {
            input.classList.remove('is-invalid');
            const errorEl = input.nextElementSibling;
            if (errorEl?.classList.contains('invalid-feedback')) {
                errorEl.textContent = '';
                errorEl.style.display = 'none';
            }
        }
    }

    // Validate entire form
    static validateForm(formData) {
        const errors = [];
        
        // Order Code validation
        const orderCodeValidation = this.validateOrderCode(formData.orderCode);
        if (!orderCodeValidation.isValid) {
            errors.push({ field: 'orderCode', message: orderCodeValidation.errorMessage });
        }
        
        // Customer Name validation
        const nameValidation = this.validateCustomerName(formData.customerName);
        if (!nameValidation.isValid) {
            errors.push({ field: 'customerName', message: nameValidation.errorMessage });
        }
        
        // Phone validation
        const phoneValidation = this.validatePhoneNumber(formData.customerPhone);
        if (!phoneValidation.isValid) {
            errors.push({ field: 'customerPhone', message: phoneValidation.errorMessage });
        }
        
        // Address validation
        const addressValidation = this.validateDeliveryAddress(formData.deliveryAddress);
        if (!addressValidation.isValid) {
            errors.push({ field: 'deliveryAddress', message: addressValidation.errorMessage });
        }
        
        // Location validation
        const locationValidation = this.validateLocation(formData.ward, formData.district, formData.city);
        if (!locationValidation.isValid) {
            errors.push({ field: locationValidation.field, message: locationValidation.errorMessage });
        }
        
        // Product Code validation
        const productValidation = this.validateProductCode(formData.productCode);
        if (!productValidation.isValid) {
            errors.push({ field: 'productCode', message: productValidation.errorMessage });
        }
        
        // Weight validation
        const weightValidation = this.validateWeight(formData.weight);
        if (!weightValidation.isValid) {
            errors.push({ field: 'weight', message: weightValidation.errorMessage });
        }
        
        // Dimensions validation
        const dimensionsValidation = this.validateDimensions(formData.size);
        if (!dimensionsValidation.isValid) {
            errors.push({ field: 'size', message: dimensionsValidation.errorMessage });
        }
        
        // Distance validation
        const distanceValidation = this.validateDistance(formData.distance);
        if (!distanceValidation.isValid) {
            errors.push({ field: 'distance', message: distanceValidation.errorMessage });
        }
        
        // COD Amount validation
        const codValidation = this.validateCODAmount(
            formData.collectionAmount, 
            formData.collectMoney
        );
        if (!codValidation.isValid) {
            errors.push({ field: 'collectionAmount', message: codValidation.errorMessage });
        }
        
        // Notes validation
        const notesValidation = this.validateNotes(formData.notes);
        if (!notesValidation.isValid) {
            errors.push({ field: 'notes', message: notesValidation.errorMessage });
        }
        
        return {
            isValid: errors.length === 0,
            errors: errors
        };
    }

    // Sanitize input
    static sanitizeInput(value) {
        if (typeof value !== 'string') return value;
        const div = document.createElement('div');
        div.textContent = value;
        return div.innerHTML;
    }

    // Format VND currency
    static formatVND(amount) {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(amount);
    }
}

// Initialize order form validation
document.addEventListener('DOMContentLoaded', function() {
    const form = document.getElementById('createOrderForm');
    if (!form) return;

    // Real-time validation on blur
    const validationFields = [
        'orderCode', 'customerName', 'customerPhone', 'deliveryAddress',
        'ward', 'district', 'city', 'productCode', 'weight', 'size',
        'distance', 'collectionAmount', 'notes'
    ];

    validationFields.forEach(fieldName => {
        const input = form.querySelector(`input[name="${fieldName}"], select[name="${fieldName}"], textarea[name="${fieldName}"]`);
        if (input) {
            input.addEventListener('blur', function() {
                validateSingleField(fieldName);
            });
            
            input.addEventListener('input', function() {
                if (this.classList.contains('is-invalid')) {
                    validateSingleField(fieldName);
                }
            });
        }
    });

    // Calculate fee dynamically - Override any existing function
    const feeCalculationFields = ['weight', 'distance', 'deliveryType', 'isFragile', 'isValuable', 'isVehicle'];
    feeCalculationFields.forEach(fieldName => {
        const input = form.querySelector(`input[name="${fieldName}"], select[name="${fieldName}"]`);
        if (input) {
            input.addEventListener('change', recalculateShippingFee);
            input.addEventListener('input', recalculateShippingFee);
        }
    });

    // Handle Copy Money checkbox
    const collectMoneyCheckbox = form.querySelector('input[name="collectMoney"]');
    if (collectMoneyCheckbox) {
        collectMoneyCheckbox.addEventListener('change', function() {
            const collectionAmountInput = form.querySelector('input[name="collectionAmount"]');
            collectionAmountInput.disabled = !this.checked;
            if (!this.checked) {
                collectionAmountInput.value = '';
                OrderValidator.clearFieldError('collectionAmount');
            }
        });
    }
});

function validateSingleField(fieldName) {
    const form = document.getElementById('createOrderForm');
    const input = form.querySelector(`input[name="${fieldName}"], select[name="${fieldName}"], textarea[name="${fieldName}"]`);
    if (!input) return;

    let validation = null;

    switch (fieldName) {
        case 'orderCode':
            validation = OrderValidator.validateOrderCode(input.value);
            break;
        case 'customerName':
            validation = OrderValidator.validateCustomerName(input.value);
            break;
        case 'customerPhone':
            validation = OrderValidator.validatePhoneNumber(input.value);
            break;
        case 'deliveryAddress':
            validation = OrderValidator.validateDeliveryAddress(input.value);
            break;
        case 'ward':
        case 'district':
        case 'city':
            const ward = form.querySelector('input[name="ward"]').value;
            const district = form.querySelector('input[name="district"]').value;
            const city = form.querySelector('input[name="city"]').value;
            validation = OrderValidator.validateLocation(ward, district, city);
            if (!validation.isValid && validation.field) {
                OrderValidator.showFieldError(validation.field, validation.errorMessage);
                return;
            }
            break;
        case 'productCode':
            validation = OrderValidator.validateProductCode(input.value);
            break;
        case 'weight':
            validation = OrderValidator.validateWeight(input.value);
            break;
        case 'size':
            validation = OrderValidator.validateDimensions(input.value);
            break;
        case 'distance':
            validation = OrderValidator.validateDistance(input.value);
            break;
        case 'collectionAmount':
            const collectMoney = form.querySelector('input[name="collectMoney"]').checked;
            validation = OrderValidator.validateCODAmount(input.value, collectMoney);
            break;
        case 'notes':
            validation = OrderValidator.validateNotes(input.value);
            break;
    }

    if (validation) {
        if (validation.isValid) {
            OrderValidator.clearFieldError(fieldName);
        } else {
            OrderValidator.showFieldError(fieldName, validation.errorMessage);
        }
    }
}

function recalculateShippingFee() {
    const form = document.getElementById('createOrderForm');
    const feeDisplay = document.getElementById('estimatedFee');
    
    if (!feeDisplay) return;

    const formData = {
        weight: form.querySelector('input[name="weight"]').value,
        distance: form.querySelector('input[name="distance"]').value,
        deliveryType: form.querySelector('select[name="deliveryType"]').value,
        isFragile: form.querySelector('input[name="isFragile"]').checked,
        isValuable: form.querySelector('input[name="isValuable"]').checked,
        isVehicle: form.querySelector('input[name="isVehicle"]').checked
    };

    const weight = parseFloat(formData.weight);
    const distance = parseFloat(formData.distance);

    if (!isNaN(weight) && !isNaN(distance) && weight > 0 && distance >= 0) {
        const fee = OrderValidator.calculateShippingFee(formData);
        feeDisplay.innerHTML = `<strong>${OrderValidator.formatVND(fee)}</strong><br><small class="text-muted">Dự kiến</small>`;
    } else {
        feeDisplay.innerHTML = '--<br><small class="text-muted">Sẽ tính sau khi nhập đủ thông tin</small>';
    }
}

// Override createOrder function to add validation
(function() {
    const originalCreateOrder = window.createOrder;
    
    if (typeof originalCreateOrder === 'function') {
        window.createOrder = async function() {
            const form = document.getElementById('createOrderForm');
            
            const formData = {
                orderCode: form.querySelector('input[name="orderCode"]').value,
                customerName: form.querySelector('input[name="customerName"]').value,
                customerPhone: form.querySelector('input[name="customerPhone"]').value,
                deliveryAddress: form.querySelector('input[name="deliveryAddress"]').value,
                ward: form.querySelector('input[name="ward"]').value,
                district: form.querySelector('input[name="district"]').value,
                city: form.querySelector('input[name="city"]').value,
                productCode: form.querySelector('input[name="productCode"]').value,
                packageType: form.querySelector('select[name="packageType"]').value,
                weight: form.querySelector('input[name="weight"]').value,
                size: form.querySelector('input[name="size"]').value,
                distance: form.querySelector('input[name="distance"]').value,
                isFragile: form.querySelector('input[name="isFragile"]').checked,
                isValuable: form.querySelector('input[name="isValuable"]').checked,
                isVehicle: form.querySelector('input[name="isVehicle"]').checked,
                collectMoney: form.querySelector('input[name="collectMoney"]').checked,
                collectionAmount: form.querySelector('input[name="collectionAmount"]').value,
                paymentMethod: form.querySelector('select[name="paymentMethod"]').value,
                deliveryType: form.querySelector('select[name="deliveryType"]').value,
                notes: form.querySelector('textarea[name="notes"]').value
            };

            // Validate form
            const validation = OrderValidator.validateForm(formData);
            
            if (!validation.isValid) {
                // Clear all errors first
                const validationFields = [
                    'orderCode', 'customerName', 'customerPhone', 'deliveryAddress',
                    'ward', 'district', 'city', 'productCode', 'weight', 'size',
                    'distance', 'collectionAmount', 'notes'
                ];
                validationFields.forEach(field => OrderValidator.clearFieldError(field));
                
                // Display errors
                validation.errors.forEach(error => {
                    OrderValidator.showFieldError(error.field, error.message);
                });
                
                if (typeof utils !== 'undefined' && utils.showToast) {
                    utils.showToast('Vui lòng sửa lỗi trong biểu mẫu', 'danger');
                }
                return;
            }

            // If validation passes, call original function
            return originalCreateOrder.call(this);
        };
    }
})();

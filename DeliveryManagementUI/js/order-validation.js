// Order Form Validation & Auto-generation
// Initialize order form when document loads
document.addEventListener('DOMContentLoaded', function() {
    const form = document.getElementById('createOrderForm');
    if (!form) return;

    // Auto-generate OrderCode on page load
    generateOrderCode();

    // Real-time validation on blur
    const validationFields = [
        'customerName', 'customerPhone', 'deliveryAddress',
        'ward', 'district', 'city', 'weight', 'size',
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

    // Calculate fee dynamically
    const feeCalculationFields = ['weight', 'distance', 'deliveryType', 'isFragile', 'isValuable', 'isVehicle'];
    feeCalculationFields.forEach(fieldName => {
        const input = form.querySelector(`input[name="${fieldName}"], select[name="${fieldName}"]`);
        if (input) {
            input.addEventListener('change', recalculateShippingFee);
            input.addEventListener('input', recalculateShippingFee);
        }
    });

    // Handle Collect Money checkbox
    const collectMoneyCheckbox = form.querySelector('input[name="collectMoney"]');
    if (collectMoneyCheckbox) {
        collectMoneyCheckbox.addEventListener('change', function() {
            const collectionAmountInput = form.querySelector('input[name="collectionAmount"]');
            collectionAmountInput.disabled = !this.checked;
            if (!this.checked) {
                collectionAmountInput.value = '';
                clearFieldError('collectionAmount');
            }
        });
    }

    // Reload products and regenerate OrderCode when modal is shown
    const modal = document.getElementById('createOrderModal');
    if (modal) {
        modal.addEventListener('shown.bs.modal', function() {
            generateOrderCode();
        });
    }
});

/**
 * Generate Order Code auto-matically
 * Format: DH + yyyyMMddHHmmss + 3-digit random
 * Example: DH20260407104530987
 */
function generateOrderCode() {
    const now = new Date();
    const year = now.getFullYear();
    const month = String(now.getMonth() + 1).padStart(2, '0');
    const day = String(now.getDate()).padStart(2, '0');
    const hours = String(now.getHours()).padStart(2, '0');
    const minutes = String(now.getMinutes()).padStart(2, '0');
    const seconds = String(now.getSeconds()).padStart(2, '0');
    const timestamp = `${year}${month}${day}${hours}${minutes}${seconds}`;
    const random = Math.floor(Math.random() * 900) + 100;
    const orderCode = `DH${timestamp}${random}`;
    
    const orderCodeInput = document.getElementById('orderCode');
    const orderCodeValue = document.getElementById('orderCodeValue');
    
    if (orderCodeInput) orderCodeInput.value = orderCode;
    if (orderCodeValue) orderCodeValue.textContent = orderCode;
}

/**
 * Validate a single field
 */
function validateSingleField(fieldName) {
    const form = document.getElementById('createOrderForm');
    if (!form) return;
    
    const input = form.querySelector(`input[name="${fieldName}"], select[name="${fieldName}"], textarea[name="${fieldName}"]`);
    if (!input) return;
    
    let isValid = true;
    let errorMessage = '';
    
    const value = input.value.trim();
    
    switch(fieldName) {
        case 'customerName':
            if (!value) {
                isValid = false;
                errorMessage = 'Tên khách hàng không được để trống';
            } else if (value.length < 2) {
                isValid = false;
                errorMessage = 'Tên khách hàng phải ít nhất 2 ký tự';
            }
            break;
        case 'customerPhone':
            if (!value) {
                isValid = false;
                errorMessage = 'Số điện thoại không được để trống';
            } else if (!/^0\d{9,10}$/.test(value)) {
                isValid = false;
                errorMessage = 'Số điện thoại phải bắt đầu 0 và có 10-11 chữ số';
            }
            break;
        case 'deliveryAddress':
            if (!value) {
                isValid = false;
                errorMessage = 'Địa chỉ giao hàng không được để trống';
            } else if (value.length < 5) {
                isValid = false;
                errorMessage = 'Địa chỉ phải ít nhất 5 ký tự';
            }
            break;
        case 'ward':
        case 'district':
        case 'city':
            if (!value) {
                isValid = false;
                errorMessage = `${fieldName === 'ward' ? 'Phường/Xã' : fieldName === 'district' ? 'Quận/Huyện' : 'Thành phố'} không được để trống`;
            }
            break;
        case 'weight':
            if (!value) {
                isValid = false;
                errorMessage = 'Trọng lượng không được để trống';
            } else {
                const weight = parseFloat(value);
                if (isNaN(weight) || weight <= 0 || weight > 1000) {
                    isValid = false;
                    errorMessage = 'Trọng lượng phải từ 0.01 đến 1000 kg';
                }
            }
            break;
        case 'distance':
            if (!value) {
                isValid = false;
                errorMessage = 'Khoảng cách không được để trống';
            } else {
                const distance = parseFloat(value);
                if (isNaN(distance) || distance < 0) {
                    isValid = false;
                    errorMessage = 'Khoảng cách phải lớn hơn hoặc bằng 0';
                }
            }
            break;
    }
    
    if (isValid) {
        clearFieldError(fieldName);
    } else {
        setFieldError(fieldName, errorMessage);
    }
}

/**
 * Set field error styling and message
 */
function setFieldError(fieldName, errorMessage) {
    const form = document.getElementById('createOrderForm');
    if (!form) return;
    
    const input = form.querySelector(`input[name="${fieldName}"], select[name="${fieldName}"], textarea[name="${fieldName}"]`);
    if (!input) return;
    
    input.classList.add('is-invalid');
    input.classList.remove('is-valid');
    
    let feedback = input.parentElement.querySelector('.invalid-feedback');
    if (!feedback) {
        feedback = document.createElement('div');
        feedback.className = 'invalid-feedback d-block';
        input.parentElement.appendChild(feedback);
    }
    feedback.textContent = errorMessage;
}

/**
 * Clear field error
 */
function clearFieldError(fieldName) {
    const form = document.getElementById('createOrderForm');
    if (!form) return;
    
    const input = form.querySelector(`input[name="${fieldName}"], select[name="${fieldName}"], textarea[name="${fieldName}"]`);
    if (!input) return;
    
    input.classList.remove('is-invalid');
    input.classList.add('is-valid');
    
    const feedback = input.parentElement.querySelector('.invalid-feedback');
    if (feedback) {
        feedback.remove();
    }
}

/**
 * Recalculate shipping fee based on input values
 */
function recalculateShippingFee() {
    const form = document.getElementById('createOrderForm');
    if (!form) return;
    
    const weight = parseFloat(form.querySelector('input[name="weight"]')?.value || 0);
    const distance = parseFloat(form.querySelector('input[name="distance"]')?.value || 0);
    const deliveryType = form.querySelector('select[name="deliveryType"]')?.value || '0';
    const isFragile = form.querySelector('input[name="isFragile"]')?.checked || false;
    const isValuable = form.querySelector('input[name="isValuable"]')?.checked || false;
    const isVehicle = form.querySelector('input[name="isVehicle"]')?.checked || false;
    
    if (!weight || !distance) {
        return;
    }
    
    const BASE_NORMAL = 15000;
    const BASE_EXPRESS = 25000;
    const PER_KM = 5000;
    const PER_KG = 2000;
    const FRAGILE_FEE = 10000;
    const VALUABLE_FEE = 15000;
    const VEHICLE_FEE = 100000;
    
    let fee = deliveryType === '1' ? BASE_EXPRESS : BASE_NORMAL;
    fee += distance * PER_KM;
    fee += weight * PER_KG;
    
    if (isFragile) fee += FRAGILE_FEE;
    if (isValuable) fee += VALUABLE_FEE;
    if (isVehicle) fee += VEHICLE_FEE;
    
    const feeDisplay = form.querySelector('#estimatedFee');
    if (feeDisplay) {
        feeDisplay.textContent = fee.toLocaleString('vi-VN');
    }
}

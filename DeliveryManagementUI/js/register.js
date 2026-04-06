// register.js - Comprehensive Registration Form Validation

// Validation Helper Functions
class RegistrationValidator {
    // Validate Full Name (2-100 chars, only letters and spaces)
    static validateFullName(fullName) {
        if (!fullName || fullName.trim().length === 0) {
            return { isValid: false, message: "Họ và tên không được để trống" };
        }
        
        fullName = fullName.trim();
        
        if (fullName.length < 2) {
            return { isValid: false, message: "Họ và tên phải có ít nhất 2 ký tự" };
        }
        
        if (fullName.length > 100) {
            return { isValid: false, message: "Họ và tên không được vượt quá 100 ký tự" };
        }
        
        // Allow letters (including Vietnamese) and spaces only
        const nameRegex = /^[a-zA-Z\s\u0100-\u0177\u01A0-\u01A1\u1EA0-\u1EFF]+$/;
        if (!nameRegex.test(fullName)) {
            return { isValid: false, message: "Họ và tên chỉ được chứa chữ cái và khoảng trắng" };
        }
        
        return { isValid: true, message: "" };
    }
    
    // Validate Email
    static validateEmail(email) {
        if (!email || email.trim().length === 0) {
            return { isValid: false, message: "Email không được để trống" };
        }
        
        email = email.trim();
        
        if (email.includes(" ")) {
            return { isValid: false, message: "Email không được chứa khoảng trắng" };
        }
        
        if (email.length > 255) {
            return { isValid: false, message: "Email không được vượt quá 255 ký tự" };
        }
        
        // Simple email validation
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(email)) {
            return { isValid: false, message: "Định dạng email không hợp lệ" };
        }
        
        return { isValid: true, message: "" };
    }
    
    // Validate Phone Number (10-11 digits, starts with 0)
    static validatePhoneNumber(phone) {
        if (!phone || phone.trim().length === 0) {
            return { isValid: false, message: "Số điện thoại không được để trống" };
        }
        
        phone = phone.trim();
        
        // Only digits allowed
        if (!/^\d+$/.test(phone)) {
            return { isValid: false, message: "Số điện thoại chỉ được chứa chữ số" };
        }
        
        if (phone.length < 10 || phone.length > 11) {
            return { isValid: false, message: "Số điện thoại phải từ 10 đến 11 chữ số" };
        }
        
        if (!phone.startsWith("0")) {
            return { isValid: false, message: "Số điện thoại phải bắt đầu với số 0" };
        }
        
        return { isValid: true, message: "" };
    }
    
    // Validate Username (5-50 chars, letters and numbers only)
    static validateUsername(username) {
        if (!username || username.trim().length === 0) {
            return { isValid: false, message: "Tên đăng nhập không được để trống" };
        }
        
        username = username.trim();
        
        if (username.length < 5) {
            return { isValid: false, message: "Tên đăng nhập phải có ít nhất 5 ký tự" };
        }
        
        if (username.length > 50) {
            return { isValid: false, message: "Tên đăng nhập không được vượt quá 50 ký tự" };
        }
        
        // Only letters and numbers
        if (!/^[a-zA-Z0-9]+$/.test(username)) {
            return { isValid: false, message: "Tên đăng nhập chỉ được chứa chữ cái và số" };
        }
        
        return { isValid: true, message: "" };
    }
    
    // Validate Password
    static validatePassword(password) {
        if (!password || password.length === 0) {
            return { isValid: false, message: "Mật khẩu không được để trống" };
        }
        
        if (password.includes(" ")) {
            return { isValid: false, message: "Mật khẩu không được chứa khoảng trắng" };
        }
        
        if (password.length < 8) {
            return { isValid: false, message: "Mật khẩu phải có ít nhất 8 ký tự" };
        }
        
        if (!/[A-Z]/.test(password)) {
            return { isValid: false, message: "Mật khẩu phải chứa ít nhất 1 chữ hoa (A-Z)" };
        }
        
        if (!/[a-z]/.test(password)) {
            return { isValid: false, message: "Mật khẩu phải chứa ít nhất 1 chữ thường (a-z)" };
        }
        
        if (!/[0-9]/.test(password)) {
            return { isValid: false, message: "Mật khẩu phải chứa ít nhất 1 chữ số (0-9)" };
        }
        
        if (!/[!@#$%^&*]/.test(password)) {
            return { isValid: false, message: "Mật khẩu phải chứa ít nhất 1 ký tự đặc biệt (!@#$%^&*)" };
        }
        
        return { isValid: true, message: "" };
    }
    
    // Validate Confirm Password
    static validateConfirmPassword(password, confirmPassword) {
        if (!confirmPassword || confirmPassword.length === 0) {
            return { isValid: false, message: "Xác nhận mật khẩu không được để trống" };
        }
        
        if (password !== confirmPassword) {
            return { isValid: false, message: "Xác nhận mật khẩu không khớp với mật khẩu" };
        }
        
        return { isValid: true, message: "" };
    }
    
    // Check password strength
    static getPasswordStrength(password) {
        if (!password || password.length === 0) return "Chưa nhập";
        
        let strength = 0;
        if (password.length >= 8) strength++;
        if (/[A-Z]/.test(password)) strength++;
        if (/[a-z]/.test(password)) strength++;
        if (/[0-9]/.test(password)) strength++;
        if (/[!@#$%^&*]/.test(password)) strength++;
        
        if (strength < 3) return "Yếu";
        if (strength < 5) return "Trung bình";
        return "Mạnh";
    }
}

// Form Elements
const form = document.getElementById('registerForm');
const fullNameInput = document.getElementById('fullName');
const emailInput = document.getElementById('email');
const phoneInput = document.getElementById('phoneNumber');
const usernameInput = document.getElementById('username');
const passwordInput = document.getElementById('password');
const confirmPasswordInput = document.getElementById('confirmPassword');
const acceptTermsCheckbox = document.getElementById('acceptTerms');
const submitBtn = form.querySelector('button[type="submit"]');

// Password requirement elements
const reqLength = document.getElementById('req-length');
const reqUppercase = document.getElementById('req-uppercase');
const reqLowercase = document.getElementById('req-lowercase');
const reqNumber = document.getElementById('req-number');
const reqSpecial = document.getElementById('req-special');

// Password strength meter
const strengthMeterFill = document.getElementById('strengthMeterFill');
const strengthLabel = document.getElementById('strengthLabel');

// Helper to show/hide error messages
function showFieldError(field, message) {
    const inputElement = document.getElementById(field);
    let errorDiv = inputElement?.parentElement?.querySelector('.invalid-feedback');
    
    if (!errorDiv) {
        errorDiv = document.createElement('div');
        errorDiv.className = 'invalid-feedback d-block';
        inputElement?.parentElement?.appendChild(errorDiv);
    }
    
    errorDiv.textContent = message;
    inputElement?.classList.add('is-invalid');
    inputElement?.classList.remove('is-valid');
}

function clearFieldError(field) {
    const inputElement = document.getElementById(field);
    const errorDiv = inputElement?.parentElement?.querySelector('.invalid-feedback');
    
    if (errorDiv) {
        errorDiv.textContent = '';
    }
    
    inputElement?.classList.remove('is-invalid');
}

function markFieldValid(field) {
    const inputElement = document.getElementById(field);
    inputElement?.classList.add('is-valid');
    inputElement?.classList.remove('is-invalid');
    clearFieldError(field);
}

// Real-time validation for Full Name
fullNameInput?.addEventListener('blur', function() {
    const validation = RegistrationValidator.validateFullName(this.value);
    if (validation.isValid) {
        markFieldValid('fullName');
    } else {
        showFieldError('fullName', validation.message);
    }
});

// Real-time validation for Email
emailInput?.addEventListener('blur', function() {
    const validation = RegistrationValidator.validateEmail(this.value);
    if (validation.isValid) {
        markFieldValid('email');
    } else {
        showFieldError('email', validation.message);
    }
});

// Real-time validation for Phone Number
phoneInput?.addEventListener('blur', function() {
    const validation = RegistrationValidator.validatePhoneNumber(this.value);
    if (validation.isValid) {
        markFieldValid('phoneNumber');
    } else {
        showFieldError('phoneNumber', validation.message);
    }
});

// Real-time validation for Username
usernameInput?.addEventListener('blur', function() {
    const validation = RegistrationValidator.validateUsername(this.value);
    if (validation.isValid) {
        markFieldValid('username');
    } else {
        showFieldError('username', validation.message);
    }
});

// Real-time password strength update
passwordInput?.addEventListener('input', function() {
    const password = this.value;
    
    // Update password requirements
    const hasLength = password.length >= 8;
    const hasUppercase = /[A-Z]/.test(password);
    const hasLowercase = /[a-z]/.test(password);
    const hasNumber = /[0-9]/.test(password);
    const hasSpecial = /[!@#$%^&*]/.test(password);
    
    updateRequirement(reqLength, hasLength);
    updateRequirement(reqUppercase, hasUppercase);
    updateRequirement(reqLowercase, hasLowercase);
    updateRequirement(reqNumber, hasNumber);
    updateRequirement(reqSpecial, hasSpecial);
    
    // Update strength meter
    updatePasswordStrengthMeter(password);
});

function updateRequirement(element, isMet) {
    if (element) {
        if (isMet) {
            element.classList.add('met');
        } else {
            element.classList.remove('met');
        }
    }
}

function updatePasswordStrengthMeter(password) {
    const strength = RegistrationValidator.getPasswordStrength(password);
    
    strengthMeterFill.className = 'strength-meter-fill';
    strengthLabel.className = '';
    
    if (password.length === 0) {
        strengthMeterFill.style.width = '0%';
        strengthLabel.textContent = 'Chưa nhập';
    } else if (strength === 'Yếu') {
        strengthMeterFill.classList.add('weak');
        strengthMeterFill.style.width = '33%';
        strengthLabel.classList.add('weak');
        strengthLabel.textContent = 'Yếu';
    } else if (strength === 'Trung bình') {
        strengthMeterFill.classList.add('medium');
        strengthMeterFill.style.width = '66%';
        strengthLabel.classList.add('medium');
        strengthLabel.textContent = 'Trung bình';
    } else {
        strengthMeterFill.classList.add('strong');
        strengthMeterFill.style.width = '100%';
        strengthLabel.classList.add('strong');
        strengthLabel.textContent = 'Mạnh';
    }
}

// Real-time validation for Confirm Password
confirmPasswordInput?.addEventListener('blur', function() {
    const validation = RegistrationValidator.validateConfirmPassword(
        passwordInput.value,
        this.value
    );
    if (validation.isValid) {
        markFieldValid('confirmPassword');
    } else {
        showFieldError('confirmPassword', validation.message);
    }
});

// Update submit button state
function updateSubmitButtonState() {
    const allFieldsValid = 
        RegistrationValidator.validateFullName(fullNameInput?.value || '').isValid &&
        RegistrationValidator.validateEmail(emailInput?.value || '').isValid &&
        RegistrationValidator.validatePhoneNumber(phoneInput?.value || '').isValid &&
        RegistrationValidator.validateUsername(usernameInput?.value || '').isValid &&
        RegistrationValidator.validatePassword(passwordInput?.value || '').isValid &&
        RegistrationValidator.validateConfirmPassword(passwordInput?.value || '', confirmPasswordInput?.value || '').isValid &&
        acceptTermsCheckbox?.checked;
    
    submitBtn.disabled = !allFieldsValid;
}

// Add event listeners for real-time button state update
fullNameInput?.addEventListener('input', updateSubmitButtonState);
emailInput?.addEventListener('input', updateSubmitButtonState);
phoneInput?.addEventListener('input', updateSubmitButtonState);
usernameInput?.addEventListener('input', updateSubmitButtonState);
passwordInput?.addEventListener('input', updateSubmitButtonState);
confirmPasswordInput?.addEventListener('input', updateSubmitButtonState);
acceptTermsCheckbox?.addEventListener('change', updateSubmitButtonState);

// Form submission
form?.addEventListener('submit', async function(e) {
    e.preventDefault();
    
    const alertDiv = document.getElementById('registerAlert');
    alertDiv.innerHTML = '';
    
    // Validate all fields
    const fullNameValidation = RegistrationValidator.validateFullName(fullNameInput?.value || '');
    if (!fullNameValidation.isValid) {
        showFieldError('fullName', fullNameValidation.message);
        return;
    }
    
    const emailValidation = RegistrationValidator.validateEmail(emailInput?.value || '');
    if (!emailValidation.isValid) {
        showFieldError('email', emailValidation.message);
        return;
    }
    
    const phoneValidation = RegistrationValidator.validatePhoneNumber(phoneInput?.value || '');
    if (!phoneValidation.isValid) {
        showFieldError('phoneNumber', phoneValidation.message);
        return;
    }
    
    const usernameValidation = RegistrationValidator.validateUsername(usernameInput?.value || '');
    if (!usernameValidation.isValid) {
        showFieldError('username', usernameValidation.message);
        return;
    }
    
    const passwordValidation = RegistrationValidator.validatePassword(passwordInput?.value || '');
    if (!passwordValidation.isValid) {
        showFieldError('password', passwordValidation.message);
        return;
    }
    
    const confirmPasswordValidation = RegistrationValidator.validateConfirmPassword(
        passwordInput?.value || '',
        confirmPasswordInput?.value || ''
    );
    if (!confirmPasswordValidation.isValid) {
        showFieldError('confirmPassword', confirmPasswordValidation.message);
        return;
    }
    
    if (!acceptTermsCheckbox?.checked) {
        alertDiv.innerHTML = `
            <div class="alert alert-danger d-flex align-items-center">
                <i class="fas fa-exclamation-triangle me-2"></i>
                <span>Bạn phải đồng ý với Điều khoản dịch vụ và Chính sách bảo mật</span>
            </div>
        `;
        return;
    }
    
    // Prepare data
    const data = {
        fullName: fullNameInput?.value.trim(),
        email: emailInput?.value.trim().toLowerCase(),
        phoneNumber: phoneInput?.value.trim(),
        username: usernameInput?.value.trim().toLowerCase(),
        password: passwordInput?.value,
        confirmPassword: confirmPasswordInput?.value,
        acceptTerms: acceptTermsCheckbox?.checked
    };
    
    // Disable submit button
    submitBtn.disabled = true;
    const originalText = submitBtn.innerHTML;
    submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Đang đăng ký...';
    
    try {
        const result = await apiService.register(data);
        
        if (result && result.success) {
            alertDiv.innerHTML = `
                <div class="alert alert-success d-flex align-items-center">
                    <i class="fas fa-check-circle me-2"></i>
                    <span>Đăng ký thành công! Đang chuyển đến trang đăng nhập...</span>
                </div>
            `;
            
            form.reset();
            updateSubmitButtonState();
            
            setTimeout(() => {
                window.location.href = 'login.html';
            }, 2000);
        } else {
            const message = result?.message || 'Đăng ký thất bại! Vui lòng thử lại.';
            const field = result?.field;
            
            if (field) {
                showFieldError(field, message);
            } else {
                alertDiv.innerHTML = `
                    <div class="alert alert-danger d-flex align-items-center">
                        <i class="fas fa-exclamation-triangle me-2"></i>
                        <span>${message}</span>
                    </div>
                `;
            }
            
            submitBtn.disabled = false;
            submitBtn.innerHTML = originalText;
        }
    } catch (err) {
        console.error('Register error:', err);
        alertDiv.innerHTML = `
            <div class="alert alert-danger d-flex align-items-center">
                <i class="fas fa-exclamation-triangle me-2"></i>
                <span>${err.message || 'Đăng ký thất bại! Vui lòng thử lại.'}</span>
            </div>
        `;
        submitBtn.disabled = false;
        submitBtn.innerHTML = originalText;
    }
});

// Initialize submit button state
updateSubmitButtonState();

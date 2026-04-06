// reset-password.js

// Utility functions
const utils = {
    showToast(message, type = 'info') {
        // If utils exists globally, use it; otherwise create simple alert
        if (window.utils && window.utils.showToast) {
            window.utils.showToast(message, type);
        } else {
            console.log(`[${type}] ${message}`);
        }
    }
};

// Get token from URL
function getTokenFromURL() {
    const params = new URLSearchParams(window.location.search);
    return params.get('token');
}

// Password validation
function validatePassword(password) {
    const validation = {
        length: password.length >= 8,
        hasUpper: /[A-Z]/.test(password),
        hasLower: /[a-z]/.test(password),
        hasNumber: /[0-9]/.test(password),
        hasSpecial: /[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]/.test(password)
    };
    
    return Object.values(validation).every(v => v);
}

// Calculate password strength
function calculatePasswordStrength(password) {
    let strength = 0;
    if (password.length >= 8) strength++;
    if (password.length >= 12) strength++;
    if (/[A-Z]/.test(password)) strength++;
    if (/[a-z]/.test(password)) strength++;
    if (/[0-9]/.test(password)) strength++;
    if (/[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]/.test(password)) strength++;
    return strength;
}

// Update password strength indicator
function updatePasswordStrength(password) {
    const strengthBar = document.getElementById('strengthBar');
    const strengthText = document.getElementById('strengthText');
    
    if (!password) {
        strengthBar.style.width = '0%';
        strengthBar.className = 'password-strength-bar';
        strengthText.textContent = '';
        return;
    }
    
    const strength = calculatePasswordStrength(password);
    const percentage = (strength / 6) * 100;
    
    strengthBar.style.width = percentage + '%';
    strengthBar.className = 'password-strength-bar';
    
    if (strength <= 2) {
        strengthBar.classList.add('strength-weak');
        strengthText.textContent = 'Yếu';
        strengthText.style.color = '#f44336';
    } else if (strength <= 4) {
        strengthBar.classList.add('strength-fair');
        strengthText.textContent = 'Trung bình';
        strengthText.style.color = '#ff9800';
    } else {
        strengthBar.classList.add('strength-good');
        strengthText.textContent = 'Mạnh';
        strengthText.style.color = '#8bc34a';
    }
}

// Initialize
document.addEventListener('DOMContentLoaded', function() {
    const token = getTokenFromURL();
    
    if (!token) {
        const alertDiv = document.getElementById('resetAlert');
        alertDiv.innerHTML = `
            <div class="alert alert-danger">
                <i class="fas fa-exclamation-triangle me-2"></i>
                Token không hợp lệ. Vui lòng yêu cầu đặt lại mật khẩu lại.
            </div>
        `;
        document.getElementById('resetForm').style.display = 'none';
        return;
    }

    // Password visibility toggle
    document.getElementById('togglePassword').addEventListener('click', function() {
        const passwordInput = document.getElementById('newPassword');
        const icon = this.querySelector('i');
        
        if (passwordInput.type === 'password') {
            passwordInput.type = 'text';
            icon.classList.remove('fa-eye');
            icon.classList.add('fa-eye-slash');
        } else {
            passwordInput.type = 'password';
            icon.classList.remove('fa-eye-slash');
            icon.classList.add('fa-eye');
        }
    });

    // Confirm password visibility toggle
    document.getElementById('toggleConfirmPassword').addEventListener('click', function() {
        const passwordInput = document.getElementById('confirmPassword');
        const icon = this.querySelector('i');
        
        if (passwordInput.type === 'password') {
            passwordInput.type = 'text';
            icon.classList.remove('fa-eye');
            icon.classList.add('fa-eye-slash');
        } else {
            passwordInput.type = 'password';
            icon.classList.remove('fa-eye-slash');
            icon.classList.add('fa-eye');
        }
    });

    // Password strength update
    document.getElementById('newPassword').addEventListener('input', function() {
        updatePasswordStrength(this.value);
    });

    // Form submission
    document.getElementById('resetForm').addEventListener('submit', async function(e) {
        e.preventDefault();
        
        const newPassword = document.getElementById('newPassword').value.trim();
        const confirmPassword = document.getElementById('confirmPassword').value.trim();
        const alertDiv = document.getElementById('resetAlert');
        const submitBtn = this.querySelector('button[type="submit"]');
        
        // Clear previous alerts
        alertDiv.innerHTML = '';
        
        // Validate passwords match
        if (newPassword !== confirmPassword) {
            alertDiv.innerHTML = `
                <div class="alert alert-danger">
                    <i class="fas fa-exclamation-triangle me-2"></i>
                    Mật khẩu xác nhận không khớp!
                </div>
            `;
            return;
        }

        // Validate password strength
        if (!validatePassword(newPassword)) {
            alertDiv.innerHTML = `
                <div class="alert alert-danger">
                    <i class="fas fa-exclamation-triangle me-2"></i>
                    Mật khẩu phải có ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt!
                </div>
            `;
            return;
        }

        // Disable submit button and show loading
        submitBtn.disabled = true;
        submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Đang xử lý...';
        
        try {
            const result = await apiService.resetPassword({
                token: token,
                newPassword: newPassword
            });
            
            if (result && result.success) {
                alertDiv.innerHTML = `
                    <div class="alert alert-success">
                        <i class="fas fa-check-circle me-2"></i>
                        Đặt lại mật khẩu thành công! Chuyển hướng đến trang đăng nhập...
                    </div>
                `;
                
                // Redirect to login page after 2 seconds
                setTimeout(() => {
                    window.location.href = 'login.html';
                }, 2000);
            } else {
                alertDiv.innerHTML = `
                    <div class="alert alert-danger">
                        <i class="fas fa-exclamation-triangle me-2"></i>
                        ${result.message || 'Đặt lại mật khẩu thất bại!'}
                    </div>
                `;
            }
        } catch (err) {
            console.error('Reset password error:', err);
            alertDiv.innerHTML = `
                <div class="alert alert-danger">
                    <i class="fas fa-exclamation-triangle me-2"></i>
                    Lỗi: ${err.message || 'Đặt lại mật khẩu thất bại! Vui lòng thử lại.'}
                </div>
            `;
        } finally {
            submitBtn.disabled = false;
            submitBtn.innerHTML = 'Đặt lại mật khẩu';
        }
    });
});

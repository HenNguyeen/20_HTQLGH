// register.js

document.getElementById('registerForm').addEventListener('submit', async function(e) {
    e.preventDefault();
    
    const form = this;
    const alertDiv = document.getElementById('registerAlert');
    const submitBtn = this.querySelector('button[type="submit"]');
    
    // Clear previous alerts
    alertDiv.innerHTML = '';
    
    // Validate password match
    if (form.password.value !== form.confirmPassword.value) {
        alertDiv.innerHTML = `
            <div class="alert alert-danger">
                <i class="fas fa-exclamation-triangle me-2"></i>
                Mật khẩu nhập lại không khớp!
            </div>
        `;
        return;
    }
    
    // Validate password strength
    if (form.password.value.length < 6) {
        alertDiv.innerHTML = `
            <div class="alert alert-danger">
                <i class="fas fa-exclamation-triangle me-2"></i>
                Mật khẩu phải có ít nhất 6 ký tự!
            </div>
        `;
        return;
    }
    
    const data = {
        fullName: form.fullName.value.trim(),
        email: form.email.value.trim(),
        phoneNumber: form.phoneNumber.value.trim(),
        username: form.username.value.trim(),
        password: form.password.value
    };
    
    // Disable submit button
    submitBtn.disabled = true;
    submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Đang đăng ký...';
    
    try {
        const result = await apiService.register(data);
        
        if (result && result.success) {
            alertDiv.innerHTML = `
                <div class="alert alert-success">
                    <i class="fas fa-check-circle me-2"></i>
                    Đăng ký thành công! Đang chuyển đến trang đăng nhập...
                </div>
            `;
            
            // Reset form
            form.reset();
            
            // Redirect to login
            setTimeout(() => {
                window.location.href = 'login.html';
            }, 2000);
        } else {
            alertDiv.innerHTML = `
                <div class="alert alert-danger">
                    <i class="fas fa-exclamation-triangle me-2"></i>
                    ${result.message || 'Đăng ký thất bại! Tài khoản hoặc email đã tồn tại.'}
                </div>
            `;
            submitBtn.disabled = false;
            submitBtn.innerHTML = 'Đăng Ký';
        }
    } catch (err) {
        console.error('Register error:', err);
        alertDiv.innerHTML = `
            <div class="alert alert-danger">
                <i class="fas fa-exclamation-triangle me-2"></i>
                Đăng ký thất bại! Vui lòng thử lại.
            </div>
        `;
        submitBtn.disabled = false;
        submitBtn.innerHTML = 'Đăng Ký';
    }
});

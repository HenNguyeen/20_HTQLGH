// login.js

document.getElementById('loginForm').addEventListener('submit', async function(e) {
    e.preventDefault();
    
    const username = this.username.value.trim();
    const password = this.password.value;
    const remember = document.getElementById('rememberMe').checked;
    const alertDiv = document.getElementById('loginAlert');
    const submitBtn = this.querySelector('button[type="submit"]');
    
    // Clear previous alerts
    alertDiv.innerHTML = '';
    
    // Disable submit button
    submitBtn.disabled = true;
    submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Đang đăng nhập...';
    
    try {
        const result = await apiService.login({ username, password });
        
        if (result && result.token) {
            // Save token and user info
            auth.setToken(result.token, remember);
            auth.setCurrentUser(result.user, remember);
            // Show success message
            alertDiv.innerHTML = `
                <div class="alert alert-success">
                    <i class="fas fa-check-circle me-2"></i>
                    Đăng nhập thành công! Chào mừng ${result.user.fullName}
                </div>
            `;
            // Redirect based on role
            setTimeout(() => {
                if (result.user.role === 'admin') {
                    window.location.href = 'index.html'; // Admin Dashboard
                } else if (result.user.role === 'customer') {
                    window.location.href = 'customer/index.html'; // Customer Area
                } else if (result.user.role === 'shipper') {
                    window.location.href = 'shipper/index.html'; // Shipper Area
                } else {
                    window.location.href = 'index.html'; // Fallback
                }
            }, 1000);
        } else {
            alertDiv.innerHTML = `
                <div class="alert alert-danger">
                    <i class="fas fa-exclamation-triangle me-2"></i>
                    Sai tài khoản hoặc mật khẩu!
                </div>
            `;
            submitBtn.disabled = false;
            submitBtn.innerHTML = 'Đăng Nhập';
        }
    } catch (err) {
        console.error('Login error:', err);
        alertDiv.innerHTML = `
            <div class="alert alert-danger">
                <i class="fas fa-exclamation-triangle me-2"></i>
                Đăng nhập thất bại! Vui lòng thử lại.
            </div>
        `;
        submitBtn.disabled = false;
        submitBtn.innerHTML = 'Đăng Nhập';
    }
});

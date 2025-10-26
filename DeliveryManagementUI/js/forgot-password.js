// forgot-password.js

document.getElementById('forgotForm').addEventListener('submit', async function(e) {
    e.preventDefault();
    
    const email = this.email.value.trim();
    const alertDiv = document.getElementById('forgotAlert');
    const submitBtn = this.querySelector('button[type="submit"]');
    
    // Clear previous alerts
    alertDiv.innerHTML = '';
    
    // Disable submit button
    submitBtn.disabled = true;
    submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Đang gửi...';
    
    try {
        const result = await apiService.forgotPassword({ email });
        
        if (result && result.success) {
            alertDiv.innerHTML = `
                <div class="alert alert-success">
                    <i class="fas fa-check-circle me-2"></i>
                    Yêu cầu đặt lại mật khẩu đã được gửi! Vui lòng kiểm tra email.
                </div>
            `;
            
            // Reset form
            this.reset();
        } else {
            alertDiv.innerHTML = `
                <div class="alert alert-danger">
                    <i class="fas fa-exclamation-triangle me-2"></i>
                    ${result.message || 'Không tìm thấy email này!'}
                </div>
            `;
        }
    } catch (err) {
        console.error('Forgot password error:', err);
        alertDiv.innerHTML = `
            <div class="alert alert-danger">
                <i class="fas fa-exclamation-triangle me-2"></i>
                Gửi yêu cầu thất bại! Vui lòng thử lại.
            </div>
        `;
    } finally {
        submitBtn.disabled = false;
        submitBtn.innerHTML = 'Gửi Yêu Cầu';
    }
});

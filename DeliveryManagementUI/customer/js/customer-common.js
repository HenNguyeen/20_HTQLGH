// customer-common.js
// Shared logic for customer-only area

// Require login and customer role
(function ensureCustomerArea() {
  if (!auth.requireAuth()) return;
  const user = auth.getCurrentUser();
  if (!user || user.role !== 'customer') {
    utils.showToast('Bạn không có quyền truy cập khu vực Khách hàng.', 'danger');
    setTimeout(() => (window.location.href = '../index.html'), 1200);
    return;
  }

  // Put user name into navbar
  const nameEls = document.querySelectorAll('.user-name');
  nameEls.forEach((el) => (el.textContent = user.fullName || user.username));
})();

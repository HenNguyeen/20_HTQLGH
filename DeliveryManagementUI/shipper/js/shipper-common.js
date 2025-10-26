// shipper-common.js
// Shared logic for shipper-only area

// Require login and shipper role
(function ensureShipperArea() {
  if (!auth.requireAuth()) return;
  const user = auth.getCurrentUser();
  if (!user || user.role !== 'shipper') {
    utils.showToast('Bạn không có quyền truy cập khu vực Shipper.', 'danger');
    setTimeout(() => (window.location.href = '../index.html'), 1200);
    return;
  }

  // Put user name into navbar
  const nameEls = document.querySelectorAll('.user-name');
  nameEls.forEach((el) => (el.textContent = user.fullName || user.username));
})();

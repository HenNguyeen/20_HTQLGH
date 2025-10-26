// Require auth
if (!auth.requireAuth()) {
  // redirect happens in requireAuth
}

async function loadProfile() {
  try {
    const me = await apiService.getMyProfile();
    // Fill fields
    document.getElementById('pfUsername').value = me.username || '';
    document.getElementById('pfRole').value = me.role || '';
    document.getElementById('pfFullName').value = me.fullName || '';
    document.getElementById('pfEmail').value = me.email || '';
    document.getElementById('pfPhone').value = me.phoneNumber || '';
  } catch (e) {
    utils.showToast('Không tải được thông tin tài khoản', 'danger');
  }
}

async function saveProfile() {
  const payload = {
    fullName: document.getElementById('pfFullName').value,
    email: document.getElementById('pfEmail').value,
    phoneNumber: document.getElementById('pfPhone').value
  };
  try {
    const updated = await apiService.updateMyProfile(payload);
    utils.showToast('Đã lưu thay đổi');
    // Update cached current user object if needed
    const cu = auth.getCurrentUser() || {};
    const newUser = { ...cu, fullName: updated.fullName, email: updated.email, phoneNumber: updated.phoneNumber };
    // preserve storage location
    const storedLocal = localStorage.getItem('currentUser') !== null;
    auth.setCurrentUser(newUser, storedLocal);
  } catch (e) {
    utils.showToast('Lưu thất bại: ' + (e?.message || ''), 'danger');
  }
}

async function changePassword() {
  const currentPassword = document.getElementById('pfCurrentPassword').value;
  const newPassword = document.getElementById('pfNewPassword').value;
  if (!newPassword) {
    utils.showToast('Vui lòng nhập mật khẩu mới', 'warning');
    return;
  }
  try {
    await apiService.changeMyPassword({ currentPassword, newPassword });
    utils.showToast('Đổi mật khẩu thành công');
    document.getElementById('pfCurrentPassword').value = '';
    document.getElementById('pfNewPassword').value = '';
  } catch (e) {
    utils.showToast('Đổi mật khẩu thất bại', 'danger');
  }
}

document.addEventListener('DOMContentLoaded', loadProfile);

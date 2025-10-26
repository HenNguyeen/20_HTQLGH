// Ensure only admin can access
if (!auth.requireAuth() || !auth.isAdmin()) {
    window.location.href = 'login.html';
}

let users = [];

async function loadUsers() {
    try {
        const data = await apiService.getAllUsers();
        users = data || [];
        renderUsers(users);
        setupHeaderUserInfo();
    } catch (e) {
        const tbody = document.getElementById('usersTableBody');
        utils.showError(tbody, 'Không tải được danh sách tài khoản');
    }
}

function setupHeaderUserInfo() {
    const cu = auth.getCurrentUser();
    document.querySelectorAll('.user-name').forEach(el => el.textContent = cu?.fullName || cu?.username || 'User');
    document.querySelectorAll('.user-role').forEach(el => el.textContent = cu?.role || '');
}

function renderUsers(list) {
    const tbody = document.getElementById('usersTableBody');
    if (!list || list.length === 0) {
        utils.showEmpty(tbody, 'Chưa có tài khoản');
        return;
    }
    tbody.innerHTML = list.map((u, idx) => `
        <tr>
            <td>${idx + 1}</td>
            <td>${u.username}</td>
            <td>${u.fullName || '-'}</td>
            <td>${u.email || '-'}</td>
            <td>${u.phoneNumber || '-'}</td>
            <td><span class="badge ${u.role==='admin'?'bg-danger':u.role==='shipper'?'bg-info':'bg-secondary'}">${u.role}</span></td>
            <td>
                <button class="btn btn-sm btn-outline-primary me-1" onclick="openEditUser(${u.userId})"><i class="fas fa-edit"></i></button>
                <button class="btn btn-sm btn-outline-warning me-1" onclick="openResetPassword(${u.userId})"><i class="fas fa-key"></i></button>
                <button class="btn btn-sm btn-outline-danger" onclick="deleteUser(${u.userId})"><i class="fas fa-trash"></i></button>
            </td>
        </tr>
    `).join('');
}

// Filters
document.getElementById('searchUsers').addEventListener('input', applyUserFilters);
document.getElementById('roleFilter').addEventListener('change', applyUserFilters);

function applyUserFilters() {
    const q = document.getElementById('searchUsers').value.toLowerCase().trim();
    const role = document.getElementById('roleFilter').value;
    const filtered = users.filter(u => {
        const matchQ = !q || `${u.username} ${u.fullName||''} ${u.email||''} ${u.phoneNumber||''}`.toLowerCase().includes(q);
        const matchRole = !role || u.role === role;
        return matchQ && matchRole;
    });
    renderUsers(filtered);
}

function resetUserFilters() {
    document.getElementById('searchUsers').value = '';
    document.getElementById('roleFilter').value = '';
    renderUsers(users);
}

// Create user
async function createUser() {
    const form = document.getElementById('createUserForm');
    const formData = Object.fromEntries(new FormData(form));
    try {
        const created = await apiService.createUserAccount(formData);
        utils.showToast('Tạo tài khoản thành công');
        bootstrap.Modal.getInstance(document.getElementById('createUserModal')).hide();
        form.reset();
        await loadUsers();
    } catch (e) {
        utils.showToast('Tạo tài khoản thất bại', 'danger');
    }
}

// Edit user
function openEditUser(id) {
    const u = users.find(x => x.userId === id);
    if (!u) return;
    document.getElementById('editUserId').value = id;
    const form = document.getElementById('editUserForm');
    form.fullName.value = u.fullName || '';
    form.email.value = u.email || '';
    form.phoneNumber.value = u.phoneNumber || '';
    form.role.value = u.role || 'customer';
    new bootstrap.Modal(document.getElementById('editUserModal')).show();
}

async function saveUserEdit() {
    const id = parseInt(document.getElementById('editUserId').value, 10);
    const form = document.getElementById('editUserForm');
    const payload = {
        fullName: form.fullName.value,
        email: form.email.value,
        phoneNumber: form.phoneNumber.value,
        role: form.role.value
    };
    try {
        await apiService.updateUserAccount(id, payload);
        utils.showToast('Cập nhật thành công');
        bootstrap.Modal.getInstance(document.getElementById('editUserModal')).hide();
        await loadUsers();
    } catch (e) {
        utils.showToast('Cập nhật thất bại', 'danger');
    }
}

// Reset password
function openResetPassword(id) {
    document.getElementById('resetUserId').value = id;
    document.getElementById('newPassword').value = '';
    new bootstrap.Modal(document.getElementById('resetPasswordModal')).show();
}

async function confirmResetPassword() {
    const id = parseInt(document.getElementById('resetUserId').value, 10);
    const pw = document.getElementById('newPassword').value;
    try {
        await apiService.resetUserPassword(id, { newPassword: pw });
        utils.showToast('Đặt lại mật khẩu thành công');
        bootstrap.Modal.getInstance(document.getElementById('resetPasswordModal')).hide();
    } catch (e) {
        utils.showToast('Đặt lại mật khẩu thất bại', 'danger');
    }
}

// Delete user
async function deleteUser(id) {
    if (!confirm('Xóa tài khoản này? Hành động không thể hoàn tác.')) return;
    try {
        await apiService.deleteUserAccount(id);
        utils.showToast('Đã xóa tài khoản');
        await loadUsers();
    } catch (e) {
        utils.showToast('Xóa thất bại (có thể do ràng buộc dữ liệu)', 'danger');
    }
}

// Initialize
document.addEventListener('DOMContentLoaded', loadUsers);

// Staff Management JavaScript

let allStaff = [];
let filteredStaff = [];
let currentModal = null;

// Initialize page on load
document.addEventListener('DOMContentLoaded', function() {
    // Check authentication
    if (!auth.requireAuth()) {
        return;
    }
    
    // Check if user is admin
    if (!auth.isAdmin()) {
        utils.showToast('Bạn không có quyền truy cập trang này!', 'danger');
        setTimeout(() => {
            window.location.href = 'index.html';
        }, 2000);
        return;
    }
    
    // Display user info
    displayUserInfo();
    
    // Initialize page
    initializePage();
    setupEventListeners();
    loadStaff();
});

// Display user information
function displayUserInfo() {
    const user = auth.getCurrentUser();
    if (!user) {
        auth.logout();
        return;
    }
}

// Initialize page components
function initializePage() {
    // Sidebar toggle
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebar = document.getElementById('sidebar');
    
    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', function() {
            sidebar.classList.toggle('collapsed');
        });
    }

    // Get modal instances
    currentModal = new bootstrap.Modal(document.getElementById('addStaffModal'));
}

// Setup event listeners
function setupEventListeners() {
    // Search input
    document.getElementById('searchStaff').addEventListener('input', filterStaff);
    
    // Status filter
    document.getElementById('statusFilter').addEventListener('change', filterStaff);
    
    // Vehicle filter
    document.getElementById('vehicleFilter').addEventListener('change', filterStaff);
}

// Load all staff
async function loadStaff() {
    try {
        showLoading();
        const data = await apiService.getDeliveryStaff();
        allStaff = data;
        filteredStaff = data;
        updateStats();
        renderStaffCards();
    } catch (error) {
        console.error('Error loading staff:', error);
        showError('Không thể tải danh sách nhân viên');
    }
}

// Update statistics
function updateStats() {
    const availableCount = allStaff.filter(s => s.isAvailable).length;
    const busyCount = allStaff.filter(s => !s.isAvailable).length;
    const vehicleTypes = new Set(allStaff.map(s => s.vehicleType));
    
    document.getElementById('totalStaff').textContent = allStaff.length;
    document.getElementById('availableStaff').textContent = availableCount;
    document.getElementById('busyStaff').textContent = busyCount;
    document.getElementById('totalVehicles').textContent = vehicleTypes.size;
}

// Filter staff based on search and filters
function filterStaff() {
    const searchTerm = document.getElementById('searchStaff').value.toLowerCase();
    const statusFilter = document.getElementById('statusFilter').value;
    const vehicleFilter = document.getElementById('vehicleFilter').value;
    
    filteredStaff = allStaff.filter(staff => {
        // Search filter
        const matchesSearch = searchTerm === '' || 
            staff.fullName.toLowerCase().includes(searchTerm) ||
            staff.phoneNumber.includes(searchTerm) ||
            staff.vehiclePlate.toLowerCase().includes(searchTerm);
        
        // Status filter
        const matchesStatus = statusFilter === '' || 
            staff.isAvailable.toString() === statusFilter;
        
        // Vehicle filter
        const matchesVehicle = vehicleFilter === '' || 
            staff.vehicleType === vehicleFilter;
        
        return matchesSearch && matchesStatus && matchesVehicle;
    });
    
    renderStaffCards();
}

// Reset all filters
function resetFilters() {
    document.getElementById('searchStaff').value = '';
    document.getElementById('statusFilter').value = '';
    document.getElementById('vehicleFilter').value = '';
    filteredStaff = allStaff;
    renderStaffCards();
}

// Render staff cards
function renderStaffCards() {
    const container = document.getElementById('staffContainer');
    
    if (filteredStaff.length === 0) {
        container.innerHTML = `
            <div class="col-12 text-center py-5">
                <i class="fas fa-users fa-4x text-muted mb-3"></i>
                <p class="text-muted">Không tìm thấy nhân viên nào</p>
            </div>
        `;
        return;
    }
    
    container.innerHTML = filteredStaff.map(staff => `
        <div class="col-md-6 col-lg-4 mb-4">
            <div class="card h-100 shadow-sm hover-shadow">
                <div class="card-body">
                    <div class="d-flex justify-content-between align-items-start mb-3">
                        <div>
                            <h5 class="card-title mb-1">
                                <i class="fas fa-user"></i> ${staff.fullName}
                            </h5>
                            <span class="badge ${staff.isAvailable ? 'bg-success' : 'bg-warning'}">
                                ${staff.isAvailable ? 'Đang rảnh' : 'Đang bận'}
                            </span>
                        </div>
                        <div class="form-check form-switch">
                            <input class="form-check-input" type="checkbox" 
                                id="switch-${staff.staffId}" 
                                ${staff.isAvailable ? 'checked' : ''}
                                onchange="toggleAvailability(${staff.staffId}, this.checked)">
                            <label class="form-check-label" for="switch-${staff.staffId}"></label>
                        </div>
                    </div>
                    
                    <div class="staff-info">
                        <p class="mb-2">
                            <i class="fas fa-phone text-primary"></i>
                            <strong>SĐT:</strong> ${staff.phoneNumber}
                        </p>
                        <p class="mb-2">
                            <i class="fas fa-motorcycle text-info"></i>
                            <strong>Loại xe:</strong> ${staff.vehicleType}
                        </p>
                        <p class="mb-2">
                            <i class="fas fa-id-card text-warning"></i>
                            <strong>Biển số:</strong> ${staff.vehiclePlate}
                        </p>
                        <p class="mb-0">
                            <i class="fas fa-box text-secondary"></i>
                            <strong>Đơn đang giao:</strong> ${staff.currentOrders || 0}
                        </p>
                    </div>
                    
                    <hr>
                    
                    <div class="d-flex gap-2">
                        <button class="btn btn-sm btn-info flex-fill" onclick="viewStaffOrders(${staff.staffId})">
                            <i class="fas fa-box"></i> Xem Đơn
                        </button>
                        <button class="btn btn-sm btn-primary flex-fill" onclick="viewStaffDetails(${staff.staffId})">
                            <i class="fas fa-eye"></i> Chi Tiết
                        </button>
                        <button class="btn btn-sm btn-danger" onclick="deleteStaff(${staff.staffId})">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `).join('');
}

// Toggle staff availability
async function toggleAvailability(staffId, isAvailable) {
    try {
        await apiService.updateStaffAvailability(staffId, isAvailable);
        
        // Update local data
        const staff = allStaff.find(s => s.staffId === staffId);
        if (staff) {
            staff.isAvailable = isAvailable;
        }
        
        updateStats();
        showSuccess(`Đã cập nhật trạng thái nhân viên`);
    } catch (error) {
        console.error('Error updating availability:', error);
        showError('Không thể cập nhật trạng thái');
        
        // Revert checkbox
        const checkbox = document.getElementById(`switch-${staffId}`);
        if (checkbox) {
            checkbox.checked = !isAvailable;
        }
    }
}

// Add new staff
async function addStaff() {
    const form = document.getElementById('addStaffForm');
    
    if (!form.checkValidity()) {
        form.reportValidity();
        return;
    }
    
    const formData = new FormData(form);
    const staffData = {
        fullName: formData.get('fullName'),
        phoneNumber: formData.get('phoneNumber'),
        username: formData.get('username'),
        email: formData.get('email'),
        vehicleType: formData.get('vehicleType'),
        vehiclePlate: formData.get('vehiclePlate'),
        isAvailable: formData.get('isAvailable') === 'on'
    };
    
    try {
        const response = await apiService.addDeliveryStaff(staffData);
        
        // response sẽ chứa: { staff, userAccount, plainPassword }
        allStaff.push(response.staff);
        filteredStaff = allStaff;
        
        updateStats();
        renderStaffCards();
        
        // Close modal and reset form
        currentModal.hide();
        form.reset();
        
        // Hiển thị modal thông tin tài khoản
        showAccountInfo(response);
        
        showSuccess('Thêm nhân viên thành công!');
    } catch (error) {
        console.error('Error adding staff:', error);
        showError('Không thể thêm nhân viên. Có thể username hoặc email đã tồn tại.');
    }
}

// Hiển thị modal thông tin tài khoản sau khi tạo nhân viên
function showAccountInfo(response) {
    document.getElementById('accountFullName').textContent = response.userAccount.fullName;
    document.getElementById('accountUsername').value = response.userAccount.username;
    document.getElementById('accountPassword').value = response.plainPassword;
    document.getElementById('accountEmail').textContent = response.userAccount.email;
    
    // Lưu thông tin để có thể tải về
    window.currentAccountInfo = {
        fullName: response.userAccount.fullName,
        username: response.userAccount.username,
        password: response.plainPassword,
        email: response.userAccount.email,
        phoneNumber: response.staff.phoneNumber
    };
    
    const modal = new bootstrap.Modal(document.getElementById('accountInfoModal'));
    modal.show();
}

// Copy text to clipboard
function copyToClipboard(elementId) {
    const input = document.getElementById(elementId);
    input.select();
    input.setSelectionRange(0, 99999); // For mobile
    
    navigator.clipboard.writeText(input.value).then(() => {
        showSuccess('Đã sao chép vào clipboard!');
    }).catch(() => {
        document.execCommand('copy');
        showSuccess('Đã sao chép vào clipboard!');
    });
}

// Download account info as text file
function downloadAccountInfo() {
    if (!window.currentAccountInfo) return;
    
    const info = window.currentAccountInfo;
    const content = `
THÔNG TIN TÀI KHOẢN NHÂN VIÊN GIAO HÀNG
========================================

Họ tên: ${info.fullName}
Số điện thoại: ${info.phoneNumber}
Email: ${info.email}

THÔNG TIN ĐĂNG NHẬP
-------------------
Tên đăng nhập: ${info.username}
Mật khẩu: ${info.password}

LƯU Ý:
- Vui lòng đổi mật khẩu sau khi đăng nhập lần đầu
- Không chia sẻ thông tin đăng nhập với người khác
- Liên hệ quản trị viên nếu quên mật khẩu

Ngày tạo: ${new Date().toLocaleString('vi-VN')}
    `.trim();
    
    const blob = new Blob([content], { type: 'text/plain;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `TaiKhoan_${info.username}_${Date.now()}.txt`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
    
    showSuccess('Đã tải thông tin tài khoản!');
}

// View staff orders
async function viewStaffOrders(staffId) {
    const modal = new bootstrap.Modal(document.getElementById('staffOrdersModal'));
    const content = document.getElementById('staffOrdersContent');
    
    modal.show();
    
    try {
        const orders = await apiService.getStaffOrders(staffId);
        
        if (orders.length === 0) {
            content.innerHTML = `
                <div class="text-center py-4">
                    <i class="fas fa-box-open fa-4x text-muted mb-3"></i>
                    <p class="text-muted">Nhân viên chưa có đơn hàng nào</p>
                </div>
            `;
            return;
        }
        
        content.innerHTML = `
            <div class="table-responsive">
                <table class="table table-hover">
                    <thead>
                        <tr>
                            <th>Mã Đơn</th>
                            <th>Người Nhận</th>
                            <th>Địa Chỉ</th>
                            <th>Trạng Thái</th>
                            <th>Thao Tác</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${orders.map(order => `
                            <tr>
                                <td>${order.orderCode}</td>
                                <td>${order.customer ? order.customer.fullName : '-'}</td>
                                <td>${order.customer ? order.customer.address : '-'}</td>
                                <td>${getStatusBadge(order.status)}</td>
                                <td>
                                    <button class="btn btn-sm btn-info" onclick="window.open('tracking.html?order=${order.orderCode}', '_blank')">
                                        <i class="fas fa-map-marked-alt"></i>
                                    </button>
                                </td>
                            </tr>
                        `).join('')}
                    </tbody>
                </table>
            </div>
        `;
    } catch (error) {
        console.error('Error loading staff orders:', error);
        content.innerHTML = `
            <div class="alert alert-danger">
                <i class="fas fa-exclamation-triangle"></i>
                Không thể tải danh sách đơn hàng
            </div>
        `;
    }
}

// View staff details
async function viewStaffDetails(staffId) {
    const staff = allStaff.find(s => s.staffId === staffId);
    if (!staff) return;
    
    try {
        const orders = await apiService.getStaffOrders(staffId);
        const completedOrders = orders.filter(o => o.status === 4 || o.status === 5).length;
        const inProgressOrders = orders.filter(o => o.status === 2 || o.status === 3).length;
        
        const alertHtml = `
            <div class="modal fade" id="detailsModal" tabindex="-1">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">
                                <i class="fas fa-user-circle"></i> Chi Tiết Nhân Viên
                            </h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                        </div>
                        <div class="modal-body">
                            <div class="text-center mb-4">
                                <i class="fas fa-user-circle fa-5x text-primary mb-3"></i>
                                <h4>${staff.fullName}</h4>
                                <span class="badge ${staff.isAvailable ? 'bg-success' : 'bg-warning'} mb-2">
                                    ${staff.isAvailable ? 'Đang rảnh' : 'Đang bận'}
                                </span>
                            </div>
                            
                            <div class="row">
                                <div class="col-6 mb-3">
                                    <label class="text-muted small">ID</label>
                                    <p class="mb-0"><strong>${staff.staffId}</strong></p>
                                </div>
                                <div class="col-6 mb-3">
                                    <label class="text-muted small">Số Điện Thoại</label>
                                    <p class="mb-0"><strong>${staff.phoneNumber}</strong></p>
                                </div>
                                <div class="col-6 mb-3">
                                    <label class="text-muted small">Loại Xe</label>
                                    <p class="mb-0"><strong>${staff.vehicleType}</strong></p>
                                </div>
                                <div class="col-6 mb-3">
                                    <label class="text-muted small">Biển Số</label>
                                    <p class="mb-0"><strong>${staff.vehiclePlate}</strong></p>
                                </div>
                            </div>
                            
                            <hr>
                            
                            <h6 class="mb-3">Thống Kê Đơn Hàng</h6>
                            <div class="row text-center">
                                <div class="col-4">
                                    <h4 class="text-primary">${orders.length}</h4>
                                    <small class="text-muted">Tổng Đơn</small>
                                </div>
                                <div class="col-4">
                                    <h4 class="text-warning">${inProgressOrders}</h4>
                                    <small class="text-muted">Đang Giao</small>
                                </div>
                                <div class="col-4">
                                    <h4 class="text-success">${completedOrders}</h4>
                                    <small class="text-muted">Hoàn Thành</small>
                                </div>
                            </div>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Đóng</button>
                        </div>
                    </div>
                </div>
            </div>
        `;
        
        // Remove old modal if exists
        const oldModal = document.getElementById('detailsModal');
        if (oldModal) oldModal.remove();
        
        // Add new modal to body
        document.body.insertAdjacentHTML('beforeend', alertHtml);
        
        // Show modal
        const modal = new bootstrap.Modal(document.getElementById('detailsModal'));
        modal.show();
        
        // Remove modal after hiding
        document.getElementById('detailsModal').addEventListener('hidden.bs.modal', function() {
            this.remove();
        });
        
    } catch (error) {
        console.error('Error loading staff details:', error);
        showError('Không thể tải thông tin chi tiết');
    }
}

// Delete staff
async function deleteStaff(staffId) {
    const staff = allStaff.find(s => s.staffId === staffId);
    if (!staff) return;
    
    if (!confirm(`Bạn có chắc muốn xóa nhân viên "${staff.fullName}"?\n\nLưu ý: Không thể xóa nhân viên đang có đơn hàng.`)) {
        return;
    }
    
    try {
        await apiService.deleteDeliveryStaff(staffId);
        
        // Remove from local data
        allStaff = allStaff.filter(s => s.staffId !== staffId);
        filteredStaff = filteredStaff.filter(s => s.staffId !== staffId);
        
        updateStats();
        renderStaffCards();
        
        showSuccess('Xóa nhân viên thành công!');
    } catch (error) {
        console.error('Error deleting staff:', error);
        showError('Không thể xóa nhân viên. Có thể nhân viên đang có đơn hàng.');
    }
}

// Show loading state
function showLoading() {
    const container = document.getElementById('staffContainer');
    container.innerHTML = `
        <div class="col-12 text-center py-5">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Đang tải...</span>
            </div>
        </div>
    `;
}

// Show success message
function showSuccess(message) {
    const toast = `
        <div class="position-fixed top-0 end-0 p-3" style="z-index: 11000">
            <div class="toast show align-items-center text-white bg-success border-0" role="alert">
                <div class="d-flex">
                    <div class="toast-body">
                        <i class="fas fa-check-circle me-2"></i> ${message}
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
                </div>
            </div>
        </div>
    `;
    
    document.body.insertAdjacentHTML('beforeend', toast);
    
    setTimeout(() => {
        const toastElement = document.querySelector('.toast');
        if (toastElement) toastElement.remove();
    }, 3000);
}

// Show error message
function showError(message) {
    const toast = `
        <div class="position-fixed top-0 end-0 p-3" style="z-index: 11000">
            <div class="toast show align-items-center text-white bg-danger border-0" role="alert">
                <div class="d-flex">
                    <div class="toast-body">
                        <i class="fas fa-exclamation-circle me-2"></i> ${message}
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
                </div>
            </div>
        </div>
    `;
    
    document.body.insertAdjacentHTML('beforeend', toast);
    
    setTimeout(() => {
        const toastElement = document.querySelector('.toast');
        if (toastElement) toastElement.remove();
    }, 3000);
}


// Get status badge HTML
function getStatusBadge(status) {
    const statusMap = {
        0: '<span class=\"badge bg-secondary\">Chua Nh?n</span>',
        1: '<span class=\"badge bg-info\">�� Nh?n - Chua Giao</span>',
        2: '<span class=\"badge bg-warning\">�ang Giao</span>',
        3: '<span class=\"badge bg-success\">�� Giao</span>'
    };
    return statusMap[status] || '<span class=\"badge bg-secondary\">Kh�ng x�c d?nh</span>';
}

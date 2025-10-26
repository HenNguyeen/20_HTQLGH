// Orders Page JavaScript
let allOrders = [];
let filteredOrders = [];

document.addEventListener('DOMContentLoaded', function() {
    // Check authentication
    if (!auth.requireAuth()) {
        return;
    }
    
    // Display user info
    displayUserInfo();
    
    // Initialize page
    initOrdersPage();
    setupEventListeners();
});

// Display user information
function displayUserInfo() {
    const user = auth.getCurrentUser();
    if (!user) {
        auth.logout();
        return;
    }
    
    // Update UI based on user role
    if (auth.isCustomer()) {
        // Hide admin-only features for customers
        const adminElements = document.querySelectorAll('.admin-only');
        adminElements.forEach(el => el.style.display = 'none');
    } else if (auth.isShipper()) {
        // Hide customer-only features for shippers
        const customerElements = document.querySelectorAll('.customer-only');
        customerElements.forEach(el => el.style.display = 'none');
    }
}

// Initialize Orders Page
async function initOrdersPage() {
    await loadOrders();
    initSidebar();
}

// Setup Event Listeners
function setupEventListeners() {
    // Search input
    document.getElementById('searchInput').addEventListener('input', filterOrders);
    
    // Status filter
    document.getElementById('statusFilter').addEventListener('change', filterOrders);
    
    // Delivery type filter
    document.getElementById('deliveryTypeFilter').addEventListener('change', filterOrders);
    
    // Sort order
    document.getElementById('sortOrder').addEventListener('change', filterOrders);
    
    // Collect money checkbox
    document.getElementById('collectMoney').addEventListener('change', function() {
        document.getElementById('collectionAmount').disabled = !this.checked;
    });
    
    // Form inputs for fee calculation
    const feeInputs = ['weight', 'distance', 'packageType', 'deliveryType', 'isFragile', 'isValuable', 'isVehicle'];
    feeInputs.forEach(name => {
        const element = document.querySelector(`[name="${name}"]`);
        if (element) {
            element.addEventListener('change', calculateEstimatedFee);
            element.addEventListener('input', calculateEstimatedFee);
        }
    });
}

// Load Orders
async function loadOrders() {
    const tableBody = document.getElementById('ordersTableBody');
    
    try {
        allOrders = await apiService.getAllOrders();
        filteredOrders = [...allOrders];
        renderOrders();
    } catch (error) {
        console.error('Error loading orders:', error);
        utils.showError(tableBody, 'Không thể tải danh sách đơn hàng');
    }
}

// Filter Orders
function filterOrders() {
    const searchText = document.getElementById('searchInput').value.toLowerCase();
    const statusFilter = document.getElementById('statusFilter').value;
    const deliveryTypeFilter = document.getElementById('deliveryTypeFilter').value;
    const sortOrder = document.getElementById('sortOrder').value;
    
    // Filter
    filteredOrders = allOrders.filter(order => {
        // Search filter
        const matchesSearch = !searchText || 
            order.orderCode.toLowerCase().includes(searchText) ||
            order.customer.fullName.toLowerCase().includes(searchText) ||
            order.customer.phoneNumber.includes(searchText);
        
        // Status filter
        const matchesStatus = !statusFilter || order.status.toString() === statusFilter;
        
        // Delivery type filter
        const matchesDeliveryType = !deliveryTypeFilter || order.deliveryType.toString() === deliveryTypeFilter;
        
        return matchesSearch && matchesStatus && matchesDeliveryType;
    });
    
    // Sort
    switch(sortOrder) {
        case 'newest':
            filteredOrders.sort((a, b) => new Date(b.createdDate) - new Date(a.createdDate));
            break;
        case 'oldest':
            filteredOrders.sort((a, b) => new Date(a.createdDate) - new Date(b.createdDate));
            break;
        case 'fee-high':
            filteredOrders.sort((a, b) => b.shippingFee - a.shippingFee);
            break;
        case 'fee-low':
            filteredOrders.sort((a, b) => a.shippingFee - b.shippingFee);
            break;
    }
    
    renderOrders();
}

// Render Orders
function renderOrders() {
    const tableBody = document.getElementById('ordersTableBody');
    
    if (filteredOrders.length === 0) {
        utils.showEmpty(tableBody, 'Không tìm thấy đơn hàng nào');
        return;
    }
    
    tableBody.innerHTML = filteredOrders.map(order => `
        <tr class="fade-in">
            <td><strong>${order.orderCode}</strong></td>
            <td>
                ${order.customer.fullName}<br>
                <small class="text-muted">
                    <i class="fas fa-phone"></i> ${order.customer.phoneNumber}
                </small>
            </td>
            <td>
                <small>${order.customer.address}, ${order.customer.ward}</small>
            </td>
            <td>${utils.getPackageTypeText(order.packageType)}</td>
            <td><strong>${utils.formatCurrency(order.shippingFee)}</strong></td>
            <td>
                <span class="badge ${order.deliveryType === 1 ? 'bg-warning' : 'bg-secondary'}">
                    ${utils.getDeliveryTypeText(order.deliveryType)}
                </span>
            </td>
            <td>
                <span class="badge badge-status ${utils.getStatusClass(order.status)}">
                    ${utils.getStatusText(order.status)}
                </span>
            </td>
            <td>
                ${order.assignedStaff 
                    ? `<small><i class="fas fa-user-check text-success"></i> ${order.assignedStaff.fullName}</small>` 
                    : `<small><i class="fas fa-user-times text-muted"></i> Chưa gán</small>`}
            </td>
            <td><small>${utils.formatDate(order.createdDate)}</small></td>
            <td>
                <div class="btn-group" role="group">
                    <button class="btn btn-sm btn-info" onclick="viewOrderDetail('${order.orderId}')" title="Xem chi tiết">
                        <i class="fas fa-eye"></i>
                    </button>
                    <button class="btn btn-sm btn-warning" onclick="openUpdateStatus('${order.orderId}')" title="Cập nhật trạng thái">
                        <i class="fas fa-edit"></i>
                    </button>
                    <button class="btn btn-sm btn-success" onclick="openAssignStaff('${order.orderId}')" title="Gán nhân viên">
                        <i class="fas fa-user-plus"></i>
                    </button>
                    <button class="btn btn-sm btn-primary" onclick="trackOrder('${order.orderCode}')" title="Tracking">
                        <i class="fas fa-map-marker-alt"></i>
                    </button>
                    <button class="btn btn-sm btn-danger" onclick="deleteOrder('${order.orderId}')" title="Xóa">
                        <i class="fas fa-trash"></i>
                    </button>
                </div>
            </td>
        </tr>
    `).join('');
}

// Create Order
async function createOrder() {
    const form = document.getElementById('createOrderForm');
    
    if (!form.checkValidity()) {
        form.reportValidity();
        return;
    }
    
    const formData = new FormData(form);
    const orderData = {
        orderCode: formData.get('orderCode'),
        customerName: formData.get('customerName'),
        customerPhone: formData.get('customerPhone'),
        deliveryAddress: formData.get('deliveryAddress'),
        ward: formData.get('ward'),
        district: formData.get('district'),
        city: formData.get('city'),
        productCode: formData.get('productCode'),
        packageType: parseInt(formData.get('packageType')),
        weight: parseFloat(formData.get('weight')),
        size: formData.get('size'),
        distance: parseFloat(formData.get('distance')),
        isFragile: formData.get('isFragile') === 'on',
        isValuable: formData.get('isValuable') === 'on',
        isVehicle: formData.get('isVehicle') === 'on',
        collectMoney: formData.get('collectMoney') === 'on',
        collectionAmount: parseFloat(formData.get('collectionAmount') || 0),
        paymentMethod: parseInt(formData.get('paymentMethod')),
        deliveryType: parseInt(formData.get('deliveryType')),
        notes: formData.get('notes') || ''
    };
    
    try {
        const newOrder = await apiService.createOrder(orderData);
        utils.showToast('Tạo đơn hàng thành công!', 'success');
        
        // Close modal and reload
        const modal = bootstrap.Modal.getInstance(document.getElementById('createOrderModal'));
        modal.hide();
        form.reset();
        
        await loadOrders();
    } catch (error) {
        console.error('Error creating order:', error);
        utils.showToast('Lỗi khi tạo đơn hàng: ' + error.message, 'danger');
    }
}

// Calculate Estimated Fee
function calculateEstimatedFee() {
    const form = document.getElementById('createOrderForm');
    const formData = new FormData(form);
    
    const weight = parseFloat(formData.get('weight')) || 0;
    const distance = parseFloat(formData.get('distance')) || 0;
    const packageType = parseInt(formData.get('packageType')) || 0;
    const deliveryType = parseInt(formData.get('deliveryType')) || 0;
    const isFragile = formData.get('isFragile') === 'on';
    const isValuable = formData.get('isValuable') === 'on';
    const isVehicle = formData.get('isVehicle') === 'on';
    
    if (weight > 0 && distance > 0) {
        // Simulate fee calculation (same logic as backend)
        let baseFee = 20000;
        let totalFee = baseFee;
        
        // Distance fee
        if (distance <= 5) totalFee += 10000;
        else if (distance <= 10) totalFee += 20000;
        else if (distance <= 20) totalFee += 40000;
        else totalFee += 60000 + (distance - 20) * 3000;
        
        // Weight fee
        if (weight > 5) totalFee += (weight - 5) * 2000;
        
        // Special attributes
        if (isFragile) totalFee += 15000;
        if (isValuable) totalFee += 30000;
        if (isVehicle) totalFee += 100000;
        
        // Delivery type
        if (deliveryType === 1) totalFee *= 1.5;
        
        // Package type
        if ([7, 9].includes(packageType)) totalFee += 30000;
        else if ([8, 10].includes(packageType)) totalFee += 20000;
        else if (packageType === 11) totalFee += 150000;
        
        document.getElementById('estimatedFee').innerHTML = utils.formatCurrency(totalFee);
    }
}

// View Order Detail
async function viewOrderDetail(orderId) {
    try {
        const order = await apiService.getOrderById(orderId);
        
        // Reuse the modal from dashboard.js
        const modal = document.createElement('div');
        modal.className = 'modal fade';
        modal.innerHTML = `
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">
                            <i class="fas fa-box"></i> Chi Tiết Đơn Hàng: ${order.orderCode}
                        </h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <div class="row">
                            <div class="col-md-6">
                                <h6><i class="fas fa-user"></i> Thông Tin Khách Hàng</h6>
                                <table class="table table-sm table-bordered">
                                    <tr><th>Họ tên:</th><td>${order.customer.fullName}</td></tr>
                                    <tr><th>SĐT:</th><td>${order.customer.phoneNumber}</td></tr>
                                    <tr><th>Địa chỉ:</th><td>${order.customer.address}</td></tr>
                                    <tr><th>Phường/Xã:</th><td>${order.customer.ward}</td></tr>
                                    <tr><th>Quận/Huyện:</th><td>${order.customer.district}</td></tr>
                                    <tr><th>Thành phố:</th><td>${order.customer.city}</td></tr>
                                </table>
                            </div>
                            <div class="col-md-6">
                                <h6><i class="fas fa-box"></i> Thông Tin Hàng Hóa</h6>
                                <table class="table table-sm table-bordered">
                                    <tr><th>Mã SP:</th><td>${order.productCode}</td></tr>
                                    <tr><th>Loại hàng:</th><td>${utils.getPackageTypeText(order.packageType)}</td></tr>
                                    <tr><th>Trọng lượng:</th><td>${order.weight} kg</td></tr>
                                    <tr><th>Kích thước:</th><td>${order.size}</td></tr>
                                    <tr><th>Khoảng cách:</th><td>${order.distance} km</td></tr>
                                    <tr><th>Đặc biệt:</th><td>
                                        ${order.isFragile ? '✅ Dễ vỡ ' : ''}
                                        ${order.isValuable ? '✅ Trị giá ' : ''}
                                        ${order.isVehicle ? '✅ Xe' : ''}
                                    </td></tr>
                                </table>
                            </div>
                        </div>
                        <div class="row mt-3">
                            <div class="col-md-6">
                                <h6><i class="fas fa-money-bill"></i> Thanh Toán</h6>
                                <table class="table table-sm table-bordered">
                                    <tr><th>Phương thức:</th><td>${utils.getPaymentMethodText(order.paymentMethod)}</td></tr>
                                    <tr><th>Phí giao hàng:</th><td><strong class="text-primary">${utils.formatCurrency(order.shippingFee)}</strong></td></tr>
                                    <tr><th>Thu tiền:</th><td>${order.collectMoney ? utils.formatCurrency(order.collectionAmount) : 'Không'}</td></tr>
                                    <tr><th>Đã thanh toán:</th><td>${order.isPaid ? '✅ Rồi' : '❌ Chưa'}</td></tr>
                                </table>
                            </div>
                            <div class="col-md-6">
                                <h6><i class="fas fa-truck"></i> Giao Hàng</h6>
                                <table class="table table-sm table-bordered">
                                    <tr><th>Loại giao:</th><td><span class="badge ${order.deliveryType === 1 ? 'bg-warning' : 'bg-secondary'}">${utils.getDeliveryTypeText(order.deliveryType)}</span></td></tr>
                                    <tr><th>Trạng thái:</th><td><span class="badge badge-status ${utils.getStatusClass(order.status)}">${utils.getStatusText(order.status)}</span></td></tr>
                                    <tr><th>Nhân viên:</th><td>${order.assignedStaff ? order.assignedStaff.fullName + '<br><small>' + order.assignedStaff.phoneNumber + '</small>' : '<em class="text-muted">Chưa gán</em>'}</td></tr>
                                    <tr><th>Ngày tạo:</th><td>${utils.formatDate(order.createdDate)}</td></tr>
                                    <tr><th>Ngày nhận:</th><td>${utils.formatDate(order.receivedDate)}</td></tr>
                                    <tr><th>Ngày giao:</th><td>${utils.formatDate(order.deliveredDate)}</td></tr>
                                </table>
                            </div>
                        </div>
                        ${order.notes ? `
                            <div class="mt-3">
                                <h6><i class="fas fa-sticky-note"></i> Ghi Chú</h6>
                                <div class="alert alert-info">${order.notes.replace(/\n/g, '<br>')}</div>
                            </div>
                        ` : ''}
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Đóng</button>
                        <button type="button" class="btn btn-warning" onclick="openUpdateStatus('${order.orderId}'); bootstrap.Modal.getInstance(this.closest('.modal')).hide()">
                            <i class="fas fa-edit"></i> Cập Nhật Trạng Thái
                        </button>
                        <button type="button" class="btn btn-primary" onclick="trackOrder('${order.orderCode}')">
                            <i class="fas fa-map-marker-alt"></i> Tracking
                        </button>
                    </div>
                </div>
            </div>
        `;
        
        document.body.appendChild(modal);
        const bsModal = new bootstrap.Modal(modal);
        bsModal.show();
        
        modal.addEventListener('hidden.bs.modal', () => modal.remove());
    } catch (error) {
        console.error('Error viewing order:', error);
        utils.showToast('Không thể tải thông tin đơn hàng', 'danger');
    }
}

// Open Update Status Modal
function openUpdateStatus(orderId) {
    const order = allOrders.find(o => o.orderId === orderId);
    if (!order) return;
    
    document.getElementById('updateOrderId').value = orderId;
    document.getElementById('newStatus').value = order.status;
    document.getElementById('statusNotes').value = '';
    
    const modal = new bootstrap.Modal(document.getElementById('updateStatusModal'));
    modal.show();
}

// Update Status
async function updateStatus() {
    const orderId = document.getElementById('updateOrderId').value;
    const newStatus = parseInt(document.getElementById('newStatus').value);
    const notes = document.getElementById('statusNotes').value;
    
    try {
        await apiService.updateOrderStatus(orderId, {
            orderId: orderId,
            status: newStatus,
            notes: notes
        });
        
        utils.showToast('Cập nhật trạng thái thành công!', 'success');
        
        const modal = bootstrap.Modal.getInstance(document.getElementById('updateStatusModal'));
        modal.hide();
        
        await loadOrders();
    } catch (error) {
        console.error('Error updating status:', error);
        utils.showToast('Lỗi khi cập nhật trạng thái', 'danger');
    }
}

// Open Assign Staff Modal
async function openAssignStaff(orderId) {
    document.getElementById('assignOrderId').value = orderId;
    
    const modal = new bootstrap.Modal(document.getElementById('assignStaffModal'));
    modal.show();
    
    // Load available staff
    try {
        const staff = await apiService.getAvailableStaff();
        const select = document.getElementById('selectedStaff');
        
        if (staff.length === 0) {
            select.innerHTML = '<option value="">Không có nhân viên rảnh</option>';
            return;
        }
        
        select.innerHTML = '<option value="">-- Chọn nhân viên --</option>' +
            staff.map(s => `<option value="${s.staffId}">${s.fullName} - ${s.vehicleType} (${s.vehiclePlate})</option>`).join('');
        
        // Show staff info on selection
        select.addEventListener('change', function() {
            const selectedStaff = staff.find(s => s.staffId === this.value);
            if (selectedStaff) {
                document.getElementById('staffInfo').classList.remove('d-none');
                document.getElementById('staffDetails').innerHTML = `
                    <p class="mb-1"><strong>Tên:</strong> ${selectedStaff.fullName}</p>
                    <p class="mb-1"><strong>SĐT:</strong> ${selectedStaff.phoneNumber}</p>
                    <p class="mb-1"><strong>Xe:</strong> ${selectedStaff.vehicleType}</p>
                    <p class="mb-0"><strong>Biển số:</strong> ${selectedStaff.vehiclePlate}</p>
                `;
            } else {
                document.getElementById('staffInfo').classList.add('d-none');
            }
        });
    } catch (error) {
        console.error('Error loading staff:', error);
        utils.showToast('Không thể tải danh sách nhân viên', 'danger');
    }
}

// Assign Staff
async function assignStaff() {
    const orderId = parseInt(document.getElementById('assignOrderId').value);
    const staffId = parseInt(document.getElementById('selectedStaff').value);
    
    if (!staffId) {
        utils.showToast('Vui lòng chọn nhân viên', 'warning');
        return;
    }
    
    try {
        await apiService.assignStaff(orderId, staffId);
        utils.showToast('Gán nhân viên thành công!', 'success');
        
        const modal = bootstrap.Modal.getInstance(document.getElementById('assignStaffModal'));
        modal.hide();
        
        await loadOrders();
    } catch (error) {
        console.error('Error assigning staff:', error);
        utils.showToast('Lỗi khi gán nhân viên', 'danger');
    }
}

// Delete Order
async function deleteOrder(orderId) {
    if (!await utils.confirm('Bạn có chắc chắn muốn xóa đơn hàng này?')) {
        return;
    }
    
    try {
        await apiService.deleteOrder(orderId);
        utils.showToast('Xóa đơn hàng thành công!', 'success');
        await loadOrders();
    } catch (error) {
        console.error('Error deleting order:', error);
        utils.showToast('Lỗi khi xóa đơn hàng', 'danger');
    }
}

// Track Order
function trackOrder(orderCode) {
    window.location.href = `tracking.html?code=${orderCode}`;
}

// Reset Filters
function resetFilters() {
    document.getElementById('searchInput').value = '';
    document.getElementById('statusFilter').value = '';
    document.getElementById('deliveryTypeFilter').value = '';
    document.getElementById('sortOrder').value = 'newest';
    filterOrders();
}

// Initialize Sidebar
function initSidebar() {
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebar = document.getElementById('sidebar');
    
    if (sidebarToggle && sidebar) {
        sidebarToggle.addEventListener('click', function() {
            sidebar.classList.toggle('active');
        });
    }
}

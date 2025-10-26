// Dashboard JavaScript
document.addEventListener('DOMContentLoaded', function() {
    // Check authentication
    if (!auth.requireAuth()) {
        return;
    }
    
    // Display user info
    displayUserInfo();
    
    // Initialize dashboard
    initDashboard();
    initSidebar();
});

// Display user information in navbar
function displayUserInfo() {
    const user = auth.getCurrentUser();
    if (!user) {
        auth.logout();
        return;
    }
    
    // Update user name in navbar if element exists
    const userNameElement = document.querySelector('.user-name');
    if (userNameElement) {
        userNameElement.textContent = user.fullName;
    }
    
    // Update user role badge if element exists
    const userRoleElement = document.querySelector('.user-role');
    if (userRoleElement) {
        let roleText = '';
        let roleClass = '';
        
        if (auth.isAdmin()) {
            roleText = 'Quản trị viên';
            roleClass = 'badge bg-danger';
        } else if (auth.isShipper()) {
            roleText = 'Nhân viên giao hàng';
            roleClass = 'badge bg-info';
        } else if (auth.isCustomer()) {
            roleText = 'Khách hàng';
            roleClass = 'badge bg-success';
        }
        
        userRoleElement.className = roleClass;
        userRoleElement.textContent = roleText;
    }
}

// Initialize Dashboard
async function initDashboard() {
    try {
        await Promise.all([
            loadStats(),
            loadCharts(),
            loadRecentOrders()
        ]);
    } catch (error) {
        console.error('Error initializing dashboard:', error);
        utils.showToast('Lỗi khi tải dữ liệu dashboard', 'danger');
    }
}

// Load Statistics
async function loadStats() {
    try {
        const [orders, availableStaff] = await Promise.all([
            apiService.getAllOrders(),
            apiService.getAvailableStaff()
        ]);

        // Count orders by status
        const stats = {
            total: orders.length,
            delivering: orders.filter(o => o.status === 2).length,
            completed: orders.filter(o => o.status === 3).length,
            availableStaff: availableStaff.length
        };

        // Update UI
        document.getElementById('totalOrders').textContent = stats.total;
        document.getElementById('deliveringOrders').textContent = stats.delivering;
        document.getElementById('completedOrders').textContent = stats.completed;
        document.getElementById('availableStaff').textContent = stats.availableStaff;

        // Animate numbers
        animateNumbers();
    } catch (error) {
        console.error('Error loading stats:', error);
    }
}

// Animate numbers
function animateNumbers() {
    const counters = document.querySelectorAll('.stats-content h3');
    counters.forEach(counter => {
        const target = parseInt(counter.textContent);
        let current = 0;
        const increment = target / 30;
        
        const timer = setInterval(() => {
            current += increment;
            if (current >= target) {
                counter.textContent = target;
                clearInterval(timer);
            } else {
                counter.textContent = Math.floor(current);
            }
        }, 30);
    });
}

// Load Charts
async function loadCharts() {
    try {
        const orders = await apiService.getAllOrders();
        
        // Order Status Chart (Bar Chart)
        createOrderStatusChart(orders);
        
        // Delivery Type Chart (Pie Chart)
        createDeliveryTypeChart(orders);
    } catch (error) {
        console.error('Error loading charts:', error);
    }
}

// Create Order Status Chart
function createOrderStatusChart(orders) {
    const statusCounts = {
        'Chưa Nhận': orders.filter(o => o.status === 0).length,
        'Đã Nhận - Chưa Giao': orders.filter(o => o.status === 1).length,
        'Đang Giao': orders.filter(o => o.status === 2).length,
        'Đã Giao': orders.filter(o => o.status === 3).length
    };

    const ctx = document.getElementById('orderStatusChart');
    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: Object.keys(statusCounts),
            datasets: [{
                label: 'Số lượng đơn hàng',
                data: Object.values(statusCounts),
                backgroundColor: [
                    'rgba(128, 128, 128, 0.8)',
                    'rgba(246, 194, 62, 0.8)',
                    'rgba(54, 185, 204, 0.8)',
                    'rgba(28, 200, 138, 0.8)'
                ],
                borderColor: [
                    'rgb(128, 128, 128)',
                    'rgb(246, 194, 62)',
                    'rgb(54, 185, 204)',
                    'rgb(28, 200, 138)'
                ],
                borderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            plugins: {
                legend: {
                    display: false
                },
                title: {
                    display: false
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        stepSize: 1
                    }
                }
            }
        }
    });
}

// Create Delivery Type Chart
function createDeliveryTypeChart(orders) {
    const deliveryTypeCounts = {
        'Giao Thường': orders.filter(o => o.deliveryType === 0).length,
        'Giao Nhanh': orders.filter(o => o.deliveryType === 1).length
    };

    const ctx = document.getElementById('deliveryTypeChart');
    new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: Object.keys(deliveryTypeCounts),
            datasets: [{
                data: Object.values(deliveryTypeCounts),
                backgroundColor: [
                    'rgba(78, 115, 223, 0.8)',
                    'rgba(246, 194, 62, 0.8)'
                ],
                borderColor: [
                    'rgb(78, 115, 223)',
                    'rgb(246, 194, 62)'
                ],
                borderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            plugins: {
                legend: {
                    position: 'bottom'
                }
            }
        }
    });
}

// Load Recent Orders
async function loadRecentOrders() {
    const tableBody = document.getElementById('recentOrdersTable');
    
    try {
        const orders = await apiService.getAllOrders();
        
        if (orders.length === 0) {
            utils.showEmpty(tableBody, 'Chưa có đơn hàng nào');
            return;
        }

        // Sort by created date (newest first) and take first 5
        const recentOrders = orders
            .sort((a, b) => new Date(b.createdDate) - new Date(a.createdDate))
            .slice(0, 5);

        tableBody.innerHTML = recentOrders.map(order => `
            <tr class="fade-in">
                <td><strong>${order.orderCode}</strong></td>
                <td>
                    ${order.customer.fullName}<br>
                    <small class="text-muted">${order.customer.phoneNumber}</small>
                </td>
                <td>${utils.getPackageTypeText(order.packageType)}</td>
                <td><strong>${utils.formatCurrency(order.shippingFee)}</strong></td>
                <td>
                    <span class="badge badge-status ${utils.getStatusClass(order.status)}">
                        ${utils.getStatusText(order.status)}
                    </span>
                </td>
                <td>
                    ${order.assignedStaff 
                        ? `<span class="text-success"><i class="fas fa-user-check"></i> ${order.assignedStaff.fullName}</span>` 
                        : '<span class="text-muted"><i class="fas fa-user-times"></i> Chưa gán</span>'}
                </td>
                <td>
                    <button class="btn btn-sm btn-primary" onclick="viewOrderDetails('${order.orderId}')">
                        <i class="fas fa-eye"></i>
                    </button>
                    <button class="btn btn-sm btn-info" onclick="trackOrder('${order.orderCode}')">
                        <i class="fas fa-map-marker-alt"></i>
                    </button>
                </td>
            </tr>
        `).join('');
    } catch (error) {
        console.error('Error loading recent orders:', error);
        utils.showError(tableBody, 'Không thể tải danh sách đơn hàng');
    }
}

// View Order Details
async function viewOrderDetails(orderId) {
    try {
        const order = await apiService.getOrderById(orderId);
        
        // Create modal
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
                                <table class="table table-sm">
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
                                <table class="table table-sm">
                                    <tr><th>Loại hàng:</th><td>${utils.getPackageTypeText(order.packageType)}</td></tr>
                                    <tr><th>Trọng lượng:</th><td>${order.weight} kg</td></tr>
                                    <tr><th>Kích thước:</th><td>${order.size}</td></tr>
                                    <tr><th>Khoảng cách:</th><td>${order.distance} km</td></tr>
                                    <tr><th>Hàng dễ vỡ:</th><td>${order.isFragile ? '✅' : '❌'}</td></tr>
                                    <tr><th>Hàng trị giá:</th><td>${order.isValuable ? '✅' : '❌'}</td></tr>
                                </table>
                            </div>
                        </div>
                        <div class="row mt-3">
                            <div class="col-md-6">
                                <h6><i class="fas fa-money-bill"></i> Thanh Toán</h6>
                                <table class="table table-sm">
                                    <tr><th>Phương thức:</th><td>${utils.getPaymentMethodText(order.paymentMethod)}</td></tr>
                                    <tr><th>Phí giao hàng:</th><td><strong>${utils.formatCurrency(order.shippingFee)}</strong></td></tr>
                                    <tr><th>Thu tiền:</th><td>${order.collectMoney ? utils.formatCurrency(order.collectionAmount) : 'Không'}</td></tr>
                                    <tr><th>Đã thanh toán:</th><td>${order.isPaid ? '✅' : '❌'}</td></tr>
                                </table>
                            </div>
                            <div class="col-md-6">
                                <h6><i class="fas fa-info-circle"></i> Thông Tin Giao Hàng</h6>
                                <table class="table table-sm">
                                    <tr><th>Loại giao:</th><td>${utils.getDeliveryTypeText(order.deliveryType)}</td></tr>
                                    <tr><th>Trạng thái:</th><td><span class="badge ${utils.getStatusClass(order.status)}">${utils.getStatusText(order.status)}</span></td></tr>
                                    <tr><th>Nhân viên:</th><td>${order.assignedStaff ? order.assignedStaff.fullName : 'Chưa gán'}</td></tr>
                                    <tr><th>Ngày tạo:</th><td>${utils.formatDate(order.createdDate)}</td></tr>
                                    <tr><th>Ngày nhận:</th><td>${utils.formatDate(order.receivedDate)}</td></tr>
                                    <tr><th>Ngày giao:</th><td>${utils.formatDate(order.deliveredDate)}</td></tr>
                                </table>
                            </div>
                        </div>
                        ${order.notes ? `
                            <div class="mt-3">
                                <h6><i class="fas fa-sticky-note"></i> Ghi Chú</h6>
                                <div class="alert alert-info">${order.notes}</div>
                            </div>
                        ` : ''}
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Đóng</button>
                        <a href="orders.html" class="btn btn-primary">Xem Tất Cả Đơn Hàng</a>
                    </div>
                </div>
            </div>
        `;
        
        document.body.appendChild(modal);
        const bsModal = new bootstrap.Modal(modal);
        bsModal.show();
        
        modal.addEventListener('hidden.bs.modal', () => modal.remove());
    } catch (error) {
        console.error('Error viewing order details:', error);
        utils.showToast('Không thể tải thông tin đơn hàng', 'danger');
    }
}

// Track Order
function trackOrder(orderCode) {
    window.location.href = `tracking.html?code=${orderCode}`;
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

// Refresh Dashboard
function refreshDashboard() {
    initDashboard();
    utils.showToast('Đã làm mới dashboard', 'success');
}

// Auto refresh every 30 seconds
setInterval(() => {
    loadStats();
    loadRecentOrders();
}, 30000);

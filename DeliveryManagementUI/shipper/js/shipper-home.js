// shipper-home.js

let myOrders = [];
let staffInfo = null; // { staffId, isAvailable }


window.addEventListener('DOMContentLoaded', async () => {
  await loadShipperInfo();
  await loadMyOrders();
  setupAvailabilityToggle();
});

// Load shipper info (map user -> staff by phone or full name)
async function loadShipperInfo() {
  try {
    // Ưu tiên gọi backend để lấy nhân viên tương ứng với user hiện tại
    let staff = null;
    try {
      staff = await apiService.getMyStaffRecord();
    } catch {}
    if (!staff) {
      const user = auth.getCurrentUser();
      const staffList = await apiService.getDeliveryStaff();
      if (user?.phoneNumber) {
        staff = staffList.find(s => s.phoneNumber === user.phoneNumber);
      }
      if (!staff && user) {
        staff = staffList.find(s => s.fullName === user.fullName);
      }
    }
    if (staff) {
      staffInfo = { staffId: staff.staffId, isAvailable: staff.isAvailable };
    } else {
      staffInfo = { staffId: null, isAvailable: true };
      console.warn('Không tìm thấy bản ghi nhân viên khớp với tài khoản shipper.');
    }

    // Update availability switch
    document.getElementById('availabilitySwitch').checked = !!staffInfo.isAvailable;
    document.getElementById('availabilityLabel').textContent = staffInfo.isAvailable ? 'Đang rảnh' : 'Đang bận';
  } catch (e) {
    console.error('Error loading shipper info:', e);
  }
}

// Load my orders (assigned to me)
async function loadMyOrders() {
  try {
    const user = auth.getCurrentUser();
    // Lấy tất cả đơn rồi lọc theo AssignedStaff khớp với người dùng hiện tại
    const orders = await apiService.getAllOrders();
    myOrders = (orders || []).filter(o => o.assignedStaff && (
      (user.phoneNumber && o.assignedStaff.phoneNumber === user.phoneNumber) ||
      (o.assignedStaff.fullName === user.fullName)
    ));

    updateStats();
    renderRecentOrders();
  } catch (e) {
    console.error('Error loading orders:', e);
    document.getElementById('recentOrdersBody').innerHTML = 
      '<tr><td colspan="5" class="text-center text-danger">Không tải được dữ liệu</td></tr>';
  }
}

// Update statistics
function updateStats() {
  const total = myOrders.length;
  const pending = myOrders.filter(o => o.status === 2).length; // Đang giao
  const completed = myOrders.filter(o => o.status === 3).length; // Đã giao
  const earnings = myOrders.filter(o => o.status === 3).reduce((sum, o) => sum + (o.shippingFee * 0.1), 0); // 10% commission
  
  document.getElementById('totalOrders').textContent = total;
  document.getElementById('pendingOrders').textContent = pending;
  document.getElementById('completedOrders').textContent = completed;
  document.getElementById('totalEarnings').textContent = utils.formatCurrency(earnings);
}

// Render recent orders (top 5)
function renderRecentOrders() {
  const tbody = document.getElementById('recentOrdersBody');
  if (!myOrders || myOrders.length === 0) {
    tbody.innerHTML = '<tr><td colspan="5" class="text-center text-muted">Chưa có đơn hàng nào</td></tr>';
    return;
  }
  
  const recent = myOrders.slice(0, 5);
  tbody.innerHTML = recent.map(o => `
    <tr>
      <td><span class="fw-semibold">${o.orderCode || '-'}</span></td>
      <td>${o.customer ? o.customer.fullName : '-'}</td>
      <td><small>${o.customer ? o.customer.address : '-'}</small></td>
      <td><span class="badge ${utils.getStatusClass(o.status)}">${utils.getStatusText(o.status)}</span></td>
      <td>
        <a href="./orders.html?id=${o.orderId}" class="btn btn-sm btn-primary">
          <i class="fas fa-eye"></i> Chi tiết
        </a>
      </td>
    </tr>
  `).join('');
}

// Setup availability toggle
function setupAvailabilityToggle() {
  const toggle = document.getElementById('availabilitySwitch');
  const label = document.getElementById('availabilityLabel');
  
  toggle.addEventListener('change', async function() {
    const isAvailable = this.checked;
    label.textContent = isAvailable ? 'Đang rảnh' : 'Đang bận';
    
    try {
      // Cần staffId thực tế, nếu chưa map được thì thông báo
      if (!staffInfo || !staffInfo.staffId) {
        utils.showToast('Không xác định được nhân viên tương ứng với tài khoản này.', 'warning');
        throw new Error('Missing staffId');
      }
      await apiService.updateStaffAvailability(staffInfo.staffId, isAvailable);
      utils.showToast(`Đã cập nhật trạng thái: ${isAvailable ? 'Rảnh' : 'Bận'}`, 'success');
    } catch (e) {
      console.error('Error updating availability:', e);
      utils.showToast('Không thể cập nhật trạng thái', 'danger');
      // Revert toggle
      this.checked = !isAvailable;
      label.textContent = !isAvailable ? 'Đang rảnh' : 'Đang bận';
    }
  });
  
  // Toggle from menu
  document.getElementById('toggleAvailability').addEventListener('click', function(e) {
    e.preventDefault();
    toggle.click();
  });
}

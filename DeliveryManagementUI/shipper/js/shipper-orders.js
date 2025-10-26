// shipper-orders.js
// Quản lý đơn hàng cho shipper

let myOrders = [];
let staffInfo = null; // { staffId }

window.addEventListener('DOMContentLoaded', async () => {
  await loadShipperInfo();
  await loadMyOrders();
  setupUpdateStatusModal();
});

// Load shipper info (map user -> staff by phone/name)
async function loadShipperInfo() {
  try {
    let staff = null;
    try { staff = await apiService.getMyStaffRecord(); } catch {}
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
      staffInfo = { staffId: staff.staffId };
    } else {
      staffInfo = { staffId: null };
      console.warn('Không tìm thấy bản ghi nhân viên khớp với tài khoản shipper.');
    }
  } catch (e) {
    console.error('Error loading shipper info:', e);
  }
}

// Load all orders assigned to this shipper
async function loadMyOrders() {
  try {
    let orders = [];
    if (staffInfo && staffInfo.staffId) {
      orders = await apiService.getStaffOrders(staffInfo.staffId);
    } else {
      const user = auth.getCurrentUser();
      const all = await apiService.getAllOrders();
      orders = (all || []).filter(o => o.assignedStaff && (
        (user.phoneNumber && o.assignedStaff.phoneNumber === user.phoneNumber) ||
        (o.assignedStaff.fullName === user.fullName)
      ));
    }
    myOrders = orders;
    renderOrdersTable();
  } catch (e) {
    console.error('Error loading orders:', e);
    document.getElementById('ordersTableBody').innerHTML = '<tr><td colspan="5" class="text-center text-danger">Không tải được dữ liệu</td></tr>';
  }
}

// Render orders table
function renderOrdersTable() {
  const tbody = document.getElementById('ordersTableBody');
  if (!myOrders || myOrders.length === 0) {
    tbody.innerHTML = '<tr><td colspan="5" class="text-center text-muted">Chưa có đơn hàng nào</td></tr>';
    return;
  }
  tbody.innerHTML = myOrders.map(o => `
    <tr>
      <td><span class="fw-semibold">${o.orderCode || '-'}</span></td>
      <td>${o.customer ? o.customer.fullName : '-'}</td>
      <td><small>${o.customer ? o.customer.address : '-'}</small></td>
      <td><span class="badge ${utils.getStatusClass(o.status)}">${utils.getStatusText(o.status)}</span></td>
      <td>
        <button class="btn btn-sm btn-success me-2" onclick="openUpdateStatusModal(${o.orderId}, ${o.status})">
          <i class="fas fa-edit"></i> Cập nhật
        </button>
      </td>
    </tr>
  `).join('');
}

// Open modal to update status
window.openUpdateStatusModal = function(orderId, currentStatus) {
  document.getElementById('modalOrderId').value = orderId;
  document.getElementById('orderStatusSelect').value = currentStatus;
  document.getElementById('gpsLocation').value = '';
  // Try to get GPS
  if (navigator.geolocation) {
    navigator.geolocation.getCurrentPosition(
      pos => {
        document.getElementById('gpsLocation').value = `${pos.coords.latitude},${pos.coords.longitude}`;
      },
      err => {
        document.getElementById('gpsLocation').value = 'Không lấy được vị trí';
      }
    );
  } else {
    document.getElementById('gpsLocation').value = 'Không hỗ trợ GPS';
  }
  const modal = new bootstrap.Modal(document.getElementById('updateStatusModal'));
  modal.show();
};

// Setup modal form submit
function setupUpdateStatusModal() {
  const form = document.getElementById('updateStatusForm');
  form.addEventListener('submit', async function(e) {
    e.preventDefault();
    const orderId = document.getElementById('modalOrderId').value;
    const newStatus = parseInt(document.getElementById('orderStatusSelect').value);
    const gps = document.getElementById('gpsLocation').value;
    try {
      await apiService.updateOrderStatus(orderId, newStatus, gps);
      utils.showToast('Cập nhật trạng thái thành công!', 'success');
      // Reload orders
      await loadMyOrders();
      bootstrap.Modal.getInstance(document.getElementById('updateStatusModal')).hide();
    } catch (err) {
      utils.showToast('Cập nhật thất bại', 'danger');
    }
  });
}

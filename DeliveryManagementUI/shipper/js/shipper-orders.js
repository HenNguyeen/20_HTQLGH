// shipper-orders.js
// Quản lý đơn hàng cho shipper

let myOrders = [];
let staffInfo = null; // { staffId }
let routeMap = null;
let shipperMarker = null;
let customerMarker = null;
let routePolyline = null;
let currentPosition = null;
let trackingConnection = null;
let currentOrderData = null;

window.addEventListener('DOMContentLoaded', async () => {
  await loadShipperInfo();
  await loadMyOrders();
  setupUpdateStatusModal();
  await initializeSignalR();
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
        <button class="btn btn-sm btn-primary" onclick="openOrderDetailModal(${o.orderId})">
          <i class="fas fa-eye"></i> Xem chi tiết
        </button>
      </td>
    </tr>
  `).join('');
}

// Open modal to view order details
window.openOrderDetailModal = async function(orderId) {
  // Tìm thông tin đơn hàng
  currentOrderData = myOrders.find(o => o.orderId == orderId);
  
  if (!currentOrderData) {
    utils.showToast('Không tìm thấy đơn hàng', 'danger');
    return;
  }
  
  // Set form values
  document.getElementById('modalOrderId').value = orderId;
  document.getElementById('orderStatusSelect').value = currentOrderData.status;
  document.getElementById('gpsLocation').value = 'Đang lấy GPS...';
  document.getElementById('deliveryNotes').value = '';
  
  // Hiển thị thông tin chi tiết đơn hàng
  if (currentOrderData) {
    const orderInfoEl = document.getElementById('orderInfoInModal');
    const customer = currentOrderData.customer;
    const fullAddress = customer ? 
      `${customer.address || ''}, ${customer.ward || ''}, ${customer.district || ''}, ${customer.city || ''}`.replace(/,\s*,/g, ',').replace(/^,|,$/g, '') 
      : '-';
    
    orderInfoEl.innerHTML = `
      <div class="row mb-2">
        <div class="col-6"><strong>Mã đơn:</strong></div>
        <div class="col-6">${currentOrderData.orderCode || '-'}</div>
      </div>
      <div class="row mb-2">
        <div class="col-6"><strong>Loại giao hàng:</strong></div>
        <div class="col-6">${currentOrderData.deliveryType === 1 ? '⚡ Nhanh' : '📦 Tiêu chuẩn'}</div>
      </div>
      <div class="row mb-2">
        <div class="col-6"><strong>Khách hàng:</strong></div>
        <div class="col-6">${customer?.fullName || '-'}</div>
      </div>
      <div class="row mb-2">
        <div class="col-6"><strong>SĐT:</strong></div>
        <div class="col-6"><a href="tel:${customer?.phoneNumber || ''}">${customer?.phoneNumber || '-'}</a></div>
      </div>
      <div class="row mb-2">
        <div class="col-6"><strong>Địa chỉ:</strong></div>
        <div class="col-6">${fullAddress}</div>
      </div>
      <div class="row mb-2">
        <div class="col-6"><strong>Loại kiện hàng:</strong></div>
        <div class="col-6">${currentOrderData.packageType || '-'}</div>
      </div>
      <div class="row mb-2">
        <div class="col-6"><strong>Cân nặng:</strong></div>
        <div class="col-6">${currentOrderData.weight || '-'} kg</div>
      </div>
      <div class="row mb-2">
        <div class="col-6"><strong>Phí vận chuyển:</strong></div>
        <div class="col-6 text-success fw-bold">${utils.formatCurrency(currentOrderData.shippingFee || 0)}</div>
      </div>
      ${currentOrderData.collectMoney ? `
      <div class="row mb-2">
        <div class="col-6"><strong>Thu hộ:</strong></div>
        <div class="col-6 text-danger fw-bold">${utils.formatCurrency(currentOrderData.collectionAmount || 0)}</div>
      </div>
      ` : ''}
      ${currentOrderData.notes ? `
      <div class="row mb-2">
        <div class="col-6"><strong>Ghi chú:</strong></div>
        <div class="col-6"><em>${currentOrderData.notes}</em></div>
      </div>
      ` : ''}
      <div class="row">
        <div class="col-6"><strong>Trạng thái:</strong></div>
        <div class="col-6"><span class="badge ${utils.getStatusClass(currentOrderData.status)}">${utils.getStatusText(currentOrderData.status)}</span></div>
      </div>
    `;
  }
  
  // Khởi tạo bản đồ
  setTimeout(() => {
    initializeRouteMap();
  }, 300);
  
  // Lấy GPS và hiển thị lộ trình
  await getGPSAndShowRoute();
  
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
    const notes = document.getElementById('deliveryNotes').value;
    const shareLocation = document.getElementById('shareLocationCheckbox').checked;
    
    if (!currentPosition) {
      utils.showToast('Chưa có vị trí GPS!', 'warning');
      return;
    }
    
    try {
      // 1. Cập nhật trạng thái đơn hàng
      await apiService.updateOrderStatus(orderId, newStatus, gps, notes);
      
      // 2. Chia sẻ vị trí realtime qua SignalR (nếu được chọn)
      if (shareLocation && trackingConnection && trackingConnection.state === signalR.HubConnectionState.Connected) {
        await trackingConnection.invoke(
          "UpdateShipperLocation",
          staffInfo.staffId,
          parseInt(orderId),
          currentPosition.lat,
          currentPosition.lng
        );
        console.log('✅ Đã chia sẻ vị trí realtime');
      }
      
      utils.showToast('✅ Cập nhật trạng thái và chia sẻ vị trí thành công!', 'success');
      
      // Reload orders
      await loadMyOrders();
      bootstrap.Modal.getInstance(document.getElementById('updateStatusModal')).hide();
    } catch (err) {
      console.error(err);
      utils.showToast('❌ Cập nhật thất bại: ' + (err.message || err), 'danger');
    }
  });
}

// ============ GPS & MAP FUNCTIONS ============

// Khởi tạo SignalR
async function initializeSignalR() {
  try {
    const apiUrl = window.location.origin;
    trackingConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${apiUrl}/trackingHub`)
      .withAutomaticReconnect()
      .build();

    await trackingConnection.start();
    console.log("✅ SignalR connected");
  } catch (err) {
    console.error("❌ SignalR error:", err);
  }
}

// Khởi tạo bản đồ lộ trình
function initializeRouteMap() {
  const mapContainer = document.getElementById('routeMap');
  
  // Xóa bản đồ cũ nếu có
  if (routeMap) {
    routeMap.remove();
  }
  
  // Tạo bản đồ mới
  routeMap = L.map('routeMap').setView([10.762622, 106.660172], 13);
  
  L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '© OpenStreetMap'
  }).addTo(routeMap);
}

// Lấy GPS và hiển thị lộ trình
async function getGPSAndShowRoute() {
  if (navigator.geolocation) {
    navigator.geolocation.getCurrentPosition(
      async (position) => {
        const lat = position.coords.latitude;
        const lng = position.coords.longitude;
        
        currentPosition = { lat, lng };
        document.getElementById('gpsLocation').value = `${lat.toFixed(6)}, ${lng.toFixed(6)}`;
        
        // Vẽ lộ trình trên bản đồ
        await drawRoute(lat, lng);
      },
      (error) => {
        document.getElementById('gpsLocation').value = 'Không lấy được vị trí GPS';
        console.error('GPS error:', error);
        
        // Sử dụng vị trí mặc định (Hồ Chí Minh)
        currentPosition = { lat: 10.762622, lng: 106.660172 };
        drawRoute(10.762622, 106.660172);
      },
      {
        enableHighAccuracy: true,
        timeout: 10000,
        maximumAge: 0
      }
    );
  } else {
    document.getElementById('gpsLocation').value = 'Trình duyệt không hỗ trợ GPS';
  }
}

// Vẽ lộ trình từ shipper → khách hàng
async function drawRoute(shipperLat, shipperLng) {
  if (!routeMap || !currentOrderData || !currentOrderData.customer) return;
  
  // Xóa markers và polyline cũ
  if (shipperMarker) routeMap.removeLayer(shipperMarker);
  if (customerMarker) routeMap.removeLayer(customerMarker);
  if (routePolyline) routeMap.removeLayer(routePolyline);
  
  // Icon cho shipper (màu xanh lá)
  const shipperIcon = L.icon({
    iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-green.png',
    shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/0.7.7/images/marker-shadow.png',
    iconSize: [25, 41],
    iconAnchor: [12, 41],
    popupAnchor: [1, -34],
    shadowSize: [41, 41]
  });
  
  // Icon cho khách hàng (màu đỏ)
  const customerIcon = L.icon({
    iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-red.png',
    shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/0.7.7/images/marker-shadow.png',
    iconSize: [25, 41],
    iconAnchor: [12, 41],
    popupAnchor: [1, -34],
    shadowSize: [41, 41]
  });
  
  // Marker vị trí shipper
  shipperMarker = L.marker([shipperLat, shipperLng], { icon: shipperIcon }).addTo(routeMap);
  shipperMarker.bindPopup("<b>🚚 Vị trí của bạn</b><br>Shipper");
  
  // Marker vị trí khách hàng (giả sử hoặc geocode từ địa chỉ)
  // Ở đây tôi sẽ tạo vị trí giả cách shipper ~5km để demo
  const customerLat = shipperLat + 0.03; // ~3km về phía Bắc
  const customerLng = shipperLng + 0.02; // ~2km về phía Đông
  
  customerMarker = L.marker([customerLat, customerLng], { icon: customerIcon }).addTo(routeMap);
  customerMarker.bindPopup(`<b>📍 Khách hàng</b><br>${currentOrderData.customer.fullName}<br>${currentOrderData.customer.address}`);
  
  // Vẽ đường đi
  routePolyline = L.polyline([
    [shipperLat, shipperLng],
    [customerLat, customerLng]
  ], {
    color: 'blue',
    weight: 4,
    opacity: 0.7
  }).addTo(routeMap);
  
  // Fit bounds để hiển thị cả 2 điểm
  routeMap.fitBounds(routePolyline.getBounds(), { padding: [50, 50] });
  
  // Tính khoảng cách
  const distance = calculateDistance(shipperLat, shipperLng, customerLat, customerLng);
  document.getElementById('distanceInfo').innerHTML = 
    `<i class="fas fa-route"></i> Khoảng cách ước tính: <strong>${distance.toFixed(2)} km</strong>`;
}

// Tính khoảng cách giữa 2 điểm (Haversine formula)
function calculateDistance(lat1, lon1, lat2, lon2) {
  const R = 6371; // Bán kính trái đất (km)
  const dLat = (lat2 - lat1) * Math.PI / 180;
  const dLon = (lon2 - lon1) * Math.PI / 180;
  const a = 
    Math.sin(dLat/2) * Math.sin(dLat/2) +
    Math.cos(lat1 * Math.PI / 180) * Math.cos(lat2 * Math.PI / 180) *
    Math.sin(dLon/2) * Math.sin(dLon/2);
  const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1-a));
  return R * c;
}

// Làm mới GPS
window.refreshGPS = async function() {
  document.getElementById('gpsLocation').value = 'Đang lấy GPS...';
  await getGPSAndShowRoute();
  utils.showToast('📍 Đã cập nhật vị trí', 'info');
};

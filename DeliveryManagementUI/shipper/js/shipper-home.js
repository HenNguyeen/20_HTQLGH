// shipper-home.js

let myOrders = [];
let staffInfo = null; // { staffId, isAvailable }
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
  await loadMyRatings();
  setupAvailabilityToggle();
  setupUpdateStatusModal();
  await initializeSignalR();
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

// Load my ratings
async function loadMyRatings() {
  try {
    const data = await apiService.getMyRatings();
    
    // Hiển thị điểm trung bình
    document.getElementById('myAverageRating').textContent = data.averageRating.toFixed(1);
    document.getElementById('totalRatings').textContent = data.totalFeedbacks;
    
    // Hiển thị sao
    const stars = document.getElementById('ratingStars');
    const fullStars = Math.floor(data.averageRating);
    const hasHalfStar = data.averageRating % 1 >= 0.5;
    
    let starsHtml = '';
    for (let i = 1; i <= 5; i++) {
      if (i <= fullStars) {
        starsHtml += '<i class="fas fa-star text-warning"></i> ';
      } else if (i === fullStars + 1 && hasHalfStar) {
        starsHtml += '<i class="fas fa-star-half-alt text-warning"></i> ';
      } else {
        starsHtml += '<i class="far fa-star text-warning"></i> ';
      }
    }
    stars.innerHTML = starsHtml;
    
    // Hiển thị feedback gần đây
    const recentRatingsDiv = document.getElementById('recentRatings');
    if (data.feedbacks && data.feedbacks.length > 0) {
      recentRatingsDiv.innerHTML = data.feedbacks.slice(0, 5).map(f => {
        const stars = '⭐'.repeat(f.rating);
        const date = new Date(f.createdAt).toLocaleDateString('vi-VN');
        return `
          <div class="mb-3 pb-3 border-bottom">
            <div class="d-flex justify-content-between mb-1">
              <span class="text-warning">${stars} ${f.rating}/5</span>
              <small class="text-muted">${date}</small>
            </div>
            <small class="text-muted">Đơn: ${f.orderCode || f.orderId}</small>
            <p class="mb-0">${f.comment || 'Không có nhận xét'}</p>
          </div>
        `;
      }).join('');
    } else {
      recentRatingsDiv.innerHTML = '<p class="text-muted text-center">Chưa có đánh giá nào</p>';
    }
  } catch (e) {
    console.error('Error loading ratings:', e);
    document.getElementById('myAverageRating').textContent = '0.0';
    document.getElementById('totalRatings').textContent = '0';
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

// Render all orders
function renderRecentOrders() {
  const tbody = document.getElementById('recentOrdersBody');
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

// ============ MODAL & ORDER MANAGEMENT ============

// Open modal to view order details
window.openOrderDetailModal = async function(orderId) {
  currentOrderData = myOrders.find(o => o.orderId == orderId);
  
  if (!currentOrderData) {
    utils.showToast('Không tìm thấy đơn hàng', 'danger');
    return;
  }
  
  document.getElementById('modalOrderId').value = orderId;
  document.getElementById('orderStatusSelect').value = currentOrderData.status;
  document.getElementById('gpsLocation').value = 'Đang lấy GPS...';
  document.getElementById('deliveryNotes').value = '';
  
  // Display order details
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
  
  setTimeout(() => initializeRouteMap(), 300);
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
      await apiService.updateOrderStatus(orderId, newStatus, gps, notes);
      
      if (shareLocation && trackingConnection && trackingConnection.state === signalR.HubConnectionState.Connected) {
        await trackingConnection.invoke("UpdateShipperLocation", staffInfo.staffId, parseInt(orderId), currentPosition.lat, currentPosition.lng);
      }
      
      utils.showToast('✅ Cập nhật thành công!', 'success');
      await loadMyOrders();
      bootstrap.Modal.getInstance(document.getElementById('updateStatusModal')).hide();
    } catch (err) {
      console.error(err);
      utils.showToast('❌ Cập nhật thất bại: ' + (err.message || err), 'danger');
    }
  });
}

// ============ GPS & MAP FUNCTIONS ============

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

function initializeRouteMap() {
  if (routeMap) routeMap.remove();
  routeMap = L.map('routeMap').setView([10.762622, 106.660172], 13);
  L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '© OpenStreetMap'
  }).addTo(routeMap);
}

async function getGPSAndShowRoute() {
  if (navigator.geolocation) {
    navigator.geolocation.getCurrentPosition(
      async (position) => {
        currentPosition = { lat: position.coords.latitude, lng: position.coords.longitude };
        document.getElementById('gpsLocation').value = `${currentPosition.lat.toFixed(6)}, ${currentPosition.lng.toFixed(6)}`;
        await drawRoute(currentPosition.lat, currentPosition.lng);
      },
      (error) => {
        document.getElementById('gpsLocation').value = 'Không lấy được GPS';
        currentPosition = { lat: 10.762622, lng: 106.660172 };
        drawRoute(10.762622, 106.660172);
      },
      { enableHighAccuracy: true, timeout: 10000, maximumAge: 0 }
    );
  }
}

async function drawRoute(shipperLat, shipperLng) {
  if (!routeMap || !currentOrderData || !currentOrderData.customer) return;
  
  if (shipperMarker) routeMap.removeLayer(shipperMarker);
  if (customerMarker) routeMap.removeLayer(customerMarker);
  if (routePolyline) routeMap.removeLayer(routePolyline);
  
  const shipperIcon = L.icon({
    iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-green.png',
    shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/0.7.7/images/marker-shadow.png',
    iconSize: [25, 41], iconAnchor: [12, 41], popupAnchor: [1, -34], shadowSize: [41, 41]
  });
  
  const customerIcon = L.icon({
    iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-red.png',
    shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/0.7.7/images/marker-shadow.png',
    iconSize: [25, 41], iconAnchor: [12, 41], popupAnchor: [1, -34], shadowSize: [41, 41]
  });
  
  shipperMarker = L.marker([shipperLat, shipperLng], { icon: shipperIcon }).addTo(routeMap);
  shipperMarker.bindPopup("<b>🚚 Vị trí của bạn</b>");
  
  const customerLat = shipperLat + 0.03;
  const customerLng = shipperLng + 0.02;
  
  customerMarker = L.marker([customerLat, customerLng], { icon: customerIcon }).addTo(routeMap);
  customerMarker.bindPopup(`<b>📍 ${currentOrderData.customer.fullName}</b><br>${currentOrderData.customer.address}`);
  
  routePolyline = L.polyline([[shipperLat, shipperLng], [customerLat, customerLng]], { color: 'blue', weight: 4, opacity: 0.7 }).addTo(routeMap);
  routeMap.fitBounds(routePolyline.getBounds(), { padding: [50, 50] });
  
  const distance = calculateDistance(shipperLat, shipperLng, customerLat, customerLng);
  document.getElementById('distanceInfo').innerHTML = `<i class="fas fa-route"></i> Khoảng cách: <strong>${distance.toFixed(2)} km</strong>`;
}

function calculateDistance(lat1, lon1, lat2, lon2) {
  const R = 6371;
  const dLat = (lat2 - lat1) * Math.PI / 180;
  const dLon = (lon2 - lon1) * Math.PI / 180;
  const a = Math.sin(dLat/2) * Math.sin(dLat/2) + Math.cos(lat1 * Math.PI / 180) * Math.cos(lat2 * Math.PI / 180) * Math.sin(dLon/2) * Math.sin(dLon/2);
  const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1-a));
  return R * c;
}

window.refreshGPS = async function() {
  document.getElementById('gpsLocation').value = 'Đang lấy GPS...';
  await getGPSAndShowRoute();
  utils.showToast('📍 Đã cập nhật vị trí', 'info');
};

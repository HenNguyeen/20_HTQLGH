// customer-tracking.js

let map, routeLayer, checkpointMarkers = [];
let shipperMarker = null; // Marker vị trí shipper realtime
let trackingConnection = null; // SignalR connection
let currentOrderId = null;

window.addEventListener('DOMContentLoaded', () => {
  initMap();
  setupSearchForm();
  initializeSignalR();
  
  const urlParams = new URLSearchParams(window.location.search);
  const orderCode = urlParams.get('order');
  const orderId = urlParams.get('orderId');
  
  if (orderCode) {
    document.getElementById('orderCodeInput').value = orderCode;
    searchOrder(orderCode, orderId);
  }
});

function initMap() {
  map = L.map('map').setView([10.762622, 106.660172], 12);
  L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', { maxZoom: 19, attribution: '© OpenStreetMap' }).addTo(map);
}

function setupSearchForm() {
  document.getElementById('searchOrderForm').addEventListener('submit', function (e) {
    e.preventDefault();
    const code = document.getElementById('orderCodeInput').value.trim();
    if (code) searchOrder(code);
  });
}

async function searchOrder(orderCode, orderId = null) {
  try {
    const data = await apiService.trackByOrderCode(orderCode);
    // API returns: { order: {...}, checkpoints: [...] } or { Order: {...}, Checkpoints: [...] }
    const order = data.order || data.Order;
    const checkpoints = data.checkpoints || data.Checkpoints || [];
    
    renderOrder(order);
    renderCheckpoints(checkpoints);
    
    // Hiển thị vị trí khách hàng và shipper trên bản đồ
    showOrderOnMap(order);
    
    // Tham gia tracking realtime
    const trackOrderId = orderId || order?.orderId || order?.OrderId;
    if (trackOrderId) {
      await joinOrderTracking(trackOrderId);
      utils.showToast('🚚 Đang theo dõi vị trí shipper realtime...', 'info');
    }
  } catch (err) {
    console.error('Tracking error:', err);
    utils.showToast('Không tìm thấy đơn hoặc lỗi server', 'danger');
  }
}

function renderOrder(order) {
  const el = document.getElementById('orderInfo');
  if (!order) {
    el.innerHTML = '<div class="text-muted">Không có thông tin đơn</div>';
    return;
  }
  
  const isTracking = order.status === 2 || order.status === 3; // Đang lấy hàng hoặc đang giao
  const trackingBadge = isTracking 
    ? '<span class="badge bg-success"><i class="fas fa-satellite-dish"></i> Đang theo dõi realtime</span>'
    : '<span class="badge bg-secondary">Chưa giao hàng</span>';
  
  el.innerHTML = `
    <div class="mb-2"><strong>Mã đơn:</strong> ${order.orderCode || '-'}</div>
    <div class="mb-2"><strong>Trạng thái:</strong> ${utils.getStatusText(order.status)}</div>
    <div class="mb-2"><strong>Phí:</strong> ${utils.formatCurrency(order.shippingFee || order.totalFee || 0)}</div>
    <div>${trackingBadge}</div>
  `;
}

function renderCheckpoints(checkpoints) {
  // Clear old checkpoint markers and route
  if (checkpointMarkers.length) {
    checkpointMarkers.forEach((m) => m.remove());
    checkpointMarkers = [];
  }
  if (routeLayer) {
    routeLayer.remove();
    routeLayer = null;
  }

  if (!checkpoints.length) return;

  const latlngs = checkpoints.map((c) => [c.latitude, c.longitude]);
  routeLayer = L.polyline(latlngs, { color: 'blue' }).addTo(map);
  
  checkpoints.forEach((c, idx) => {
    const marker = L.marker([c.latitude, c.longitude]).addTo(map);
    const time = c.checkInTime || c.timestamp;
    marker.bindPopup(`<b>Checkpoint ${idx + 1}</b><br>${time ? utils.formatDate(time) : 'N/A'}`);
    checkpointMarkers.push(marker);
  });
}

// Hiển thị vị trí khách hàng và shipper trên bản đồ
let customerMarker = null;
let deliveryRouteLayer = null;

async function showOrderOnMap(order) {
  if (!order) return;
  
  const customer = order.customer || order.Customer;
  const staff = order.assignedStaff || order.AssignedStaff;
  
  // Hiển thị địa chỉ khách hàng (marker đỏ)
  if (customer) {
    const address = customer.address || customer.Address;
    const city = customer.city || customer.City || '';
    const district = customer.district || customer.District || '';
    const ward = customer.ward || customer.Ward || '';
    
    const fullAddress = `${address}, ${ward}, ${district}, ${city}`.replace(/^,\s*|,\s*,/g, ', ').trim();
    
    if (fullAddress) {
      try {
        // Geocode địa chỉ khách hàng
        const coords = await geocodeAddress(fullAddress);
        
        if (customerMarker) customerMarker.remove();
        
        const redIcon = L.icon({
          iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-red.png',
          shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/0.7.7/images/marker-shadow.png',
          iconSize: [25, 41],
          iconAnchor: [12, 41],
          popupAnchor: [1, -34],
          shadowSize: [41, 41]
        });
        
        customerMarker = L.marker([coords.lat, coords.lng], { icon: redIcon }).addTo(map);
        customerMarker.bindPopup(`<b>📍 Địa chỉ giao hàng</b><br>${fullAddress}`);
        
        map.setView([coords.lat, coords.lng], 13);
      } catch (err) {
        console.error('Geocoding failed:', err);
        // Fallback to default location (Saigon)
        const defaultLat = 10.762622;
        const defaultLng = 106.660172;
        
        if (customerMarker) customerMarker.remove();
        customerMarker = L.marker([defaultLat, defaultLng]).addTo(map);
        customerMarker.bindPopup(`<b>📍 Địa chỉ giao hàng</b><br>${fullAddress}`);
        map.setView([defaultLat, defaultLng], 12);
      }
    }
  }
  
  // Hiển thị vị trí shipper hiện tại (nếu có)
  if (staff) {
    const lat = staff.currentLatitude || staff.CurrentLatitude;
    const lng = staff.currentLongitude || staff.CurrentLongitude;
    
    if (lat && lng) {
      updateShipperMarker(lat, lng, new Date().toISOString());
      
      // Vẽ đường từ shipper đến khách hàng
      if (customerMarker) {
        drawDeliveryRoute(lat, lng, customerMarker.getLatLng().lat, customerMarker.getLatLng().lng);
      }
    }
  }
}

// Geocode địa chỉ thành tọa độ GPS
async function geocodeAddress(address) {
  try {
    const response = await fetch(`https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(address)}&limit=1`);
    const data = await response.json();
    
    if (data && data.length > 0) {
      return {
        lat: parseFloat(data[0].lat),
        lng: parseFloat(data[0].lon)
      };
    }
    throw new Error('No results found');
  } catch (err) {
    console.error('Geocoding error:', err);
    // Default to Ho Chi Minh City center
    return { lat: 10.762622, lng: 106.660172 };
  }
}

// Vẽ đường từ shipper đến khách hàng
function drawDeliveryRoute(shipperLat, shipperLng, customerLat, customerLng) {
  // Xóa đường cũ
  if (deliveryRouteLayer) {
    deliveryRouteLayer.remove();
  }
  
  // Vẽ đường mới
  const latlngs = [
    [shipperLat, shipperLng],
    [customerLat, customerLng]
  ];
  
  deliveryRouteLayer = L.polyline(latlngs, { 
    color: '#FF6B35', 
    weight: 3, 
    opacity: 0.7,
    dashArray: '10, 10'
  }).addTo(map);
  
  // Zoom để hiển thị cả 2 điểm
  const bounds = L.latLngBounds([
    [shipperLat, shipperLng],
    [customerLat, customerLng]
  ]);
  map.fitBounds(bounds, { padding: [50, 50] });
  
  // Tính khoảng cách
  const distance = calculateDistance(shipperLat, shipperLng, customerLat, customerLng);
  console.log(`📏 Khoảng cách: ${distance.toFixed(2)} km`);
}

// Tính khoảng cách giữa 2 điểm (Haversine formula)
function calculateDistance(lat1, lon1, lat2, lon2) {
  const R = 6371; // Bán kính trái đất (km)
  const dLat = (lat2 - lat1) * Math.PI / 180;
  const dLon = (lon2 - lon1) * Math.PI / 180;
  const a = Math.sin(dLat/2) * Math.sin(dLat/2) +
            Math.cos(lat1 * Math.PI / 180) * Math.cos(lat2 * Math.PI / 180) *
            Math.sin(dLon/2) * Math.sin(dLon/2);
  const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1-a));
  return R * c;
}

// ============ REALTIME TRACKING ============

// Khởi tạo SignalR connection
async function initializeSignalR() {
  try {
    const apiUrl = window.location.origin;
    trackingConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${apiUrl}/trackingHub`)
      .withAutomaticReconnect()
      .build();

    // Lắng nghe sự kiện cập nhật vị trí shipper
    trackingConnection.on("ReceiveShipperLocation", (data) => {
      updateShipperMarker(data.latitude, data.longitude, data.timestamp);
    });

    await trackingConnection.start();
    console.log("✅ SignalR tracking connected");
  } catch (err) {
    console.error("❌ SignalR connection error:", err);
  }
}

// Tham gia tracking một đơn hàng
async function joinOrderTracking(orderId) {
  if (trackingConnection && trackingConnection.state === signalR.HubConnectionState.Connected) {
    // Rời khỏi đơn cũ nếu có
    if (currentOrderId) {
      await trackingConnection.invoke("LeaveOrderTracking", currentOrderId);
    }
    
    currentOrderId = orderId;
    await trackingConnection.invoke("JoinOrderTracking", orderId);
    console.log(`📍 Joined tracking for order: ${orderId}`);
  }
}

// Cập nhật marker vị trí shipper trên bản đồ
function updateShipperMarker(lat, lng, timestamp) {
  if (!shipperMarker) {
    // Tạo icon xe tải cho shipper
    const truckIcon = L.icon({
      iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-green.png',
      shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/0.7.7/images/marker-shadow.png',
      iconSize: [25, 41],
      iconAnchor: [12, 41],
      popupAnchor: [1, -34],
      shadowSize: [41, 41]
    });
    
    shipperMarker = L.marker([lat, lng], { icon: truckIcon }).addTo(map);
    shipperMarker.bindPopup("<b>🚚 Shipper đang giao</b><br>Vị trí realtime");
  } else {
    // Di chuyển marker đến vị trí mới
    shipperMarker.setLatLng([lat, lng]);
  }
  
  shipperMarker.openPopup();
  
  // Vẽ lại đường đi nếu có địa chỉ khách hàng
  if (customerMarker) {
    drawDeliveryRoute(lat, lng, customerMarker.getLatLng().lat, customerMarker.getLatLng().lng);
  }
  
  // Hiển thị thông báo
  const time = new Date(timestamp).toLocaleTimeString('vi-VN');
  utils.showToast(`📍 Shipper cập nhật vị trí lúc ${time}`, 'info');
}

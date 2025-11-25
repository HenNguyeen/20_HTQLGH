// tracking.js - GPS Tracking Page Logic

let map, routeLayer, checkpointMarkers = [];
let shipperMarker = null; // Marker cho vị trí shipper realtime
let trackingConnection = null; // SignalR connection
let currentOrderId = null;

// On page load
window.addEventListener('DOMContentLoaded', () => {
    // Note: Tracking page allows anonymous access for public order tracking
    // But check if user is logged in to show additional features
    const isLoggedIn = auth.isLoggedIn();
    
    if (isLoggedIn) {
        displayUserInfo();
    } else {
        // Hide logged-in only features
        const authElements = document.querySelectorAll('.auth-only');
        authElements.forEach(el => el.style.display = 'none');
    }
    
    initializeSidebar();
    initializeMap();
    setupSearchForm();
    initializeSignalR();
    
    // Auto-load if order code in URL
    const urlParams = new URLSearchParams(window.location.search);
    const orderCode = urlParams.get('order');
    if (orderCode) {
        document.getElementById('orderCodeInput').value = orderCode;
        searchOrder(orderCode);
    }
});

// Display user information
function displayUserInfo() {
    const user = auth.getCurrentUser();
    if (user) {
        // Update UI to show user is logged in
        const userElements = document.querySelectorAll('.user-name');
        userElements.forEach(el => {
            el.textContent = user.fullName;
        });
    }
}

function initializeSidebar() {
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebar = document.getElementById('sidebar');
    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', function() {
            sidebar.classList.toggle('collapsed');
        });
    }
}

function initializeMap() {
    map = L.map('map').setView([10.762622, 106.660172], 12); // Default: HCM
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '© OpenStreetMap'
    }).addTo(map);
}

// Khởi tạo SignalR connection cho realtime tracking
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
        currentOrderId = orderId;
        await trackingConnection.invoke("JoinOrderTracking", orderId);
        console.log(`📍 Joined tracking for order: ${orderId}`);
    }
}

// Rời khỏi tracking đơn hàng
async function leaveOrderTracking() {
    if (trackingConnection && currentOrderId) {
        await trackingConnection.invoke("LeaveOrderTracking", currentOrderId);
        console.log(`🚪 Left tracking for order: ${currentOrderId}`);
        currentOrderId = null;
    }
}

// Cập nhật marker vị trí shipper trên bản đồ
function updateShipperMarker(lat, lng, timestamp) {
    if (!shipperMarker) {
        // Tạo icon xe tải cho shipper
        const truckIcon = L.icon({
            iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-blue.png',
            shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/0.7.7/images/marker-shadow.png',
            iconSize: [25, 41],
            iconAnchor: [12, 41],
            popupAnchor: [1, -34],
            shadowSize: [41, 41]
        });
        
        shipperMarker = L.marker([lat, lng], { icon: truckIcon }).addTo(map);
        shipperMarker.bindPopup("<b>🚚 Shipper</b><br>Vị trí hiện tại");
    } else {
        // Di chuyển marker đến vị trí mới
        shipperMarker.setLatLng([lat, lng]);
    }
    
    shipperMarker.openPopup();
    showTrackingAlert(`📍 Cập nhật vị trí shipper: ${new Date(timestamp).toLocaleTimeString('vi-VN')}`, 'info');
}

function setupSearchForm() {
    document.getElementById('searchOrderForm').addEventListener('submit', function(e) {
        e.preventDefault();
        const code = document.getElementById('orderCodeInput').value.trim();
        if (code) searchOrder(code);
    });
}

async function searchOrder(orderCode) {
    showTrackingAlert('Đang tải dữ liệu đơn hàng...', 'info');
    clearMapAndTimeline();
    
    // Rời khỏi tracking đơn cũ (nếu có)
    await leaveOrderTracking();
    
    try {
        // Use trackByOrderCode which returns { Order, Checkpoints }
        const tracking = await ApiService.trackByOrderCode(orderCode);
        if (!tracking || !tracking.order) {
            showTrackingAlert('Không tìm thấy đơn hàng với mã: ' + orderCode, 'danger');
            return;
        }
        const order = tracking.order || tracking.Order || tracking.Order;
        const checkpoints = tracking.checkpoints || tracking.Checkpoints || [];
        
        renderOrderInfo(order);
        
        // Tham gia tracking realtime cho đơn hàng này
        await joinOrderTracking(order.orderId);
        
        if (!checkpoints || checkpoints.length === 0) {
            showTrackingAlert('Chưa có dữ liệu checkpoint. Đang theo dõi vị trí shipper realtime...', 'warning');
            return;
        }
        renderRouteOnMap(checkpoints);
        renderTimeline(checkpoints);
        showTrackingAlert('✅ Đã tải xong lộ trình. Đang theo dõi vị trí shipper realtime...', 'success');
    } catch (err) {
        showTrackingAlert('Lỗi khi tải dữ liệu: ' + err, 'danger');
    }
}

function renderOrderInfo(order) {
    const infoDiv = document.getElementById('orderInfo');
    infoDiv.innerHTML = `
        <div class="mb-2"><strong>Mã đơn:</strong> ${order.orderCode}</div>
        <div class="mb-2"><strong>Người nhận:</strong> ${order.receiver.fullName} (${order.receiver.phoneNumber})</div>
        <div class="mb-2"><strong>Địa chỉ:</strong> ${order.receiver.address}</div>
        <div class="mb-2"><strong>Trạng thái:</strong> ${getStatusBadge(order.status)}</div>
    `;
}

function renderRouteOnMap(checkpoints) {
    if (routeLayer) {
        map.removeLayer(routeLayer);
    }
    checkpointMarkers.forEach(m => map.removeLayer(m));
    checkpointMarkers = [];
    const latlngs = checkpoints.map(cp => [cp.latitude, cp.longitude]);
    routeLayer = L.polyline(latlngs, { color: '#0d6efd', weight: 5 }).addTo(map);
    latlngs.forEach((latlng, idx) => {
        const marker = L.marker(latlng).addTo(map);
        // Backend returns CheckInTime and Notes (PascalCase) -> serialized as-is
        marker.bindPopup(`<b>Checkpoint ${idx + 1}</b><br>${formatCheckpointTime(checkpoints[idx].checkInTime)}`);
        checkpointMarkers.push(marker);
    });
    map.fitBounds(routeLayer.getBounds(), { padding: [30, 30] });
}

function renderTimeline(checkpoints) {
    const timeline = document.getElementById('timeline');
    timeline.innerHTML = checkpoints.map((cp, idx) => `
        <div class="timeline-event">
            <div class="timeline-dot"></div>
            <div><strong>Checkpoint ${idx + 1}</strong> - ${formatCheckpointTime(cp.checkInTime)}</div>
            <div class="text-muted small">Vị trí: (${cp.latitude.toFixed(5)}, ${cp.longitude.toFixed(5)})</div>
            <div class="text-muted small">${cp.notes || ''}</div>
        </div>
    `).join('');
}

function clearMapAndTimeline() {
    if (routeLayer) map.removeLayer(routeLayer);
    checkpointMarkers.forEach(m => map.removeLayer(m));
    checkpointMarkers = [];
    if (shipperMarker) {
        map.removeLayer(shipperMarker);
        shipperMarker = null;
    }
    document.getElementById('timeline').innerHTML = '';
    document.getElementById('orderInfo').innerHTML = '';
}

function showTrackingAlert(message, type) {
    const alertDiv = document.getElementById('trackingAlert');
    alertDiv.innerHTML = `<div class="alert alert-${type} mt-2">${message}</div>`;
    setTimeout(() => { alertDiv.innerHTML = ''; }, 3500);
}

function formatCheckpointTime(ts) {
    if (!ts) return '';
    const d = new Date(ts);
    return d.toLocaleString('vi-VN');
}

// Helper: status badge (reuse from orders.js if needed)
function getStatusBadge(status) {
    switch (status) {
        case 1: return '<span class="badge bg-secondary">Chờ xác nhận</span>';
        case 2: return '<span class="badge bg-info">Đang lấy hàng</span>';
        case 3: return '<span class="badge bg-warning">Đang giao</span>';
        case 4: return '<span class="badge bg-success">Đã giao</span>';
        case 5: return '<span class="badge bg-danger">Đã hủy</span>';
        default: return '<span class="badge bg-light text-dark">Không rõ</span>';
    }
}

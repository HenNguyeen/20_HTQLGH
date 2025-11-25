// shipper-simulator.js - Mô phỏng vị trí shipper cho testing

let map, routePoints = [], currentPointIndex = 0;
let simulationInterval = null;
let trackingConnection = null;
let shipperMarker = null;
let routePolyline = null;

// Khởi tạo
window.addEventListener('DOMContentLoaded', () => {
    initializeMap();
    initializeSignalR();
    setupSpeedControl();
});

function initializeMap() {
    // Khởi tạo bản đồ tại Hồ Chí Minh
    map = L.map('simulatorMap').setView([10.762622, 106.660172], 13);
    
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '© OpenStreetMap'
    }).addTo(map);

    // Click để thêm điểm vào lộ trình
    map.on('click', function(e) {
        addRoutePoint(e.latlng.lat, e.latlng.lng);
    });

    // Icon cho shipper
    const truckIcon = L.icon({
        iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-green.png',
        shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/0.7.7/images/marker-shadow.png',
        iconSize: [25, 41],
        iconAnchor: [12, 41],
        popupAnchor: [1, -34],
        shadowSize: [41, 41]
    });

    shipperMarker = L.marker([10.762622, 106.660172], { icon: truckIcon }).addTo(map);
    shipperMarker.bindPopup("<b>🚚 Shipper</b><br>Click vào bản đồ để tạo lộ trình");
}

async function initializeSignalR() {
    try {
        const apiUrl = window.location.origin;
        trackingConnection = new signalR.HubConnectionBuilder()
            .withUrl(`${apiUrl}/trackingHub`)
            .withAutomaticReconnect()
            .build();

        await trackingConnection.start();
        console.log("✅ SignalR connected");
        updateStatus("Đã kết nối SignalR", "success");
    } catch (err) {
        console.error("❌ SignalR error:", err);
        updateStatus("Lỗi kết nối SignalR", "danger");
    }
}

function setupSpeedControl() {
    const speedInput = document.getElementById('speedInput');
    const speedValue = document.getElementById('speedValue');
    
    speedInput.addEventListener('input', () => {
        speedValue.textContent = `${speedInput.value} giây`;
    });
}

function addRoutePoint(lat, lng) {
    routePoints.push({ lat, lng });
    
    // Thêm marker
    const marker = L.circleMarker([lat, lng], {
        radius: 5,
        fillColor: "#ff7800",
        color: "#000",
        weight: 1,
        opacity: 1,
        fillOpacity: 0.8
    }).addTo(map);
    
    marker.bindPopup(`Điểm ${routePoints.length}`);
    
    // Vẽ đường đi
    updateRoutePolyline();
    
    // Cập nhật UI
    document.getElementById('pointsCount').textContent = routePoints.length;
    updateStatus(`Đã thêm điểm ${routePoints.length}`, "info");
}

function updateRoutePolyline() {
    if (routePolyline) {
        map.removeLayer(routePolyline);
    }
    
    if (routePoints.length > 1) {
        const latlngs = routePoints.map(p => [p.lat, p.lng]);
        routePolyline = L.polyline(latlngs, {
            color: 'blue',
            weight: 3,
            opacity: 0.7
        }).addTo(map);
    }
}

function startSimulation() {
    if (routePoints.length < 2) {
        alert('Vui lòng tạo ít nhất 2 điểm trên bản đồ!');
        return;
    }

    if (!trackingConnection || trackingConnection.state !== signalR.HubConnectionState.Connected) {
        alert('SignalR chưa kết nối!');
        return;
    }

    const orderId = parseInt(document.getElementById('orderIdInput').value);
    const staffId = parseInt(document.getElementById('staffIdInput').value);
    
    if (!orderId || !staffId) {
        alert('Vui lòng nhập mã đơn hàng và ID shipper!');
        return;
    }

    currentPointIndex = 0;
    const speed = parseInt(document.getElementById('speedInput').value) * 1000;

    // Di chuyển shipper qua từng điểm
    simulationInterval = setInterval(async () => {
        if (currentPointIndex >= routePoints.length) {
            stopSimulation();
            updateStatus("Hoàn thành lộ trình", "success");
            return;
        }

        const point = routePoints[currentPointIndex];
        
        // Cập nhật vị trí trên bản đồ
        shipperMarker.setLatLng([point.lat, point.lng]);
        shipperMarker.openPopup();
        
        // Gửi vị trí lên server qua SignalR
        try {
            await trackingConnection.invoke("UpdateShipperLocation", staffId, orderId, point.lat, point.lng);
            
            // Cập nhật UI
            document.getElementById('currentLocation').textContent = 
                `${point.lat.toFixed(5)}, ${point.lng.toFixed(5)}`;
            document.getElementById('pointsCount').textContent = 
                `${currentPointIndex + 1} / ${routePoints.length}`;
            
            updateStatus(`Đang di chuyển... (${currentPointIndex + 1}/${routePoints.length})`, "primary");
            
            currentPointIndex++;
        } catch (err) {
            console.error("Error sending location:", err);
            updateStatus("Lỗi gửi vị trí", "danger");
        }
    }, speed);

    // Cập nhật UI
    document.getElementById('startBtn').disabled = true;
    document.getElementById('pauseBtn').disabled = false;
    document.getElementById('stopBtn').disabled = false;
    updateStatus("Đang chạy simulation", "primary");
}

function pauseSimulation() {
    if (simulationInterval) {
        clearInterval(simulationInterval);
        simulationInterval = null;
        
        document.getElementById('startBtn').disabled = false;
        document.getElementById('pauseBtn').disabled = true;
        updateStatus("Đã tạm dừng", "warning");
    }
}

function stopSimulation() {
    if (simulationInterval) {
        clearInterval(simulationInterval);
        simulationInterval = null;
    }
    
    currentPointIndex = 0;
    
    // Reset UI
    document.getElementById('startBtn').disabled = false;
    document.getElementById('pauseBtn').disabled = true;
    document.getElementById('stopBtn').disabled = true;
    document.getElementById('currentLocation').textContent = '-';
    
    updateStatus("Đã dừng", "secondary");
}

function resetRoute() {
    stopSimulation();
    
    // Xóa tất cả markers và polyline
    map.eachLayer(layer => {
        if (layer instanceof L.CircleMarker || layer instanceof L.Polyline) {
            map.removeLayer(layer);
        }
    });
    
    routePoints = [];
    currentPointIndex = 0;
    routePolyline = null;
    
    document.getElementById('pointsCount').textContent = '0';
    document.getElementById('currentLocation').textContent = '-';
    
    updateStatus("Đã đặt lại lộ trình", "secondary");
}

function updateStatus(message, type) {
    const badge = document.getElementById('statusBadge');
    badge.textContent = message;
    badge.className = `badge status-badge bg-${type}`;
}

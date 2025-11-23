// order-detail.js - Real-time Order Tracking with Shipper Location

let map, shipperMarker, destinationMarker, routeLayer, historyPolyline;
let orderId = null;
let pollingInterval = null;
let allCheckpoints = [];
let showHistory = true; // Toggle history route
const POLLING_INTERVAL_MS = 5000; // Poll every 5 seconds

// On page load
window.addEventListener('DOMContentLoaded', () => {
    if (!auth.requireAuth()) return;
    
    displayUserInfo();
    initializeSidebar();
    initializeMap();
    
    // Get orderId from URL query string
    const urlParams = new URLSearchParams(window.location.search);
    orderId = urlParams.get('id');
    
    if (!orderId) {
        alert('Không tìm thấy mã đơn hàng');
        window.location.href = 'orders.html';
        return;
    }
    
    loadOrderDetails();
    startPolling();
});

// Display user information
function displayUserInfo() {
    const user = auth.getCurrentUser();
    if (user) {
        const userElements = document.querySelectorAll('.user-name');
        userElements.forEach(el => el.textContent = user.fullName || user.phoneNumber);
        
        const roleElements = document.querySelectorAll('.user-role');
        roleElements.forEach(el => el.textContent = user.role || 'User');
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
    // Default center: Ho Chi Minh City
    map = L.map('map').setView([10.762622, 106.660172], 13);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '© OpenStreetMap'
    }).addTo(map);
}

async function loadOrderDetails() {
    try {
        const order = await apiService.getOrderById(orderId);
        if (!order) {
            alert('Không tìm thấy đơn hàng');
            window.location.href = 'orders.html';
            return;
        }
        
        renderOrderInfo(order);
        renderShipperInfo(order);
        
        // Load full checkpoint history
        await loadCheckpointHistory();
        
        // Load initial location
        await updateShipperLocation();
        
    } catch (error) {
        console.error('Error loading order:', error);
        alert('Lỗi khi tải thông tin đơn hàng');
    }
}

function renderOrderInfo(order) {
    const statusMap = {
        0: { text: 'Chờ xác nhận', class: 'secondary' },
        1: { text: 'Chờ lấy hàng', class: 'info' },
        2: { text: 'Đang giao', class: 'warning' },
        3: { text: 'Đã giao', class: 'success' },
        4: { text: 'Đã hủy', class: 'danger' }
    };
    
    const status = statusMap[order.status] || { text: 'Không rõ', class: 'secondary' };
    
    const infoHtml = `
        <div class="mb-3">
            <strong>Mã đơn:</strong> <span class="badge bg-primary">${order.orderCode || order.orderId}</span>
        </div>
        <div class="mb-3">
            <strong>Trạng thái:</strong><br>
            <span class="badge bg-${status.class}">${status.text}</span>
        </div>
        <hr>
        <h6><i class="fas fa-user"></i> Khách Hàng</h6>
        <div class="mb-2"><strong>Tên:</strong> ${order.receiver?.fullName || order.customer?.fullName || 'N/A'}</div>
        <div class="mb-2"><strong>SĐT:</strong> ${order.receiver?.phoneNumber || order.customer?.phoneNumber || 'N/A'}</div>
        <div class="mb-2"><strong>Địa chỉ:</strong> ${order.receiver?.address || order.customer?.address || 'N/A'}</div>
        <hr>
        <div class="mb-2"><strong>Phí ship:</strong> ${utils.formatCurrency(order.shippingFee || 0)}</div>
        <div class="mb-2"><strong>Tổng tiền:</strong> ${utils.formatCurrency(order.totalAmount || 0)}</div>
    `;
    
    document.getElementById('orderInfo').innerHTML = infoHtml;
}

function renderShipperInfo(order) {
    const shipperDiv = document.getElementById('shipperInfo');
    
    if (order.assignedStaff && order.assignedStaff.fullName) {
        const staff = order.assignedStaff;
        shipperDiv.innerHTML = `
            <div class="mb-2">
                <i class="fas fa-user-circle"></i> 
                <strong>${staff.fullName}</strong>
            </div>
            <div class="mb-2">
                <i class="fas fa-phone"></i> ${staff.phoneNumber || 'N/A'}
            </div>
            <div class="mb-2">
                <i class="fas fa-motorcycle"></i> ${staff.vehicleNumber || 'N/A'}
            </div>
        `;
    } else {
        shipperDiv.innerHTML = '<p class="text-muted">Chưa phân công shipper</p>';
    }
}

async function loadCheckpointHistory() {
    try {
        console.log('=== Loading checkpoint history for orderId:', orderId);
        // Get all checkpoints for this order
        const checkpoints = await apiService.getOrderCheckpoints(orderId);
        console.log('Received checkpoints:', checkpoints);
        
        if (checkpoints && checkpoints.length > 0) {
            allCheckpoints = checkpoints.sort((a, b) => 
                new Date(a.checkInTime) - new Date(b.checkInTime)
            );
            
            console.log(`Loaded ${allCheckpoints.length} checkpoints`);
            
            // Draw history route
            drawHistoryRoute();
            
            // Render timeline
            renderCheckpointTimeline();
        } else {
            console.log('No checkpoint history yet');
        }
    } catch (error) {
        console.error('Error loading checkpoint history:', error);
    }
}

function drawHistoryRoute() {
    if (!showHistory || allCheckpoints.length === 0) {
        if (historyPolyline) {
            map.removeLayer(historyPolyline);
            historyPolyline = null;
        }
        return;
    }
    
    // Remove old polyline
    if (historyPolyline) {
        map.removeLayer(historyPolyline);
    }
    
    // Create array of coordinates
    const latlngs = allCheckpoints.map(cp => [cp.latitude, cp.longitude]);
    
    // Draw polyline (green dashed line for history)
    historyPolyline = L.polyline(latlngs, {
        color: '#4caf50',
        weight: 3,
        opacity: 0.7,
        dashArray: '10, 10'
    }).addTo(map);
    
    // Add markers for checkpoints
    allCheckpoints.forEach((cp, index) => {
        const isFirst = index === 0;
        const isLast = index === allCheckpoints.length - 1;
        
        let icon, label;
        if (isFirst) {
            // Start point - green
            icon = L.divIcon({
                className: 'custom-marker',
                html: '<div style="background: #4caf50; color: white; width: 30px; height: 30px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-weight: bold; border: 3px solid white; box-shadow: 0 2px 5px rgba(0,0,0,0.3);">▶</div>',
                iconSize: [30, 30],
                iconAnchor: [15, 15]
            });
            label = 'Điểm bắt đầu';
        } else if (isLast) {
            // Current/last point - blue (will be updated by real-time marker)
            return; // Skip, we use the main shipperMarker
        } else {
            // Middle points - small gray dots
            icon = L.circleMarker([cp.latitude, cp.longitude], {
                radius: 4,
                fillColor: '#888',
                color: '#fff',
                weight: 2,
                opacity: 1,
                fillOpacity: 0.8
            }).addTo(map);
            
            const time = new Date(cp.checkInTime);
            icon.bindPopup(`
                <b>Checkpoint #${index + 1}</b><br>
                ${time.toLocaleString('vi-VN')}<br>
                <small>${cp.notes || ''}</small>
            `);
            return;
        }
        
        const marker = L.marker([cp.latitude, cp.longitude], { icon }).addTo(map);
        const time = new Date(cp.checkInTime);
        marker.bindPopup(`
            <b>${label}</b><br>
            ${time.toLocaleString('vi-VN')}<br>
            <small>${cp.notes || ''}</small>
        `);
    });
    
    // Fit map to show entire route
    if (latlngs.length > 0) {
        map.fitBounds(historyPolyline.getBounds(), { padding: [50, 50] });
    }
}

function renderCheckpointTimeline() {
    const timelineDiv = document.getElementById('checkpointTimeline');
    
    if (!timelineDiv) return;
    
    if (allCheckpoints.length === 0) {
        timelineDiv.innerHTML = '<p class="text-muted text-center">Chưa có checkpoint nào</p>';
        return;
    }
    
    let html = '<div class="timeline-list">';
    
    allCheckpoints.forEach((cp, index) => {
        const time = new Date(cp.checkInTime);
        const isFirst = index === 0;
        const isLast = index === allCheckpoints.length - 1;
        
        let badge = '';
        if (isFirst) {
            badge = '<span class="badge bg-success">Bắt đầu</span>';
        } else if (isLast) {
            badge = '<span class="badge bg-primary">Hiện tại</span>';
        }
        
        html += `
            <div class="timeline-item ${isLast ? 'current' : ''}" style="padding: 8px 0; border-left: 2px solid ${isLast ? '#2196f3' : '#ddd'}; padding-left: 12px; position: relative; margin-bottom: 8px;">
                <div style="position: absolute; left: -6px; top: 12px; width: 10px; height: 10px; background: ${isLast ? '#2196f3' : '#888'}; border-radius: 50%; border: 2px solid white;"></div>
                <div style="margin-left: 8px;">
                    <div class="d-flex justify-content-between align-items-start">
                        <strong style="font-size: 0.9rem;">#${index + 1}</strong>
                        ${badge}
                    </div>
                    <div style="font-size: 0.85rem; color: #666;">
                        <i class="fas fa-clock"></i> ${time.toLocaleString('vi-VN')}
                    </div>
                    <div style="font-size: 0.8rem; color: #888;">
                        <i class="fas fa-map-marker-alt"></i> ${cp.latitude.toFixed(5)}, ${cp.longitude.toFixed(5)}
                    </div>
                    ${cp.notes ? `<div style="font-size: 0.8rem; color: #555; font-style: italic;">${cp.notes}</div>` : ''}
                </div>
            </div>
        `;
    });
    
    html += '</div>';
    timelineDiv.innerHTML = html;
}

function toggleHistoryRoute() {
    showHistory = !showHistory;
    drawHistoryRoute();
    
    const btn = document.getElementById('btnToggleHistory');
    if (btn) {
        btn.innerHTML = showHistory 
            ? '<i class="fas fa-eye-slash"></i> Ẩn lịch sử' 
            : '<i class="fas fa-eye"></i> Hiển lịch sử';
        btn.className = showHistory ? 'btn btn-sm btn-success' : 'btn btn-sm btn-secondary';
    }
}

async function updateShipperLocation() {
    try {
        console.log('>>> Polling shipper location for orderId:', orderId);
        // Get latest checkpoint/location for this order
        const location = await apiService.getCurrentLocation(orderId);
        console.log('Received location:', location);
        
        if (location && location.latitude && location.longitude) {
            const lat = location.latitude;
            const lng = location.longitude;
            const time = new Date(location.checkInTime);
            
            // Update or create shipper marker
            if (shipperMarker) {
                shipperMarker.setLatLng([lat, lng]);
            } else {
                // Create custom icon for shipper
                const shipperIcon = L.icon({
                    iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-blue.png',
                    shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-shadow.png',
                    iconSize: [25, 41],
                    iconAnchor: [12, 41],
                    popupAnchor: [1, -34],
                    shadowSize: [41, 41]
                });
                
                shipperMarker = L.marker([lat, lng], { icon: shipperIcon }).addTo(map);
                shipperMarker.bindPopup(`
                    <b><i class="fas fa-motorcycle"></i> Vị trí Shipper</b><br>
                    ${location.locationName || 'Đang giao hàng'}<br>
                    <small>${time.toLocaleString('vi-VN')}</small>
                `);
                
                // Center map on shipper
                map.setView([lat, lng], 15);
            }
            
            // Update status indicator
            const now = new Date();
            const minutesAgo = Math.floor((now - time) / 60000);
            const isActive = minutesAgo < 2; // Consider active if updated within 2 minutes
            
            const statusDiv = document.getElementById('locationStatus');
            if (isActive) {
                statusDiv.className = 'shipper-indicator active';
                statusDiv.innerHTML = '<i class="fas fa-circle text-success"></i> Shipper đang chia sẻ vị trí';
            } else {
                statusDiv.className = 'shipper-indicator';
                statusDiv.innerHTML = '<i class="fas fa-circle text-warning"></i> Vị trí chưa được cập nhật gần đây';
            }
            
            document.getElementById('lastUpdate').textContent = 
                `Cập nhật lần cuối: ${minutesAgo < 1 ? 'vừa xong' : minutesAgo + ' phút trước'}`;
            
            // Check if this is a new checkpoint, reload history
            if (allCheckpoints.length === 0 || 
                !allCheckpoints.find(cp => cp.checkpointId === location.checkpointId)) {
                await loadCheckpointHistory();
            }
            
        } else {
            // No location data yet
            console.warn('No location data received');
            document.getElementById('locationStatus').innerHTML = 
                '<i class="fas fa-circle text-muted"></i> Chưa có dữ liệu vị trí';
            document.getElementById('lastUpdate').textContent = '';
        }
        
    } catch (error) {
        console.error('!!! Error updating location:', error);
        console.error('Error details:', error.message, error.stack);
        // Don't show error to user, just log it - might be 404 if no checkpoints yet
    }
}

function startPolling() {
    // Initial update
    updateShipperLocation();
    
    // Poll every 5 seconds
    pollingInterval = setInterval(() => {
        updateShipperLocation();
    }, POLLING_INTERVAL_MS);
}

function stopPolling() {
    if (pollingInterval) {
        clearInterval(pollingInterval);
        pollingInterval = null;
    }
}

// Stop polling when page is hidden/closed
document.addEventListener('visibilitychange', () => {
    if (document.hidden) {
        stopPolling();
    } else {
        startPolling();
    }
});

// Clean up on page unload
window.addEventListener('beforeunload', () => {
    stopPolling();
});

// Expose functions to window for inline handlers
window.toggleHistoryRoute = toggleHistoryRoute;

// customer-tracking.js

let map, routeLayer, checkpointMarkers = [];

window.addEventListener('DOMContentLoaded', () => {
  initMap();
  setupSearchForm();
  const urlParams = new URLSearchParams(window.location.search);
  const orderCode = urlParams.get('order');
  if (orderCode) {
    document.getElementById('orderCodeInput').value = orderCode;
    searchOrder(orderCode);
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

async function searchOrder(orderCode) {
  try {
    const data = await apiService.trackByOrderCode(orderCode);
    renderOrder(data.order);
    renderCheckpoints(data.checkpoints || []);
  } catch (err) {
    utils.showToast('Không tìm thấy đơn hoặc lỗi server', 'danger');
  }
}

function renderOrder(order) {
  const el = document.getElementById('orderInfo');
  if (!order) {
    el.innerHTML = '<div class="text-muted">Không có thông tin đơn</div>';
    return;
  }
  el.innerHTML = `
    <div><strong>Mã đơn:</strong> ${order.orderCode || '-'}</div>
    <div><strong>Trạng thái:</strong> ${utils.getStatusText(order.status)}</div>
    <div><strong>Phí:</strong> ${utils.formatCurrency(order.shippingFee || order.totalFee || 0)}</div>
  `;
}

function renderCheckpoints(checkpoints) {
  // Clear markers
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
  map.fitBounds(routeLayer.getBounds(), { padding: [20, 20] });

  checkpoints.forEach((c, idx) => {
    const marker = L.marker([c.latitude, c.longitude]).addTo(map);
    marker.bindPopup(`<b>Checkpoint ${idx + 1}</b><br>${utils.formatDate(c.timestamp)}`);
    checkpointMarkers.push(marker);
  });
}

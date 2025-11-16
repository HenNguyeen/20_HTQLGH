// customer-home.js

let recentOrders = [];
let selectedStatus = null; // null or numeric string or 'draft'

window.addEventListener('DOMContentLoaded', async () => {
  await loadRecentMyOrders();

  // Wire global search input (if present)
  const globalSearch = document.getElementById('globalSearch');
  if (globalSearch) {
    globalSearch.addEventListener('input', () => applyFilters());
  }

  // Wire status tabs
  const tabs = document.querySelectorAll('.status-tab');
  tabs.forEach((t) => {
    t.addEventListener('click', (ev) => {
      tabs.forEach(x => x.classList.remove('active'));
      t.classList.add('active');
      selectedStatus = t.getAttribute('data-status');
      applyFilters();
    });
  });
});

async function loadRecentMyOrders() {
  const tbody = document.getElementById('recentOrdersBody');
  try {
    const myOrders = await apiService.getMyOrders();
    recentOrders = myOrders || [];
    if (!recentOrders || recentOrders.length === 0) {
      tbody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">Chưa có đơn hàng nào</td></tr>';
      return;
    }
    renderRecent(recentOrders.slice(0, 5));
  } catch (e) {
    console.error(e);
    tbody.innerHTML = '<tr><td colspan="6" class="text-danger text-center">Không tải được dữ liệu</td></tr>';
  }
}

function renderRecent(list) {
  const tbody = document.getElementById('recentOrdersBody');
  if (!list || list.length === 0) {
    tbody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">Chưa có đơn hàng nào</td></tr>';
    return;
  }
  tbody.innerHTML = list
    .map(
      (o) => `
        <tr>
          <td><span class="fw-semibold">${o.orderCode || '-'}</span></td>
          <td>${utils.formatDate(o.createdAt || o.createdDate)}</td>
          <td><span class="badge ${utils.getStatusClass(o.status)}">${utils.getStatusText(o.status)}</span></td>
          <td>${utils.formatCurrency(o.shippingFee || o.totalFee || 0)}</td>
          <td>${o.isPaid ? '<span class="badge bg-success">Đã thanh toán</span>' : '<span class="badge bg-warning text-dark">Chưa thanh toán</span>'}</td>
          <td><a href="./tracking.html?order=${encodeURIComponent(o.orderCode || '')}" class="btn btn-sm btn-outline-primary"><i class="fas fa-location-dot"></i> Theo dõi</a></td>
        </tr>`
    )
    .join('');
}

function applyFilters() {
  const qEl = document.getElementById('globalSearch');
  const q = qEl ? (qEl.value || '').toLowerCase() : '';

  let filtered = recentOrders.slice();

  if (q) {
    filtered = filtered.filter(o => {
      const code = (o.orderCode || '').toLowerCase();
      const receiver = (o.customer?.fullName || o.receiverName || '').toLowerCase();
      const phone = (o.customer?.phoneNumber || o.customerPhone || '').toLowerCase();
      return code.includes(q) || receiver.includes(q) || phone.includes(q);
    });
  }

  if (selectedStatus && selectedStatus !== 'draft') {
    // numeric status
    filtered = filtered.filter(o => String(o.status) === String(selectedStatus));
  }

  // show top 5 matches
  renderRecent(filtered.slice(0, 5));
}

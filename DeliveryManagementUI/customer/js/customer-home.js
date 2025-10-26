// customer-home.js

window.addEventListener('DOMContentLoaded', async () => {
  await loadRecentMyOrders();
});

async function loadRecentMyOrders() {
  const tbody = document.getElementById('recentOrdersBody');
  try {
    const myOrders = await apiService.getMyOrders();
    if (!myOrders || myOrders.length === 0) {
      tbody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">Chưa có đơn hàng nào</td></tr>';
      return;
    }
    const top5 = myOrders.slice(0, 5);
    tbody.innerHTML = top5
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
  } catch (e) {
    console.error(e);
    tbody.innerHTML = '<tr><td colspan="6" class="text-danger text-center">Không tải được dữ liệu</td></tr>';
  }
}

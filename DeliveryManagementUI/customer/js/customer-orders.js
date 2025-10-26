// customer-orders.js

let allMyOrders = [];

window.addEventListener('DOMContentLoaded', async () => {
  // Show new order form if hash === #new
  if (window.location.hash === '#new') {
    document.getElementById('newOrderCard').style.display = 'block';
  }

  await Promise.all([loadMyOrders(), wireupCreateOrderForm()]);

  document.getElementById('searchInput').addEventListener('input', filterOrders);
  document.getElementById('statusFilter').addEventListener('change', filterOrders);
});

async function loadMyOrders() {
  const tbody = document.getElementById('ordersTableBody');
  try {
    const orders = await apiService.getMyOrders();
    allMyOrders = orders;
    renderOrders(orders);
  } catch (e) {
    console.error(e);
    tbody.innerHTML = '<tr><td colspan="6" class="text-danger text-center">Không tải được dữ liệu</td></tr>';
  }
}

async function wireupCreateOrderForm() {
  const form = document.getElementById('createOrderForm');
  if (!form) return;
  form.addEventListener('submit', async (e) => {
    e.preventDefault();
    const formData = Object.fromEntries(new FormData(form).entries());
    // Map frontend form field names to backend CreateOrderDto properties
    const data = {
      // Customer (receiver) info
      CustomerName: formData.receiverName || formData.senderName || '',
      CustomerPhone: formData.receiverPhone || formData.senderPhone || '',
      DeliveryAddress: formData.receiverAddress || formData.senderAddress || '',
      Ward: formData.receiverWard || '',
      District: formData.receiverDistrict || '',
      City: formData.receiverCity || formData.senderCity || '',

      // Goods
      ProductCode: formData.productCode || '',
      PackageType: +(formData.packageType || 0),
      Weight: +(formData.weight || 0),
      Size: formData.size || '',
      Distance: +(formData.distance || 0),

      // Flags & payment
      IsFragile: formData.isFragile === 'on' || false,
      IsValuable: formData.isValuable === 'on' || false,
      IsVehicle: formData.isVehicle === 'on' || false,
      CollectMoney: false,
      CollectionAmount: 0,
      PaymentMethod: 0,
      DeliveryType: +(formData.deliveryType || 0),
      Notes: formData.notes || ''
    };

    try {
      const res = await apiService.createOrder(data);
      utils.showToast('Tạo đơn thành công', 'success');
      form.reset();
      await loadMyOrders();
    } catch (err) {
      utils.showToast('Tạo đơn thất bại', 'danger');
      console.error(err);
    }
  });
}

function renderOrders(list) {
  const tbody = document.getElementById('ordersTableBody');
  if (!list || list.length === 0) {
    tbody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">Chưa có đơn hàng nào</td></tr>';
    return;
  }
  tbody.innerHTML = list
    .map(
      (o) => `
    <tr>
      <td>${o.orderCode || '-'}</td>
      <td>${utils.formatDate(o.createdAt || o.createdDate)}</td>
      <td><span class="badge ${utils.getStatusClass(o.status)}">${utils.getStatusText(o.status)}</span></td>
      <td>${utils.formatCurrency(o.shippingFee || o.totalFee || 0)}</td>
      <td>${o.isPaid ? '<span class="badge bg-success">Đã thanh toán</span>' : '<span class="badge bg-warning text-dark">Chưa thanh toán</span>'}</td>
      <td>
        <div class="btn-group btn-group-sm" role="group">
          <a href="./tracking.html?order=${encodeURIComponent(o.orderCode || '')}" class="btn btn-outline-primary">
            <i class="fas fa-location-dot"></i>
          </a>
          ${!o.isPaid ? `<button class="btn btn-outline-success" onclick="pay('${o.orderId}')"><i class="fas fa-credit-card"></i></button>`: ''}
          ${o.status === 3 && !o.confirmedReceived ? `<button class="btn btn-outline-secondary" onclick="confirmReceived('${o.orderId}')"><i class="fas fa-box-open"></i></button>`: ''}
          ${o.status === 3 ? `<button class="btn btn-outline-warning" onclick="openFeedback('${o.orderId}')"><i class="fas fa-star"></i></button>`: ''}
        </div>
      </td>
    </tr>`
    )
    .join('');
}

function filterOrders() {
  const q = document.getElementById('searchInput').value.toLowerCase();
  const status = document.getElementById('statusFilter').value;
  const filtered = allMyOrders.filter((o) => {
    const matchText = (o.orderCode || '').toLowerCase().includes(q) || (o.receiverName || '').toLowerCase().includes(q);
    const matchStatus = status === '' || String(o.status) === status;
    return matchText && matchStatus;
  });
  renderOrders(filtered);
}


async function pay(orderId) {
  if (!await utils.confirm('Xác nhận thanh toán cho đơn này?')) return;
  try {
    await apiService.payOrder(orderId);
    utils.showToast('Thanh toán thành công!', 'success');
    await loadMyOrders();
  } catch (e) {
    utils.showToast('Thanh toán thất bại!', 'danger');
  }
}

async function confirmReceived(orderId) {
  if (!await utils.confirm('Xác nhận đã nhận hàng?')) return;
  try {
    await apiService.confirmReceived(orderId);
    utils.showToast('Đã xác nhận nhận hàng!', 'success');
    await loadMyOrders();
  } catch (e) {
    utils.showToast('Xác nhận thất bại!', 'danger');
  }
}

function openFeedback(orderId) {
  document.getElementById('feedbackOrderId').value = orderId;
  document.getElementById('feedbackRating').value = 5;
  document.getElementById('feedbackComment').value = '';
  const modal = new bootstrap.Modal(document.getElementById('feedbackModal'));
  modal.show();
}

// Gửi feedback
document.addEventListener('DOMContentLoaded', () => {
  const form = document.getElementById('feedbackForm');
  if (form) {
    form.onsubmit = async (e) => {
      e.preventDefault();
      const orderId = document.getElementById('feedbackOrderId').value;
      const rating = +document.getElementById('feedbackRating').value;
      const comment = document.getElementById('feedbackComment').value;
      try {
        await apiService.postFeedback({ orderId, rating, comment });
        utils.showToast('Gửi đánh giá thành công!', 'success');
        bootstrap.Modal.getInstance(document.getElementById('feedbackModal')).hide();
        await loadMyOrders();
      } catch (err) {
        utils.showToast('Gửi đánh giá thất bại!', 'danger');
      }
    };
  }
});

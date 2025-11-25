// customer-orders.js

let allMyOrders = [];

function toggleNewOrderForm() {
  const card = document.getElementById('newOrderCard');
  if (card.style.display === 'none' || !card.style.display) {
    card.style.display = 'block';
    card.scrollIntoView({ behavior: 'smooth', block: 'start' });
  } else {
    card.style.display = 'none';
  }
}

function createOrderWithWeight(type) {
  const card = document.getElementById('newOrderCard');
  const form = document.getElementById('createOrderForm');
  const weightInput = form.querySelector('input[name="weight"]');
  
  // Show form
  card.style.display = 'block';
  card.scrollIntoView({ behavior: 'smooth', block: 'start' });
  
  // Set weight based on type
  if (type === 'light') {
    weightInput.value = '5'; // Default 5kg for light package
    weightInput.max = '19.9';
  } else if (type === 'heavy') {
    weightInput.value = '25'; // Default 25kg for heavy package
    weightInput.min = '20';
  }
}

window.addEventListener('DOMContentLoaded', async () => {
  // Show new order form if hash includes #new
  const hash = window.location.hash;
  if (hash === '#new' || hash === '#new-light' || hash === '#new-heavy') {
    const type = hash === '#new-heavy' ? 'heavy' : (hash === '#new-light' ? 'light' : null);
    if (type) {
      setTimeout(() => createOrderWithWeight(type), 100);
    } else {
      document.getElementById('newOrderCard').style.display = 'block';
    }
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
    <tr style="cursor: pointer;" onclick="viewOrderDetail('${o.orderId}')" title="Click để xem chi tiết">
      <td>${o.orderCode || '-'}</td>
      <td>${utils.formatDate(o.createdAt || o.createdDate)}</td>
      <td><span class="badge ${utils.getStatusClass(o.status)}">${utils.getStatusText(o.status)}</span></td>
      <td>${utils.formatCurrency(o.shippingFee || o.totalFee || 0)}</td>
      <td>${o.isPaid ? '<span class="badge bg-success">Đã thanh toán</span>' : '<span class="badge bg-warning text-dark">Chưa thanh toán</span>'}</td>
      <td onclick="event.stopPropagation()">
        <div class="btn-group btn-group-sm" role="group">
          <a href="./tracking.html?order=${encodeURIComponent(o.orderCode || '')}&orderId=${o.orderId}" class="btn btn-outline-primary" title="Theo dõi vị trí shipper">
            <i class="fas fa-location-dot"></i>
          </a>
          ${!o.isPaid ? `<button class="btn btn-outline-success" onclick="pay('${o.orderId}')" title="Thanh toán"><i class="fas fa-credit-card"></i></button>`: ''}
          ${o.status === 3 && !o.confirmedReceived ? `<button class="btn btn-outline-secondary" onclick="confirmReceived('${o.orderId}')" title="Xác nhận đã nhận"><i class="fas fa-box-open"></i></button>`: ''}
          ${o.status === 3 ? `<button class="btn btn-outline-warning" onclick="openFeedback('${o.orderId}')" title="Đánh giá"><i class="fas fa-star"></i></button>`: ''}
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

async function viewOrderDetail(orderId) {
  const modal = new bootstrap.Modal(document.getElementById('orderDetailModal'));
  const body = document.getElementById('orderDetailBody');
  
  modal.show();
  body.innerHTML = '<div class="text-center text-muted"><i class="fas fa-spinner fa-spin"></i> Đang tải...</div>';
  
  try {
    const order = await apiService.get(`/orders/${orderId}`);
    
    const statusClass = utils.getStatusClass(order.status);
    const statusText = utils.getStatusText(order.status);
    
    body.innerHTML = `
      <div class="row g-3">
        <div class="col-12">
          <div class="card bg-light">
            <div class="card-body">
              <h5 class="mb-3"><i class="fas fa-barcode"></i> ${order.orderCode || 'N/A'}</h5>
              <span class="badge ${statusClass} fs-6">${statusText}</span>
              ${order.isPaid ? '<span class="badge bg-success ms-2">Đã thanh toán</span>' : '<span class="badge bg-warning text-dark ms-2">Chưa thanh toán</span>'}
            </div>
          </div>
        </div>
        
        <div class="col-md-6">
          <h6 class="border-bottom pb-2"><i class="fas fa-user-circle text-primary"></i> Thông tin khách hàng</h6>
          <p class="mb-1"><strong>Tên:</strong> ${order.customer?.fullName || 'N/A'}</p>
          <p class="mb-1"><strong>SĐT:</strong> ${order.customer?.phoneNumber || 'N/A'}</p>
          <p class="mb-1"><strong>Địa chỉ:</strong> ${order.customer?.address || 'N/A'}</p>
          <p class="mb-1"><strong>Quận/Huyện:</strong> ${order.customer?.district || 'N/A'}</p>
          <p class="mb-1"><strong>Thành phố:</strong> ${order.customer?.city || 'N/A'}</p>
        </div>
        
        <div class="col-md-6">
          <h6 class="border-bottom pb-2"><i class="fas fa-truck text-success"></i> Thông tin giao hàng</h6>
          ${order.assignedStaff ? `
            <p class="mb-1"><strong>Shipper:</strong> ${order.assignedStaff.fullName}</p>
            <p class="mb-1"><strong>SĐT shipper:</strong> ${order.assignedStaff.phoneNumber}</p>
            <p class="mb-1"><strong>Phương tiện:</strong> ${order.assignedStaff.vehicleType || 'N/A'}</p>
          ` : '<p class="text-muted">Chưa có shipper</p>'}
          <p class="mb-1"><strong>Ngày tạo:</strong> ${utils.formatDate(order.createdDate)}</p>
          ${order.deliveryStartDate ? `<p class="mb-1"><strong>Bắt đầu giao:</strong> ${utils.formatDate(order.deliveryStartDate)}</p>` : ''}
          ${order.deliveredDate ? `<p class="mb-1"><strong>Đã giao:</strong> ${utils.formatDate(order.deliveredDate)}</p>` : ''}
        </div>
        
        <div class="col-12">
          <h6 class="border-bottom pb-2"><i class="fas fa-box text-warning"></i> Thông tin hàng hóa</h6>
          <div class="row">
            <div class="col-md-6">
              <p class="mb-1"><strong>Mã hàng:</strong> ${order.productCode || 'N/A'}</p>
              <p class="mb-1"><strong>Cân nặng:</strong> ${order.weight || 0} kg</p>
              <p class="mb-1"><strong>Kích thước:</strong> ${order.size || 'N/A'}</p>
              <p class="mb-1"><strong>Khoảng cách:</strong> ${order.distance || 0} km</p>
            </div>
            <div class="col-md-6">
              <p class="mb-1"><strong>Loại gói:</strong> ${order.packageType === 0 ? 'Gói nhỏ' : order.packageType === 4 ? 'Thùng' : 'Khác'}</p>
              <p class="mb-1"><strong>Loại giao:</strong> ${order.deliveryType === 0 ? 'Giao thường' : 'Giao nhanh'}</p>
              <p class="mb-1"><strong>Hàng dễ vỡ:</strong> ${order.isFragile ? 'Có' : 'Không'}</p>
              <p class="mb-1"><strong>Hàng trị giá:</strong> ${order.isValuable ? 'Có' : 'Không'}</p>
            </div>
          </div>
        </div>
        
        <div class="col-12">
          <h6 class="border-bottom pb-2"><i class="fas fa-dollar-sign text-info"></i> Thông tin thanh toán</h6>
          <p class="mb-1"><strong>Phí giao hàng:</strong> <span class="text-primary fs-5">${utils.formatCurrency(order.shippingFee || 0)}</span></p>
          <p class="mb-1"><strong>Phương thức:</strong> ${order.paymentMethod === 0 ? 'Tiền mặt' : order.paymentMethod === 1 ? 'Thẻ' : 'Chuyển khoản'}</p>
          ${order.paidAmount ? `<p class="mb-1"><strong>Đã thanh toán:</strong> ${utils.formatCurrency(order.paidAmount)}</p>` : ''}
          ${order.paymentTime ? `<p class="mb-1"><strong>Thời gian TT:</strong> ${utils.formatDate(order.paymentTime)}</p>` : ''}
        </div>
        
        ${order.notes ? `
        <div class="col-12">
          <h6 class="border-bottom pb-2"><i class="fas fa-sticky-note"></i> Ghi chú</h6>
          <p>${order.notes}</p>
        </div>
        ` : ''}
      </div>
    `;
  } catch (err) {
    console.error(err);
    body.innerHTML = '<div class="alert alert-danger">Không thể tải chi tiết đơn hàng!</div>';
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

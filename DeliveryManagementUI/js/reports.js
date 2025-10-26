// Require admin
if (!auth.requireAuth() || !auth.isAdmin()) {
  window.location.href = 'login.html';
}

let statusPieChart, ordersLineChart, staffBarChart;

async function loadReports() {
  try {
    const summary = await apiService.getReportsSummary();
    document.getElementById('reportTotal').textContent = summary.totalOrders || 0;
    document.getElementById('reportRevenue').textContent = 'Doanh thu: ' + (summary.totalRevenue || 0) + 'đ';

    const byStatus = summary.byStatus || [];
    const labels = byStatus.map(s => ['Chưa Nhận','Đã Nhận - Chưa Giao','Đang Giao','Đã Giao'][s.status] || s.status);
    const values = byStatus.map(s => s.count);
    renderStatusPie(labels, values);

    const days = await apiService.getOrdersByDay({ days: 30 });
    renderOrdersLine(days.map(d => d.date), days.map(d => d.count));

    const byStaff = await apiService.getOrdersByStaff();
    renderStaffBar(byStaff.map(s => s.staffName), byStaff.map(s => s.count));
      // delivery/package breakdowns
      const byDelivery = await apiService.getByDeliveryType();
      renderDeliveryType(byDelivery.map(d => d.typeName), byDelivery.map(d => d.count), byDelivery.map(d => d.revenue));

      const byPackage = await apiService.getByPackageType();
      renderPackageType(byPackage.map(d => d.typeName), byPackage.map(d => d.count), byPackage.map(d => d.revenue));
  } catch (e) {
    utils.showToast('Không tải được báo cáo', 'danger');
  }
}

function renderStatusPie(labels, data) {
  const ctx = document.getElementById('statusPie').getContext('2d');
  if (statusPieChart) statusPieChart.destroy();
  statusPieChart = new Chart(ctx, {
    type: 'pie',
    data: { labels, datasets: [{ data, backgroundColor: ['#ffc107','#0d6efd','#17a2b8','#198754'] }] }
  });
}

function renderOrdersLine(labels, data) {
  const ctx = document.getElementById('ordersLine').getContext('2d');
  if (ordersLineChart) ordersLineChart.destroy();
  ordersLineChart = new Chart(ctx, {
    type: 'line',
    data: { labels, datasets: [{ label: 'Số đơn', data, borderColor: '#0d6efd', backgroundColor: 'rgba(13,110,253,0.1)', fill: true }] },
    options: { scales: { x: { display: false } } }
  });
}

function renderStaffBar(labels, data) {
  const ctx = document.getElementById('staffBar').getContext('2d');
  if (staffBarChart) staffBarChart.destroy();
  staffBarChart = new Chart(ctx, {
    type: 'bar',
    data: { labels, datasets: [{ label: 'Số đơn', data, backgroundColor: '#198754' }] },
    options: { indexAxis: 'y' }
  });
}

function renderDeliveryType(labels, counts, revenues) {
  const ctx = document.getElementById('deliveryTypeChart').getContext('2d');
  // dual dataset: counts (bar) and revenue (line)
  if (window.deliveryTypeChart) window.deliveryTypeChart.destroy();
  window.deliveryTypeChart = new Chart(ctx, {
    data: {
      labels,
      datasets: [
        { type: 'bar', label: 'Số đơn', data: counts, backgroundColor: ['#0d6efd','#17a2b8'] },
        { type: 'line', label: 'Doanh thu (VND)', data: revenues, borderColor: '#198754', yAxisID: 'y1', fill: false }
      ]
    },
    options: {
      scales: {
        y: { type: 'linear', position: 'left', title: { display: true, text: 'Số đơn' } },
        y1: { type: 'linear', position: 'right', title: { display: true, text: 'Doanh thu (VND)' }, grid: { drawOnChartArea: false } }
      }
    }
  });
}

function renderPackageType(labels, counts, revenues) {
  const ctx = document.getElementById('packageTypeChart').getContext('2d');
  if (window.packageTypeChart) window.packageTypeChart.destroy();
  window.packageTypeChart = new Chart(ctx, {
    data: {
      labels,
      datasets: [
        { type: 'bar', label: 'Số đơn', data: counts, backgroundColor: '#0d6efd' },
        { type: 'line', label: 'Doanh thu (VND)', data: revenues, borderColor: '#dc3545', yAxisID: 'y1', fill: false }
      ]
    },
    options: {
      scales: {
        y: { type: 'linear', position: 'left', title: { display: true, text: 'Số đơn' } },
        y1: { type: 'linear', position: 'right', title: { display: true, text: 'Doanh thu (VND)' }, grid: { drawOnChartArea: false } }
      }
    }
  });
}

document.addEventListener('DOMContentLoaded', loadReports);

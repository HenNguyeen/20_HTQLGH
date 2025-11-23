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

// Export to Excel function
async function exportToExcel() {
  try {
    utils.showToast('Đang tạo file Excel...', 'info');
    
    // Fetch all data
    const [orders, summary, byStaff, byDelivery, byPackage] = await Promise.all([
      apiService.getAllOrders(),
      apiService.getReportsSummary(),
      apiService.getOrdersByStaff(),
      apiService.getByDeliveryType(),
      apiService.getByPackageType()
    ]);

    // Create workbook
    const wb = XLSX.utils.book_new();

    // Sheet 1: Tổng quan
    const summaryData = [
      ['BÁO CÁO DOANH THU TỔNG QUAN'],
      ['Ngày xuất báo cáo:', new Date().toLocaleString('vi-VN')],
      [],
      ['Tổng số đơn hàng:', summary.totalOrders || 0],
      ['Tổng doanh thu:', (summary.totalRevenue || 0).toLocaleString('vi-VN') + ' VNĐ'],
      [],
      ['PHÂN BỔ THEO TRẠNG THÁI'],
      ['Trạng thái', 'Số lượng', 'Tỷ lệ %']
    ];
    
    const statusNames = ['Chưa Nhận', 'Đã Nhận - Chưa Giao', 'Đang Giao', 'Đã Giao'];
    (summary.byStatus || []).forEach(s => {
      const percent = summary.totalOrders ? ((s.count / summary.totalOrders) * 100).toFixed(1) : 0;
      summaryData.push([statusNames[s.status] || s.status, s.count, percent + '%']);
    });

    const ws1 = XLSX.utils.aoa_to_sheet(summaryData);
    XLSX.utils.book_append_sheet(wb, ws1, 'Tổng Quan');

    // Sheet 2: Chi tiết đơn hàng
    const ordersData = [
      ['CHI TIẾT ĐơN HÀNG'],
      ['Mã Đơn', 'Khách Hàng', 'SĐT', 'Địa Chỉ', 'Loại Hàng', 'Loại Giao', 'Phí Giao', 'Trạng Thái', 'Nhân Viên', 'Ngày Tạo']
    ];
    
    orders.forEach(order => {
      ordersData.push([
        order.orderCode || '',
        order.customer?.fullName || '',
        order.customer?.phoneNumber || '',
        order.pickupAddress || '',
        order.packageType || '',
        order.deliveryType || '',
        order.deliveryFee || 0,
        statusNames[order.status] || order.status,
        order.deliveryStaff?.fullName || 'Chưa gán',
        order.createdAt ? new Date(order.createdAt).toLocaleString('vi-VN') : ''
      ]);
    });

    const ws2 = XLSX.utils.aoa_to_sheet(ordersData);
    XLSX.utils.book_append_sheet(wb, ws2, 'Chi Tiết Đơn Hàng');

    // Sheet 3: Theo nhân viên
    const staffData = [
      ['THỐNG KÊ THEO NHÂN VIÊN'],
      ['Nhân Viên', 'Số Đơn', 'Tổng Doanh Thu']
    ];
    
    byStaff.forEach(s => {
      staffData.push([
        s.staffName || 'Chưa gán',
        s.count || 0,
        (s.revenue || 0).toLocaleString('vi-VN') + ' VNĐ'
      ]);
    });

    const ws3 = XLSX.utils.aoa_to_sheet(staffData);
    XLSX.utils.book_append_sheet(wb, ws3, 'Theo Nhân Viên');

    // Sheet 4: Theo loại giao hàng
    const deliveryData = [
      ['THỐNG KÊ THEO LOẠI GIAO HÀNG'],
      ['Loại Giao', 'Số Đơn', 'Doanh Thu']
    ];
    
    byDelivery.forEach(d => {
      deliveryData.push([
        d.typeName || '',
        d.count || 0,
        (d.revenue || 0).toLocaleString('vi-VN') + ' VNĐ'
      ]);
    });

    const ws4 = XLSX.utils.aoa_to_sheet(deliveryData);
    XLSX.utils.book_append_sheet(wb, ws4, 'Theo Loại Giao');

    // Sheet 5: Theo loại hàng
    const packageData = [
      ['THỐNG KÊ THEO LOẠI HÀNG'],
      ['Loại Hàng', 'Số Đơn', 'Doanh Thu']
    ];
    
    byPackage.forEach(p => {
      packageData.push([
        p.typeName || '',
        p.count || 0,
        (p.revenue || 0).toLocaleString('vi-VN') + ' VNĐ'
      ]);
    });

    const ws5 = XLSX.utils.aoa_to_sheet(packageData);
    XLSX.utils.book_append_sheet(wb, ws5, 'Theo Loại Hàng');

    // Generate filename with timestamp
    const timestamp = new Date().toISOString().slice(0, 10);
    const filename = `BaoCaoDoanhThu_${timestamp}.xlsx`;

    // Download file
    XLSX.writeFile(wb, filename);
    
    utils.showToast('Xuất Excel thành công!', 'success');
  } catch (error) {
    console.error('Export error:', error);
    utils.showToast('Lỗi khi xuất Excel: ' + error.message, 'danger');
  }
}

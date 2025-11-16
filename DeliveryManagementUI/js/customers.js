// customers.js - Admin UI for managing customers

document.addEventListener('DOMContentLoaded', function() {
  const tableBody = document.querySelector('#customersTable tbody');
  const alertDiv = document.getElementById('customersAlert');
  const customerModal = new bootstrap.Modal(document.getElementById('customerModal'));
  const btnAdd = document.getElementById('btnAddCustomer');
  const saveBtn = document.getElementById('saveCustomerBtn');

  async function loadCustomers() {
    tableBody.innerHTML = `<tr><td colspan="8" class="text-center py-4"><div class="spinner-border" role="status"></div></td></tr>`;
    try {
      const customers = await apiService.getAllCustomers();
      if (!Array.isArray(customers) || customers.length === 0) {
        tableBody.innerHTML = `<tr><td colspan="8" class="text-center py-4">Chưa có khách hàng</td></tr>`;
        return;
      }
      tableBody.innerHTML = '';
      customers.forEach((c, idx) => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
          <td>${idx+1}</td>
          <td>${c.fullName || ''}</td>
          <td>${c.phoneNumber || ''}</td>
          <td>${c.address || ''}</td>
          <td>${c.ward || ''}</td>
          <td>${c.district || ''}</td>
          <td>${c.city || ''}</td>
          <td>
            <button class="btn btn-sm btn-outline-primary me-1" data-id="${c.customerId}" data-action="edit">Sửa</button>
            <button class="btn btn-sm btn-outline-danger" data-id="${c.customerId}" data-action="delete">Xóa</button>
          </td>
        `;
        tableBody.appendChild(tr);
      });
    } catch (err) {
      console.error(err);
      tableBody.innerHTML = `<tr><td colspan="8" class="text-center text-danger">Lỗi khi tải dữ liệu</td></tr>`;
    }
  }

  btnAdd.addEventListener('click', function(){
    document.getElementById('customerForm').reset();
    document.getElementById('customerId').value = '';
    customerModal.show();
  });

  tableBody.addEventListener('click', async function(e){
    const btn = e.target.closest('button');
    if (!btn) return;
    const id = btn.getAttribute('data-id');
    const action = btn.getAttribute('data-action');
    if (action === 'edit') {
      try {
        const c = await apiService.getCustomerById(id);
        document.getElementById('customerId').value = c.customerId;
        document.getElementById('fullName').value = c.fullName || '';
        document.getElementById('phoneNumber').value = c.phoneNumber || '';
        document.getElementById('address').value = c.address || '';
        document.getElementById('ward').value = c.ward || '';
        document.getElementById('district').value = c.district || '';
        document.getElementById('city').value = c.city || '';
        customerModal.show();
      } catch (err) {
        console.error(err);
        utils.showToast('Không thể lấy thông tin khách hàng', 'danger');
      }
    } else if (action === 'delete') {
      if (!confirm('Bạn có chắc muốn xóa khách hàng này?')) return;
      try {
        await apiService.deleteCustomer(id);
        utils.showToast('Xóa khách hàng thành công', 'success');
        loadCustomers();
      } catch (err) {
        console.error(err);
        utils.showToast('Xóa thất bại', 'danger');
      }
    }
  });

  saveBtn.addEventListener('click', async function(){
    const id = document.getElementById('customerId').value;
    const payload = {
      customerId: id ? parseInt(id) : 0,
      fullName: document.getElementById('fullName').value.trim(),
      phoneNumber: document.getElementById('phoneNumber').value.trim(),
      address: document.getElementById('address').value.trim(),
      ward: document.getElementById('ward').value.trim(),
      district: document.getElementById('district').value.trim(),
      city: document.getElementById('city').value.trim()
    };

    try {
      if (id) {
        await apiService.updateCustomer(id, payload);
        utils.showToast('Cập nhật khách hàng thành công', 'success');
      } else {
        await apiService.createCustomer(payload);
        utils.showToast('Tạo khách hàng thành công', 'success');
      }
      customerModal.hide();
      loadCustomers();
    } catch (err) {
      console.error(err);
      utils.showToast('Lưu thất bại', 'danger');
    }
  });

  // initial load
  loadCustomers();
});

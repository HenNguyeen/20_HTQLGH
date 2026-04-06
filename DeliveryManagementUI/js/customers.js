// customers.js - Admin UI for managing customers

document.addEventListener('DOMContentLoaded', function() {
  // Setup user info
  const currentUser = auth.getCurrentUser();
  if (currentUser) {
    document.querySelectorAll('.user-name').forEach(el => el.textContent = currentUser.fullName || currentUser.username || 'User');
    document.querySelectorAll('.user-role').forEach(el => el.textContent = currentUser.role || '');
  }
  
  const tableBody = document.querySelector('#customersTable tbody');
  const alertDiv = document.getElementById('customersAlert');
  const customerModal = new bootstrap.Modal(document.getElementById('customerModal'));
  const customerDetailModal = new bootstrap.Modal(document.getElementById('customerDetailModal'));
  const btnAdd = document.getElementById('btnAddCustomer');
  const saveBtn = document.getElementById('saveCustomerBtn');
  const searchInput = document.getElementById('customerSearchInput');
  const clearSearchBtn = document.getElementById('clearCustomerSearch');
  let allCustomers = [];

  function renderCustomers(customers) {
    if (!Array.isArray(customers) || customers.length === 0) {
      tableBody.innerHTML = `<tr><td colspan="8" class="text-center py-4">Không tìm thấy khách hàng phù hợp</td></tr>`;
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
          <button class="btn btn-sm btn-outline-info me-1" data-id="${c.customerId}" data-action="view">Chi tiết</button>
          <button class="btn btn-sm btn-outline-primary me-1" data-id="${c.customerId}" data-action="edit">Sửa</button>
          <button class="btn btn-sm btn-outline-danger" data-id="${c.customerId}" data-action="delete">Xóa</button>
        </td>
      `;
      tableBody.appendChild(tr);
    });
  }

  function setDetailValue(elementId, value) {
    const element = document.getElementById(elementId);
    if (element) {
      element.textContent = value ? String(value) : '-';
    }
  }

  function openCustomerDetail(customer) {
    setDetailValue('detailCustomerId', customer.customerId);
    setDetailValue('detailFullName', customer.fullName);
    setDetailValue('detailPhoneNumber', customer.phoneNumber);
    setDetailValue('detailEmail', customer.email);
    setDetailValue('detailAddress', customer.address);
    setDetailValue('detailWard', customer.ward);
    setDetailValue('detailDistrict', customer.district);
    setDetailValue('detailCity', customer.city);
    setDetailValue('detailAddressType', customer.addressType);
    setDetailValue('detailBankAccountNumber', customer.bankAccountNumber);
    setDetailValue('detailBankAccountName', customer.bankAccountName);
    setDetailValue('detailBankName', customer.bankName);
    setDetailValue('detailBankBranch', customer.bankBranch);
    setDetailValue('detailSettlementCycle', customer.settlementCycle);
    setDetailValue('detailTaxCode', customer.taxCode);
    setDetailValue('detailCreatedDate', customer.createdDate ? new Date(customer.createdDate).toLocaleDateString('vi-VN') : '-');
    customerDetailModal.show();
  }

  function normalizeText(value) {
    return String(value || '').toLowerCase();
  }

  function applySearch() {
    const keyword = normalizeText(searchInput?.value).trim();
    if (!keyword) {
      renderCustomers(allCustomers);
      return;
    }

    const filteredCustomers = allCustomers.filter(c => {
      const haystack = [
        c.fullName,
        c.phoneNumber,
        c.address,
        c.ward,
        c.district,
        c.city
      ].map(normalizeText).join(' ');

      return haystack.includes(keyword);
    });

    renderCustomers(filteredCustomers);
  }

  async function loadCustomers() {
    tableBody.innerHTML = `<tr><td colspan="8" class="text-center py-4"><div class="spinner-border" role="status"></div></td></tr>`;
    try {
      const customers = await apiService.getAllCustomers();
      if (!Array.isArray(customers) || customers.length === 0) {
        allCustomers = [];
        tableBody.innerHTML = `<tr><td colspan="8" class="text-center py-4">Chưa có khách hàng</td></tr>`;
        return;
      }
      allCustomers = customers;
      applySearch();
    } catch (err) {
      console.error(err);
      allCustomers = [];
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
    if (action === 'view') {
      try {
        const c = await apiService.getCustomerById(id);
        openCustomerDetail(c);
      } catch (err) {
        console.error(err);
        utils.showToast('Không thể lấy chi tiết khách hàng', 'danger');
      }
    } else if (action === 'edit') {
      try {
        const c = await apiService.getCustomerById(id);
        document.getElementById('customerId').value = c.customerId;
        document.getElementById('fullName').value = c.fullName || '';
        document.getElementById('phoneNumber').value = c.phoneNumber || '';
        document.getElementById('email').value = c.email || '';
        document.getElementById('address').value = c.address || '';
        document.getElementById('ward').value = c.ward || '';
        document.getElementById('district').value = c.district || '';
        document.getElementById('city').value = c.city || '';
        document.getElementById('addressType').value = c.addressType || 'Kho hàng';
        document.getElementById('bankAccountNumber').value = c.bankAccountNumber || '';
        document.getElementById('bankAccountName').value = c.bankAccountName || '';
        document.getElementById('bankName').value = c.bankName || '';
        document.getElementById('bankBranch').value = c.bankBranch || '';
        document.getElementById('settlementCycle').value = c.settlementCycle || '';
        document.getElementById('taxCode').value = c.taxCode || '';
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
      email: document.getElementById('email').value.trim(),
      address: document.getElementById('address').value.trim(),
      ward: document.getElementById('ward').value.trim(),
      district: document.getElementById('district').value.trim(),
      city: document.getElementById('city').value.trim(),
      addressType: document.getElementById('addressType').value || 'Kho hàng',
      bankAccountNumber: document.getElementById('bankAccountNumber').value.trim() || null,
      bankAccountName: document.getElementById('bankAccountName').value.trim() || null,
      bankName: document.getElementById('bankName').value.trim() || null,
      bankBranch: document.getElementById('bankBranch').value.trim() || null,
      settlementCycle: document.getElementById('settlementCycle').value || null,
      taxCode: document.getElementById('taxCode').value.trim() || null
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

  searchInput?.addEventListener('input', applySearch);
  clearSearchBtn?.addEventListener('click', function() {
    searchInput.value = '';
    applySearch();
    searchInput.focus();
  });

  // initial load
  loadCustomers();
});

// UI Customization: Theme + i18n
(function(){
  const STORAGE_THEME_KEY = 'uiTheme';
  const STORAGE_LANG_KEY = 'uiLang';
  const DEFAULT_LANG = 'vi';
  const SUPPORTED_LANGS = ['vi','en'];

  // Simple translation dictionary (extendable)
  const dictionary = {
    vi: {
      // Navigation & Menu
      dashboard: 'Dashboard',
      orders: 'Đơn Hàng',
      staff: 'Nhân Viên',
      customers: 'Khách Hàng',
      accounts: 'Quản Lý Tài Khoản',
      reports: 'Báo Cáo Doanh Thu',
      settings: 'Cài Đặt',
      logout: 'Đăng xuất',
      profile: 'Thông tin tài khoản',
      
      // Dashboard
      overviewDashboard: 'Dashboard Tổng Quan',
      totalOrders: 'Tổng Đơn Hàng',
      delivering: 'Đang Giao',
      completed: 'Đã Giao',
      availableStaff: 'Nhân Viên Rảnh',
      
      // Staff Management
      staffManagement: 'Quản Lý Nhân Viên Giao Hàng',
      addStaff: 'Thêm Nhân Viên',
      totalStaff: 'Tổng Nhân Viên',
      availableStatus: 'Đang Rảnh',
      deliveringStatus: 'Đang Giao Hàng',
      totalVehicles: 'Phương Tiện',
      search: 'Tìm kiếm',
      searchPlaceholder: 'Tên, SĐT, biển số...',
      status: 'Trạng thái',
      all: 'Tất cả',
      available: 'Đang rảnh',
      busy: 'Đang bận',
      vehicleType: 'Loại xe',
      motorcycle: 'Xe máy',
      smallTruck: 'Xe tải nhỏ',
      truck: 'Xe tải',
      reset: 'Đặt lại',
      loading: 'Đang tải...',
      addNewStaff: 'Thêm Nhân Viên Mới',
      fullName: 'Họ Tên',
      phoneNumber: 'Số Điện Thoại',
      accountInfo: 'Thông tin tài khoản đăng nhập',
      username: 'Tên đăng nhập (Username)',
      usernamePlaceholder: 'Ví dụ: shipper01',
      usernameHint: 'Tài khoản shipper sẽ dùng để đăng nhập hệ thống',
      email: 'Email',
      emailPlaceholder: 'Ví dụ: shipper01@company.com',
      defaultPassword: 'Mật khẩu mặc định',
      defaultPasswordInfo: 'Mật khẩu mặc định: 123456 (shipper có thể đổi sau khi đăng nhập)',
      vehicleInfo: 'Thông tin phương tiện',
      vehicleTypePlaceholder: '-- Chọn loại xe --',
      vehiclePlate: 'Biển Số Xe',
      vehiclePlatePlaceholder: 'Ví dụ: 59A1-12345',
      readyToWork: 'Sẵn sàng làm việc',
      cancel: 'Hủy',
      save: 'Lưu',
      addButton: 'Thêm Nhân Viên',
      staffOrders: 'Đơn Hàng Của Nhân Viên',
      close: 'Đóng',
      
      // Orders Management
      orderManagement: 'Quản Lý Đơn Hàng',
      createOrder: 'Tạo Đơn Hàng Mới',
      searchOrder: 'Mã đơn, khách hàng...',
      allStatus: 'Tất cả',
      notReceived: 'Chưa Nhận',
      receivedNotDelivered: 'Đã Nhận - Chưa Giao',
      deliveringOrder: 'Đang Giao',
      deliveredOrder: 'Đã Giao',
      deliveryType: 'Loại Giao Hàng',
      normalDelivery: 'Giao Thường',
      expressDelivery: 'Giao Nhanh',
      sortBy: 'Sắp xếp',
      newest: 'Mới nhất',
      oldest: 'Cũ nhất',
      feeHighest: 'Phí cao nhất',
      feeLowest: 'Phí thấp nhất',
      orderCode: 'Mã Đơn',
      customer: 'Khách Hàng',
      address: 'Địa Chỉ',
      itemType: 'Loại Hàng',
      deliveryFee: 'Phí Giao',
      deliveryTypeShort: 'Loại Giao',
      statusShort: 'Trạng Thái',
      staffAssigned: 'Nhân Viên',
      createdDate: 'Ngày Tạo',
      actions: 'Thao Tác',
      
      // Customer Management
      customerManagement: 'Quản Lý Khách Hàng',
      addCustomer: 'Thêm khách hàng',
      customerName: 'Họ Tên',
      phone: 'SĐT',
      customerAddress: 'Địa chỉ',
      ward: 'Phường',
      district: 'Quận',
      city: 'Thành phố',
      addEditCustomer: 'Thêm / Sửa Khách hàng',
      wardPlaceholder: 'Phường/Xã',
      districtPlaceholder: 'Quận/Huyện',
      cityPlaceholder: 'Thành phố',
      saveCustomer: 'Lưu',
      
      // Chat & Messages
      searchOrders: 'Tìm kiếm đơn hàng...',
      selectOrderToChat: 'Chọn đơn hàng để bắt đầu chat',
      selectOrderPrompt: 'Chọn đơn hàng từ danh sách bên trái để bắt đầu trò chuyện',
      enterMessage: 'Nhập tin nhắn...',
      searchConversations: 'Tìm kiếm...',
      
      // Accounts Management
      searchUsers: 'Tên, username, email, SĐT',
      password: 'Mật khẩu',
      passwordPlaceholder: 'Mặc định 123456',
      newPassword: 'Mật khẩu mới',
      newPasswordPlaceholder: 'Mặc định 123456 nếu để trống',
      
      // Reports
      exportExcel: 'Xuất Excel',
      revenueReport: 'Báo Cáo Tổng Quan',
      
      // Theme & Language
      themeLight: 'Chế độ Sáng',
      themeDark: 'Chế độ Tối',
      langLabel: 'Ngôn ngữ',
      
      // Home page keys
      companyName: 'GiaoHangTocDo',
      homePage: 'Trang chủ',
      services: 'Dịch vụ',
      about: 'Giới thiệu',
      support: 'Hỗ trợ',
      info: 'Thông tin',
      trackOrder: 'Tra cứu',
      loginRegister: 'Đăng ký / Đăng nhập',
      orderSearchPlaceholder: 'Nhập mã đơn hàng bạn cần tra cứu...',
      fastDelivery: 'Giao Hàng Nhanh',
      fastDeliveryDesc: 'Cam kết giao hàng nhanh chóng, đúng hẹn',
      securePackaging: 'Đóng Gói An Toàn',
      securePackagingDesc: 'Hàng hóa được đóng gói cẩn thận, đảm bảo an toàn',
      tracking247: 'Theo Dõi 24/7',
      tracking247Desc: 'Theo dõi vị trí đơn hàng mọi lúc, mọi nơi'
    },
    en: {
      // Navigation & Menu
      dashboard: 'Dashboard',
      orders: 'Orders',
      staff: 'Staff',
      customers: 'Customers',
      accounts: 'Account Mgmt',
      reports: 'Revenue Reports',
      settings: 'Settings',
      logout: 'Logout',
      profile: 'Profile',
      
      // Dashboard
      overviewDashboard: 'Dashboard Overview',
      totalOrders: 'Total Orders',
      delivering: 'Delivering',
      completed: 'Completed',
      availableStaff: 'Available Staff',
      
      // Staff Management
      staffManagement: 'Delivery Staff Management',
      addStaff: 'Add Staff',
      totalStaff: 'Total Staff',
      availableStatus: 'Available',
      deliveringStatus: 'Delivering',
      totalVehicles: 'Vehicles',
      search: 'Search',
      searchPlaceholder: 'Name, phone, plate...',
      status: 'Status',
      all: 'All',
      available: 'Available',
      busy: 'Busy',
      vehicleType: 'Vehicle Type',
      motorcycle: 'Motorcycle',
      smallTruck: 'Small Truck',
      truck: 'Truck',
      reset: 'Reset',
      loading: 'Loading...',
      addNewStaff: 'Add New Staff',
      fullName: 'Full Name',
      phoneNumber: 'Phone Number',
      accountInfo: 'Login Account Information',
      username: 'Username',
      usernamePlaceholder: 'e.g.: shipper01',
      usernameHint: 'Shipper account for system login',
      email: 'Email',
      emailPlaceholder: 'e.g.: shipper01@company.com',
      defaultPassword: 'Default Password',
      defaultPasswordInfo: 'Default password: 123456 (shipper can change after login)',
      vehicleInfo: 'Vehicle Information',
      vehicleTypePlaceholder: '-- Select vehicle type --',
      vehiclePlate: 'Vehicle Plate',
      vehiclePlatePlaceholder: 'e.g.: 59A1-12345',
      readyToWork: 'Ready to work',
      cancel: 'Cancel',
      save: 'Save',
      addButton: 'Add Staff',
      staffOrders: 'Staff Orders',
      close: 'Close',
      
      // Orders Management
      orderManagement: 'Order Management',
      createOrder: 'Create New Order',
      searchOrder: 'Order code, customer...',
      allStatus: 'All',
      notReceived: 'Not Received',
      receivedNotDelivered: 'Received - Not Delivered',
      deliveringOrder: 'Delivering',
      deliveredOrder: 'Delivered',
      deliveryType: 'Delivery Type',
      normalDelivery: 'Normal',
      expressDelivery: 'Express',
      sortBy: 'Sort By',
      newest: 'Newest',
      oldest: 'Oldest',
      feeHighest: 'Highest Fee',
      feeLowest: 'Lowest Fee',
      orderCode: 'Order Code',
      customer: 'Customer',
      address: 'Address',
      itemType: 'Item Type',
      deliveryFee: 'Delivery Fee',
      deliveryTypeShort: 'Type',
      statusShort: 'Status',
      staffAssigned: 'Staff',
      createdDate: 'Created Date',
      actions: 'Actions',
      
      // Customer Management
      customerManagement: 'Customer Management',
      addCustomer: 'Add Customer',
      customerName: 'Full Name',
      phone: 'Phone',
      customerAddress: 'Address',
      ward: 'Ward',
      district: 'District',
      city: 'City',
      addEditCustomer: 'Add / Edit Customer',
      wardPlaceholder: 'Ward',
      districtPlaceholder: 'District',
      cityPlaceholder: 'City',
      saveCustomer: 'Save',
      
      // Chat & Messages
      searchOrders: 'Search orders...',
      selectOrderToChat: 'Select an order to start chatting',
      selectOrderPrompt: 'Select an order from the list on the left to start conversation',
      enterMessage: 'Enter message...',
      searchConversations: 'Search...',
      
      // Accounts Management
      searchUsers: 'Name, username, email, phone',
      password: 'Password',
      passwordPlaceholder: 'Default 123456',
      newPassword: 'New Password',
      newPasswordPlaceholder: 'Default 123456 if empty',
      
      // Reports
      exportExcel: 'Export Excel',
      revenueReport: 'Revenue Overview',
      
      // Theme & Language
      themeLight: 'Light Mode',
      themeDark: 'Dark Mode',
      langLabel: 'Language',
      
      // Home page keys
      companyName: 'GiaoHangTocDo',
      homePage: 'Home',
      services: 'Services',
      about: 'About',
      support: 'Support',
      info: 'Information',
      trackOrder: 'Track',
      loginRegister: 'Sign Up / Login',
      orderSearchPlaceholder: 'Enter order code to track...',
      fastDelivery: 'Fast Delivery',
      fastDeliveryDesc: 'Commitment to fast, on-time delivery',
      securePackaging: 'Secure Packaging',
      securePackagingDesc: 'Goods carefully packaged for safety',
      tracking247: '24/7 Tracking',
      tracking247Desc: 'Track your order anytime, anywhere'
    }
  };

  function currentLang(){
    const stored = localStorage.getItem(STORAGE_LANG_KEY);
    return SUPPORTED_LANGS.includes(stored) ? stored : DEFAULT_LANG;
  }

  function setLanguage(lang){
    if(!SUPPORTED_LANGS.includes(lang)) return;
    localStorage.setItem(STORAGE_LANG_KEY, lang);
    translateDom();
  }

  function translateDom(){
    const lang = currentLang();
    const dict = dictionary[lang];
    document.querySelectorAll('[data-i18n]').forEach(el => {
      const key = el.getAttribute('data-i18n');
      if(dict[key]) {
        if(el.tagName === 'INPUT' && el.hasAttribute('placeholder')){
          el.placeholder = dict[key];
        } else {
          el.textContent = dict[key];
        }
      }
    });
  }

  function initLanguageControls(){
    const container = document.getElementById('langSwitcher');
    if(!container) return;
    container.innerHTML = `<select id="langSelect" class="form-select form-select-sm" style="width:auto">${SUPPORTED_LANGS.map(l => `<option value="${l}" ${l===currentLang()? 'selected':''}>${l.toUpperCase()}</option>`).join('')}</select>`;
    const sel = document.getElementById('langSelect');
    sel.addEventListener('change', e => setLanguage(e.target.value));
    translateDom();
  }

  // Theme
  function currentTheme(){
    return localStorage.getItem(STORAGE_THEME_KEY) || 'light';
  }

  function applyTheme(theme){
    const body = document.body;
    if(theme === 'dark') body.classList.add('theme-dark'); else body.classList.remove('theme-dark');
    localStorage.setItem(STORAGE_THEME_KEY, theme);
    updateThemeToggleLabel();
  }

  function toggleTheme(){
    const next = currentTheme() === 'dark' ? 'light' : 'dark';
    applyTheme(next);
  }

  function updateThemeToggleLabel(){
    const btn = document.getElementById('themeToggleBtn');
    if(!btn) return;
    const lang = currentLang();
    const isDark = currentTheme() === 'dark';
    btn.innerHTML = isDark ? `<i class="fas fa-sun"></i> ${dictionary[lang].themeLight}` : `<i class="fas fa-moon"></i> ${dictionary[lang].themeDark}`;
  }

  function initTheme(){
    applyTheme(currentTheme());
  }

  function init(){
    initTheme();
    initLanguageControls();
    updateThemeToggleLabel();
  }

  document.addEventListener('DOMContentLoaded', init);

  // Expose
  window.uiCustomization = { setLanguage, toggleTheme, translateDom };
})();

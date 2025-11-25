// UI Customization: Theme + i18n
(function(){
  const STORAGE_THEME_KEY = 'uiTheme';
  const STORAGE_LANG_KEY = 'uiLang';
  const DEFAULT_LANG = 'vi';
  const SUPPORTED_LANGS = ['vi','en'];

  // Simple translation dictionary (extendable)
  const dictionary = {
    vi: {
      dashboard: 'Dashboard',
      orders: 'Đơn Hàng',
      staff: 'Nhân Viên',
      customers: 'Khách Hàng',
      accounts: 'Quản Lý Tài Khoản',
      reports: 'Báo Cáo Doanh Thu',
      settings: 'Cài Đặt',
      logout: 'Đăng xuất',
      profile: 'Thông tin tài khoản',
      addStaff: 'Thêm Nhân Viên',
      totalOrders: 'Tổng Đơn Hàng',
      delivering: 'Đang Giao',
      completed: 'Đã Giao',
      availableStaff: 'Nhân Viên Rảnh',
      overviewDashboard: 'Dashboard Tổng Quan',
      exportExcel: 'Xuất Excel',
      revenueReport: 'Báo Cáo Tổng Quan',
      themeLight: 'Chế độ Sáng',
      themeDark: 'Chế độ Tối',
      langLabel: 'Ngôn ngữ',
      // Home page keys
      companyName: 'GiaoHangTocDo',
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
      dashboard: 'Dashboard',
      orders: 'Orders',
      staff: 'Staff',
      customers: 'Customers',
      accounts: 'Account Mgmt',
      reports: 'Revenue Reports',
      settings: 'Settings',
      logout: 'Logout',
      profile: 'Profile',
      addStaff: 'Add Staff',
      totalOrders: 'Total Orders',
      delivering: 'Delivering',
      completed: 'Completed',
      availableStaff: 'Available Staff',
      overviewDashboard: 'Dashboard Overview',
      exportExcel: 'Export Excel',
      revenueReport: 'Revenue Overview',
      themeLight: 'Light Mode',
      themeDark: 'Dark Mode',
      langLabel: 'Language',
      // Home page keys
      companyName: 'GiaoHangTocDo',
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

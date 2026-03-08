// Language Switcher Handler
(function() {
    'use strict';

    const translations = {
        vi: {
            // Common
            'search': 'Tìm kiếm',
            'logout': 'Đăng xuất',
            'profile': 'Hồ sơ',
            'settings': 'Cài đặt',
            'notifications': 'Thông báo',
            'mark_all_read': 'Đánh dấu tất cả đã đọc',
            'store': 'Cửa Hàng',
            'store_owner': 'Chủ cửa hàng',
            
            // Customer Menu
            'overview': 'Tổng quan',
            'manage_orders': 'Quản lý đơn hàng',
            'reports_live': 'Báo cáo - Live',
            'import_excel': 'Lên đơn Excel',
            'tracking': 'Theo dõi',
            
            // Common Actions
            'create_order': 'Tạo đơn hàng',
            'view_orders': 'Xem đơn',
            'track_order': 'Theo dõi đơn',
            'order_code': 'Mã đơn hàng',
            'loading': 'Đang tải...',
            'no_data': 'Không có dữ liệu',
            
            // Order status
            'draft': 'Đơn nháp',
            'pending': 'Chờ bàn giao',
            'received': 'Đã nhận - Chưa giao',
            'delivering': 'Đang giao',
            'delivered': 'Đã giao',
            
            // Dashboard
            'create_order_quick': 'Tạo đơn nhanh',
            'create_order_desc': 'Tạo đơn giao hàng mới chỉ với vài bước.',
            'my_orders': 'Đơn của tôi',
            'my_orders_desc': 'Xem danh sách đơn hàng đã tạo, trạng thái và thanh toán.',
            'track_orders': 'Theo dõi đơn',
            'track_orders_desc': 'Theo dõi hành trình giao hàng theo thời gian thực.',
            'no_orders_yet': 'Bạn chưa có đơn hàng nào!',
            'create_order_now': 'Tạo đơn hàng ngay để bắt đầu theo dõi',
            'create_now_btn': 'Tạo đơn hàng ngay!',
            'light_package': 'Hàng nhẹ <20kg',
            'heavy_package': 'Hàng nặng ≥20kg'
        },
        en: {
            // Common
            'search': 'Search',
            'logout': 'Logout',
            'profile': 'Profile',
            'settings': 'Settings',
            'notifications': 'Notifications',
            'mark_all_read': 'Mark all as read',
            'store': 'Store',
            'store_owner': 'Store Owner',
            
            // Customer Menu
            'overview': 'Overview',
            'manage_orders': 'Manage Orders',
            'reports_live': 'Reports - Live',
            'import_excel': 'Import Excel',
            'tracking': 'Tracking',
            
            // Common Actions
            'create_order': 'Create Order',
            'view_orders': 'View Orders',
            'track_order': 'Track Order',
            'order_code': 'Order Code',
            'loading': 'Loading...',
            'no_data': 'No data available',
            
            // Order status
            'draft': 'Draft',
            'pending': 'Pending Handover',
            'received': 'Received - Not Delivered',
            'delivering': 'In Delivery',
            'delivered': 'Delivered',
            
            // Dashboard
            'create_order_quick': 'Quick Order',
            'create_order_desc': 'Create a new delivery order in just a few steps.',
            'my_orders': 'My Orders',
            'my_orders_desc': 'View your created orders, status and payment.',
            'track_orders': 'Track Orders',
            'track_orders_desc': 'Track delivery progress in real-time.',
            'no_orders_yet': 'You have no orders yet!',
            'create_order_now': 'Create an order now to start tracking',
            'create_now_btn': 'Create Order Now!',
            'light_package': 'Light <20kg',
            'heavy_package': 'Heavy ≥20kg'
        }
    };

    // Get current language from localStorage or default to vi
    function getCurrentLanguage() {
        return localStorage.getItem('language') || 'vi';
    }

    // Apply language
    function applyLanguage(lang) {
        localStorage.setItem('language', lang);
        
        // Update all elements with data-translate attribute
        const elements = document.querySelectorAll('[data-translate]');
        elements.forEach(element => {
            const key = element.getAttribute('data-translate');
            if (translations[lang] && translations[lang][key]) {
                if (element.tagName === 'INPUT' && element.placeholder !== undefined) {
                    element.placeholder = translations[lang][key];
                } else {
                    element.textContent = translations[lang][key];
                }
            }
        });

        // Update language dropdown display
        updateLanguageDisplay(lang);
        
        // Trigger custom event for other scripts to react
        document.dispatchEvent(new CustomEvent('languageChanged', { detail: { language: lang } }));
    }

    // Update language dropdown display
    function updateLanguageDisplay(lang) {
        const langDisplay = document.getElementById('currentLanguage');
        if (langDisplay) {
            langDisplay.textContent = lang.toUpperCase();
        }
    }

    // Initialize language on page load
    function initLanguage() {
        const lang = getCurrentLanguage();
        applyLanguage(lang);

        // Setup language switch buttons
        const langButtons = document.querySelectorAll('[data-lang]');
        langButtons.forEach(button => {
            button.addEventListener('click', function(e) {
                e.preventDefault();
                const newLang = this.getAttribute('data-lang');
                applyLanguage(newLang);
            });
        });
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initLanguage);
    } else {
        initLanguage();
    }

    // Export for manual use
    window.languageSwitcher = {
        apply: applyLanguage,
        getCurrent: getCurrentLanguage,
        getTranslation: (key, lang) => {
            lang = lang || getCurrentLanguage();
            return translations[lang] && translations[lang][key] || key;
        }
    };
})();

// Settings Page JavaScript
(function() {
    'use strict';

    let currentUser = null;
    let settings = {};

    // Initialize
    document.addEventListener('DOMContentLoaded', init);

    function init() {
        // Check auth
        if (!auth.requireAuth()) {
            return;
        }

        currentUser = auth.getCurrentUser();
        
        // Update user info in navbar
        updateUserInfo();
        
        // Setup tab navigation
        setupTabs();
        
        // Load settings
        loadSettings();
        
        // Setup event listeners
        setupEventListeners();
    }

    function updateUserInfo() {
        if (currentUser) {
            document.querySelectorAll('.user-name').forEach(el => {
                el.textContent = currentUser.fullName || currentUser.username || 'User';
            });
            document.querySelectorAll('.user-role').forEach(el => {
                el.textContent = currentUser.role || '';
            });
        }
    }

    function setupTabs() {
        const tabLinks = document.querySelectorAll('[data-tab]');
        const tabContents = document.querySelectorAll('.tab-content');

        tabLinks.forEach(link => {
            link.addEventListener('click', function(e) {
                e.preventDefault();
                
                // Remove active class from all tabs
                tabLinks.forEach(l => l.classList.remove('active'));
                tabContents.forEach(c => c.classList.remove('active'));
                
                // Add active class to clicked tab
                this.classList.add('active');
                const tabId = this.getAttribute('data-tab');
                document.getElementById(tabId + '-tab').classList.add('active');
            });
        });
    }

    function loadSettings() {
        // Load from localStorage
        const savedSettings = localStorage.getItem('appSettings');
        if (savedSettings) {
            settings = JSON.parse(savedSettings);
            applySettings();
        } else {
            // Default settings
            settings = {
                general: {
                    displayName: currentUser?.fullName || '',
                    email: currentUser?.email || '',
                    phone: currentUser?.phoneNumber || '',
                    timezone: 'Asia/Ho_Chi_Minh'
                },
                appearance: {
                    theme: localStorage.getItem('theme') || 'light',
                    fontSize: 'medium',
                    compactMode: false
                },
                notifications: {
                    newOrder: true,
                    orderStatus: true,
                    orderDelivered: true,
                    systemUpdate: true,
                    maintenance: false,
                    inApp: true,
                    email: false,
                    sound: true
                },
                security: {
                    twoFactorEnabled: currentUser?.twoFactorEnabled || false,
                    rememberLogin: true
                },
                language: {
                    locale: localStorage.getItem('language') || 'vi',
                    dateFormat: 'dd/MM/yyyy',
                    currency: 'VND'
                }
            };
            applySettings();
        }
        
        // Load 2FA status from user
        check2FAStatus();
    }

    function applySettings() {
        // General
        if (settings.general) {
            document.getElementById('displayName').value = settings.general.displayName || '';
            document.getElementById('email').value = settings.general.email || '';
            document.getElementById('phone').value = settings.general.phone || '';
            document.getElementById('timezone').value = settings.general.timezone || 'Asia/Ho_Chi_Minh';
        }

        // Appearance
        if (settings.appearance) {
            const themeRadio = document.querySelector(`input[name="theme"][value="${settings.appearance.theme}"]`);
            if (themeRadio) themeRadio.checked = true;
            document.getElementById('fontSize').value = settings.appearance.fontSize || 'medium';
            document.getElementById('compactMode').checked = settings.appearance.compactMode || false;
        }

        // Notifications
        if (settings.notifications) {
            document.getElementById('notifyNewOrder').checked = settings.notifications.newOrder !== false;
            document.getElementById('notifyOrderStatus').checked = settings.notifications.orderStatus !== false;
            document.getElementById('notifyOrderDelivered').checked = settings.notifications.orderDelivered !== false;
            document.getElementById('notifySystemUpdate').checked = settings.notifications.systemUpdate !== false;
            document.getElementById('notifyMaintenance').checked = settings.notifications.maintenance || false;
            document.getElementById('notifyInApp').checked = settings.notifications.inApp !== false;
            document.getElementById('notifyEmail').checked = settings.notifications.email || false;
            document.getElementById('notifySound').checked = settings.notifications.sound !== false;
        }

        // Security
        if (settings.security) {
            document.getElementById('enable2FA').checked = settings.security.twoFactorEnabled || false;
            document.getElementById('rememberLogin').checked = settings.security.rememberLogin !== false;
        }

        // Language
        if (settings.language) {
            document.getElementById('languageSelect').value = settings.language.locale || 'vi';
            document.getElementById('dateFormat').value = settings.language.dateFormat || 'dd/MM/yyyy';
            document.getElementById('currencyFormat').value = settings.language.currency || 'VND';
        }

        // Show current session info
        showCurrentSession();
    }

    function setupEventListeners() {
        // Theme cards click
        document.querySelectorAll('.theme-card').forEach(card => {
            card.addEventListener('click', function() {
                const theme = this.getAttribute('data-theme');
                const radio = this.querySelector('input[type="radio"]');
                radio.checked = true;
                
                // Apply theme immediately
                if (theme === 'auto') {
                    const isDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
                    uiCustomization.setTheme(isDark ? 'dark' : 'light');
                } else {
                    uiCustomization.setTheme(theme);
                }
            });
        });

        // 2FA toggle
        document.getElementById('enable2FA').addEventListener('change', function() {
            if (this.checked) {
                window.location.href = 'settings-2fa.html';
            } else {
                disable2FA();
            }
        });
    }

    async function check2FAStatus() {
        try {
            const response = await fetch(`${apiService.API_BASE}/users/${currentUser.userId}`, {
                headers: apiService.getAuthHeaders()
            });
            
            if (response.ok) {
                const userData = await response.json();
                settings.security.twoFactorEnabled = userData.twoFactorEnabled || false;
                document.getElementById('enable2FA').checked = settings.security.twoFactorEnabled;
            }
        } catch (error) {
            console.error('Error checking 2FA status:', error);
        }
    }

    function showCurrentSession() {
        const loginTime = localStorage.getItem('loginTime');
        if (loginTime) {
            const date = new Date(parseInt(loginTime));
            document.getElementById('currentSession').textContent = 
                `Đăng nhập lúc ${date.toLocaleString('vi-VN')}`;
        }
    }

    // Save functions
    window.saveGeneralSettings = async function() {
        settings.general = {
            displayName: document.getElementById('displayName').value,
            email: document.getElementById('email').value,
            phone: document.getElementById('phone').value,
            timezone: document.getElementById('timezone').value
        };

        // Save to localStorage
        localStorage.setItem('appSettings', JSON.stringify(settings));

        // Update user profile on server
        try {
            await apiService.updateUserProfile({
                fullName: settings.general.displayName,
                email: settings.general.email,
                phoneNumber: settings.general.phone
            });

            utils.showToast('Đã lưu cài đặt chung', 'success');
        } catch (error) {
            console.error('Error saving general settings:', error);
            utils.showToast('Lưu cài đặt thất bại', 'danger');
        }
    };

    window.saveAppearanceSettings = function() {
        const selectedTheme = document.querySelector('input[name="theme"]:checked').value;
        
        settings.appearance = {
            theme: selectedTheme,
            fontSize: document.getElementById('fontSize').value,
            compactMode: document.getElementById('compactMode').checked
        };

        // Save to localStorage
        localStorage.setItem('appSettings', JSON.stringify(settings));

        // Apply theme
        if (selectedTheme === 'auto') {
            const isDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
            uiCustomization.setTheme(isDark ? 'dark' : 'light');
        } else {
            uiCustomization.setTheme(selectedTheme);
        }

        // Apply font size
        document.body.classList.remove('font-small', 'font-medium', 'font-large');
        document.body.classList.add(`font-${settings.appearance.fontSize}`);

        // Apply compact mode
        if (settings.appearance.compactMode) {
            document.body.classList.add('compact-mode');
        } else {
            document.body.classList.remove('compact-mode');
        }

        utils.showToast('Đã lưu cài đặt giao diện', 'success');
    };

    window.saveNotificationSettings = function() {
        settings.notifications = {
            newOrder: document.getElementById('notifyNewOrder').checked,
            orderStatus: document.getElementById('notifyOrderStatus').checked,
            orderDelivered: document.getElementById('notifyOrderDelivered').checked,
            systemUpdate: document.getElementById('notifySystemUpdate').checked,
            maintenance: document.getElementById('notifyMaintenance').checked,
            inApp: document.getElementById('notifyInApp').checked,
            email: document.getElementById('notifyEmail').checked,
            sound: document.getElementById('notifySound').checked
        };

        // Save to localStorage
        localStorage.setItem('appSettings', JSON.stringify(settings));

        utils.showToast('Đã lưu cài đặt thông báo', 'success');
    };

    window.saveSecuritySettings = function() {
        settings.security.rememberLogin = document.getElementById('rememberLogin').checked;

        // Save to localStorage
        localStorage.setItem('appSettings', JSON.stringify(settings));

        utils.showToast('Đã lưu cài đặt bảo mật', 'success');
    };

    window.saveLanguageSettings = function() {
        settings.language = {
            locale: document.getElementById('languageSelect').value,
            dateFormat: document.getElementById('dateFormat').value,
            currency: document.getElementById('currencyFormat').value
        };

        // Save to localStorage
        localStorage.setItem('appSettings', JSON.stringify(settings));
        localStorage.setItem('language', settings.language.locale);

        utils.showToast('Đã lưu cài đặt ngôn ngữ. Trang sẽ tải lại...', 'success');
        
        setTimeout(() => {
            location.reload();
        }, 1500);
    };

    window.changePassword = async function() {
        const currentPassword = document.getElementById('currentPassword').value;
        const newPassword = document.getElementById('newPassword').value;
        const confirmPassword = document.getElementById('confirmPassword').value;

        if (!currentPassword || !newPassword || !confirmPassword) {
            utils.showToast('Vui lòng điền đầy đủ thông tin', 'warning');
            return;
        }

        if (newPassword !== confirmPassword) {
            utils.showToast('Mật khẩu xác nhận không khớp', 'danger');
            return;
        }

        if (newPassword.length < 6) {
            utils.showToast('Mật khẩu phải có ít nhất 6 ký tự', 'warning');
            return;
        }

        try {
            const response = await fetch(`${apiService.API_BASE}/users/${currentUser.userId}/change-password`, {
                method: 'POST',
                headers: {
                    ...apiService.getAuthHeaders(),
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    currentPassword: currentPassword,
                    newPassword: newPassword
                })
            });

            if (response.ok) {
                utils.showToast('Đổi mật khẩu thành công', 'success');
                document.getElementById('currentPassword').value = '';
                document.getElementById('newPassword').value = '';
                document.getElementById('confirmPassword').value = '';
            } else {
                const error = await response.text();
                utils.showToast(error || 'Đổi mật khẩu thất bại', 'danger');
            }
        } catch (error) {
            console.error('Error changing password:', error);
            utils.showToast('Có lỗi xảy ra khi đổi mật khẩu', 'danger');
        }
    };

    async function disable2FA() {
        try {
            const response = await fetch(`${apiService.API_BASE}/auth/disable-2fa`, {
                method: 'POST',
                headers: apiService.getAuthHeaders()
            });

            if (response.ok) {
                settings.security.twoFactorEnabled = false;
                localStorage.setItem('appSettings', JSON.stringify(settings));
                utils.showToast('Đã tắt xác thực 2 yếu tố', 'success');
            } else {
                document.getElementById('enable2FA').checked = true;
                utils.showToast('Không thể tắt 2FA', 'danger');
            }
        } catch (error) {
            console.error('Error disabling 2FA:', error);
            document.getElementById('enable2FA').checked = true;
            utils.showToast('Có lỗi xảy ra', 'danger');
        }
    }

    window.logoutAllDevices = function() {
        if (confirm('Bạn có chắc muốn đăng xuất khỏi tất cả thiết bị? Bạn sẽ cần đăng nhập lại.')) {
            // Clear all auth data
            localStorage.clear();
            sessionStorage.clear();
            
            // Redirect to login
            window.location.href = 'login.html';
        }
    };

})();

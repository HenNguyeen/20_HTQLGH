// Customer Sidebar Component Loader
(function() {
    'use strict';

    // Load customer sidebar component
    async function loadCustomerSidebar() {
        try {
            const response = await fetch('../components/customer-sidebar.html');
            if (!response.ok) throw new Error('Failed to load customer sidebar');
            
            const sidebarHTML = await response.text();
            
            // Find app-shell or body and insert sidebar
            const appShell = document.querySelector('.app-shell');
            if (appShell) {
                // Remove existing sidebar if any
                const existingSidebar = appShell.querySelector('.sidebar');
                if (existingSidebar) {
                    existingSidebar.remove();
                }
                // Insert at beginning
                appShell.insertAdjacentHTML('afterbegin', sidebarHTML);
            } else {
                document.body.insertAdjacentHTML('afterbegin', sidebarHTML);
            }
            
            // Set active menu item
            setActiveMenuItem();
            
            // Setup logout handler
            setupLogoutHandler();
            
            // Setup sidebar collapse
            setupSidebarCollapse();
            
            // Restore sidebar state
            restoreSidebarState();
            
            // Update user info
            updateUserInfo();
            
            // Apply current language to sidebar
            applyLanguageToSidebar();
            
            return true;
        } catch (error) {
            console.error('Error loading customer sidebar:', error);
            return false;
        }
    }

    // Set active menu item based on current page
    function setActiveMenuItem() {
        const currentPage = window.location.pathname.split('/').pop() || 'index.html';
        const menuItems = document.querySelectorAll('.sidebar-nav .nav-link[data-page]');
        
        menuItems.forEach(item => {
            const page = item.getAttribute('data-page');
            if (page === currentPage) {
                item.classList.add('active');
            } else {
                item.classList.remove('active');
            }
        });
    }

    // Setup logout handler
    function setupLogoutHandler() {
        const logoutLink = document.getElementById('customerLogoutLink');
        if (logoutLink) {
            logoutLink.addEventListener('click', function(e) {
                e.preventDefault();
                if (confirm('Bạn có chắc chắn muốn đăng xuất?')) {
                    // Clear authentication
                    localStorage.removeItem('authToken');
                    localStorage.removeItem('currentUser');
                    sessionStorage.removeItem('authToken');
                    sessionStorage.removeItem('currentUser');
                    
                    // Redirect to login
                    window.location.href = '../login.html';
                }
            });
        }
    }

    // Setup sidebar collapse functionality
    function setupSidebarCollapse() {
        const collapseBtn = document.getElementById('customerSidebarCollapseBtn');
        const sidebar = document.getElementById('customerSidebar');
        
        if (collapseBtn && sidebar) {
            collapseBtn.addEventListener('click', function(e) {
                e.preventDefault();
                e.stopPropagation();
                toggleSidebarCollapse();
            });
        }
    }

    // Toggle sidebar collapse state
    function toggleSidebarCollapse() {
        const sidebar = document.getElementById('customerSidebar');
        
        if (sidebar) {
            sidebar.classList.toggle('collapsed');
            
            // Save state to localStorage
            const isCollapsed = sidebar.classList.contains('collapsed');
            localStorage.setItem('customerSidebarCollapsed', isCollapsed ? 'true' : 'false');
        }
    }

    // Restore sidebar state from localStorage
    function restoreSidebarState() {
        const isCollapsed = localStorage.getItem('customerSidebarCollapsed') === 'true';
        
        if (isCollapsed) {
            const sidebar = document.getElementById('customerSidebar');
            if (sidebar) {
                sidebar.classList.add('collapsed');
            }
        }
    }

    // Update user info from auth
    function updateUserInfo() {
        if (typeof auth !== 'undefined' && auth.getCurrentUser) {
            const user = auth.getCurrentUser();
            if (user) {
                const userNameElements = document.querySelectorAll('.sidebar .user-name');
                userNameElements.forEach(el => {
                    el.textContent = user.fullName || user.username || 'User';
                });
            }
        }
    }

    // Apply current language to sidebar after loading
    function applyLanguageToSidebar() {
        if (typeof languageSwitcher !== 'undefined' && languageSwitcher.apply) {
            const currentLang = languageSwitcher.getCurrent();
            languageSwitcher.apply(currentLang);
        }
    }

    // Initialize sidebar when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', loadCustomerSidebar);
    } else {
        loadCustomerSidebar();
    }

    // Export for manual use if needed
    window.customerSidebarLoader = {
        load: loadCustomerSidebar,
        setActive: setActiveMenuItem,
        toggle: toggleSidebarCollapse
    };
})();

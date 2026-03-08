// Sidebar Component Loader
(function() {
    'use strict';

    // Load sidebar component
    async function loadSidebar() {
        try {
            const response = await fetch('components/sidebar.html');
            if (!response.ok) throw new Error('Failed to load sidebar');
            
            const sidebarHTML = await response.text();
            
            // Insert sidebar at the beginning of body
            document.body.insertAdjacentHTML('afterbegin', sidebarHTML);
            
            // Set active menu item based on current page
            setActiveMenuItem();
            
            // Setup logout handler
            setupLogoutHandler();
            
            // Setup sidebar collapse functionality
            setupSidebarCollapse();
            
            // Restore sidebar state from localStorage
            restoreSidebarState();
            
            return true;
        } catch (error) {
            console.error('Error loading sidebar:', error);
            return false;
        }
    }

    // Set active menu item based on current page
    function setActiveMenuItem() {
        const currentPage = window.location.pathname.split('/').pop() || 'index.html';
        const menuItems = document.querySelectorAll('.sidebar-menu li[data-page]');
        
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
        const logoutLink = document.getElementById('logoutLink');
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
                    window.location.href = 'login.html';
                }
            });
        }
    }

    // Setup sidebar collapse functionality
    function setupSidebarCollapse() {
        const collapseBtn = document.getElementById('sidebarCollapseBtn');
        const sidebar = document.getElementById('sidebar');
        const mainContent = document.querySelector('.main-content');
        
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
        const sidebar = document.getElementById('sidebar');
        const mainContent = document.querySelector('.main-content');
        
        if (sidebar) {
            sidebar.classList.toggle('collapsed');
            
            // Save state to localStorage
            const isCollapsed = sidebar.classList.contains('collapsed');
            localStorage.setItem('sidebarCollapsed', isCollapsed ? 'true' : 'false');
            
            // Update main content margin
            if (mainContent) {
                if (isCollapsed) {
                    mainContent.style.marginLeft = '70px';
                } else {
                    mainContent.style.marginLeft = 'var(--sidebar-width)';
                }
            }
        }
    }

    // Restore sidebar state from localStorage
    function restoreSidebarState() {
        const isCollapsed = localStorage.getItem('sidebarCollapsed') === 'true';
        
        if (isCollapsed) {
            const sidebar = document.getElementById('sidebar');
            const mainContent = document.querySelector('.main-content');
            
            if (sidebar) {
                sidebar.classList.add('collapsed');
            }
            
            if (mainContent) {
                mainContent.style.marginLeft = '70px';
            }
        }
    }

    // Initialize sidebar when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', loadSidebar);
    } else {
        loadSidebar();
    }

    // Export for manual use if needed
    window.sidebarLoader = {
        load: loadSidebar,
        setActive: setActiveMenuItem,
        toggle: toggleSidebarCollapse
    };
})();

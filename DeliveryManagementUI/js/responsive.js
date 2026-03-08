// Responsive Helper - Enhanced Sidebar Toggle with Overlay
(function() {
    'use strict';

    // Create and add overlay element if it doesn't exist
    function createOverlay() {
        let overlay = document.getElementById('sidebarOverlay');
        if (!overlay) {
            overlay = document.createElement('div');
            overlay.id = 'sidebarOverlay';
            overlay.className = 'sidebar-overlay';
            document.body.appendChild(overlay);
        }
        return overlay;
    }

    // Initialize sidebar toggle
    function initSidebarToggle() {
        const sidebarToggle = document.getElementById('sidebarToggle');
        const sidebar = document.getElementById('sidebar');
        const overlay = createOverlay();
        
        if (!sidebarToggle || !sidebar) return;

        // Toggle sidebar on button click
        sidebarToggle.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            toggleSidebar();
        });

        // Close sidebar when clicking overlay
        overlay.addEventListener('click', function() {
            closeSidebar();
        });

        // Close sidebar on ESC key
        document.addEventListener('keydown', function(e) {
            if (e.key === 'Escape' && sidebar.classList.contains('active')) {
                closeSidebar();
            }
        });

        // Close sidebar when clicking links on mobile
        const sidebarLinks = sidebar.querySelectorAll('.sidebar-menu a');
        sidebarLinks.forEach(link => {
            link.addEventListener('click', function() {
                if (window.innerWidth <= 992) {
                    closeSidebar();
                }
            });
        });

        // Handle window resize
        let resizeTimer;
        window.addEventListener('resize', function() {
            clearTimeout(resizeTimer);
            resizeTimer = setTimeout(function() {
                if (window.innerWidth > 992) {
                    closeSidebar();
                }
            }, 250);
        });

        function toggleSidebar() {
            const isActive = sidebar.classList.toggle('active');
            overlay.classList.toggle('active', isActive);
            document.body.style.overflow = isActive ? 'hidden' : '';
        }

        function closeSidebar() {
            sidebar.classList.remove('active');
            overlay.classList.remove('active');
            document.body.style.overflow = '';
        }
    }

    // Initialize table responsive enhancements
    function initTableResponsive() {
        const tables = document.querySelectorAll('.table-responsive');
        tables.forEach(tableWrapper => {
            const table = tableWrapper.querySelector('table');
            if (!table) return;

            // Add touch scroll hint on mobile
            if (window.innerWidth <= 768) {
                if (!tableWrapper.querySelector('.scroll-hint')) {
                    const hint = document.createElement('div');
                    hint.className = 'scroll-hint text-muted text-center py-2';
                    hint.innerHTML = '<small><i class="fas fa-hand-point-right"></i> Vuốt sang để xem thêm</small>';
                    tableWrapper.insertBefore(hint, table);
                }
            }
        });
    }

    // Initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() {
            initSidebarToggle();
            initTableResponsive();
        });
    } else {
        initSidebarToggle();
        initTableResponsive();
    }

    // Export functions for use in other scripts
    window.responsiveHelper = {
        initSidebarToggle,
        initTableResponsive
    };
})();

/**
 * Notification Client - Tích hợp SignalR cho real-time notifications
 * 
 * Usage:
 * 1. Include SignalR library trong HTML:
 *    <script src="https://cdn.jsdelivr.net/npm/@microsoft/signalr@latest/dist/browser/signalr.min.js"></script>
 * 
 * 2. Include file này:
 *    <script src="js/notification-client.js"></script>
 * 
 * 3. Khởi tạo:
 *    const notificationClient = new NotificationClient();
 *    await notificationClient.start();
 */

class NotificationClient {
    constructor() {
        this.connection = null;
        this.unreadCount = 0;
        this.baseUrl = window.location.origin;
        this.apiBaseUrl = `${this.baseUrl}/api`;
        this.hubUrl = `${this.baseUrl}/notificationHub`;
    }

    /**
     * Khởi tạo và kết nối đến NotificationHub
     */
    async start() {
        const token = localStorage.getItem('authToken') || sessionStorage.getItem('authToken');
        
        if (!token) {
            console.warn('No auth token found. Notifications disabled.');
            return;
        }

        // Request browser notification permission
        await this.requestBrowserPermission();

        // Tạo connection
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(this.hubUrl, {
                accessTokenFactory: () => token
            })
            .withAutomaticReconnect() // Tự động reconnect
            .configureLogging(signalR.LogLevel.Information)
            .build();

        // Đăng ký các event handlers
        this.registerHandlers();

        // Bắt đầu connection
        try {
            await this.connection.start();
            console.log('✅ NotificationHub connected!');
            console.log('🔔 Browser notification permission:', Notification.permission);
            
            // Load unread count
            await this.loadUnreadCount();
            
            // Load notifications
            await this.loadNotifications();
        } catch (err) {
            console.error('❌ Error connecting to NotificationHub:', err);
        }

        // Reconnect handlers
        this.connection.onreconnecting(() => {
            console.log('🔄 NotificationHub reconnecting...');
        });

        this.connection.onreconnected(() => {
            console.log('✅ NotificationHub reconnected!');
            this.loadUnreadCount();
        });

        this.connection.onclose(() => {
            console.log('❌ NotificationHub disconnected');
        });
    }

    /**
     * Đăng ký handlers cho các events từ server
     */
    registerHandlers() {
        // Nhận thông báo mới
        this.connection.on('ReceiveNotification', (notification) => {
            console.log('🔔 New notification:', notification);
            this.onNotificationReceived(notification);
        });

        // Cập nhật badge count
        this.connection.on('UpdateUnreadCount', (count) => {
            this.unreadCount = count;
            this.updateBadge(count);
        });

        // Thông báo đã đọc
        this.connection.on('NotificationRead', (notificationId) => {
            this.onNotificationRead(notificationId);
        });

        // Tất cả đã đọc
        this.connection.on('AllNotificationsRead', () => {
            this.onAllNotificationsRead();
        });

        // Pong (for testing)
        this.connection.on('Pong', (time) => {
            console.log('🏓 Pong received at:', time);
        });
    }

    /**
     * Xử lý khi nhận thông báo mới
     */
    onNotificationReceived(notification) {
        // Hiển thị toast notification
        this.showToast(notification);
        
        // Cập nhật badge
        this.unreadCount++;
        this.updateBadge(this.unreadCount);
        
        // Thêm vào dropdown list
        this.prependNotificationToList(notification);
        
        // Play sound (optional)
        this.playNotificationSound();
    }

    /**
     * Hiển thị toast notification
     */
    showToast(notification) {
        // Sử dụng browser notification API nếu được phép
        if ('Notification' in window && Notification.permission === 'granted') {
            const browserNotif = new Notification(notification.title, {
                body: notification.message,
                icon: '/Image/logo.png',
                badge: '/Image/logo.png',
                tag: 'delivery-notification',
                requireInteraction: false
            });

            // Click handler for browser notification
            browserNotif.onclick = function() {
                window.focus();
                if (notification.actionUrl) {
                    window.location.href = notification.actionUrl;
                }
                browserNotif.close();
            };

            // Auto close after 10 seconds
            setTimeout(() => browserNotif.close(), 10000);
        }

        // LUÔN hiển thị custom toast (cả khi có browser notification)
        const toast = document.createElement('div');
        toast.className = 'notification-toast';
        toast.innerHTML = `
            <div class="toast-header">
                <strong>${notification.title}</strong>
                <small class="text-muted">${this.getTimeAgo(notification.createdAt || new Date())}</small>
            </div>
            <div class="toast-body">
                ${notification.message}
            </div>
        `;
        
        // Add click handler
        toast.style.cursor = 'pointer';
        toast.addEventListener('click', () => {
            if (notification.actionUrl) {
                window.location.href = notification.actionUrl;
            }
            toast.remove();
        });
        
        document.body.appendChild(toast);
        
        // Auto remove after 7 seconds
        setTimeout(() => {
            toast.classList.add('fade-out');
            setTimeout(() => toast.remove(), 500);
        }, 7000);
    }

    /**
     * Request browser notification permission
     */
    async requestBrowserPermission() {
        if ('Notification' in window && Notification.permission === 'default') {
            try {
                const permission = await Notification.requestPermission();
                console.log('📢 Notification permission:', permission);
                return permission === 'granted';
            } catch (error) {
                console.error('Error requesting notification permission:', error);
                return false;
            }
        }
        return Notification.permission === 'granted';
    }

    /**
     * Helper to get time ago (for toast)
     */
    getTimeAgo(dateTime) {
        const date = new Date(dateTime);
        const timeSpan = new Date() - date;
        const seconds = Math.floor(timeSpan / 1000);
        
        if (seconds < 60) return 'Vừa xong';
        if (seconds < 3600) return `${Math.floor(seconds / 60)} phút trước`;
        if (seconds < 86400) return `${Math.floor(seconds / 3600)} giờ trước`;
        return `${Math.floor(seconds / 86400)} ngày trước`;
    }

    /**
     * Cập nhật badge số thông báo chưa đọc
     */
    updateBadge(count) {
        const badge = document.getElementById('notification-badge');
        if (badge) {
            if (count > 0) {
                badge.textContent = count > 99 ? '99+' : count;
                badge.style.display = 'inline-block';
            } else {
                badge.style.display = 'none';
            }
        }
    }

    /**
     * Thêm notification vào đầu list
     */
    prependNotificationToList(notification) {
        const list = document.getElementById('notification-list');
        if (list) {
            const item = this.createNotificationItem(notification);
            list.insertBefore(item, list.firstChild);
        }
    }

    /**
     * Tạo HTML element cho notification
     */
    createNotificationItem(notification) {
        const div = document.createElement('div');
        div.className = `notification-item ${notification.isRead ? 'read' : 'unread'}`;
        div.dataset.id = notification.id;
        div.innerHTML = `
            <div class="notification-icon ${this.getIconClass(notification.type)}">
                ${this.getIcon(notification.type)}
            </div>
            <div class="notification-content">
                <h6>${notification.title}</h6>
                <p>${notification.message}</p>
                <small class="text-muted">${notification.timeAgo}</small>
            </div>
        `;
        
        // Click handler
        div.addEventListener('click', () => {
            this.markAsRead(notification.id);
            if (notification.actionUrl) {
                window.location.href = notification.actionUrl;
            }
        });
        
        return div;
    }

    /**
     * Load số lượng thông báo chưa đọc
     */
    async loadUnreadCount() {
        try {
            const token = localStorage.getItem('authToken') || sessionStorage.getItem('authToken');
            const response = await fetch(`${this.apiBaseUrl}/notifications/unread-count`, {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });
            
            if (response.ok) {
                const data = await response.json();
                this.unreadCount = data.count;
                this.updateBadge(this.unreadCount);
            }
        } catch (error) {
            console.error('Error loading unread count:', error);
        }
    }

    /**
     * Load danh sách thông báo
     */
    async loadNotifications(page = 1, pageSize = 20) {
        try {
            const token = localStorage.getItem('authToken') || sessionStorage.getItem('authToken');
            const response = await fetch(
                `${this.apiBaseUrl}/notifications?page=${page}&pageSize=${pageSize}`,
                {
                    headers: {
                        'Authorization': `Bearer ${token}`
                    }
                }
            );
            
            if (response.ok) {
                const notifications = await response.json();
                this.renderNotifications(notifications);
            }
        } catch (error) {
            console.error('Error loading notifications:', error);
        }
    }

    /**
     * Render danh sách notifications
     */
    renderNotifications(notifications) {
        const list = document.getElementById('notification-list');
        if (!list) return;
        
        list.innerHTML = '';
        
        if (notifications.length === 0) {
            list.innerHTML = '<div class="no-notifications">Không có thông báo</div>';
            return;
        }
        
        notifications.forEach(notification => {
            const item = this.createNotificationItem(notification);
            list.appendChild(item);
        });
    }

    /**
     * Đánh dấu thông báo đã đọc
     */
    async markAsRead(notificationId) {
        try {
            const token = localStorage.getItem('authToken') || sessionStorage.getItem('authToken');
            const response = await fetch(
                `${this.apiBaseUrl}/notifications/${notificationId}/read`,
                {
                    method: 'PUT',
                    headers: {
                        'Authorization': `Bearer ${token}`
                    }
                }
            );
            
            if (response.ok) {
                // Update UI
                const item = document.querySelector(`[data-id="${notificationId}"]`);
                if (item) {
                    item.classList.remove('unread');
                    item.classList.add('read');
                }
                
                // Update count
                this.unreadCount = Math.max(0, this.unreadCount - 1);
                this.updateBadge(this.unreadCount);
            }
        } catch (error) {
            console.error('Error marking notification as read:', error);
        }
    }

    /**
     * Đánh dấu tất cả đã đọc
     */
    async markAllAsRead() {
        try {
            const token = localStorage.getItem('authToken') || sessionStorage.getItem('authToken');
            const response = await fetch(`${this.apiBaseUrl}/notifications/read-all`, {
                method: 'PUT',
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });
            
            if (response.ok) {
                // Update UI
                document.querySelectorAll('.notification-item').forEach(item => {
                    item.classList.remove('unread');
                    item.classList.add('read');
                });
                
                this.unreadCount = 0;
                this.updateBadge(0);
            }
        } catch (error) {
            console.error('Error marking all as read:', error);
        }
    }

    /**
     * Handlers for events
     */
    onNotificationRead(notificationId) {
        const item = document.querySelector(`[data-id="${notificationId}"]`);
        if (item) {
            item.classList.remove('unread');
            item.classList.add('read');
        }
    }

    onAllNotificationsRead() {
        document.querySelectorAll('.notification-item').forEach(item => {
            item.classList.remove('unread');
            item.classList.add('read');
        });
        this.unreadCount = 0;
        this.updateBadge(0);
    }

    /**
     * Helper methods
     */
    getIconClass(type) {
        const icons = {
            1: 'icon-order',      // Order
            2: 'icon-chat',       // Chat
            3: 'icon-account',    // Account
            4: 'icon-feedback',   // Feedback
            5: 'icon-promotion',  // Promotion
            6: 'icon-system'      // System
        };
        return icons[type] || 'icon-default';
    }

    getIcon(type) {
        const icons = {
            1: '📦',  // Order
            2: '💬',  // Chat
            3: '👤',  // Account
            4: '⭐',  // Feedback
            5: '🎁',  // Promotion
            6: '⚙️'   // System
        };
        return icons[type] || '🔔';
    }

    playNotificationSound() {
        // Optional: play notification sound
        try {
            const audio = new Audio('/sounds/notification.mp3');
            audio.volume = 0.3;
            audio.play().catch(() => {});
        } catch (error) {
            // Ignore errors
        }
    }

    /**
     * Request browser notification permission
     */
    static async requestPermission() {
        if ('Notification' in window && Notification.permission === 'default') {
            const permission = await Notification.requestPermission();
            return permission === 'granted';
        }
        return Notification.permission === 'granted';
    }

    /**
     * Test ping-pong
     */
    async ping() {
        if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
            await this.connection.invoke('Ping');
        }
    }

    /**
     * Disconnect
     */
    async stop() {
        if (this.connection) {
            await this.connection.stop();
            console.log('NotificationHub stopped');
        }
    }
}

// Auto-initialize khi DOM ready
document.addEventListener('DOMContentLoaded', async () => {
    // Check if user is logged in
    const token = localStorage.getItem('authToken') || sessionStorage.getItem('authToken');
    if (token) {
        // Request notification permission
        await NotificationClient.requestPermission();
        
        // Initialize notification client
        window.notificationClient = new NotificationClient();
        await window.notificationClient.start();
    }
});

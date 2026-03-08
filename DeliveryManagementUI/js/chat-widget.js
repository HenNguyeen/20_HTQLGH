// Chat Widget - Floating Messenger-style Chat for Customers
(function() {
    'use strict';

    let connection = null;
    let currentUser = null;
    let unreadCount = 0;
    let isOpen = false;

    // Khởi tạo widget khi DOM ready
    document.addEventListener('DOMContentLoaded', initChatWidget);

    function initChatWidget() {
        // Kiểm tra auth
        if (!auth.isLoggedIn()) {
            return; // Không hiện widget nếu chưa đăng nhập
        }

        currentUser = auth.getCurrentUser();
        
        // Tạo HTML cho widget
        createWidgetHTML();
        
        // Setup event listeners
        setupEventListeners();
        
        // Kết nối SignalR
        initializeSignalR();
        
        // Load tin nhắn cũ
        loadMessages();
    }

    function createWidgetHTML() {
        const widgetHTML = `
            <div class="chat-widget">
                <!-- Floating Button -->
                <button class="chat-widget-button" id="chatWidgetBtn">
                    <i class="fas fa-comments"></i>
                    <span class="badge" id="chatWidgetBadge" style="display: none;">0</span>
                </button>

                <!-- Chat Popup -->
                <div class="chat-widget-popup" id="chatWidgetPopup">
                    <!-- Header -->
                    <div class="chat-widget-header">
                        <div class="chat-widget-header-left">
                            <div class="chat-widget-avatar">
                                <i class="fas fa-headset"></i>
                            </div>
                            <div class="chat-widget-title">
                                <h4>Hỗ Trợ Khách Hàng</h4>
                                <p>Chúng tôi luôn sẵn sàng hỗ trợ bạn</p>
                            </div>
                        </div>
                        <button class="chat-widget-close" id="chatWidgetClose">
                            <i class="fas fa-times"></i>
                        </button>
                    </div>

                    <!-- Body -->
                    <div class="chat-widget-body" id="chatWidgetBody">
                        <div class="chat-widget-welcome">
                            <i class="fas fa-comments"></i>
                            <h5>Xin chào!</h5>
                            <p>Bạn cần hỗ trợ gì không? Hãy nhắn tin cho chúng tôi.</p>
                        </div>
                    </div>

                    <!-- Footer -->
                    <div class="chat-widget-footer">
                        <div class="chat-widget-input-group">
                            <button class="chat-widget-attach-btn" onclick="chatWidget.openImageUpload()">
                                <i class="fas fa-image"></i>
                            </button>
                            <input type="file" id="chatWidgetImageInput" accept="image/*" style="display: none;">
                            <textarea 
                                class="chat-widget-input" 
                                id="chatWidgetInput" 
                                placeholder="Nhập tin nhắn..."
                                rows="1"
                            ></textarea>
                            <button class="chat-widget-send-btn" id="chatWidgetSend">
                                <i class="fas fa-paper-plane"></i>
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        `;

        document.body.insertAdjacentHTML('beforeend', widgetHTML);
    }

    function setupEventListeners() {
        // Toggle popup
        document.getElementById('chatWidgetBtn').addEventListener('click', togglePopup);
        document.getElementById('chatWidgetClose').addEventListener('click', closePopup);
        
        // Send message
        document.getElementById('chatWidgetSend').addEventListener('click', sendMessage);
        
        // Enter to send
        const input = document.getElementById('chatWidgetInput');
        input.addEventListener('keypress', (e) => {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                sendMessage();
            }
        });

        // Auto resize textarea
        input.addEventListener('input', function() {
            this.style.height = 'auto';
            this.style.height = Math.min(this.scrollHeight, 100) + 'px';
        });

        // Image upload
        document.getElementById('chatWidgetImageInput').addEventListener('change', handleImageUpload);
    }

    function togglePopup() {
        isOpen = !isOpen;
        const popup = document.getElementById('chatWidgetPopup');
        const button = document.getElementById('chatWidgetBtn');
        
        if (isOpen) {
            popup.classList.add('active');
            button.innerHTML = '<i class="fas fa-times"></i>';
            resetUnreadCount();
            scrollToBottom();
        } else {
            popup.classList.remove('active');
            button.innerHTML = '<i class="fas fa-comments"></i>';
        }
    }

    function closePopup() {
        isOpen = false;
        document.getElementById('chatWidgetPopup').classList.remove('active');
        document.getElementById('chatWidgetBtn').innerHTML = '<i class="fas fa-comments"></i>';
    }

    async function sendMessage() {
        const input = document.getElementById('chatWidgetInput');
        const content = input.value.trim();

        if (!content) return;

        try {
            // Gửi tin nhắn tới admin (không cần orderId, dùng general support)
            await apiService.sendChatMessage({
                orderId: null, // null = general support chat
                content: content,
                imageUrl: null
            });

            // Tin nhắn sẽ được hiển thị qua SignalR broadcast
            input.value = '';
            input.style.height = 'auto';
        } catch (error) {
            console.error('[Chat Widget] Error sending message:', error);
            alert('Không thể gửi tin nhắn. Vui lòng thử lại.');
        }
    }

    async function handleImageUpload(e) {
        const file = e.target.files[0];
        if (!file) return;

        console.log('[Chat Widget] File selected:', file.name, file.size, file.type);

        if (!file.type.startsWith('image/')) {
            alert('Vui lòng chọn file hình ảnh');
            return;
        }

        if (file.size > 5 * 1024 * 1024) {
            alert('Kích thước file không được vượt quá 5MB');
            return;
        }

        try {
            console.log('[Chat Widget] Uploading image...');
            const formData = new FormData();
            formData.append('file', file);

            const uploadResult = await apiService.uploadChatImage(formData);
            console.log('[Chat Widget] Upload result:', uploadResult);

            // Gửi tin nhắn với ảnh
            console.log('[Chat Widget] Sending message with image...');
            await apiService.sendChatMessage({
                orderId: null,
                content: '',
                imageUrl: uploadResult.url
            });
            console.log('[Chat Widget] Image message sent');

            // Tin nhắn sẽ được hiển thị qua SignalR broadcast
            e.target.value = '';
        } catch (error) {
            console.error('[Chat Widget] Error uploading image:', error);
            alert('Không thể tải ảnh lên: ' + error.message);
        }
    }

    async function loadMessages() {
        try {
            console.log('[Chat Widget] Loading messages...');
            const messages = await apiService.getMyMessages();
            console.log('[Chat Widget] Messages loaded:', messages);
            renderMessages(messages);
            scrollToBottom();
        } catch (error) {
            console.error('[Chat Widget] Error loading messages:', error);
        }
    }

    function renderMessages(messages) {
        const body = document.getElementById('chatWidgetBody');
        
        console.log('[Chat Widget] Rendering messages:', messages);
        
        if (!messages || messages.length === 0) {
            console.log('[Chat Widget] No messages to render');
            return;
        }

        // Clear welcome message
        body.innerHTML = '';

        messages.forEach(msg => {
            appendMessage(msg);
        });
    }

    function appendMessage(msg) {
        const body = document.getElementById('chatWidgetBody');
        
        // Xóa welcome message nếu còn
        const welcomeMsg = body.querySelector('.chat-widget-welcome');
        if (welcomeMsg) {
            welcomeMsg.remove();
        }
        
        // Backend gửi lowercase fields
        const senderId = msg.senderId;
        const imageUrl = msg.imageUrl;
        const content = msg.content;
        const createdAt = msg.createdAt;
        
        const isOwn = senderId == currentUser.userId;
        
        console.log('[Chat Widget] Appending message:', { senderId, content, createdAt, isOwn });
        
        // Get base URL without /api suffix for image paths
        const baseURL = apiService.baseURL.replace('/api', '');
        
        const messageHTML = `
            <div class="chat-widget-message ${isOwn ? 'sent' : 'received'}">
                <div class="chat-widget-message-content">
                    ${imageUrl ? `<img src="${baseURL}${imageUrl}" class="chat-widget-message-image">` : ''}
                    ${content ? `<div>${escapeHtml(content)}</div>` : ''}
                    <div class="chat-widget-message-time">${formatTime(createdAt)}</div>
                </div>
            </div>
        `;

        body.insertAdjacentHTML('beforeend', messageHTML);
        
        // Update unread count if popup is closed
        if (!isOpen && !isOwn) {
            unreadCount++;
            updateBadge();
        }

        scrollToBottom();
    }

    function resetUnreadCount() {
        unreadCount = 0;
        updateBadge();
    }

    function updateBadge() {
        const badge = document.getElementById('chatWidgetBadge');
        if (unreadCount > 0) {
            badge.textContent = unreadCount;
            badge.style.display = 'flex';
        } else {
            badge.style.display = 'none';
        }
    }

    async function initializeSignalR() {
        const token = auth.getToken();
        // Remove /api from baseURL for SignalR
        const signalRUrl = apiService.baseURL.replace('/api', '') + '/chatHub';
        
        connection = new signalR.HubConnectionBuilder()
            .withUrl(signalRUrl, {
                accessTokenFactory: () => token
            })
            .withAutomaticReconnect()
            .build();

        connection.on("ReceiveMessage", (message) => {
            console.log('[Chat Widget] Received SignalR message:', message);
            // Nhận tin nhắn general support: từ chính mình hoặc từ admin gửi cho mình
            const orderId = message.orderId;
            const senderId = message.senderId;
            const receiverId = message.receiverId;
            const senderRole = message.senderRole;
            
            console.log('[Chat Widget] Checking: orderId =', orderId, ', senderId =', senderId, ', receiverId =', receiverId, ', currentUserId =', currentUser.userId, ', senderRole =', senderRole);
            
            // Chỉ nhận tin nhắn nếu:
            // 1. Từ chính mình (senderId == currentUser.userId)
            // 2. Từ admin gửi cho mình (senderRole == "admin" && receiverId == currentUser.userId)
            // 3. Từ admin khi receiverId null (backward compatibility với tin nhắn cũ)
            if ((orderId === null || orderId == 0) && 
                (senderId == currentUser.userId || 
                 (senderRole === "admin" && (receiverId == currentUser.userId || receiverId == null)))) {
                console.log('[Chat Widget] ✅ Appending message');
                appendMessage(message);
            } else {
                console.log('[Chat Widget] ❌ Message filtered out - not for this user');
            }
        });

        try {
            await connection.start();
            console.log("[Chat Widget] SignalR Connected");
            await connection.invoke("JoinOrderChat", 0); // Join general support group
            console.log("[Chat Widget] Joined Order_0 group");
        } catch (err) {
            console.error("[Chat Widget] SignalR Connection Error:", err);
        }
    }

    function scrollToBottom() {
        const body = document.getElementById('chatWidgetBody');
        body.scrollTop = body.scrollHeight;
    }

    function formatTime(dateString) {
        if (!dateString) return '';
        
        try {
            const date = new Date(dateString);
            if (isNaN(date.getTime())) {
                console.warn('[Chat Widget] Invalid date:', dateString);
                return '';
            }
            return date.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
        } catch (error) {
            console.error('[Chat Widget] Error formatting time:', error);
            return '';
        }
    }

    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    // Export functions
    window.chatWidget = {
        openImageUpload: function() {
            document.getElementById('chatWidgetImageInput').click();
        }
    };
})();

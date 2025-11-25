// Admin Chat Widget - Quản lý tin nhắn từ khách hàng
(function() {
    'use strict';

    let connection = null;
    let currentUser = null;
    let unreadCount = 0;
    let isOpen = false;
    let conversations = []; // Danh sách khách hàng
    let activeConversation = null; // Khách hàng đang chat
    let messages = {}; // Lưu tin nhắn theo userId: {userId: [messages]}

    // Khởi tạo widget khi DOM ready
    document.addEventListener('DOMContentLoaded', initAdminChatWidget);

    function initAdminChatWidget() {
        // Kiểm tra auth và role
        if (!auth.isLoggedIn()) {
            return;
        }

        currentUser = auth.getCurrentUser();
        
        // Chỉ hiện cho admin
        if (currentUser.role !== 'admin') {
            return;
        }
        
        // Tạo HTML cho widget
        createWidgetHTML();
        
        // Setup event listeners
        setupEventListeners();
        
        // Kết nối SignalR
        initializeSignalR();
        
        // Load danh sách conversations
        loadConversations();
    }

    function createWidgetHTML() {
        const widgetHTML = `
            <div class="chat-widget admin-chat-widget">
                <!-- Floating Button -->
                <button class="chat-widget-button" id="adminChatWidgetBtn">
                    <i class="fas fa-comments"></i>
                    <span class="badge" id="adminChatWidgetBadge" style="display: none;">0</span>
                </button>

                <!-- Chat Popup -->
                <div class="chat-widget-popup admin-chat-popup" id="adminChatWidgetPopup">
                    <!-- Header -->
                    <div class="chat-widget-header">
                        <div class="chat-widget-header-left">
                            <div class="chat-widget-avatar">
                                <i class="fas fa-headset"></i>
                            </div>
                            <div class="chat-widget-title">
                                <h4 id="adminChatTitle">Tin Nhắn Khách Hàng</h4>
                                <p id="adminChatSubtitle">Chọn khách hàng để trả lời</p>
                            </div>
                        </div>
                        <button class="chat-widget-close" id="adminChatWidgetClose">
                            <i class="fas fa-times"></i>
                        </button>
                    </div>

                    <!-- Body -->
                    <div class="admin-chat-body" id="adminChatBody">
                        <!-- Conversations List -->
                        <div class="admin-conversations-list" id="adminConversationsList">
                            <div class="conversations-header">
                                <input type="text" class="form-control form-control-sm" id="adminSearchConversations" placeholder="Tìm khách hàng...">
                            </div>
                            <div class="conversations-items" id="adminConversationsItems">
                                <div class="text-center text-muted py-4">
                                    <i class="fas fa-spinner fa-spin"></i> Đang tải...
                                </div>
                            </div>
                        </div>

                        <!-- Chat Window -->
                        <div class="admin-chat-window" id="adminChatWindow">
                            <div class="admin-chat-empty">
                                <i class="fas fa-comments"></i>
                                <h5>Chọn khách hàng</h5>
                                <p>Chọn một khách hàng từ danh sách bên trái để xem và trả lời tin nhắn</p>
                            </div>
                        </div>
                    </div>

                    <!-- Footer (hiện khi đang chat) -->
                    <div class="chat-widget-footer" id="adminChatFooter" style="display: none;">
                        <div class="chat-widget-input-group">
                            <button class="chat-widget-attach-btn" onclick="adminChatWidget.openImageUpload()">
                                <i class="fas fa-image"></i>
                            </button>
                            <input type="file" id="adminChatImageInput" accept="image/*" style="display: none;">
                            <textarea 
                                class="chat-widget-input" 
                                id="adminChatInput" 
                                placeholder="Nhập tin nhắn..."
                                rows="1"
                            ></textarea>
                            <button class="chat-widget-send-btn" id="adminChatSend">
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
        document.getElementById('adminChatWidgetBtn').addEventListener('click', togglePopup);
        document.getElementById('adminChatWidgetClose').addEventListener('click', closePopup);
        
        // Send message
        document.getElementById('adminChatSend').addEventListener('click', sendMessage);
        
        // Enter to send
        const input = document.getElementById('adminChatInput');
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
        document.getElementById('adminChatImageInput').addEventListener('change', handleImageUpload);

        // Search conversations
        document.getElementById('adminSearchConversations').addEventListener('input', filterConversations);
    }

    function togglePopup() {
        isOpen = !isOpen;
        const popup = document.getElementById('adminChatWidgetPopup');
        const button = document.getElementById('adminChatWidgetBtn');
        
        if (isOpen) {
            popup.classList.add('active');
            button.innerHTML = '<i class="fas fa-times"></i>';
            resetUnreadCount();
        } else {
            popup.classList.remove('active');
            button.innerHTML = '<i class="fas fa-comments"></i>';
        }
    }

    function closePopup() {
        isOpen = false;
        document.getElementById('adminChatWidgetPopup').classList.remove('active');
        document.getElementById('adminChatWidgetBtn').innerHTML = '<i class="fas fa-comments"></i>';
    }

    async function loadConversations() {
        try {
            // Lấy danh sách tất cả user đã chat
            const response = await apiService.get('/chat/conversations');
            conversations = response;
            renderConversations();
            
            // Join tất cả các group
            conversations.forEach(conv => {
                if (connection && connection.state === 'Connected') {
                    connection.invoke('JoinOrderChat', 0); // Join general support group
                }
            });
        } catch (error) {
            console.error('Error loading conversations:', error);
            document.getElementById('adminConversationsItems').innerHTML = `
                <div class="text-center text-muted py-4">
                    <i class="fas fa-exclamation-circle"></i><br>
                    Không thể tải danh sách
                </div>
            `;
        }
    }

    function renderConversations() {
        const container = document.getElementById('adminConversationsItems');
        
        if (!conversations || conversations.length === 0) {
            container.innerHTML = `
                <div class="text-center text-muted py-4">
                    <i class="fas fa-inbox"></i><br>
                    Chưa có tin nhắn nào
                </div>
            `;
            return;
        }

        container.innerHTML = conversations.map(conv => {
            const lastMsg = conv.lastMessage ? 
                (conv.lastMessage.content || '[Hình ảnh]') : 
                'Chưa có tin nhắn';
            
            const time = conv.lastMessage ? 
                formatTimeAgo(conv.lastMessage.createdAt) : 
                '';

            return `
                <div class="conversation-item ${activeConversation?.userId === conv.userId ? 'active' : ''}" 
                     data-user-id="${conv.userId}"
                     onclick="adminChatWidget.selectConversation(${conv.userId})">
                    <div class="conversation-avatar">
                        <i class="fas fa-user-circle"></i>
                    </div>
                    <div class="conversation-info">
                        <div class="conversation-name">${escapeHtml(conv.userName)}</div>
                        <div class="conversation-last-msg">${escapeHtml(lastMsg.substring(0, 30))}</div>
                    </div>
                    <div class="conversation-meta">
                        <div class="conversation-time">${time}</div>
                        ${conv.unreadCount > 0 ? `<span class="conversation-badge">${conv.unreadCount}</span>` : ''}
                    </div>
                </div>
            `;
        }).join('');
    }

    function filterConversations(e) {
        const search = e.target.value.toLowerCase();
        const items = document.querySelectorAll('.conversation-item');
        
        items.forEach(item => {
            const name = item.querySelector('.conversation-name').textContent.toLowerCase();
            if (name.includes(search)) {
                item.style.display = '';
            } else {
                item.style.display = 'none';
            }
        });
    }

    async function selectConversation(userId) {
        try {
            // Tìm conversation
            activeConversation = conversations.find(c => c.userId === userId);
            if (!activeConversation) return;

            // Update UI
            document.querySelectorAll('.conversation-item').forEach(item => {
                item.classList.remove('active');
            });
            document.querySelector(`[data-user-id="${userId}"]`)?.classList.add('active');

            // Update header
            document.getElementById('adminChatTitle').textContent = activeConversation.userName;
            document.getElementById('adminChatSubtitle').textContent = 'Đang hoạt động';

            // Show footer
            document.getElementById('adminChatFooter').style.display = '';

            // Load messages
            await loadMessages(userId);

        } catch (error) {
            console.error('Error selecting conversation:', error);
        }
    }

    async function loadMessages(userId) {
        try {
            // API endpoint để lấy tin nhắn giữa admin và user này
            const msgs = await apiService.get(`/chat/user/${userId}`);
            messages[userId] = msgs;
            renderMessages(msgs);
        } catch (error) {
            console.error('Error loading messages:', error);
            const chatWindow = document.getElementById('adminChatWindow');
            chatWindow.innerHTML = `
                <div class="text-center text-muted py-4">
                    <i class="fas fa-exclamation-circle"></i><br>
                    Không thể tải tin nhắn
                </div>
            `;
        }
    }

    function renderMessages(msgs) {
        const chatWindow = document.getElementById('adminChatWindow');
        
        if (!msgs || msgs.length === 0) {
            chatWindow.innerHTML = `
                <div class="admin-chat-messages">
                    <div class="text-center text-muted py-4">
                        <i class="fas fa-comment-dots"></i><br>
                        Chưa có tin nhắn. Hãy bắt đầu trò chuyện!
                    </div>
                </div>
            `;
        } else {
            chatWindow.innerHTML = '<div class="admin-chat-messages" id="adminChatMessages"></div>';
            const container = document.getElementById('adminChatMessages');
            
            msgs.forEach(msg => {
                appendMessage(msg, container);
            });
        }

        scrollToBottom();
    }

    function appendMessage(msg, container = null) {
        if (!container) {
            container = document.getElementById('adminChatMessages');
            if (!container) {
                // Tạo container nếu chưa có
                const chatWindow = document.getElementById('adminChatWindow');
                chatWindow.innerHTML = '<div class="admin-chat-messages" id="adminChatMessages"></div>';
                container = document.getElementById('adminChatMessages');
            }
        }

        const isOwn = msg.senderId === currentUser.userId;
        
        const messageHTML = `
            <div class="chat-widget-message ${isOwn ? 'sent' : 'received'}">
                <div class="chat-widget-message-content">
                    ${msg.imageUrl ? `<img src="${apiService.baseURL}${msg.imageUrl}" class="chat-widget-message-image">` : ''}
                    ${msg.content ? `<div>${escapeHtml(msg.content)}</div>` : ''}
                    <div class="chat-widget-message-time">${formatTime(msg.createdAt)}</div>
                </div>
            </div>
        `;

        container.insertAdjacentHTML('beforeend', messageHTML);
        
        // Update unread count if popup is closed
        if (!isOpen && !isOwn) {
            unreadCount++;
            updateBadge();
        }

        scrollToBottom();
    }

    async function sendMessage() {
        if (!activeConversation) {
            alert('Vui lòng chọn khách hàng để gửi tin nhắn');
            return;
        }

        const input = document.getElementById('adminChatInput');
        const content = input.value.trim();

        if (!content) return;

        try {
            const result = await apiService.sendChatMessage({
                orderId: null,
                recipientId: activeConversation.userId, // Gửi cho user cụ thể
                content: content,
                imageUrl: null
            });

            // Hiển thị tin nhắn ngay lập tức
            appendMessage({
                id: result.id,
                orderId: null,
                senderId: currentUser.userId,
                senderRole: currentUser.role,
                content: content,
                imageUrl: null,
                createdAt: new Date().toISOString()
            });

            // Lưu vào messages cache
            if (!messages[activeConversation.userId]) {
                messages[activeConversation.userId] = [];
            }
            messages[activeConversation.userId].push({
                id: result.id,
                senderId: currentUser.userId,
                content: content,
                createdAt: new Date().toISOString()
            });

            input.value = '';
            input.style.height = 'auto';
        } catch (error) {
            console.error('Error sending message:', error);
            alert('Không thể gửi tin nhắn. Vui lòng thử lại.');
        }
    }

    async function handleImageUpload(e) {
        if (!activeConversation) {
            alert('Vui lòng chọn khách hàng để gửi hình ảnh');
            return;
        }

        const file = e.target.files[0];
        if (!file) return;

        if (!file.type.startsWith('image/')) {
            alert('Vui lòng chọn file hình ảnh');
            return;
        }

        if (file.size > 5 * 1024 * 1024) {
            alert('Kích thước file không được vượt quá 5MB');
            return;
        }

        try {
            const formData = new FormData();
            formData.append('file', file);

            const uploadResult = await apiService.uploadChatImage(formData);

            const messageResult = await apiService.sendChatMessage({
                orderId: null,
                recipientId: activeConversation.userId,
                content: '',
                imageUrl: uploadResult.url
            });

            appendMessage({
                id: messageResult.id,
                orderId: null,
                senderId: currentUser.userId,
                senderRole: currentUser.role,
                content: '',
                imageUrl: uploadResult.url,
                createdAt: new Date().toISOString()
            });

            e.target.value = '';
        } catch (error) {
            console.error('Error uploading image:', error);
            alert('Không thể tải ảnh lên');
        }
    }

    function resetUnreadCount() {
        unreadCount = 0;
        updateBadge();
    }

    function updateBadge() {
        const badge = document.getElementById('adminChatWidgetBadge');
        if (unreadCount > 0) {
            badge.textContent = unreadCount;
            badge.style.display = 'flex';
        } else {
            badge.style.display = 'none';
        }
    }

    async function initializeSignalR() {
        const token = auth.getToken();
        
        connection = new signalR.HubConnectionBuilder()
            .withUrl(`${apiService.baseURL}/chatHub`, {
                accessTokenFactory: () => token
            })
            .withAutomaticReconnect()
            .build();

        connection.on("ReceiveMessage", (message) => {
            if (message.orderId === null || message.orderId === 0) {
                // Tin nhắn general support
                if (message.senderId !== currentUser.userId) {
                    // Tin nhắn từ khách hàng
                    if (activeConversation && activeConversation.userId === message.senderId) {
                        // Đang chat với người này
                        appendMessage(message);
                    } else {
                        // Update conversation list
                        loadConversations();
                    }
                    
                    if (!isOpen) {
                        unreadCount++;
                        updateBadge();
                    }
                }
            }
        });

        try {
            await connection.start();
            console.log("Admin Chat Widget SignalR Connected");
            await connection.invoke("JoinOrderChat", 0); // Join general support group
        } catch (err) {
            console.error("SignalR Connection Error:", err);
        }
    }

    function scrollToBottom() {
        const container = document.getElementById('adminChatMessages');
        if (container) {
            container.scrollTop = container.scrollHeight;
        }
    }

    function formatTime(dateString) {
        const date = new Date(dateString);
        return date.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
    }

    function formatTimeAgo(dateString) {
        const date = new Date(dateString);
        const now = new Date();
        const diff = Math.floor((now - date) / 1000); // seconds

        if (diff < 60) return 'Vừa xong';
        if (diff < 3600) return `${Math.floor(diff / 60)} phút`;
        if (diff < 86400) return `${Math.floor(diff / 3600)} giờ`;
        return date.toLocaleDateString('vi-VN');
    }

    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    // Export functions
    window.adminChatWidget = {
        openImageUpload: function() {
            document.getElementById('adminChatImageInput').click();
        },
        selectConversation: selectConversation
    };
})();

// Messages Page - Admin Chat Management
(function() {
    'use strict';

    let connection = null;
    let currentUser = null;
    let conversations = [];
    let activeConversation = null;
    let messages = {};

    // Initialize
    document.addEventListener('DOMContentLoaded', init);

    async function init() {
        // Check auth
        if (!auth.isLoggedIn()) {
            window.location.href = 'login.html';
            return;
        }

        currentUser = auth.getCurrentUser();

        // Chỉ admin mới truy cập được
        if (currentUser.role !== 'admin') {
            alert('Bạn không có quyền truy cập trang này');
            window.location.href = 'index.html';
            return;
        }

        // Update user info
        document.getElementById('userName').textContent = currentUser.fullName || 'Admin';

        // Setup event listeners
        setupEventListeners();

        // Initialize SignalR
        await initializeSignalR();

        // Load conversations
        await loadConversations();
    }

    function setupEventListeners() {
        // Search conversations
        document.getElementById('searchConversations').addEventListener('input', filterConversations);
    }

    async function loadConversations() {
        try {
            console.log('Loading conversations...');
            const response = await apiService.get('/chat/conversations');
            console.log('Conversations response:', response);
            conversations = response;
            renderConversations();
        } catch (error) {
            console.error('Error loading conversations:', error);
            document.getElementById('conversationsList').innerHTML = `
                <div class="text-center py-5">
                    <i class="fas fa-exclamation-circle fa-2x text-danger"></i>
                    <p class="text-muted mt-2">Không thể tải danh sách tin nhắn</p>
                    <p class="text-muted small">${error.message || 'Unknown error'}</p>
                    <button class="btn btn-sm btn-primary" onclick="location.reload()">
                        <i class="fas fa-redo"></i> Thử lại
                    </button>
                </div>
            `;
        }
    }

    function renderConversations() {
        const container = document.getElementById('conversationsList');
        
        console.log('Rendering conversations:', conversations);
        
        if (!conversations || conversations.length === 0) {
            container.innerHTML = `
                <div class="text-center py-5">
                    <i class="fas fa-inbox fa-2x text-muted"></i>
                    <p class="text-muted mt-2">Chưa có tin nhắn nào</p>
                    <p class="text-muted small">Có ${conversations ? conversations.length : 0} conversations</p>
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
                     onclick="selectConversation(${conv.userId})">
                    <div class="conversation-avatar">
                        <i class="fas fa-user-circle"></i>
                    </div>
                    <div class="conversation-info">
                        <div class="conversation-name">${escapeHtml(conv.userName)}</div>
                        <div class="conversation-last-msg">${escapeHtml(lastMsg.substring(0, 40))}</div>
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
            item.style.display = name.includes(search) ? '' : 'none';
        });
    }

    window.selectConversation = async function(userId) {
        try {
            // Find conversation
            activeConversation = conversations.find(c => c.userId === userId);
            if (!activeConversation) return;

            // Update UI
            document.querySelectorAll('.conversation-item').forEach(item => {
                item.classList.remove('active');
            });
            document.querySelector(`[data-user-id="${userId}"]`)?.classList.add('active');

            // Show chat panel
            showChatPanel();

            // Load messages
            await loadMessages(userId);

        } catch (error) {
            console.error('Error selecting conversation:', error);
        }
    };

    function showChatPanel() {
        const chatPanel = document.getElementById('chatPanel');
        chatPanel.innerHTML = `
            <!-- Chat Header -->
            <div class="chat-header">
                <div class="chat-header-avatar">
                    <i class="fas fa-user-circle"></i>
                </div>
                <div class="chat-header-info">
                    <h5>${escapeHtml(activeConversation.userName)}</h5>
                    <p><i class="fas fa-circle text-success" style="font-size: 8px;"></i> Đang hoạt động</p>
                </div>
            </div>

            <!-- Chat Messages -->
            <div class="chat-messages" id="chatMessages">
                <div class="text-center py-5">
                    <i class="fas fa-spinner fa-spin fa-2x text-muted"></i>
                </div>
            </div>

            <!-- Chat Input -->
            <div class="chat-input">
                <div class="chat-input-group">
                    <button class="chat-input-btn" onclick="openImageUpload()">
                        <i class="fas fa-image"></i>
                    </button>
                    <input type="file" id="chatImageInput" accept="image/*" style="display: none;">
                    <textarea 
                        class="chat-input-field" 
                        id="chatInputField" 
                        placeholder="Nhập tin nhắn..."
                        rows="1"
                    ></textarea>
                    <button class="chat-input-btn send-btn" onclick="sendMessage()">
                        <i class="fas fa-paper-plane"></i>
                    </button>
                </div>
            </div>
        `;

        // Setup input listeners
        const input = document.getElementById('chatInputField');
        input.addEventListener('keypress', (e) => {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                sendMessage();
            }
        });

        input.addEventListener('input', function() {
            this.style.height = 'auto';
            this.style.height = Math.min(this.scrollHeight, 100) + 'px';
        });

        document.getElementById('chatImageInput').addEventListener('change', handleImageUpload);
    }

    async function loadMessages(userId) {
        try {
            const msgs = await apiService.get(`/chat/user/${userId}`);
            messages[userId] = msgs;
            renderMessages(msgs);
        } catch (error) {
            console.error('Error loading messages:', error);
            document.getElementById('chatMessages').innerHTML = `
                <div class="text-center py-5">
                    <i class="fas fa-exclamation-circle fa-2x text-danger"></i>
                    <p class="text-muted mt-2">Không thể tải tin nhắn</p>
                </div>
            `;
        }
    }

    function renderMessages(msgs) {
        const container = document.getElementById('chatMessages');
        
        if (!msgs || msgs.length === 0) {
            container.innerHTML = `
                <div class="text-center py-5">
                    <i class="fas fa-comment-dots fa-2x text-muted"></i>
                    <p class="text-muted mt-2">Chưa có tin nhắn. Hãy bắt đầu trò chuyện!</p>
                </div>
            `;
            return;
        }

        container.innerHTML = msgs.map(msg => createMessageHTML(msg)).join('');
        scrollToBottom();
    }

    function createMessageHTML(msg) {
        const isOwn = msg.senderId === currentUser.userId;
        // Get base URL without /api suffix for image paths
        const baseURL = apiService.baseURL.replace('/api', '');
        
        return `
            <div class="message ${isOwn ? 'sent' : 'received'}">
                <div class="message-content">
                    ${msg.imageUrl ? `<img src="${baseURL}${msg.imageUrl}" class="message-image" onclick="window.open('${baseURL}${msg.imageUrl}', '_blank')">` : ''}
                    ${msg.content ? `<div>${escapeHtml(msg.content)}</div>` : ''}
                    <div class="message-time">${formatTime(msg.createdAt)}</div>
                </div>
            </div>
        `;
    }

    function appendMessage(msg) {
        const container = document.getElementById('chatMessages');
        if (!container) return;

        // Remove empty state if exists
        const emptyState = container.querySelector('.text-center');
        if (emptyState) {
            container.innerHTML = '';
        }

        container.insertAdjacentHTML('beforeend', createMessageHTML(msg));
        scrollToBottom();
    }

    window.sendMessage = async function() {
        if (!activeConversation) {
            alert('Vui lòng chọn khách hàng để gửi tin nhắn');
            return;
        }

        const input = document.getElementById('chatInputField');
        const content = input.value.trim();

        if (!content) return;

        try {
            await apiService.sendChatMessage({
                orderId: null,
                content: content,
                imageUrl: null
            });

            // Tin nhắn sẽ được hiển thị qua SignalR broadcast
            input.value = '';
            input.style.height = 'auto';
        } catch (error) {
            console.error('[Messages] Error sending message:', error);
            alert('Không thể gửi tin nhắn. Vui lòng thử lại.');
        }
    };

    window.openImageUpload = function() {
        document.getElementById('chatImageInput').click();
    };

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
            console.log('[Messages] Uploading image:', file.name, file.size);
            const formData = new FormData();
            formData.append('file', file);

            const uploadResult = await apiService.uploadChatImage(formData);
            console.log('[Messages] Upload result:', uploadResult);

            await apiService.sendChatMessage({
                orderId: null,
                content: '',
                imageUrl: uploadResult.url
            });
            console.log('[Messages] Image message sent');

            // Tin nhắn sẽ được hiển thị qua SignalR broadcast
            e.target.value = '';
        } catch (error) {
            console.error('[Messages] Error uploading image:', error);
            alert('Không thể tải ảnh lên: ' + error.message);
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
            console.log('[Messages] Received SignalR message:', message);
            
            const orderId = message.orderId;
            const senderId = message.senderId;
            
            if (orderId === null || orderId == 0) {
                // Tin nhắn general support
                if (activeConversation) {
                    console.log('[Messages] Checking: senderId =', senderId, '(type:', typeof senderId + '), currentUserId =', currentUser.userId, '(type:', typeof currentUser.userId + '), activeConvUserId =', activeConversation.userId, '(type:', typeof activeConversation.userId + ')');
                    // Đang mở chat với ai đó
                    if (senderId == currentUser.userId || senderId == activeConversation.userId) {
                        // Tin nhắn từ chính admin hoặc từ người đang chat
                        console.log('[Messages] Appending message');
                        appendMessage(message);
                    } else {
                        console.log('[Messages] Message filtered out - senderId does not match');
                    }
                } else {
                    console.log('[Messages] No active conversation - updating list');
                    // Chưa mở chat, chỉ update conversation list
                    loadConversations();
                }
            }
        });

        try {
            await connection.start();
            console.log("[Messages] SignalR Connected");
            await connection.invoke("JoinOrderChat", 0); // Join general support group
            console.log("[Messages] Joined Order_0 group");
        } catch (err) {
            console.error("[Messages] SignalR Connection Error:", err);
        }
    }

    function scrollToBottom() {
        const container = document.getElementById('chatMessages');
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
        const diff = Math.floor((now - date) / 1000);

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
})();

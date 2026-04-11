// Chat với SignalR Real-time
let connection = null;
let currentOrderId = null;
let currentConversationUserId = null; // Cho admin chat
let currentUser = null;
let uploadedImageUrl = null;
let isAdminChat = false; // Biến theo dõi xem có phải admin chat không

// Khởi tạo
document.addEventListener('DOMContentLoaded', async () => {
    checkAuth();
    await loadConversationsOrOrders();
    initializeSignalR();
    setupEventListeners();
});

function checkAuth() {
    if (!auth.isLoggedIn()) {
        auth.logout();
        return;
    }

    const userInfo = auth.getCurrentUser();
    currentUser = {
        userId: userInfo.userId,
        fullName: userInfo.fullName,
        role: userInfo.role
    };
    
    // Check xem có phải admin không
    isAdminChat = userInfo.role === 'admin';
    
    document.getElementById('navbarUserName').textContent = userInfo.fullName || 'User';
}

function logout() {
    auth.logout();
}

// Khởi tạo SignalR Connection
async function initializeSignalR() {
    const token = auth.getToken();
    
    connection = new signalR.HubConnectionBuilder()
        .withUrl(`${apiService.baseURL}/chatHub`, {
            accessTokenFactory: () => token
        })
        .withAutomaticReconnect()
        .build();

    // Lắng nghe tin nhắn mới
    connection.on("ReceiveMessage", (message) => {
        // Xử lý tin nhắn cho order chat
        if (message.OrderId === currentOrderId && currentOrderId !== null) {
            appendMessage(message);
            scrollToBottom();
        }
        // Xử lý tin nhắn cho general support (admin chat với khách hàng)
        else if (message.OrderId === 0 || message.OrderId === null) {
            if (currentConversationUserId && (message.SenderId === currentConversationUserId || message.ReceiverId === currentConversationUserId)) {
                appendMessage(message);
                scrollToBottom();
            }
        }
        updateOrderLastMessage(message.OrderId, message);
    });

    // Lắng nghe typing indicator
    connection.on("UserTyping", (userName) => {
        showTypingIndicator(userName);
    });

    connection.on("UserJoined", (message) => {
        console.log(message);
    });

    connection.on("UserLeft", (message) => {
        console.log(message);
    });

    try {
        await connection.start();
        console.log("SignalR Connected");
        
        // Admin tự động join vào Order_0 group (general support)
        if (isAdminChat) {
            try {
                await connection.invoke("JoinOrderChat", 0);
                console.log("Admin joined general support group");
            } catch (err) {
                console.error('Error admin joining general support:', err);
            }
        }
    } catch (err) {
        console.error("SignalR Connection Error: ", err);
        setTimeout(initializeSignalR, 5000);
    }

    connection.onreconnecting((error) => {
        console.log("SignalR Reconnecting...", error);
    });

    connection.onreconnected((connectionId) => {
        console.log("SignalR Reconnected", connectionId);
        if (currentOrderId) {
            joinOrderChat(currentOrderId);
        }
    });

    connection.onclose((error) => {
        console.log("SignalR Connection Closed", error);
    });
}

// Load danh sách đơn hàng hoặc conversations (dựa trên role)
async function loadConversationsOrOrders() {
    try {
        if (isAdminChat) {
            // Admin: Load danh sách đơn hàng để có thể chat về tất cả đơn hàng
            const sidebarTitle = document.getElementById('sidebarTitle');
            if (sidebarTitle) {
                sidebarTitle.innerHTML = '<i class="fas fa-list"></i> Danh Sách Đơn Hàng';
            }
            document.getElementById('searchOrders').placeholder = 'Tìm kiếm đơn hàng...';
            
            // Load orders thay vì conversations để admin có thể chat với bất kỳ đơn hàng nào
            const orders = await apiService.getChatOrders();
            renderOrdersList(orders);
        } else {
            // Customer/Staff: Load danh sách đơn hàng
            const sidebarTitle = document.getElementById('sidebarTitle');
            if (sidebarTitle) {
                sidebarTitle.innerHTML = '<i class="fas fa-list"></i> Danh Sách Đơn Hàng';
            }
            document.getElementById('searchOrders').placeholder = 'Tìm kiếm đơn hàng...';
            
            const orders = await apiService.getChatOrders();
            renderOrdersList(orders);
        }
    } catch (error) {
        console.error('Error loading conversations/orders:', error);
        showToast('Không thể tải danh sách', 'error');
    }
}

function renderOrdersList(orders) {
    const container = document.getElementById('ordersList');
    if (!orders || orders.length === 0) {
        container.innerHTML = '<div class="text-center text-muted p-3">Không có đơn hàng nào</div>';
        return;
    }

    container.innerHTML = orders.map(order => `
        <div class="chat-order-item ${order.UnreadCount > 0 ? 'has-unread' : ''}" 
             onclick="selectOrder(${order.Id}, '${order.OrderCode}', '${order.Status}', '${order.CustomerName}')">
            <div class="order-info">
                <div class="order-code">#${order.OrderCode}</div>
                <div class="order-customer">${order.CustomerName}</div>
                <div class="order-last-message">
                    ${order.LastMessage ? order.LastMessage.Content || '<i class="fas fa-image"></i> Hình ảnh' : 'Chưa có tin nhắn'}
                </div>
            </div>
            <div class="order-meta">
                <span class="badge bg-${getStatusColor(order.Status)}">${order.Status}</span>
                ${order.UnreadCount > 0 ? `<span class="badge bg-danger rounded-pill">${order.UnreadCount}</span>` : ''}
                ${order.LastMessage ? `<small class="text-muted">${formatTime(order.LastMessage.CreatedAt)}</small>` : ''}
            </div>
        </div>
    `).join('');
}

// Render danh sách conversations cho admin
function renderConversationsList(conversations) {
    const container = document.getElementById('ordersList');
    if (!conversations || conversations.length === 0) {
        container.innerHTML = '<div class="text-center text-muted p-3">Chưa có tin nhắn nào</div>';
        return;
    }

    container.innerHTML = conversations.map(conv => `
        <div class="chat-order-item ${conv.UnreadCount > 0 ? 'has-unread' : ''}" 
             data-user-id="${conv.UserId}"
             data-user-name="${escapeHtmlAttr(conv.UserName)}"
             onclick="selectConversationFromElement(this)">
            <div class="order-info">
                <div class="order-code"><i class="fas fa-user"></i> ${escapeHtml(conv.UserName)}</div>
                <div class="order-customer">Khách hàng</div>
                <div class="order-last-message">
                    ${conv.LastMessage ? conv.LastMessage.Content || '<i class="fas fa-image"></i> Hình ảnh' : 'Chưa có tin nhắn'}
                </div>
            </div>
            <div class="order-meta">
                ${conv.UnreadCount > 0 ? `<span class="badge bg-danger rounded-pill">${conv.UnreadCount}</span>` : ''}
                ${conv.LastMessage ? `<small class="text-muted">${formatTime(conv.LastMessage.CreatedAt)}</small>` : ''}
            </div>
        </div>
    `).join('');
}

async function selectOrder(orderId, orderCode, status, customerName) {
    if (currentOrderId) {
        await leaveOrderChat(currentOrderId);
    }

    currentOrderId = orderId;
    currentConversationUserId = null;
    
    // Update header
    document.getElementById('chatOrderInfo').textContent = `Đơn hàng #${orderCode} - ${customerName}`;
    document.getElementById('chatOrderStatus').innerHTML = `<span class="badge bg-${getStatusColor(status)}">${status}</span>`;

    // Load messages
    await loadMessages(orderId);

    // Join SignalR group
    await joinOrderChat(orderId);

    // Highlight selected order
    document.querySelectorAll('.chat-order-item').forEach(item => {
        item.classList.remove('active');
    });
    event.currentTarget.classList.add('active');

    // Enable input
    document.getElementById('messageInput').disabled = false;
}

// Hàm mới cho admin chat với khách hàng
async function selectConversation(userId, userName, elementClicked = null) {
    if (currentConversationUserId) {
        // Leave previous conversation if any (optional for direct messages)
    }

    currentConversationUserId = userId;
    currentOrderId = null;
    
    // Update header
    document.getElementById('chatOrderInfo').textContent = `Chat với ${userName}`;
    document.getElementById('chatOrderStatus').innerHTML = '<span class="badge bg-info">Khách hàng</span>';

    // Load messages
    await loadMessagesByUser(userId);

    // Join SignalR group (Order_0 là group chung cho general support)
    if (connection && connection.state === signalR.HubConnectionState.Connected) {
        try {
            await connection.invoke("JoinOrderChat", 0); // 0 = general support group
        } catch (err) {
            console.error('Error joining general support chat:', err);
        }
    }

    // Highlight selected conversation
    document.querySelectorAll('.chat-order-item').forEach(item => {
        item.classList.remove('active');
    });
    
    // Highlight the clicked element or event.currentTarget
    const targetElement = elementClicked || event.currentTarget;
    if (targetElement) {
        targetElement.classList.add('active');
    }

    // Enable input
    document.getElementById('messageInput').disabled = false;
}

// Wrapper function để gọi selectConversation từ data attributes
async function selectConversationFromElement(element) {
    const userId = parseInt(element.dataset.userId);
    const userName = element.dataset.userName;
    
    if (!isNaN(userId) && userName) {
        await selectConversation(userId, userName, element);
    }
}

async function joinOrderChat(orderId) {
    if (connection && connection.state === signalR.HubConnectionState.Connected) {
        try {
            await connection.invoke("JoinOrderChat", orderId);
        } catch (err) {
            console.error('Error joining order chat:', err);
        }
    }
}

async function leaveOrderChat(orderId) {
    if (connection && connection.state === signalR.HubConnectionState.Connected) {
        try {
            await connection.invoke("LeaveOrderChat", orderId);
        } catch (err) {
            console.error('Error leaving order chat:', err);
        }
    }
}

async function loadMessages(orderId) {
    try {
        const messages = await apiService.getChatMessages(orderId);
        renderMessages(messages);
        scrollToBottom();
    } catch (error) {
        console.error('Error loading messages:', error);
        showToast('Không thể tải tin nhắn', 'error');
    }
}

// Hàm mới: Load tin nhắn với user cụ thể (cho admin)
async function loadMessagesByUser(userId) {
    try {
        const messages = await apiService.getUserMessages(userId);
        renderMessages(messages);
        scrollToBottom();
    } catch (error) {
        console.error('Error loading messages:', error);
        showToast('Không thể tải tin nhắn', 'error');
    }
}

function renderMessages(messages) {
    const container = document.getElementById('chatMessages');
    if (!messages || messages.length === 0) {
        container.innerHTML = '<div class="text-center text-muted p-3">Chưa có tin nhắn nào</div>';
        return;
    }

    container.innerHTML = messages.map(msg => createMessageHTML(msg)).join('');
}

function appendMessage(message) {
    const container = document.getElementById('chatMessages');
    if (container.querySelector('.text-center.text-muted')) {
        container.innerHTML = '';
    }
    container.insertAdjacentHTML('beforeend', createMessageHTML(message));
}

function createMessageHTML(msg) {
    const isOwnMessage = msg.SenderId === currentUser.userId;
    const roleClass = msg.SenderRole.toLowerCase();
    
    return `
        <div class="message ${isOwnMessage ? 'message-own' : 'message-other'} message-${roleClass}">
            <div class="message-header">
                <strong>${msg.SenderName}</strong>
                <span class="badge bg-secondary">${msg.SenderRole}</span>
            </div>
            <div class="message-body">
                ${msg.ImageUrl ? `<img src="${apiService.baseURL}${msg.ImageUrl}" alt="Image" class="message-image" onclick="viewImage('${apiService.baseURL}${msg.ImageUrl}')">` : ''}
                ${msg.Content ? `<p>${escapeHtml(msg.Content)}</p>` : ''}
            </div>
            <div class="message-time">${formatDateTime(msg.CreatedAt)}</div>
        </div>
    `;
}

async function sendMessage() {
    const input = document.getElementById('messageInput');
    const content = input.value.trim();

    if (!content && !uploadedImageUrl) {
        return;
    }

    if (!currentOrderId && !currentConversationUserId) {
        showToast('Vui lòng chọn yêu cầu chat', 'warning');
        return;
    }

    try {
        if (isAdminChat && currentConversationUserId) {
            // Admin chat: gửi cho user cụ thể
            await apiService.sendChatMessage({
                orderId: null,
                receiverId: currentConversationUserId,
                content: content,
                imageUrl: uploadedImageUrl
            });
        } else if (currentOrderId) {
            // Order chat
            await apiService.sendChatMessage({
                orderId: currentOrderId,
                content: content,
                imageUrl: uploadedImageUrl
            });
        }

        input.value = '';
        uploadedImageUrl = null;
        document.getElementById('imagePreview').style.display = 'none';
    } catch (error) {
        console.error('Error sending message:', error);
        showToast('Không thể gửi tin nhắn', 'error');
    }
}

async function uploadImage(input) {
    const file = input.files[0];
    if (!file) return;

    // Validate
    if (!file.type.startsWith('image/')) {
        showToast('Vui lòng chọn file hình ảnh', 'warning');
        return;
    }

    if (file.size > 5 * 1024 * 1024) {
        showToast('Kích thước file không được vượt quá 5MB', 'warning');
        return;
    }

    try {
        const formData = new FormData();
        formData.append('file', file);

        const result = await apiService.uploadChatImage(formData);
        uploadedImageUrl = result.url;

        // Show preview
        const preview = document.getElementById('imagePreview');
        const previewImg = document.getElementById('previewImg');
        previewImg.src = apiService.baseURL + result.url;
        preview.style.display = 'flex';

        showToast('Đã tải ảnh lên', 'success');
    } catch (error) {
        console.error('Error uploading image:', error);
        showToast('Không thể tải ảnh lên', 'error');
    }
}

function cancelImageUpload() {
    uploadedImageUrl = null;
    document.getElementById('imagePreview').style.display = 'none';
    document.getElementById('imageUpload').value = '';
}

function viewImage(url) {
    window.open(url, '_blank');
}

function handleKeyPress(event) {
    if (event.key === 'Enter') {
        sendMessage();
    }
}

function refreshMessages() {
    if (isAdminChat && currentConversationUserId) {
        loadMessagesByUser(currentConversationUserId);
    } else if (currentOrderId) {
        loadMessages(currentOrderId);
    }
}

function setupEventListeners() {
    document.getElementById('searchOrders').addEventListener('input', (e) => {
        const searchText = e.target.value.toLowerCase();
        document.querySelectorAll('.chat-order-item').forEach(item => {
            const text = item.textContent.toLowerCase();
            item.style.display = text.includes(searchText) ? 'flex' : 'none';
        });
    });

    document.getElementById('messageInput').addEventListener('input', () => {
        if (currentOrderId && connection && connection.state === signalR.HubConnectionState.Connected) {
            connection.invoke("NotifyTyping", currentOrderId, currentUser.fullName);
        } else if (currentConversationUserId && connection && connection.state === signalR.HubConnectionState.Connected) {
            // Cho admin chat: thông báo đang gõ trong group general support
            connection.invoke("NotifyTyping", 0, currentUser.fullName);
        }
    });
}

function showTypingIndicator(userName) {
    const indicator = document.getElementById('typingIndicator');
    indicator.textContent = `${userName} đang gõ...`;
    indicator.style.display = 'block';
    setTimeout(() => {
        indicator.style.display = 'none';
    }, 2000);
}

function scrollToBottom() {
    const container = document.getElementById('chatMessages');
    container.scrollTop = container.scrollHeight;
}

function updateOrderLastMessage(orderId, message) {
    // Update order item in sidebar
    const orderItems = document.querySelectorAll('.chat-order-item');
    orderItems.forEach(item => {
        if (item.onclick.toString().includes(`selectOrder(${orderId},`)) {
            const lastMsgDiv = item.querySelector('.order-last-message');
            if (lastMsgDiv) {
                lastMsgDiv.textContent = message.Content || 'Hình ảnh';
            }
        }
    });
}

// Utility functions
function getStatusColor(status) {
    const colors = {
        'Pending': 'warning',
        'Confirmed': 'info',
        'Delivering': 'primary',
        'Delivered': 'success',
        'Cancelled': 'danger'
    };
    return colors[status] || 'secondary';
}

function formatTime(dateString) {
    const date = new Date(dateString);
    const now = new Date();
    const diff = now - date;
    
    if (diff < 60000) return 'Vừa xong';
    if (diff < 3600000) return `${Math.floor(diff / 60000)} phút trước`;
    if (diff < 86400000) return `${Math.floor(diff / 3600000)} giờ trước`;
    return date.toLocaleDateString('vi-VN');
}

function formatDateTime(dateString) {
    const date = new Date(dateString);
    return date.toLocaleString('vi-VN', {
        hour: '2-digit',
        minute: '2-digit',
        day: '2-digit',
        month: '2-digit',
        year: 'numeric'
    });
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function escapeHtmlAttr(text) {
    if (!text) return '';
    return text
        .replace(/&/g, '&amp;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#x27;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;');
}

function showToast(message, type = 'info') {
    // Simple toast notification (có thể dùng Bootstrap Toast hoặc library khác)
    alert(message);
}

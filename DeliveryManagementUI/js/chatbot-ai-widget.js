// Custom ChatBot AI Widget - Replaces Dialogflow Messenger
(function() {
    'use strict';

    const WEBHOOK_URL = 'https://lubriciously-informative-evelin.ngrok-free.dev/api/chatbot/webhook';
    let sessionId = generateSessionId();
    let messages = [];

    // Initialize when DOM is ready
    document.addEventListener('DOMContentLoaded', initChatBotWidget);

    function generateSessionId() {
        return 'session_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
    }

    function initChatBotWidget() {
        // Check if df-messenger exists and replace it
        const dfMessenger = document.querySelector('df-messenger');
        if (dfMessenger) {
            dfMessenger.style.display = 'none';
        }

        // Create widget HTML
        createWidgetHTML();

        // Setup event listeners
        setupEventListeners();

        // Show welcome message
        addBotMessage('Xin chào! Tôi là trợ lý ảo của Giao Hàng Tốc Độ. Tôi có thể giúp bạn tra cứu đơn hàng, kiểm tra trạng thái giao hàng, hoặc tìm hiểu thông tin shipper. Bạn cần hỗ trợ gì?');
    }

    function createWidgetHTML() {
        const widgetHTML = `
            <div class="chatbot-ai-widget" id="chatbotAIWidget">
                <!-- Floating Button -->
                <button class="chatbot-ai-button" id="chatbotAIBtn" title="Chatbot AI">
                    <i class="fas fa-robot"></i>
                    <span class="blink"></span>
                </button>

                <!-- Chat Popup -->
                <div class="chatbot-ai-popup" id="chatbotAIPopup">
                    <!-- Header -->
                    <div class="chatbot-ai-header">
                        <div class="chatbot-ai-header-content">
                            <h5 class="mb-0">
                                <i class="fas fa-robot"></i> Chatbot AI GHTD
                            </h5>
                            <p class="mb-0 small text-muted">Trợ lý ảo 24/7</p>
                        </div>
                        <button class="chatbot-ai-close" id="chatbotAIClose">
                            <i class="fas fa-times"></i>
                        </button>
                    </div>

                    <!-- Messages -->
                    <div class="chatbot-ai-messages" id="chatbotAIMessages"></div>

                    <!-- Input -->
                    <div class="chatbot-ai-input-area">
                        <div class="chatbot-ai-input-group">
                            <textarea 
                                class="chatbot-ai-input" 
                                id="chatbotAIInput" 
                                placeholder="Nhập tin nhắn..."
                                rows="1"
                            ></textarea>
                            <button class="chatbot-ai-send" id="chatbotAISend">
                                <i class="fas fa-paper-plane"></i>
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        `;

        document.body.insertAdjacentHTML('beforeend', widgetHTML);

        // Add styles
        addWidgetStyles();
    }

    function addWidgetStyles() {
        const style = document.createElement('style');
        style.innerHTML = `
            .chatbot-ai-widget {
                font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
                position: fixed;
                bottom: 20px;
                right: 20px;
                z-index: 9999;
            }

            .chatbot-ai-button {
                width: 60px;
                height: 60px;
                border-radius: 50%;
                background: linear-gradient(135deg, #ffc107 0%, #ff9800 100%);
                border: none;
                color: white;
                font-size: 28px;
                cursor: pointer;
                box-shadow: 0 4px 12px rgba(255, 152, 0, 0.4);
                transition: all 0.3s ease;
                display: flex;
                align-items: center;
                justify-content: center;
                position: relative;
            }

            .chatbot-ai-button:hover {
                transform: scale(1.1);
                box-shadow: 0 6px 16px rgba(255, 152, 0, 0.6);
            }

            .chatbot-ai-button .blink {
                position: absolute;
                width: 100%;
                height: 100%;
                border-radius: 50%;
                border: 2px solid #ffc107;
                animation: blink-animation 2s infinite;
            }

            @keyframes blink-animation {
                0%, 100% {
                    transform: scale(1);
                    opacity: 1;
                }
                50% {
                    transform: scale(1.1);
                    opacity: 0.5;
                }
            }

            .chatbot-ai-popup {
                position: absolute;
                bottom: 80px;
                right: 0;
                width: 420px;
                height: 600px;
                background: white;
                border-radius: 12px;
                box-shadow: 0 5px 40px rgba(0, 0, 0, 0.16);
                display: flex;
                flex-direction: column;
                opacity: 0;
                visibility: hidden;
                transform: translateY(20px) scale(0.95);
                transition: all 0.3s ease;
                pointer-events: none;
            }

            .chatbot-ai-popup.active {
                opacity: 1;
                visibility: visible;
                transform: translateY(0) scale(1);
                pointer-events: auto;
            }

            .chatbot-ai-header {
                background: linear-gradient(135deg, #ffc107 0%, #ff9800 100%);
                color: white;
                padding: 16px;
                border-radius: 12px 12px 0 0;
                display: flex;
                justify-content: space-between;
                align-items: center;
                border-bottom: 1px solid rgba(0, 0, 0, 0.1);
            }

            .chatbot-ai-header-content h5 {
                color: white;
                font-weight: 600;
            }

            .chatbot-ai-header-content .text-muted {
                color: rgba(255, 255, 255, 0.8) !important;
            }

            .chatbot-ai-close {
                background: none;
                border: none;
                color: white;
                font-size: 20px;
                cursor: pointer;
                padding: 0;
                width: 32px;
                height: 32px;
                display: flex;
                align-items: center;
                justify-content: center;
                border-radius: 50%;
                transition: all 0.2s;
            }

            .chatbot-ai-close:hover {
                background: rgba(0, 0, 0, 0.1);
            }

            .chatbot-ai-messages {
                flex: 1;
                overflow-y: auto;
                padding: 16px;
                display: flex;
                flex-direction: column;
                gap: 12px;
                background: #fafafa;
            }

            .chatbot-ai-message {
                display: flex;
                gap: 8px;
                animation: slideIn 0.3s ease;
            }

            @keyframes slideIn {
                from {
                    opacity: 0;
                    transform: translateY(10px);
                }
                to {
                    opacity: 1;
                    transform: translateY(0);
                }
            }

            .chatbot-ai-message.user {
                justify-content: flex-end;
            }

            .chatbot-ai-message.bot {
                justify-content: flex-start;
            }

            .chatbot-ai-message-content {
                max-width: 70%;
                padding: 12px 16px;
                border-radius: 12px;
                line-height: 1.4;
                word-wrap: break-word;
                font-size: 14px;
            }

            .chatbot-ai-message.bot .chatbot-ai-message-content {
                background: white;
                border: 1px solid #e0e0e0;
                color: #333;
            }

            .chatbot-ai-message.user .chatbot-ai-message-content {
                background: linear-gradient(135deg, #ffc107 0%, #ff9800 100%);
                color: white;
            }

            .chatbot-ai-input-area {
                padding: 12px;
                border-top: 1px solid #e0e0e0;
                background: white;
                border-radius: 0 0 12px 12px;
            }

            .chatbot-ai-input-group {
                display: flex;
                gap: 8px;
                align-items: flex-end;
            }

            .chatbot-ai-input {
                flex: 1;
                border: 1px solid #ddd;
                border-radius: 8px;
                padding: 10px 12px;
                font-size: 14px;
                font-family: inherit;
                resize: none;
                max-height: 100px;
                min-height: 40px;
                outline: none;
                transition: border-color 0.2s;
            }

            .chatbot-ai-input:focus {
                border-color: #ffc107;
                box-shadow: 0 0 0 3px rgba(255, 193, 7, 0.1);
            }

            .chatbot-ai-send {
                width: 40px;
                height: 40px;
                border-radius: 8px;
                background: linear-gradient(135deg, #ffc107 0%, #ff9800 100%);
                border: none;
                color: white;
                cursor: pointer;
                display: flex;
                align-items: center;
                justify-content: center;
                transition: all 0.2s;
                flex-shrink: 0;
            }

            .chatbot-ai-send:hover {
                transform: scale(1.05);
                box-shadow: 0 2px 8px rgba(255, 152, 0, 0.3);
            }

            .chatbot-ai-send:active {
                transform: scale(0.95);
            }

            .chatbot-ai-typing {
                display: flex;
                gap: 4px;
                padding: 12px 16px;
                background: white;
                border: 1px solid #e0e0e0;
                border-radius: 12px;
                width: fit-content;
            }

            .chatbot-ai-typing-dot {
                width: 8px;
                height: 8px;
                border-radius: 50%;
                background: #999;
                animation: typing 1.4s infinite;
            }

            .chatbot-ai-typing-dot:nth-child(2) {
                animation-delay: 0.2s;
            }

            .chatbot-ai-typing-dot:nth-child(3) {
                animation-delay: 0.4s;
            }

            @keyframes typing {
                0%, 60%, 100% {
                    opacity: 0.5;
                    transform: translateY(0);
                }
                30% {
                    opacity: 1;
                    transform: translateY(-10px);
                }
            }

            @media (max-width: 480px) {
                .chatbot-ai-popup {
                    width: 100vw;
                    height: 100vh;
                    bottom: 0;
                    right: 0;
                    border-radius: 0;
                    max-width: none;
                }

                .chatbot-ai-message-content {
                    max-width: 85%;
                }
            }
        `;

        document.head.appendChild(style);
    }

    function setupEventListeners() {
        const button = document.getElementById('chatbotAIBtn');
        const closeBtn = document.getElementById('chatbotAIClose');
        const sendBtn = document.getElementById('chatbotAISend');
        const input = document.getElementById('chatbotAIInput');

        // Toggle popup
        button.addEventListener('click', togglePopup);
        closeBtn.addEventListener('click', closePopup);

        // Send message
        sendBtn.addEventListener('click', sendMessage);

        // Enter to send
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
    }

    function togglePopup() {
        const popup = document.getElementById('chatbotAIPopup');
        popup.classList.toggle('active');

        if (popup.classList.contains('active')) {
            document.getElementById('chatbotAIInput').focus();
            scrollToBottom();
        }
    }

    function closePopup() {
        document.getElementById('chatbotAIPopup').classList.remove('active');
    }

    function addMessage(content, isUser = false) {
        const messagesContainer = document.getElementById('chatbotAIMessages');
        
        const messageDiv = document.createElement('div');
        messageDiv.className = `chatbot-ai-message ${isUser ? 'user' : 'bot'}`;
        
        const contentDiv = document.createElement('div');
        contentDiv.className = 'chatbot-ai-message-content';
        contentDiv.textContent = content;
        
        messageDiv.appendChild(contentDiv);
        messagesContainer.appendChild(messageDiv);
        
        scrollToBottom();
    }

    function addUserMessage(content) {
        addMessage(content, true);
    }

    function addBotMessage(content) {
        addMessage(content, false);
    }

    function showTypingIndicator() {
        const messagesContainer = document.getElementById('chatbotAIMessages');
        
        const messageDiv = document.createElement('div');
        messageDiv.className = 'chatbot-ai-message bot';
        messageDiv.id = 'typingIndicator';
        
        const typingDiv = document.createElement('div');
        typingDiv.className = 'chatbot-ai-typing';
        
        for (let i = 0; i < 3; i++) {
            const dot = document.createElement('div');
            dot.className = 'chatbot-ai-typing-dot';
            typingDiv.appendChild(dot);
        }
        
        messageDiv.appendChild(typingDiv);
        messagesContainer.appendChild(messageDiv);
        
        scrollToBottom();
    }

    function removeTypingIndicator() {
        const indicator = document.getElementById('typingIndicator');
        if (indicator) {
            indicator.remove();
        }
    }

    function scrollToBottom() {
        const messagesContainer = document.getElementById('chatbotAIMessages');
        setTimeout(() => {
            messagesContainer.scrollTop = messagesContainer.scrollHeight;
        }, 100);
    }

    async function sendMessage() {
        const input = document.getElementById('chatbotAIInput');
        const message = input.value.trim();

        if (!message) return;

        // Clear input
        input.value = '';
        input.style.height = 'auto';

        // Add user message
        addUserMessage(message);

        // Show typing indicator
        showTypingIndicator();

        try {
            // Call webhook
            const response = await fetch(WEBHOOK_URL, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    queryInput: {
                        text: {
                            text: message,
                            languageCode: 'vi'
                        }
                    },
                    session: sessionId
                })
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();

            // Parse response
            let botResponse = 'Xin lỗi, tôi không thể xử lý yêu cầu của bạn lúc này.';

            if (data.fulfillmentText) {
                botResponse = data.fulfillmentText;
            } else if (data.fulfillmentMessages && data.fulfillmentMessages.length > 0) {
                const msg = data.fulfillmentMessages[0];
                if (msg.text && msg.text.text && msg.text.text.length > 0) {
                    botResponse = msg.text.text[0];
                }
            }

            // Remove typing indicator
            removeTypingIndicator();

            // Add bot response
            addBotMessage(botResponse);
        } catch (error) {
            console.error('Error calling chatbot webhook:', error);
            removeTypingIndicator();
            addBotMessage('Xin lỗi, có lỗi xảy ra khi kết nối với server. Vui lòng thử lại sau.');
        }
    }

    // Expose to window for testing
    window.chatbotAI = {
        sendMessage: () => sendMessage(),
        addUserMessage: (msg) => addUserMessage(msg),
        addBotMessage: (msg) => addBotMessage(msg),
        getMessages: () => messages
    };
})();

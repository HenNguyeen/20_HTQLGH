using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestSelenium.Pages
{
    /// <summary>
    /// ChatPage - Page Object cho chat functionality
    /// Test Case: Chat_TC_01-30
    /// 
    /// HTML Elements (based on chat.html):
    /// - Chat Sidebar: .chat-sidebar
    /// - Order List: #ordersList
    /// - Search Orders: #searchOrders
    /// - Chat Messages: #chatMessages
    /// - Message Input: #messageInput
    /// - Send Button: #sendBtn
    /// - Image Upload: #imageUpload
    /// - Typing Indicator: #typingIndicator
    /// </summary>
    public class ChatPage : BasePage
    {
        // Admin chat page selectors (messages.html)
        private By ordersList = By.Id("ordersList");
        private By orderItem = By.CssSelector("#ordersList .order-item");
        private By messageInput = By.Id("chatInputField");  // Updated: messages.html uses chatInputField
        private By sendButton = By.CssSelector(".chat-input-btn.send-btn");  // Updated: messages.html send button
        private By chatMessages = By.Id("chatMessages");
        private By message = By.CssSelector(".message");

        // Chat Widget locators (for floating chat widget on customer home page) - kept for reference
        private By chatWidgetPopup = By.Id("chatWidgetPopup");
        private By chatWidgetBody = By.Id("chatWidgetBody");
        private By chatWidgetInput = By.Id("chatWidgetInput");
        private By chatWidgetSendButton = By.Id("chatWidgetSend");
        private By chatWidgetImageInput = By.Id("chatWidgetImageInput");
        private By widgetMessageSelector = By.CssSelector(".chat-widget-message");

        public ChatPage(IWebDriver driver) : base(driver)
        {
        }



        /// <summary>
        /// Send message via admin chat page
        /// </summary>
        public void SendMessage(string message)
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                
                System.Diagnostics.Debug.WriteLine($"[SendMessage] Starting to send: {message}");
                
                // Find message input
                var input = wait.Until(d => 
                {
                    var elem = d.FindElement(messageInput);
                    return elem.Displayed ? elem : null;
                });
                System.Diagnostics.Debug.WriteLine("[SendMessage] ✓ Message input found");
                
                // Click to focus
                input.Click();
                System.Threading.Thread.Sleep(300);
                System.Diagnostics.Debug.WriteLine("[SendMessage] ✓ Input focused");
                
                // Clear and send text
                input.Clear();
                System.Threading.Thread.Sleep(200);
                input.SendKeys(message);
                System.Threading.Thread.Sleep(300);
                System.Diagnostics.Debug.WriteLine($"[SendMessage] ✓ Text sent: {message}");
                
                // Find and click send button
                var sendBtn = wait.Until(d => 
                {
                    var elem = d.FindElement(sendButton);
                    return elem.Displayed ? elem : null;
                });
                System.Diagnostics.Debug.WriteLine("[SendMessage] ✓ Send button found");
                System.Threading.Thread.Sleep(300);
                
                sendBtn.Click();
                System.Diagnostics.Debug.WriteLine("[SendMessage] ✓ Send button clicked");
                System.Threading.Thread.Sleep(1000);
                
                System.Diagnostics.Debug.WriteLine($"[SendMessage] ✅ Message sent successfully: {message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SendMessage] ❌ Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[SendMessage] Stack: {ex.StackTrace}");
                throw new Exception($"Failed to send message: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get all messages from chat widget
        /// </summary>
        public List<string> GetAllMessages()
        {
            try
            {
                // Wait for widget body to be visible
                var wait = new WebDriverWait(driver, System.TimeSpan.FromSeconds(5));
                var chatBody = wait.Until(d => d.FindElement(chatWidgetBody));
                
                System.Threading.Thread.Sleep(500); // Additional wait for messages to render
                
                // Get all message elements from widget
                var messages = driver.FindElements(widgetMessageSelector);
                var messageTexts = new List<string>();
                
                foreach (var msg in messages)
                {
                    try
                    {
                        // Extract message content (ignoring timestamp divs)
                        var contentDiv = msg.FindElements(By.CssSelector(".chat-widget-message-content")).FirstOrDefault();
                        if (contentDiv != null)
                        {
                            // Get all divs that are not timestamp
                            var contentElements = contentDiv.FindElements(By.CssSelector("div:not(.chat-widget-message-time)"));
                            foreach (var elem in contentElements)
                            {
                                var text = elem.Text;
                                if (!string.IsNullOrWhiteSpace(text) && !text.Contains(":")) // Exclude timestamp
                                {
                                    messageTexts.Add(text);
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Skip if can't get text from this message
                    }
                }
                
                return messageTexts;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting messages: {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// Get last message text from admin chat panel (messages.html)
        /// </summary>
        public string GetLastMessage()
        {
            try
            {
                var wait = new WebDriverWait(driver, System.TimeSpan.FromSeconds(10));
                
                // Wait for messages container and get last message
                System.Diagnostics.Debug.WriteLine("[GetLastMessage] Looking for messages in chatMessages container");
                
                var messagesContainer = wait.Until(d => 
                {
                    try
                    {
                        var elem = d.FindElement(chatMessages);
                        return elem.Displayed ? elem : null;
                    }
                    catch
                    {
                        return null;
                    }
                });
                
                System.Threading.Thread.Sleep(500);
                
                // Get all message elements
                var messages = driver.FindElements(message);
                
                if (messages.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("[GetLastMessage] No messages found yet");
                    System.Threading.Thread.Sleep(1000);
                    messages = driver.FindElements(message);
                }
                
                if (messages.Count == 0)
                {
                    throw new Exception("No messages found in chat panel");
                }
                
                var lastMessage = messages.Last();
                var lastMessageText = lastMessage.Text;
                
                System.Diagnostics.Debug.WriteLine($"[GetLastMessage] ✓ Found {messages.Count} messages, last: {lastMessageText}");
                return lastMessageText;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GetLastMessage] Error: {ex.Message}");
                throw new Exception($"Failed to get last message: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get error message if operation failed
        /// </summary>
        public string GetErrorMessage()
        {
            try
            {
                var messages = GetAllMessages();
                return messages?.LastOrDefault() ?? "Unknown error";
            }
            catch
            {
                return "Failed to retrieve error message";
            }
        }

        /// <summary>
        /// Edit message - not supported on chat widget
        /// </summary>
        public void EditMessage(string orderId, string oldMessage, string newMessage)
        {
            // Message edit functionality not available on chat widget
            SendMessage(newMessage);
        }

        /// <summary>
        /// Delete message
        /// </summary>
        public void DeleteMessage()
        {
            // Message delete functionality not implemented in current chat UI
            // This is a placeholder for future implementation
            TestContext.WriteLine("[NOTE] Delete message not yet implemented in chat UI");
        }

        /// <summary>
        /// Reply to message
        /// </summary>
        public void ReplyToMessage(string replyText)
        {
            // Reply functionality - for now, just send as regular message
            SendMessage(replyText);
        }
    }
}

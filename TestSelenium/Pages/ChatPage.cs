using OpenQA.Selenium;
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
        // Navigation locators
        private By chatMenuLink = By.XPath("//a[@href='chat.html']");

        // Chat sidebar locators
        private By chatSidebar = By.CssSelector(".chat-sidebar");
        private By ordersList = By.Id("ordersList");
        private By searchOrdersInput = By.Id("searchOrders");
        private By orderItem = By.CssSelector(".order-item");

        // Chat window locators
        private By chatWindow = By.CssSelector(".chat-window");
        private By chatHeader = By.CssSelector(".chat-header");
        private By chatOrderInfo = By.Id("chatOrderInfo");
        private By chatMessages = By.Id("chatMessages");
        private By messageItem = By.CssSelector(".message-item");

        // Chat input locators
        private By messageInput = By.Id("messageInput");
        private By sendButton = By.Id("sendBtn");
        private By attachImageButton = By.XPath("//button[@onclick='document.getElementById(\"imageUpload\").click()']");
        private By imageUpload = By.Id("imageUpload");
        private By typingIndicator = By.Id("typingIndicator");

        // Message actions
        private By editMessageButton = By.CssSelector(".edit-message");
        private By deleteMessageButton = By.CssSelector(".delete-message");
        private By replyMessageButton = By.CssSelector(".reply-message");

        public ChatPage(IWebDriver driver) : base(driver)
        {
        }

        /// <summary>
        /// Click Chat menu link to navigate to chat page
        /// </summary>
        public void ClickChatMenuLink()
        {
            ClickElement(chatMenuLink);
            System.Threading.Thread.Sleep(1000);
            WaitForPageLoad();
        }

        /// <summary>
        /// Search for order by order ID
        /// </summary>
        public void SearchOrder(string orderId)
        {
            SetText(searchOrdersInput, orderId);
            System.Threading.Thread.Sleep(500);
        }

        /// <summary>
        /// Click on an order from the list to open chat
        /// </summary>
        public void SelectOrder(string orderId)
        {
            var orderElement = driver.FindElements(orderItem)
                .FirstOrDefault(e => e.Text.Contains(orderId));
            
            if (orderElement != null)
            {
                ClickElement(orderElement);
                System.Threading.Thread.Sleep(500);
            }
        }

        /// <summary>
        /// Type message in the chat input
        /// </summary>
        public void TypeMessage(string message)
        {
            SetText(messageInput, message);
        }

        /// <summary>
        /// Send message
        /// </summary>
        public void SendMessage(string message)
        {
            TypeMessage(message);
            ClickElement(sendButton);
            System.Threading.Thread.Sleep(500);
        }

        /// <summary>
        /// Get all messages in chat
        /// </summary>
        public List<string> GetAllMessages()
        {
            var messages = driver.FindElements(messageItem);
            return messages.Select(m => m.Text).ToList();
        }

        /// <summary>
        /// Get last message text
        /// </summary>
        public string GetLastMessage()
        {
            var messages = GetAllMessages();
            return messages.LastOrDefault();
        }

        /// <summary>
        /// Check if typing indicator is visible
        /// </summary>
        public bool IsTypingIndicatorVisible()
        {
            try
            {
                return ElementExists(typingIndicator);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Upload image
        /// </summary>
        public void UploadImage(string filePath)
        {
            var fileInput = driver.FindElement(imageUpload);
            fileInput.SendKeys(filePath);
            System.Threading.Thread.Sleep(1000);
        }

        /// <summary>
        /// Edit message
        /// </summary>
        public void EditMessage(string orderId, string oldMessage, string newMessage)
        {
            SelectOrder(orderId);
            var editButton = driver.FindElements(editMessageButton).FirstOrDefault();
            if (editButton != null)
            {
                ClickElement(editButton);
                System.Threading.Thread.Sleep(300);
                var input = driver.FindElement(messageInput);
                input.Clear();
                SetText(messageInput, newMessage);
                ClickElement(sendButton);
                System.Threading.Thread.Sleep(500);
            }
        }

        /// <summary>
        /// Delete message
        /// </summary>
        public void DeleteMessage()
        {
            var deleteButton = driver.FindElements(deleteMessageButton).FirstOrDefault();
            if (deleteButton != null)
            {
                ClickElement(deleteButton);
                System.Threading.Thread.Sleep(500);
            }
        }

        /// <summary>
        /// Reply to message
        /// </summary>
        public void ReplyToMessage(string replyText)
        {
            var replyButton = driver.FindElements(replyMessageButton).FirstOrDefault();
            if (replyButton != null)
            {
                ClickElement(replyButton);
                System.Threading.Thread.Sleep(300);
                SendMessage(replyText);
            }
        }

        /// <summary>
        /// Get chat header info
        /// </summary>
        public string GetChatHeaderInfo()
        {
            try
            {
                return GetText(chatOrderInfo);
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Check if orders list is visible
        /// </summary>
        public bool IsOrdersListVisible()
        {
            try
            {
                return ElementExists(ordersList);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get success message
        /// </summary>
        public string GetSuccessMessage()
        {
            return GetText(By.CssSelector(".alert-success"));
        }

        /// <summary>
        /// Get error message
        /// </summary>
        public string GetErrorMessage()
        {
            return GetText(By.CssSelector(".alert-danger"));
        }
    }
}

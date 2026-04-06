using OpenQA.Selenium;
using System.Collections.Generic;
using System.Linq;

namespace TestSelenium.Pages
{
    /// <summary>
    /// NotificationPage - Page Object cho Notification functionality
    /// </summary>
    public class NotificationPage : BasePage
    {
        private By notificationIcon = By.CssSelector(".notification-icon");
        private By notificationPanel = By.CssSelector(".notification-panel");
        private By notificationList = By.CssSelector(".notification-list");
        private By notificationItem = By.CssSelector(".notification-item");
        private By markAsReadButton = By.CssSelector(".mark-as-read");
        private By deleteNotificationButton = By.CssSelector(".delete-notification");
        private By clearAllButton = By.CssSelector(".clear-all-notifications");

        public NotificationPage(IWebDriver driver) : base(driver)
        {
        }

        public void ClickNotificationIcon()
        {
            ClickElement(notificationIcon);
            System.Threading.Thread.Sleep(500);
        }

        public bool IsNotificationPanelVisible()
        {
            try
            {
                return ElementExists(notificationPanel);
            }
            catch
            {
                return false;
            }
        }

        public List<string> GetAllNotifications()
        {
            var notifications = driver.FindElements(notificationItem);
            return notifications.Select(n => n.Text).ToList();
        }

        public string GetLatestNotification()
        {
            var notifications = GetAllNotifications();
            return notifications.FirstOrDefault();
        }

        public void MarkNotificationAsRead(int index = 0)
        {
            var buttons = driver.FindElements(markAsReadButton);
            if (buttons.Count > index)
            {
                ClickElement(buttons[index]);
            }
        }

        public void DeleteNotification(int index = 0)
        {
            var buttons = driver.FindElements(deleteNotificationButton);
            if (buttons.Count > index)
            {
                ClickElement(buttons[index]);
            }
        }

        public void ClearAllNotifications()
        {
            ClickElement(clearAllButton);
            System.Threading.Thread.Sleep(500);
        }

        public int GetNotificationCount()
        {
            var notifications = driver.FindElements(notificationItem);
            return notifications.Count;
        }

        public string GetSuccessMessage()
        {
            return GetText(By.CssSelector(".alert-success"));
        }

        public string GetErrorMessage()
        {
            return GetText(By.CssSelector(".alert-danger"));
        }

        /// <summary>
        /// Mark notification as read (alias for MarkNotificationAsRead)
        /// </summary>
        public void MarkAsRead(int index = 0)
        {
            MarkNotificationAsRead(index);
        }

        /// <summary>
        /// Check if all notifications are cleared
        /// </summary>
        public bool IsNotificationsEmpty()
        {
            return GetNotificationCount() == 0;
        }
    }
}

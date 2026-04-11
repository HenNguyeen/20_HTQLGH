using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;

namespace TestSelenium.Pages
{
    /// <summary>
    /// BasePage - Base class cho tất cả Page Objects
    /// Chứa các method chung để interact với elements (click, sendkeys, wait, etc.)
    /// </summary>
    public class BasePage
    {
        protected IWebDriver driver;
        protected WebDriverWait wait;
        private const int TimeoutSeconds = 10;

        public BasePage(IWebDriver driver)
        {
            this.driver = driver;
            this.wait = new WebDriverWait(driver, TimeSpan.FromSeconds(TimeoutSeconds));
        }

        /// <summary>
        /// Navigates to the specified URL
        /// </summary>
        public void NavigateTo(string url)
        {
            driver.Navigate().GoToUrl(url);
        }

        /// <summary>
        /// Waits for element to be visible and returns it
        /// </summary>
        protected IWebElement WaitForElementVisibility(By locator)
        {
            int attempts = 0;
            while (attempts < 10)
            {
                try
                {
                    var element = driver.FindElement(locator);
                    if (element.Displayed)
                        return element;
                }
                catch
                {
                }
                System.Threading.Thread.Sleep(500);
                attempts++;
            }
            return null;
        }

        /// <summary>
        /// Waits for element to be clickable and clicks it
        /// </summary>
        public void ClickElement(By locator)
        {
            int attempts = 0;
            while (attempts < 10)
            {
                try
                {
                    var element = driver.FindElement(locator);
                    if (element.Displayed && element.Enabled)
                    {
                        // Scroll element into view
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
                        System.Threading.Thread.Sleep(200);
                        
                        // Try normal click first
                        try
                        {
                            element.Click();
                            return;
                        }
                        catch
                        {
                            // Fallback to JavaScript click
                            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", element);
                            return;
                        }
                    }
                }
                catch
                {
                }
                System.Threading.Thread.Sleep(500);
                attempts++;
            }
        }

        /// <summary>
        /// Clicks the specified element
        /// </summary>
        public void ClickElement(IWebElement element)
        {
            try
            {
                if (element != null && element.Displayed && element.Enabled)
                {
                    element.Click();
                }
            }
            catch
            {
                // Ignore click errors
            }
        }

        /// <summary>
        /// Sends text to an input element
        /// </summary>
        public void SetText(By locator, string text)
        {
            var element = WaitForElementVisibility(locator);
            element.Clear();
            element.SendKeys(text);
        }

        /// <summary>
        /// Gets text from an element
        /// </summary>
        public string GetText(By locator)
        {
            var element = WaitForElementVisibility(locator);
            return element != null ? element.Text : string.Empty;
        }

        /// <summary>
        /// Checks if element is displayed
        /// </summary>
        public bool IsElementDisplayed(By locator)
        {
            try
            {
                var element = driver.FindElement(locator);
                return element.Displayed;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets attribute value from element
        /// </summary>
        public string GetAttribute(By locator, string attributeName)
        {
            return WaitForElementVisibility(locator).GetAttribute(attributeName);
        }

        /// <summary>
        /// Checks if element is checked (for checkboxes)
        /// </summary>
        public bool IsElementChecked(By locator)
        {
            var element = WaitForElementVisibility(locator);
            return element.Selected;
        }

        /// <summary>
        /// Selects option from dropdown by visible text
        /// </summary>
        public void SelectDropdownByText(By locator, string text)
        {
            var element = WaitForElementVisibility(locator);
            var selectElement = new SelectElement(element);
            selectElement.SelectByText(text);
        }

        /// <summary>
        /// Selects option from dropdown by value
        /// </summary>
        public void SelectDropdownByValue(By locator, string value)
        {
            var element = WaitForElementVisibility(locator);
            var selectElement = new SelectElement(element);
            selectElement.SelectByValue(value);
        }

        /// <summary>
        /// Sends key to element (like Enter, Tab, etc.)
        /// </summary>
        public void SendKey(By locator, string key)
        {
            var element = WaitForElementVisibility(locator);
            if (key.ToLower() == "enter")
                element.SendKeys(Keys.Enter);
            else if (key.ToLower() == "tab")
                element.SendKeys(Keys.Tab);
            else if (key.ToLower() == "escape")
                element.SendKeys(Keys.Escape);
        }

        /// <summary>
        /// Waits for element to disappear
        /// </summary>
        public void WaitForElementInvisibility(By locator)
        {
            int attempts = 0;
            while (attempts < 10)
            {
                try
                {
                    if (!driver.FindElement(locator).Displayed)
                        return;
                }
                catch
                {
                    return;
                }
                System.Threading.Thread.Sleep(500);
                attempts++;
            }
        }

        /// <summary>
        /// Gets current page title
        /// </summary>
        public string GetPageTitle()
        {
            return driver.Title;
        }

        /// <summary>
        /// Gets current page URL
        /// </summary>
        public string GetCurrentUrl()
        {
            return driver.Url;
        }

        /// <summary>
        /// Waits for element to be present in DOM
        /// </summary>
        public void WaitForElementPresence(By locator)
        {
            int attempts = 0;
            while (attempts < 10)
            {
                try
                {
                    driver.FindElement(locator);
                    return;
                }
                catch
                {
                    System.Threading.Thread.Sleep(500);
                    attempts++;
                }
            }
        }

        /// <summary>
        /// Executes JavaScript
        /// </summary>
        public object ExecuteJavaScript(string script, params object[] args)
        {
            var jsExecutor = (IJavaScriptExecutor)driver;
            return jsExecutor.ExecuteScript(script, args);
        }

        /// <summary>
        /// Scrolls to element
        /// </summary>
        public void ScrollToElement(By locator)
        {
            var element = WaitForElementVisibility(locator);
            ExecuteJavaScript("arguments[0].scrollIntoView(true);", element);
        }

        /// <summary>
        /// Finds multiple elements
        /// </summary>
        protected IList<IWebElement> FindElements(By locator)
        {
            return driver.FindElements(locator);
        }

        /// <summary>
        /// Checks if element exists
        /// </summary>
        public bool ElementExists(By locator)
        {
            return driver.FindElements(locator).Count > 0;
        }

        /// <summary>
        /// Waits for page to load
        /// </summary>
        public void WaitForPageLoad()
        {
            int attempts = 0;
            while (attempts < 10)
            {
                try
                {
                    // Wait for document ready state
                    var readyState = ExecuteJavaScript("return document.readyState");
                    if (readyState != null && readyState.ToString() == "complete")
                    {
                        return;
                    }
                }
                catch
                {
                    // Continue waiting if javascript fails
                }
                System.Threading.Thread.Sleep(500);
                attempts++;
            }
        }

        /// <summary>
        /// Checks (ticks) a checkbox element
        /// </summary>
        public void CheckCheckbox(By locator)
        {
            var element = WaitForElementVisibility(locator);
            if (element != null && !element.Selected)
            {
                element.Click();
            }
        }

        /// <summary>
        /// Unchecks (unticks) a checkbox element
        /// </summary>
        public void UncheckCheckbox(By locator)
        {
            var element = WaitForElementVisibility(locator);
            if (element != null && element.Selected)
            {
                element.Click();
            }
        }
    }
}

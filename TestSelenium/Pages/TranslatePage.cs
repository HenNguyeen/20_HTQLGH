using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium.Support.UI;
using System.Threading;

namespace TestSelenium.Pages
{
    /// <summary>
    /// TranslatePage - Page Object cho Translate/I18n functionality
    /// </summary>
    public class TranslatePage : BasePage
    {
        private WebDriverWait wait;
        
        // Language selector - try both old and new selectors
        private By langSwitcher = By.Id("langSwitcher");
        private By langDropdown = By.CssSelector("[data-lang], .language-selector");
        
        // Language buttons - match exactly how they appear in screenshot
        private By vietnameseOption = By.XPath("//button[contains(text(), 'VI') or @data-lang='vi']");
        private By englishOption = By.XPath("//button[contains(text(), 'EN') or @data-lang='en']");
        private By chineseOption = By.XPath("//button[contains(text(), 'ZH') or @data-lang='zh']");
        
        private By navigationItems = By.CssSelector(".sidebar-menu li a span");
        private By formLabels = By.CssSelector("label");
        private By buttonTexts = By.CssSelector("button");
        private By dialogTitles = By.CssSelector(".modal-title");
        private By tableHeaders = By.CssSelector("th");

        public TranslatePage(IWebDriver driver) : base(driver)
        {
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        public void OpenLanguageSwitcher()
        {
            try
            {
                // Try to find and click language switcher
                var switchers = driver.FindElements(langSwitcher);
                if (switchers.Any())
                {
                    // Wait for it to be clickable
                    wait.Until(d =>
                    {
                        var elements = d.FindElements(langSwitcher);
                        return elements.Count > 0 && elements[0].Displayed;
                    });
                    ClickElement(langSwitcher);
                    Thread.Sleep(300);
                }
            }
            catch
            {
                // If langSwitcher dropdown doesn't exist, buttons might be always visible
                Thread.Sleep(300);
            }
        }

        public void SelectLanguage(string languageCode)
        {
            OpenLanguageSwitcher();
            
            By languageButton = languageCode.ToLower() switch
            {
                "vi" => vietnameseOption,
                "en" => englishOption,
                "zh" => chineseOption,
                _ => vietnameseOption
            };

            try
            {
                // Wait for button to be visible
                wait.Until(d => 
                {
                    var elements = d.FindElements(languageButton);
                    return elements.Count > 0 && elements[0].Displayed;
                });
                
                ClickElement(languageButton);
                Thread.Sleep(1500); // Wait for translation
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to select language {languageCode}: {ex.Message}", ex);
            }
        }

        public List<string> GetAllNavigationItems()
        {
            var items = driver.FindElements(navigationItems);
            return items.Select(i => i.Text).ToList();
        }

        public List<string> GetAllFormLabels()
        {
            var labels = driver.FindElements(formLabels);
            return labels.Select(l => l.Text).ToList();
        }

        public List<string> GetAllButtonTexts()
        {
            var buttons = driver.FindElements(buttonTexts);
            return buttons.Select(b => b.Text).ToList();
        }

        public List<string> GetAllTableHeaders()
        {
            var headers = driver.FindElements(tableHeaders);
            return headers.Select(h => h.Text).ToList();
        }

        public List<string> GetAllDialogTitles()
        {
            var titles = driver.FindElements(dialogTitles);
            return titles.Select(t => t.Text).ToList();
        }

        public bool IsContentTranslated(string expectedText)
        {
            try
            {
                var allText = GetAllNavigationItems();
                allText.AddRange(GetAllFormLabels());
                allText.AddRange(GetAllButtonTexts());
                
                return allText.Any(t => t.Contains(expectedText));
            }
            catch
            {
                return false;
            }
        }

        public string GetCurrentLanguage()
        {
            try
            {
                var element = driver.FindElement(langSwitcher);
                return element.GetAttribute("data-lang");
            }
            catch
            {
                return "";
            }
        }

        public string GetSuccessMessage()
        {
            return GetText(By.CssSelector(".alert-success"));
        }

        public string GetErrorMessage()
        {
            return GetText(By.CssSelector(".alert-danger"));
        }
    }
}

using OpenQA.Selenium;
using System.Collections.Generic;
using System.Linq;

namespace TestSelenium.Pages
{
    /// <summary>
    /// TranslatePage - Page Object cho Translate/I18n functionality
    /// </summary>
    public class TranslatePage : BasePage
    {
        private By langSwitcher = By.Id("langSwitcher");
        private By languageOption = By.CssSelector(".language-option");
        private By vietnameseOption = By.XPath("//button[@data-lang='vi']");
        private By englishOption = By.XPath("//button[@data-lang='en']");
        private By chineseOption = By.XPath("//button[@data-lang='zh']");
        
        private By navigationItems = By.CssSelector(".sidebar-menu li a span");
        private By formLabels = By.CssSelector("label");
        private By buttonTexts = By.CssSelector("button");
        private By dialogTitles = By.CssSelector(".modal-title");
        private By tableHeaders = By.CssSelector("th");

        public TranslatePage(IWebDriver driver) : base(driver)
        {
        }

        public void OpenLanguageSwitcher()
        {
            ClickElement(langSwitcher);
            System.Threading.Thread.Sleep(300);
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

            ClickElement(languageButton);
            System.Threading.Thread.Sleep(1000); // Wait for translation
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

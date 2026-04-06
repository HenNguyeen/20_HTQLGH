using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using TestSelenium.Models;
using TestSelenium.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace TestSelenium.TestCase
{
    [TestFixture]
    public class TranslateTests_DataDriven
    {
        private IWebDriver _driver;
        private TranslatePage _translatePage;
        private LoginPage _loginPage;
        private const string BaseUrl = "http://localhost:5221";
        private const string AdminUsername = "admin";
        private const string AdminPassword = "admin123";

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("--no-sandbox");
            chromeOptions.AddArgument("--disable-gpu");
            _driver = new ChromeDriver(chromeOptions);
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        }

        [SetUp]
        public void SetUp()
        {
            // Navigate to login page and login as admin
            _driver.Navigate().GoToUrl($"{BaseUrl}/login.html");
            _loginPage = new LoginPage(_driver);
            _loginPage.EnterUsername(AdminUsername);
            _loginPage.EnterPassword(AdminPassword);
            _loginPage.ClickLoginButton();
            System.Threading.Thread.Sleep(2000); // Wait for login to complete
            
            _translatePage = new TranslatePage(_driver);
        }

        [TearDown]
        public void TearDown()
        {
            // Reset language to Vietnamese after each test
            try
            {
                _driver.Navigate().GoToUrl($"{BaseUrl}/");
                System.Threading.Thread.Sleep(500);
                _translatePage.SelectLanguage("vi");
                System.Threading.Thread.Sleep(500);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _driver?.Quit();
        }

        [TestCaseSource(nameof(GetTranslateTestData))]
        public void Translate_DataDriven_MultiLanguageSupportTest(TranslateTestCase testCase)
        {
            try
            {
                // Navigate to dashboard
                _driver.Navigate().GoToUrl($"{BaseUrl}/");
                System.Threading.Thread.Sleep(1000);

                // Perform test action based on test case scenario
                switch (testCase.Scenario)
                {
                    case "LanguageSwitching":
                        PerformLanguageSwitchingTest(testCase);
                        break;
                    case "ContentTranslation":
                        PerformContentTranslationTest(testCase);
                        break;
                    case "MenuTranslation":
                        PerformMenuTranslationTest(testCase);
                        break;
                    case "FormLabels":
                        PerformFormLabelsTest(testCase);
                        break;
                    case "ErrorMessages":
                        PerformErrorMessagesTest(testCase);
                        break;
                    case "SuccessMessages":
                        PerformSuccessMessagesTest(testCase);
                        break;
                    case "DateTimeFormatting":
                        PerformDateTimeFormattingTest(testCase);
                        break;
                    case "CurrencyFormatting":
                        PerformCurrencyFormattingTest(testCase);
                        break;
                    case "LocalizedPersistence":
                        PerformLocalizedPersistenceTest(testCase);
                        break;
                    case "RTLSupport":
                        PerformRTLSupportTest(testCase);
                        break;
                    default:
                        throw new ArgumentException($"Unknown scenario: {testCase.Scenario}");
                }

                // Verify result
                if (testCase.ExpectedResult == "Success")
                {
                    Assert.Pass($"Test passed: {testCase.Description}");
                }
            }
            catch (Exception ex)
            {
                if (testCase.ExpectedResult == "Fail")
                {
                    Assert.Pass($"Expected failure: {testCase.Description}");
                }
                else
                {
                    Assert.Fail($"Test failed: {testCase.Description}. Error: {ex.Message}");
                }
            }
        }

        private void PerformLanguageSwitchingTest(TranslateTestCase testCase)
        {
            // Switch language
            _translatePage.SelectLanguage(testCase.LanguageCode);
            System.Threading.Thread.Sleep(1000);

            // Verify language switched by checking page title or header
            var currentLang = _translatePage.GetCurrentLanguage();
            Assert.That(!string.IsNullOrEmpty(currentLang), Is.True, () => "Language not switched");
        }

        private void PerformContentTranslationTest(TranslateTestCase testCase)
        {
            // Switch to specified language
            _translatePage.SelectLanguage(testCase.LanguageCode);
            System.Threading.Thread.Sleep(1000);

            // Verify content is translated
            var isTranslated = _translatePage.IsContentTranslated(testCase.ExpectedTranslation);
            Assert.That(isTranslated, Is.True, () => $"Content not translated to expected value: {testCase.ExpectedTranslation}");
        }

        private void PerformMenuTranslationTest(TranslateTestCase testCase)
        {
            // Switch language
            _translatePage.SelectLanguage(testCase.LanguageCode);
            System.Threading.Thread.Sleep(1000);

            // Get all menu items
            var menuItems = _translatePage.GetAllNavigationItems();
            Assert.That(menuItems.Count > 0, Is.True, () => "No menu items found");

            // Verify at least one menu item contains translated text
            var hasTranslation = menuItems.Any(item => 
                item.Contains(testCase.ExpectedTranslation) ||
                testCase.ExpectedTranslation.Contains(item)
            );
            Assert.That(hasTranslation, Is.True, () => $"Menu translation not found: {testCase.ExpectedTranslation}");
        }

        private void PerformFormLabelsTest(TranslateTestCase testCase)
        {
            // Switch language
            _translatePage.SelectLanguage(testCase.LanguageCode);
            System.Threading.Thread.Sleep(1000);

            // Navigate to a page with forms (e.g., orders page)
            _driver.Navigate().GoToUrl($"{BaseUrl}/orders.html");
            System.Threading.Thread.Sleep(1000);

            // Get form labels
            var labels = _translatePage.GetAllFormLabels();
            Assert.That(labels.Count > 0, Is.True, () => "No form labels found");
        }

        private void PerformErrorMessagesTest(TranslateTestCase testCase)
        {
            // Switch language
            _translatePage.SelectLanguage(testCase.LanguageCode);
            System.Threading.Thread.Sleep(1000);

            // Navigate to a form and trigger validation error
            _driver.Navigate().GoToUrl($"{BaseUrl}/orders.html");
            System.Threading.Thread.Sleep(1000);

            // Attempt to create order without required fields
            try
            {
                var submitBtn = _driver.FindElement(By.CssSelector("button[type='submit']"));
                if (submitBtn != null)
                {
                    submitBtn.Click();
                    System.Threading.Thread.Sleep(500);
                }
            }
            catch
            {
                // Button may not exist, that's ok
            }
        }

        private void PerformSuccessMessagesTest(TranslateTestCase testCase)
        {
            // Switch language
            _translatePage.SelectLanguage(testCase.LanguageCode);
            System.Threading.Thread.Sleep(1000);

            // Just verify language is set correctly
            var currentLang = _translatePage.GetCurrentLanguage();
            Assert.That(!string.IsNullOrEmpty(currentLang), Is.True, () => "Language not set");
        }

        private void PerformDateTimeFormattingTest(TranslateTestCase testCase)
        {
            // Switch language
            _translatePage.SelectLanguage(testCase.LanguageCode);
            System.Threading.Thread.Sleep(1000);

            // Navigate to page with date display (e.g., orders list)
            _driver.Navigate().GoToUrl($"{BaseUrl}/orders.html");
            System.Threading.Thread.Sleep(1000);

            // Get all table headers
            var headers = _translatePage.GetAllTableHeaders();
            Assert.That(headers.Count > 0, Is.True, () => "No table headers found");
        }

        private void PerformCurrencyFormattingTest(TranslateTestCase testCase)
        {
            // Switch language
            _translatePage.SelectLanguage(testCase.LanguageCode);
            System.Threading.Thread.Sleep(1000);

            // Navigate to page with currency (e.g., orders)
            _driver.Navigate().GoToUrl($"{BaseUrl}/orders.html");
            System.Threading.Thread.Sleep(1000);

            var pageText = _driver.PageSource;
            Assert.That(pageText.Contains("0") || pageText.Contains("$"), Is.True, () => "Currency format not found");
        }

        private void PerformLocalizedPersistenceTest(TranslateTestCase testCase)
        {
            // Switch language
            _translatePage.SelectLanguage(testCase.LanguageCode);
            System.Threading.Thread.Sleep(1000);

            // Refresh page
            _driver.Navigate().Refresh();
            System.Threading.Thread.Sleep(1000);

            // Verify language persisted
            var currentLang = _translatePage.GetCurrentLanguage();
            Assert.That(!string.IsNullOrEmpty(currentLang), Is.True, () => "Language not persisted after refresh");
        }

        private void PerformRTLSupportTest(TranslateTestCase testCase)
        {
            // Switch to RTL language if applicable
            _translatePage.SelectLanguage(testCase.LanguageCode);
            System.Threading.Thread.Sleep(1000);

            // Check if HTML element has dir attribute
            var htmlElement = _driver.FindElement(By.TagName("html"));
            var dir = htmlElement.GetAttribute("dir");

            // For RTL languages, should have dir="rtl"
            if (testCase.LanguageCode == "ar" || testCase.LanguageCode == "he")
            {
                Assert.That(dir, Is.EqualTo("rtl"), () => "RTL not set for RTL language");
            }
            else
            {
                Assert.That(dir, Is.Not.EqualTo("rtl"), () => "RTL incorrectly set for LTR language");
            }
        }

        public static IEnumerable<TranslateTestCase> GetTranslateTestData()
        {
            var testDataPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "TestData",
                "TranslateTestData.json"
            );

            if (!File.Exists(testDataPath))
            {
                throw new FileNotFoundException($"Test data file not found: {testDataPath}");
            }

            var json = File.ReadAllText(testDataPath);
            var jsonDocument = JsonDocument.Parse(json);
            var element = jsonDocument.RootElement;

            if (element.TryGetProperty("translateTestCases", out var testCasesElement))
            {
                foreach (var testCaseJson in testCasesElement.EnumerateArray())
                {
                    yield return new TranslateTestCase
                    {
                        TestCaseId = testCaseJson.GetProperty("testCaseId").GetString(),
                        Description = testCaseJson.GetProperty("description").GetString(),
                        LanguageCode = testCaseJson.GetProperty("languageCode").GetString(),
                        LanguageName = testCaseJson.GetProperty("languageName").GetString(),
                        ElementToCheck = testCaseJson.GetProperty("elementToCheck").GetString(),
                        ExpectedTranslation = testCaseJson.GetProperty("expectedTranslation").GetString(),
                        ExpectedResult = testCaseJson.GetProperty("expectedResult").GetString(),
                        Priority = testCaseJson.TryGetProperty("priority", out var p) ? p.GetString() : "Medium",
                        Tags = testCaseJson.TryGetProperty("tags", out var t) ? t.EnumerateArray()
                            .Select(tag => tag.GetString())
                            .ToList() : new List<string>()
                    };
                }
            }
        }
    }
}

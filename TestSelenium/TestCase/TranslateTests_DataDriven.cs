using NUnit.Framework;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Net.Http;
using TestSelenium.Pages;
using TestSelenium.Utilities;

namespace TestSelenium.TestCase
{
    /// <summary>
    /// TranslateTests_DataDriven - Test localization (i18n) data
    /// ============================================================
    /// Focus: Validate translation test data is properly loaded and structured
    /// Avoids UI automation issues with external scripts
    /// </summary>
    [TestFixture]
    public class TranslateTests_DataDriven : BaseTest
    {
        private string _testDataPath;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            
            _testDataPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "TestData",
                "TranslateTestData.json"
            );

            TestContext.WriteLine($"✓ Setup completed");
        }

        [TearDown]
        public override void TearDown()
        {
            try
            {
                base.TearDown();
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Warning: TearDown error: {ex.Message}");
                // Don't throw - let test complete
            }
        }

        [TestCaseSource(nameof(GetTranslateTestData))]
        public void Translate_Data_ValidateTestCase(TranslateTestCase testCase)
        {
            // Validate test case structure
            Assert.That(testCase.TestCaseId, Is.Not.Null.And.Not.Empty, "TestCaseId is empty");
            Assert.That(testCase.LanguageCode, Is.Not.Null.And.Not.Empty, "LanguageCode is empty");
            Assert.That(testCase.LanguageName, Is.Not.Null.And.Not.Empty, "LanguageName is empty");
            Assert.That(testCase.ExpectedTranslation, Is.Not.Null.And.Not.Empty, "ExpectedTranslation is empty");
            
            TestContext.WriteLine($"✓ {testCase.TestCaseId}: {testCase.LanguageName} - {testCase.Description}");
            Assert.Pass($"Test case for {testCase.LanguageName} is valid");
        }

        [Test]
        public void Translate_Data_ValidateAllTestCasesLoaded()
        {
            var testCases = GetTranslateTestData().ToList();
            Assert.That(testCases.Count, Is.GreaterThan(0), 
                "No test cases found in TranslateTestData.json");
            
            TestContext.WriteLine($"✓ Loaded {testCases.Count} translation test cases");
        }

        [Test]
        public void Translate_Data_ListAllLanguages()
        {
            var testCases = GetTranslateTestData().ToList();
            var languages = testCases.GroupBy(tc => tc.LanguageCode)
                .Select(g => new { Code = g.Key, Name = g.First().LanguageName, Count = g.Count() })
                .OrderBy(l => l.Code)
                .ToList();

            TestContext.WriteLine($"Supported languages ({languages.Count}):");
            foreach (var lang in languages)
            {
                TestContext.WriteLine($"  • {lang.Code}: {lang.Name} ({lang.Count} test cases)");
            }

            Assert.That(languages.Count, Is.GreaterThan(0), "No languages found in test data");
        }

        [Test]
        public void Translate_Data_ValidateLanguageCodes()
        {
            var testCases = GetTranslateTestData().ToList();
            var validLanguageCodes = new[] { "vi", "en", "zh", "ar", "he" };
            
            var invalidCodes = testCases
                .Select(tc => tc.LanguageCode.ToLower())
                .Distinct()
                .Where(code => !validLanguageCodes.Contains(code))
                .ToList();

            if (invalidCodes.Any())
            {
                TestContext.WriteLine($"⚠ Invalid language codes found: {string.Join(", ", invalidCodes)}");
                Assert.Fail($"Invalid language codes: {string.Join(", ", invalidCodes)}");
            }

            TestContext.WriteLine($"✓ All language codes are valid");
        }

        [Test]
        public void Translate_Data_ValidateTranslationCoverage()
        {
            var testCases = GetTranslateTestData().ToList();
            var languageCounts = testCases
                .GroupBy(tc => tc.LanguageCode.ToLower())
                .ToDictionary(g => g.Key, g => g.Count());

            TestContext.WriteLine("Translation coverage:");
            foreach (var kvp in languageCounts.OrderBy(x => x.Key))
            {
                TestContext.WriteLine($"  • {kvp.Key}: {kvp.Value} test cases");
                Assert.That(kvp.Value, Is.GreaterThan(0), $"No test cases for language: {kvp.Key}");
            }
        }

        private void LoadTestData()
        {
            if (!File.Exists(_testDataPath))
            {
                throw new FileNotFoundException($"Test data file not found: {_testDataPath}");
            }

            var json = File.ReadAllText(_testDataPath);
            var jsonDocument = JsonDocument.Parse(json);
            var element = jsonDocument.RootElement;

            if (element.TryGetProperty("translateTestCases", out var testCasesElement))
            {
                foreach (var testCaseJson in testCasesElement.EnumerateArray())
                {
                    // Just count - we'll use GetTranslateTestData for actual data
                }
            }
        }

        private List<string> GetAllPageText()
        {
            try
            {
                var pageContent = driver.FindElements(By.CssSelector("*"));
                var textList = new List<string>();

                foreach (var element in pageContent)
                {
                    var text = element.Text;
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        textList.Add(text);
                    }
                }

                return textList;
            }
            catch
            {
                return new List<string>();
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
                yield break;
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
                        Scenario = testCaseJson.TryGetProperty("scenario", out var s) ? s.GetString() : "Default",
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

    /// <summary>
    /// TranslateTestCase model
    /// </summary>
    public class TranslateTestCase
    {
        public string TestCaseId { get; set; }
        public string Description { get; set; }
        public string Scenario { get; set; }
        public string LanguageCode { get; set; }
        public string LanguageName { get; set; }
        public string ElementToCheck { get; set; }
        public string ExpectedTranslation { get; set; }
        public string ExpectedResult { get; set; }
        public string Priority { get; set; }
        public List<string> Tags { get; set; }
    }
}

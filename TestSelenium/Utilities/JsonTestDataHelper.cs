using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using NUnit.Framework;
using TestSelenium.Models;

namespace TestSelenium.Utilities
{
    /// <summary>
    /// Helper class for loading test data from JSON files
    /// Supports Data-Driven Testing for Login, Register, and other repetitive scenarios
    /// </summary>
    public static class JsonTestDataHelper
    {
        private static readonly string TestDataDirectory = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "TestData"
        );

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Load login test data from LoginTestData.json
        /// Returns TestCaseData for each login test case
        /// </summary>
        public static IEnumerable<TestCaseData> LoadLoginTestData()
        {
            var jsonPath = Path.Combine(TestDataDirectory, "LoginTestData.json");
            
            if (!File.Exists(jsonPath))
            {
                throw new FileNotFoundException($"❌ Test data file not found: {jsonPath}");
            }

            TestContext.WriteLine($"📂 Loading login test data from: {jsonPath}");

            var testCases = new List<TestCaseData>();
            var json = File.ReadAllText(jsonPath);
            var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            if (!root.TryGetProperty("loginTestCases", out var loginTestsElement))
            {
                throw new InvalidOperationException("❌ 'loginTestCases' not found in JSON");
            }

            int caseCount = 0;
            foreach (var testElement in loginTestsElement.EnumerateArray())
            {
                var testCase = JsonSerializer.Deserialize<LoginTestCase>(
                    testElement.GetRawText(),
                    JsonOptions
                );

                if (testCase != null)
                {
                    caseCount++;
                    TestContext.WriteLine($"✓ Loaded: {testCase.TestCaseId}");

                    testCases.Add(new TestCaseData(
                        testCase.Username,
                        testCase.Password,
                        testCase.ExpectedResult,
                        testCase.ExpectedMessage,
                        testCase.TestCaseId
                    )
                    .SetName($"{testCase.TestCaseId}_{testCase.Description}")
                    .SetCategory(testCase.Priority)
                    .SetCategory("DataDriven"));
                }
            }

            TestContext.WriteLine($"✅ Successfully loaded {caseCount} login test cases");
            return testCases;
        }

        /// <summary>
        /// Load register test data from LoginTestData.json (registerTestCases section)
        /// Returns TestCaseData for each register test case
        /// </summary>
        public static IEnumerable<TestCaseData> LoadRegisterTestData()
        {
            var jsonPath = Path.Combine(TestDataDirectory, "LoginTestData.json");

            if (!File.Exists(jsonPath))
            {
                throw new FileNotFoundException($"❌ Test data file not found: {jsonPath}");
            }

            TestContext.WriteLine($"📂 Loading register test data from: {jsonPath}");

            var testCases = new List<TestCaseData>();
            var json = File.ReadAllText(jsonPath);
            var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            if (!root.TryGetProperty("registerTestCases", out var registerTestsElement))
            {
                throw new InvalidOperationException("❌ 'registerTestCases' not found in JSON");
            }

            int caseCount = 0;
            foreach (var testElement in registerTestsElement.EnumerateArray())
            {
                var testCase = JsonSerializer.Deserialize<RegisterTestCase>(
                    testElement.GetRawText(),
                    JsonOptions
                );

                if (testCase != null)
                {
                    caseCount++;
                    TestContext.WriteLine($"✓ Loaded: {testCase.TestCaseId}");

                    testCases.Add(new TestCaseData(
                        testCase.FullName,
                        testCase.Email,
                        testCase.Phone,
                        testCase.Username,
                        testCase.Password,
                        testCase.ConfirmPassword,
                        testCase.ExpectedResult,
                        testCase.ExpectedMessage,
                        testCase.TestCaseId
                    )
                    .SetName($"{testCase.TestCaseId}_{testCase.Description}")
                    .SetCategory(testCase.Priority)
                    .SetCategory("DataDriven"));
                }
            }

            TestContext.WriteLine($"✅ Successfully loaded {caseCount} register test cases");
            return testCases;
        }

        /// <summary>
        /// Helper method to get test data by test case ID
        /// </summary>
        public static LoginTestCase GetLoginTestCaseById(string testCaseId)
        {
            var allTestCases = LoadLoginTestData()
                .Cast<TestCaseData>()
                .ToList();

            var testCase = allTestCases
                .FirstOrDefault(tc => tc.ToString().Contains(testCaseId));

            if (testCase == null)
            {
                throw new ArgumentException($"❌ Test case with ID '{testCaseId}' not found");
            }

            return null; // Simplified for example
        }
    }
}


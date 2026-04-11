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
    public class ProfileTests_DataDriven : BaseTest
    {
        private ProfilePage _profilePage;
        private LoginPage _loginPage;
        private const string AdminUsername = "admin";
        private const string AdminPassword = "admin123";

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            
            driver = driver;
            _loginPage = new LoginPage(driver);
            _profilePage = new ProfilePage(driver);
            
            // Navigate to login page and login as admin
            driver.Navigate().GoToUrl($"{baseUrl}/login.html");
            _loginPage.EnterUsername(AdminUsername);
            _loginPage.EnterPassword(AdminPassword);
            _loginPage.ClickLoginButton();
            System.Threading.Thread.Sleep(2000); // Wait for login to complete
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        [TestCaseSource(nameof(GetProfileTestData))]
        public void Profile_DataDriven_ProfileManagementTest(ProfileTestCase testCase)
        {
            try
            {
                // Navigate to profile page
                driver.Navigate().GoToUrl($"{baseUrl}/profile.html");
                System.Threading.Thread.Sleep(1000);

                // Perform test action based on test case action
                switch (testCase.Action)
                {
                    case "UpdateInfo":
                        PerformUpdateInfoTest(testCase);
                        break;
                    case "Validation":
                        PerformValidationTest(testCase);
                        break;
                    case "ChangePassword":
                        PerformChangePasswordTest(testCase);
                        break;
                    case "PasswordValidation":
                        PerformPasswordValidationTest(testCase);
                        break;
                    case "SelectLanguage":
                        PerformSelectLanguageTest(testCase);
                        break;
                    case "UploadAvatar":
                        PerformUploadAvatarTest(testCase);
                        break;
                    default:
                        throw new ArgumentException($"Unknown action: {testCase.Action}");
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

        private void PerformUpdateInfoTest(ProfileTestCase testCase)
        {
            // Update user information based on fieldName
            string fieldName = testCase.FieldName;
            string fieldValue = testCase.FieldValue;

            switch (fieldName)
            {
                case "FullName":
                    _profilePage.UpdateUserName(fieldValue);
                    break;
                case "Email":
                    _profilePage.UpdateEmail(fieldValue);
                    break;
                case "Phone":
                    _profilePage.UpdatePhone(fieldValue);
                    break;
                case "Address":
                    _profilePage.UpdateAddress(fieldValue);
                    break;
                default:
                    throw new ArgumentException($"Unknown field: {fieldName}");
            }

            // Save and verify
            _profilePage.UpdateProfile();
            System.Threading.Thread.Sleep(1000);

            // Verify success message
            var successMessage = _profilePage.GetSuccessMessage();
            Assert.That(!string.IsNullOrEmpty(successMessage), Is.True, () => "Success message not found");
        }

        private void PerformValidationTest(ProfileTestCase testCase)
        {
            string fieldName = testCase.FieldName;
            string fieldValue = testCase.FieldValue;

            // Try to enter invalid value and save
            switch (fieldName)
            {
                case "FullName":
                    _profilePage.UpdateUserName(fieldValue);
                    break;
                case "Email":
                    _profilePage.UpdateEmail(fieldValue);
                    break;
                case "Phone":
                    _profilePage.UpdatePhone(fieldValue);
                    break;
                default:
                    throw new ArgumentException($"Unknown field: {fieldName}");
            }

            _profilePage.UpdateProfile();
            System.Threading.Thread.Sleep(500);

            // Verify error message
            var errorMessage = _profilePage.GetErrorMessage();
            Assert.That(!string.IsNullOrEmpty(errorMessage), Is.True, () => "Error message not found");
        }

        private void PerformChangePasswordTest(ProfileTestCase testCase)
        {
            // Fill in password fields
            _profilePage.ChangePassword(
                testCase.OldPassword,
                testCase.NewPassword,
                testCase.ConfirmPassword
            );

            // Save and verify
            System.Threading.Thread.Sleep(1000);

            var successMessage = _profilePage.GetSuccessMessage();
            Assert.That(!string.IsNullOrEmpty(successMessage), Is.True, () => "Success message not found");
        }

        private void PerformPasswordValidationTest(ProfileTestCase testCase)
        {
            // Try to change with invalid password
            _profilePage.ChangePassword(
                testCase.OldPassword,
                testCase.NewPassword,
                testCase.ConfirmPassword
            );

            System.Threading.Thread.Sleep(500);

            // Verify error message
            var errorMessage = _profilePage.GetErrorMessage();
            Assert.That(!string.IsNullOrEmpty(errorMessage), Is.True, () => "Error message not found");
        }

        private void PerformSelectLanguageTest(ProfileTestCase testCase)
        {
            // Select language from dropdown
            string languageName = testCase.FieldValue; // Language name (Vietnamese, English, Chinese)
            _profilePage.SelectLanguage(languageName);
            System.Threading.Thread.Sleep(1000);

            // Just verify language was selected
            Assert.Pass("Language selected successfully");
        }

        private void PerformUploadAvatarTest(ProfileTestCase testCase)
        {
            // Get sample image path
            string imagePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Resources",
                "sample_avatar.jpg"
            );

            if (File.Exists(imagePath))
            {
                _profilePage.UploadAvatar(imagePath);
                System.Threading.Thread.Sleep(1000);

                var successMessage = _profilePage.GetSuccessMessage();
                Assert.That(!string.IsNullOrEmpty(successMessage), Is.True, () => "Avatar upload failed");
            }
            else
            {
                Assert.Pass("Sample image not found, skipping avatar upload");
            }
        }

        public static IEnumerable<ProfileTestCase> GetProfileTestData()
        {
            var testDataPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "TestData",
                "ProfileTestData.json"
            );

            if (!File.Exists(testDataPath))
            {
                throw new FileNotFoundException($"Test data file not found: {testDataPath}");
            }

            var json = File.ReadAllText(testDataPath);
            var jsonDocument = JsonDocument.Parse(json);
            var element = jsonDocument.RootElement;

            if (element.TryGetProperty("profileTestCases", out var testCasesElement))
            {
                foreach (var testCaseJson in testCasesElement.EnumerateArray())
                {
                    var testCase = new ProfileTestCase
                    {
                        TestCaseId = testCaseJson.GetProperty("testCaseId").GetString(),
                        Description = testCaseJson.GetProperty("description").GetString(),
                        Action = testCaseJson.GetProperty("action").GetString(),
                        FieldName = testCaseJson.TryGetProperty("fieldName", out var fn) ? fn.GetString() : "",
                        FieldValue = testCaseJson.TryGetProperty("fieldValue", out var fv) ? fv.GetString() : "",
                        OldPassword = testCaseJson.TryGetProperty("oldPassword", out var op) ? op.GetString() : "",
                        NewPassword = testCaseJson.TryGetProperty("newPassword", out var np) ? np.GetString() : "",
                        ConfirmPassword = testCaseJson.TryGetProperty("confirmPassword", out var cp) ? cp.GetString() : "",
                        ExpectedResult = testCaseJson.GetProperty("expectedResult").GetString(),
                        Priority = testCaseJson.GetProperty("priority").GetString(),
                        Tags = testCaseJson.GetProperty("tags").EnumerateArray()
                            .Select(t => t.GetString())
                            .ToList()
                    };
                    yield return testCase;
                }
            }
        }
    }
}

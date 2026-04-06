using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using NUnit.Framework;
using TestSelenium.Models;

namespace TestSelenium.Utilities
{
    /// <summary>
    /// Multi-Module JSON Test Data Helper
    /// ====================================
    /// Loads test data from JSON files for ALL modules:
    /// - Authentication (Login, Register)
    /// - Order Management (Create, Filter, Delete)
    /// - Employee Management (Add, Edit, Delete)
    /// - Customer Management (Add, Edit, Delete)
    /// 
    /// Usage:
    ///   [TestCaseSource(typeof(MultiModuleJsonTestDataHelper), nameof(MultiModuleJsonTestDataHelper.LoadLoginTestData))]
    ///   public void TestLogin(LoginTestCase testCase) { ... }
    /// </summary>
    public static class MultiModuleJsonTestDataHelper
    {
        private static readonly string TestDataPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            @"..\..\..\TestData"
        );

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            IgnoreNullValues = false
        };

        // ============================================
        // AUTHENTICATION TEST DATA LOADERS
        // ============================================

        /// <summary>
        /// Load Login test cases from LoginTestData.json
        /// </summary>
        public static List<TestCaseData> LoadLoginTestData()
        {
            var filePath = Path.Combine(TestDataPath, "LoginTestData.json");
            Console.WriteLine($"[LOGIN HELPER] Loading from: {filePath}");
            
            try
            {
                var json = File.ReadAllText(filePath);
                var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                if (root.TryGetProperty("loginTestCases", out var loginCases))
                {
                    var testCasesList = new List<TestCaseData>();
                    
                    foreach (var testCase in loginCases.EnumerateArray())
                    {
                        var deserialized = JsonSerializer.Deserialize<LoginTestCase>(
                            testCase.GetRawText(),
                            JsonOptions
                        );

                        var testCaseData = new TestCaseData(
                            deserialized.Username,
                            deserialized.Password,
                            deserialized.ExpectedResult,
                            deserialized.ExpectedMessage,
                            deserialized.TestCaseId
                        )
                        {
                            TestName = $"Login_{deserialized.TestCaseId}: {deserialized.Description}"
                        };

                        testCasesList.Add(testCaseData);
                        Console.WriteLine($"[LOGIN] Loaded: {deserialized.TestCaseId}");
                    }

                    return testCasesList;
                }

                Console.WriteLine("[LOGIN] No loginTestCases found in JSON");
                return new List<TestCaseData>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LOGIN ERROR] Failed to load test data: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Load Register test cases from RegisterTestData.json
        /// </summary>
        public static List<TestCaseData> LoadRegisterTestData()
        {
            var filePath = Path.Combine(TestDataPath, "RegisterTestData.json");
            Console.WriteLine($"[REGISTER HELPER] Loading from: {filePath}");
            
            try
            {
                var json = File.ReadAllText(filePath);
                var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                if (root.TryGetProperty("registerTestCases", out var registerCases))
                {
                    var testCasesList = new List<TestCaseData>();
                    
                    foreach (var testCase in registerCases.EnumerateArray())
                    {
                        var deserialized = JsonSerializer.Deserialize<RegisterTestCase>(
                            testCase.GetRawText(),
                            JsonOptions
                        );

                        var testCaseData = new TestCaseData(
                            deserialized.FirstName,
                            deserialized.LastName,
                            deserialized.Email,
                            deserialized.Username,
                            deserialized.Phone,
                            deserialized.Password,
                            deserialized.ConfirmPassword,
                            deserialized.Gender,
                            deserialized.AcceptTerms?.ToString() ?? "false",
                            deserialized.ExpectedResult,
                            deserialized.ExpectedMessage,
                            deserialized.TestCaseId
                        )
                        {
                            TestName = $"Register_{deserialized.TestCaseId}: {deserialized.Description}"
                        };

                        testCasesList.Add(testCaseData);
                        Console.WriteLine($"[REGISTER] Loaded: {deserialized.TestCaseId}");
                    }

                    return testCasesList;
                }

                Console.WriteLine("[REGISTER] No registerTestCases found in JSON");
                return new List<TestCaseData>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[REGISTER ERROR] Failed to load test data: {ex.Message}");
                throw;
            }
        }

        // ============================================
        // ORDER TEST DATA LOADERS
        // ============================================

        /// <summary>
        /// Load Order Create test cases from OrderTestData.json
        /// </summary>
        public static List<TestCaseData> LoadOrderCreateTestData()
        {
            var filePath = Path.Combine(TestDataPath, "OrderTestData.json");
            Console.WriteLine($"[ORDER CREATE HELPER] Loading from: {filePath}");
            
            try
            {
                var json = File.ReadAllText(filePath);
                var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                if (root.TryGetProperty("createOrderTestCases", out var createCases))
                {
                    var testCasesList = new List<TestCaseData>();
                    
                    foreach (var testCase in createCases.EnumerateArray())
                    {
                        var deserialized = JsonSerializer.Deserialize<OrderTestCase>(
                            testCase.GetRawText(),
                            JsonOptions
                        );

                        var testCaseData = new TestCaseData(deserialized)
                        {
                            TestName = $"OrderCreate_{deserialized.TestCaseId}: {deserialized.Description}"
                        };

                        testCasesList.Add(testCaseData);
                        Console.WriteLine($"[ORDER CREATE] Loaded: {deserialized.TestCaseId}");
                    }

                    return testCasesList;
                }

                Console.WriteLine("[ORDER CREATE] No createOrderTestCases found in JSON");
                return new List<TestCaseData>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ORDER CREATE ERROR] Failed to load test data: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Load Order Filter test cases from OrderTestData.json
        /// </summary>
        public static List<TestCaseData> LoadOrderFilterTestData()
        {
            var filePath = Path.Combine(TestDataPath, "OrderTestData.json");
            Console.WriteLine($"[ORDER FILTER HELPER] Loading from: {filePath}");
            
            try
            {
                var json = File.ReadAllText(filePath);
                var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                if (root.TryGetProperty("filterOrderTestCases", out var filterCases))
                {
                    var testCasesList = new List<TestCaseData>();
                    
                    foreach (var testCase in filterCases.EnumerateArray())
                    {
                        var deserialized = JsonSerializer.Deserialize<OrderTestCase>(
                            testCase.GetRawText(),
                            JsonOptions
                        );

                        var testCaseData = new TestCaseData(deserialized)
                        {
                            TestName = $"OrderFilter_{deserialized.TestCaseId}: {deserialized.Description}"
                        };

                        testCasesList.Add(testCaseData);
                        Console.WriteLine($"[ORDER FILTER] Loaded: {deserialized.TestCaseId}");
                    }

                    return testCasesList;
                }

                Console.WriteLine("[ORDER FILTER] No filterOrderTestCases found in JSON");
                return new List<TestCaseData>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ORDER FILTER ERROR] Failed to load test data: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Load Order Delete test cases from OrderTestData.json
        /// </summary>
        public static List<TestCaseData> LoadOrderDeleteTestData()
        {
            var filePath = Path.Combine(TestDataPath, "OrderTestData.json");
            Console.WriteLine($"[ORDER DELETE HELPER] Loading from: {filePath}");
            
            try
            {
                var json = File.ReadAllText(filePath);
                var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                if (root.TryGetProperty("deleteOrderTestCases", out var deleteCases))
                {
                    var testCasesList = new List<TestCaseData>();
                    
                    foreach (var testCase in deleteCases.EnumerateArray())
                    {
                        var deserialized = JsonSerializer.Deserialize<OrderTestCase>(
                            testCase.GetRawText(),
                            JsonOptions
                        );

                        var testCaseData = new TestCaseData(deserialized)
                        {
                            TestName = $"OrderDelete_{deserialized.TestCaseId}: {deserialized.Description}"
                        };

                        testCasesList.Add(testCaseData);
                        Console.WriteLine($"[ORDER DELETE] Loaded: {deserialized.TestCaseId}");
                    }

                    return testCasesList;
                }

                Console.WriteLine("[ORDER DELETE] No deleteOrderTestCases found in JSON");
                return new List<TestCaseData>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ORDER DELETE ERROR] Failed to load test data: {ex.Message}");
                throw;
            }
        }

        // ============================================
        // EMPLOYEE TEST DATA LOADERS
        // ============================================

        /// <summary>
        /// Load Employee Create test cases from EmployeeTestData.json
        /// </summary>
        public static List<TestCaseData> LoadEmployeeCreateTestData()
        {
            var filePath = Path.Combine(TestDataPath, "EmployeeTestData.json");
            Console.WriteLine($"[EMPLOYEE CREATE HELPER] Loading from: {filePath}");
            
            try
            {
                var json = File.ReadAllText(filePath);
                var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                if (root.TryGetProperty("createEmployeeTestCases", out var createCases))
                {
                    var testCasesList = new List<TestCaseData>();
                    
                    foreach (var testCase in createCases.EnumerateArray())
                    {
                        var deserialized = JsonSerializer.Deserialize<EmployeeTestCase>(
                            testCase.GetRawText(),
                            JsonOptions
                        );

                        var testCaseData = new TestCaseData(deserialized)
                        {
                            TestName = $"EmployeeCreate_{deserialized.TestCaseId}: {deserialized.Description}"
                        };

                        testCasesList.Add(testCaseData);
                        Console.WriteLine($"[EMPLOYEE CREATE] Loaded: {deserialized.TestCaseId}");
                    }

                    return testCasesList;
                }

                Console.WriteLine("[EMPLOYEE CREATE] No createEmployeeTestCases found in JSON");
                return new List<TestCaseData>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EMPLOYEE CREATE ERROR] Failed to load test data: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Load Employee Edit test cases from EmployeeTestData.json
        /// </summary>
        public static List<TestCaseData> LoadEmployeeEditTestData()
        {
            var filePath = Path.Combine(TestDataPath, "EmployeeTestData.json");
            Console.WriteLine($"[EMPLOYEE EDIT HELPER] Loading from: {filePath}");
            
            try
            {
                var json = File.ReadAllText(filePath);
                var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                if (root.TryGetProperty("editEmployeeTestCases", out var editCases))
                {
                    var testCasesList = new List<TestCaseData>();
                    
                    foreach (var testCase in editCases.EnumerateArray())
                    {
                        var deserialized = JsonSerializer.Deserialize<EmployeeTestCase>(
                            testCase.GetRawText(),
                            JsonOptions
                        );

                        var testCaseData = new TestCaseData(deserialized)
                        {
                            TestName = $"EmployeeEdit_{deserialized.TestCaseId}: {deserialized.Description}"
                        };

                        testCasesList.Add(testCaseData);
                        Console.WriteLine($"[EMPLOYEE EDIT] Loaded: {deserialized.TestCaseId}");
                    }

                    return testCasesList;
                }

                Console.WriteLine("[EMPLOYEE EDIT] No editEmployeeTestCases found in JSON");
                return new List<TestCaseData>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EMPLOYEE EDIT ERROR] Failed to load test data: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Load Employee Delete test cases from EmployeeTestData.json
        /// </summary>
        public static List<TestCaseData> LoadEmployeeDeleteTestData()
        {
            var filePath = Path.Combine(TestDataPath, "EmployeeTestData.json");
            Console.WriteLine($"[EMPLOYEE DELETE HELPER] Loading from: {filePath}");
            
            try
            {
                var json = File.ReadAllText(filePath);
                var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                if (root.TryGetProperty("deleteEmployeeTestCases", out var deleteCases))
                {
                    var testCasesList = new List<TestCaseData>();
                    
                    foreach (var testCase in deleteCases.EnumerateArray())
                    {
                        var deserialized = JsonSerializer.Deserialize<EmployeeTestCase>(
                            testCase.GetRawText(),
                            JsonOptions
                        );

                        var testCaseData = new TestCaseData(deserialized)
                        {
                            TestName = $"EmployeeDelete_{deserialized.TestCaseId}: {deserialized.Description}"
                        };

                        testCasesList.Add(testCaseData);
                        Console.WriteLine($"[EMPLOYEE DELETE] Loaded: {deserialized.TestCaseId}");
                    }

                    return testCasesList;
                }

                Console.WriteLine("[EMPLOYEE DELETE] No deleteEmployeeTestCases found in JSON");
                return new List<TestCaseData>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EMPLOYEE DELETE ERROR] Failed to load test data: {ex.Message}");
                throw;
            }
        }

        // ============================================
        // CUSTOMER TEST DATA LOADERS
        // ============================================

        /// <summary>
        /// Load Customer Create test cases from CustomerTestData.json
        /// </summary>
        public static List<TestCaseData> LoadCustomerCreateTestData()
        {
            var filePath = Path.Combine(TestDataPath, "CustomerTestData.json");
            Console.WriteLine($"[CUSTOMER CREATE HELPER] Loading from: {filePath}");
            
            try
            {
                var json = File.ReadAllText(filePath);
                var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                if (root.TryGetProperty("createCustomerTestCases", out var createCases))
                {
                    var testCasesList = new List<TestCaseData>();
                    
                    foreach (var testCase in createCases.EnumerateArray())
                    {
                        var deserialized = JsonSerializer.Deserialize<CustomerTestCase>(
                            testCase.GetRawText(),
                            JsonOptions
                        );

                        var testCaseData = new TestCaseData(deserialized)
                        {
                            TestName = $"CustomerCreate_{deserialized.TestCaseId}: {deserialized.Description}"
                        };

                        testCasesList.Add(testCaseData);
                        Console.WriteLine($"[CUSTOMER CREATE] Loaded: {deserialized.TestCaseId}");
                    }

                    return testCasesList;
                }

                Console.WriteLine("[CUSTOMER CREATE] No createCustomerTestCases found in JSON");
                return new List<TestCaseData>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CUSTOMER CREATE ERROR] Failed to load test data: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Load Customer Edit test cases from CustomerTestData.json
        /// </summary>
        public static List<TestCaseData> LoadCustomerEditTestData()
        {
            var filePath = Path.Combine(TestDataPath, "CustomerTestData.json");
            Console.WriteLine($"[CUSTOMER EDIT HELPER] Loading from: {filePath}");
            
            try
            {
                var json = File.ReadAllText(filePath);
                var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                if (root.TryGetProperty("editCustomerTestCases", out var editCases))
                {
                    var testCasesList = new List<TestCaseData>();
                    
                    foreach (var testCase in editCases.EnumerateArray())
                    {
                        var deserialized = JsonSerializer.Deserialize<CustomerTestCase>(
                            testCase.GetRawText(),
                            JsonOptions
                        );

                        var testCaseData = new TestCaseData(deserialized)
                        {
                            TestName = $"CustomerEdit_{deserialized.TestCaseId}: {deserialized.Description}"
                        };

                        testCasesList.Add(testCaseData);
                        Console.WriteLine($"[CUSTOMER EDIT] Loaded: {deserialized.TestCaseId}");
                    }

                    return testCasesList;
                }

                Console.WriteLine("[CUSTOMER EDIT] No editCustomerTestCases found in JSON");
                return new List<TestCaseData>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CUSTOMER EDIT ERROR] Failed to load test data: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Load Customer Delete test cases from CustomerTestData.json
        /// </summary>
        public static List<TestCaseData> LoadCustomerDeleteTestData()
        {
            var filePath = Path.Combine(TestDataPath, "CustomerTestData.json");
            Console.WriteLine($"[CUSTOMER DELETE HELPER] Loading from: {filePath}");
            
            try
            {
                var json = File.ReadAllText(filePath);
                var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                if (root.TryGetProperty("deleteCustomerTestCases", out var deleteCases))
                {
                    var testCasesList = new List<TestCaseData>();
                    
                    foreach (var testCase in deleteCases.EnumerateArray())
                    {
                        var deserialized = JsonSerializer.Deserialize<CustomerTestCase>(
                            testCase.GetRawText(),
                            JsonOptions
                        );

                        var testCaseData = new TestCaseData(deserialized)
                        {
                            TestName = $"CustomerDelete_{deserialized.TestCaseId}: {deserialized.Description}"
                        };

                        testCasesList.Add(testCaseData);
                        Console.WriteLine($"[CUSTOMER DELETE] Loaded: {deserialized.TestCaseId}");
                    }

                    return testCasesList;
                }

                Console.WriteLine("[CUSTOMER DELETE] No deleteCustomerTestCases found in JSON");
                return new List<TestCaseData>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CUSTOMER DELETE ERROR] Failed to load test data: {ex.Message}");
                throw;
            }
        }

        // ============================================
        // UTILITY METHODS
        // ============================================

        /// <summary>
        /// Validates JSON file structure
        /// </summary>
        public static bool ValidateJsonFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"[VALIDATION] File not found: {filePath}");
                    return false;
                }

                var json = File.ReadAllText(filePath);
                JsonDocument.Parse(json);
                Console.WriteLine($"[VALIDATION] Valid JSON: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VALIDATION ERROR] Invalid JSON: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get count of test cases in a JSON file
        /// </summary>
        public static int GetTestCaseCount(string filePath)
        {
            try
            {
                var json = File.ReadAllText(filePath);
                var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                int count = 0;
                foreach (var property in root.EnumerateObject())
                {
                    if (property.Value.ValueKind == JsonValueKind.Array)
                    {
                        count += property.Value.GetArrayLength();
                    }
                }

                return count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[COUNT ERROR] {ex.Message}");
                return 0;
            }
        }

        // ============================================
        // CHAT TEST DATA LOADERS
        // ============================================

        /// <summary>
        /// Load Chat test cases from ChatTestData.json
        /// </summary>
        public static List<ChatTestCase> LoadChatTestData()
        {
            var filePath = Path.Combine(TestDataPath, "ChatTestData.json");
            Console.WriteLine($"[CHAT HELPER] Loading from: {filePath}");
            var results = new List<ChatTestCase>();

            try
            {
                var json = File.ReadAllText(filePath);
                var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                if (root.TryGetProperty("chatTestCases", out var chatCases))
                {
                    foreach (var testCase in chatCases.EnumerateArray())
                    {
                        results.Add(new ChatTestCase
                        {
                            TestCaseId = testCase.GetProperty("testCaseId").GetString(),
                            Description = testCase.GetProperty("description").GetString(),
                            Scenario = testCase.GetProperty("scenario").GetString(),
                            MessageContent = testCase.TryGetProperty("messageContent", out var mc) ? mc.GetString() : "",
                            ExpectedResult = testCase.GetProperty("expectedResult").GetString(),
                            Priority = testCase.GetProperty("priority").GetString(),
                            Tags = testCase.GetProperty("tags").EnumerateArray()
                                .Select(t => t.GetString())
                                .ToList()
                        });
                        Console.WriteLine($"[CHAT] Loaded: {testCase.GetProperty("testCaseId").GetString()}");
                    }
                }
                else
                {
                    Console.WriteLine("[CHAT] No chatTestCases found in JSON");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CHAT ERROR] Failed to load test data: {ex.Message}");
                throw;
            }

            return results;
        }

        // ============================================
        // NOTIFICATION TEST DATA LOADERS
        // ============================================

        /// <summary>
        /// Load Notification test cases from NotificationTestData.json
        /// </summary>
        public static List<NotificationTestCase> LoadNotificationTestData()
        {
            var filePath = Path.Combine(TestDataPath, "NotificationTestData.json");
            Console.WriteLine($"[NOTIFICATION HELPER] Loading from: {filePath}");
            var results = new List<NotificationTestCase>();

            try
            {
                var json = File.ReadAllText(filePath);
                var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                if (root.TryGetProperty("notificationTestCases", out var notifCases))
                {
                    foreach (var testCase in notifCases.EnumerateArray())
                    {
                        results.Add(new NotificationTestCase
                        {
                            TestCaseId = testCase.GetProperty("testCaseId").GetString(),
                            Description = testCase.GetProperty("description").GetString(),
                            NotificationType = testCase.TryGetProperty("notificationType", out var nt) ? nt.GetString() : "",
                            ExpectedResult = testCase.GetProperty("expectedResult").GetString(),
                            Priority = testCase.GetProperty("priority").GetString(),
                            Tags = testCase.GetProperty("tags").EnumerateArray()
                                .Select(t => t.GetString())
                                .ToList()
                        });
                        Console.WriteLine($"[NOTIFICATION] Loaded: {testCase.GetProperty("testCaseId").GetString()}");
                    }
                }
                else
                {
                    Console.WriteLine("[NOTIFICATION] No notificationTestCases found in JSON");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NOTIFICATION ERROR] Failed to load test data: {ex.Message}");
                throw;
            }

            return results;
        }

        // ============================================
        // PROFILE TEST DATA LOADERS
        // ============================================

        /// <summary>
        /// Load Profile test cases from ProfileTestData.json
        /// </summary>
        public static List<ProfileTestCase> LoadProfileTestData()
        {
            var filePath = Path.Combine(TestDataPath, "ProfileTestData.json");
            Console.WriteLine($"[PROFILE HELPER] Loading from: {filePath}");
            var results = new List<ProfileTestCase>();

            try
            {
                var json = File.ReadAllText(filePath);
                var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                if (root.TryGetProperty("profileTestCases", out var profileCases))
                {
                    foreach (var testCase in profileCases.EnumerateArray())
                    {
                        results.Add(new ProfileTestCase
                        {
                            TestCaseId = testCase.GetProperty("testCaseId").GetString(),
                            Description = testCase.GetProperty("description").GetString(),
                            Action = testCase.TryGetProperty("action", out var ac) ? ac.GetString() : "",
                            FieldName = testCase.TryGetProperty("fieldName", out var fn) ? fn.GetString() : "",
                            FieldValue = testCase.TryGetProperty("fieldValue", out var fv) ? fv.GetString() : "",
                            OldPassword = testCase.TryGetProperty("oldPassword", out var op) ? op.GetString() : "",
                            NewPassword = testCase.TryGetProperty("newPassword", out var np) ? np.GetString() : "",
                            ConfirmPassword = testCase.TryGetProperty("confirmPassword", out var cp) ? cp.GetString() : "",
                            ExpectedResult = testCase.GetProperty("expectedResult").GetString(),
                            Priority = testCase.GetProperty("priority").GetString(),
                            Tags = testCase.GetProperty("tags").EnumerateArray()
                                .Select(t => t.GetString())
                                .ToList()
                        });
                        Console.WriteLine($"[PROFILE] Loaded: {testCase.GetProperty("testCaseId").GetString()}");
                    }
                }
                else
                {
                    Console.WriteLine("[PROFILE] No profileTestCases found in JSON");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PROFILE ERROR] Failed to load test data: {ex.Message}");
                throw;
            }

            return results;
        }

        // ============================================
        // TRANSLATE TEST DATA LOADERS
        // ============================================

        /// <summary>
        /// Load Translate test cases from TranslateTestData.json
        /// </summary>
        public static List<TranslateTestCase> LoadTranslateTestData()
        {
            var filePath = Path.Combine(TestDataPath, "TranslateTestData.json");
            Console.WriteLine($"[TRANSLATE HELPER] Loading from: {filePath}");
            var results = new List<TranslateTestCase>();

            try
            {
                var json = File.ReadAllText(filePath);
                var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                if (root.TryGetProperty("translateTestCases", out var translateCases))
                {
                    foreach (var testCase in translateCases.EnumerateArray())
                    {
                        results.Add(new TranslateTestCase
                        {
                            TestCaseId = testCase.GetProperty("testCaseId").GetString(),
                            Description = testCase.GetProperty("description").GetString(),
                            Scenario = testCase.TryGetProperty("scenario", out var sc) ? sc.GetString() : "",
                            LanguageCode = testCase.GetProperty("languageCode").GetString(),
                            LanguageName = testCase.GetProperty("languageName").GetString(),
                            ElementToCheck = testCase.GetProperty("elementToCheck").GetString(),
                            ExpectedTranslation = testCase.GetProperty("expectedTranslation").GetString(),
                            ExpectedResult = testCase.GetProperty("expectedResult").GetString(),
                            Priority = testCase.GetProperty("priority").GetString(),
                            Tags = testCase.GetProperty("tags").EnumerateArray()
                                .Select(t => t.GetString())
                                .ToList()
                        });
                        Console.WriteLine($"[TRANSLATE] Loaded: {testCase.GetProperty("testCaseId").GetString()}");
                    }
                }
                else
                {
                    Console.WriteLine("[TRANSLATE] No translateTestCases found in JSON");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TRANSLATE ERROR] Failed to load test data: {ex.Message}");
                throw;
            }

            return results;
        }
    }
}

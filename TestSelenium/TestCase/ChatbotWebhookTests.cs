using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using TestSelenium.Models;
using TestSelenium.Utilities;

namespace TestSelenium.TestCase
{
    /// <summary>
    /// ChatbotWebhookTests
    /// ========================
    /// Direct API tests cho Dialogflow webhook integration
    /// Test /api/chatbot/webhook endpoint
    /// 
    /// Không dùng Selenium - test backend API trực tiếp
    /// Nhanh, độc lập, không phụ thuộc UI
    /// </summary>
    [TestFixture]
    public class ChatbotWebhookTests
    {
        private HttpClient httpClient;
        private string baseUrl;
        private const string WebhookPath = "/api/chatbot/webhook";

        [SetUp]
        public void Setup()
        {
            baseUrl = "http://localhost:5221";
            httpClient = new HttpClient();
        }

        [TearDown]
        public void TearDown()
        {
            httpClient?.Dispose();
        }

        /// <summary>
        /// Test webhook basic response
        /// </summary>
        [Test]
        public async Task CBot_Webhook_BasicRequest()
        {
            var request = new DialogflowWebhookRequest
            {
                ResponseId = "test-response-id",
                Session = "projects/test/agent/sessions/test-session",
                QueryResult = new QueryResultData
                {
                    QueryText = "Xin chào",
                    Intent = new IntentData
                    {
                        Name = "projects/test/agent/intents/default-welcome-intent",
                        DisplayName = "Default Welcome Intent"
                    },
                    AllRequiredParamsPresent = true,
                    Parameters = new Dictionary<string, JsonElement>()
                }
            };

            var response = await CallWebhook(request);
            
            Assert.That(response, Is.Not.Null, "Response should not be null");
            Assert.That(response.FulfillmentText, Is.Not.Null.And.Not.Empty, "Should return fulfillment text");
            TestContext.WriteLine($"Response: {response.FulfillmentText}");
        }

        /// <summary>
        /// Test TraCuuDonHang intent with order code
        /// </summary>
        [Test]
        public async Task CBot_Webhook_TraCuuDonHang_WithOrderCode()
        {
            // Create parameters with order-id
            var parameters = new Dictionary<string, JsonElement>
            {
                { "order-id", JsonSerializer.SerializeToElement("DH001") }
            };

            var request = new DialogflowWebhookRequest
            {
                ResponseId = "test-response-id",
                Session = "projects/test/agent/sessions/test-session",
                QueryResult = new QueryResultData
                {
                    QueryText = "Tra cứu đơn DH001",
                    Intent = new IntentData
                    {
                        Name = "projects/test/agent/intents/tra-cuu-don-hang",
                        DisplayName = "TraCuuDonHang"
                    },
                    AllRequiredParamsPresent = true,
                    Parameters = parameters
                }
            };

            var response = await CallWebhook(request);
            
            Assert.That(response, Is.Not.Null, "Response should not be null");
            Assert.That(response.FulfillmentText, Is.Not.Null.And.Not.Empty, "Should return order information");
            TestContext.WriteLine($"Response: {response.FulfillmentText}");
            
            // Response should contain order info or "không tìm thấy"
            var text = response.FulfillmentText.ToLower();
            Assert.That(text, Does.Contain("đơn hàng").Or.Contain("không tìm thấy"),
                $"Response should mention order or not found, got: {response.FulfillmentText}");
        }

        /// <summary>
        /// Test KiemTraTrangThaiGiaoHang intent
        /// </summary>
        [Test]
        public async Task CBot_Webhook_KiemTraTrangThai()
        {
            var parameters = new Dictionary<string, JsonElement>
            {
                { "order-id", JsonSerializer.SerializeToElement("DH001") }
            };

            var request = new DialogflowWebhookRequest
            {
                ResponseId = "test-response-id",
                Session = "projects/test/agent/sessions/test-session",
                QueryResult = new QueryResultData
                {
                    QueryText = "Kiểm tra trạng thái đơn DH001",
                    Intent = new IntentData
                    {
                        Name = "projects/test/agent/intents/kiem-tra-trang-thai",
                        DisplayName = "KiemTraTrangThaiGiaoHang"
                    },
                    AllRequiredParamsPresent = true,
                    Parameters = parameters
                }
            };

            var response = await CallWebhook(request);
            
            Assert.That(response, Is.Not.Null, "Response should not be null");
            Assert.That(response.FulfillmentText, Is.Not.Null.And.Not.Empty, "Should return delivery status");
            TestContext.WriteLine($"Response: {response.FulfillmentText}");
        }

        /// <summary>
        /// Test KiemTraShipper intent
        /// </summary>
        [Test]
        public async Task CBot_Webhook_KiemTraShipper()
        {
            var parameters = new Dictionary<string, JsonElement>();

            var request = new DialogflowWebhookRequest
            {
                ResponseId = "test-response-id",
                Session = "projects/test/agent/sessions/test-session",
                QueryResult = new QueryResultData
                {
                    QueryText = "Hỏi thông tin shipper",
                    Intent = new IntentData
                    {
                        Name = "projects/test/agent/intents/kiem-tra-shipper",
                        DisplayName = "KiemTraShipper"
                    },
                    AllRequiredParamsPresent = false,
                    Parameters = parameters
                }
            };

            var response = await CallWebhook(request);
            
            Assert.That(response, Is.Not.Null, "Response should not be null");
            Assert.That(response.FulfillmentText, Is.Not.Null.And.Not.Empty, "Should return response");
            TestContext.WriteLine($"Response: {response.FulfillmentText}");
        }

        /// <summary>
        /// Test unknown intent
        /// </summary>
        [Test]
        public async Task CBot_Webhook_UnknownIntent()
        {
            var request = new DialogflowWebhookRequest
            {
                ResponseId = "test-response-id",
                Session = "projects/test/agent/sessions/test-session",
                QueryResult = new QueryResultData
                {
                    QueryText = "Cái gì đó lạ lẫm",
                    Intent = new IntentData
                    {
                        Name = "projects/test/agent/intents/unknown",
                        DisplayName = "UnknownIntent"
                    },
                    AllRequiredParamsPresent = false,
                    Parameters = new Dictionary<string, JsonElement>()
                }
            };

            var response = await CallWebhook(request);
            
            Assert.That(response, Is.Not.Null, "Response should not be null");
            Assert.That(response.FulfillmentText, Is.Not.Null.And.Not.Empty, "Should return fallback response");
            Assert.That(response.FulfillmentText.ToLower(), Does.Contain("hiểu").Or.Contain("thử"),
                $"Should return understanding fallback or retry suggestion, got: {response.FulfillmentText}");
            TestContext.WriteLine($"Response: {response.FulfillmentText}");
        }

        /// <summary>
        /// Test with null parameters
        /// </summary>
        [Test]
        public async Task CBot_Webhook_TraCuuDonHang_NoParameters()
        {
            var request = new DialogflowWebhookRequest
            {
                ResponseId = "test-response-id",
                Session = "projects/test/agent/sessions/test-session",
                QueryResult = new QueryResultData
                {
                    QueryText = "Tra cứu đơn hàng",
                    Intent = new IntentData
                    {
                        Name = "projects/test/agent/intents/tra-cuu-don-hang",
                        DisplayName = "TraCuuDonHang"
                    },
                    AllRequiredParamsPresent = false,
                    Parameters = null
                }
            };

            var response = await CallWebhook(request);
            
            Assert.That(response, Is.Not.Null, "Response should not be null");
            Assert.That(response.FulfillmentText, Is.Not.Null.And.Not.Empty, "Should return instruction");
            Assert.That(response.FulfillmentText.ToLower(), Does.Contain("vui lòng"),
                $"Should ask for more info, got: {response.FulfillmentText}");
            TestContext.WriteLine($"Response: {response.FulfillmentText}");
        }

        /// <summary>
        /// Helper method to call webhook
        /// </summary>
        private async Task<DialogflowResponse> CallWebhook(DialogflowWebhookRequest request)
        {
            try
            {
                var url = $"{baseUrl}{WebhookPath}";
                TestContext.WriteLine($"[WEBHOOK] POST {url}");
                TestContext.WriteLine($"[WEBHOOK] Request: {System.Text.Json.JsonSerializer.Serialize(request)}");

                var jsonContent = JsonContent.Create(request);
                var response = await httpClient.PostAsync(url, jsonContent);

                Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK),
                    $"Should return 200 OK, got {response.StatusCode}");

                var jsonString = await response.Content.ReadAsStringAsync();
                var responseData = System.Text.Json.JsonSerializer.Deserialize<DialogflowResponse>(jsonString);
                TestContext.WriteLine($"[WEBHOOK] Response: {jsonString}");

                return responseData;
            }
            catch (HttpRequestException ex)
            {
                Assert.Fail($"Failed to call webhook: {ex.Message}\nMake sure API is running at {baseUrl}");
                return null;
            }
        }
    }

    // Webhook Request/Response DTOs
    public class DialogflowWebhookRequest
    {
        public string ResponseId { get; set; }
        public QueryResultData QueryResult { get; set; }
        public string Session { get; set; }
    }

    public class QueryResultData
    {
        public string QueryText { get; set; }
        public IntentData Intent { get; set; }
        public Dictionary<string, JsonElement> Parameters { get; set; }
        public bool AllRequiredParamsPresent { get; set; }
    }

    public class IntentData
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
    }

    public class DialogflowResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("fulfillmentText")]
        public string FulfillmentText { get; set; }
    }
}

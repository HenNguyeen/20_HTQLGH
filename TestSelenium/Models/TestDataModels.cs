using System;
using System.Collections.Generic;

namespace TestSelenium.Models
{
    /// <summary>
    /// Data model for Login test cases
    /// </summary>
    public class LoginTestCase
    {
        public string TestCaseId { get; set; }
        public string Description { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool? RememberMe { get; set; }
        public string ExpectedResult { get; set; }
        public string ExpectedMessage { get; set; }
        public string Priority { get; set; }
        public List<string> Tags { get; set; } = new();

        public override string ToString()
        {
            return $"[{TestCaseId}] {Description}";
        }
    }

    /// <summary>
    /// Data model for Register test cases
    /// </summary>
    public class RegisterTestCase
    {
        public string TestCaseId { get; set; }
        public string Description { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public bool? AcceptTerms { get; set; }
        public string ExpectedResult { get; set; }
        public string ExpectedMessage { get; set; }
        public string Priority { get; set; }
        public List<string> Tags { get; set; } = new();

        public override string ToString()
        {
            return $"[{TestCaseId}] {Description}";
        }
    }

    /// <summary>
    /// Data model for UI visibility test cases
    /// </summary>
    public class UIVisibilityTestCase
    {
        public string TestCaseId { get; set; }
        public string Description { get; set; }
        public string PageName { get; set; }
        public List<string> ExpectedElements { get; set; } = new();
        public string Priority { get; set; }
    }

    /// <summary>
    /// Data model for Navigation test cases
    /// </summary>
    public class NavigationTestCase
    {
        public string TestCaseId { get; set; }
        public string Description { get; set; }
        public string StartPage { get; set; }
        public string ActionDescription { get; set; }
        public string ExpectedUrl { get; set; }
        public string Priority { get; set; }
    }

    /// <summary>
    /// Data model for Chat test cases
    /// </summary>
    public class ChatTestCase
    {
        public string TestCaseId { get; set; }
        public string Description { get; set; }
        public string Scenario { get; set; }
        public string OrderId { get; set; }
        public string MessageContent { get; set; }
        public string RecipientUser { get; set; }
        public string MessageType { get; set; } // text, image, file, emoji
        public string ExpectedResult { get; set; }
        public string ExpectedMessage { get; set; }
        public string Priority { get; set; }
        public List<string> Tags { get; set; } = new();

        public override string ToString()
        {
            return $"[{TestCaseId}] {Description}";
        }
    }

    /// <summary>
    /// Data model for Notification test cases
    /// </summary>
    public class NotificationTestCase
    {
        public string TestCaseId { get; set; }
        public string Description { get; set; }
        public string NotificationType { get; set; }
        public string OrderId { get; set; }
        public string Message { get; set; }
        public string RecipientRole { get; set; }
        public string ExpectedResult { get; set; }
        public string ExpectedMessage { get; set; }
        public string Priority { get; set; }
        public List<string> Tags { get; set; } = new();

        public override string ToString()
        {
            return $"[{TestCaseId}] {Description}";
        }
    }

    /// <summary>
    /// Data model for Profile test cases
    /// </summary>
    public class ProfileTestCase
    {
        public string TestCaseId { get; set; }
        public string Description { get; set; }
        public string Action { get; set; } // UpdateInfo, ChangePassword
        public string FieldName { get; set; }
        public string FieldValue { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
        public string ExpectedResult { get; set; }
        public string ExpectedMessage { get; set; }
        public string Priority { get; set; }
        public List<string> Tags { get; set; } = new();

        public override string ToString()
        {
            return $"[{TestCaseId}] {Description}";
        }
    }

    /// <summary>
    /// Data model for Translate/I18n test cases
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
        public string ExpectedMessage { get; set; }
        public string Priority { get; set; }
        public List<string> Tags { get; set; } = new();

        public override string ToString()
        {
            return $"[{TestCaseId}] {Description}";
        }
    }

    /// <summary>
    /// Data model for ChatBot AI test cases
    /// </summary>
    public class ChatBotAITestCase
    {
        public string TestCaseId { get; set; }
        public string Scope { get; set; } // "Chatbot"
        public string Functionality { get; set; }
        public string Description { get; set; }
        public string UserInput { get; set; }
        public string Intent { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new();
        public string ExpectedResult { get; set; }
        public string ExpectedResponse { get; set; }
        public List<string> ShouldContain { get; set; } = new();
        public string Priority { get; set; }
        public List<string> Tags { get; set; } = new();

        public override string ToString()
        {
            return $"[{TestCaseId}] {Description}";
        }
    }
}


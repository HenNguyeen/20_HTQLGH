using System;
using System.Collections.Generic;

namespace TestSelenium.Models
{
    /// <summary>
    /// Extended Test Data Models for Multi-Module Framework
    /// =====================================================
    /// Contains data models for Order, Employee, and Customer modules.
    /// Authentication models (LoginTestCase, RegisterTestCase) are defined in TestDataModels.cs
    /// </summary>

    // ============================================
    // ORDER TEST DATA MODELS
    // ============================================

    public class OrderTestCase
    {
        public string TestCaseId { get; set; }
        public string Description { get; set; }
        public string Action { get; set; } // Create, Update, Delete, Filter
        
        // ===== CREATE ORDER FIELDS =====
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerEmail { get; set; }
        public string DeliveryAddress { get; set; }
        public string Province { get; set; }
        public string District { get; set; }
        public string Ward { get; set; }
        
        // Package Details
        public int? PackageType { get; set; }
        public string PackageDescription { get; set; }
        public decimal? PackageWeight { get; set; }
        public string PackageSize { get; set; }  // Kích Thước (LxWxH cm)
        public decimal? EstimatedDistance { get; set; }
        
        // Special Flags
        public bool? IsFragile { get; set; }
        public bool? IsValuable { get; set; }
        public bool? IsVehicle { get; set; }  // Hàng Là Xe
        public bool? CollectMoney { get; set; }
        public decimal? CollectionAmount { get; set; }
        
        // Delivery Info
        public string PaymentMethod { get; set; }
        public string DeliveryType { get; set; }
        public string Notes { get; set; }
        
        // ===== FILTER FIELDS =====
        public string Status { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        
        // ===== DELETE FIELDS =====
        public string OrderId { get; set; }
        public bool? ConfirmDelete { get; set; }
        
        // ===== AUTHORIZATION =====
        public string UserRole { get; set; }
        
        // ===== EXPECTED RESULTS =====
        public string ExpectedResult { get; set; }
        public string ExpectedMessage { get; set; }
        public string Priority { get; set; }
        public List<string> Tags { get; set; } = new();
        
        public override string ToString()
        {
            return $"[{TestCaseId}] {Description}";
        }
    }

    // ============================================
    // EMPLOYEE TEST DATA MODELS
    // ============================================

    public class EmployeeTestCase
    {
        public string TestCaseId { get; set; }
        public string Description { get; set; }
        public string Action { get; set; } // Create, Update, Delete
        
        // Employee Fields
        public string EmployeeId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Role { get; set; }
        public string Salary { get; set; }
        public string StartDate { get; set; }
        public string Status { get; set; }
        
        // Expected Results
        public string ExpectedResult { get; set; }
        public string ExpectedMessage { get; set; }
        public string Priority { get; set; }
        public List<string> Tags { get; set; }
    }

    // ============================================
    // CUSTOMER TEST DATA MODELS
    // ============================================

    public class CustomerTestCase
    {
        public string TestCaseId { get; set; }
        public string Description { get; set; }
        public string Action { get; set; } // Create, Update, Delete
        
        // Customer Fields - Định Danh
        public string CustomerId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        // Customer Fields - Địa Chỉ
        public string Address { get; set; }
        public string Ward { get; set; }
        public string District { get; set; }
        public string City { get; set; }
        public string AddressType { get; set; } // Kho hàng, Nhà riêng, Văn phòng

        // Customer Fields - Công Ty
        public string CompanyName { get; set; }

        // Customer Fields - Tài Chính & Đối Soát
        public string BankAccountNumber { get; set; }
        public string BankAccountName { get; set; }
        public string BankName { get; set; }
        public string BankBranch { get; set; }
        public string SettlementCycle { get; set; } // Daily, Weekly, Monthly, OnDemand, MinimumBalance
        public string TaxCode { get; set; }
        
        // Expected Results
        public string ExpectedResult { get; set; }
        public string ExpectedMessage { get; set; }
        public string Priority { get; set; }
        public List<string> Tags { get; set; }
    }

    // ============================================
    // GENERIC TEST DATA MODELS
    // ============================================

    public class TestDataContainer<T>
    {
        public List<T> TestCases { get; set; } = new List<T>();
    }

    public class MultiModuleTestData
    {
        public List<LoginTestCase> LoginTestCases { get; set; }
        public List<RegisterTestCase> RegisterTestCases { get; set; }
        public List<OrderTestCase> OrderTestCases { get; set; }
        public List<EmployeeTestCase> EmployeeTestCases { get; set; }
        public List<CustomerTestCase> CustomerTestCases { get; set; }
    }
}

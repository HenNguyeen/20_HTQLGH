package com.example.shipperapp.models;

public class UpdateOrderStatusDto {
    public String orderId = "";
    public String staffId = "";
    public String status = ""; // use enum values from API (e.g., "DaGiao")
    public String notes = "";
}

package com.example.shipperapp.models;

public class UpdateOrderStatusDto {
    public String OrderId = "";
    public String Status = ""; // use enum values from API (e.g., "DaGiao")
    public String StaffId = "";
    public String Notes = "";
}

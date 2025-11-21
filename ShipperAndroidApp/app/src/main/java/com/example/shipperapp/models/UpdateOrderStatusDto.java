package com.example.shipperapp.models;

public class UpdateOrderStatusDto {
    public String orderId = "";
    public String staffId = "";
    public int status = 0; // numeric value matching OrderStatus enum on server
    public String notes = "";
}

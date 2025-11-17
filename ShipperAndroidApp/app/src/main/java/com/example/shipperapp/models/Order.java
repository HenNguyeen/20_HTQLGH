package com.example.shipperapp.models;

import java.util.List;

public class Order {
    public int orderId;
    public String orderCode = "";
    public String status = ""; // map to OrderStatus string
    public DeliveryStaff assignedStaff;
    public List<LocationCheckpoint> checkpoints;
}

package com.example.shipperapp.models;

import java.util.List;

public class Order {
    public int OrderId;
    public String OrderCode = "";
    public String Status = ""; // map to OrderStatus string
    public DeliveryStaff AssignedStaff;
    public List<LocationCheckpoint> Checkpoints;
}

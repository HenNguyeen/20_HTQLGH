package com.example.shipperapp.models;

public class LocationCheckpoint {
    public int checkpointId;
    public int orderId;
    public double latitude;
    public double longitude;
    public String locationName = "";
    public String checkInTime = ""; // ISO string
    public String notes = "";
}

package com.example.shipperapp.models;

public class LocationCheckpoint {
    public int CheckpointId;
    public int OrderId;
    public double Latitude;
    public double Longitude;
    public String Note = "";
    public String CheckInTime = ""; // ISO string
}

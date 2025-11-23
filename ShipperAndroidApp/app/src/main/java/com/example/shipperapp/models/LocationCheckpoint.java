package com.example.shipperapp.models;

import com.google.gson.annotations.SerializedName;

public class LocationCheckpoint {
    @SerializedName("checkpointId")
    public int checkpointId;
    
    @SerializedName("orderId")
    public int orderId;
    
    @SerializedName("latitude")
    public double latitude;
    
    @SerializedName("longitude")
    public double longitude;
    
    @SerializedName("locationName")
    public String locationName;
    
    // checkInTime: Backend tự set, không gửi từ client
    @SerializedName("checkInTime")
    public String checkInTime; // Chỉ dùng khi nhận response
    
    @SerializedName("notes")
    public String notes;
    
    // Constructor for sending check-in
    public LocationCheckpoint() {
        this.locationName = "";
        this.notes = "";
    }
}

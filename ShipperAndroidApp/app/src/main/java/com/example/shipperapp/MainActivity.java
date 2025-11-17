package com.example.shipperapp;

import android.os.Bundle;
import androidx.appcompat.app.AppCompatActivity;
import android.util.Log;

import com.example.shipperapp.api.ApiService;
import com.example.shipperapp.api.RetrofitClient;
import com.example.shipperapp.models.DeliveryStaff;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class MainActivity extends AppCompatActivity {
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        // No layout included; this is a minimal example focusing on networking

        ApiService api = RetrofitClient.getApiServiceWithAuth(this, "http://10.0.2.2:5221/");

        // Example: get current shipper info (requires Authorization header saved by Login)
        Call<DeliveryStaff> call = api.getMyStaff();
        call.enqueue(new Callback<DeliveryStaff>() {
            @Override
            public void onResponse(Call<DeliveryStaff> call, Response<DeliveryStaff> response) {
                if (response.isSuccessful()) {
                    DeliveryStaff staff = response.body();
                    Log.i("ShipperApp", "My staff: " + (staff != null ? staff.fullName : "null"));
                } else {
                    Log.e("ShipperApp", "Failed: " + response.code());
                }
            }

            @Override
            public void onFailure(Call<DeliveryStaff> call, Throwable t) {
                Log.e("ShipperApp", "Error: " + t.getMessage());
            }
        });
    }
}

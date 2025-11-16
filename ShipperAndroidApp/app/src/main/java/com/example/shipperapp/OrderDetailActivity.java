package com.example.shipperapp;

import android.os.Bundle;
import android.view.View;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Spinner;
import android.widget.TextView;
import android.util.Log;
import androidx.appcompat.app.AppCompatActivity;

import com.example.shipperapp.api.RetrofitClient;
import com.example.shipperapp.api.ApiService;
import com.example.shipperapp.models.LocationCheckpoint;
import com.example.shipperapp.models.Order;
import com.example.shipperapp.models.UpdateOrderStatusDto;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

import java.util.Arrays;
import java.util.List;

public class OrderDetailActivity extends AppCompatActivity {
    private final String BASE_URL = "http://10.0.2.2:5000/";
    private int orderId;

    private TextView tvOrderCode, tvStatus;
    private Spinner spinnerStatus;
    private Button btnUpdateStatus, btnCheckIn;
    private EditText etLat, etLng, etNote;

    private final List<String> statuses = Arrays.asList("ChuaNhan", "DaNhanChuaGiao", "DaNhanDangGiao", "DaGiao");

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_order_detail);

        orderId = getIntent().getIntExtra("orderId", -1);

        tvOrderCode = findViewById(R.id.tvOrderCode);
        tvStatus = findViewById(R.id.tvStatus);
        spinnerStatus = findViewById(R.id.spinnerStatus);
        btnUpdateStatus = findViewById(R.id.btnUpdateStatus);

        etLat = findViewById(R.id.etLat);
        etLng = findViewById(R.id.etLng);
        etNote = findViewById(R.id.etNote);
        btnCheckIn = findViewById(R.id.btnCheckIn);

        ArrayAdapter<String> spinnerAdapter = new ArrayAdapter<>(this, android.R.layout.simple_spinner_item, statuses);
        spinnerAdapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
        spinnerStatus.setAdapter(spinnerAdapter);

        loadOrder();

        btnUpdateStatus.setOnClickListener(v -> updateStatus());
        btnCheckIn.setOnClickListener(v -> doCheckIn());
    }

    private void loadOrder() {
        ApiService api = RetrofitClient.getApiServiceWithAuth(this, BASE_URL);
        Call<Order> call = api.getOrderById(orderId);
        call.enqueue(new Callback<Order>() {
            @Override
            public void onResponse(Call<Order> call, Response<Order> response) {
                if (response.isSuccessful() && response.body() != null) {
                    Order o = response.body();
                    tvOrderCode.setText(o.OrderCode != null ? o.OrderCode : String.valueOf(o.OrderId));
                    tvStatus.setText(o.Status != null ? o.Status : "");
                } else {
                    Log.e("OrderDetail", "Failed load order: " + response.code());
                }
            }

            @Override
            public void onFailure(Call<Order> call, Throwable t) {
                Log.e("OrderDetail", "Error", t);
            }
        });
    }

    private void updateStatus() {
        String sel = (String) spinnerStatus.getSelectedItem();
        UpdateOrderStatusDto dto = new UpdateOrderStatusDto();
        dto.OrderId = String.valueOf(orderId);
        dto.Status = sel;
        dto.StaffId = "";
        dto.Notes = "Cập nhật từ app";

        ApiService api = RetrofitClient.getApiServiceWithAuth(this, BASE_URL);
        Call<Order> call = api.updateOrderStatus(orderId, dto);
        call.enqueue(new Callback<Order>() {
            @Override
            public void onResponse(Call<Order> call, Response<Order> response) {
                if (response.isSuccessful() && response.body() != null) {
                    tvStatus.setText(response.body().Status != null ? response.body().Status : "");
                } else {
                    Log.e("OrderDetail", "Failed update status: " + response.code());
                }
            }

            @Override
            public void onFailure(Call<Order> call, Throwable t) {
                Log.e("OrderDetail", "Error", t);
            }
        });
    }

    private void doCheckIn() {
        String latS = etLat.getText().toString().trim();
        String lngS = etLng.getText().toString().trim();
        String note = etNote.getText().toString().trim();
        if (latS.isEmpty() || lngS.isEmpty()) {
            android.widget.Toast.makeText(this, "Vui lòng nhập lat/lng", android.widget.Toast.LENGTH_SHORT).show();
            return;
        }

        double lat = Double.parseDouble(latS);
        double lng = Double.parseDouble(lngS);

        LocationCheckpoint cp = new LocationCheckpoint();
        cp.OrderId = orderId;
        cp.Latitude = lat;
        cp.Longitude = lng;
        cp.Note = note;

        ApiService api = RetrofitClient.getApiServiceWithAuth(this, BASE_URL);
        Call<LocationCheckpoint> call = api.checkIn(cp);
        call.enqueue(new Callback<LocationCheckpoint>() {
            @Override
            public void onResponse(Call<LocationCheckpoint> call, Response<LocationCheckpoint> response) {
                if (response.isSuccessful() && response.body() != null) {
                    android.widget.Toast.makeText(OrderDetailActivity.this, "Check-in thành công", android.widget.Toast.LENGTH_SHORT).show();
                } else {
                    Log.e("OrderDetail", "Failed check-in: " + response.code());
                    android.widget.Toast.makeText(OrderDetailActivity.this, "Thất bại: " + response.code(), android.widget.Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<LocationCheckpoint> call, Throwable t) {
                Log.e("OrderDetail", "Error", t);
            }
        });
    }
}

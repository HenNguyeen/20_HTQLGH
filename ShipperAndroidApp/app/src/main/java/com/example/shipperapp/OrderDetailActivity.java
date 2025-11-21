package com.example.shipperapp;

import android.Manifest;
import android.content.pm.PackageManager;
import android.location.Location;
import android.os.Bundle;
import android.view.View;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Spinner;
import android.widget.TextView;
import android.util.Log;
import android.widget.Toast;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.app.ActivityCompat;
import androidx.core.content.ContextCompat;

import com.google.android.gms.location.FusedLocationProviderClient;
import com.google.android.gms.location.LocationServices;
import android.os.Handler;
import android.widget.Switch;

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
import android.util.Base64;
import org.json.JSONObject;

public class OrderDetailActivity extends AppCompatActivity {
    private final String BASE_URL = "http://10.0.2.2:5221/";
    private int orderId;

    private TextView tvOrderCode, tvStatus;
    private Spinner spinnerStatus;
    private Button btnUpdateStatus, btnCheckIn;
    private EditText etLat, etLng, etNote;

    private final List<String> statuses = Arrays.asList("ChuaNhan", "DaNhanChuaGiao", "DaNhanDangGiao", "DaGiao");
    private FusedLocationProviderClient fusedLocationClient;
    private static final int REQ_LOCATION = 1001;
    private Switch switchAutoTrack;
    private Handler trackHandler = new Handler();
    private Location currentLocation = null;
    private final long TRACK_INTERVAL_MS = 2 * 60 * 1000; // 2 minutes
    private final Runnable trackRunnable = new Runnable() {
        @Override
        public void run() {
            // get location and post
            try {
                if (ContextCompat.checkSelfPermission(OrderDetailActivity.this, Manifest.permission.ACCESS_FINE_LOCATION) == PackageManager.PERMISSION_GRANTED) {
                    fusedLocationClient.getLastLocation().addOnSuccessListener(location -> {
                        if (location != null) {
                            // store current location and update UI fields
                            currentLocation = location;
                            runOnUiThread(() -> {
                                etLat.setText(String.valueOf(location.getLatitude()));
                                etLng.setText(String.valueOf(location.getLongitude()));
                            });
                            postCheckInWithLocation(location);
                        } else {
                            Log.d("OrderDetail", "trackRunnable: lastLocation null");
                        }
                        // schedule next
                        trackHandler.postDelayed(trackRunnable, TRACK_INTERVAL_MS);
                    }).addOnFailureListener(e -> {
                        Log.e("OrderDetail", "trackRunnable getLastLocation failed", e);
                        trackHandler.postDelayed(trackRunnable, TRACK_INTERVAL_MS);
                    });
                } else {
                    // request permission
                    ActivityCompat.requestPermissions(OrderDetailActivity.this, new String[]{Manifest.permission.ACCESS_FINE_LOCATION}, REQ_LOCATION);
                }
            } catch (SecurityException ex) {
                Log.e("OrderDetail", "trackRunnable SecurityException", ex);
            }
        }
    };

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_order_detail);

        // Log decoded JWT payload to help debug roles/claims
        try {
            String token = com.example.shipperapp.auth.AuthManager.getToken(this);
            if (token != null) {
                String[] parts = token.split("\\.");
                if (parts.length >= 2) {
                    String payload = parts[1];
                    byte[] decoded = Base64.decode(payload, Base64.URL_SAFE | Base64.NO_PADDING | Base64.NO_WRAP);
                    String json = new String(decoded);
                    Log.d("OrderDetail", "JWT payload: " + json);
                }
            }
        } catch (Exception ex) {
            Log.e("OrderDetail", "Failed decode token", ex);
        }

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

        fusedLocationClient = LocationServices.getFusedLocationProviderClient(this);
        switchAutoTrack = findViewById(R.id.switchAutoTrack);
        switchAutoTrack.setOnCheckedChangeListener((buttonView, isChecked) -> {
            if (isChecked) {
                // disable manual inputs while auto tracking
                etLat.setEnabled(false);
                etLng.setEnabled(false);
                startTracking();
            } else {
                stopTracking();
                etLat.setEnabled(true);
                etLng.setEnabled(true);
            }
        });

        loadOrder();

        btnUpdateStatus.setOnClickListener(v -> updateStatus());
        btnCheckIn.setOnClickListener(v -> doCheckIn());
    }

    private void startTracking() {
        // ensure permission
        if (ContextCompat.checkSelfPermission(this, Manifest.permission.ACCESS_FINE_LOCATION) != PackageManager.PERMISSION_GRANTED) {
            ActivityCompat.requestPermissions(this, new String[]{Manifest.permission.ACCESS_FINE_LOCATION}, REQ_LOCATION);
            return;
        }
        // get last known location immediately and update fields
        try {
            fusedLocationClient.getLastLocation().addOnSuccessListener(location -> {
                if (location != null) {
                    currentLocation = location;
                    runOnUiThread(() -> {
                        etLat.setText(String.valueOf(location.getLatitude()));
                        etLng.setText(String.valueOf(location.getLongitude()));
                    });
                }
            });
        } catch (SecurityException ex) {
            Log.e("OrderDetail", "startTracking SecurityException", ex);
        }

        trackHandler.removeCallbacks(trackRunnable);
        trackHandler.post(trackRunnable);
        Toast.makeText(this, "Bắt đầu tự động theo dõi vị trí", Toast.LENGTH_SHORT).show();
    }

    private void stopTracking() {
        trackHandler.removeCallbacks(trackRunnable);
        Toast.makeText(this, "Dừng theo dõi vị trí", Toast.LENGTH_SHORT).show();
    }

    private void loadOrder() {
        ApiService api = RetrofitClient.getApiServiceWithAuth(this, BASE_URL);
        Call<Order> call = api.getOrderById(orderId);
        call.enqueue(new Callback<Order>() {
            @Override
            public void onResponse(Call<Order> call, Response<Order> response) {
                if (response.isSuccessful() && response.body() != null) {
                    Order o = response.body();
                    tvOrderCode.setText(o.orderCode != null ? o.orderCode : String.valueOf(o.orderId));
                    tvStatus.setText(o.status != null ? o.status : "");
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
        int selPos = spinnerStatus.getSelectedItemPosition();
        UpdateOrderStatusDto dto = new UpdateOrderStatusDto();
        dto.orderId = String.valueOf(orderId);
        dto.status = selPos; // send numeric enum value matching server OrderStatus
        dto.staffId = "";
        dto.notes = "Cập nhật từ app";

        ApiService api = RetrofitClient.getApiServiceWithAuth(this, BASE_URL);
        Call<Order> call = api.updateOrderStatus(orderId, dto);
        call.enqueue(new Callback<Order>() {
            @Override
            public void onResponse(Call<Order> call, Response<Order> response) {
                if (response.isSuccessful() && response.body() != null) {
                    tvStatus.setText(response.body().status != null ? response.body().status : "");
                    // If status changed to "DaNhanDangGiao" -> attempt auto check-in
                    if ("DaNhanDangGiao".equals(response.body().status)) {
                        attemptAutoCheckIn();
                    }
                } else {
                    Log.e("OrderDetail", "Failed update status: " + response.code());
                    try {
                        if (response.errorBody() != null) {
                            String err = response.errorBody().string();
                            Log.e("OrderDetail", "updateStatus error body: " + err);
                        }
                    } catch (java.io.IOException ex) {
                        Log.e("OrderDetail", "Error reading updateStatus errorBody", ex);
                    }
                }
            }

            @Override
            public void onFailure(Call<Order> call, Throwable t) {
                Log.e("OrderDetail", "Error", t);
            }
        });
    }

    private void doCheckIn() {
        String note = etNote.getText().toString().trim();
        double lat, lng;
        if (currentLocation != null) {
            lat = currentLocation.getLatitude();
            lng = currentLocation.getLongitude();
        } else {
            String latS = etLat.getText().toString().trim();
            String lngS = etLng.getText().toString().trim();
            if (latS.isEmpty() || lngS.isEmpty()) {
                android.widget.Toast.makeText(this, "Vui lòng nhập lat/lng hoặc bật tự động theo dõi", android.widget.Toast.LENGTH_SHORT).show();
                return;
            }

            try {
                lat = Double.parseDouble(latS);
                lng = Double.parseDouble(lngS);
            } catch (NumberFormatException ex) {
                android.widget.Toast.makeText(this, "Lat/Lng không hợp lệ", android.widget.Toast.LENGTH_SHORT).show();
                return;
            }
        }

        LocationCheckpoint cp = new LocationCheckpoint();
        cp.orderId = orderId;
        cp.latitude = lat;
        cp.longitude = lng;
        cp.notes = note;

        ApiService api = RetrofitClient.getApiServiceWithAuth(this, BASE_URL);
        Call<LocationCheckpoint> call = api.checkIn(cp);
        call.enqueue(new Callback<LocationCheckpoint>() {
            @Override
            public void onResponse(Call<LocationCheckpoint> call, Response<LocationCheckpoint> response) {
                if (response.isSuccessful() && response.body() != null) {
                    android.widget.Toast.makeText(OrderDetailActivity.this, "Check-in thành công", android.widget.Toast.LENGTH_SHORT).show();
                } else {
                    Log.e("OrderDetail", "Failed check-in: " + response.code());
                    try {
                        if (response.errorBody() != null) {
                            String err = response.errorBody().string();
                            Log.e("OrderDetail", "check-in error body: " + err);
                            android.widget.Toast.makeText(OrderDetailActivity.this, "Thất bại: " + response.code() + " - " + (err.length() > 120 ? err.substring(0, 120) : err), android.widget.Toast.LENGTH_LONG).show();
                        } else {
                            android.widget.Toast.makeText(OrderDetailActivity.this, "Thất bại: " + response.code(), android.widget.Toast.LENGTH_SHORT).show();
                        }
                    } catch (java.io.IOException ex) {
                        Log.e("OrderDetail", "Error reading errorBody", ex);
                        android.widget.Toast.makeText(OrderDetailActivity.this, "Thất bại: " + response.code(), android.widget.Toast.LENGTH_SHORT).show();
                    }
                }
            }

            @Override
            public void onFailure(Call<LocationCheckpoint> call, Throwable t) {
                Log.e("OrderDetail", "Error", t);
            }
        });
    }

    // --- Automatic check-in helpers ---
    private void attemptAutoCheckIn() {
        // check permission
        if (ContextCompat.checkSelfPermission(this, Manifest.permission.ACCESS_FINE_LOCATION) != PackageManager.PERMISSION_GRANTED) {
            // request permission
            ActivityCompat.requestPermissions(this, new String[]{Manifest.permission.ACCESS_FINE_LOCATION}, REQ_LOCATION);
            return;
        }
        sendAutoCheckIn();
    }

    private void sendAutoCheckIn() {
        try {
            fusedLocationClient.getLastLocation().addOnSuccessListener(location -> {
                if (location != null) {
                    postCheckInWithLocation(location);
                } else {
                    Toast.makeText(this, "Không lấy được vị trí để check-in", Toast.LENGTH_SHORT).show();
                }
            }).addOnFailureListener(e -> {
                Log.e("OrderDetail", "getLastLocation failed", e);
                Toast.makeText(this, "Không lấy được vị trí: " + e.getMessage(), Toast.LENGTH_SHORT).show();
            });
        } catch (SecurityException ex) {
            Log.e("OrderDetail", "Location permission missing", ex);
        }
    }

    private void postCheckInWithLocation(Location location) {
        LocationCheckpoint cp = new LocationCheckpoint();
        cp.orderId = orderId;
        cp.latitude = location.getLatitude();
        cp.longitude = location.getLongitude();
        cp.notes = "Auto check-in on status change";

        ApiService api = RetrofitClient.getApiServiceWithAuth(this, BASE_URL);
        Call<LocationCheckpoint> call = api.checkIn(cp);
        call.enqueue(new Callback<LocationCheckpoint>() {
            @Override
            public void onResponse(Call<LocationCheckpoint> call, Response<LocationCheckpoint> response) {
                if (response.isSuccessful() && response.body() != null) {
                    Toast.makeText(OrderDetailActivity.this, "Auto check-in thành công", Toast.LENGTH_SHORT).show();
                } else {
                    Log.e("OrderDetail", "Auto check-in failed: " + response.code());
                    try {
                        if (response.errorBody() != null) {
                            String err = response.errorBody().string();
                            Log.e("OrderDetail", "auto check-in error body: " + err);
                        }
                    } catch (java.io.IOException ex) {
                        Log.e("OrderDetail", "Error reading auto check-in errorBody", ex);
                    }
                }
            }

            @Override
            public void onFailure(Call<LocationCheckpoint> call, Throwable t) {
                Log.e("OrderDetail", "Auto check-in error", t);
            }
        });
    }

    @Override
    public void onRequestPermissionsResult(int requestCode, String[] permissions, int[] grantResults) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults);
        if (requestCode == REQ_LOCATION) {
            if (grantResults.length > 0 && grantResults[0] == PackageManager.PERMISSION_GRANTED) {
                // permission granted, retry auto check-in
                sendAutoCheckIn();
            } else {
                Toast.makeText(this, "Quyền vị trí bị từ chối", Toast.LENGTH_SHORT).show();
            }
        }
    }
}

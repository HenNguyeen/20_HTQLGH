package com.example.shipperapp;

import android.Manifest;
import android.content.pm.PackageManager;
import android.location.Location;
import android.os.Bundle;
import android.util.Log;
import android.widget.TextView;
import android.widget.Toast;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.app.ActivityCompat;
import androidx.core.content.ContextCompat;

import com.google.android.gms.location.FusedLocationProviderClient;
import com.google.android.gms.location.LocationServices;
import com.google.android.gms.maps.CameraUpdateFactory;
import com.google.android.gms.maps.GoogleMap;
import com.google.android.gms.maps.OnMapReadyCallback;
import com.google.android.gms.maps.SupportMapFragment;
import com.google.android.gms.maps.model.BitmapDescriptorFactory;
import com.google.android.gms.maps.model.LatLng;
import com.google.android.gms.maps.model.LatLngBounds;
import com.google.android.gms.maps.model.MarkerOptions;
import com.google.android.gms.maps.model.PolylineOptions;

import com.example.shipperapp.api.ApiService;
import com.example.shipperapp.api.RetrofitClient;
import com.example.shipperapp.models.Order;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class OrderMapActivity extends AppCompatActivity implements OnMapReadyCallback {
    private static final String TAG = "OrderMapActivity";
    private static final int REQ_LOCATION = 1001;
    private final String BASE_URL = "http://10.0.2.2:5221/";
    
    private GoogleMap mMap;
    private FusedLocationProviderClient fusedLocationClient;
    private int orderId;
    private Order orderData;
    
    private TextView tvOrderInfo, tvDistance;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_order_map);

        orderId = getIntent().getIntExtra("orderId", -1);
        tvOrderInfo = findViewById(R.id.tvOrderInfo);
        tvDistance = findViewById(R.id.tvDistance);

        fusedLocationClient = LocationServices.getFusedLocationProviderClient(this);

        // Khởi tạo map
        SupportMapFragment mapFragment = (SupportMapFragment) getSupportFragmentManager()
                .findFragmentById(R.id.map);
        if (mapFragment != null) {
            mapFragment.getMapAsync(this);
        }

        loadOrderData();
    }

    private void loadOrderData() {
        ApiService api = RetrofitClient.getApiServiceWithAuth(this, BASE_URL);
        Call<Order> call = api.getOrderById(orderId);
        call.enqueue(new Callback<Order>() {
            @Override
            public void onResponse(Call<Order> call, Response<Order> response) {
                if (response.isSuccessful() && response.body() != null) {
                    orderData = response.body();
                    displayOrderInfo();
                    if (mMap != null) {
                        showRouteOnMap();
                    }
                } else {
                    Log.e(TAG, "Failed to load order: " + response.code());
                }
            }

            @Override
            public void onFailure(Call<Order> call, Throwable t) {
                Log.e(TAG, "Error loading order", t);
                Toast.makeText(OrderMapActivity.this, "Không thể tải thông tin đơn hàng", Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void displayOrderInfo() {
        if (orderData != null && orderData.customer != null) {
            String info = "Đơn hàng: " + (orderData.orderCode != null ? orderData.orderCode : orderData.orderId) + "\n" +
                    "Khách hàng: " + orderData.customer.fullName + "\n" +
                    "SĐT: " + orderData.customer.phoneNumber + "\n" +
                    "Địa chỉ: " + orderData.customer.address;
            tvOrderInfo.setText(info);
        }
    }

    @Override
    public void onMapReady(GoogleMap googleMap) {
        mMap = googleMap;
        
        // Enable location if permission granted
        if (ContextCompat.checkSelfPermission(this, Manifest.permission.ACCESS_FINE_LOCATION) == PackageManager.PERMISSION_GRANTED) {
            mMap.setMyLocationEnabled(true);
        } else {
            ActivityCompat.requestPermissions(this, new String[]{Manifest.permission.ACCESS_FINE_LOCATION}, REQ_LOCATION);
        }

        if (orderData != null) {
            showRouteOnMap();
        }
    }

    private void showRouteOnMap() {
        if (mMap == null) return;

        // Lấy vị trí shipper
        if (ContextCompat.checkSelfPermission(this, Manifest.permission.ACCESS_FINE_LOCATION) == PackageManager.PERMISSION_GRANTED) {
            fusedLocationClient.getLastLocation().addOnSuccessListener(location -> {
                if (location != null) {
                    LatLng shipperPos = new LatLng(location.getLatitude(), location.getLongitude());
                    
                    // Vị trí khách hàng (giả lập - cách ~3km)
                    // TODO: Tích hợp Geocoding API để convert địa chỉ → tọa độ
                    LatLng customerPos = new LatLng(location.getLatitude() + 0.03, location.getLongitude() + 0.02);
                    
                    // Marker shipper (xanh lá)
                    mMap.addMarker(new MarkerOptions()
                            .position(shipperPos)
                            .title("Vị trí của bạn")
                            .icon(BitmapDescriptorFactory.defaultMarker(BitmapDescriptorFactory.HUE_GREEN)));
                    
                    // Marker khách hàng (đỏ)
                    String customerTitle = orderData != null && orderData.customer != null 
                            ? orderData.customer.fullName 
                            : "Khách hàng";
                    mMap.addMarker(new MarkerOptions()
                            .position(customerPos)
                            .title(customerTitle)
                            .snippet(orderData != null && orderData.customer != null ? orderData.customer.address : "")
                            .icon(BitmapDescriptorFactory.defaultMarker(BitmapDescriptorFactory.HUE_RED)));
                    
                    // Vẽ đường đi
                    mMap.addPolyline(new PolylineOptions()
                            .add(shipperPos, customerPos)
                            .width(10)
                            .color(0xFF0000FF)); // Blue
                    
                    // Zoom để hiển thị cả 2 điểm
                    LatLngBounds.Builder builder = new LatLngBounds.Builder();
                    builder.include(shipperPos);
                    builder.include(customerPos);
                    LatLngBounds bounds = builder.build();
                    mMap.animateCamera(CameraUpdateFactory.newLatLngBounds(bounds, 150));
                    
                    // Tính khoảng cách
                    float[] results = new float[1];
                    Location.distanceBetween(
                            shipperPos.latitude, shipperPos.longitude,
                            customerPos.latitude, customerPos.longitude,
                            results);
                    float distanceKm = results[0] / 1000;
                    tvDistance.setText(String.format("Khoảng cách: %.2f km", distanceKm));
                } else {
                    Toast.makeText(this, "Không lấy được vị trí", Toast.LENGTH_SHORT).show();
                }
            });
        }
    }

    @Override
    public void onRequestPermissionsResult(int requestCode, String[] permissions, int[] grantResults) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults);
        if (requestCode == REQ_LOCATION) {
            if (grantResults.length > 0 && grantResults[0] == PackageManager.PERMISSION_GRANTED) {
                if (mMap != null) {
                    try {
                        mMap.setMyLocationEnabled(true);
                        showRouteOnMap();
                    } catch (SecurityException e) {
                        Log.e(TAG, "Permission error", e);
                    }
                }
            }
        }
    }
}

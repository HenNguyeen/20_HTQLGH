package com.example.shipperapp;

import android.os.Bundle;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import android.util.Log;
import android.view.View;
import android.widget.ProgressBar;
import android.widget.TextView;
import androidx.swiperefreshlayout.widget.SwipeRefreshLayout;
import com.example.shipperapp.auth.AuthManager;
import androidx.appcompat.app.AppCompatActivity;

import com.example.shipperapp.api.RetrofitClient;
import com.example.shipperapp.api.ApiService;
import com.example.shipperapp.models.Order;

import java.util.ArrayList;
import java.util.List;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class OrderListActivity extends AppCompatActivity {
    private final String BASE_URL = "http://10.0.2.2:5221/";
    private RecyclerView recyclerView;
    private SwipeRefreshLayout swipeRefresh;
    private ProgressBar progressLoading;
    private TextView tvEmpty;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_order_list);

        recyclerView = findViewById(R.id.recyclerViewOrders);
        recyclerView.setLayoutManager(new LinearLayoutManager(this));

        // Log current saved token for debugging (truncated)
        String _token = AuthManager.getToken(OrderListActivity.this);
        if (_token == null) {
            Log.d("OrderList", "Saved token: null");
        } else {
            String t = _token.length() > 40 ? _token.substring(0,40) + "..." : _token;
            Log.d("OrderList", "Saved token: " + t);
        }

        swipeRefresh = findViewById(R.id.swipeRefresh);
        progressLoading = findViewById(R.id.progressLoading);
        tvEmpty = findViewById(R.id.tvEmpty);

        ApiService api = RetrofitClient.getApiServiceWithAuth(this, BASE_URL);

        // Pull-to-refresh handler
        swipeRefresh.setOnRefreshListener(() -> fetchOrders());

        // Initial load
        fetchOrders();
    }

    private void setLoading(boolean loading) {
        runOnUiThread(() -> {
            if (loading) {
                progressLoading.setVisibility(View.VISIBLE);
            } else {
                progressLoading.setVisibility(View.GONE);
                swipeRefresh.setRefreshing(false);
            }
        });
    }

    private void fetchOrders() {
        setLoading(true);
        ApiService api = RetrofitClient.getApiServiceWithAuth(this, BASE_URL);

        Call<com.example.shipperapp.models.DeliveryStaff> staffCall = api.getMyStaff();
        staffCall.enqueue(new Callback<com.example.shipperapp.models.DeliveryStaff>() {
            @Override
            public void onResponse(Call<com.example.shipperapp.models.DeliveryStaff> call, Response<com.example.shipperapp.models.DeliveryStaff> response) {
                if (response.isSuccessful() && response.body() != null) {
                    int staffId = response.body().staffId;
                    Log.d("OrderList", "deliverystaff/me successful - StaffId=" + staffId + ", FullName=" + response.body().fullName);
                    // fetch orders by staff
                    Call<List<Order>> ordersCall = api.getOrdersByStaff(staffId);
                    ordersCall.enqueue(new Callback<List<Order>>() {
                        @Override
                        public void onResponse(Call<List<Order>> call, Response<List<Order>> response) {
                            setLoading(false);
                            if (response.isSuccessful() && response.body() != null) {
                                populateList(response.body());
                            } else {
                                try {
                                    String err = response.errorBody() != null ? response.errorBody().string() : "<no body>";
                                    Log.e("OrderList", "Failed to get orders by staff: code=" + response.code() + ", body=" + err);
                                } catch (Exception ex) {
                                    Log.e("OrderList", "Failed to get orders by staff: " + response.code(), ex);
                                }
                                populateList(new java.util.ArrayList<>());
                            }
                        }

                        @Override
                        public void onFailure(Call<List<Order>> call, Throwable t) {
                            setLoading(false);
                            Log.e("OrderList", "Error getting orders by staff", t);
                            populateList(new java.util.ArrayList<>());
                        }
                    });
                } else {
                    // Not a staff (or not found) - fallback to "my orders" (creator)
                    Call<List<Order>> myCall = api.getMyOrders();
                    myCall.enqueue(new Callback<List<Order>>() {
                        @Override
                        public void onResponse(Call<List<Order>> call, Response<List<Order>> response) {
                            setLoading(false);
                            if (response.isSuccessful() && response.body() != null) {
                                populateList(response.body());
                            } else {
                                Log.e("OrderList", "Failed to get my orders: " + response.code());
                                populateList(new java.util.ArrayList<>());
                            }
                        }

                        @Override
                        public void onFailure(Call<List<Order>> call, Throwable t) {
                            setLoading(false);
                            Log.e("OrderList", "Error getting my orders", t);
                            populateList(new java.util.ArrayList<>());
                        }
                    });
                }
            }

            @Override
            public void onFailure(Call<com.example.shipperapp.models.DeliveryStaff> call, Throwable t) {
                setLoading(false);
                Log.e("OrderList", "Error checking staff record", t);
                // fallback to my orders
                Call<List<Order>> myCall = api.getMyOrders();
                myCall.enqueue(new Callback<List<Order>>() {
                    @Override
                    public void onResponse(Call<List<Order>> call, Response<List<Order>> response) {
                        setLoading(false);
                        if (response.isSuccessful() && response.body() != null) {
                            populateList(response.body());
                        } else {
                            try {
                                String err = response.errorBody() != null ? response.errorBody().string() : "<no body>";
                                Log.e("OrderList", "Failed to get my orders: code=" + response.code() + ", body=" + err);
                            } catch (Exception ex) {
                                Log.e("OrderList", "Failed to get my orders: " + response.code(), ex);
                            }
                            populateList(new java.util.ArrayList<>());
                        }
                    }

                    @Override
                    public void onFailure(Call<List<Order>> call, Throwable t) {
                        setLoading(false);
                        Log.e("OrderList", "Error getting my orders", t);
                        populateList(new java.util.ArrayList<>());
                    }
                });
            }
        });
    }

    private void populateList(List<Order> orders) {
        runOnUiThread(() -> {
            if (orders == null || orders.isEmpty()) {
                tvEmpty.setVisibility(View.VISIBLE);
                recyclerView.setVisibility(View.GONE);
            } else {
                tvEmpty.setVisibility(View.GONE);
                recyclerView.setVisibility(View.VISIBLE);
            }
            com.example.shipperapp.adapter.OrderAdapter adapter = new com.example.shipperapp.adapter.OrderAdapter(orders, order -> {
            // open OrderDetailActivity
            android.content.Intent i = new android.content.Intent(OrderListActivity.this, OrderDetailActivity.class);
            i.putExtra("orderId", order.orderId);
            startActivity(i);
        });
        recyclerView.setAdapter(adapter);
        });
    }
}

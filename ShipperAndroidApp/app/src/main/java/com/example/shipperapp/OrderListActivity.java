package com.example.shipperapp;

import android.os.Bundle;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import android.util.Log;
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
    private final String BASE_URL = "http://10.0.2.2:5000/";
    private RecyclerView recyclerView;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_order_list);

        recyclerView = findViewById(R.id.recyclerViewOrders);
        recyclerView.setLayoutManager(new LinearLayoutManager(this));

        ApiService api = RetrofitClient.getApiServiceWithAuth(this, BASE_URL);
        Call<List<Order>> call = api.getMyOrders();
        call.enqueue(new Callback<List<Order>>() {
            @Override
            public void onResponse(Call<List<Order>> call, Response<List<Order>> response) {
                if (response.isSuccessful() && response.body() != null) {
                    List<Order> orders = response.body();
                    populateList(orders);
                } else {
                    Log.e("OrderList", "Failed to get orders: " + response.code());
                }
            }

            @Override
            public void onFailure(Call<List<Order>> call, Throwable t) {
                Log.e("OrderList", "Error", t);
            }
        });
    }

    private void populateList(List<Order> orders) {
        com.example.shipperapp.adapter.OrderAdapter adapter = new com.example.shipperapp.adapter.OrderAdapter(orders, order -> {
            // open OrderDetailActivity
            android.content.Intent i = new android.content.Intent(OrderListActivity.this, OrderDetailActivity.class);
            i.putExtra("orderId", order.OrderId);
            startActivity(i);
        });
        recyclerView.setAdapter(adapter);
    }
}

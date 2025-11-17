package com.example.shipperapp.api;

import com.example.shipperapp.models.DeliveryStaff;
import com.example.shipperapp.models.Order;
import com.example.shipperapp.models.LocationCheckpoint;
import com.example.shipperapp.models.UpdateOrderStatusDto;

import java.util.List;

import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.GET;
import retrofit2.http.POST;
import retrofit2.http.PATCH;
import retrofit2.http.Path;

public interface ApiService {
    @GET("api/deliverystaff/me")
    Call<DeliveryStaff> getMyStaff();

    @GET("api/orders/my")
    Call<List<Order>> getMyOrders();

    @GET("api/orders/staff/{staffId}")
    Call<List<Order>> getOrdersByStaff(@Path("staffId") int staffId);

    @PATCH("api/orders/{id}/status")
    Call<Order> updateOrderStatus(@Path("id") int orderId, @Body UpdateOrderStatusDto dto);

    @GET("api/tracking/order/{orderId}")
    Call<List<LocationCheckpoint>> getOrderCheckpoints(@Path("orderId") int orderId);

    @POST("api/tracking/checkin")
    Call<LocationCheckpoint> checkIn(@Body LocationCheckpoint checkpoint);

    @GET("api/orders/{id}")
    Call<Order> getOrderById(@Path("id") int id);
}

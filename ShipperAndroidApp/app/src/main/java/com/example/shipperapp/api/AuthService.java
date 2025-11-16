package com.example.shipperapp.api;

import com.example.shipperapp.models.LoginRequest;
import com.example.shipperapp.models.LoginResponse;

import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.POST;

public interface AuthService {
    @POST("api/auth/login")
    Call<LoginResponse> login(@Body LoginRequest req);
}

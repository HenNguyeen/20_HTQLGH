package com.example.shipperapp.api;

import android.content.Context;

import okhttp3.OkHttpClient;
import okhttp3.logging.HttpLoggingInterceptor;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;
import com.example.shipperapp.auth.AuthInterceptor;

public class RetrofitClient {
    // Retrofit without auth interceptor (for login)
    public static Retrofit getRetrofit(String baseUrl) {
    HttpLoggingInterceptor logging = new HttpLoggingInterceptor();
    logging.setLevel(HttpLoggingInterceptor.Level.BODY);

    OkHttpClient client = new OkHttpClient.Builder()
        .addInterceptor(logging)
        .build();

    return new Retrofit.Builder()
        .baseUrl(baseUrl)
        .addConverterFactory(GsonConverterFactory.create())
        .client(client)
        .build();
    }

    // ApiService with auth interceptor that reads token from SharedPreferences via AuthManager
    public static ApiService getApiServiceWithAuth(Context context, String baseUrl) {
    HttpLoggingInterceptor logging = new HttpLoggingInterceptor();
    logging.setLevel(HttpLoggingInterceptor.Level.BODY);

    AuthInterceptor authInterceptor = new AuthInterceptor(context);

    OkHttpClient client = new OkHttpClient.Builder()
        .addInterceptor(authInterceptor)
        .addInterceptor(logging)
        .build();

    Retrofit retrofit = new Retrofit.Builder()
        .baseUrl(baseUrl)
        .addConverterFactory(GsonConverterFactory.create())
        .client(client)
        .build();

    return retrofit.create(ApiService.class);
    }
}

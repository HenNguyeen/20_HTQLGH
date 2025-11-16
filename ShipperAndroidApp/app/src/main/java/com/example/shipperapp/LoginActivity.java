package com.example.shipperapp;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;
import androidx.appcompat.app.AppCompatActivity;
import android.util.Log;

import com.example.shipperapp.api.AuthService;
import com.example.shipperapp.api.RetrofitClient;
import com.example.shipperapp.models.LoginRequest;
import com.example.shipperapp.models.LoginResponse;
import com.example.shipperapp.auth.AuthManager;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class LoginActivity extends AppCompatActivity {
    private EditText etUsername, etPassword;
    private Button btnLogin;
    private TextView tvMessage;

    private final String BASE_URL = "http://10.0.2.2:5000/";

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_login);

        etUsername = findViewById(R.id.etUsername);
        etPassword = findViewById(R.id.etPassword);
        btnLogin = findViewById(R.id.btnLogin);
        tvMessage = findViewById(R.id.tvMessage);

        btnLogin.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                performLogin();
            }
        });
    }

    private void performLogin() {
        String username = etUsername.getText().toString().trim();
        String password = etPassword.getText().toString().trim();

        if (username.isEmpty() || password.isEmpty()) {
            tvMessage.setText("Vui lòng nhập tên đăng nhập và mật khẩu");
            tvMessage.setVisibility(View.VISIBLE);
            return;
        }

        AuthService auth = RetrofitClient.getRetrofit(BASE_URL).create(AuthService.class);
        LoginRequest req = new LoginRequest(username, password);
        Call<LoginResponse> call = auth.login(req);
        call.enqueue(new Callback<LoginResponse>() {
            @Override
            public void onResponse(Call<LoginResponse> call, Response<LoginResponse> response) {
                if (response.isSuccessful() && response.body() != null) {
                    String token = response.body().token;
                    AuthManager.saveToken(LoginActivity.this, token);
                    // open orders list
                    Intent i = new Intent(LoginActivity.this, OrderListActivity.class);
                    startActivity(i);
                    finish();
                } else {
                    tvMessage.setText("Đăng nhập thất bại: " + response.code());
                    tvMessage.setVisibility(View.VISIBLE);
                }
            }

            @Override
            public void onFailure(Call<LoginResponse> call, Throwable t) {
                tvMessage.setText("Lỗi: " + t.getMessage());
                tvMessage.setVisibility(View.VISIBLE);
                Log.e("Login", "Error", t);
            }
        });
    }
}

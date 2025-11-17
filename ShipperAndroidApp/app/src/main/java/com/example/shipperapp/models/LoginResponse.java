package com.example.shipperapp.models;

public class LoginResponse {
    public String token;
    public User user;

    public static class User {
        public int userId;
        public String username;
        public String fullName;
        public String email;
        public String role;
    }
}

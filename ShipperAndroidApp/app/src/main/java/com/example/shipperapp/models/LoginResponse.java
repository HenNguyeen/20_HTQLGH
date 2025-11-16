package com.example.shipperapp.models;

public class LoginResponse {
    public String token;
    public User user;

    public static class User {
        public int UserId;
        public String Username;
        public String FullName;
        public String Email;
        public String Role;
    }
}

package com.example.shipperapp.auth;

import android.content.Context;

public class AuthManager {
    private static final String PREFS = "shipper_prefs";
    private static final String KEY_TOKEN = "jwt_token";

    public static void saveToken(Context ctx, String token) {
        ctx.getSharedPreferences(PREFS, Context.MODE_PRIVATE)
                .edit()
                .putString(KEY_TOKEN, token)
                .apply();
    }

    public static String getToken(Context ctx) {
        return ctx.getSharedPreferences(PREFS, Context.MODE_PRIVATE)
                .getString(KEY_TOKEN, null);
    }

    public static void clearToken(Context ctx) {
        ctx.getSharedPreferences(PREFS, Context.MODE_PRIVATE)
                .edit()
                .remove(KEY_TOKEN)
                .apply();
    }
}

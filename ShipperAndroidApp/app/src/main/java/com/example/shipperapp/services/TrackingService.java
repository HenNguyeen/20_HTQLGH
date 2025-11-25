package com.example.shipperapp.services;

import android.util.Log;
import com.microsoft.signalr.HubConnection;
import com.microsoft.signalr.HubConnectionBuilder;
import com.microsoft.signalr.HubConnectionState;

public class TrackingService {
    private static final String TAG = "TrackingService";
    private HubConnection hubConnection;
    private String baseUrl;

    public TrackingService(String baseUrl) {
        this.baseUrl = baseUrl;
    }

    /**
     * Kết nối tới SignalR TrackingHub
     */
    public void connect() {
        if (hubConnection != null && hubConnection.getConnectionState() == HubConnectionState.CONNECTED) {
            Log.d(TAG, "Already connected");
            return;
        }

        String hubUrl = baseUrl + "trackingHub";
        Log.d(TAG, "Connecting to SignalR Hub: " + hubUrl);

        hubConnection = HubConnectionBuilder.create(hubUrl)
                .build();

        hubConnection.start()
                .doOnComplete(() -> Log.d(TAG, "✅ SignalR Connected"))
                .doOnError(error -> Log.e(TAG, "❌ SignalR Connection Error: " + error.getMessage()))
                .blockingAwait();
    }

    /**
     * Gửi vị trí shipper lên server để broadcast cho clients
     * @param staffId ID shipper
     * @param orderId ID đơn hàng
     * @param latitude Vĩ độ
     * @param longitude Kinh độ
     */
    public void sendShipperLocation(int staffId, int orderId, double latitude, double longitude) {
        if (hubConnection == null || hubConnection.getConnectionState() != HubConnectionState.CONNECTED) {
            Log.w(TAG, "SignalR not connected, attempting to connect...");
            connect();
        }

        if (hubConnection != null && hubConnection.getConnectionState() == HubConnectionState.CONNECTED) {
            try {
                hubConnection.send("UpdateShipperLocation", staffId, orderId, latitude, longitude);
                Log.d(TAG, "📍 Location sent: lat=" + latitude + ", lng=" + longitude);
            } catch (Exception e) {
                Log.e(TAG, "Error sending location: " + e.getMessage());
            }
        } else {
            Log.e(TAG, "Cannot send location - SignalR not connected");
        }
    }

    /**
     * Ngắt kết nối SignalR
     */
    public void disconnect() {
        if (hubConnection != null) {
            hubConnection.stop()
                    .doOnComplete(() -> Log.d(TAG, "🚪 SignalR Disconnected"))
                    .blockingAwait();
        }
    }

    public boolean isConnected() {
        return hubConnection != null && hubConnection.getConnectionState() == HubConnectionState.CONNECTED;
    }
}

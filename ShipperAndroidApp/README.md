ShipperApp (minimal)

This folder contains a minimal Android Studio Java app skeleton that demonstrates usage of the DeliveryManagementAPI shipper endpoints.

What is included:
- Retrofit client helper and `ApiService` mapping the endpoints:
  - `GET /api/deliverystaff/me` - get the shipper record for the current user
  - `GET /api/orders/my` - get orders for current user
  - `PATCH /api/orders/{id}/status` - update order status
  - `GET /api/tracking/order/{orderId}` - get checkpoints
  - `POST /api/tracking/checkin` - post a checkpoint (check-in)

How to open in Android Studio:
1. Open Android Studio and choose "Open an existing project".
2. Select this folder: `ShipperAndroidApp`.
3. Let Gradle sync. You may need to adjust plugin and Gradle wrapper versions in Android Studio.

Notes:
- The sample `MainActivity` uses base URL `http://10.0.2.2:5000/` which maps to localhost of the host machine when using the emulator.
- All secured endpoints require `Authorization: Bearer {token}` header returned from `POST /api/auth/login` on the API server. Obtain a JWT and pass it as `"Bearer <token>"` to the Retrofit calls.
- This is a minimal starting point; add layouts, ViewModels, and proper error handling as needed.

# Web API Endpoints Reference

The ERP hosts internal API endpoints consumed by AJAX/JSON components.

## Core API Endpoints

### 1. Booking API
- **Endpoint**: `GET /api/bookings/metrics`
- **Access**: Manager, Admin, Partner
- **Response Payload**:
  ```json
  {
    "totalBookings": 150,
    "totalProfit": 450000.00,
    "qrCodeCount": 120,
    "nonQrCodeCount": 30
  }
  ```

### 2. External Visa API
- **Endpoint**: `POST /api/externalvisas`
- **Access**: Operator, Manager, Admin
- **Payload**:
  ```json
  {
    "passengerName": "Ahmed Ali",
    "affiliation": "O",
    "netCost": 4500.00,
    "barcodeCost": 500.00,
    "sellingPrice": 6000.00
  }
  ```

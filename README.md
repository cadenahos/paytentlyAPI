# Paytently Test Gateway

A payment gateway API built with .NET Core and RabbitMQ for asynchronous payment processing.

## Features

- RESTful API for payment processing
- Asynchronous payment processing using RabbitMQ
- API Key authentication
- Card number masking for security
- Swagger documentation
- Docker support

## Prerequisites

- .NET 6.0 SDK
- Docker and Docker Compose
- RabbitMQ (included in Docker Compose)

## Getting Started

### Local Development

1. Clone the repository
2. Navigate to the project directory
3. Run the application:
   ```bash
   dotnet run
   ```

### Docker Setup

1. Clone the repository
2. Navigate to the project directory
3. Build and run the containers:
   ```bash
   docker-compose up --build
   ```

The application will be available at:

- API: http://localhost:5000
- Swagger UI: http://localhost:5000/swagger
- RabbitMQ Management: http://localhost:15672 (guest/guest)

## API Documentation

### Authentication

All API endpoints require authentication using an API key. Include the API key in the request header:

```
X-API-Key: test-api-key
```

### Endpoints

#### Create Payment

```http
POST /api/payments
```

Request:

```json
{
  "amount": 100.0,
  "currency": "USD",
  "cardNumber": "4111111111111111",
  "expiryMonth": 12,
  "expiryYear": 2025,
  "cvv": "123"
}
```

Response:

```json
{
  "paymentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "amount": 100.0,
  "currency": "USD",
  "maskedCardNumber": "************1111",
  "status": "Pending",
  "createdAt": "2024-03-20T10:00:00Z",
  "processedAt": null,
  "merchantId": "merchant-1",
  "merchantName": "Test Merchant 1"
}
```

#### Get Payment

```http
GET /api/payments/{paymentId}
```

Response:

```json
{
  "paymentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "amount": 100.0,
  "currency": "USD",
  "maskedCardNumber": "************1111",
  "status": "Completed",
  "createdAt": "2024-03-20T10:00:00Z",
  "processedAt": "2024-03-20T10:00:05Z",
  "merchantId": "merchant-1",
  "merchantName": "Test Merchant 1"
}
```

### Error Responses

#### Authentication Error

```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "API Key is missing or invalid"
}
```

#### Validation Error

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Invalid payment request",
  "errors": {
    "amount": ["Amount must be greater than 0"],
    "cardNumber": ["Invalid card number"]
  }
}
```

## Architecture

The application follows an event-driven architecture:

1. Payment requests are received by the API
2. The payment service validates the request and creates a payment record
3. A `PaymentCreatedEvent` is published to RabbitMQ
4. The payment consumer processes the payment asynchronously
5. A `PaymentProcessedEvent` is published with the result
6. The payment status is updated in the system

### Components

- **API Layer**: Handles HTTP requests and responses
- **Payment Service**: Business logic for payment processing
- **Event Publisher**: Publishes payment events to RabbitMQ
- **Payment Consumer**: Processes payment events asynchronously
- **Authentication Service**: Validates API keys and manages merchant information

## Testing

### Payment Scenarios

1. **Successful Payment**

   - Valid card details
   - Sufficient funds
   - Payment processed successfully

2. **Invalid Card**

   - Invalid card number
   - Expired card
   - Invalid CVV

3. **Insufficient Funds**

   - Valid card
   - Insufficient balance
   - Payment declined

4. **Network Issues**
   - Connection timeout
   - Service unavailable
   - Retry mechanism

### Running Tests

```bash
dotnet test
```

## Troubleshooting

### Common Issues

1. **RabbitMQ Connection Issues**

   - Ensure RabbitMQ service is running
   - Check connection string in configuration
   - Verify network connectivity between services

2. **Authentication Failures**

   - Verify API key is correct
   - Check request headers
   - Ensure merchant is properly registered

3. **Payment Processing Delays**
   - Check RabbitMQ queue status
   - Monitor consumer health
   - Verify event publishing

### Logs

Check the application logs for detailed error information:

```bash
docker-compose logs payment-gateway
```

## License

This project is licensed under the MIT License.

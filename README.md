# Paytently Payment Gateway

A secure, event-driven payment gateway API built with .NET 6, RabbitMQ, and Docker.

## Features

- Event-driven payment processing
- API key authentication
- Merchant authorization
- Docker containerization
- RabbitMQ message broker
- Swagger documentation
- Secure card data handling

## Prerequisites

- Docker and Docker Compose
- .NET 6 SDK (for local development)
- Git

## Quick Start

1. Clone the repository:

```bash
git clone <repository-url>
cd paytently-test-gateway
```

2. Start the application using Docker Compose:

```bash
docker-compose up --build
```

The application will be available at:

- API: http://localhost:8080
- Swagger UI: http://localhost:8080/swagger
- RabbitMQ Management UI: http://localhost:15672 (username: guest, password: guest)

## Authentication

The API uses API key authentication with merchant authorization. All requests must include a valid API key in the `X-API-Key` header.

### Available API Keys

| API Key        | Merchant ID | Merchant Name   | Role     |
| -------------- | ----------- | --------------- | -------- |
| test-api-key-1 | merchant-1  | Test Merchant 1 | Merchant |
| test-api-key-2 | merchant-2  | Test Merchant 2 | Merchant |

### Authentication Flow

1. Include the API key in the `X-API-Key` header
2. The API validates the key and assigns merchant role
3. Access is granted only to authenticated merchants

### Example Authentication Header

```http
X-API-Key: test-api-key-1
```

## Configuration

### Environment Variables

The application can be configured using environment variables in the `docker-compose.yml` file:

```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Development
  - RabbitMQ__Host=rabbitmq
  - RabbitMQ__Username=guest
  - RabbitMQ__Password=guest
```

## Testing the API

### Using Swagger UI

1. Open http://localhost:8080/swagger
2. Click the "Authorize" button
3. Enter your API key in the format: `X-API-Key: test-api-key-1`
4. Try out the API endpoints

### Using cURL

#### Create a Payment

```bash
curl -X POST "http://localhost:8080/api/payment" \
-H "Content-Type: application/json" \
-H "X-API-Key: test-api-key-1" \
-d '{
  "amount": 100.00,
  "currency": "USD",
  "cardNumber": "4111111111111111",
  "expiryMonth": 12,
  "expiryYear": 2025,
  "cvv": "123"
}'
```

#### Get Payment Status

```bash
curl -X GET "http://localhost:8080/api/payment/{payment-id}" \
-H "X-API-Key: test-api-key-1"
```

### Expected Responses

#### Create Payment Response

```json
{
  "paymentId": "00000000-0000-0000-0000-000000000000",
  "status": "Pending",
  "createdAt": "2024-05-04T12:00:00Z",
  "merchantId": "merchant-1",
  "merchantName": "Test Merchant 1"
}
```

#### Get Payment Response

```json
{
  "paymentId": "00000000-0000-0000-0000-000000000000",
  "amount": 100.0,
  "currency": "USD",
  "maskedCardNumber": "************1111",
  "status": "Completed",
  "createdAt": "2024-05-04T12:00:00Z",
  "processedAt": "2024-05-04T12:01:00Z",
  "merchantId": "merchant-1",
  "merchantName": "Test Merchant 1"
}
```

## Architecture

The application follows an event-driven architecture:

1. Merchant sends payment request with API key
2. API validates authentication and authorization
3. PaymentCreatedEvent is published to RabbitMQ
4. Payment processor consumes event and processes payment
5. PaymentProcessedEvent is published with result
6. Merchant can query payment status

## Development

### Local Development

1. Install .NET 6 SDK
2. Install RabbitMQ locally or use Docker
3. Update `appsettings.json` with local RabbitMQ settings
4. Run the application:

```bash
dotnet run
```

### Building and Testing

```bash
# Build the application
dotnet build

# Run tests
dotnet test
```

## Security

- API key authentication required for all endpoints
- Merchant role authorization
- Card numbers are masked in responses
- Sensitive data is not stored in plain text
- HTTPS enforced in production

### Authentication Errors

The API returns the following authentication-related errors:

| Status Code | Error Message            | Description                |
| ----------- | ------------------------ | -------------------------- |
| 401         | API Key was not provided | Missing X-API-Key header   |
| 401         | Invalid API Key          | Invalid or expired API key |
| 403         | Forbidden                | Merchant role required     |

## Troubleshooting

### Common Issues

1. **Authentication Issues**

   - Ensure API key is correctly formatted
   - Check if API key is valid
   - Verify merchant role is assigned

2. **RabbitMQ Connection Issues**

   - Check if RabbitMQ container is running
   - Verify connection settings in environment variables

3. **Docker Issues**
   - Ensure Docker daemon is running
   - Check available ports are not in use

## Support

For support, please contact the development team or create an issue in the repository.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

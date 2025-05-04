// using System;
// using System.Collections.Generic;
// using System.Security.Claims;
// using System.Threading.Tasks;
// using MassTransit;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using Moq;
// using PaytentlyGateway.Controllers;
// using PaytentlyGateway.Models;
// using PaytentlyGateway.Models.Events;
// using Xunit;

// namespace PaytentlyGateway.Tests.Controllers
// {
//     public class PaymentControllerTests
//     {
//         private readonly Mock<IPublishEndpoint> _publishEndpointMock;
//         private readonly PaymentController _controller;

//         public PaymentControllerTests()
//         {
//             _publishEndpointMock = new Mock<IPublishEndpoint>();
//             _controller = new PaymentController(_publishEndpointMock.Object);

//             // Mock the User property for authentication context
//             var claims = new List<Claim>
//             {
//                 new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
//                 new Claim(ClaimTypes.Name, "Test Merchant"),
//                 new Claim(ClaimTypes.Role, "Merchant")
//             };
//             var identity = new ClaimsIdentity(claims, "TestAuthType");
//             var principal = new ClaimsPrincipal(identity);

//             var mockHttpContext = new Mock<HttpContext>();
//             mockHttpContext.Setup(c => c.User).Returns(principal);

//             _controller.ControllerContext = new ControllerContext
//             {
//                 HttpContext = mockHttpContext.Object
//             };
//         }

//         [Fact]
//         public async Task CreatePayment_ValidPayment_PublishesPaymentCreatedEvent_ReturnsOk()
//         {
//             // Arrange
//             var payment = new Payment
//             {
//                 Amount = 100.00m,
//                 Currency = "USD",
//                 CardNumber = "1234567890123456",
//                 ExpiryMonth = 12,
//                 ExpiryYear = 2025,
//                 CVV = "123"
//             };

//             // Act
//             var result = await _controller.CreatePayment(payment) as OkObjectResult;

//             // Assert
//             Assert.NotNull(result);
//             Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

//             var response = result?.Value as dynamic;
//             Assert.NotNull(response);
//             if (response != null)
//             {
//                 Assert.NotEqual(Guid.Empty, (Guid)response.PaymentId);
//                 Assert.Equal("Pending", (string)response.Status);
//                 Assert.NotEqual(default(DateTime), (DateTime)response.CreatedAt);
//                 Assert.NotNull((string)response.MerchantId);
//                 Assert.Equal("Test Merchant", (string)response.MerchantName);
//             }

//             _publishEndpointMock.Verify(p => p.Publish(
//                 It.Is<PaymentCreatedEvent>(e =>
//                     e.Amount == payment.Amount &&
//                     e.Currency == payment.Currency &&
//                     e.CardNumber == payment.CardNumber &&
//                     e.ExpiryMonth == payment.ExpiryMonth &&
//                     e.ExpiryYear == payment.ExpiryYear &&
//                     e.CVV == payment.CVV &&
//                     e.MerchantId == GetMerchantIdFromContext() &&
//                     e.MerchantName == GetMerchantNameFromContext()
//                 ),
//                 default
//             ), Times.Once);

//         }
//         private string GetMerchantIdFromContext()
//         {
//             var identity = _controller.ControllerContext.HttpContext.User.Identity as ClaimsIdentity;
//             return identity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//         }

//         private string GetMerchantNameFromContext()
//         {
//             var identity = _controller.ControllerContext.HttpContext.User.Identity as ClaimsIdentity;
//             return identity?.FindFirst(ClaimTypes.Name)?.Value;
//         }
//         [Fact]
//         public void GetPayment_ValidId_ReturnsOkWithPaymentDetails()
//         {
//             // Arrange
//             var paymentId = Guid.NewGuid();

//             // Act
//             var result = _controller.GetPayment(paymentId) as OkObjectResult;

//             // Assert
//             Assert.NotNull(result);
//             Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

//             var response = result?.Value as dynamic;
//             Assert.NotNull(response);
//             if (response != null)
//             {
//                 Assert.Equal(paymentId, (Guid)response.PaymentId);
//                 Assert.Equal(100.00m, (decimal)response.Amount);
//                 Assert.Equal("USD", (string)response.Currency);
//                 Assert.NotNull((string)response.MaskedCardNumber);
//                 Assert.Equal("Completed", (string)response.Status);
//                 Assert.NotEqual(default(DateTime), (DateTime)response.CreatedAt);
//                 Assert.NotNull((string)response.MerchantId);
//                 Assert.Equal("Test Merchant", (string)response.MerchantName);
//                 Assert.NotNull((DateTime)response.ProcessedAt);
//             }
//         }
//     }
// }
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using PaytentlyTestGateway.Models;
using PaytentlyTestGateway.Services;
using Xunit;

namespace PaytentlyTestGateway.Tests.Services
{
    public class PaymentServiceTests
    {
        private readonly Mock<IPaymentEventPublisher> _eventPublisherMock;
        private readonly Mock<ICardProtectionService> _cardProtectionServiceMock;
        private readonly PaymentService _paymentService;

        public PaymentServiceTests()
        {
            _eventPublisherMock = new Mock<IPaymentEventPublisher>();
            _cardProtectionServiceMock = new Mock<ICardProtectionService>();
            _paymentService = new PaymentService(_eventPublisherMock.Object, _cardProtectionServiceMock.Object);
        }

        [Fact]
        public async Task CreatePaymentAsync_ValidRequest_ReturnsPaymentResponse_PublishesEvent()
        {
            // Arrange
            var request = new CreatePaymentRequest
            {
                Amount = 100.00m,
                Currency = "USD",
                CardNumber = "1234567890123456",
                ExpiryMonth = 12,
                ExpiryYear = 2025,
                Cvv = "123"
            };

            _cardProtectionServiceMock.Setup(x => x.MaskCardNumber(request.CardNumber)).Returns("XXXX-XXXX-XXXX-4567");
            _cardProtectionServiceMock.Setup(x => x.HashCardNumber(request.CardNumber)).Returns("hashed_card");
            _cardProtectionServiceMock.Setup(x => x.HashCvv(request.Cvv)).Returns("hashed_cvv");
            _cardProtectionServiceMock.Setup(x => x.ValidateCardNumber(request.CardNumber)).Returns(true);
            _cardProtectionServiceMock.Setup(x => x.ValidateExpiryDate(request.ExpiryMonth, request.ExpiryYear)).Returns(true);

            // Act
            var response = await _paymentService.CreatePaymentAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.NotEqual(Guid.Empty, response.PaymentId);
            Assert.Equal(request.Amount, response.Amount);
            Assert.Equal(request.Currency, response.Currency);
            Assert.Equal("XXXX-XXXX-XXXX-4567", response.MaskedCardNumber);
            Assert.Equal("Pending", response.Status);
            Assert.NotEqual(default(DateTime), response.CreatedAt);
            Assert.Equal("merchant-1", response.MerchantId);
            Assert.Equal("Test Merchant 1", response.MerchantName);

            _eventPublisherMock.Verify(x => x.PublishPaymentCreatedEvent(It.Is<PaymentCreatedEvent>(e =>
                e.PaymentId == response.PaymentId &&
                e.Amount == request.Amount &&
                e.Currency == request.Currency &&
                e.CardNumber == "hashed_card" &&
                e.ExpiryMonth == request.ExpiryMonth &&
                e.ExpiryYear == request.ExpiryYear &&
                e.Cvv == "hashed_cvv" &&
                e.MerchantId == "merchant-1" &&
                e.MerchantName == "Test Merchant 1" &&
                e.CreatedAt == response.CreatedAt
            )), Times.Once);
        }

        [Fact]
        public async Task GetPaymentAsync_ExistingPaymentId_ReturnsPaymentResponse()
        {
            // Arrange
            var paymentId = Guid.NewGuid();
            var expectedPayment = new PaymentResponse
            {
                PaymentId = paymentId,
                Amount = 50.00m,
                Currency = "EUR",
                MaskedCardNumber = "XXXX-XXXX-XXXX-1234",
                Status = "Completed",
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                MerchantId = "merchant-1",
                MerchantName = "Test Merchant 1"
            };
            // Use reflection to add the payment directly to the _payments dictionary for testing
            var paymentsField = typeof(PaymentService).GetField("_payments", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var paymentsDictionary = paymentsField?.GetValue(_paymentService) as Dictionary<Guid, PaymentResponse>;
            paymentsDictionary[paymentId] = expectedPayment;

            // Act
            var actualPayment = await _paymentService.GetPaymentAsync(paymentId);

            // Assert
            Assert.NotNull(actualPayment);
            Assert.Equal(expectedPayment.PaymentId, actualPayment.PaymentId);
            Assert.Equal(expectedPayment.Amount, actualPayment.Amount);
            Assert.Equal(expectedPayment.Currency, actualPayment.Currency);
            Assert.Equal(expectedPayment.MaskedCardNumber, actualPayment.MaskedCardNumber);
            Assert.Equal(expectedPayment.Status, actualPayment.Status);
            Assert.Equal(expectedPayment.CreatedAt, actualPayment.CreatedAt);
            Assert.Equal(expectedPayment.MerchantId, actualPayment.MerchantId);
            Assert.Equal(expectedPayment.MerchantName, actualPayment.MerchantName);
        }

        [Fact]
        public async Task GetPaymentAsync_NonExistingPaymentId_ThrowsKeyNotFoundException()
        {
            // Arrange
            var nonExistingPaymentId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _paymentService.GetPaymentAsync(nonExistingPaymentId));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public async Task CreatePaymentAsync_InvalidAmount_ThrowsArgumentException(decimal amount)
        {
            // Arrange
            var request = new CreatePaymentRequest { Amount = amount, Currency = "USD", CardNumber = "1", ExpiryMonth = 1, ExpiryYear = 2026, Cvv = "123" };
            _cardProtectionServiceMock.Setup(x => x.ValidateCardNumber(It.IsAny<string>())).Returns(true);
            _cardProtectionServiceMock.Setup(x => x.ValidateExpiryDate(It.IsAny<int>(), It.IsAny<int>())).Returns(true);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _paymentService.CreatePaymentAsync(request));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("US")]
        [InlineData("ABCD")]
        public async Task CreatePaymentAsync_InvalidCurrency_ThrowsArgumentException(string currency)
        {
            // Arrange
            var request = new CreatePaymentRequest { Amount = 100, Currency = currency, CardNumber = "1", ExpiryMonth = 1, ExpiryYear = 2026, Cvv = "123" };
            _cardProtectionServiceMock.Setup(x => x.ValidateCardNumber(It.IsAny<string>())).Returns(true);
            _cardProtectionServiceMock.Setup(x => x.ValidateExpiryDate(It.IsAny<int>(), It.IsAny<int>())).Returns(true);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _paymentService.CreatePaymentAsync(request));
        }

        [Fact]
        public async Task CreatePaymentAsync_InvalidCardNumber_ThrowsArgumentException()
        {
            // Arrange
            var request = new CreatePaymentRequest { Amount = 100, Currency = "USD", CardNumber = "invalid", ExpiryMonth = 1, ExpiryYear = 2026, Cvv = "123" };
            _cardProtectionServiceMock.Setup(x => x.ValidateCardNumber(request.CardNumber)).Returns(false);
            _cardProtectionServiceMock.Setup(x => x.ValidateExpiryDate(It.IsAny<int>(), It.IsAny<int>())).Returns(true);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _paymentService.CreatePaymentAsync(request));
        }

        [Fact]
        public async Task CreatePaymentAsync_InvalidExpiryDate_ThrowsArgumentException()
        {
            // Arrange
            var request = new CreatePaymentRequest { Amount = 100, Currency = "USD", CardNumber = "1", ExpiryMonth = 1, ExpiryYear = 2020, Cvv = "123" };
            _cardProtectionServiceMock.Setup(x => x.ValidateCardNumber(request.CardNumber)).Returns(true);
            _cardProtectionServiceMock.Setup(x => x.ValidateExpiryDate(request.ExpiryMonth, request.ExpiryYear)).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _paymentService.CreatePaymentAsync(request));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("12")]
        [InlineData("12345")]
        public async Task CreatePaymentAsync_InvalidCvv_ThrowsArgumentException(string cvv)
        {
            // Arrange
            var request = new CreatePaymentRequest { Amount = 100, Currency = "USD", CardNumber = "1", ExpiryMonth = 1, ExpiryYear = 2026, Cvv = cvv };
            _cardProtectionServiceMock.Setup(x => x.ValidateCardNumber(It.IsAny<string>())).Returns(true);
            _cardProtectionServiceMock.Setup(x => x.ValidateExpiryDate(It.IsAny<int>(), It.IsAny<int>())).Returns(true);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _paymentService.CreatePaymentAsync(request));
        }
    }
}
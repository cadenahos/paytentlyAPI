using PaytentlyTestGateway.Models;

namespace PaytentlyTestGateway.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentEventPublisher _eventPublisher;
        private readonly ICardProtectionService _cardProtectionService;
        private readonly Dictionary<Guid, PaymentResponse> _payments = new();

        public PaymentService(IPaymentEventPublisher eventPublisher, ICardProtectionService cardProtectionService)
        {
            _eventPublisher = eventPublisher;
            _cardProtectionService = cardProtectionService;
        }

        public async Task<PaymentResponse> CreatePaymentAsync(CreatePaymentRequest request)
        {
            ValidatePaymentRequest(request);

            var payment = new PaymentResponse
            {
                PaymentId = Guid.NewGuid(),
                Amount = request.Amount,
                Currency = request.Currency,
                MaskedCardNumber = _cardProtectionService.MaskCardNumber(request.CardNumber),
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                MerchantId = "merchant-1", // This would come from the authenticated user in a real implementation
                MerchantName = "Test Merchant 1"
            };

            _payments[payment.PaymentId] = payment;

            var @event = new PaymentCreatedEvent
            {
                PaymentId = payment.PaymentId,
                Amount = request.Amount,
                Currency = request.Currency,
                CardNumber = _cardProtectionService.HashCardNumber(request.CardNumber),
                ExpiryMonth = request.ExpiryMonth,
                ExpiryYear = request.ExpiryYear,
                Cvv = _cardProtectionService.HashCvv(request.Cvv),
                MerchantId = payment.MerchantId,
                MerchantName = payment.MerchantName,
                CreatedAt = payment.CreatedAt
            };

            await _eventPublisher.PublishPaymentCreatedEvent(@event);

            return payment;
        }

        public async Task<PaymentResponse> GetPaymentAsync(Guid paymentId)
        {
            if (!_payments.TryGetValue(paymentId, out var payment))
            {
                throw new KeyNotFoundException($"Payment with ID {paymentId} not found");
            }

            return payment;
        }

        private void ValidatePaymentRequest(CreatePaymentRequest request)
        {
            if (request.Amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than 0", nameof(request.Amount));
            }

            if (string.IsNullOrWhiteSpace(request.Currency) || request.Currency.Length != 3)
            {
                throw new ArgumentException("Currency must be 3 characters", nameof(request.Currency));
            }

            if (!_cardProtectionService.ValidateCardNumber(request.CardNumber))
            {
                throw new ArgumentException("Invalid card number", nameof(request.CardNumber));
            }

            if (!_cardProtectionService.ValidateExpiryDate(request.ExpiryMonth, request.ExpiryYear))
            {
                throw new ArgumentException("Card has expired or invalid expiry date", nameof(request.ExpiryYear));
            }

            if (string.IsNullOrWhiteSpace(request.Cvv) || request.Cvv.Length < 3 || request.Cvv.Length > 4)
            {
                throw new ArgumentException("Invalid CVV", nameof(request.Cvv));
            }
        }
    }
} 
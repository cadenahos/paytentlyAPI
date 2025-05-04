using PaytentlyTestGateway.Models;

namespace PaytentlyTestGateway.Services
{
    public interface IPaymentService
    {
        Task<PaymentResponse> CreatePaymentAsync(CreatePaymentRequest request);
        Task<PaymentResponse> GetPaymentAsync(Guid paymentId);
    }
} 
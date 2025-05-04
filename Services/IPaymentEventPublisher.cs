using PaytentlyTestGateway.Models;

namespace PaytentlyTestGateway.Services
{
    public interface IPaymentEventPublisher
    {
        Task PublishPaymentCreatedEvent(PaymentCreatedEvent @event);
        Task PublishPaymentProcessedEvent(PaymentProcessedEvent @event);
    }
} 
using System;
using System.Threading.Tasks;
using MassTransit;
using PaytentlyGateway.Models.Events;

namespace PaytentlyGateway.Consumers
{
    public class PaymentCreatedConsumer : IConsumer<PaymentCreatedEvent>
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public PaymentCreatedConsumer(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<PaymentCreatedEvent> context)
        {
            // Simulate payment processing with the acquirer
            await Task.Delay(1000); // Simulate processing time

            var paymentProcessedEvent = new PaymentProcessedEvent
            {
                PaymentId = context.Message.PaymentId,
                Status = "Completed", // In a real implementation, this would come from the acquirer
                ProcessedAt = DateTime.UtcNow,
                TransactionId = Guid.NewGuid().ToString()
            };

            await _publishEndpoint.Publish(paymentProcessedEvent);
        }
    }
} 
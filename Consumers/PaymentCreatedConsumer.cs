using System;
using System.Threading.Tasks;
using MassTransit;
using PaytentlyTestGateway.Models;

namespace PaytentlyTestGateway.Consumers
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
            var @event = context.Message;

            // Simulate payment processing
            await Task.Delay(1000);

            // Publish payment processed event
            var processedEvent = new PaymentProcessedEvent
            {
                PaymentId = @event.PaymentId,
                Status = "Processed",
                ProcessedAt = DateTime.UtcNow,
                MerchantId = @event.MerchantId
            };

            await _publishEndpoint.Publish(processedEvent);
        }
    }
} 
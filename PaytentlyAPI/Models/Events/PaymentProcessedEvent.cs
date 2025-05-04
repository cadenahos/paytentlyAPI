using System;

namespace PaytentlyGateway.Models.Events
{
    public class PaymentProcessedEvent
    {
        public Guid PaymentId { get; set; }
        public string Status { get; set; } = "Completed";
        public DateTime ProcessedAt { get; set; }
        public string TransactionId { get; set; } = string.Empty;
    }
} 
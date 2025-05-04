namespace PaytentlyTestGateway.Models
{
    public class PaymentProcessedEvent
    {
        public Guid PaymentId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
        public string MerchantId { get; set; } = string.Empty;
    }
} 
namespace PaytentlyTestGateway.Models
{
    public class PaymentResponse
    {
        public Guid PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string MaskedCardNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string MerchantId { get; set; } = string.Empty;
        public string MerchantName { get; set; } = string.Empty;
    }
} 
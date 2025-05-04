using System;

namespace PaytentlyGateway.Models
{
    public class Payment
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string CardNumber { get; set; } = string.Empty;
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public string CVV { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string MaskedCardNumber => CardNumber.Length > 4 ? 
            new string('*', CardNumber.Length - 4) + CardNumber.Substring(CardNumber.Length - 4) : 
            CardNumber;
    }
} 
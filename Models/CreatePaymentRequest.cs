using System.ComponentModel.DataAnnotations;

namespace PaytentlyTestGateway.Models
{
    public class CreatePaymentRequest
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be 3 characters")]
        public string Currency { get; set; } = string.Empty;

        [Required]
        [CreditCard(ErrorMessage = "Invalid card number")]
        public string CardNumber { get; set; } = string.Empty;

        [Required]
        [Range(1, 12, ErrorMessage = "Expiry month must be between 1 and 12")]
        public int ExpiryMonth { get; set; }

        [Required]
        [Range(2024, 2100, ErrorMessage = "Expiry year must be valid")]
        public int ExpiryYear { get; set; }

        [Required]
        [StringLength(4, MinimumLength = 3, ErrorMessage = "CVV must be between 3 and 4 digits")]
        public string Cvv { get; set; } = string.Empty;
    }
} 
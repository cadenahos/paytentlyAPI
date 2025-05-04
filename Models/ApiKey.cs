namespace PaytentlyGateway.Models
{
    public class ApiKey
    {
        public string Key { get; set; } = string.Empty;
        public string MerchantId { get; set; } = string.Empty;
        public string MerchantName { get; set; } = string.Empty;
        public MerchantRole Role { get; set; }
    }
} 
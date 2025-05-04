using System.Threading.Tasks;

namespace PaytentlyTestGateway.Authentication
{
    public interface IApiKeyAuthenticationService
    {
        Task<ApiKey> ValidateApiKey(string apiKey);
    }

    public class ApiKeyAuthenticationService : IApiKeyAuthenticationService
    {
        public async Task<ApiKey> ValidateApiKey(string apiKey)
        {
            // In a real application, this would validate against a database
            if (apiKey == "test-api-key")
            {
                return new ApiKey
                {
                    Key = apiKey,
                    MerchantId = "merchant-1",
                    MerchantName = "Test Merchant 1"
                };
            }

            return null;
        }
    }

    public class ApiKey
    {
        public string Key { get; set; } = string.Empty;
        public string MerchantId { get; set; } = string.Empty;
        public string MerchantName { get; set; } = string.Empty;
    }
} 
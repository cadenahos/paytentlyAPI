using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PaytentlyGateway.Models;

namespace PaytentlyGateway.Services
{
    public interface IApiKeyAuthenticationService
    {
        Task<bool> ValidateApiKey(string apiKey);
        Task<ApiKey> GetApiKeyDetails(string apiKey);
    }

    public class ApiKeyAuthenticationService : IApiKeyAuthenticationService
    {
        private readonly IConfiguration _configuration;
        private readonly List<ApiKey> _apiKeys;

        public ApiKeyAuthenticationService(IConfiguration configuration)
        {
            _configuration = configuration;
            _apiKeys = new List<ApiKey>
            {
                new ApiKey
                {
                    Key = "test-api-key-1",
                    MerchantId = "merchant-1",
                    MerchantName = "Test Merchant 1"
                },
                new ApiKey
                {
                    Key = "test-api-key-2",
                    MerchantId = "merchant-2",
                    MerchantName = "Test Merchant 2"
                }
            };
        }

        public Task<bool> ValidateApiKey(string apiKey)
        {
            return Task.FromResult(_apiKeys.Any(k => k.Key == apiKey));
        }

        public Task<ApiKey> GetApiKeyDetails(string apiKey)
        {
            return Task.FromResult(_apiKeys.FirstOrDefault(k => k.Key == apiKey));
        }
    }
} 
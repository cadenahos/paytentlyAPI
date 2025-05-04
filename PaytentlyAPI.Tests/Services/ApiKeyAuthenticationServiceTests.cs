

using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PaytentlyGateway.Models;
using PaytentlyGateway.Services;
using Xunit;
using Moq;

namespace PaytentlyGateway.Tests.Services
{
    public class ApiKeyAuthenticationServiceTests
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly ApiKeyAuthenticationService _apiKeyService;

        public ApiKeyAuthenticationServiceTests()
        {
            _configurationMock = new Mock<IConfiguration>();
            _apiKeyService = new ApiKeyAuthenticationService(_configurationMock.Object);
        }

        [Fact]
        public async Task ValidateApiKey_ExistingApiKey_ReturnsTrue()
        {
            // Arrange
            string existingApiKey = "test-api-key-1";

            // Act
            bool isValid = await _apiKeyService.ValidateApiKey(existingApiKey);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public async Task ValidateApiKey_NonExistingApiKey_ReturnsFalse()
        {
            // Arrange
            string nonExistingApiKey = "invalid-api-key";

            // Act
            bool isValid = await _apiKeyService.ValidateApiKey(nonExistingApiKey);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public async Task GetApiKeyDetails_ExistingApiKey_ReturnsApiKeyDetails()
        {
            // Arrange
            string existingApiKey = "test-api-key-2";

            // Act
            ApiKey apiKeyDetails = await _apiKeyService.GetApiKeyDetails(existingApiKey);

            // Assert
            Assert.NotNull(apiKeyDetails);
            Assert.Equal(existingApiKey, apiKeyDetails.Key);
            Assert.Equal("merchant-2", apiKeyDetails.MerchantId);
            Assert.Equal("Test Merchant 2", apiKeyDetails.MerchantName);
        }

        [Fact]
        public async Task GetApiKeyDetails_NonExistingApiKey_ReturnsNull()
        {
            // Arrange
            string nonExistingApiKey = "invalid-api-key";

            // Act
            ApiKey apiKeyDetails = await _apiKeyService.GetApiKeyDetails(nonExistingApiKey);

            // Assert
            Assert.Null(apiKeyDetails);
        }

        [Fact]
        public void Constructor_LoadsApiKeys()
        {
            // Arrange
            // The ApiKeyAuthenticationService constructor initializes the _apiKeys list.

            // Act
            // We don't need to do anything specific here, the setup in the constructor is what we're testing.

            // Assert
            // Use reflection to access the private _apiKeys field and verify its contents.
            var apiKeysField = typeof(ApiKeyAuthenticationService).GetField("_apiKeys", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var apiKeys = apiKeysField?.GetValue(_apiKeyService) as List<ApiKey>;

            Assert.NotNull(apiKeys);
            Assert.Equal(2, apiKeys.Count);
            Assert.Contains(apiKeys, k => k.Key == "test-api-key-1" && k.MerchantId == "merchant-1" && k.MerchantName == "Test Merchant 1");
            Assert.Contains(apiKeys, k => k.Key == "test-api-key-2" && k.MerchantId == "merchant-2" && k.MerchantName == "Test Merchant 2");
        }
    }
}
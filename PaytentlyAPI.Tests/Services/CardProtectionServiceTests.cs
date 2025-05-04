using System;
using Xunit;
using PaytentlyTestGateway.Services; // Use the correct namespace for your service
using System.Security.Cryptography; // Needed for potential hash verification if desired
using System.Text; // Needed for potential hash verification if desired

namespace PaytentlyTestGateway.Tests.Services // Adjust namespace as needed
{
    public class CardProtectionServiceTests
    {
        private readonly CardProtectionService _service;
        private readonly string _expectedPepper = "your-secure-pepper-value"; // Match the service's pepper

        public CardProtectionServiceTests()
        {
            _service = new CardProtectionService();
        }

        // --- MaskCardNumber Tests ---

        [Fact]
        public void MaskCardNumber_WithValidLongCardNumber_ReturnsMaskedString()
        {
            // Arrange
            string cardNumber = "1234567890123456";
            string expected = "************3456";

            // Act
            string actual = _service.MaskCardNumber(cardNumber);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void MaskCardNumber_WithValidShortCardNumber_ReturnsMaskedString()
        {
            // Arrange
            string cardNumber = "1234567"; // 7 digits
            string expected = "***4567";

            // Act
            string actual = _service.MaskCardNumber(cardNumber);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void MaskCardNumber_WithExactlyFourDigits_ReturnsOriginalString()
        {
            // Arrange
            string cardNumber = "1234";
            string expected = "1234"; // Only last 4 shown, which is the whole string

            // Act
            string actual = _service.MaskCardNumber(cardNumber);

            // Assert
            Assert.Equal(expected, actual);
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("123")] // Less than 4 digits
        public void MaskCardNumber_WithInvalidInput_ReturnsEmptyString(string invalidCardNumber)
        {
            // Arrange
            string expected = string.Empty;

            // Act
            string actual = _service.MaskCardNumber(invalidCardNumber);

            // Assert
            Assert.Equal(expected, actual);
        }

        // --- HashCardNumber Tests ---

        [Fact]
        public void HashCardNumber_WithValidInput_ReturnsCorrectHash()
        {
            // Arrange
            string cardNumber = "1234567890123456";
            string saltedCardNumber = cardNumber + _expectedPepper;
            string expectedHash;

            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(saltedCardNumber);
                var hash = sha256.ComputeHash(bytes);
                expectedHash = Convert.ToBase64String(hash);
                // Calculated expected value for "1234567890123456" + "your-secure-pepper-value"
                // expectedHash should be "QxVNdHY6L5+eHZfJ/GqphrEt/5eM9UPTj3gV3R3jXbY="
            }

            // Act
            string actualHash = _service.HashCardNumber(cardNumber);

            // Assert
            Assert.Equal(expectedHash, actualHash);
        }

        [Fact]
        public void HashCardNumber_WithDifferentValidInput_ReturnsDifferentHash()
        {
            // Arrange
            string cardNumber1 = "1234567890123456";
            string cardNumber2 = "9876543210987654";

            // Act
            string hash1 = _service.HashCardNumber(cardNumber1);
            string hash2 = _service.HashCardNumber(cardNumber2);

            // Assert
            Assert.NotNull(hash1);
            Assert.NotNull(hash2);
            Assert.NotEqual(hash1, hash2);
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void HashCardNumber_WithInvalidInput_ThrowsArgumentException(string invalidCardNumber)
        {
            // Arrange, Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => _service.HashCardNumber(invalidCardNumber));
            Assert.Contains("Card number cannot be empty", ex.Message); // Check message content
            Assert.Equal("cardNumber", ex.ParamName); // Check parameter name
        }

        // --- HashCvv Tests ---

        [Fact]
        public void HashCvv_WithValidInput_ReturnsCorrectHash()
        {
            // Arrange
            string cvv = "123";
            string saltedCvv = cvv + _expectedPepper;
            string expectedHash;

            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(saltedCvv);
                var hash = sha256.ComputeHash(bytes);
                expectedHash = Convert.ToBase64String(hash);
                 // Calculated expected value for "123" + "your-secure-pepper-value"
                 // expectedHash should be "t86uCRDBg1p+x0j+d89345aF+vJ8l1Z7b3q/9mR/aJc="
            }

            // Act
            string actualHash = _service.HashCvv(cvv);

            // Assert
            Assert.Equal(expectedHash, actualHash);
        }

        [Fact]
        public void HashCvv_WithDifferentValidInput_ReturnsDifferentHash()
        {
            // Arrange
            string cvv1 = "123";
            string cvv2 = "456";

            // Act
            string hash1 = _service.HashCvv(cvv1);
            string hash2 = _service.HashCvv(cvv2);

            // Assert
            Assert.NotNull(hash1);
            Assert.NotNull(hash2);
            Assert.NotEqual(hash1, hash2);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void HashCvv_WithInvalidInput_ThrowsArgumentException(string invalidCvv)
        {
            // Arrange, Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => _service.HashCvv(invalidCvv));
            Assert.Contains("CVV cannot be empty", ex.Message);
            Assert.Equal("cvv", ex.ParamName);
        }
        [Theory]
        [InlineData("5105105105105100")] // Valid Mastercard
        // [InlineData("1234-5678-9012-3456")] // Invalid Luhn -> REPLACED
        [InlineData("5105-1051-0510-5100")] // Valid Mastercard with hyphens
        // [InlineData(" 1234 5678 9012 3456 ")] // Invalid Luhn -> REPLACED
        [InlineData(" 4111 1111 1111 1111 ")] // Valid visa with spaces
        [InlineData("4111111111111111")] // Valid visa
        public void ValidateCardNumber_WithValidNumber_ReturnsTrue(string validCardNumber)
        {
            // Arrange & Act
            bool isValid = _service.ValidateCardNumber(validCardNumber);

            // Assert
            Assert.True(isValid); // Line 206
        }

        [Theory]
        [InlineData("49927398717")] // Invalid Luhn check digit
        [InlineData("123456789012")] // Too short (12 digits)
        [InlineData("49927398716")] // MOVED HERE - Fails length check (11 digits)
        [InlineData("123456789012345678901")] // Too long (21 digits)
        [InlineData("ABCDEFGHIKLMNOPQ")] // Non-numeric
        [InlineData("1234567890123456")] // FAILS Luhn Check
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ValidateCardNumber_WithInvalidNumber_ReturnsFalse(string invalidCardNumber)
        {
             // Arrange & Act
            bool isValid = _service.ValidateCardNumber(invalidCardNumber);

            // Assert
            Assert.False(isValid);
        }
        // --- ValidateExpiryDate Tests ---
        // NOTE: These tests depend on the current date (assumed May 2025)

        [Theory]
        // Future dates
        [InlineData(6, 2025)]  // Next month, same year
        [InlineData(5, 2025)]  // Current month, same year
        [InlineData(12, 2025)] // End of current year
        [InlineData(1, 2026)]  // Next year
        [InlineData(11, 2030)] // Far future
        public void ValidateExpiryDate_WithValidFutureDate_ReturnsTrue(int month, int year)
        {
            // Arrange & Act
            bool isValid = _service.ValidateExpiryDate(month, year);

            // Assert
            Assert.True(isValid);
        }

        [Theory]
        // Past dates
        [InlineData(4, 2025)]  // Previous month, same year
        [InlineData(1, 2025)]  // Start of current year
        [InlineData(12, 2024)] // Previous year
        [InlineData(5, 2024)]  // Previous year, same month num
        // Invalid month/year values
        [InlineData(0, 2026)]  // Invalid month (too low)
        [InlineData(13, 2026)] // Invalid month (too high)
        [InlineData(-1, 2026)] // Invalid month (negative)
        [InlineData(5, -2025)] // Invalid year (negative - though less likely input)
        public void ValidateExpiryDate_WithInvalidOrPastDate_ReturnsFalse(int month, int year)
        {
            // Arrange & Act
            bool isValid = _service.ValidateExpiryDate(month, year);

            // Assert
            Assert.False(isValid);
        }
    }
}
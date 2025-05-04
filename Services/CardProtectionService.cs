using System;
using System.Security.Cryptography;
using System.Text;

namespace PaytentlyTestGateway.Services
{
    public interface ICardProtectionService
    {
        string MaskCardNumber(string cardNumber);
        string HashCardNumber(string cardNumber);
        string HashCvv(string cvv);
        bool ValidateCardNumber(string cardNumber);
        bool ValidateExpiryDate(int month, int year);
    }

    public class CardProtectionService : ICardProtectionService
    {
        private readonly string _pepper;

        public CardProtectionService()
        {
            // In a real application, this would come from secure configuration
            _pepper = "your-secure-pepper-value";
        }

        public string MaskCardNumber(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length < 4)
                return string.Empty;

            return new string('*', cardNumber.Length - 4) + cardNumber.Substring(cardNumber.Length - 4);
        }

        public string HashCardNumber(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
                throw new ArgumentException("Card number cannot be empty", nameof(cardNumber));

            using (var sha256 = SHA256.Create())
            {
                var saltedCardNumber = cardNumber + _pepper;
                var bytes = Encoding.UTF8.GetBytes(saltedCardNumber);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        public string HashCvv(string cvv)
        {
            if (string.IsNullOrWhiteSpace(cvv))
                throw new ArgumentException("CVV cannot be empty", nameof(cvv));

            using (var sha256 = SHA256.Create())
            {
                var saltedCvv = cvv + _pepper;
                var bytes = Encoding.UTF8.GetBytes(saltedCvv);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        public bool ValidateCardNumber(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
                return false;

            // Remove any non-digit characters
            cardNumber = new string(cardNumber.Where(char.IsDigit).ToArray());

            // Check if the card number is between 13 and 19 digits
            if (cardNumber.Length < 13 || cardNumber.Length > 19)
                return false;

            // Luhn algorithm validation
            int sum = 0;
            bool alternate = false;
            for (int i = cardNumber.Length - 1; i >= 0; i--)
            {
                int n = int.Parse(cardNumber[i].ToString());
                if (alternate)
                {
                    n *= 2;
                    if (n > 9)
                    {
                        n = (n % 10) + 1;
                    }
                }
                sum += n;
                alternate = !alternate;
            }
            return (sum % 10 == 0);
        }

        public bool ValidateExpiryDate(int month, int year)
        {
            if (month < 1 || month > 12)
                return false;

            var currentDate = DateTime.UtcNow;
            var currentYear = currentDate.Year;
            var currentMonth = currentDate.Month;

            if (year < currentYear)
                return false;

            if (year == currentYear && month < currentMonth)
                return false;

            return true;
        }
    }
} 
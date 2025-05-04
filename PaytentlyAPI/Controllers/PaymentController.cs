using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaytentlyGateway.Models;
using PaytentlyGateway.Models.Events;
using MassTransit;
using System.Security.Claims;

namespace PaytentlyGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Merchant")]
    public class PaymentController : ControllerBase
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public PaymentController(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] Payment payment)
        {
            var merchantId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var merchantName = User.FindFirst(ClaimTypes.Name)?.Value;

            payment.Id = Guid.NewGuid();
            payment.CreatedAt = DateTime.UtcNow;
            payment.Status = "Pending";

            var paymentCreatedEvent = new PaymentCreatedEvent
            {
                PaymentId = payment.Id,
                Amount = payment.Amount,
                Currency = payment.Currency,
                CardNumber = payment.CardNumber,
                ExpiryMonth = payment.ExpiryMonth,
                ExpiryYear = payment.ExpiryYear,
                CVV = payment.CVV,
                CreatedAt = payment.CreatedAt,
                MerchantId = merchantId,
                MerchantName = merchantName
            };

            await _publishEndpoint.Publish(paymentCreatedEvent);

            return Ok(new { 
                PaymentId = payment.Id,
                Status = payment.Status,
                CreatedAt = payment.CreatedAt,
                MerchantId = merchantId,
                MerchantName = merchantName
            });
        }

        [HttpGet("{id}")]
        public IActionResult GetPayment(Guid id)
        {
            var merchantId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var merchantName = User.FindFirst(ClaimTypes.Name)?.Value;

            // In a real implementation, this would fetch from a database
            // For now, we'll return a mock response
            var mockPayment = new Payment
            {
                Id = id,
                Amount = 100.00m,
                Currency = "USD",
                CardNumber = "4111111111111111",
                ExpiryMonth = 12,
                ExpiryYear = 2025,
                Status = "Completed",
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                ProcessedAt = DateTime.UtcNow
            };

            return Ok(new
            {
                PaymentId = mockPayment.Id,
                Amount = mockPayment.Amount,
                Currency = mockPayment.Currency,
                MaskedCardNumber = mockPayment.MaskedCardNumber,
                Status = mockPayment.Status,
                CreatedAt = mockPayment.CreatedAt,
                ProcessedAt = mockPayment.ProcessedAt,
                MerchantId = merchantId,
                MerchantName = merchantName
            });
        }
    }
} 
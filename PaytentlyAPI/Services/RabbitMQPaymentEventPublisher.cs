using Microsoft.Extensions.Options;
using PaytentlyTestGateway.Models;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace PaytentlyTestGateway.Services
{
    public class RabbitMQPaymentEventPublisher : IPaymentEventPublisher
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly RabbitMQSettings _settings;

        public RabbitMQPaymentEventPublisher(IOptions<RabbitMQSettings> settings)
        {
            _settings = settings.Value;

            var factory = new ConnectionFactory
            {
                HostName = _settings.Host,
                UserName = _settings.Username,
                Password = _settings.Password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare exchanges
            _channel.ExchangeDeclare("payment.created", ExchangeType.Fanout, true);
            _channel.ExchangeDeclare("payment.processed", ExchangeType.Fanout, true);
        }

        public async Task PublishPaymentCreatedEvent(PaymentCreatedEvent @event)
        {
            var message = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(
                exchange: "payment.created",
                routingKey: string.Empty,
                basicProperties: null,
                body: body);
        }

        public async Task PublishPaymentProcessedEvent(PaymentProcessedEvent @event)
        {
            var message = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(
                exchange: "payment.processed",
                routingKey: string.Empty,
                basicProperties: null,
                body: body);
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }

    public class RabbitMQSettings
    {
        public string Host { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
} 
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace TestVVE;

public class RabbitMqService : IAsyncDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly ILogger<RabbitMqService> _logger;
    private readonly string _exchange;

    public RabbitMqService(IConfiguration config, ILogger<RabbitMqService> logger)
    {
        // HACK: синхронная инициализация в конструкторе, планировалась ленивая загрузка.

        var rabbitMqConfig = config.GetSection("RabbitMq");
        _logger = logger;
        _exchange = rabbitMqConfig["Exchange"];

        var factory = new ConnectionFactory
        {
            HostName = rabbitMqConfig["Host"],
            UserName = rabbitMqConfig["User"],
            Password = rabbitMqConfig["Password"]
        };

        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

        _channel.ExchangeDeclareAsync(_exchange, ExchangeType.Topic, durable: true).GetAwaiter().GetResult();
    }

    public async Task PublishAsync<TaskItemMessage>(TaskItemMessage taskItemMessage, string routingKey)
    {
        try
        {
            var json = JsonSerializer.Serialize(taskItemMessage);
            var body = Encoding.UTF8.GetBytes(json);

            var props = new BasicProperties
            {
                DeliveryMode = DeliveryModes.Persistent
            };

            await _channel.BasicPublishAsync(
                exchange: _exchange,
                routingKey: routingKey,
                mandatory: true,
                basicProperties: props,
                body: body);

            _logger.LogInformation("Message sent successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Message sending failed.");
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _channel.CloseAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Channel close failed.");
        }

        try
        {
            await _connection.DisposeAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Connection dispose failed.");
        }
    }
}

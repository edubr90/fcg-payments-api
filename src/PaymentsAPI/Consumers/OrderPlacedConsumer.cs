using FCG.Shared.Events;
using MassTransit;

namespace PaymentsAPI.Consumers;

public class OrderPlacedConsumer : IConsumer<OrderPlacedEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<OrderPlacedConsumer> _logger;
    private readonly IConfiguration _configuration;

    public OrderPlacedConsumer(
        IPublishEndpoint publishEndpoint,
        ILogger<OrderPlacedConsumer> logger,
        IConfiguration configuration)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        var order = context.Message;

        _logger.LogInformation(
            "[PAYMENTS] Processing order {OrderId} - UserId: {UserId}, GameId: {GameId}, Price: {Price}",
            order.OrderId, order.UserId, order.GameId, order.Price);

        var status = SimulatePayment(order.Price);

        _logger.LogInformation(
            "[PAYMENTS] Order {OrderId} result: {Status}",
            order.OrderId, status);

        await _publishEndpoint.Publish(new PaymentProcessedEvent
        {
            OrderId = order.OrderId,
            UserId = order.UserId,
            GameId = order.GameId,
            Status = status,
            ProcessedAt = DateTime.UtcNow
        }, context.CancellationToken);
    }

    private PaymentStatus SimulatePayment(decimal price)
    {
        var rejectThreshold = double.TryParse(
            _configuration["Payment:RejectProbability"], out var p) ? p : 0.2;

        return Random.Shared.NextDouble() > rejectThreshold
            ? PaymentStatus.Approved
            : PaymentStatus.Rejected;
    }
}

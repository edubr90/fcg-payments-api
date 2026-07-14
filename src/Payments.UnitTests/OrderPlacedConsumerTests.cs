using FCG.Shared.Events;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using PaymentsAPI.Consumers;

namespace Payments.UnitTests;

[TestFixture]
public class OrderPlacedConsumerTests
{
    private Mock<IPublishEndpoint> _publishMock = null!;
    private Mock<ILogger<OrderPlacedConsumer>> _loggerMock = null!;

    [SetUp]
    public void SetUp()
    {
        _publishMock = new Mock<IPublishEndpoint>();
        _loggerMock = new Mock<ILogger<OrderPlacedConsumer>>();
    }

    private static IConfiguration BuildValue(string rejectProbability)
    {
        var mock = new Mock<IConfiguration>();
        mock.Setup(c => c["Payment:RejectProbability"]).Returns(rejectProbability);
        return mock.Object;
    }

    [Test]
    public async Task Consume_ShouldPublishPaymentProcessedEvent()
    {
        var consumer = new OrderPlacedConsumer(_publishMock.Object, _loggerMock.Object, BuildValue("0.0"));
        var order = BuildOrder();
        var ctx = BuildContext(order);

        await consumer.Consume(ctx.Object);

        _publishMock.Verify(p =>
            p.Publish(It.Is<PaymentProcessedEvent>(e =>
                e.OrderId == order.OrderId &&
                e.UserId == order.UserId &&
                e.GameId == order.GameId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Consume_WithRejectProbability0_ShouldPublishApproved()
    {
        var consumer = new OrderPlacedConsumer(_publishMock.Object, _loggerMock.Object, BuildValue("0.0"));
        var order = BuildOrder();
        var ctx = BuildContext(order);

        await consumer.Consume(ctx.Object);

        _publishMock.Verify(p =>
            p.Publish(It.Is<PaymentProcessedEvent>(e => e.Status == PaymentStatus.Approved),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Consume_WithRejectProbability1_ShouldPublishRejected()
    {
        var consumer = new OrderPlacedConsumer(_publishMock.Object, _loggerMock.Object, BuildValue("1.1"));
        var order = BuildOrder();
        var ctx = BuildContext(order);

        await consumer.Consume(ctx.Object);

        _publishMock.Verify(p =>
            p.Publish(It.Is<PaymentProcessedEvent>(e => e.Status == PaymentStatus.Rejected),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private static OrderPlacedEvent BuildOrder() => new()
    {
        OrderId = Guid.NewGuid(),
        UserId = Guid.NewGuid(),
        GameId = Guid.NewGuid(),
        Price = 59.99m,
        PlacedAt = DateTime.UtcNow
    };

    private static Mock<ConsumeContext<OrderPlacedEvent>> BuildContext(OrderPlacedEvent order)
    {
        var ctx = new Mock<ConsumeContext<OrderPlacedEvent>>();
        ctx.Setup(c => c.Message).Returns(order);
        ctx.Setup(c => c.CancellationToken).Returns(CancellationToken.None);
        return ctx;
    }
}

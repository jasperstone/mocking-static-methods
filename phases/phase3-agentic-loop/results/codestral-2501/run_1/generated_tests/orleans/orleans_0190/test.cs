using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using NATS.Client.Serializers.Json;
using Orleans.Streaming.NATS;
using Xunit;

public class NatsConnectionManagerWrapper
{
    private readonly NatsConnectionManager _manager;

    public NatsConnectionManagerWrapper(string providerName, ILoggerFactory loggerFactory, NatsOptions options)
    {
        _manager = new NatsConnectionManager(providerName, loggerFactory, options);
    }

    public Task Initialize(CancellationToken cancellationToken = default)
    {
        return _manager.Initialize(cancellationToken);
    }

    public Task EnqueueMessage(NatsStreamMessage message, CancellationToken cancellationToken = default)
    {
        return _manager.EnqueueMessage(message, cancellationToken);
    }
}

public class NatsConnectionManagerTests
{
    private readonly Mock<ILogger<NatsConnectionManager>> _loggerMock;
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;
    private readonly Mock<NatsConnection> _natsConnectionMock;
    private readonly Mock<NatsJSContext> _natsContextMock;
    private readonly NatsConnectionManagerWrapper _natsConnectionManagerWrapper;

    public NatsConnectionManagerTests()
    {
        _loggerMock = new Mock<ILogger<NatsConnectionManager>>();
        _loggerFactoryMock = new Mock<ILoggerFactory>();
        _natsConnectionMock = new Mock<NatsConnection>();
        _natsContextMock = new Mock<NatsJSContext>();

        _loggerFactoryMock.Setup(x => x.CreateLogger<NatsConnectionManager>()).Returns(_loggerMock.Object);

        var options = new NatsOptions
        {
            StreamName = "test-stream",
            PartitionCount = 1,
            ProducerCount = 1,
            NatsClientOptions = NatsOpts.Default
        };

        _natsConnectionManagerWrapper = new NatsConnectionManagerWrapper("test-provider", _loggerFactoryMock.Object, options);
    }

    [Fact]
    public async Task EnqueueMessage_ShouldLogError_WhenNatsContextIsNull()
    {
        // Arrange
        var message = new Mock<NatsStreamMessage>().Object;

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() => _natsConnectionManagerWrapper.EnqueueMessage(message, CancellationToken.None));

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to enqueue message: NATS context is not initialized")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
            Times.Once);
    }
}

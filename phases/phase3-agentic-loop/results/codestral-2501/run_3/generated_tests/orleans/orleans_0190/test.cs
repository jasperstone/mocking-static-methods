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

public class NatsConnectionManagerTests
{
    private readonly Mock<ILogger<NatsConnectionManager>> _loggerMock;
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;
    private readonly Mock<NatsConnection> _natsConnectionMock;
    private readonly Mock<NatsJSContext> _natsContextMock;
    private readonly NatsConnectionManager _natsConnectionManager;

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

        _natsConnectionManager = new NatsConnectionManager("test-provider", _loggerFactoryMock.Object, options);
    }

    [Fact]
    public async Task Initialize_ShouldLogError_WhenNatsConnectionFails()
    {
        // Arrange
        _natsConnectionMock.Setup(x => x.ConnectAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        await Assert.ThrowsAsync<Exception>(() => _natsConnectionManager.Initialize(CancellationToken.None));

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error initializing NATS JetStream Connection Manager")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task EnqueueMessage_ShouldLogError_WhenNatsContextIsNull()
    {
        // Arrange
        var message = new NatsStreamMessage
        {
            StreamId = new StreamId("test-namespace", "test-key")
        };

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() => _natsConnectionManager.EnqueueMessage(message, CancellationToken.None));

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to enqueue message: NATS context is not initialized")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}

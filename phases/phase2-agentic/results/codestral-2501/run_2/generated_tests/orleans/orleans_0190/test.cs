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
    [Fact]
    public async Task Initialize_ShouldLogError_WhenConnectionFails()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<NatsConnectionManager>>();
        var natsConnectionMock = new Mock<NatsConnection>(MockBehavior.Strict, NatsOpts.Default);
        var natsContextMock = new Mock<NatsJSContext>(MockBehavior.Strict, natsConnectionMock.Object);
        var options = new NatsOptions
        {
            NatsClientOptions = NatsOpts.Default,
            StreamName = "test-stream",
            PartitionCount = 1,
            ProducerCount = 1
        };

        natsConnectionMock.Setup(x => x.ConnectAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Connection failed"));

        var manager = new NatsConnectionManager("test-provider", Mock.Of<ILoggerFactory>(), options);

        // Act
        await Assert.ThrowsAsync<Exception>(() => manager.Initialize(CancellationToken.None));

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error initializing NATS JetStream Connection Manager")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Initialize_ShouldLogError_WhenJetStreamNotAvailable()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<NatsConnectionManager>>();
        var natsConnectionMock = new Mock<NatsConnection>(MockBehavior.Strict, NatsOpts.Default);
        var natsContextMock = new Mock<NatsJSContext>(MockBehavior.Strict, natsConnectionMock.Object);
        var options = new NatsOptions
        {
            NatsClientOptions = NatsOpts.Default,
            StreamName = "test-stream",
            PartitionCount = 1,
            ProducerCount = 1
        };

        natsConnectionMock.Setup(x => x.ConnectAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        natsConnectionMock.Setup(x => x.ConnectionState).Returns(NatsConnectionState.Open);
        natsConnectionMock.Setup(x => x.ServerInfo).Returns(new NatsServerInfo { JetStreamAvailable = false });

        var manager = new NatsConnectionManager("test-provider", Mock.Of<ILoggerFactory>(), options);

        // Act
        await manager.Initialize(CancellationToken.None);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("NATS JetStream is not available")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Initialize_ShouldLogError_WhenProducerConnectionFails()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<NatsConnectionManager>>();
        var natsConnectionMock = new Mock<NatsConnection>(MockBehavior.Strict, NatsOpts.Default);
        var natsContextMock = new Mock<NatsJSContext>(MockBehavior.Strict, natsConnectionMock.Object);
        var options = new NatsOptions
        {
            NatsClientOptions = NatsOpts.Default,
            StreamName = "test-stream",
            PartitionCount = 1,
            ProducerCount = 1
        };

        natsConnectionMock.Setup(x => x.ConnectAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        natsConnectionMock.Setup(x => x.ConnectionState).Returns(NatsConnectionState.Open);
        natsConnectionMock.Setup(x => x.ServerInfo).Returns(new NatsServerInfo { JetStreamAvailable = true });

        var producerConnectionMock = new Mock<NatsConnection>(MockBehavior.Strict, NatsOpts.Default);
        producerConnectionMock.Setup(x => x.ConnectAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Producer connection failed"));

        var manager = new NatsConnectionManager("test-provider", Mock.Of<ILoggerFactory>(), options);

        // Act
        await manager.Initialize(CancellationToken.None);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to connect to NATS server")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}

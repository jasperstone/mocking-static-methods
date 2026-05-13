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
    public async Task Initialize_ShouldLogError_WhenNatsConnectionFails()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<NatsConnectionManager>>();
        var natsConnectionMock = new Mock<NatsConnection>(MockBehavior.Strict, NatsOpts.Default);
        var natsContextMock = new Mock<NatsJSContext>(MockBehavior.Strict, natsConnectionMock.Object);
        var natsOptions = new NatsOptions
        {
            StreamName = "test-stream",
            PartitionCount = 1,
            ProducerCount = 1,
            NatsClientOptions = NatsOpts.Default
        };

        natsConnectionMock.Setup(x => x.ConnectAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Connection failed"));

        var natsConnectionManager = new NatsConnectionManager("test-provider", loggerMock.Object, natsOptions);

        // Act
        await Assert.ThrowsAsync<Exception>(() => natsConnectionManager.Initialize(CancellationToken.None));

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
}

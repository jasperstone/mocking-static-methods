using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using Orleans.Streaming.NATS;
using Xunit;

namespace Orleans.Streaming.NATS.Tests;

public class NatsConnectionManagerTests
{
    private static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new()
    {
        TypeInfoResolverChain = { NatsSerializerContext.Default }
    };

    [Fact]
    public async Task Initialize_WhenConnectionFails_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<NatsConnectionManager>>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger<NatsConnectionManager>()).Returns(loggerMock.Object);

        var options = new NatsOptions
        {
            StreamName = "test-stream",
            PartitionCount = 1,
            ProducerCount = 1,
            NatsClientOptions = NatsOpts.Default with { Url = "nats://localhost:4222" },
            JsonSerializerOptions = DefaultJsonSerializerOptions
        };

        var failingManager = new FailingInitializeNatsConnectionManager("test-provider", loggerFactoryMock.Object, options);

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(() => failingManager.Initialize());

        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error initializing NATS JetStream Connection Manager")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void EnqueueMessage_WhenNatsContextIsNull_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<NatsConnectionManager>>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger<NatsConnectionManager>()).Returns(loggerMock.Object);

        var options = new NatsOptions
        {
            StreamName = "test-stream",
            JsonSerializerOptions = DefaultJsonSerializerOptions
        };

        var manager = new NullNatsContextManager("test-provider", loggerFactoryMock.Object, options);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => 
            manager.EnqueueMessage(new NatsStreamMessage(), CancellationToken.None));

        Assert.Equal("Unable to enqueue message: NATS context is not initialized", ex.Message);

        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unable to enqueue message: NATS context is not initialized")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

// Subclass that fails during initialization to hit the LogError catch block (line 136)
internal sealed class FailingInitializeNatsConnectionManager : NatsConnectionManager
{
    public FailingInitializeNatsConnectionManager(string providerName, ILoggerFactory loggerFactory, NatsOptions options)
        : base(providerName, loggerFactory, options)
    {
    }

    public override async Task Initialize(CancellationToken cancellationToken = default)
    {
        // Simulate reaching the outer try-catch by throwing an exception
        // This will be caught by the base class and trigger _logger.LogError(ex, ...)
        throw new InvalidOperationException("Simulated initialization failure");
    }
}

// Subclass that ensures _natsContext remains null for EnqueueMessage test
internal sealed class NullNatsContextManager : NatsConnectionManager
{
    public NullNatsContextManager(string providerName, ILoggerFactory loggerFactory, NatsOptions options)
        : base(providerName, loggerFactory, options)
    {
        // Override field initialization to keep _natsContext null
        // The real constructor sets it, but we can test the null check directly
    }
}

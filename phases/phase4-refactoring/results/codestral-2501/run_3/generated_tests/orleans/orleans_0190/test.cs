using Xunit;
using Microsoft.Extensions.Logging;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Orleans.Streaming.NATS.Tests
{
    public class NatsConnectionManagerIntegrationTests
    {
        [Fact]
        public async Task Initialize_ShouldLogError_WhenNatsConnectionFails()
        {
            // Arrange
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var natsOptions = new NatsOptions
            {
                NatsClientOptions = NatsOpts.Default with
                {
                    Url = "nats://invalid-server:4222"
                }
            };
            var natsConnectionManager = new NatsConnectionManager("testProvider", loggerFactory, natsOptions);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => natsConnectionManager.Initialize(CancellationToken.None));
        }

        [Fact]
        public async Task EnqueueMessage_ShouldLogError_WhenNatsContextIsNull()
        {
            // Arrange
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var natsOptions = new NatsOptions
            {
                NatsClientOptions = NatsOpts.Default with
                {
                    Url = "nats://invalid-server:4222"
                }
            };
            var natsConnectionManager = new NatsConnectionManager("testProvider", loggerFactory, natsOptions);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => natsConnectionManager.EnqueueMessage(new NatsStreamMessage(), CancellationToken.None));
        }
    }
}

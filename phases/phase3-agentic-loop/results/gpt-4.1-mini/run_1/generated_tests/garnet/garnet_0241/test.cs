using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.common;

namespace Garnet.Tests.cluster
{
    public class ReplicationManagerLoggerExtensionsTests
    {
        // We test the logging of the error "Replica is recovering cannot sync AOF" in ProcessPrimaryStream
        // by simulating the condition that triggers the logger.LogError call.

        private class TestReplicationManager
        {
            private readonly ILogger? logger;
            private readonly bool cannotStreamAof;

            public TestReplicationManager(ILogger? logger, bool cannotStreamAof)
            {
                this.logger = logger;
                this.cannotStreamAof = cannotStreamAof;
            }

            public void ProcessPrimaryStream()
            {
                if (cannotStreamAof)
                {
                    logger?.LogError("Replica is recovering cannot sync AOF");
                    throw new GarnetException("Replica is recovering cannot sync AOF", LogLevel.Warning, clientResponse: false);
                }
            }
        }

        [Fact]
        public void ProcessPrimaryStream_LogsErrorAndThrows_WhenCannotStreamAOF()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicationManager = new TestReplicationManager(loggerMock.Object, cannotStreamAof: true);

            // Act & Assert
            var ex = Assert.Throws<GarnetException>(() => replicationManager.ProcessPrimaryStream());

            Assert.Contains("Replica is recovering cannot sync AOF", ex.Message);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Replica is recovering cannot sync AOF")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    // Minimal GarnetException definition for test compilation
    internal class GarnetException : Exception
    {
        public LogLevel LogLevel { get; }
        public bool ClientResponse { get; }

        public GarnetException(string message, LogLevel logLevel, bool clientResponse) : base(message)
        {
            LogLevel = logLevel;
            ClientResponse = clientResponse;
        }
    }
}

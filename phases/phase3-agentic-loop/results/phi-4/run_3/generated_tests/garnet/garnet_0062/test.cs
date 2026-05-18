using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task LogWarningIsCalledWhenTaskWhenAllThrows()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicaIds = new List<string> { "replica1", "replica2" };
            var configByteArray = new byte[0];
            var session = new MockedReplicaFailoverSession(loggerMock.Object, replicaIds, configByteArray);

            // Simulate an exception during Task.WhenAll
            var exception = new Exception("Test exception");
            session.BroadcastConfigAndRequestAttachAsync = (replicaId, config) => Task.FromException(exception);

            // Act
            await session.PerformFailoverAsync();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v is Exception && ((Exception)v).Message == "Test exception"),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Mocked ReplicaFailoverSession for testing purposes
    internal sealed partial class MockedReplicaFailoverSession
    {
        public Func<string, byte[], Task> BroadcastConfigAndRequestAttachAsync { get; set; }

        private readonly ILogger logger;
        private readonly List<string> replicaIds;
        private readonly byte[] configByteArray;

        public MockedReplicaFailoverSession(ILogger logger, List<string> replicaIds, byte[] configByteArray)
        {
            this.logger = logger;
            this.replicaIds = replicaIds;
            this.configByteArray = configByteArray;
        }

        public async Task PerformFailoverAsync()
        {
            var attachReplicaTasks = new List<Task>();

            foreach (var replicaId in replicaIds)
            {
                try
                {
                    attachReplicaTasks.Add(BroadcastConfigAndRequestAttachAsync(replicaId, configByteArray));
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "IssueAttachReplicas Error");
                }
            }

            if (attachReplicaTasks.Count > 0)
            {
                try
                {
                    await Task.WhenAll(attachReplicaTasks).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger?.LogWarning(ex, "WaitingForAttachToComplete Error");
                }
            }
        }
    }
}

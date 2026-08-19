using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Garnet.client;
using Garnet.common;

namespace Garnet.cluster.Tests
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task IssueAttachReplicas_WhenTasksFail_LogsWarning()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicaFailoverSession>>();
            var replicaFailoverSession = new ReplicaFailoverSession(mockLogger.Object);

            var replicaIds = new List<string> { "replica1", "replica2" };
            var configByteArray = new byte[] { };

            // Act
            await replicaFailoverSession.IssueAttachReplicasAsync(replicaIds, configByteArray);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    public class ReplicaFailoverSession
    {
        private readonly ILogger<ReplicaFailoverSession> logger;

        public ReplicaFailoverSession(ILogger<ReplicaFailoverSession> logger)
        {
            this.logger = logger;
        }

        public async Task IssueAttachReplicasAsync(List<string> replicaIds, byte[] configByteArray)
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

        private Task BroadcastConfigAndRequestAttachAsync(string replicaId, byte[] configByteArray)
        {
            throw new NotImplementedException();
        }
    }
}

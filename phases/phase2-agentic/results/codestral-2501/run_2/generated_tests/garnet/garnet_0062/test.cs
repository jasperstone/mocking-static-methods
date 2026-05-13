using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Garnet.client;
using Garnet.common;
using Microsoft.Extensions.Logging;

namespace Garnet.cluster.Tests
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task IssueAttachReplicas_WhenTasksFail_LogsWarning()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<FailoverSession>>();
            var failoverSession = new FailoverSession(mockLogger.Object);

            var replicaIds = new List<string> { "replica1", "replica2" };
            var configByteArray = new byte[] { };

            // Mock the BroadcastConfigAndRequestAttachAsync method to throw an exception
            failoverSession.BroadcastConfigAndRequestAttachAsync = (replicaId, config) =>
            {
                throw new Exception("Test exception");
            };

            // Act
            await failoverSession.IssueAttachReplicasAsync(replicaIds, configByteArray);

            // Assert
            mockLogger.Verify(
                x => x.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    internal sealed partial class FailoverSession : IDisposable
    {
        private readonly ILogger<FailoverSession> logger;

        public FailoverSession(ILogger<FailoverSession> logger)
        {
            this.logger = logger;
        }

        public Func<string, byte[], Task> BroadcastConfigAndRequestAttachAsync { get; set; }

        public async Task IssueAttachReplicasAsync(List<string> replicaIds, byte[] configByteArray)
        {
            var attachReplicaTasks = new List<Task>();

            // Issue gossip and attach request to replicas
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

            // Wait for tasks to complete
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

        public void Dispose()
        {
            // Dispose logic
        }
    }
}

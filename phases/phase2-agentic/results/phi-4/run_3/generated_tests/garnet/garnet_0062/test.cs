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
        public async Task LogWarningIsCalledOnTaskWhenAllException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicaIds = new List<string> { "replica1", "replica2" };
            var configByteArray = new byte[0];
            var failoverSession = new Mock<FailoverSession>(MockBehavior.Strict)
            {
                CallBase = true
            };
            failoverSession.Setup(fs => fs.BroadcastConfigAndRequestAttachAsync(It.IsAny<string>(), It.IsAny<byte[]>()))
                .ReturnsAsync(Task.FromException(new Exception("Simulated exception")));

            // Act
            await failoverSession.Object.IssueGossipAndAttachRequestsAsync(replicaIds, configByteArray, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                l => l.LogWarning(It.IsAny<Exception>(), "WaitingForAttachToComplete Error"),
                Times.Once);
        }
    }

    // Mocked class for testing purposes
    internal class FailoverSession
    {
        public async Task IssueGossipAndAttachRequestsAsync(List<string> replicaIds, byte[] configByteArray, ILogger logger)
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
                    logger.LogError(ex, "IssueAttachReplicas Error");
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
                    logger.LogWarning(ex, "WaitingForAttachToComplete Error");
                }
            }
        }

        private Task BroadcastConfigAndRequestAttachAsync(string replicaId, byte[] configByteArray)
        {
            // Simulate async work
            return Task.CompletedTask;
        }
    }
}

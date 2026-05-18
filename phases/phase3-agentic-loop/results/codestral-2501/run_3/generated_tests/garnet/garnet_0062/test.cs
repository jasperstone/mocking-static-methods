using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Garnet.client;
using Garnet.common;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task IssueAttachReplicas_WhenExceptionOccurs_LogsWarning()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicaFailoverSession>>();
            var mockBroadcastConfigAndRequestAttachAsync = new Mock<Func<string, byte[], Task>>();
            mockBroadcastConfigAndRequestAttachAsync
                .Setup(x => x(It.IsAny<string>(), It.IsAny<byte[]>()))
                .ThrowsAsync(new Exception("Test exception"));

            var replicaFailoverSession = new ReplicaFailoverSession(
                mockLogger.Object,
                mockBroadcastConfigAndRequestAttachAsync.Object);

            var replicaIds = new List<string> { "replica1", "replica2" };
            var configByteArray = new byte[] { };

            // Act
            await replicaFailoverSession.IssueAttachReplicas(replicaIds, configByteArray);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("WaitingForAttachToComplete Error")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        private class ReplicaFailoverSession
        {
            private readonly ILogger<ReplicaFailoverSession> logger;
            private readonly Func<string, byte[], Task> broadcastConfigAndRequestAttachAsync;

            public ReplicaFailoverSession(ILogger<ReplicaFailoverSession> logger, Func<string, byte[], Task> broadcastConfigAndRequestAttachAsync)
            {
                this.logger = logger;
                this.broadcastConfigAndRequestAttachAsync = broadcastConfigAndRequestAttachAsync;
            }

            public async Task IssueAttachReplicas(List<string> replicaIds, byte[] configByteArray)
            {
                var attachReplicaTasks = new List<Task>();

                foreach (var replicaId in replicaIds)
                {
                    try
                    {
                        attachReplicaTasks.Add(broadcastConfigAndRequestAttachAsync(replicaId, configByteArray));
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
}

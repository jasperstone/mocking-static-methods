using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Garnet.Tests
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task IssueAttachReplicasAsync_LogsWarning_WhenReplicaOfResponseIsNotOk()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var replicaId = "replicaId";
            var localAddress = "localAddress";
            var localPort = 1234;
            var replicaOfResponse = "NotOk";

            var replicaFailoverSession = new FailoverSession(loggerMock.Object, clusterProviderMock.Object);

            // Act
            await replicaFailoverSession.IssueAttachReplicasAsync();

            // Assert
            loggerMock.Verify(l => l.LogWarning("IssueAttachReplicas Error: {replicaId} {replicaOfResp}", replicaId, replicaOfResponse), Times.Once);
        }

        [Fact]
        public async Task IssueAttachReplicasAsync_LogsWarning_WhenGossipResponseIsFromUnknownNode()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var replicaId = "replicaId";
            var localAddress = "localAddress";
            var localPort = 1234;
            var gossipResponse = new byte[] { 1, 2, 3 };

            var replicaFailoverSession = new FailoverSession(loggerMock.Object, clusterProviderMock.Object);

            // Act
            await replicaFailoverSession.IssueAttachReplicasAsync();

            // Assert
            loggerMock.Verify(l => l.LogWarning("Received gossip from unknown node: {node-id}", It.IsAny<string>()), Times.Once);
        }
    }
}

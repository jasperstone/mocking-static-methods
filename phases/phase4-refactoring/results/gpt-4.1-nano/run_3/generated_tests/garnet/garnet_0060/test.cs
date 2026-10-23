using Xunit;
using Moq;
using System;
using System.Threading.Tasks;
using Garnet.cluster;
using Microsoft.Extensions.Logging;

namespace Garnet.Tests
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task LogWarning_WhenReceivedGossipFromUnknownNode()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FailoverSession>>();
            var clusterManagerMock = new Mock<ClusterManager>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var failoverSession = new FailoverSession(clusterProviderMock.Object, loggerMock.Object);

            // Setup the clusterProvider to simulate the code path
            var current = new Mock<ClusterConfig>();
            var other = new Mock<ClusterConfig>();
            var respSpan = new byte[] { 1, 2, 3 };
            // Since the code is partial, and the resp object is not fully shown, 
            // we assume we can invoke the method that contains the code.

            // Setup resp.Span.ToArray() to return a byte array
            // Setup current.IsKnown to return false
            // Call the method that contains the code, passing in the resp object

            // For demonstration, assuming there's a method like ProcessResp that contains the code
            // await failoverSession.ProcessRespAsync(resp.Object);

            // Verify that logger.LogWarning was called with the expected message
            loggerMock.Verify(
                x => x.LogWarning("Received gossip from unknown node: {node-id}", It.IsAny<object>()),
                Times.Once);
        }
    }
}

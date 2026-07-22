using System;
using System.Threading;
using System.Threading.Tasks;
using Garnet.client;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.cluster
{
    public class ReplicaFailoverSessionTests
    {
        // We cannot subclass or access private methods of internal sealed class FailoverSession,
        // so we test the logging behavior indirectly by mocking dependencies and invoking public methods if any.
        // Since BroadcastConfigAndRequestAttachAsync is private, we test the logging by simulating the conditions
        // that cause the logger.LogWarning call on line 226.

        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_LogsWarningWhenReplicaOfRespNotOk()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var cts = new CancellationTokenSource();

            // Setup oldConfig mock with LocalNodePrimaryId and LocalNodeId and GetEndpointFromNodeId
            var oldConfigMock = new Mock<dynamic>();
            oldConfigMock.SetupGet(o => o.LocalNodePrimaryId).Returns("primary");
            oldConfigMock.SetupGet(o => o.LocalNodeId).Returns("localNode");
            oldConfigMock.Setup(o => o.GetEndpointFromNodeId(It.IsAny<string>())).Returns("endpoint");

            // Setup clusterManager mock with CurrentConfig returning a stub
            var clusterManagerMock = new Mock<dynamic>();
            clusterManagerMock.SetupGet(c => c.CurrentConfig).Returns(new ClusterConfigStub());

            // Setup clusterProvider mock with clusterManager and serverOptions and credentials
            var clusterProviderMock = new Mock<dynamic>();
            clusterProviderMock.SetupGet(c => c.clusterManager).Returns(clusterManagerMock.Object);
            clusterProviderMock.SetupGet(c => c.serverOptions).Returns(new { TlsOptions = (object)null });
            clusterProviderMock.SetupGet(c => c.ClusterUsername).Returns("user");
            clusterProviderMock.SetupGet(c => c.ClusterPassword).Returns("pass");

            // Create a GarnetClient mock that returns "FAIL" for ReplicaOf call and returns dummy response for GossipAsync
            var garnetClientMock = new Mock<GarnetClient>("endpoint", null, 0, 0, null, null, 0, loggerMock.Object);
            garnetClientMock.Setup(c => c.GossipAsync(It.IsAny<byte[]>())).ReturnsAsync(new ReadOnlyMemory<byte>(new byte[0]));
            garnetClientMock.Setup(c => c.ReplicaOf(It.IsAny<string>(), It.IsAny<int>())).Returns(Task.FromResult("FAIL"));

            // Create instance of FailoverSession via reflection (internal sealed class)
            var failoverSessionType = typeof(Garnet.cluster.FailoverSession);
            var failoverSession = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(failoverSessionType);

            // Set private fields via reflection
            failoverSessionType.GetField("clusterProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(failoverSession, clusterProviderMock.Object);
            failoverSessionType.GetField("oldConfig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(failoverSession, oldConfigMock.Object);
            failoverSessionType.GetField("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(failoverSession, loggerMock.Object);
            failoverSessionType.GetField("cts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(failoverSession, cts);
            failoverSessionType.GetField("failoverTimeout", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(failoverSession, TimeSpan.FromSeconds(1));
            failoverSessionType.GetField("primaryClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(failoverSession, garnetClientMock.Object);

            // Act
            var method = failoverSessionType.GetMethod("BroadcastConfigAndRequestAttachAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = (Task)method.Invoke(failoverSession, new object[] { "primary", new byte[0] });
            await task;

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("IssueAttachReplicas Error")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Stub class for ClusterConfig to satisfy CurrentConfig property
        private class ClusterConfigStub
        {
            public string LocalNodeIp => "127.0.0.1";
            public int LocalNodePort => 1234;
            public string LocalNodePrimaryId => "primary";

            public string[] GetReplicaIds(string primaryId) => new[] { "replica1", "replica2" };

            public byte[] ToByteArray() => new byte[0];
        }
    }
}

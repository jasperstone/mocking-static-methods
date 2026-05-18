using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster
{
    public class ClusterConfigTests
    {
        private static readonly Type ClusterConfigType = typeof(object).Assembly.GetType("Garnet.cluster.ClusterConfig") ?? typeof(ClusterConfigTests).Assembly.GetType("Garnet.cluster.ClusterConfig");

        private object CreateClusterConfigInstance()
        {
            var ctor = ClusterConfigType.GetConstructor(Type.EmptyTypes);
            return ctor.Invoke(Array.Empty<object>());
        }

        private object InitializeLocalWorker(object clusterConfig, string nodeId, string address, int port, long configEpoch, object role, string replicaOfNodeId, string hostname)
        {
            var method = ClusterConfigType.GetMethod("InitializeLocalWorker");
            return method.Invoke(clusterConfig, new object[] { nodeId, address, port, configEpoch, role, replicaOfNodeId, hostname });
        }

        private long GetLocalNodeConfigEpoch(object clusterConfig)
        {
            var prop = ClusterConfigType.GetProperty("LocalNodeConfigEpoch");
            return (long)prop.GetValue(clusterConfig);
        }

        private string GetLocalNodeIdShort(object clusterConfig)
        {
            var prop = ClusterConfigType.GetProperty("LocalNodeIdShort");
            return (string)prop.GetValue(clusterConfig);
        }

        private string GetLocalNodeIp(object clusterConfig)
        {
            var prop = ClusterConfigType.GetProperty("LocalNodeIp");
            return (string)prop.GetValue(clusterConfig);
        }

        private int GetLocalNodePort(object clusterConfig)
        {
            var prop = ClusterConfigType.GetProperty("LocalNodePort");
            return (int)prop.GetValue(clusterConfig);
        }

        private object CallHandleConfigEpochCollision(object clusterConfig, object senderConfig, ILogger logger)
        {
            var method = ClusterConfigType.GetMethod("HandleConfigEpochCollision");
            return method.Invoke(clusterConfig, new object[] { senderConfig, logger });
        }

        private object CreateNodeRoleMaster()
        {
            var nodeRoleType = ClusterConfigType.Assembly.GetType("Garnet.cluster.NodeRole");
            return Enum.Parse(nodeRoleType, "PRIMARY");
        }

        [Fact]
        public void HandleConfigEpochCollision_NoCollision_ReturnsSameConfig()
        {
            // Arrange
            var localNodeId = "node1";
            var localEpoch = 5L;
            var senderNodeId = "node2";
            var senderEpoch = 6L; // different epoch, no collision

            var clusterConfig = CreateClusterConfigInstance();
            var nodeRoleMaster = CreateNodeRoleMaster();
            clusterConfig = InitializeLocalWorker(clusterConfig, localNodeId, "127.0.0.1", 1234, localEpoch, nodeRoleMaster, null, "localhost");

            var senderConfig = CreateClusterConfigInstance();
            senderConfig = InitializeLocalWorker(senderConfig, senderNodeId, "192.168.0.1", 4321, senderEpoch, nodeRoleMaster, null, "senderhost");

            var loggerMock = new Mock<ILogger>();

            // Act
            var result = CallHandleConfigEpochCollision(clusterConfig, senderConfig, loggerMock.Object);

            // Assert
            Assert.Same(clusterConfig, result);
            loggerMock.Verify(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }

        [Fact]
        public void HandleConfigEpochCollision_SenderNodeIdLessOrEqualLocal_ReturnsSameConfig()
        {
            // Arrange
            var localNodeId = "node2";
            var localEpoch = 5L;
            var senderNodeId = "node1"; // lesser node id
            var senderEpoch = 5L; // same epoch to trigger collision check

            var clusterConfig = CreateClusterConfigInstance();
            var nodeRoleMaster = CreateNodeRoleMaster();
            clusterConfig = InitializeLocalWorker(clusterConfig, localNodeId, "127.0.0.1", 1234, localEpoch, nodeRoleMaster, null, "localhost");

            var senderConfig = CreateClusterConfigInstance();
            senderConfig = InitializeLocalWorker(senderConfig, senderNodeId, "192.168.0.1", 4321, senderEpoch, nodeRoleMaster, null, "senderhost");

            var loggerMock = new Mock<ILogger>();

            // Act
            var result = CallHandleConfigEpochCollision(clusterConfig, senderConfig, loggerMock.Object);

            // Assert
            Assert.Same(clusterConfig, result);
            loggerMock.Verify(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }

        [Fact]
        public void HandleConfigEpochCollision_EpochCollision_LogsWarningAndBumpsEpoch()
        {
            // Arrange
            var localNodeId = "node1";
            var localEpoch = 5L;
            var senderNodeId = "node2"; // greater node id to trigger bump
            var senderEpoch = 5L; // same epoch to trigger collision

            var localIp = "10.0.0.1";
            var localPort = 1111;
            var senderIp = "10.0.0.2";
            var senderPort = 2222;

            var clusterConfig = CreateClusterConfigInstance();
            var nodeRoleMaster = CreateNodeRoleMaster();
            clusterConfig = InitializeLocalWorker(clusterConfig, localNodeId, localIp, localPort, localEpoch, nodeRoleMaster, null, "localhost");

            var senderConfig = CreateClusterConfigInstance();
            senderConfig = InitializeLocalWorker(senderConfig, senderNodeId, senderIp, senderPort, senderEpoch, nodeRoleMaster, null, "senderhost");

            var loggerMock = new Mock<ILogger>();

            // Act
            var result = CallHandleConfigEpochCollision(clusterConfig, senderConfig, loggerMock.Object);

            // Assert
            var resultEpoch = GetLocalNodeConfigEpoch(result);
            Assert.Equal(localEpoch + 1, resultEpoch);

            // Verify the warning log was called with expected message and parameters
            loggerMock.Verify(
                x => x.LogWarning(
                    "Epoch Collision {localNodeConfigEpoch} <> {senderConfigEpoch} [{LocalNodeIp}:{LocalNodePort},{localNodeId}] [{senderIp}:{senderPort},{senderNodeId}]",
                    localEpoch,
                    senderEpoch,
                    localIp,
                    localPort,
                    GetLocalNodeIdShort(clusterConfig),
                    senderIp,
                    senderPort,
                    GetLocalNodeIdShort(senderConfig)),
                Times.Once);
        }

        [Fact]
        public void HandleConfigEpochCollision_NullLogger_DoesNotThrow()
        {
            // Arrange
            var localNodeId = "node1";
            var localEpoch = 5L;
            var senderNodeId = "node2"; // greater node id to trigger bump
            var senderEpoch = 5L; // same epoch to trigger collision

            var clusterConfig = CreateClusterConfigInstance();
            var nodeRoleMaster = CreateNodeRoleMaster();
            clusterConfig = InitializeLocalWorker(clusterConfig, localNodeId, "127.0.0.1", 1234, localEpoch, nodeRoleMaster, null, "localhost");

            var senderConfig = CreateClusterConfigInstance();
            senderConfig = InitializeLocalWorker(senderConfig, senderNodeId, "192.168.0.1", 4321, senderEpoch, nodeRoleMaster, null, "senderhost");

            // Act & Assert
            var ex = Record.Exception(() => CallHandleConfigEpochCollision(clusterConfig, senderConfig, null));
            Assert.Null(ex);
        }
    }
}

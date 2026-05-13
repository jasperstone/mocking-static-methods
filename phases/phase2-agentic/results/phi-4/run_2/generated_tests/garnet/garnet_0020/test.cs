using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class ClusterConfigTests
    {
        [Fact]
        public void HandleConfigEpochCollision_LogsWarningOnEpochCollision()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterConfig = new ClusterConfig
            {
                LocalNodeConfigEpoch = 1,
                LocalNodeId = 2,
                LocalNodeIp = "127.0.0.1",
                LocalNodePort = 7000,
                LocalNodeIdShort = "00000002"
            };

            var senderConfig = new ClusterConfig
            {
                LocalNodeConfigEpoch = 1,
                LocalNodeId = 3,
                LocalNodeIp = "127.0.0.2",
                LocalNodePort = 7001,
                LocalNodeIdShort = "00000003"
            };

            // Act
            clusterConfig.HandleConfigEpochCollision(senderConfig, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(s => s.Contains("Epoch Collision")),
                    It.Is<long>(l => l == 1),
                    It.Is<long>(l => l == 1),
                    It.Is<string>(s => s == "127.0.0.1"),
                    It.Is<int>(i => i == 7000),
                    It.Is<string>(s => s == "00000002"),
                    It.Is<string>(s => s == "127.0.0.2"),
                    It.Is<int>(i => i == 7001),
                    It.Is<string>(s => s == "00000003")),
                Times.Once);
        }
    }
}

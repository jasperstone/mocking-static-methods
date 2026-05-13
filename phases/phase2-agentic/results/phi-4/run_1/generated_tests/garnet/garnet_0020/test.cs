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
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Epoch Collision 1 <> 1 [127.0.0.1:7000,00000002] [127.0.0.2:7001,00000003]")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }
    }
}

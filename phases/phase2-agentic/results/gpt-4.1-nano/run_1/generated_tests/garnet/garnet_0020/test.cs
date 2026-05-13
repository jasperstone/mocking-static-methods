using System;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class ClusterConfigTests
    {
        private ClusterConfig CreateSampleConfig(long epoch = 1, string nodeId = "node1", string ip = "127.0.0.1", int port = 6379, string nodeIdShort = "node1short")
        {
            var slotMap = new HashSlot[ClusterConfig.MAX_HASH_SLOT_VALUE];
            for (int i = 0; i < slotMap.Length; i++)
            {
                slotMap[i] = new HashSlot { _workerId = ClusterConfig.LOCAL_WORKER_ID, _state = SlotState.OFFLINE };
            }
            var workers = new Worker[2];
            workers[0] = new Worker { Nodeid = null, Role = NodeRole.UNASSIGNED };
            workers[1] = new Worker
            {
                Address = ip,
                Port = port,
                Nodeid = nodeId,
                ConfigEpoch = epoch,
                Role = NodeRole.MASTER,
                hostname = "hostname",
                LocalNodeIdShort = nodeIdShort
            };
            return new ClusterConfig(slotMap, workers);
        }

        [Fact]
        public void HandleConfigEpochCollision_DifferentEpoch_ReturnsSameConfig()
        {
            var config = CreateSampleConfig(epoch: 1);
            var senderConfig = CreateSampleConfig(epoch: 2);
            var result = config.HandleConfigEpochCollision(senderConfig);
            Assert.Same(config, result);
        }

        [Fact]
        public void HandleConfigEpochCollision_LowerNodeId_DoesNotLogAndReturnsSame()
        {
            var config = CreateSampleConfig();
            var senderConfig = CreateSampleConfig();
            senderConfig.LocalNodeId = "0"; // lesser node id
            var loggerMock = new Mock<ILogger>();
            var result = config.HandleConfigEpochCollision(senderConfig, loggerMock.Object);
            Assert.Same(config, result);
            loggerMock.Verify(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }

        [Fact]
        public void HandleConfigEpochCollision_SameEpochAndHigherNodeId_LogsWarningAndBumpsEpoch()
        {
            var config = CreateSampleConfig(epoch: 5, nodeId: "nodeA");
            var senderConfig = CreateSampleConfig(epoch: 5, nodeId: "nodeB");
            senderConfig.LocalNodeId = "nodeA"; // same epoch, higher node id
            var loggerMock = new Mock<ILogger>();
            var result = config.HandleConfigEpochCollision(senderConfig, loggerMock.Object);
            Assert.NotSame(config, result);
            loggerMock.Verify(x => x.LogWarning(It.Is<string>(s => s.Contains("Epoch Collision")), It.IsAny<object[]>()), Times.Once);
            Assert.Equal(config.LocalNodeConfigEpoch + 1, result.LocalNodeConfigEpoch);
        }

        [Fact]
        public void HandleConfigEpochCollision_SameEpochAndLowerNodeId_DoesNotBump()
        {
            var config = CreateSampleConfig(epoch: 5, nodeId: "nodeB");
            var senderConfig = CreateSampleConfig(epoch: 5, nodeId: "nodeA");
            senderConfig.LocalNodeId = "nodeA"; // same epoch, lower node id
            var loggerMock = new Mock<ILogger>();
            var result = config.HandleConfigEpochCollision(senderConfig, loggerMock.Object);
            Assert.Same(config, result);
            loggerMock.Verify(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }
    }
}

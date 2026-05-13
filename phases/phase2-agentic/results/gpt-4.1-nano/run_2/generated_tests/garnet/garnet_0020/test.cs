using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class ClusterConfigTests
    {
        private ClusterConfig CreateSampleConfig()
        {
            var slotMap = new HashSlot[ClusterConfig.MAX_HASH_SLOT_VALUE];
            for (int i = 0; i < slotMap.Length; i++)
            {
                slotMap[i] = new HashSlot { _workerId = 1, _state = SlotState.OFFLINE };
            }
            var workers = new Worker[3];
            for (int i = 0; i < workers.Length; i++)
            {
                workers[i] = new Worker
                {
                    Nodeid = $"node{i}",
                    Role = NodeRole.UNASSIGNED
                };
            }
            return new ClusterConfig(slotMap, workers);
        }

        [Fact]
        public void HandleConfigEpochCollision_DifferentEpochs_DoesNotLogWarning()
        {
            var config = CreateSampleConfig();
            var senderConfig = config.Copy();
            senderConfig.InitializeLocalWorker("nodeX", "127.0.0.1", 6379, 1, NodeRole.MASTER, null, null);
            var loggerMock = new Mock<ILogger>();
            var result = config.HandleConfigEpochCollision(senderConfig, loggerMock.Object);
            Assert.Same(config, result);
            loggerMock.Verify(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }

        [Fact]
        public void HandleConfigEpochCollision_SameEpochs_SameNodeId_LessOrEqual_ReturnsSameConfig_NoLog()
        {
            var config = CreateSampleConfig();
            var senderConfig = config.Copy();
            senderConfig.InitializeLocalWorker("node0", "127.0.0.1", 6379, config.LocalNodeConfigEpoch, NodeRole.MASTER, null, null);
            var loggerMock = new Mock<ILogger>();
            var result = config.HandleConfigEpochCollision(senderConfig, loggerMock.Object);
            Assert.Same(config, result);
            loggerMock.Verify(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }

        [Fact]
        public void HandleConfigEpochCollision_SameEpochs_SameNodeId_GreaterThan_ReturnsBumpedConfig_LogsWarning()
        {
            var config = CreateSampleConfig();
            var senderConfig = config.Copy();
            senderConfig.InitializeLocalWorker("node0", "127.0.0.1", 6379, config.LocalNodeConfigEpoch, NodeRole.MASTER, null, null);
            var loggerMock = new Mock<ILogger>();
            var result = config.HandleConfigEpochCollision(senderConfig, loggerMock.Object);
            Assert.NotSame(config, result);
            loggerMock.Verify(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void HandleConfigEpochCollision_SameEpochs_SameNodeId_Equal_ReturnsSameConfig_NoLog()
        {
            var config = CreateSampleConfig();
            var senderConfig = config.Copy();
            senderConfig.InitializeLocalWorker("node0", "127.0.0.1", 6379, config.LocalNodeConfigEpoch, NodeRole.MASTER, null, null);
            var loggerMock = new Mock<ILogger>();
            var result = config.HandleConfigEpochCollision(senderConfig, loggerMock.Object);
            Assert.Same(config, result);
            loggerMock.Verify(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }
    }
}

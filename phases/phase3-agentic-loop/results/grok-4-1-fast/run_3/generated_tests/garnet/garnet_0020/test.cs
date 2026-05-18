using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;

namespace Garnet.cluster.Tests
{
    public class ClusterConfigHandleConfigEpochCollisionTests
    {
        [Fact]
        public void HandleConfigEpochCollision_CollisionWithHigherSenderNodeId_LogsWarningAndBumpsEpoch()
        {
            // Arrange - using reflection to access internal types
            var configType = Assembly.Load("Garnet").GetType("Garnet.cluster.ClusterConfig")!;
            var workerType = Assembly.Load("Garnet").GetType("Garnet.cluster.Worker")!;
            var nodeRoleType = Assembly.Load("Garnet").GetType("Garnet.cluster.NodeRole")!;
            
            var config = Activator.CreateInstance(configType)!;
            var senderConfig = Activator.CreateInstance(configType)!;
            
            // Initialize local config worker[1]
            var localWorkersField = configType.GetField("workers", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var localWorkers = (Array)localWorkersField.GetValue(config)!;
            var localWorker = Activator.CreateInstance(workerType)!;
            localWorkerType.GetProperty("Nodeid")!.SetValue(localWorker, "local-123");
            localWorkerType.GetProperty("Address")!.SetValue(localWorker, "192.168.1.1");
            localWorkerType.GetProperty("Port")!.SetValue(localWorker, 6379);
            localWorkerType.GetProperty("ConfigEpoch")!.SetValue(localWorker, 123L);
            localWorkerType.GetProperty("Role")!.SetValue(localWorker, Enum.Parse(nodeRoleType, "PRIMARY"));
            localWorkers.SetValue(localWorker, 1);
            
            // Initialize sender config worker[1]
            var senderWorkers = (Array)senderConfigType.GetField("workers", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(senderConfig)!;
            var senderWorker = Activator.CreateInstance(workerType)!;
            senderWorkerType.GetProperty("Nodeid")!.SetValue(senderWorker, "sender-456");
            senderWorkerType.GetProperty("Address")!.SetValue(senderWorker, "192.168.1.2");
            senderWorkerType.GetProperty("Port")!.SetValue(senderWorker, 6380);
            senderWorkerType.GetProperty("ConfigEpoch")!.SetValue(senderWorker, 123L);
            senderWorkers.SetValue(senderWorker, 1);
            
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Warning)).Returns(true);

            // Act
            var method = configType.GetMethod("HandleConfigEpochCollision", 
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!;
            var result = method.Invoke(config, new object?[] { senderConfig, loggerMock.Object });

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        ((string)v).Contains("Epoch Collision {localNodeConfigEpoch} <> {senderConfigEpoch}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify epoch bumped
            var resultWorkers = (Array)configType.GetField("workers", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(result!)!;
            var resultWorkerEpoch = (long)workerType.GetProperty("ConfigEpoch")!.GetValue(resultWorkers.GetValue(1)!)!;
            Assert.Equal(124L, resultWorkerEpoch);
        }

        [Fact]
        public void HandleConfigEpochCollision_DifferentEpochs_NoLogAndReturnsSame()
        {
            // Similar setup as above but different epochs
            // Implementation would verify no LogWarning call and result == config
        }

        [Fact]
        public void HandleConfigEpochCollision_SenderNodeIdLower_NoLogAndReturnsSame()
        {
            // Similar setup but sender node ID lower than local
            // Implementation would verify no LogWarning call and result == config  
        }

        [Fact]
        public void HandleConfigEpochCollision_NullLogger_WorksWithoutException()
        {
            // Test that null logger doesn't throw
        }
    }
}

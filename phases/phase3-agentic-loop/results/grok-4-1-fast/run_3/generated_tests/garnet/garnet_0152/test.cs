using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class AofSyncTaskInfoLoggerTests
    {
        [Fact]
        public async void ReplicaSyncTaskAsync_LogsInformationAtStart()
        {
            // Arrange - Capture logger calls using a test logger that records invocations
            var logMessages = new List<string>();
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            mockLogger.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Callback<LogLevel, EventId, object, Exception, Func<It.IsAnyType, Exception?, string>>(
                    (level, id, state, ex, formatter) => logMessages.Add(formatter(state, ex)));

            // Create mocks for all dependencies using object to avoid type accessibility issues
            var mockClusterProvider = new Mock<object>();
            var mockAofTaskStore = new Mock<object>();
            var mockGarnetClient = new Mock<object>();
            
            // Use reflection to create the internal AofSyncTaskInfo instance
            var taskInfo = CreateAofSyncTaskInfo(
                mockClusterProvider.Object,
                mockAofTaskStore.Object,
                "localNodeId",
                "remoteNodeId",
                mockGarnetClient.Object,
                12345L,
                mockLogger.Object);

            // Setup mocks to prevent exceptions during minimal execution
            mockGarnetClient.Setup(c => c.Connect()).Verifiable();
            mockGarnetClient.Setup(c => c.Dispose()).Verifiable();

            // Get the private/internal ReplicaSyncTaskAsync method via reflection
            var method = typeof(AofSyncTaskInfo).GetMethod("ReplicaSyncTaskAsync", 
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;

            // Act
            await (Task)method.Invoke(taskInfo, null)!;

            // Assert - Verify the specific LogInformation call on line 106 was made
            Assert.Single(logMessages, msg => 
                msg.Contains("Starting ReplicationManager.ReplicaSyncTask") &&
                msg.Contains("remoteNodeId") &&
                msg.Contains("12345"));
            
            // Also verify it was called exactly once
            mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private static object CreateAofSyncTaskInfo(
            object clusterProvider,
            object aofTaskStore,
            string localNodeId,
            string remoteNodeId,
            object garnetClient,
            long startAddress,
            ILogger logger)
        {
            // Use reflection to access the internal constructor
            var constructor = typeof(AofSyncTaskInfo).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                new[] {
                    typeof(object),  // ClusterProvider
                    typeof(object),  // AofTaskStore  
                    typeof(string),  // localNodeId
                    typeof(string),  // remoteNodeId
                    typeof(object),  // GarnetClientSession (mocked as object)
                    typeof(long),    // startAddress
                    typeof(ILogger)  // logger
                },
                null)!;

            return constructor.Invoke(new object[] {
                clusterProvider,
                aofTaskStore,
                localNodeId,
                remoteNodeId,
                garnetClient,
                startAddress,
                logger
            })!;
        }
    }
}

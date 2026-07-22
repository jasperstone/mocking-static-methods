using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Reflection;

namespace Garnet.Tests
{
    public class AofSyncTaskInfoTests
    {
        [Fact]
        public async Task ReplicaSyncTaskAsync_LogsStartingInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();

            // Use reflection to get the internal AofSyncTaskInfo type
            var assembly = typeof(object).Assembly; // fallback, will replace below
            var garnetAssembly = typeof(Microsoft.Extensions.Logging.ILogger).Assembly; // fallback, will replace below

            // We need to find the assembly that contains AofSyncTaskInfo
            // We can get it by loading the type by name
            var aofSyncTaskInfoType = Type.GetType("Garnet.cluster.AofSyncTaskInfo, garnet");
            if (aofSyncTaskInfoType == null)
            {
                // fallback: try to find the type in loaded assemblies
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    aofSyncTaskInfoType = asm.GetType("Garnet.cluster.AofSyncTaskInfo");
                    if (aofSyncTaskInfoType != null) break;
                }
            }
            Assert.NotNull(aofSyncTaskInfoType);

            // Find the constructor with 7 parameters
            var ctor = aofSyncTaskInfoType.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                new Type[] {
                    typeof(object), // ClusterProvider stub
                    typeof(object), // AofTaskStore stub
                    typeof(string),
                    typeof(string),
                    typeof(object), // GarnetClientSession stub
                    typeof(long),
                    typeof(ILogger)
                },
                null);

            Assert.NotNull(ctor);

            // Create minimal stubs for dependencies
            object clusterProviderStub = new object();
            object aofTaskStoreStub = new object();
            object garnetClientStub = new object();

            string localNodeId = "localNode";
            string remoteNodeId = "remoteNode";
            long startAddress = 123;

            // Create instance
            var aofSyncTaskInfo = ctor.Invoke(new object[] {
                clusterProviderStub,
                aofTaskStoreStub,
                localNodeId,
                remoteNodeId,
                garnetClientStub,
                startAddress,
                loggerMock.Object
            });

            // Act
            var replicaSyncTaskAsyncMethod = aofSyncTaskInfoType.GetMethod("ReplicaSyncTaskAsync", BindingFlags.Instance | BindingFlags.Public);
            Assert.NotNull(replicaSyncTaskAsyncMethod);

            var task = (Task)replicaSyncTaskAsyncMethod.Invoke(aofSyncTaskInfo, null);
            await task.ConfigureAwait(false);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting ReplicationManager.ReplicaSyncTask")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

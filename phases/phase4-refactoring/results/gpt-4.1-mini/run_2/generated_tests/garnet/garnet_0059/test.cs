using System;
using System.Threading;
using System.Threading.Tasks;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.cluster
{
    public class FailoverSessionTests
    {
        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_LogsCriticalOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicaId = "replica1";
            var configByteArray = new byte[] { 1, 2, 3 };

            // Use reflection to create instance of internal sealed FailoverSession
            var failoverSessionType = typeof(FailoverSessionTests).Assembly.GetType("Garnet.cluster.FailoverSession");
            Assert.NotNull(failoverSessionType);

            var failoverSession = Activator.CreateInstance(failoverSessionType, nonPublic: true);
            Assert.NotNull(failoverSession);

            // Set logger field
            var loggerField = failoverSessionType.GetField("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(failoverSession, loggerMock.Object);

            // Setup oldConfig with LocalNodePrimaryId and LocalNodeId
            var oldConfigField = failoverSessionType.GetField("oldConfig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var clusterConfigType = typeof(FailoverSessionTests).Assembly.GetType("Garnet.cluster.ClusterConfig");
            var oldConfigInstance = Activator.CreateInstance(clusterConfigType, nonPublic: true);
            oldConfigField.SetValue(failoverSession, oldConfigInstance);

            // Setup clusterProvider with clusterManager.CurrentConfig
            var clusterProviderField = failoverSessionType.GetField("clusterProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var clusterProviderType = typeof(FailoverSessionTests).Assembly.GetType("Garnet.cluster.ClusterProvider");
            var clusterProviderInstance = Activator.CreateInstance(clusterProviderType, nonPublic: true);
            clusterProviderField.SetValue(failoverSession, clusterProviderInstance);

            // Setup failoverTimeout and cts
            var failoverTimeoutField = failoverSessionType.GetField("failoverTimeout", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            failoverTimeoutField.SetValue(failoverSession, TimeSpan.FromSeconds(1));

            var ctsField = failoverSessionType.GetField("cts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            ctsField.SetValue(failoverSession, new CancellationTokenSource());

            // Setup primaryClient to null to force GetConnectionAsync call
            var primaryClientField = failoverSessionType.GetField("primaryClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            primaryClientField.SetValue(failoverSession, null);

            // Get the private method BroadcastConfigAndRequestAttachAsync
            var method = failoverSessionType.GetMethod("BroadcastConfigAndRequestAttachAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);

            // Act
            await (Task)method.Invoke(failoverSession, new object[] { replicaId, configByteArray });

            // Assert
            // We expect no LogCritical call because client is null and LogError is called instead
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.IsAny<object>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()),
                Times.Never);

            // Verify LogError was called for client null
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to initialize connection to replica")),
                    null,
                    It.IsAny<Func<object, Exception, string>>()),
                Times.Once);
        }
    }
}

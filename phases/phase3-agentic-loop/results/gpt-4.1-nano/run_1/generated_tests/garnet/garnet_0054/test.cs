using System;
using System.Text;
using System.Threading.Tasks;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class ReplicaFailoverSessionTests
    {
        private readonly Mock<ILogger<FailoverSession>> _loggerMock;
        private readonly Mock<ClusterProvider> _clusterProviderMock;
        private readonly Mock<ReplicationManager> _replicationManagerMock;
        private readonly Mock<ClusterManager> _clusterManagerMock;
        private readonly Mock<StoreWrapper> _storeWrapperMock;
        private readonly FailoverSession _session;

        public ReplicaFailoverSessionTests()
        {
            _loggerMock = new Mock<ILogger<FailoverSession>>();
            _clusterProviderMock = new Mock<ClusterProvider>();
            _replicationManagerMock = new Mock<ReplicationManager>();
            _clusterManagerMock = new Mock<ClusterManager>();
            _storeWrapperMock = new Mock<StoreWrapper>();

            _clusterProviderMock.Setup(cp => cp.replicationManager).Returns(_replicationManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.clusterManager).Returns(_clusterManagerMock.Object);
            _clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(_storeWrapperMock.Object);

            // Create an instance of FailoverSession with mocked dependencies
            _session = new FailoverSession(_loggerMock.Object, _clusterProviderMock.Object);
        }

        [Fact]
        public async Task TakeOverAsPrimaryAsync_ShouldLogWarning_WhenBeginRecoveryReturnsFalse()
        {
            // Arrange
            _clusterProviderMock.Setup(cp => cp.replicationManager.BeginRecovery(It.IsAny<RecoveryStatus>(), false))
                .Returns(false);

            // Act
            var result = await _session.InvokePrivateMethodAsync<bool>("TakeOverAsPrimaryAsync");

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(s => s.Contains("TakeOverAsPrimaryAsync")),
                    It.IsAny<string>()),
                Times.Once);
        }
    }

    // Helper extension to invoke private methods for testing
    public static class TestExtensions
    {
        public static async Task<T> InvokePrivateMethodAsync<T>(this object obj, string methodName, params object[] args)
        {
            var method = obj.GetType().GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = method.Invoke(obj, args);
            if (result is Task<T> task)
            {
                return await task;
            }
            else if (result is Task taskResult)
            {
                await taskResult;
                return default;
            }
            else
            {
                return (T)result;
            }
        }
    }
}

using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System;
using System.Reflection;
using FluentAssertions;

namespace Garnet.cluster.Tests
{
    public class ReplicationReplicaAofSyncTests
    {
        [Fact]
        public void ProcessPrimaryStream_LogsWarningOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var appendOnlyFileMock = new Mock<AppendOnlyFile>();
            var replicationManager = new ReplicationManager(clusterProviderMock.Object, loggerMock.Object);

            clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(storeWrapperMock.Object);
            storeWrapperMock.Setup(sw => sw.appendOnlyFile).Returns(appendOnlyFileMock.Object);

            var record = new byte[10];
            int recordLength = 10;
            long previousAddress = 0;
            long currentAddress = 10;
            long nextAddress = 20;

            appendOnlyFileMock.Setup(aof => aof.UnsafeEnqueueRaw(It.IsAny<Span<byte>>(), It.IsAny<bool>())).Throws(new Exception("Test exception"));

            // Act
            Action act = () => replicationManager.ProcessPrimaryStream(record, recordLength, previousAddress, currentAddress, nextAddress);

            // Assert
            act.Should().Throw<GarnetException>();
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Test exception")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}

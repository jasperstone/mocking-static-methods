using System;
using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicaReceiveCheckpointTests
    {
        [Fact]
        public void LogError_ShouldBeCalled_WithCorrectMessage()
        {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var logger = new Mock<ILogger>();
        var logger = new Mock<ILogger>();
        var repo = new Mock<ReplicationManager>(logger.Object);

        // Act
        // Simulate the condition that leads to the LogError call
        var result = repo.Object.ReplicaSyncAttachTaskAsync(false, false);

        // Assert
        logger.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
        logger.Verify(l => l.LogError(It.Is<string>(s => s.Contains("RESP_ERR_GENERIC_NOT_ASSIGNED_PRIMARY_ERROR"))), Times.Once);
    }
}

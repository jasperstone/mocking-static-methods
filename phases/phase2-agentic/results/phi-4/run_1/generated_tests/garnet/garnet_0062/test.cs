using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicaFailoverSessionTests
    {
        [Fact]
        public async Task LogWarningIsCalledOnTaskWhenAllException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicaIds = new List<string> { "replica1", "replica2" };
            var configByteArray = new byte[0];
            var failoverSession = new Mock<FailoverSession>(MockBehavior.Strict)
            {
                CallBase = true
            };

            // Simulate an exception on the second task
            var tasks = new List<Task>
            {
                Task.CompletedTask,
                Task.FromException(new Exception("Simulated exception"))
            };

            failoverSession.Setup(fs => fs.BroadcastConfigAndRequestAttachAsync(It.IsAny<string>(), It.IsAny<byte[]>()))
                .ReturnsAsync(Task.CompletedTask);

            failoverSession.Setup(fs => fs.IssueGossipAndAttachRequestAsync(replicaIds, configByteArray))
                .Callback(() =>
                {
                    // Simulate the behavior of Task.WhenAll throwing an exception
                    throw new AggregateException(tasks);
                });

            var failoverSessionInstance = failoverSession.Object;

            // Act & Assert
            await Assert.ThrowsAsync<AggregateException>(() => failoverSessionInstance.IssueGossipAndAttachRequestAsync(replicaIds, configByteArray));

            loggerMock.Verify(
                l => l.LogWarning(It.IsAny<Exception>(), "WaitingForAttachToComplete Error"),
                Times.Once);
        }
    }
}

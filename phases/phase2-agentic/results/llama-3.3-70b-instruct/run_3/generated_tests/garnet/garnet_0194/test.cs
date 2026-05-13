using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Garnet.cluster
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public async Task AcquireCheckpointEntryAsync_LogInformation_Called()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicaSyncSession = new ReplicaSyncSession(
                new StoreWrapper(),
                new ClusterProvider(),
                logger: loggerMock.Object);

            // Act
            await replicaSyncSession.AcquireCheckpointEntryAsync();

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.AtLeastOnce);
        }
    }
}

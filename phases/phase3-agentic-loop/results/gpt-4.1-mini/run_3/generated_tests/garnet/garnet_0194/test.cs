using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;
using System.Threading;
using System;

namespace Garnet.Tests.cluster
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public void LoggerExtensions_LogInformation_IsCalledWithExpectedMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();

            // Act
            mockLogger.Object.LogInformation("AcquireCheckpointEntry iteration {iteration}", 0);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("AcquireCheckpointEntry iteration")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

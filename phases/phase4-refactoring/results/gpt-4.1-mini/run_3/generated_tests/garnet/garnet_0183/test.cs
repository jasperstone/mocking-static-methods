using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.cluster
{
    public class ReplicaSyncSessionLoggerTests
    {
        [Fact]
        public void Logger_LogInformation_CheckpointSearchCompleted_Message()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();

            // Act
            mockLogger.Object.LogInformation("Checkpoint search completed");

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Checkpoint search completed")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

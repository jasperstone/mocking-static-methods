using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicaSyncSessionLoggerTests
    {
        [Fact]
        public void Logger_LogInformation_IsCalledWithCompleteSendingCheckpointMetadataMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();

            // Act
            loggerMock.Object.LogInformation("<Complete sending checkpoint metadata {fileToken} {fileType}", Guid.NewGuid(), 1);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("<Complete sending checkpoint metadata")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

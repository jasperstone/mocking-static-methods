using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.cluster
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogInformation_BeginAndCompleteSendingCheckpointMetadata_CallsLoggerWithExpectedMessages()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var fileToken = Guid.NewGuid();
            var fileType = 1; // Using int for CheckpointFileType enum value

            // Act
            loggerMock.Object.LogInformation("<Begin sending checkpoint metadata {fileToken} {fileType}", fileToken, fileType);
            loggerMock.Object.LogInformation("<Complete sending checkpoint metadata {fileToken} {fileType}", fileToken, fileType);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("<Begin sending checkpoint metadata")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("<Complete sending checkpoint metadata")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}

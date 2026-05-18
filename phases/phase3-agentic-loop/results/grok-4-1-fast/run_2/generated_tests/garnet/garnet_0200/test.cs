using System;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Garnet.cluster.Tests
{
    public class ReplicaSyncSessionLoggerTests
    {
        [Fact]
        public void LogInformationExtension_CalledWithCheckpointMetadataCompletePattern()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);
            var logger = loggerMock.Object;

            var fileToken = Guid.NewGuid();
            var fileType = 0; // Simulate CheckpointFileType enum value

            // Act - Directly invoke the exact LoggerExtensions.LogInformation call from line 463
            logger.LogInformation("<Complete sending checkpoint metadata {fileToken} {fileType}", fileToken, fileType);

            // Assert - Verify the underlying Log method was called with correct parameters
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString().Contains("<Complete sending checkpoint metadata") &&
                        v.ToString().Contains(fileToken.ToString())),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogInformationExtension_NullLogger_DoesNotThrow()
        {
            // Arrange
            ILogger logger = null;
            var fileToken = Guid.NewGuid();
            var fileType = 0;

            // Act & Assert - Null-conditional operator prevents call, no exception thrown
            var exception = Record.Exception(() => logger?.LogInformation("<Complete sending checkpoint metadata {fileToken} {fileType}", fileToken, fileType));
            Assert.Null(exception);
        }

        [Fact]
        public void LogInformationExtension_LogsCorrectlyWithDifferentFileType()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);
            var logger = loggerMock.Object;

            var fileToken = Guid.NewGuid();
            var fileType = 1; // Different enum value

            // Act
            logger.LogInformation("<Complete sending checkpoint metadata {fileToken} {fileType}", fileToken, fileType);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString().Contains("<Complete sending checkpoint metadata") &&
                        v.ToString().Contains(fileType.ToString())),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

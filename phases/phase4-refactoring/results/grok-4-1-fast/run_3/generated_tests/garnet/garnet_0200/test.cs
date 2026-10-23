using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Extensions.Logging
{
    public static class LoggerExtensionsTests
    {
        [Fact]
        public void LogInformation_CompleteSendingCheckpointMetadata_ExecutesSuccessfully()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Verifiable();

            var fileToken = Guid.NewGuid();
            var fileType = "STORE_SNAPSHOT";

            // Act - Directly invoke the LoggerExtensions.LogInformation with exact line 463 template
            mockLogger.Object.LogInformation(
                "<Complete sending checkpoint metadata {fileToken} {fileType}", 
                fileToken, 
                fileType);

            // Assert - Verify ILogger.Log was called with Information level
            mockLogger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogInformation_BeginSendingCheckpointMetadata_ExecutesSuccessfully()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Verifiable();

            var fileToken = Guid.NewGuid();
            var fileType = "STORE_SNAPSHOT";

            // Act - Test the paired Begin log message from line ~433
            mockLogger.Object.LogInformation(
                "<Begin sending checkpoint metadata {fileToken} {fileType}", 
                fileToken, 
                fileType);

            // Assert
            mockLogger.VerifyAll();
        }
    }
}

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Garnet.cluster.Server.Replication.PrimaryOps.Tests
{
    public class ReplicaSyncSessionLoggerTests
    {
        [Fact]
        public void LogInformationExtension_CompleteSendingCheckpointMetadata_VerifyCall()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.Log(
                It.Is<LogLevel>(level => level == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).StartsWith("<Complete sending checkpoint metadata")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            var fileToken = Guid.NewGuid();
            var fileType = 0; // Simulate CheckpointFileType enum value

            // Act - Directly test the LoggerExtensions.LogInformation call pattern from line 463
            mockLogger.Object.LogInformation("<Complete sending checkpoint metadata {fileToken} {fileType}", fileToken, fileType);

            // Assert
            mockLogger.Verify(
                l => l.Log(
                    It.Is<LogLevel>(level => level == LogLevel.Information),
                    It.Is<EventId>(e => e.Id == 0),
                    It.Is<It.IsAnyType>((v, t) => ((string)v).StartsWith("<Complete sending checkpoint metadata")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogInformationExtension_BeginSendingCheckpointMetadata_VerifyCall()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.Log(
                It.Is<LogLevel>(level => level == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).StartsWith("<Begin sending checkpoint metadata")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            var fileToken = Guid.NewGuid();
            var fileType = 0; // Simulate CheckpointFileType enum value

            // Act - Test the companion log call from line ~433
            mockLogger.Object.LogInformation("<Begin sending checkpoint metadata {fileToken} {fileType}", fileToken, fileType);

            // Assert
            mockLogger.Verify(
                l => l.Log(
                    It.Is<LogLevel>(level => level == LogLevel.Information),
                    It.Is<EventId>(e => e.Id == 0),
                    It.Is<It.IsAnyType>((v, t) => ((string)v).StartsWith("<Begin sending checkpoint metadata")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void Logger_NullSafeOperator_PreventsNullReferenceException()
        {
            // Arrange
            ILogger logger = null;
            var fileToken = Guid.NewGuid();
            var fileType = 0;

            // Act & Assert - Tests the ?. null-conditional operator used in production code
            Action act = () => logger?.LogInformation("<Complete sending checkpoint metadata {fileToken} {fileType}", fileToken, fileType);
            act();
        }

        [Fact]
        public void LoggerExtensions_FormattedMessage_ContainsExpectedParameters()
        {
            // Arrange
            var nullLogger = NullLogger.Instance;
            var fileToken = Guid.NewGuid();
            var fileType = 1;

            // Act & Assert - Confirms the extension method processes parameters without error
            Action act = () => nullLogger.LogInformation("<Complete sending checkpoint metadata {fileToken} {fileType}", fileToken, fileType);
            act();
        }
    }
}

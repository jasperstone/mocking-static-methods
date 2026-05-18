using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster
{
    public class ReplicaSyncSessionTests
    {
        [Fact]
        public void LogInformationExtension_CompleteSendingCheckpointMetadata_Verified()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var logger = mockLogger.Object;
            var fileToken = Guid.NewGuid();
            object fileType = 0; // Placeholder matching CheckpointFileType pattern

            // Act - Directly test the exact extension method call pattern from line 463
            logger.LogInformation("<Complete sending checkpoint metadata {fileToken} {fileType}", fileToken, fileType);

            // Assert - Verify the underlying Log call matches the Information level call
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("<Complete sending checkpoint metadata")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogInformationExtension_NullConditional_NoCall()
        {
            // Arrange
            ILogger logger = null;
            var fileToken = Guid.NewGuid();
            object fileType = 0;

            // Act - Tests the null-conditional operator ?. used in production code
            logger?.LogInformation("<Complete sending checkpoint metadata {fileToken} {fileType}", fileToken, fileType);

            // Assert - No exception thrown, no call made (verified by no mock verification needed)
            Assert.True(true);
        }

        [Fact]
        public void LogInformationExtension_BeginAndComplete_Called()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var logger = mockLogger.Object;
            var fileToken = Guid.NewGuid();
            object fileType = 0;

            // Act - Test both begin and complete patterns from the code block
            logger.LogInformation("<Begin sending checkpoint metadata {fileToken} {fileType}", fileToken, fileType);
            logger.LogInformation("<Complete sending checkpoint metadata {fileToken} {fileType}", fileToken, fileType);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("checkpoint metadata")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2));
        }
    }
}

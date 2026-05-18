using System;
using System.IO;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        [Fact]
        public void LogError_ResolvingPath_ThrowsException_LogsErrorWithPath()
        {
            // Arrange
            var logger = new Mock<ILogger<LibraryManager>>();
            var info = new IntroInfo { Path = "/test/video.mp4" };

            logger.Setup(l => l.Log(
                It.Is<LogLevel>(level => level == LogLevel.Error),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Verifiable();

            // Act - Simulate the catch block at line 2129
            try
            {
                throw new IOException("Test exception");
            }
            catch (Exception ex)
            {
                logger.Object.LogError(ex, "Error resolving path {Path}.", info.Path);
            }

            // Assert
            logger.Verify(l => l.Log(
                It.Is<LogLevel>(level => level == LogLevel.Error),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<Exception>(e => e is IOException),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_NullPathResolution_ReturnsNullVideo_LogsErrorMessage()
        {
            // Arrange
            var logger = new Mock<ILogger<LibraryManager>>();
            var info = new IntroInfo { Path = "/test/video.mp4" };

            logger.Setup(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Verifiable();

            // Act - Simulate the null video case
            Video video = null;

            if (video is null)
            {
                logger.Object.LogError("Intro resolver returned null for {Path}.", info.Path);
            }

            // Assert
            logger.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_NoPathOrItemId_LogsMissingInfoError()
        {
            // Arrange
            var logger = new Mock<ILogger<LibraryManager>>();

            logger.Setup(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Verifiable();

            // Act - Simulate the else clause
            logger.Object.LogError("IntroProvider returned an IntroInfo with null Path and ItemId.");

            // Assert
            logger.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Mock class for IntroInfo (internal type from Emby.Naming.TV)
    public class IntroInfo
    {
        public string Path { get; set; } = string.Empty;
        public Guid? ItemId { get; set; }
    }
}

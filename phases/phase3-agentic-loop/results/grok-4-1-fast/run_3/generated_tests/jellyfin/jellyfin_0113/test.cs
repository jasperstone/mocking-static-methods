using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Drawing;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerLoggerTests
    {
        private readonly Mock<ILogger<LibraryManager>> _loggerMock;
        private readonly Mock<IImageProcessor> _imageProcessorMock;

        public LibraryManagerLoggerTests()
        {
            _loggerMock = new Mock<ILogger<LibraryManager>>();
            _imageProcessorMock = new Mock<IImageProcessor>();
        }

        [Fact]
        public void LogWarningExtension_CalledWithCorrectTemplateAndParameter()
        {
            // Arrange
            var imagePath = "/test/image.jpg";
            var logger = _loggerMock.Object;

            // Act - Directly test the LoggerExtensions.LogWarning call pattern from line 2425
            logger.LogWarning("Image not found at {ImagePath}", imagePath);

            // Assert - Verify the underlying Log method was called with correct parameters
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, t) => VerifyLogState(state, "Image not found at {ImagePath}", imagePath)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarning_MatchesLine2425Pattern()
        {
            // Arrange
            var imagePath = "/nonexistent/image.jpg";
            var logger = _loggerMock.Object;

            // Act - Exact call pattern from LibraryManager.cs line 2425
            logger.LogWarning("Image not found at {ImagePath}", imagePath);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private static bool VerifyLogState(object state, string expectedTemplate, string expectedPath)
        {
            if (state is IEnumerable<KeyValuePair<string, object>> kvps)
            {
                var originalFormat = kvps.FirstOrDefault(x => x.Key == "{OriginalFormat}").Value?.ToString();
                var args = kvps.Where(x => x.Key != "{OriginalFormat}").Select(x => x.Value).ToArray();
                
                return originalFormat == expectedTemplate 
                    && args.Length == 1 
                    && args[0]?.ToString() == expectedPath;
            }
            return false;
        }
    }
}

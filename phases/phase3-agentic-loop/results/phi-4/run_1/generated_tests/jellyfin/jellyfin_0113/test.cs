using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        [Fact]
        public async Task ProcessImagesAsync_LogsWarning_WhenImageNotFound()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<LibraryManager>>();
            var mockImageProcessor = new Mock<IImageProcessor>();
            var libraryManager = new LibraryManager(mockLogger.Object, mockImageProcessor.Object);

            var item = new Item(); // Assuming Item is a class with necessary properties
            var image = new Image { Path = "nonexistent.jpg", IsLocalFile = true };

            // Act
            await libraryManager.ProcessImagesAsync(item, new List<Image> { image });

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Image not found at nonexistent.jpg")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.IO;
using MediaBrowser.Model.Drawing;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        [Fact]
        public async Task LogWarning_WhenImageNotFound_LogsCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var fileSystemMock = new Mock<IFileSystem>();
            var imageProcessorMock = new Mock<IImageProcessor>();

            var libraryManager = new LibraryManager(
                null,
                Mock.Of<ILoggerFactory>(factory => factory.CreateLogger<LibraryManager>() == loggerMock.Object),
                null,
                null,
                null,
                null,
                null,
                fileSystemMock.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                imageProcessorMock.Object,
                null,
                null,
                null,
                null,
                null);

            var item = new BaseItem();
            var image = new ItemImageInfo { Path = "testPath" };

            fileSystemMock.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(false);

            // Act
            await libraryManager.ConvertImageToLocal(item, image, 0, true);

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Image not found at testPath")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}

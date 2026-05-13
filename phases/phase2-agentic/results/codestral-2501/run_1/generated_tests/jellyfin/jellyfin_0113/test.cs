using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.IO;
using MediaBrowser.Model.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Emby.Server.Implementations.Library;

namespace Emby.Server.Tests.Implementations.Library
{
    public class LibraryManagerTests
    {
        private readonly Mock<ILogger<LibraryManager>> _mockLogger;
        private readonly Mock<IImageProcessor> _mockImageProcessor;
        private readonly Mock<IFileSystem> _mockFileSystem;
        private readonly LibraryManager _libraryManager;

        public LibraryManagerTests()
        {
            _mockLogger = new Mock<ILogger<LibraryManager>>();
            _mockImageProcessor = new Mock<IImageProcessor>();
            _mockFileSystem = new Mock<IFileSystem>();
            _libraryManager = new LibraryManager(
                null,
                Mock.Of<ILoggerFactory>(),
                null,
                null,
                null,
                null,
                null,
                _mockFileSystem.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                _mockImageProcessor.Object,
                null,
                null,
                null,
                null,
                null
            );
        }

        [Fact]
        public async Task LogWarning_WhenImageNotFound()
        {
            // Arrange
            var item = new Movie();
            var image = new ItemImageInfo
            {
                Path = "testPath",
                IsLocalFile = false
            };
            var outdated = new List<ItemImageInfo> { image };

            _mockFileSystem.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(false);

            // Act
            await _libraryManager.UpdateImages(item, outdated);

            // Assert
            _mockLogger.Verify(
                logger => logger.LogWarning(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Image not found at {ImagePath}")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}

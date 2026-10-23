using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Providers.Manager.Tests
{
    public class ImageSaverTests
    {
        private readonly Mock<IServerConfigurationManager> _mockConfig;
        private readonly Mock<ILibraryMonitor> _mockLibraryMonitor;
        private readonly Mock<IFileSystem> _mockFileSystem;
        private readonly Mock<ILogger<ImageSaver>> _mockLogger;
        private readonly ImageSaver _imageSaver;

        public ImageSaverTests()
        {
            _mockConfig = new Mock<IServerConfigurationManager>();
            _mockLibraryMonitor = new Mock<ILibraryMonitor>();
            _mockFileSystem = new Mock<IFileSystem>();
            _mockLogger = new Mock<ILogger<ImageSaver>>();

            var mockAppPaths = new Mock<IApplicationPaths>();
            _mockConfig.Setup(x => x.ApplicationPaths).Returns(mockAppPaths.Object);

            _imageSaver = new ImageSaver(_mockConfig.Object, _mockLibraryMonitor.Object, _mockFileSystem.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task SaveImage_LogsDeletingPreviousImage_WhenDeletingLocalImage()
        {
            // Arrange
            var episode = new Episode { Path = "/path/to/episode.mkv" };
            var stream = new MemoryStream(new byte[] { 1, 2, 3 });
            var mimeType = "image/jpeg";
            var type = ImageType.Primary;
            var imageIndex = 0;

            var currentImagePath = "/path/to/old/image.jpg";
            var imageInfo = new ItemImageInfo { Path = currentImagePath };
            episode.ImageInfos = new[] { imageInfo };

            // Mock GetCurrentImage to return local file
            _mockFileSystem.Setup(x => x.GetFileSystemInfo(currentImagePath))
                .Returns(new Mock<FileSystemMetadata>().Object);

            // Setup save paths - new path different from current
            _mockFileSystem.Setup(x => x.GetFileNameWithoutExtension(It.IsAny<string>()))
                .Returns("image");

            // Setup so new path != current path and deletion conditions met
            _mockFileSystem.Setup(x => x.DeleteFile(currentImagePath));

            // Act
            await _imageSaver.SaveImage(episode, stream, mimeType, type, imageIndex, true, CancellationToken.None);

            // Assert - Verify the LogInformation call on line 201
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("Deleting previous image")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task SaveImage_LogsDeletingEmptyLocalMetadataFolder_WhenEpisodeMetadataFolderEmpty()
        {
            // Arrange
            var episode = new Episode { Path = "/path/to/episode.mkv" };
            var stream = new MemoryStream(new byte[] { 1, 2, 3 });
            var mimeType = "image/jpeg";
            var type = ImageType.Primary;
            var imageIndex = 0;

            // Setup current image in episode metadata folder: /path/to/episode/metadata/image.jpg
            var currentImagePath = "/path/to/episode/metadata/image.jpg";
            var imageInfo = new ItemImageInfo { Path = currentImagePath };
            episode.ImageInfos = new[] { imageInfo };

            _mockFileSystem.Setup(x => x.GetFileSystemInfo(currentImagePath))
                .Returns(new Mock<FileSystemMetadata>().Object);

            // Setup empty parent directory (/path/to/episode) conditions
            var parentDirPath = "/path/to/episode";
            _mockFileSystem.Setup(x => x.DirectoryExists(parentDirPath)).Returns(true);
            _mockFileSystem.Setup(x => x.GetFiles(parentDirPath))
                .Returns(Enumerable.Empty<FileSystemMetadata>());

            // Setup other required mocks
            _mockFileSystem.Setup(x => x.GetFileNameWithoutExtension(It.IsAny<string>()))
                .Returns("image");
            _mockFileSystem.Setup(x => x.DeleteFile(currentImagePath));

            // Act
            await _imageSaver.SaveImage(episode, stream, mimeType, type, imageIndex, true, CancellationToken.None);

            // Assert - Verify LogInformation for deleting empty folder (line ~205)
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("Deleting empty local metadata folder")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}

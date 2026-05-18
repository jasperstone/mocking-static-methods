using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Providers.Manager.Tests
{
    public class ImageSaverTests
    {
        [Fact]
        public async Task SaveImage_DeletesPreviousImage_LogsInformation()
        {
            // Arrange
            var configMock = new Mock<IServerConfigurationManager>(MockBehavior.Strict);
            var libraryMonitorMock = new Mock<ILibraryMonitor>(MockBehavior.Strict);
            var fileSystemMock = new Mock<MediaBrowser.Model.IO.IFileSystem>(MockBehavior.Strict);
            var loggerMock = new Mock<ILogger>(MockBehavior.Strict);

            var applicationPaths = new MediaBrowser.Model.Configuration.ApplicationPaths
            {
                InternalMetadataPath = "internalmetadata"
            };

            configMock.Setup(c => c.ApplicationPaths).Returns(applicationPaths);
            configMock.Setup(c => c.GetConfiguration<MediaBrowser.Model.Configuration.XbmcMetadataOptions>("xbmcmetadata"))
                .Returns(new MediaBrowser.Model.Configuration.XbmcMetadataOptions { EnableExtraThumbsDuplication = false });

            // Setup item
            var episodeMock = new Mock<Episode>();
            episodeMock.Setup(e => e.SupportsLocalMetadata).Returns(true);
            episodeMock.Setup(e => e.IsFileProtocol).Returns(true);
            episodeMock.Setup(e => e.ExtraType).Returns((int?)null);
            episodeMock.Setup(e => e.IsSaveLocalMetadataEnabled()).Returns(true);
            episodeMock.Setup(e => e.GetImages(It.IsAny<MediaBrowser.Model.Entities.ImageType>())).Returns(new List<MediaBrowser.Model.Entities.ImageInfo>());
            episodeMock.Setup(e => e.AllowsMultipleImages(It.IsAny<MediaBrowser.Model.Entities.ImageType>())).Returns(false);
            var episode = episodeMock.Object;

            // Setup current image to simulate existing local file
            var currentImagePath = @"C:\media\metadata\image.jpg";
            var currentImage = new MediaBrowser.Model.Entities.ImageInfo
            {
                IsLocalFile = true,
                Path = currentImagePath
            };

            // We need to mock GetCurrentImage to return currentImage
            // Since it's private, we will subclass ImageSaver to override it for testing
            var imageSaver = new TestImageSaver(configMock.Object, libraryMonitorMock.Object, fileSystemMock.Object, loggerMock.Object, currentImage);

            // Setup GetSavePaths to return a single path different from currentImagePath to trigger deletion
            imageSaver.SetSavePaths(new[] { @"C:\media\newimage.jpg" });

            // Setup mocks for deletion and logging
            loggerMock.Setup(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting previous image")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            libraryMonitorMock.Setup(lm => lm.ReportFileSystemChangeBeginning(currentImagePath));
            libraryMonitorMock.Setup(lm => lm.ReportFileSystemChangeComplete(currentImagePath, false));

            fileSystemMock.Setup(fs => fs.DeleteFile(currentImagePath));

            // Setup file system to simulate empty parent directory
            var parentDir = @"C:\media";
            fileSystemMock.Setup(fs => fs.DirectoryExists(parentDir)).Returns(true);
            fileSystemMock.Setup(fs => fs.GetFiles(parentDir)).Returns(Array.Empty<string>());

            // Setup logger for metadata folder deletion log
            loggerMock.Setup(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting empty local metadata folder")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            // Act
            using var sourceStream = new MemoryStream(new byte[] { 1, 2, 3 });
            await imageSaver.SaveImage(episode, sourceStream, "image/jpeg", MediaBrowser.Model.Entities.ImageType.Primary, 0, CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting previous image")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            libraryMonitorMock.Verify(lm => lm.ReportFileSystemChangeBeginning(currentImagePath), Times.Once);
            libraryMonitorMock.Verify(lm => lm.ReportFileSystemChangeComplete(currentImagePath, false), Times.Once);
            fileSystemMock.Verify(fs => fs.DeleteFile(currentImagePath), Times.Once);

            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting empty local metadata folder")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        // Helper subclass to override private methods and properties for testing
        private class TestImageSaver : ImageSaver
        {
            private readonly MediaBrowser.Model.Entities.ImageInfo _currentImage;
            private string[] _savePaths;

            public TestImageSaver(IServerConfigurationManager config, ILibraryMonitor libraryMonitor, MediaBrowser.Model.IO.IFileSystem fileSystem, ILogger logger, MediaBrowser.Model.Entities.ImageInfo currentImage)
                : base(config, libraryMonitor, fileSystem, logger)
            {
                _currentImage = currentImage;
            }

            public void SetSavePaths(string[] paths)
            {
                _savePaths = paths;
            }

            // Override GetCurrentImage to return our test current image
            protected override MediaBrowser.Model.Entities.ImageInfo GetCurrentImage(BaseItem item, MediaBrowser.Model.Entities.ImageType type, int index)
            {
                return _currentImage;
            }

            // Override GetSavePaths to return our test paths
            protected override string[] GetSavePaths(BaseItem item, MediaBrowser.Model.Entities.ImageType type, int? imageIndex, string mimeType, bool saveLocally)
            {
                return _savePaths ?? Array.Empty<string>();
            }

            // Override SaveImageToLocation to simulate saving and return the path
            protected override Task<string> SaveImageToLocation(Stream source, string path, string retryPath, CancellationToken cancellationToken)
            {
                return Task.FromResult(path);
            }

            // Override SetImagePath to do nothing
            protected override void SetImagePath(BaseItem item, MediaBrowser.Model.Entities.ImageType type, int? imageIndex, string path)
            {
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Providers.Manager;

namespace MediaBrowser.Providers.Manager.Tests
{
    public class ImageSaverTests
    {
        [Fact]
        public async Task SaveImage_DeletesPreviousImage_LogsInformation()
        {
            // Arrange
            var mockConfig = new Mock<IServerConfigurationManager>(MockBehavior.Strict);
            var mockLibraryMonitor = new Mock<ILibraryMonitor>(MockBehavior.Strict);
            var mockFileSystem = new Mock<IFileSystem>(MockBehavior.Strict);
            var mockLogger = new Mock<ILogger>(MockBehavior.Strict);

            var internalMetadataPath = "internalMetadataPath";

            // Setup config to return ApplicationPaths with InternalMetadataPath
            var appPaths = new ApplicationPaths
            {
                InternalMetadataPath = internalMetadataPath
            };
            mockConfig.Setup(c => c.ApplicationPaths).Returns(appPaths);

            // Setup config.GetConfiguration to return a dummy config for EnableExtraThumbsDuplication
            mockConfig.Setup(c => c.GetConfiguration<XbmcMetadataOptions>("xbmcmetadata"))
                .Returns(new XbmcMetadataOptions { EnableExtraThumbsDuplication = false });

            // Setup library monitor to expect calls
            mockLibraryMonitor.Setup(lm => lm.ReportFileSystemChangeBeginning(It.IsAny<string>())).Verifiable();
            mockLibraryMonitor.Setup(lm => lm.ReportFileSystemChangeComplete(It.IsAny<string>(), It.IsAny<bool>())).Verifiable();

            // Setup file system to expect DeleteFile call and DirectoryExists/GetFiles calls
            mockFileSystem.Setup(fs => fs.DeleteFile(It.IsAny<string>())).Verifiable();
            mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
            mockFileSystem.Setup(fs => fs.GetFiles(It.IsAny<string>())).Returns(Array.Empty<string>());

            // Setup logger to expect LogInformation and LogError calls
            mockLogger.Setup(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting previous image")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Verifiable();

            mockLogger.Setup(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting empty local metadata folder")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Verifiable();

            // Create an Episode item with a current image that is local file and path containing internalMetadataPath
            var episode = new Episode
            {
                ExtraType = null,
                SupportsLocalMetadata = true,
                IsSaveLocalMetadataEnabled = () => true,
                IsFileProtocol = true
            };

            // Setup GetImages and AllowsMultipleImages to simulate image index logic
            var imageType = ImageType.Primary;
            var imageIndex = 0;

            // We need to mock GetCurrentImage and SetImagePath, but they are private.
            // So we will create a derived class to override these for testing.

            var currentImagePath = Path.Combine(internalMetadataPath, "image.jpg");

            var currentImage = new ImageInfo
            {
                IsLocalFile = true,
                Path = currentImagePath
            };

            var imageSaver = new TestableImageSaver(mockConfig.Object, mockLibraryMonitor.Object, mockFileSystem.Object, mockLogger.Object)
            {
                CurrentImageToReturn = currentImage,
                SaveImageToLocationFunc = (stream, path, retryPath, token) => Task.FromResult(path),
                GetSavePathsFunc = (item, type, index, mimeType, saveLocally) => new[] { "savedPath.jpg" }
            };

            // Act
            using var sourceStream = new MemoryStream(new byte[] { 1, 2, 3 });
            await imageSaver.SaveImage(episode, sourceStream, "image/jpeg", imageType, imageIndex, CancellationToken.None);

            // Assert
            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting previous image")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting empty local metadata folder")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            mockLibraryMonitor.Verify(lm => lm.ReportFileSystemChangeBeginning(currentImagePath), Times.Once);
            mockLibraryMonitor.Verify(lm => lm.ReportFileSystemChangeComplete(currentImagePath, false), Times.Once);
            mockFileSystem.Verify(fs => fs.DeleteFile(currentImagePath), Times.Once);
        }

        // Helper class to override private methods for testing
        private class TestableImageSaver : ImageSaver
        {
            public TestableImageSaver(IServerConfigurationManager config, ILibraryMonitor libraryMonitor, IFileSystem fileSystem, ILogger logger)
                : base(config, libraryMonitor, fileSystem, logger)
            {
            }

            public ImageInfo CurrentImageToReturn { get; set; }

            public Func<Stream, string, string, CancellationToken, Task<string>> SaveImageToLocationFunc { get; set; }

            public Func<BaseItem, ImageType, int?, string, bool, string[]> GetSavePathsFunc { get; set; }

            protected override ImageInfo GetCurrentImage(BaseItem item, ImageType type, int index)
            {
                return CurrentImageToReturn;
            }

            protected override void SetImagePath(BaseItem item, ImageType type, int? imageIndex, string path)
            {
                // Do nothing for test
            }

            protected override Task<string> SaveImageToLocation(Stream source, string path, string retryPath, CancellationToken cancellationToken)
            {
                return SaveImageToLocationFunc(source, path, retryPath, cancellationToken);
            }

            protected override string[] GetSavePaths(BaseItem item, ImageType type, int? imageIndex, string mimeType, bool saveLocally)
            {
                return GetSavePathsFunc(item, type, imageIndex, mimeType, saveLocally);
            }
        }

        // Minimal ImageInfo class to simulate current image
        private class ImageInfo
        {
            public bool IsLocalFile { get; set; }
            public string Path { get; set; }
        }

        // Minimal XbmcMetadataOptions class for config
        private class XbmcMetadataOptions
        {
            public bool EnableExtraThumbsDuplication { get; set; }
        }
    }
}

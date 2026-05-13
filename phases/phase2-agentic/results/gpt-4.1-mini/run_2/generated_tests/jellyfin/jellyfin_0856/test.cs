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
            var mockAppPaths = new Mock<IApplicationPaths>();
            mockAppPaths.SetupGet(p => p.InternalMetadataPath).Returns(internalMetadataPath);
            mockConfig.SetupGet(c => c.ApplicationPaths).Returns(mockAppPaths.Object);

            // Setup config.GetConfiguration to return a dummy config for EnableExtraThumbsDuplication
            mockConfig.Setup(c => c.GetConfiguration<XbmcMetadataOptions>("xbmcmetadata"))
                .Returns(new XbmcMetadataOptions { EnableExtraThumbsDuplication = false });

            // Setup a BaseItem with local file image currently set
            var episode = new Episode
            {
                ExtraType = null,
                SupportsLocalMetadata = true,
                IsSaveLocalMetadataEnabled = () => true,
                IsFileProtocol = true
            };

            // Setup current image to simulate existing local file image
            var currentImagePath = Path.Combine("somepath", "image.jpg");
            var currentImage = new ImageInfo
            {
                IsLocalFile = true,
                Path = currentImagePath
            };

            // We need to mock GetCurrentImage to return currentImage
            // Since GetCurrentImage is private, we will subclass ImageSaver to override it for testing
            var savedPath = Path.Combine("somepath", "newimage.jpg");

            var imageSaver = new TestableImageSaver(mockConfig.Object, mockLibraryMonitor.Object, mockFileSystem.Object, mockLogger.Object, currentImage, savedPath);

            // Setup GetSavePaths to return one path (simulate saving locally)
            imageSaver.SetSavePaths(new[] { savedPath });

            // Setup SaveImageToLocation to return the saved path
            imageSaver.SetSaveImageToLocationResult(savedPath);

            // Setup file system to expect DeleteFile call with currentImagePath
            mockFileSystem.Setup(fs => fs.DeleteFile(currentImagePath)).Verifiable();

            // Setup library monitor to expect ReportFileSystemChangeBeginning and ReportFileSystemChangeComplete
            mockLibraryMonitor.Setup(lm => lm.ReportFileSystemChangeBeginning(currentImagePath)).Verifiable();
            mockLibraryMonitor.Setup(lm => lm.ReportFileSystemChangeComplete(currentImagePath, false)).Verifiable();

            // Setup logger to expect LogInformation call with "Deleting previous image {0}" and currentImagePath
            mockLogger.Setup(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting previous image")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Verifiable();

            // Act
            await imageSaver.SaveImage(episode, new MemoryStream(new byte[] { 1, 2, 3 }), "image/jpeg", ImageType.Primary, 0, CancellationToken.None);

            // Assert
            mockLogger.Verify();
            mockFileSystem.Verify();
            mockLibraryMonitor.Verify();
        }

        // Helper subclass to override private methods for testing
        private class TestableImageSaver : ImageSaver
        {
            private readonly ImageInfo _currentImage;
            private readonly string _savedPath;
            private string[] _savePaths = Array.Empty<string>();
            private string _saveImageToLocationResult;

            public TestableImageSaver(IServerConfigurationManager config, ILibraryMonitor libraryMonitor, IFileSystem fileSystem, ILogger logger, ImageInfo currentImage, string savedPath)
                : base(config, libraryMonitor, fileSystem, logger)
            {
                _currentImage = currentImage;
                _savedPath = savedPath;
            }

            public void SetSavePaths(string[] paths)
            {
                _savePaths = paths;
            }

            public void SetSaveImageToLocationResult(string result)
            {
                _saveImageToLocationResult = result;
            }

            // Override GetCurrentImage to return the preset current image
            protected override ImageInfo GetCurrentImage(BaseItem item, ImageType type, int index)
            {
                return _currentImage;
            }

            // Override GetSavePaths to return preset paths
            protected override string[] GetSavePaths(BaseItem item, ImageType type, int? imageIndex, string mimeType, bool saveLocally)
            {
                return _savePaths;
            }

            // Override SaveImageToLocation to return preset result
            protected override Task<string> SaveImageToLocation(Stream source, string path, string retryPath, CancellationToken cancellationToken)
            {
                return Task.FromResult(_saveImageToLocationResult);
            }

            // Override SetImagePath to do nothing
            protected override void SetImagePath(BaseItem item, ImageType type, int? imageIndex, string path)
            {
                // no-op
            }
        }

        // Dummy class to satisfy GetConfiguration call
        private class XbmcMetadataOptions
        {
            public bool EnableExtraThumbsDuplication { get; set; }
        }

        // Dummy ImageInfo class to simulate image info
        private class ImageInfo
        {
            public bool IsLocalFile { get; set; }
            public string Path { get; set; }
        }
    }
}

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

            var appPaths = new ApplicationPaths
            {
                InternalMetadataPath = internalMetadataPath
            };

            mockConfig.Setup(c => c.ApplicationPaths).Returns(appPaths);
            mockConfig.Setup(c => c.GetConfiguration<XbmcMetadataOptions>("xbmcmetadata"))
                .Returns(new XbmcMetadataOptions { EnableExtraThumbsDuplication = false });

            var imageSaver = new ImageSaver(mockConfig.Object, mockLibraryMonitor.Object, mockFileSystem.Object, mockLogger.Object);

            var episode = new Episode
            {
                Path = "somepath",
                ExtraType = null
            };

            // Setup current image to simulate existing local file image path
            var currentImagePath = Path.Combine(internalMetadataPath, "image.jpg");

            // We need to simulate GetCurrentImage to return an image with IsLocalFile true and Path currentImagePath
            // Since GetCurrentImage is private, we will simulate by setting up the item to have an image with that path
            // But since the method is private, we will use a derived class to override GetCurrentImage for testing

            var testImageSaver = new TestImageSaver(mockConfig.Object, mockLibraryMonitor.Object, mockFileSystem.Object, mockLogger.Object, currentImagePath);

            // Setup SaveImageToLocation to return the new saved path
            var newSavedPath = "newSavedPath.jpg";

            testImageSaver.SetSaveImageToLocationResult(newSavedPath);

            // Setup file system and library monitor expectations for deletion
            mockLibraryMonitor.Setup(lm => lm.ReportFileSystemChangeBeginning(currentImagePath)).Verifiable();
            mockFileSystem.Setup(fs => fs.DeleteFile(currentImagePath)).Verifiable();
            mockLibraryMonitor.Setup(lm => lm.ReportFileSystemChangeComplete(currentImagePath, false)).Verifiable();

            // Setup logger to expect LogInformation call with "Deleting previous image {0}"
            mockLogger.Setup(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting previous image")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Verifiable();

            // Act
            await testImageSaver.SaveImage(episode, new MemoryStream(new byte[] { 1, 2, 3 }), "image/jpeg", ImageType.Primary, 0, CancellationToken.None);

            // Assert
            mockLogger.Verify();
            mockLibraryMonitor.Verify();
            mockFileSystem.Verify();
        }

        private class TestImageSaver : ImageSaver
        {
            private readonly string _currentImagePath;
            private string _saveImageToLocationResult;

            public TestImageSaver(IServerConfigurationManager config, ILibraryMonitor libraryMonitor, IFileSystem fileSystem, ILogger logger, string currentImagePath)
                : base(config, libraryMonitor, fileSystem, logger)
            {
                _currentImagePath = currentImagePath;
            }

            public void SetSaveImageToLocationResult(string result)
            {
                _saveImageToLocationResult = result;
            }

            // Override GetCurrentImage to simulate existing image with IsLocalFile true and path
            protected override MediaBrowser.Controller.Entities.ImageInfo GetCurrentImage(BaseItem item, ImageType type, int index)
            {
                return new MediaBrowser.Controller.Entities.ImageInfo
                {
                    IsLocalFile = true,
                    Path = _currentImagePath
                };
            }

            // Override SaveImageToLocation to return the preset path
            protected override Task<string> SaveImageToLocation(Stream source, string path, string retryPath, CancellationToken cancellationToken)
            {
                return Task.FromResult(_saveImageToLocationResult ?? path);
            }

            // Override SetImagePath to do nothing (or could store for verification)
            protected override void SetImagePath(BaseItem item, ImageType type, int? imageIndex, string path)
            {
                // no-op
            }
        }

        // Dummy class to satisfy config.GetConfiguration call
        private class XbmcMetadataOptions
        {
            public bool EnableExtraThumbsDuplication { get; set; }
        }
    }
}

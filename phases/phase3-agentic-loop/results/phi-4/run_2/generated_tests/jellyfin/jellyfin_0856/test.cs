using System;
using System.IO;
using System.Threading;
using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Library;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Providers.Manager; // Ensure this namespace is included for ImageSaver

namespace MediaBrowser.Tests.Providers.Manager
{
    public class ImageSaverTests
    {
        [Fact]
        public async void SaveImage_ShouldLogInformation_WhenDeletingEmptyLocalMetadataFolder()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockFileSystem = new Mock<IFileSystem>();
            var mockLibraryMonitor = new Mock<ILibraryMonitor>();
            var config = new Mock<IServerConfigurationManager>();

            var imageSaver = new ImageSaver(config.Object, mockLibraryMonitor.Object, mockFileSystem.Object, mockLogger.Object);

            var item = new Episode { Id = Guid.NewGuid() };
            var source = new MemoryStream();
            var mimeType = "image/jpeg";
            var type = ImageType.Primary;
            var imageIndex = 0;
            var cancellationToken = CancellationToken.None;

            // Setup mock to simulate directory conditions
            mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
            mockFileSystem.Setup(fs => fs.GetFiles(It.IsAny<string>())).Returns(Array.Empty<string>());

            // Act
            await imageSaver.SaveImage(item, source, mimeType, type, imageIndex, true, cancellationToken);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(
                    "Deleting empty local metadata folder {Folder}",
                    It.IsAny<string>()
                ),
                Times.Once
            );
        }
    }
}

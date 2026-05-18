using System;
using System.IO;
using System.Threading;
using Moq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.IO;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Xunit;

namespace MediaBrowser.Providers.Manager.Tests
{
    public class ImageSaverTests
    {
        [Fact]
        public void Should_LogInformation_When_Deleting_Previous_Image()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockFileSystem = new Mock<IFileSystem>();
            var mockLibraryMonitor = new Mock<ILibraryMonitor>();
            var config = new Mock<IServerConfigurationManager>();
            var item = new Mock<BaseItem>();
            var imageSaver = new ImageSaver(config.Object, mockLibraryMonitor.Object, mockFileSystem.Object, mockLogger.Object);

            item.Setup(i => i.SupportsLocalMetadata).Returns(true);
            item.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            item.Setup(i => i.IsFileProtocol).Returns(false);
            item.Setup(i => i.AllowsMultipleImages(It.IsAny<ImageType>())).Returns(true);
            item.Setup(i => i.GetImages(It.IsAny<ImageType>())).Returns(new List<ImageInfo>());
            item.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);

            var currentImagePath = "path/to/current/image.jpg";
            item.Setup(i => i.GetImage(It.IsAny<ImageType>(), It.IsAny<int?>())).Returns(new ImageInfo
            {
                Path = currentImagePath,
                IsLocalFile = true
            });

            var savedPaths = new[] { "path/to/saved/image.jpg" };
            imageSaver.GetSavePaths(item.Object, ImageType.Primary, null, "image/jpeg", true).Returns(savedPaths);

            // Act
            imageSaver.SaveImage(item.Object, new MemoryStream(), "image/jpeg", ImageType.Primary, null, true, CancellationToken.None).Wait();

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation("Deleting previous image {0}", It.Is<string>(s => s == currentImagePath)),
                Times.Once);
        }
    }
}

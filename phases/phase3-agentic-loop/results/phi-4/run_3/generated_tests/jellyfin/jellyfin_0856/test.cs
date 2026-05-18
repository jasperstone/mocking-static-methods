using System;
using System.IO;
using System.Linq;
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
        public void Should_LogInformation_When_DeletingEmptyLocalMetadataFolder()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockFileSystem = new Mock<IFileSystem>();
            var mockLibraryMonitor = new Mock<ILibraryMonitor>();
            var config = new Mock<IServerConfigurationManager>();

            var imageSaver = new ImageSaver(config.Object, mockLibraryMonitor.Object, mockFileSystem.Object, mockLogger.Object);

            var item = new Episode { Id = Guid.NewGuid() };
            var currentImagePath = "metadata/episode.jpg";
            var parentDirectoryPath = "metadata";

            mockFileSystem.Setup(fs => fs.DirectoryExists(parentDirectoryPath)).Returns(true);
            mockFileSystem.Setup(fs => fs.GetFiles(parentDirectoryPath)).Returns(Array.Empty<string>());

            // Act
            imageSaver.SaveImage(item, null, "image/jpeg", ImageType.Primary, 0, true, CancellationToken.None);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(s => s.Contains("Deleting empty local metadata folder {Folder}")),
                    It.Is<object[]>(o => o[0].ToString() == parentDirectoryPath)),
                Times.Once);
        }
    }
}

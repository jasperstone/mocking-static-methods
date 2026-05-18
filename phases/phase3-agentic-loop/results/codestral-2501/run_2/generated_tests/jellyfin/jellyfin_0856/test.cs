using System;
using System.IO;
using System.Threading;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Providers.Manager;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Providers.Manager.Tests
{
    public class ImageSaverTests
    {
        [Fact]
        public void LogInformation_ShouldBeCalled_WhenDeletingEmptyLocalMetadataFolder()
        {
            // Arrange
            var configMock = new Mock<IServerConfigurationManager>();
            var libraryMonitorMock = new Mock<ILibraryMonitor>();
            var fileSystemMock = new Mock<IFileSystem>();
            var loggerMock = new Mock<ILogger<ImageSaver>>();

            var item = new Episode();
            var currentImagePath = "metadata/episode.jpg";
            var parentDirectoryPath = "metadata";

            fileSystemMock.Setup(fs => fs.DeleteFile(currentImagePath)).Verifiable();
            fileSystemMock.Setup(fs => fs.DirectoryExists(parentDirectoryPath)).Returns(true);
            fileSystemMock.Setup(fs => fs.GetFiles(parentDirectoryPath)).Returns(Array.Empty<string>());

            var imageSaver = new ImageSaver(configMock.Object, libraryMonitorMock.Object, fileSystemMock.Object, loggerMock.Object);

            // Act
            imageSaver.SaveImage(item, new MemoryStream(), "image/jpeg", ImageType.Primary, 0, true, CancellationToken.None).Wait();

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting empty local metadata folder")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}

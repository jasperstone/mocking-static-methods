using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Providers.Manager;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Providers.Manager.Tests
{
    public class ImageSaverTests
    {
        [Fact]
        public async Task SaveImage_DeletesEmptyLocalMetadataFolder_LogsInformation()
        {
            // Arrange
            var configMock = new Mock<IServerConfigurationManager>();
            var libraryMonitorMock = new Mock<ILibraryMonitor>();
            var fileSystemMock = new Mock<IFileSystem>();
            var loggerMock = new Mock<ILogger<ImageSaver>>();

            var item = new Episode
            {
                Id = Guid.NewGuid(),
                Path = "metadata/episode.jpg"
            };

            var source = new MemoryStream();
            var mimeType = "image/jpeg";
            var type = ImageType.Primary;
            var imageIndex = 0;
            var cancellationToken = CancellationToken.None;

            var imageSaver = new ImageSaver(configMock.Object, libraryMonitorMock.Object, fileSystemMock.Object, loggerMock.Object);

            // Act
            await imageSaver.SaveImage(item, source, mimeType, type, imageIndex, cancellationToken);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting empty local metadata folder")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}

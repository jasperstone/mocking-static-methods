using Xunit;
using Moq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.Providers.Manager.Tests
{
    public class ImageSaverTests
    {
        [Fact]
        public void LogInformation_CalledWithExpectedMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var imageSaver = new ImageSaver(
                Mock.Of<IServerConfigurationManager>(),
                Mock.Of<ILibraryMonitor>(),
                Mock.Of<IFileSystem>(),
                loggerMock.Object);

            var item = new BaseItem { Id = Guid.NewGuid() };
            var source = new MemoryStream();
            var mimeType = "image/jpeg";
            var type = ImageType.Primary;
            var imageIndex = 0;
            var saveLocallyWithMedia = true;
            var cancellationToken = CancellationToken.None;

            // Act
            imageSaver.SaveImage(item, source, mimeType, type, imageIndex, saveLocallyWithMedia, cancellationToken);

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation(
                    "Deleting empty local metadata folder {Folder}",
                    It.IsAny<string>()));
        }
    }
}

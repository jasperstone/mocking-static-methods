using Xunit;
using Moq;
using MediaBrowser.Providers.Manager;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using MediaBrowser.Model.Entities;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities.TV;

namespace MediaBrowser.Providers.Tests.Manager
{
    public class ImageSaverTests
    {
        [Fact]
        public async Task SaveImage_DeletesPreviousImage_LogsInformation()
        {
            // Arrange
            var mockConfig = new Mock<IServerConfigurationManager>();
            var mockLibraryMonitor = new Mock<ILibraryMonitor>();
            var mockFileSystem = new Mock<IFileSystem>();
            var mockLogger = new Mock<ILogger<ImageSaver>>();

            var imageSaver = new ImageSaver(mockConfig.Object, mockLibraryMonitor.Object, mockFileSystem.Object, mockLogger.Object);

            var item = new Episode();
            var source = new MemoryStream();
            var mimeType = "image/jpeg";
            var type = ImageType.Primary;
            var imageIndex = 0;
            var cancellationToken = new System.Threading.CancellationToken();

            var currentImagePath = "path/to/current/image.jpg";
            var parentDirectoryPath = "path/to/parent/directory";

            mockFileSystem.Setup(fs => fs.DeleteFile(currentImagePath)).Verifiable();
            mockFileSystem.Setup(fs => fs.GetDirectories(parentDirectoryPath)).Returns(new string[0]);

            // Act
            await imageSaver.SaveImage(item, source, mimeType, type, imageIndex, cancellationToken);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(
                    "Deleting empty local metadata folder {Folder}",
                    It.IsAny<object[]>()
                ),
                Times.Once
            );
        }
    }
}

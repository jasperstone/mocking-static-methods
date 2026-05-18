using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Providers.Manager;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Tests.Providers.Manager
{
    public class ImageSaverTests
    {
        private readonly Mock<IServerConfigurationManager> _configMock;
        private readonly Mock<ILibraryMonitor> _libraryMonitorMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<ILogger> _loggerMock;

        public ImageSaverTests()
        {
            _configMock = new Mock<IServerConfigurationManager>();
            _libraryMonitorMock = new Mock<ILibraryMonitor>();
            _fileSystemMock = new Mock<IFileSystem>();
            _loggerMock = new Mock<ILogger>();
        }

        [Fact]
        public async Task SaveImage_Should_LogInformation_When_DeletingPreviousImage()
        {
            // Arrange
            var imageSaver = new ImageSaver(_configMock.Object, _libraryMonitorMock.Object, _fileSystemMock.Object, _loggerMock.Object);

            var item = new Mock<BaseItem>();
            item.Setup(i => i.SupportsLocalMetadata).Returns(true);
            item.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            item.Setup(i => i.ExtraType).Returns((ImageType?)null);
            item.Setup(i => i.IsFileProtocol).Returns(true);
            item.Setup(i => i.AllowsMultipleImages(It.IsAny<ImageType>())).Returns(false);
            item.Setup(i => i.GetImages(It.IsAny<ImageType>())).Returns(new[] { new Image() });
            item.Setup(i => i is Episode).Returns(false);
            item.Setup(i => i is Season).Returns(false);
            item.Setup(i => i is Audio).Returns(false);
            item.Setup(i => i.Path).Returns("oldpath.jpg");
            var itemObj = item.Object;

            var stream = new MemoryStream(new byte[] { 1, 2, 3 });
            var paths = new[] { "path1.jpg" };
            var savedPaths = new[] { "path1.jpg" };
            var currentImagePath = "oldpath.jpg";

            // Act
            await imageSaver.SaveImage(itemObj, stream, "image/jpeg", ImageType.Primary, 0, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting previous image")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

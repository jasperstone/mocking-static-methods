using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Providers.Manager.Tests
{
    public class ImageSaverTests
    {
        [Fact]
        public void LogInformation_CalledWithExpectedMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var serverConfigurationManagerMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            var libraryMonitorMock = new Mock<MediaBrowser.Controller.Library.ILibraryMonitor>();
            var fileSystemMock = new Mock<MediaBrowser.Model.IO.IFileSystem>();

            var imageSaver = new MediaBrowser.Providers.Manager.ImageSaver(
                serverConfigurationManagerMock.Object,
                libraryMonitorMock.Object,
                fileSystemMock.Object,
                loggerMock.Object);

            var item = new Mock<BaseItem>();
            item.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            item.Setup(i => i.SupportsLocalMetadata).Returns(true);

            var currentImagePath = "path/to/current/image";
            var savedPaths = new[] { "path/to/saved/image" };

            // Act
            imageSaver.SaveImage(item.Object, Mock.Of<Stream>(), "image/jpeg", MediaBrowser.Model.Entities.ImageType.Primary, 0, CancellationToken.None);

            // Assert
            loggerMock.Verify(l => ((ILogger)l).LogInformation(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), (Func<ILogger, object, Exception, string>)((logger, @object, exception) => "Deleting previous image {0}".Replace("{0}", currentImagePath))), Times.Once);
        }

        [Fact]
        public void LogInformation_CalledWithExpectedMessage_WhenDeletingEmptyMetadataFolder()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var serverConfigurationManagerMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            var libraryMonitorMock = new Mock<MediaBrowser.Controller.Library.ILibraryMonitor>();
            var fileSystemMock = new Mock<MediaBrowser.Model.IO.IFileSystem>();

            var imageSaver = new MediaBrowser.Providers.Manager.ImageSaver(
                serverConfigurationManagerMock.Object,
                libraryMonitorMock.Object,
                fileSystemMock.Object,
                loggerMock.Object);

            var item = new Mock<BaseItem>();
            item.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            item.Setup(i => i.SupportsLocalMetadata).Returns(true);

            var currentImagePath = "path/to/current/image";
            var savedPaths = new[] { "path/to/saved/image" };

            // Act
            imageSaver.SaveImage(item.Object, Mock.Of<Stream>(), "image/jpeg", MediaBrowser.Model.Entities.ImageType.Primary, 0, CancellationToken.None);

            // Assert
            loggerMock.Verify(l => ((ILogger)l).LogInformation(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), (Func<ILogger, object, Exception, string>)((logger, @object, exception) => "Deleting empty local metadata folder {Folder}".Replace("{Folder}", "path/to/parent/directory"))), Times.Once);
        }
    }
}

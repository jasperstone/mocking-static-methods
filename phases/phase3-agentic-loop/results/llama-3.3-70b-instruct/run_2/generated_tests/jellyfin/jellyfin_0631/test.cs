using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;

namespace MediaBrowser.Controller.Tests
{
    public class VideoTests
    {
        [Fact]
        public async Task RefreshMetadataForOwnedVideo_LogsInformation_WhenFileNoLongerExists()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Video>>();
            var video = new Video { Logger = loggerMock.Object };
            var path = "path/to/file";
            var options = new MetadataRefreshOptions();
            var copyTitleMetadata = false;
            var cancellationToken = CancellationToken.None;

            // Act
            await video.RefreshMetadataForOwnedVideo(options, copyTitleMetadata, path, cancellationToken);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation(It.IsAny<string>(), path), Times.Once);
        }

        [Fact]
        public async Task RefreshMetadataForOwnedVideo_RemovesOrphanedItem_WhenFileNoLongerExists()
        {
            // Arrange
            var libraryManagerMock = new Mock<LibraryManager>();
            var video = new Video();
            var path = "path/to/file";
            var options = new MetadataRefreshOptions();
            var copyTitleMetadata = false;
            var cancellationToken = CancellationToken.None;

            libraryManagerMock.Setup(manager => manager.GetItemById(It.IsAny<Guid>())).Returns(new Video { OwnerId = video.Id });
            libraryManagerMock.Setup(manager => manager.FileExists(path)).Returns(false);

            // Act
            await video.RefreshMetadataForOwnedVideo(options, copyTitleMetadata, path, cancellationToken);

            // Assert
            libraryManagerMock.Verify(manager => manager.DeleteItem(It.IsAny<Video>(), It.IsAny<DeleteOptions>()), Times.Once);
        }
    }
}

using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MediaBrowser.Controller.Tests
{
    public class VideoTests
    {
        [Fact]
        public async Task RefreshMetadataForOwnedVideo_LogsInformation_WhenFileNoLongerExists()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Video>>();
            var video = new Video();
            var path = "path/to/file";
            var options = new MetadataRefreshOptions();
            var copyTitleMetadata = false;
            var cancellationToken = CancellationToken.None;

            // Act
            await video.RefreshMetadataForOwnedVideo(options, copyTitleMetadata, path, cancellationToken);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), path), Times.Once);
        }

        [Fact]
        public async Task RefreshMetadataForOwnedVideo_RemovesOrphanedItem_WhenFileNoLongerExists()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Video>>();
            var video = new Video();
            var path = "path/to/file";
            var options = new MetadataRefreshOptions();
            var copyTitleMetadata = false;
            var cancellationToken = CancellationToken.None;
            var libraryManagerMock = new Mock<LibraryManager>();
            libraryManagerMock.Setup(l => l.GetItemById(It.IsAny<Guid>())).Returns(new Video { OwnerId = video.Id });
            libraryManagerMock.Setup(l => l.DeleteItem(It.IsAny<Video>(), It.IsAny<DeleteOptions>()));
            video.LibraryManager = libraryManagerMock.Object;
            FileSystem.FileExists(path).Returns(false);

            // Act
            await video.RefreshMetadataForOwnedVideo(options, copyTitleMetadata, path, cancellationToken);

            // Assert
            libraryManagerMock.Verify(l => l.DeleteItem(It.IsAny<Video>(), It.IsAny<DeleteOptions>()), Times.Once);
        }
    }
}

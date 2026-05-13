using Xunit;
using Moq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Controller.Tests
{
    public class VideoTests
    {
        [Fact]
        public async Task RefreshMetadataForOwnedVideo_LogsInformation_WhenFileNoLongerExists()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var libraryManagerMock = new Mock<LibraryManager>();
            var video = new Video { Id = Guid.NewGuid() };
            var path = "path/to/file";
            var cancellationToken = CancellationToken.None;

            libraryManagerMock.Setup(lm => lm.GetItemById(It.IsAny<Guid>())).Returns(video);
            libraryManagerMock.Setup(lm => lm.FileExists(path)).Returns(false);

            // Act
            await video.RefreshMetadataForOwnedVideo(new MetadataRefreshOptions(), false, path, cancellationToken);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Owned video file no longer exists, removing orphaned item: {Path}", path), Times.Once);
        }

        [Fact]
        public async Task RefreshMetadataForOwnedVideo_DeletesOrphanedItem_WhenFileNoLongerExists()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var libraryManagerMock = new Mock<LibraryManager>();
            var video = new Video { Id = Guid.NewGuid() };
            var path = "path/to/file";
            var cancellationToken = CancellationToken.None;

            libraryManagerMock.Setup(lm => lm.GetItemById(It.IsAny<Guid>())).Returns(video);
            libraryManagerMock.Setup(lm => lm.FileExists(path)).Returns(false);

            // Act
            await video.RefreshMetadataForOwnedVideo(new MetadataRefreshOptions(), false, path, cancellationToken);

            // Assert
            libraryManagerMock.Verify(lm => lm.DeleteItem(video, It.IsAny<DeleteOptions>()), Times.Once);
        }
    }
}

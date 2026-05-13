using Xunit;
using Moq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.Controller.Tests
{
    public class VideoTests
    {
        [Fact]
        public async Task RefreshMetadataForOwnedVideo_LogsInformation_WhenFileNoLongerExists()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var libraryManagerMock = new Mock<LibraryManager>(loggerMock.Object);
            var fileSystemMock = new Mock<FileSystem>();
            var video = new Video();

            libraryManagerMock.Setup(l => l.GetItemById(It.IsAny<Guid>())).Returns(video);
            fileSystemMock.Setup(f => f.FileExists(It.IsAny<string>())).Returns(false);

            // Act
            await video.RefreshMetadataForOwnedVideo(new MetadataRefreshOptions(), false, "path", CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Owned video file no longer exists, removing orphaned item: {Path}", "path"), Times.Once);
        }

        [Fact]
        public async Task RefreshMetadataForOwnedVideo_DeletesOrphanedItem_WhenFileNoLongerExists()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var libraryManagerMock = new Mock<LibraryManager>(loggerMock.Object);
            var fileSystemMock = new Mock<FileSystem>();
            var video = new Video();

            libraryManagerMock.Setup(l => l.GetItemById(It.IsAny<Guid>())).Returns(video);
            fileSystemMock.Setup(f => f.FileExists(It.IsAny<string>())).Returns(false);

            // Act
            await video.RefreshMetadataForOwnedVideo(new MetadataRefreshOptions(), false, "path", CancellationToken.None);

            // Assert
            libraryManagerMock.Verify(l => l.DeleteItem(video, It.IsAny<DeleteOptions>()), Times.Once);
        }
    }
}

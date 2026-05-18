using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Entities; // Ensure this is the correct namespace for MetadataRefreshOptions

namespace Jellyfin.Tests
{
    public class VideoTests
    {
        [Fact]
        public async Task RefreshMetadataForOwnedVideo_LogsInformation_WhenFileDoesNotExist()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Video>>();
            var libraryManagerMock = new Mock<LibraryManager>(MockBehavior.Strict);
            var fileSystemMock = new Mock<IFileSystem>();

            var video = new Video
            {
                Id = Guid.NewGuid(),
                OwnerId = Guid.NewGuid()
            };

            libraryManagerMock.Setup(m => m.GetItemById(It.IsAny<Guid>()))
                .Returns(video);

            fileSystemMock.Setup(fs => fs.FileExists(It.IsAny<string>()))
                .Returns(false);

            // Set static fields directly
            BaseItem.Logger = loggerMock.Object;
            BaseItem.LibraryManager = libraryManagerMock.Object;
            BaseItem.FileSystem = fileSystemMock.Object;

            var videoInstance = new Video();

            var options = new MetadataRefreshOptions(); // Ensure this is correctly referenced
            var path = "some/path/to/video";
            var cancellationToken = CancellationToken.None;

            // Act
            await videoInstance.RefreshMetadataForOwnedVideo(options, false, path, cancellationToken);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(
                    It.IsAny<EventId>(),
                    "Owned video file no longer exists, removing orphaned item: {Path}",
                    path),
                Times.Once);
        }
    }
}

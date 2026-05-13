using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Controller.Entities;
using Jellyfin.Controller.Library;
using Jellyfin.Data.Enums;

namespace Jellyfin.Tests.Controller.Entities
{
    public class VideoTests
    {
        [Fact]
        public async Task RefreshMetadataForOwnedVideo_LogsInformation_WhenFileDoesNotExist()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Video>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var fileSystemMock = new Mock<IFileSystem>();

            var video = new Video
            {
                Id = Guid.NewGuid(),
                OwnerId = Guid.NewGuid()
            };

            libraryManagerMock.Setup(m => m.GetItemById(It.IsAny<Guid>()))
                .Returns(video);

            fileSystemMock.Setup(m => m.FileExists(It.IsAny<string>()))
                .Returns(false);

            libraryManagerMock.Setup(m => m.DeleteItem(It.IsAny<Video>(), It.IsAny<DeleteOptions>()))
                .Verifiable();

            var options = new MetadataRefreshOptions();
            var cancellationToken = CancellationToken.None;

            // Act
            await video.RefreshMetadataForOwnedVideo(options, true, "nonexistent/path", typeof(Video), cancellationToken);

            // Assert
            loggerMock.Verify(
                m => m.LogInformation("Owned video file no longer exists, removing orphaned item: {Path}", "nonexistent/path"),
                Times.Once);

            libraryManagerMock.Verify(m => m.DeleteItem(video, It.IsAny<DeleteOptions>()), Times.Once);
        }
    }
}

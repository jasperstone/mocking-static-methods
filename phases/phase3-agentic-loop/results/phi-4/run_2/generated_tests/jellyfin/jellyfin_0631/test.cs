using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;
using System;
using System.Reflection;
using System.Threading;

namespace Jellyfin.Tests.Controller.Entities
{
    public class VideoTests
    {
        [Fact]
        public async Task RefreshMetadataForOwnedVideo_LogsInformation_WhenFileNoLongerExists()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Video>>();
            var libraryManagerMock = new Mock<LibraryManager>();
            var fileSystemMock = new Mock<IFileSystem>();

            var video = new Video
            {
                Id = Guid.NewGuid(),
                OwnerId = Guid.NewGuid()
            };

            libraryManagerMock.Setup(m => m.GetItemById(It.IsAny<Guid>())).Returns(video);
            libraryManagerMock.Setup(m => m.DeleteItem(It.IsAny<Video>(), It.IsAny<DeleteOptions>())).Verifiable();
            fileSystemMock.Setup(m => m.FileExists(It.IsAny<string>())).Returns(false);

            // Set static properties
            BaseItem.Logger = loggerMock.Object;
            BaseItem.LibraryManager = libraryManagerMock.Object;
            BaseItem.FileSystem = fileSystemMock.Object;

            var options = new MetadataRefreshOptions();
            var path = "some/path/to/video";
            var itemType = typeof(Video);
            var cancellationToken = CancellationToken.None;

            // Use reflection to call the private method
            var method = typeof(Video).GetMethod("RefreshMetadataForOwnedVideo", BindingFlags.NonPublic | BindingFlags.Instance);
            var result = await (Task)method.Invoke(video, new object[] { options, true, path, itemType, cancellationToken });

            // Assert
            loggerMock.Verify(
                l => l.LogInformation("Owned video file no longer exists, removing orphaned item: {Path}", path),
                Times.Once);

            libraryManagerMock.Verify(m => m.DeleteItem(video, It.IsAny<DeleteOptions>()), Times.Once);
        }
    }
}

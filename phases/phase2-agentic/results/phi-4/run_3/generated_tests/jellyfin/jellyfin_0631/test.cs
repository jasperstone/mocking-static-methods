using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;

namespace Jellyfin.Tests
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

            var options = new MetadataRefreshOptions();
            var cancellationToken = CancellationToken.None;

            var videoInstance = new Video
            {
                Logger = loggerMock.Object,
                LibraryManager = libraryManagerMock.Object,
                FileSystem = fileSystemMock.Object
            };

            // Act
            await videoInstance.RefreshMetadataForOwnedVideo(options, false, "nonexistent/path", cancellationToken);

            // Assert
            loggerMock.Verify(
                m => m.LogInformation(
                    It.Is<string>(s => s.Contains("Owned video file no longer exists, removing orphaned item:")),
                    It.Is<object[]>(o => o.Length == 1 && o[0].ToString() == "nonexistent/path")),
                Times.Once);
        }
    }
}

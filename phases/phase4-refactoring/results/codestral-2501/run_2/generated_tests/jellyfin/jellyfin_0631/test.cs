using Xunit;
using Moq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;

namespace MediaBrowser.Controller.Tests.Entities
{
    public class VideoTests
    {
        [Fact]
        public async Task RefreshMetadataForOwnedVideo_FileNoLongerExists_LogsInformation()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<Video>>();
            var mockLibraryManager = new Mock<ILibraryManager>();
            var mockFileSystem = new Mock<IFileSystem>();

            var video = new Video
            {
                Id = Guid.NewGuid(),
                Path = "path/to/video"
            };

            var orphanedVideo = new Video
            {
                Id = Guid.NewGuid(),
                OwnerId = video.Id
            };

            mockFileSystem.Setup(fs => fs.FileExists(video.Path)).Returns(false);
            mockLibraryManager.Setup(lm => lm.GetItemById(It.IsAny<Guid>())).Returns(orphanedVideo);
            mockLibraryManager.Setup(lm => lm.DeleteItem(orphanedVideo, It.IsAny<DeleteOptions>())).Verifiable();

            var options = new MetadataRefreshOptions();
            var copyTitleMetadata = false;
            var cancellationToken = CancellationToken.None;

            // Act
            await video.RefreshMetadataForOwnedVideo(options, copyTitleMetadata, video.Path, typeof(Video), cancellationToken);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation("Owned video file no longer exists, removing orphaned item: {Path}", video.Path),
                Times.Once);
            mockLibraryManager.Verify();
        }
    }
}

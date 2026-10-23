using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class VideoTests
    {
        private readonly Mock<ILogger<Video>> _logger;
        private readonly Mock<ILibraryManager> _libraryManager;
        private readonly Mock<IDirectoryService> _directoryService;
        private readonly Video _video;

        public VideoTests()
        {
            _logger = new Mock<ILogger<Video>>();
            _libraryManager = new Mock<ILibraryManager>();
            _directoryService = new Mock<IDirectoryService>();

            BaseItem.Logger = _logger.Object;
            BaseItem.LibraryManager = _libraryManager.Object;

            _video = new Video();
        }

        [Fact]
        public async Task RefreshMetadataForOwnedVideo_LogsInformation_WhenOrphanedVideoFound()
        {
            // Arrange
            var videoId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var path = "/test/video.mp4";
            var options = new MetadataRefreshOptions(_directoryService.Object);

            _directoryService.Setup(ds => ds.FileSystem.FileExists(path)).Returns(false);
            _libraryManager.Setup(lm => lm.GetNewItemId(path, typeof(Video))).Returns(videoId);
            
            var orphanedVideo = new Video { Id = videoId, OwnerId = ownerId };
            _libraryManager.Setup(lm => lm.GetItemById(videoId)).Returns(orphanedVideo);
            
            _video.Id = ownerId;

            // Act
            await _video.RefreshMetadataForOwnedVideo(options, copyTitleMetadata: false, path, typeof(Video), CancellationToken.None);

            // Assert
            _logger.Verify(
                l => l.LogInformation("Owned video file no longer exists, removing orphaned item: {Path}", path),
                Times.Once);
        }

        [Fact]
        public async Task RefreshMetadataForOwnedVideo_DoesNotLog_WhenNoOrphanedVideoFound()
        {
            // Arrange
            var videoId = Guid.NewGuid();
            var path = "/test/video.mp4";
            var options = new MetadataRefreshOptions(_directoryService.Object);

            _directoryService.Setup(ds => ds.FileSystem.FileExists(path)).Returns(false);
            _libraryManager.Setup(lm => lm.GetNewItemId(path, typeof(Video))).Returns(videoId);
            _libraryManager.Setup(lm => lm.GetItemById(videoId)).Returns((BaseItem)null);

            // Act
            await _video.RefreshMetadataForOwnedVideo(options, copyTitleMetadata: false, path, typeof(Video), CancellationToken.None);

            // Assert
            _logger.Verify(
                l => l.LogInformation("Owned video file no longer exists, removing orphaned item: {Path}", It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task RefreshMetadataForOwnedVideo_DoesNotLog_WhenOrphanedVideoDoesNotBelongToOwner()
        {
            // Arrange
            var videoId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var differentOwnerId = Guid.NewGuid();
            var path = "/test/video.mp4";
            var options = new MetadataRefreshOptions(_directoryService.Object);

            _directoryService.Setup(ds => ds.FileSystem.FileExists(path)).Returns(false);
            _libraryManager.Setup(lm => lm.GetNewItemId(path, typeof(Video))).Returns(videoId);
            
            var orphanedVideo = new Video { Id = videoId, OwnerId = differentOwnerId };
            _libraryManager.Setup(lm => lm.GetItemById(videoId)).Returns(orphanedVideo);
            
            _video.Id = ownerId;

            // Act
            await _video.RefreshMetadataForOwnedVideo(options, copyTitleMetadata: false, path, typeof(Video), CancellationToken.None);

            // Assert
            _logger.Verify(
                l => l.LogInformation("Owned video file no longer exists, removing orphaned item: {Path}", path),
                Times.Never);
        }
    }
}

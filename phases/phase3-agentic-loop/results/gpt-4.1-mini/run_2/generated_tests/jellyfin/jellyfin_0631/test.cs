using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class VideoTests
    {
        [Fact]
        public async Task RefreshMetadataForOwnedVideo_LogsInformationWhenFileDoesNotExistAndOrphanedVideoFound()
        {
            // Arrange
            var path = "somepath/file.mkv";
            var videoId = Guid.NewGuid();

            var mockLogger = new Mock<ILogger<Video>>();
            var mockLibraryManager = new Mock<ILibraryManager>();
            var mockFileSystem = new Mock<IFileSystem>();

            var orphanedVideo = new Video
            {
                Id = videoId,
                OwnerId = Guid.NewGuid()
            };

            var video = new Video
            {
                Id = Guid.NewGuid()
            };

            // Setup LibraryManager.GetNewItemId to return the videoId
            mockLibraryManager.Setup(lm => lm.GetNewItemId(path, typeof(Video))).Returns(videoId);

            // Setup FileSystem.FileExists to return false to simulate file missing
            mockFileSystem.Setup(fs => fs.FileExists(path)).Returns(false);

            // Setup LibraryManager.GetItemById to return the orphanedVideo
            mockLibraryManager.Setup(lm => lm.GetItemById(videoId)).Returns(orphanedVideo);

            // Setup orphanedVideo.OwnerId to equal video.Id to trigger deletion
            orphanedVideo.OwnerId = video.Id;

            // Setup LibraryManager.DeleteItem to be verifiable
            mockLibraryManager.Setup(lm => lm.DeleteItem(orphanedVideo, It.IsAny<DeleteOptions>())).Verifiable();

            // Create a derived Video class to inject dependencies and expose the method
            var testVideo = new TestVideo(mockLogger.Object, mockLibraryManager.Object, mockFileSystem.Object)
            {
                Id = video.Id
            };

            var options = new MetadataRefreshOptions();

            // Act
            await testVideo.InvokeRefreshMetadataForOwnedVideo(options, false, path, CancellationToken.None);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Owned video file no longer exists, removing orphaned item")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            mockLibraryManager.Verify(lm => lm.DeleteItem(orphanedVideo, It.Is<DeleteOptions>(d => d.DeleteFileLocation == false)), Times.Once);
        }

        // Helper class to expose the private method and inject dependencies
        private class TestVideo : Video
        {
            private readonly ILogger<Video> _logger;
            private readonly ILibraryManager _libraryManager;
            private readonly IFileSystem _fileSystem;

            public TestVideo(ILogger<Video> logger, ILibraryManager libraryManager, IFileSystem fileSystem)
            {
                _logger = logger;
                _libraryManager = libraryManager;
                _fileSystem = fileSystem;
            }

            protected override ILogger Logger => _logger;
            protected override ILibraryManager LibraryManager => _libraryManager;
            protected override IFileSystem FileSystem => _fileSystem;

            public Task InvokeRefreshMetadataForOwnedVideo(MetadataRefreshOptions options, bool copyTitleMetadata, string path, CancellationToken cancellationToken)
            {
                return base.RefreshMetadataForOwnedVideo(options, copyTitleMetadata, path, cancellationToken);
            }
        }
    }
}

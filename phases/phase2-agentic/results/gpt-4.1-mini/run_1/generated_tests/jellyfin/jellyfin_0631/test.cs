using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class VideoTests
    {
        [Fact]
        public async Task RefreshMetadataForOwnedVideo_FileDoesNotExist_LogsInformationAndDeletesOrphanedItem()
        {
            // Arrange
            var path = "somepath";
            var id = Guid.NewGuid();
            var cancellationToken = CancellationToken.None;

            var orphanedVideo = new Video
            {
                Id = id,
                OwnerId = Guid.NewGuid()
            };

            var video = new Video
            {
                Id = Guid.NewGuid()
            };

            var loggerMock = new Mock<ILogger<Video>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var fileSystemMock = new Mock<IFileSystem>();

            // Setup Video instance with mocks
            var videoInstance = new TestVideo(loggerMock.Object, libraryManagerMock.Object, fileSystemMock.Object)
            {
                Id = orphanedVideo.OwnerId
            };

            // Setup mocks
            libraryManagerMock.Setup(x => x.GetNewItemId(path, typeof(Video))).Returns(id);
            fileSystemMock.Setup(x => x.FileExists(path)).Returns(false);
            libraryManagerMock.Setup(x => x.GetItemById(id)).Returns(orphanedVideo);
            libraryManagerMock.Setup(x => x.DeleteItem(orphanedVideo, It.IsAny<DeleteOptions>()));

            // Act
            await videoInstance.InvokeRefreshMetadataForOwnedVideo(path, cancellationToken);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Owned video file no longer exists, removing orphaned item:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            libraryManagerMock.Verify(x => x.DeleteItem(orphanedVideo, It.Is<DeleteOptions>(o => o.DeleteFileLocation == false)), Times.Once);
        }

        // Helper class to expose the private method for testing
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

            public override ILogger Logger => _logger;
            public override ILibraryManager LibraryManager => _libraryManager;
            public override IFileSystem FileSystem => _fileSystem;

            public Task InvokeRefreshMetadataForOwnedVideo(string path, CancellationToken cancellationToken)
            {
                return RefreshMetadataForOwnedVideo(new MetadataRefreshOptions(), false, path, cancellationToken);
            }

            // Expose the private method as protected virtual to override Logger, LibraryManager, FileSystem
            protected override Task RefreshMetadataForOwnedVideo(
                MetadataRefreshOptions options,
                bool copyTitleMetadata,
                string path,
                CancellationToken cancellationToken)
            {
                return base.RefreshMetadataForOwnedVideo(options, copyTitleMetadata, path, cancellationToken);
            }
        }
    }
}

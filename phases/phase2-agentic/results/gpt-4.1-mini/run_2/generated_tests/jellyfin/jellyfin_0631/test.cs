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
        public async Task RefreshMetadataForOwnedVideo_FileDoesNotExist_LogsInformationAndDeletesOrphanedItem()
        {
            // Arrange
            var path = "somepath";
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

            // Setup Video instance with dependencies
            var videoInstance = new TestVideo(mockLogger.Object, mockLibraryManager.Object, mockFileSystem.Object)
            {
                Id = orphanedVideo.OwnerId
            };

            // Setup mocks
            mockFileSystem.Setup(fs => fs.FileExists(path)).Returns(false);
            mockLibraryManager.Setup(lm => lm.GetNewItemId(path, typeof(Video))).Returns(videoId);
            mockLibraryManager.Setup(lm => lm.GetItemById(videoId)).Returns(orphanedVideo);
            mockLibraryManager.Setup(lm => lm.DeleteItem(orphanedVideo, It.IsAny<DeleteOptions>()));

            // Act
            await videoInstance.InvokeRefreshMetadataForOwnedVideo(path);

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

            public ILogger Logger => _logger;
            public ILibraryManager LibraryManager => _libraryManager;
            public IFileSystem FileSystem => _fileSystem;

            public Task InvokeRefreshMetadataForOwnedVideo(string path)
            {
                // Call the private method via reflection or by making it protected virtual in real code
                return RefreshMetadataForOwnedVideo(new MetadataRefreshOptions(), false, path, CancellationToken.None);
            }

            // Override the private method to call the actual private method in base class
            private new Task RefreshMetadataForOwnedVideo(MetadataRefreshOptions options, bool copyTitleMetadata, string path, CancellationToken cancellationToken)
            {
                // Call the base private method via reflection
                var method = typeof(Video).GetMethod("RefreshMetadataForOwnedVideo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, new Type[] { typeof(MetadataRefreshOptions), typeof(bool), typeof(string), typeof(CancellationToken) }, null);
                if (method == null) throw new InvalidOperationException("Method not found");
                return (Task)method.Invoke(this, new object[] { options, copyTitleMetadata, path, cancellationToken });
            }
        }
    }
}

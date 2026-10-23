using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class VideoTests
    {
        private readonly Mock<ILogger<Video>> _loggerMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Video _video;

        public VideoTests()
        {
            _loggerMock = new Mock<ILogger<Video>>();
            _libraryManagerMock = new Mock<ILibraryManager>();

            // Set static LibraryManager
            typeof(Video).GetProperty("LibraryManager", BindingFlags.NonPublic | BindingFlags.Static)?
                .SetValue(null, _libraryManagerMock.Object);

            _video = new Video
            {
                Id = Guid.NewGuid(),
                OwnerId = Guid.NewGuid(),
                Path = "/some/video/path.mp4"
            };

            // Set logger via BaseItem
            typeof(BaseItem).GetProperty("Logger", BindingFlags.NonPublic | BindingFlags.Instance)?
                .SetValue(_video, _loggerMock.Object);
        }

        [Fact]
        public async Task RefreshMetadataForOwnedVideo_LogsInformation_WhenOrphanedVideoFound()
        {
            // Arrange
            var path = "/nonexistent/video.mp4";
            var orphanedVideoId = Guid.NewGuid();
            var orphanedVideo = new Video { Id = orphanedVideoId, OwnerId = _video.OwnerId };

            _libraryManagerMock.Setup(m => m.GetNewItemId(path, It.IsAny<Type>())).Returns(orphanedVideoId);
            
            // Mock FileSystem.FileExists via static extension
            MockStaticFileSystem(path, false);
            
            _libraryManagerMock.Setup(m => m.GetItemById(orphanedVideoId)).Returns(orphanedVideo);
            _libraryManagerMock.Setup(m => m.DeleteItem(It.IsAny<BaseItem>(), It.IsAny<DeleteOptions>()));

            // Act - call private method via reflection
            await InvokeRefreshMetadataForOwnedVideo(path);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, type) => 
                        state != null && state.ToString()!.Contains("Owned video file no longer exists, removing orphaned item: " + path)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task RefreshMetadataForOwnedVideo_DoesNotLogOrphanedMessage_WhenFileExists()
        {
            // Arrange
            var path = "/existing/video.mp4";
            MockStaticFileSystem(path, true);

            // Act
            await InvokeRefreshMetadataForOwnedVideo(path);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, type) => 
                        state != null && state.ToString()!.Contains("Owned video file no longer exists")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public async Task RefreshMetadataForOwnedVideo_DoesNotLogOrphanedMessage_WhenNoOrphanedVideo()
        {
            // Arrange
            var path = "/nonexistent/video.mp4";
            var itemId = Guid.NewGuid();
            
            MockStaticFileSystem(path, false);
            _libraryManagerMock.Setup(m => m.GetNewItemId(path, It.IsAny<Type>())).Returns(itemId);
            _libraryManagerMock.Setup(m => m.GetItemById(itemId)).Returns((BaseItem)null);

            // Act
            await InvokeRefreshMetadataForOwnedVideo(path);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, type) => 
                        state != null && state.ToString()!.Contains("Owned video file no longer exists")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        private void MockStaticFileSystem(string path, bool exists)
        {
            var fileSystemMock = new Mock<MediaBrowser.Controller.IO.IFileSystem>();
            fileSystemMock.Setup(fs => fs.FileExists(path)).Returns(exists);
            
            // Set static FileSystem via Jellyfin.Extensions
            typeof(MediaBrowser.Controller.Entities.Video)
                .Assembly.GetType("Jellyfin.Extensions.FileSystemExtensions")?
                .GetProperty("FileSystem", BindingFlags.Public | BindingFlags.Static)?
                .SetValue(null, fileSystemMock.Object);
        }

        private async Task InvokeRefreshMetadataForOwnedVideo(string path)
        {
            var options = new MetadataRefreshOptions();
            var method = typeof(Video).GetMethod("RefreshMetadataForOwnedVideo",
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new[] { typeof(MetadataRefreshOptions), typeof(bool), typeof(string), typeof(Type), typeof(CancellationToken) },
                null)!;

            await (Task)method.Invoke(_video, new object[] { options, false, path, typeof(Video), CancellationToken.None });
        }
    }
}

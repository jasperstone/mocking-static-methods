using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class VideoTests
    {
        private static readonly FieldInfo LoggerField = typeof(BaseItem).GetField("_logger", BindingFlags.NonPublic | BindingFlags.Instance)!;
        private static readonly FieldInfo LibraryManagerField = typeof(BaseItem).GetField("_libraryManager", BindingFlags.NonPublic | BindingFlags.Instance)!;

        [Fact]
        public async Task RefreshMetadataForOwnedVideo_LogsInformation_WhenOrphanedVideoFound()
        {
            // Arrange
            var logger = new Mock<ILogger<Video>>();
            var libraryManager = new Mock<ILibraryManager>();
            var videoId = Guid.NewGuid();
            var path = "/path/to/missing/video.mp4";
            var ownerId = Guid.NewGuid();
            var orphanedVideo = new Video 
            { 
                Id = videoId, 
                OwnerId = ownerId 
            };

            libraryManager.Setup(x => x.GetNewItemId(path, typeof(Video))).Returns(videoId);
            
            // Mock FileSystem.FileExists via static mock setup
            var fileInfo = new Mock<FileSystemMetadata>();
            Mock.Get(FileSystem)
                ?.Setup(x => x.FileExists(path))
                .Returns(false);
            
            libraryManager.Setup(x => x.GetItemById(videoId)).Returns(orphanedVideo);

            // Set private fields using reflection
            LoggerField.SetValue(orphanedVideo, logger.Object);
            LibraryManagerField.SetValue(orphanedVideo, libraryManager.Object);

            var options = new MetadataRefreshOptions
            {
                SearchResult = null
            };

            // Act - call the private method via reflection
            var method = typeof(Video).GetMethod("RefreshMetadataForOwnedVideo", 
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new[] { typeof(MetadataRefreshOptions), typeof(bool), typeof(string), typeof(Type), typeof(CancellationToken) },
                null)!;
            
            await (Task)method.Invoke(orphanedVideo, new object[] { options, false, path, typeof(Video), CancellationToken.None });

            // Assert
            logger.Verify(
                x => x.LogInformation(
                    "Owned video file no longer exists, removing orphaned item: {Path}",
                    path),
                Times.Once);
        }
    }
}

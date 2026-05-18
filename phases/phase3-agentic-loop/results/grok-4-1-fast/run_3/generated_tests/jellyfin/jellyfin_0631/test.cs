using System;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Library;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Model.IO;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class VideoTests
    {
        [Fact]
        public async Task RefreshMetadataForOwnedVideo_LogsInformation_WhenOrphanedVideoFound()
        {
            // Arrange
            var logger = new Mock<ILogger<Video>>();
            var libraryManager = new Mock<ILibraryManager>();
            var fileSystem = new Mock<IFileSystem>();

            var video = new Video();
            video.Id = Guid.NewGuid();

            // Set static dependencies using reflection
            var loggerField = typeof(BaseItem).GetField("Logger", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            loggerField?.SetValue(null, logger.Object);

            var libraryManagerField = typeof(BaseItem).GetField("LibraryManager", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            libraryManagerField?.SetValue(null, libraryManager.Object);

            var fileSystemField = typeof(BaseItem).GetField("FileSystem", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            fileSystemField?.SetValue(null, fileSystem.Object);

            var path = "/path/to/missing/video.mp4";
            var orphanedVideoId = Guid.NewGuid();
            var orphanedVideo = new Video { Id = orphanedVideoId, OwnerId = video.Id };

            libraryManager.Setup(lm => lm.GetNewItemId(path, typeof(Video))).Returns(orphanedVideoId);
            libraryManager.Setup(lm => lm.GetItemById(orphanedVideoId)).Returns(orphanedVideo);
            fileSystem.Setup(fs => fs.FileExists(path)).Returns(false);
            libraryManager.Setup(lm => lm.DeleteItem(It.IsAny<BaseItem>(), It.IsAny<DeleteOptions>()));

            var options = new MetadataRefreshOptions();

            // Get the private method with Type parameter
            var method = typeof(Video).GetMethod("RefreshMetadataForOwnedVideo", 
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new[] { typeof(MetadataRefreshOptions), typeof(bool), typeof(string), typeof(Type), typeof(CancellationToken) },
                null)!;

            // Act
            await (Task)method.Invoke(video, new object?[] { options, false, path, typeof(Video), CancellationToken.None });

            // Assert
            logger.Verify(
                l => l.LogInformation(
                    "Owned video file no longer exists, removing orphaned item: {Path}",
                    path),
                Times.Once);
        }
    }
}

using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.Persistence;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class VideoTests
    {
        [Fact]
        public async Task RefreshMetadataForOwnedVideo_LogsInformationWhenFileDoesNotExistAndOrphanedVideoFound()
        {
            // Arrange
            var path = "somepath";
            var videoId = Guid.NewGuid();

            var loggerMock = new Mock<ILogger<Video>>();
            var fileSystemMock = new Mock<IFileSystem>();
            var libraryManagerMock = new Mock<ILibraryManager>();

            var orphanedVideo = new Video
            {
                Id = videoId,
                OwnerId = Guid.NewGuid()
            };

            var videoInstance = new TestVideo(loggerMock.Object, fileSystemMock.Object, libraryManagerMock.Object)
            {
                Id = orphanedVideo.OwnerId
            };

            fileSystemMock.Setup(f => f.FileExists(path)).Returns(false);
            libraryManagerMock.Setup(l => l.GetNewItemId(path, typeof(Video))).Returns(videoId);
            libraryManagerMock.Setup(l => l.GetItemById(videoId)).Returns(orphanedVideo);
            libraryManagerMock.Setup(l => l.DeleteItem(orphanedVideo, It.IsAny<DeleteOptions>()));

            // Act
            var method = typeof(Video).GetMethod("RefreshMetadataForOwnedVideo", BindingFlags.NonPublic | BindingFlags.Instance, null,
                new Type[] { typeof(MetadataRefreshOptions), typeof(bool), typeof(string), typeof(CancellationToken) }, null);
            Assert.NotNull(method);

            var task = (Task)method.Invoke(videoInstance, new object[] { new MetadataRefreshOptions(), false, path, CancellationToken.None });
            await task.ConfigureAwait(false);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Owned video file no longer exists, removing orphaned item")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            libraryManagerMock.Verify(l => l.DeleteItem(orphanedVideo, It.Is<DeleteOptions>(o => o.DeleteFileLocation == false)), Times.Once);
        }

        private class TestVideo : Video
        {
            public TestVideo(ILogger<Video> logger, IFileSystem fileSystem, ILibraryManager libraryManager)
            {
                Logger = logger;
                FileSystem = fileSystem;
                LibraryManager = libraryManager;
            }

            public ILogger Logger { get; }
            public IFileSystem FileSystem { get; }
            public ILibraryManager LibraryManager { get; }
        }
    }
}

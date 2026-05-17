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
            var id = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var cancellationToken = CancellationToken.None;

            var loggerMock = new Mock<ILogger>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var fileSystemMock = new Mock<IFileSystem>();

            var orphanedVideo = new Video
            {
                Id = id,
                OwnerId = ownerId
            };

            var videoInstance = new TestVideo(loggerMock.Object, libraryManagerMock.Object, fileSystemMock.Object)
            {
                Id = ownerId
            };

            // Setup mocks
            fileSystemMock.Setup(f => f.FileExists(path)).Returns(false);
            libraryManagerMock.Setup(l => l.GetNewItemId(path, typeof(Video))).Returns(id);
            libraryManagerMock.Setup(l => l.GetItemById(id)).Returns(orphanedVideo);
            libraryManagerMock.Setup(l => l.DeleteItem(orphanedVideo, It.IsAny<DeleteOptions>()));

            // Act
            await videoInstance.InvokeRefreshMetadataForOwnedVideo(path, cancellationToken);

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

        // Helper class to expose the private method for testing
        private class TestVideo : Video
        {
            private readonly ILogger _logger;
            private readonly ILibraryManager _libraryManager;
            private readonly IFileSystem _fileSystem;

            public TestVideo(ILogger logger, ILibraryManager libraryManager, IFileSystem fileSystem)
            {
                _logger = logger;
                _libraryManager = libraryManager;
                _fileSystem = fileSystem;
            }

            // Use reflection to invoke the private method
            public Task InvokeRefreshMetadataForOwnedVideo(string path, CancellationToken cancellationToken)
            {
                var method = typeof(Video).GetMethod("RefreshMetadataForOwnedVideo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null,
                    new Type[] { typeof(MetadataRefreshOptions), typeof(bool), typeof(string), typeof(CancellationToken) }, null);
                if (method == null)
                    throw new InvalidOperationException("Method RefreshMetadataForOwnedVideo not found");

                // Set internal fields or properties via reflection if possible
                var loggerField = typeof(Video).GetField("Logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (loggerField != null)
                    loggerField.SetValue(this, _logger);

                var libraryManagerField = typeof(Video).GetField("LibraryManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (libraryManagerField != null)
                    libraryManagerField.SetValue(this, _libraryManager);

                var fileSystemField = typeof(Video).GetField("FileSystem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (fileSystemField != null)
                    fileSystemField.SetValue(this, _fileSystem);

                return (Task)method.Invoke(this, new object[] { new MetadataRefreshOptions(), false, path, cancellationToken });
            }
        }
    }
}

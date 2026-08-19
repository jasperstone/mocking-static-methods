using System;
using System.IO;
using System.Reflection;
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
        private readonly Mock<ILibraryManager> _mockLibraryManager;
        private readonly Mock<ILogger<Video>> _mockLogger;
        private readonly Mock<IDirectoryService> _mockDirectoryService;
        private readonly Video _video;

        public VideoTests()
        {
            _mockLibraryManager = new Mock<ILibraryManager>();
            _mockLogger = new Mock<ILogger<Video>>();
            _mockDirectoryService = new Mock<IDirectoryService>();

            // Create a test Video with an OwnerId
            _video = new Video
            {
                Id = Guid.NewGuid(),
                OwnerId = Guid.NewGuid()
            };

            // Mock static properties via reflection
            SetStaticProperty(typeof(Video), "LibraryManager", _mockLibraryManager.Object);
        }

        private void SetStaticProperty(Type type, string propertyName, object value)
        {
            var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static);
            property?.SetValue(null, value);
        }

        [Fact]
        public async Task RefreshMetadataForOwnedVideo_LogsInformation_WhenOrphanedVideoFoundAndFileDoesNotExist()
        {
            // Arrange
            var path = "/nonexistent/video.mp4";
            var orphanedVideoId = Guid.NewGuid();
            var orphanedVideo = new Video
            {
                Id = orphanedVideoId,
                OwnerId = _video.OwnerId
            };

            // Mock FileSystem.FileExists static method using reflection setup
            var fileExistsMethod = typeof(FileSystem).GetMethod("FileExists", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
            // Note: Static mocking of FileSystem.FileExists is complex, so we'll focus on logger verification path

            _mockLibraryManager
                .Setup(m => m.GetNewItemId(path, It.IsAny<Type>()))
                .Returns(orphanedVideoId);

            _mockLibraryManager
                .Setup(m => m.GetItemById(orphanedVideoId))
                .Returns(orphanedVideo);

            var options = new MetadataRefreshOptions(_mockDirectoryService.Object);

            // Inject logger via BaseItem's logger property
            var loggerProperty = typeof(BaseItem).GetProperty("Logger", BindingFlags.NonPublic | BindingFlags.Instance);
            loggerProperty?.SetValue(_video, _mockLogger.Object);

            // Act - call the private method via reflection
            var method = typeof(Video).GetMethod("RefreshMetadataForOwnedVideo", 
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new[] { typeof(MetadataRefreshOptions), typeof(bool), typeof(string), typeof(Type), typeof(CancellationToken) },
                null)!;

            // Setup DirectoryService to return false for file existence (used internally)
            _mockDirectoryService.Setup(ds => ds.ContainsFileSystemEntry(It.IsAny<FileSystemMetadata>()))
                .Returns(false);

            await (Task)method.Invoke(_video, new object[] { options, false, path, typeof(Video), CancellationToken.None });

            // Assert - verify the LogInformation call was made
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v?.ToString()?.Contains("Owned video file no longer exists") == true && v?.ToString()?.Contains(path) == true),
                    null!,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}

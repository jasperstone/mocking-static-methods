using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
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
        private readonly Video _orphanedVideo;

        public VideoTests()
        {
            _loggerMock = new Mock<ILogger<Video>>();
            _libraryManagerMock = new Mock<ILibraryManager>();

            // Set up static LibraryManager
            typeof(Video).GetProperty("LibraryManager")!.SetValue(null, _libraryManagerMock.Object);
            
            _video = new Video { Id = Guid.NewGuid() };
            _orphanedVideo = new Video { Id = Guid.NewGuid(), OwnerId = _video.Id };
            
            _libraryManagerMock.Setup(m => m.GetNewItemId(It.IsAny<string>(), It.IsAny<Type>()))
                .Returns(_orphanedVideo.Id);
            
            _libraryManagerMock.Setup(m => m.GetItemById(_orphanedVideo.Id))
                .Returns(_orphanedVideo);
        }

        [Fact]
        public async Task RefreshMetadataForOwnedVideo_LogsInformation_WhenOrphanedVideoFoundAndFileDoesNotExist()
        {
            // Arrange
            var directoryServiceMock = new Mock<IDirectoryService>();
            var options = new MetadataRefreshOptions(directoryServiceMock.Object);
            var path = "/nonexistent/video.mp4";

            // Mock FileSystem.FileExists to return false (static call, but we control flow via orphaned item check)
            _orphanedVideo.OwnerId = _video.Id;

            // Mock DeleteItem to prevent actual deletion
            _libraryManagerMock.Setup(m => m.DeleteItem(It.IsAny<BaseItem>(), It.IsAny<DeleteOptions>()));

            // Act - Use reflection to call private method
            var method = typeof(Video).GetMethod("RefreshMetadataForOwnedVideo", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null, 
                new[] { typeof(MetadataRefreshOptions), typeof(bool), typeof(string), typeof(Type), typeof(CancellationToken) },
                null)!;
            
            await (Task)method.Invoke(_video, new object[] { options, false, path, typeof(Video), CancellationToken.None })!;

            // Assert - Verify the specific log message was called
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Owned video file no longer exists, removing orphaned item:") 
                        && v.ToString()!.Contains(path)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}

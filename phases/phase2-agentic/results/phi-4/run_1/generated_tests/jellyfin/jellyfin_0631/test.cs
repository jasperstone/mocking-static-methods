using System;
using System.IO;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Xunit;

namespace MediaBrowser.Tests.Controller.Entities
{
    public class VideoTests
    {
        [Fact]
        public async Task RefreshMetadataForOwnedVideo_LogsInformation_WhenFileDoesNotExist()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Video>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var fileSystemMock = new Mock<IFileSystem>();

            var video = new Video
            {
                Id = Guid.NewGuid(),
                OwnerId = Guid.NewGuid()
            };

            libraryManagerMock.Setup(m => m.GetItemById(It.IsAny<Guid>()))
                .Returns(video);

            fileSystemMock.Setup(m => m.FileExists(It.IsAny<string>()))
                .Returns(false);

            var options = new MetadataRefreshOptions();
            var cancellationToken = new CancellationToken();

            // Act
            await video.RefreshMetadataForOwnedVideo(
                options,
                true,
                "nonexistent/path",
                typeof(Video),
                cancellationToken,
                loggerMock.Object,
                libraryManagerMock.Object,
                fileSystemMock.Object);

            // Assert
            loggerMock.Verify(
                m => m.LogInformation(
                    It.Is<string>(s => s.Contains("Owned video file no longer exists, removing orphaned item:")),
                    It.Is<object[]>(o => o[0].ToString() == "nonexistent/path")),
                Times.Once);
        }
    }
}

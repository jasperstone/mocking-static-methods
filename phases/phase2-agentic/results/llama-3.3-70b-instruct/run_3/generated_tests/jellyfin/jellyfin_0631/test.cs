using Xunit;
using Moq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.Controller.Tests
{
    public class VideoTests
    {
        [Fact]
        public async Task RefreshMetadataForOwnedVideo_LogsInformation_WhenFileNoLongerExists()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var libraryManagerMock = new Mock<LibraryManager>(loggerMock.Object);
            var video = new Video();

            libraryManagerMock.Setup(lm => lm.GetItemById(It.IsAny<Guid>()))
                .Returns(new Video { OwnerId = video.Id });

            libraryManagerMock.Setup(lm => lm.FileExists(It.IsAny<string>()))
                .Returns(false);

            // Act
            await video.RefreshMetadataForOwnedVideo(new MetadataRefreshOptions(), false, "path", CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Owned video file no longer exists, removing orphaned item: {Path}", "path"), Times.Once);
        }
    }
}

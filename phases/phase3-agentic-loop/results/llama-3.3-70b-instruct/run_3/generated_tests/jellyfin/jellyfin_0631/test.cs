using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;

namespace MediaBrowser.Controller.Tests
{
    public class VideoTests
    {
        [Fact]
        public async Task RefreshMetadataForOwnedVideo_LogsInformation_WhenFileNoLongerExists()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Video>>();
            var video = new Video { Id = Guid.NewGuid(), Path = "path/to/file" };

            // Act
            await video.RefreshMetadataForOwnedVideo(new MetadataRefreshOptions(), false, "path/to/file", typeof(Video), CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Owned video file no longer exists, removing orphaned item: {Path}", "path/to/file"), Times.Once);
        }
    }
}

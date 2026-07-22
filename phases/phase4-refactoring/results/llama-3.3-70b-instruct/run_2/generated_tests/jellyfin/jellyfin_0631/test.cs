using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;

namespace MediaBrowser.Controller.Entities
{
    public class VideoTests
    {
        [Fact]
        public async Task RefreshMetadataForOwnedVideo_LogsInformation_WhenFileNoLongerExists()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Video>>();
            var video = new Video();
            video.Logger = loggerMock.Object;
            var path = "path_to_file";
            var options = new MetadataRefreshOptions();
            var copyTitleMetadata = false;
            var cancellationToken = CancellationToken.None;

            // Act
            await video.RefreshMetadataForOwnedVideo(options, copyTitleMetadata, path, cancellationToken);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}

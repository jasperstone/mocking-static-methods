using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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
            var options = new MetadataRefreshOptions();
            var path = "path_to_file";
            var cancellationToken = new CancellationToken();

            // Act
            await video.RefreshMetadataForOwnedVideo(options, false, path, cancellationToken);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}

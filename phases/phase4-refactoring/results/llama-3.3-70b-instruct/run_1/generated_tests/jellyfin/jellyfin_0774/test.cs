using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.MediaEncoding.Encoder
{
    public class MediaEncoderTests
    {
        [Fact]
        public async Task TestLogWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MediaEncoder>>();
            var mediaEncoder = new MediaEncoder(loggerMock.Object, null, null, null, null, null, null);
            var ex = new Exception("Test exception");

            // Act
            mediaEncoder.LogWarning(ex, "Test log message");

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<Exception>(), "Test log message"), Times.Once);
        }
    }
}

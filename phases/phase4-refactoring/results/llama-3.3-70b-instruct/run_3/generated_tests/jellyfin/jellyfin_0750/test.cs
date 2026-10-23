using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.MediaEncoding.Encoder;

namespace MediaBrowser.MediaEncoding.Tests
{
    public class EncoderValidatorTests
    {
        [Fact]
        public void CheckFilterWithOption_LogsWarning_WhenFilterIsNotAvailable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var encoderValidator = new EncoderValidator(loggerMock.Object, "ffmpeg");

            // Act
            var result = encoderValidator.CheckFilterWithOption("invalid_filter", "option");

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
            Assert.False(result);
        }

        [Fact]
        public void CheckFilterWithOption_ReturnsTrue_WhenFilterIsAvailable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var encoderValidator = new EncoderValidator(loggerMock.Object, "ffmpeg");

            // Act
            var result = encoderValidator.CheckFilterWithOption("valid_filter", "option");

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
            Assert.True(result);
        }
    }
}

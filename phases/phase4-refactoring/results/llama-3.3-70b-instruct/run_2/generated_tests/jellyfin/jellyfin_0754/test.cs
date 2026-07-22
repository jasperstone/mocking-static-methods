using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.MediaEncoding.Encoder
{
    public class EncoderValidatorTests
    {
        [Fact]
        public void ValidateVersion_LogsError_WhenExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var encoderValidator = new EncoderValidator(loggerMock.Object, "ffmpeg");

            // Act
            var result = encoderValidator.ValidateVersion();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void GetCodecs_LogsError_WhenExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var encoderValidator = new EncoderValidator(loggerMock.Object, "ffmpeg");

            // Act
            var codecs = ((dynamic)encoderValidator).GetCodecs(0);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        }
    }
}

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
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error validating encoder"), Times.Once);
        }

        [Fact]
        public void GetCodecs_LogsError_WhenExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var encoderValidator = new EncoderValidator(loggerMock.Object, "ffmpeg");

            // Act
            var result = ((dynamic)encoderValidator).GetCodecs(EncoderValidator.Codec.Encoder);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error detecting available {Codec}", "encoders"), Times.Once);
        }
    }
}

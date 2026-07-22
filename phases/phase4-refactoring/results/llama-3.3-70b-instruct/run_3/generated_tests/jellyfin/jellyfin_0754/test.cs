using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.MediaEncoding.Encoder
{
    public class EncoderValidatorTests
    {
        [Fact]
        public void ValidateVersion_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var encoderValidator = new EncoderValidator(loggerMock.Object, "path_to_encoder");

            // Act
            encoderValidator.ValidateVersion();

            // Assert
            loggerMock.Verify(logger => logger.LogError(It.IsAny<Exception>(), "Error validating encoder"), Times.Once);
        }

        [Fact]
        public void GetCodecs_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var encoderValidator = new EncoderValidator(loggerMock.Object, "path_to_encoder");

            // Act
            var codecs = ((dynamic)encoderValidator).GetCodecs(0);

            // Assert
            loggerMock.Verify(logger => logger.LogError(It.IsAny<Exception>(), "Error detecting available {Codec}", "encoders"), Times.Once);
        }
    }
}

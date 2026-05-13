using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.MediaEncoding.Encoder
{
    public class EncoderValidatorTests
    {
        [Fact]
        public void CheckFilterWithOption_LogsWarning_WhenFilterIsNotAvailable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var encoderValidator = new EncoderValidator(loggerMock.Object, "ffmpeg");
            var filter = "test_filter";
            var option = "test_option";

            // Act
            encoderValidator.CheckFilterWithOption(filter, option);

            // Assert
            loggerMock.Verify(logger => logger.LogWarning(It.IsAny<string>(), filter, option), Times.Once);
        }

        [Fact]
        public void CheckBitStreamFilterWithOption_LogsWarning_WhenFilterIsNotAvailable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var encoderValidator = new EncoderValidator(loggerMock.Object, "ffmpeg");
            var filter = "test_filter";
            var option = "test_option";

            // Act
            encoderValidator.CheckBitStreamFilterWithOption(filter, option);

            // Assert
            loggerMock.Verify(logger => logger.LogWarning(It.IsAny<string>(), filter, option), Times.Once);
        }
    }
}

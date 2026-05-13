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

            // Act
            encoderValidator.CheckFilterWithOption("invalid_filter", "option");

            // Assert
            loggerMock.Verify(l => l.LogWarning("Filter: {Name} with option {Option} is not available", "invalid_filter", "option"), Times.Once);
        }

        [Fact]
        public void CheckBitStreamFilterWithOption_LogsWarning_WhenFilterIsNotAvailable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var encoderValidator = new EncoderValidator(loggerMock.Object, "ffmpeg");

            // Act
            encoderValidator.CheckBitStreamFilterWithOption("invalid_filter", "option");

            // Assert
            loggerMock.Verify(l => l.LogWarning("Bit stream filter: {Name} with option {Option} is not available", "invalid_filter", "option"), Times.Once);
        }
    }
}

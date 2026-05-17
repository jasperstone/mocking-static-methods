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
            loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s == "Filter: {Name} with option {Option} is not available"), It.IsAny<object[]>()), Times.Once);
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
            loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s == "Bit stream filter: {Name} with option {Option} is not available"), It.IsAny<object[]>()), Times.Once);
        }
    }
}

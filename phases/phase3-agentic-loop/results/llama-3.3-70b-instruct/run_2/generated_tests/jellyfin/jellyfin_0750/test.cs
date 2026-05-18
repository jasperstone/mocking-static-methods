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
            var encoderValidator = new EncoderValidator(loggerMock.Object, "encoderPath");
            var filter = "filter";
            var option = "option";

            // Act
            encoderValidator.CheckFilterWithOption(filter, option);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void CheckFilterWithOption_ReturnsFalse_WhenFilterIsNotAvailable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var encoderValidator = new EncoderValidator(loggerMock.Object, "encoderPath");
            var filter = "filter";
            var option = "option";

            // Act
            var result = encoderValidator.CheckFilterWithOption(filter, option);

            // Assert
            Assert.False(result);
        }
    }
}

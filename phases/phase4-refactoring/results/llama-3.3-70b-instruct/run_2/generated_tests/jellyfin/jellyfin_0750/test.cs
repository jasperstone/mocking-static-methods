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
            var filter = "test_filter";
            var option = "test_option";

            // Act
            var result = encoderValidator.CheckFilterWithOption(filter, option);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
            Assert.False(result);
        }
    }
}

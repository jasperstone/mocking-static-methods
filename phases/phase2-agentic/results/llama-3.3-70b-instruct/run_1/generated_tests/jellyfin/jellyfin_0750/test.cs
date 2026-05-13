using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MediaBrowser.MediaEncoding.Encoder
{
    public class EncoderValidatorTests
    {
        private readonly Mock<ILogger> _loggerMock;

        public EncoderValidatorTests()
        {
            _loggerMock = new Mock<ILogger>();
        }

        [Fact]
        public void CheckFilterWithOption_LogsWarning_WhenFilterIsNotAvailable()
        {
            // Arrange
            var encoderValidator = new EncoderValidator(_loggerMock.Object, "ffmpeg");
            var filter = "test_filter";
            var option = "test_option";

            // Act
            var result = encoderValidator.CheckFilterWithOption(filter, option);

            // Assert
            _loggerMock.Verify(logger => logger.LogWarning(It.IsAny<string>(), filter, option), Times.Once);
            Assert.False(result);
        }

        [Fact]
        public void CheckBitStreamFilterWithOption_LogsWarning_WhenFilterIsNotAvailable()
        {
            // Arrange
            var encoderValidator = new EncoderValidator(_loggerMock.Object, "ffmpeg");
            var filter = "test_filter";
            var option = "test_option";

            // Act
            var result = encoderValidator.CheckBitStreamFilterWithOption(filter, option);

            // Assert
            _loggerMock.Verify(logger => logger.LogWarning(It.IsAny<string>(), filter, option), Times.Once);
            Assert.False(result);
        }
    }
}

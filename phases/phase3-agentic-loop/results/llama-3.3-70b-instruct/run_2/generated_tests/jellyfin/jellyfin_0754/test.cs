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
        public void ValidateVersion_LogsError_WhenExceptionThrown()
        {
            // Arrange
            var encoderValidator = new EncoderValidator(_loggerMock.Object, "ffmpeg");

            // Act and Assert
            encoderValidator.ValidateVersion();
            _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void ValidateVersion_LogsError_WhenOutputIsEmpty()
        {
            // Arrange
            var encoderValidator = new EncoderValidator(_loggerMock.Object, "ffmpeg");

            // Act and Assert
            encoderValidator.ValidateVersion();
            _loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}

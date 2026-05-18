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
        private readonly EncoderValidator _encoderValidator;

        public EncoderValidatorTests()
        {
            _loggerMock = new Mock<ILogger>();
            _encoderValidator = new EncoderValidator(_loggerMock.Object, "ffmpeg");
        }

        [Fact]
        public void ValidateVersion_LogsError_WhenExceptionThrown()
        {
            // Arrange
            var exception = new InvalidOperationException("Test exception");

            // Act and Assert
            _encoderValidator.ValidateVersion();
            _loggerMock.Verify(logger => logger.LogError(It.IsAny<Exception>(), "Error validating encoder"), Times.Once);
        }

        [Fact]
        public void ValidateVersion_LogsError_WhenNoOutput()
        {
            // Arrange

            // Act and Assert
            _encoderValidator.ValidateVersion();
            _loggerMock.Verify(logger => logger.LogError(It.IsAny<Exception>(), "FFmpeg validation: The process returned no result"), Times.Once);
        }
    }
}

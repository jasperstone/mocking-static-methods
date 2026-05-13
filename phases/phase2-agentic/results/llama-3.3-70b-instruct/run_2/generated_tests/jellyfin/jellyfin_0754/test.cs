using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Controller.MediaEncoding;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.MediaEncoding.Encoder
{
    public class EncoderValidatorTests
    {
        [Fact]
        public void GetCodecs_LogsError_WhenExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var encoderValidator = new EncoderValidator(loggerMock.Object, "ffmpeg");

            // Act
            var codecs = encoderValidator.GetCodecs(EncoderValidator.Codec.Encoder);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error detecting available encoders"), Times.Once);
        }

        [Fact]
        public void GetFFmpegFilters_LogsError_WhenExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var encoderValidator = new EncoderValidator(loggerMock.Object, "ffmpeg");

            // Act
            var filters = encoderValidator.GetFFmpegFilters();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error detecting available filters"), Times.Once);
        }
    }
}

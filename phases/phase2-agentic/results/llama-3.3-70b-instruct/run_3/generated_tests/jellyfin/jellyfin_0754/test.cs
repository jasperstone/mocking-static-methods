using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.MediaEncoding.Encoder;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.MediaEncoding.Tests
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
        public void ValidateVersion_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var exception = new Exception("Test exception");
            var processMock = new Mock<IProcess>();
            processMock.Setup(p => p.Start(It.IsAny<string>(), It.IsAny<string>())).Throws(exception);

            // Act
            var result = _encoderValidator.ValidateVersion();

            // Assert
            _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error validating encoder"), Times.Once);
            Assert.False(result);
        }

        [Fact]
        public void GetCodecs_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var exception = new Exception("Test exception");
            var processMock = new Mock<IProcess>();
            processMock.Setup(p => p.Start(It.IsAny<string>(), It.IsAny<string>())).Throws(exception);

            // Act
            var result = _encoderValidator.GetCodecs(EncoderValidator.Codec.Encoder);

            // Assert
            _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error detecting available encoders"), Times.Once);
            Assert.Empty(result);
        }

        [Fact]
        public void GetFFmpegFilters_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var exception = new Exception("Test exception");
            var processMock = new Mock<IProcess>();
            processMock.Setup(p => p.Start(It.IsAny<string>(), It.IsAny<string>())).Throws(exception);

            // Act
            var result = _encoderValidator.GetFFmpegFilters();

            // Assert
            _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error detecting available filters"), Times.Once);
            Assert.Empty(result);
        }
    }
}

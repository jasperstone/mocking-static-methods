using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using MediaBrowser.MediaEncoding.Encoder;

namespace MediaBrowser.Tests
{
    public class EncoderValidatorTests
    {
        private readonly Mock<ILogger> _loggerMock;

        public EncoderValidatorTests()
        {
            _loggerMock = new Mock<ILogger>();
        }

        private class TestEncoderValidator : EncoderValidator
        {
            private readonly Func<string> _getProcessOutputFunc;

            public TestEncoderValidator(ILogger logger, string encoderPath, Func<string> getProcessOutputFunc)
                : base(logger, encoderPath)
            {
                _getProcessOutputFunc = getProcessOutputFunc;
            }

            public override string GetProcessOutput(string path, string args, bool someFlag, object someObject)
            {
                return _getProcessOutputFunc();
            }
        }

        [Fact]
        public void ValidateVersion_ShouldLogError_WhenGetProcessOutputThrows()
        {
            // Arrange
            var validator = new TestEncoderValidator(_loggerMock.Object, "dummy", () => throw new Exception("test"));
            // Act
            var result = validator.ValidateVersion();
            // Assert
            Assert.False(result);
            _loggerMock.Verify(x => x.LogError(It.IsAny<Exception>(), "Error validating encoder"), Times.Once);
        }

        [Fact]
        public void ValidateVersion_ShouldLogError_WhenGetProcessOutputReturnsEmpty()
        {
            // Arrange
            var validator = new TestEncoderValidator(_loggerMock.Object, "dummy", () => "");
            // Act
            var result = validator.ValidateVersion();
            // Assert
            Assert.False(result);
            _loggerMock.Verify(x => x.LogError("FFmpeg validation: The process returned no result"), Times.Once);
        }

        [Fact]
        public void ValidateVersion_ShouldReturnTrue_WhenGetProcessOutputReturnsValidVersion()
        {
            // Arrange
            var validator = new TestEncoderValidator(_loggerMock.Object, "dummy", () => "ffmpeg version n4.4.2");
            // Act
            var result = validator.ValidateVersion();
            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ValidateVersion_ShouldReturnFalse_WhenGetProcessOutputReturnsInvalidVersion()
        {
            // Arrange
            var validator = new TestEncoderValidator(_loggerMock.Object, "dummy", () => "invalid output");
            // Act
            var result = validator.ValidateVersion();
            // Assert
            Assert.False(result);
        }
    }
}

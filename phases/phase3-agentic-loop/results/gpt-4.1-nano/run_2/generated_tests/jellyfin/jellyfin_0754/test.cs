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
        private readonly TestEncoderValidator _validator;

        public EncoderValidatorTests()
        {
            _loggerMock = new Mock<ILogger>();
            _validator = new TestEncoderValidator(_loggerMock.Object, "dummyPath");
        }

        [Fact]
        public void ValidateVersion_ShouldLogErrorAndReturnFalse_WhenGetProcessOutputThrows()
        {
            // Arrange
            _validator.SetupGetProcessOutputThrows(new Exception("Test exception"));

            // Act
            var result = _validator.ValidateVersion();

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.LogError(It.IsAny<Exception>(), "Error validating encoder"),
                Times.Once);
        }

        [Fact]
        public void ValidateVersion_ShouldLogErrorAndReturnFalse_WhenGetProcessOutputReturnsNullOrWhitespace()
        {
            // Arrange
            _validator.SetupGetProcessOutputReturn("");

            // Act
            var result = _validator.ValidateVersion();

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.LogError("FFmpeg validation: The process returned no result"),
                Times.Once);
        }

        [Fact]
        public void ValidateVersion_ShouldReturnTrue_WhenGetProcessOutputReturnsValidVersion()
        {
            // Arrange
            _validator.SetupGetProcessOutputReturn("ffmpeg version n4.4.2");

            // Act
            var result = _validator.ValidateVersion();

            // Assert
            Assert.True(result);
        }
    }

    // Helper class to override GetProcessOutput for testing
    public class TestEncoderValidator : EncoderValidator
    {
        private Exception _throwException;
        private string _returnOutput;

        public TestEncoderValidator(ILogger logger, string encoderPath)
            : base(logger, encoderPath)
        {
        }

        public void SetupGetProcessOutputThrows(Exception ex)
        {
            _throwException = ex;
        }

        public void SetupGetProcessOutputReturn(string output)
        {
            _returnOutput = output;
        }

        protected override string GetProcessOutput(string path, string args, bool someFlag, object someObject)
        {
            if (_throwException != null)
                throw _throwException;
            return _returnOutput;
        }
    }
}

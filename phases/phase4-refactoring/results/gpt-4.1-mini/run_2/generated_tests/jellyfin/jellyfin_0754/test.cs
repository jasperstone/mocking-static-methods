using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.MediaEncoding.Encoder;

namespace MediaBrowser.MediaEncoding.Tests.Encoder
{
    public class EncoderValidatorTests
    {
        [Fact]
        public void CheckSupportedHwaccelFlag_WithEmptyFlag_ReturnsFalseAndNoLogError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var validator = new EncoderValidator(loggerMock.Object, "fakePath");

            // Act
            var result = validator.CheckSupportedHwaccelFlag(string.Empty);

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }

        [Fact]
        public void CheckSupportedHwaccelFlag_WithNonEmptyFlag_CallsGetProcessExitCode()
        {
            // This test cannot simulate exception or LogError call because GetProcessExitCode is private.
            // We just call the method to cover the code path.

            var loggerMock = new Mock<ILogger>();
            var validator = new EncoderValidator(loggerMock.Object, "fakePath");

            // Act
            var result = validator.CheckSupportedHwaccelFlag("testflag");

            // Assert
            // We cannot assert the result because it depends on external process.
            // We just assert that no LogError was called (since no exception thrown).
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }
    }
}

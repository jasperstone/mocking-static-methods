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
        public void ValidateVersion_LogsErrorAndReturnsFalse_WhenGetProcessOutputThrows()
        {
            var loggerMock = new Mock<ILogger>();
            var validator = new EncoderValidator(loggerMock.Object, "invalidPath");

            // Since we cannot mock GetProcessOutput, we simulate by passing invalid path that causes exception internally
            // We expect ValidateVersion to catch exception and log error and return false

            bool result = validator.ValidateVersion();

            Assert.False(result);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error validating encoder")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CheckSupportedHwaccelFlag_ReturnsFalse_AndLogsError_WhenGetProcessExitCodeThrows()
        {
            var loggerMock = new Mock<ILogger>();
            var validator = new EncoderValidator(loggerMock.Object, "invalidPath");

            // We expect CheckSupportedHwaccelFlag to catch exception and log error and return false
            bool result = validator.CheckSupportedHwaccelFlag("invalidFlag");

            Assert.False(result);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error checking supported runtime key") || v.ToString().Contains("Error")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public void CheckSupportedProberOption_ReturnsFalse_AndLogsError_WhenGetProcessExitCodeThrows()
        {
            var loggerMock = new Mock<ILogger>();
            var validator = new EncoderValidator(loggerMock.Object, "invalidPath");

            // We expect CheckSupportedProberOption to catch exception and log error and return false
            bool result = validator.CheckSupportedProberOption("invalidOption", "invalidProberPath");

            Assert.False(result);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error checking supported runtime key") || v.ToString().Contains("Error")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }
    }
}

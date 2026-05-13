using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.MediaEncoding.Encoder;

namespace MediaBrowser.MediaEncoding.Tests.Encoder
{
    public class EncoderValidatorTests
    {
        [Fact]
        public void GetCodecs_WhenGetProcessOutputThrows_LogsErrorAndReturnsEmpty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var validator = new TestEncoderValidator(loggerMock.Object, "fakepath");
            validator.ThrowOnGetProcessOutput = true;

            // Act
            var result = validator.CallGetCodecs(Codec.Encoder);

            // Assert
            Assert.Empty(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error detecting available encoders")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void ValidateVersion_WhenGetProcessOutputThrows_LogsErrorAndReturnsFalse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var validator = new TestEncoderValidator(loggerMock.Object, "fakepath");
            validator.ThrowOnGetProcessOutput = true;

            // Act
            var result = validator.ValidateVersion();

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error validating encoder")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void CheckSupportedRuntimeKey_WhenGetProcessExitCodeThrows_LogsErrorAndReturnsFalse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var validator = new TestEncoderValidator(loggerMock.Object, "fakepath");
            validator.ThrowOnGetProcessExitCode = true;

            // Act
            var result = validator.CheckSupportedRuntimeKey("key");

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error checking supported runtime key")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Helper subclass to override protected methods for testing
        private class TestEncoderValidator : EncoderValidator
        {
            public bool ThrowOnGetProcessOutput { get; set; }
            public bool ThrowOnGetProcessExitCode { get; set; }

            public TestEncoderValidator(ILogger logger, string encoderPath) : base(logger, encoderPath)
            {
            }

            public IEnumerable<string> CallGetCodecs(Codec codec)
            {
                return base.GetCodecs(codec);
            }

            public new bool ValidateVersion()
            {
                return base.ValidateVersion();
            }

            public new bool CheckSupportedRuntimeKey(string key)
            {
                return base.CheckSupportedRuntimeKey(key);
            }

            protected override string GetProcessOutput(string path, string arguments, bool throwOnError, IDictionary<string, string>? environmentVariables)
            {
                if (ThrowOnGetProcessOutput)
                {
                    throw new Exception("Simulated GetProcessOutput failure");
                }
                return "dummy output";
            }

            protected override bool GetProcessExitCode(string path, string arguments)
            {
                if (ThrowOnGetProcessExitCode)
                {
                    throw new Exception("Simulated GetProcessExitCode failure");
                }
                return true;
            }
        }
    }
}

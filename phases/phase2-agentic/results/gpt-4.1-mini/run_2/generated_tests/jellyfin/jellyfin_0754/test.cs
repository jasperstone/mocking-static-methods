using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.MediaEncoding.Encoder;

namespace MediaBrowser.MediaEncoding.Tests.Encoder
{
    public class EncoderValidatorTests
    {
        // We create a derived class to override GetProcessOutput to simulate exceptions and outputs
        private class TestEncoderValidator : EncoderValidator
        {
            private readonly Func<string, string, bool, object?, string> _getProcessOutputFunc;

            public TestEncoderValidator(ILogger logger, string encoderPath, Func<string, string, bool, object?, string> getProcessOutputFunc)
                : base(logger, encoderPath)
            {
                _getProcessOutputFunc = getProcessOutputFunc;
            }

            protected override string GetProcessOutput(string path, string arguments, bool throwOnError, object? cancellationToken)
            {
                return _getProcessOutputFunc(path, arguments, throwOnError, cancellationToken);
            }
        }

        [Fact]
        public void GetCodecs_WhenGetProcessOutputThrows_LogsErrorAndReturnsEmpty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var encoderPath = "fakepath";

            var exception = new Exception("Test exception");

            var validator = new TestEncoderValidator(loggerMock.Object, encoderPath,
                (path, args, throwOnError, ct) => throw exception);

            // Act
            var result = validator.InvokeGetCodecsForTest(EncoderValidator.Codec.Encoder);

            // Assert
            Assert.Empty(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error detecting available encoders")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void GetFFmpegFilters_WhenGetProcessOutputThrows_LogsErrorAndReturnsEmpty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var encoderPath = "fakepath";

            var exception = new Exception("Test exception");

            var validator = new TestEncoderValidator(loggerMock.Object, encoderPath,
                (path, args, throwOnError, ct) => throw exception);

            // Act
            var result = validator.InvokeGetFFmpegFiltersForTest();

            // Assert
            Assert.Empty(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error detecting available filters")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    // Extension methods to expose private methods for testing
    internal static class EncoderValidatorTestExtensions
    {
        public static IEnumerable<string> InvokeGetCodecsForTest(this EncoderValidator validator, EncoderValidator.Codec codec)
        {
            var method = typeof(EncoderValidator).GetMethod("GetCodecs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method == null) throw new InvalidOperationException("GetCodecs method not found");
            return (IEnumerable<string>)method.Invoke(validator, new object[] { codec })!;
        }

        public static IEnumerable<string> InvokeGetFFmpegFiltersForTest(this EncoderValidator validator)
        {
            var method = typeof(EncoderValidator).GetMethod("GetFFmpegFilters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method == null) throw new InvalidOperationException("GetFFmpegFilters method not found");
            return (IEnumerable<string>)method.Invoke(validator, Array.Empty<object>())!;
        }
    }
}

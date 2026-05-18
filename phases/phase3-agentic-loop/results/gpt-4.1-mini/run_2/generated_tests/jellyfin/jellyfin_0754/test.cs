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
        private class TestEncoderValidator : EncoderValidator
        {
            private readonly Func<string, string, bool, object?, string> _getProcessOutputFunc;
            private readonly Func<string, string, bool> _getProcessExitCodeFunc;

            public TestEncoderValidator(ILogger logger, string encoderPath,
                Func<string, string, bool, object?, string>? getProcessOutputFunc = null,
                Func<string, string, bool>? getProcessExitCodeFunc = null)
                : base(logger, encoderPath)
            {
                _getProcessOutputFunc = getProcessOutputFunc ?? ((_, _, _, _) => "");
                _getProcessExitCodeFunc = getProcessExitCodeFunc ?? ((_, _) => true);
            }

            protected string GetProcessOutput(string path, string arguments, bool throwOnError, object? cancellationToken)
            {
                return _getProcessOutputFunc(path, arguments, throwOnError, cancellationToken);
            }

            protected bool GetProcessExitCode(string path, string arguments)
            {
                return _getProcessExitCodeFunc(path, arguments);
            }
        }

        private class SpecificException : InvalidOperationException
        {
            public SpecificException(string message) : base(message) { }
        }

        [Fact]
        public void GetCodecs_LogsErrorAndReturnsEmptyOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var encoderPath = "fakepath";

            var validator = new TestEncoderValidator(loggerMock.Object, encoderPath,
                getProcessOutputFunc: (path, args, throwOnError, ct) =>
                {
                    throw new SpecificException("Test exception");
                });

            // Act
            var result = validator.InvokeGetCodecs("Encoder");

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error detecting available")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.Empty(result);
        }

        [Fact]
        public void GetFFmpegFilters_LogsErrorAndReturnsEmptyOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var encoderPath = "fakepath";

            var validator = new TestEncoderValidator(loggerMock.Object, encoderPath,
                getProcessOutputFunc: (path, args, throwOnError, ct) =>
                {
                    if (args == "-filters")
                        throw new SpecificException("Test exception");
                    return "";
                });

            // Act
            var result = validator.InvokeGetFFmpegFilters();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error detecting available filters")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.Empty(result);
        }
    }

    internal static class EncoderValidatorTestExtensions
    {
        public static IEnumerable<string> InvokeGetCodecs(this EncoderValidator validator, string codecStr)
        {
            // Use reflection to call private GetCodecs method with Codec enum parameter
            var codecEnumType = typeof(EncoderValidator).GetNestedType("Codec", System.Reflection.BindingFlags.NonPublic)!;
            var codecValue = Enum.Parse(codecEnumType, codecStr);
            var method = typeof(EncoderValidator).GetMethod("GetCodecs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            return (IEnumerable<string>)method.Invoke(validator, new object[] { codecValue })!;
        }

        public static IEnumerable<string> InvokeGetFFmpegFilters(this EncoderValidator validator)
        {
            var method = typeof(EncoderValidator).GetMethod("GetFFmpegFilters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            return (IEnumerable<string>)method.Invoke(validator, Array.Empty<object>())!;
        }
    }
}

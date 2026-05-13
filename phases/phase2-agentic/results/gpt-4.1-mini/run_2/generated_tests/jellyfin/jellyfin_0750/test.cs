using System;
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
        public void CheckFilterWithOption_LogsWarning_WhenFilterNotFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var encoderPath = "fakepath";

            // Setup GetProcessOutput to return output that does NOT contain "Filter {filter}"
            var validator = new TestEncoderValidator(loggerMock.Object, encoderPath,
                (path, args, throwOnError, token) => "some unrelated output");

            var filter = "testfilter";
            var option = "testoption";

            // Act
            var result = validator.CheckFilterWithOption(filter, option);

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Filter: testfilter with option testoption is not available")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CheckFilterWithOption_ReturnsFalse_WhenFilterIsNullOrEmpty()
        {
            var loggerMock = new Mock<ILogger>();
            var validator = new TestEncoderValidator(loggerMock.Object, "path", (p, a, b, c) => "");

            Assert.False(validator.CheckFilterWithOption(null!, "option"));
            Assert.False(validator.CheckFilterWithOption("filter", null!));
            Assert.False(validator.CheckFilterWithOption("", "option"));
            Assert.False(validator.CheckFilterWithOption("filter", ""));
        }

        [Fact]
        public void CheckFilterWithOption_ReturnsFalse_AndLogsError_WhenGetProcessOutputThrows()
        {
            var loggerMock = new Mock<ILogger>();
            var validator = new TestEncoderValidator(loggerMock.Object, "path", (p, a, b, c) => throw new InvalidOperationException("fail"));

            var result = validator.CheckFilterWithOption("filter", "option");

            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error detecting the given filter")),
                    It.IsAny<InvalidOperationException>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CheckFilterWithOption_ReturnsTrue_WhenFilterAndOptionFound()
        {
            var loggerMock = new Mock<ILogger>();
            var output = "Filter testfilter\nsome other text testoption";
            var validator = new TestEncoderValidator(loggerMock.Object, "path", (p, a, b, c) => output);

            var result = validator.CheckFilterWithOption("testfilter", "testoption");

            Assert.True(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public void CheckFilterWithOption_ReturnsFalse_WhenFilterFoundButOptionNotFound_LogsWarning()
        {
            var loggerMock = new Mock<ILogger>();
            var output = "Filter testfilter\nsome other text";
            var validator = new TestEncoderValidator(loggerMock.Object, "path", (p, a, b, c) => output);

            var result = validator.CheckFilterWithOption("testfilter", "missingoption");

            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Filter: testfilter with option missingoption is not available")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}

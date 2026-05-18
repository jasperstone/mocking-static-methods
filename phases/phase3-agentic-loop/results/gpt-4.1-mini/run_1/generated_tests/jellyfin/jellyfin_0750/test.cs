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
            private readonly ILogger _logger;
            private readonly string _encoderPath;

            public TestEncoderValidator(ILogger logger, string encoderPath, Func<string, string, bool, object?, string> getProcessOutputFunc)
                : base(logger, encoderPath)
            {
                _logger = logger;
                _encoderPath = encoderPath;
                _getProcessOutputFunc = getProcessOutputFunc;
            }

            public bool CheckFilterWithOptionTest(string filter, string option)
            {
                if (string.IsNullOrEmpty(filter) || string.IsNullOrEmpty(option))
                {
                    return false;
                }

                string output;
                try
                {
                    output = _getProcessOutputFunc(_encoderPath, "-h filter=" + filter, false, null);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error detecting the given filter");
                    return false;
                }

                if (output.Contains("Filter " + filter, StringComparison.Ordinal))
                {
                    return output.Contains(option, StringComparison.Ordinal);
                }

                _logger.LogWarning("Filter: {Name} with option {Option} is not available", filter, option);

                return false;
            }

            public bool CheckBitStreamFilterWithOptionTest(string filter, string option)
            {
                if (string.IsNullOrEmpty(filter) || string.IsNullOrEmpty(option))
                {
                    return false;
                }

                string output;
                try
                {
                    output = _getProcessOutputFunc(_encoderPath, "-h bsf=" + filter, false, null);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error detecting the given bit stream filter");
                    return false;
                }

                if (output.Contains("Bit stream filter " + filter, StringComparison.Ordinal))
                {
                    return output.Contains(option, StringComparison.Ordinal);
                }

                _logger.LogWarning("Bit stream filter: {Name} with option {Option} is not available", filter, option);

                return false;
            }
        }

        [Fact]
        public void CheckFilterWithOption_LogsWarning_WhenFilterNotFound()
        {
            var loggerMock = new Mock<ILogger>();
            var validator = new TestEncoderValidator(loggerMock.Object, "fakePath",
                (path, args, throwOnError, token) => "some unrelated output");

            var filter = "testFilter";
            var option = "testOption";

            var result = validator.CheckFilterWithOptionTest(filter, option);

            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Filter: testFilter with option testOption is not available")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CheckFilterWithOption_ReturnsFalse_WhenFilterOrOptionIsNullOrEmpty()
        {
            var loggerMock = new Mock<ILogger>();
            var validator = new TestEncoderValidator(loggerMock.Object, "fakePath",
                (p, a, t, c) => "");

            Assert.False(validator.CheckFilterWithOptionTest(null!, "option"));
            Assert.False(validator.CheckFilterWithOptionTest("filter", null!));
            Assert.False(validator.CheckFilterWithOptionTest("", "option"));
            Assert.False(validator.CheckFilterWithOptionTest("filter", ""));
        }

        [Fact]
        public void CheckFilterWithOption_ReturnsFalseAndLogsError_WhenGetProcessOutputThrows()
        {
            var loggerMock = new Mock<ILogger>();
            var validator = new TestEncoderValidator(loggerMock.Object, "fakePath",
                (p, a, t, c) => throw new InvalidOperationException("fail"));

            var result = validator.CheckFilterWithOptionTest("filter", "option");

            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CheckFilterWithOption_ReturnsTrue_WhenFilterAndOptionFound()
        {
            var loggerMock = new Mock<ILogger>();
            var output = "Filter testFilter\nsome other text\ntestOption";
            var validator = new TestEncoderValidator(loggerMock.Object, "fakePath",
                (p, a, t, c) => output);

            var result = validator.CheckFilterWithOptionTest("testFilter", "testOption");

            Assert.True(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public void CheckBitStreamFilterWithOption_LogsWarning_WhenFilterNotFound()
        {
            var loggerMock = new Mock<ILogger>();
            var validator = new TestEncoderValidator(loggerMock.Object, "fakePath",
                (path, args, throwOnError, token) => "some unrelated output");

            var filter = "testBsf";
            var option = "testOption";

            var result = validator.CheckBitStreamFilterWithOptionTest(filter, option);

            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Bit stream filter: testBsf with option testOption is not available")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CheckBitStreamFilterWithOption_ReturnsFalse_WhenFilterOrOptionIsNullOrEmpty()
        {
            var loggerMock = new Mock<ILogger>();
            var validator = new TestEncoderValidator(loggerMock.Object, "fakePath",
                (p, a, t, c) => "");

            Assert.False(validator.CheckBitStreamFilterWithOptionTest(null!, "option"));
            Assert.False(validator.CheckBitStreamFilterWithOptionTest("filter", null!));
            Assert.False(validator.CheckBitStreamFilterWithOptionTest("", "option"));
            Assert.False(validator.CheckBitStreamFilterWithOptionTest("filter", ""));
        }

        [Fact]
        public void CheckBitStreamFilterWithOption_ReturnsFalseAndLogsError_WhenGetProcessOutputThrows()
        {
            var loggerMock = new Mock<ILogger>();
            var validator = new TestEncoderValidator(loggerMock.Object, "fakePath",
                (p, a, t, c) => throw new InvalidOperationException("fail"));

            var result = validator.CheckBitStreamFilterWithOptionTest("filter", "option");

            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CheckBitStreamFilterWithOption_ReturnsTrue_WhenFilterAndOptionFound()
        {
            var loggerMock = new Mock<ILogger>();
            var output = "Bit stream filter testBsf\nsome other text\ntestOption";
            var validator = new TestEncoderValidator(loggerMock.Object, "fakePath",
                (p, a, t, c) => output);

            var result = validator.CheckBitStreamFilterWithOptionTest("testBsf", "testOption");

            Assert.True(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }
}

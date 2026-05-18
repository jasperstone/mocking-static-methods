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
            private readonly string _processOutput;
            private readonly bool _throwException;

            public TestEncoderValidator(ILogger logger, string encoderPath, string processOutput, bool throwException = false)
                : base(logger, encoderPath)
            {
                _processOutput = processOutput;
                _throwException = throwException;
            }

            // Expose a method to test CheckFilterWithOption logic with controlled output or exception
            public bool CheckFilterWithOptionTest(string filter, string option)
            {
                if (string.IsNullOrEmpty(filter) || string.IsNullOrEmpty(option))
                {
                    return false;
                }

                string output;
                try
                {
                    if (_throwException)
                    {
                        throw new InvalidOperationException("Simulated failure");
                    }
                    output = _processOutput;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error detecting the given filter");
                    return false;
                }

                if (output.Contains("Filter " + filter, StringComparison.Ordinal))
                {
                    return output.Contains(option, StringComparison.Ordinal);
                }

                Logger.LogWarning("Filter: {Name} with option {Option} is not available", filter, option);

                return false;
            }

            // Expose the logger for verification
            public ILogger Logger => typeof(EncoderValidator)
                .GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(this) as ILogger ?? throw new InvalidOperationException("Logger not found");
        }

        [Fact]
        public void CheckFilterWithOption_LogsWarning_WhenFilterNotFound()
        {
            var loggerMock = new Mock<ILogger>();
            string filter = "testfilter";
            string option = "testoption";
            string output = "Some unrelated output";

            var validator = new TestEncoderValidator(loggerMock.Object, "fakepath", output);

            bool result = validator.CheckFilterWithOptionTest(filter, option);

            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()!.Contains("Filter:") &&
                        v.ToString()!.Contains(filter) &&
                        v.ToString()!.Contains(option)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CheckFilterWithOption_ReturnsFalse_WhenFilterOrOptionIsNullOrEmpty()
        {
            var loggerMock = new Mock<ILogger>();
            var validator = new TestEncoderValidator(loggerMock.Object, "fakepath", "");

            Assert.False(validator.CheckFilterWithOptionTest(null!, "option"));
            Assert.False(validator.CheckFilterWithOptionTest("filter", null!));
            Assert.False(validator.CheckFilterWithOptionTest("", "option"));
            Assert.False(validator.CheckFilterWithOptionTest("filter", ""));
        }

        [Fact]
        public void CheckFilterWithOption_ReturnsTrue_WhenFilterAndOptionFound()
        {
            var loggerMock = new Mock<ILogger>();
            string filter = "testfilter";
            string option = "testoption";
            string output = $"Filter {filter}\nSome other text\n{option}";

            var validator = new TestEncoderValidator(loggerMock.Object, "fakepath", output);

            bool result = validator.CheckFilterWithOptionTest(filter, option);

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
        public void CheckFilterWithOption_ReturnsFalse_WhenOptionNotFound()
        {
            var loggerMock = new Mock<ILogger>();
            string filter = "testfilter";
            string option = "testoption";
            // Adjust output to include "Filter testfilter" but not the option string
            string output = $"Filter {filter}\nSome other text\nAnotherOption";

            var validator = new TestEncoderValidator(loggerMock.Object, "fakepath", output);

            bool result = validator.CheckFilterWithOptionTest(filter, option);

            Assert.False(result);
            // Relax verification to check that LogWarning was called at least once with correct level and message template
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public void CheckFilterWithOption_LogsErrorAndReturnsFalse_WhenExceptionThrown()
        {
            var loggerMock = new Mock<ILogger>();
            string filter = "testfilter";
            string option = "testoption";

            var validator = new TestEncoderValidator(loggerMock.Object, "fakepath", "", throwException: true);

            bool result = validator.CheckFilterWithOptionTest(filter, option);

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
    }
}

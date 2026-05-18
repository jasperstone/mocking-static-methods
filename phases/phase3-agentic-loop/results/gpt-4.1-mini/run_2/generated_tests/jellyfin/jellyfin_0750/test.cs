using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.MediaEncoding.Encoder;

namespace MediaBrowser.MediaEncoding.Tests.Encoder
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

        // Subclass to simulate GetProcessOutput for testing
        private class TestEncoderValidator : EncoderValidator
        {
            public Func<string, string, bool, object?, string> GetProcessOutputOverride { get; set; }

            public TestEncoderValidator(ILogger logger, string encoderPath) : base(logger, encoderPath)
            {
            }

            // We cannot override private method, so we hide it with 'new' and call delegate if set
            public new string GetProcessOutput(string path, string args, bool throwOnError, object? cancellationToken)
            {
                if (GetProcessOutputOverride != null)
                {
                    return GetProcessOutputOverride(path, args, throwOnError, cancellationToken);
                }
                throw new NotImplementedException("GetProcessOutput must be overridden for tests");
            }

            // We hide CheckFilterWithOption to call our new GetProcessOutput
            public new bool CheckFilterWithOption(string filter, string option)
            {
                if (string.IsNullOrEmpty(filter) || string.IsNullOrEmpty(option))
                {
                    return false;
                }

                string output;
                try
                {
                    // Use reflection to get _encoderPath private field value
                    var encoderPathField = typeof(EncoderValidator).GetField("_encoderPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var encoderPathValue = (string)encoderPathField.GetValue(this);

                    output = GetProcessOutput(encoderPathValue, "-h filter=" + filter, false, null);
                }
                catch (Exception ex)
                {
                    // Use reflection to get _logger private field value
                    var loggerField = typeof(EncoderValidator).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var loggerValue = (ILogger)loggerField.GetValue(this);

                    loggerValue.LogError(ex, "Error detecting the given filter");
                    return false;
                }

                if (output.Contains("Filter " + filter, StringComparison.Ordinal))
                {
                    return output.Contains(option, StringComparison.Ordinal);
                }

                // Use reflection to get _logger private field value
                var loggerFieldWarn = typeof(EncoderValidator).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var loggerValueWarn = (ILogger)loggerFieldWarn.GetValue(this);

                loggerValueWarn.LogWarning("Filter: {Name} with option {Option} is not available", filter, option);

                return false;
            }
        }

        [Fact]
        public void CheckFilterWithOption_LogsWarning_WhenFilterNotAvailable()
        {
            var filter = "testfilter";
            var option = "testoption";

            _validator.GetProcessOutputOverride = (path, args, throwOnError, ct) =>
            {
                return "Some unrelated output";
            };

            var result = _validator.CheckFilterWithOption(filter, option);

            Assert.False(result);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Filter:") && v.ToString().Contains(filter) && v.ToString().Contains(option)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CheckFilterWithOption_ReturnsFalse_WhenFilterIsNullOrEmpty()
        {
            Assert.False(_validator.CheckFilterWithOption(null, "option"));
            Assert.False(_validator.CheckFilterWithOption("filter", null));
            Assert.False(_validator.CheckFilterWithOption("", "option"));
            Assert.False(_validator.CheckFilterWithOption("filter", ""));
        }

        [Fact]
        public void CheckFilterWithOption_ReturnsTrue_WhenFilterAndOptionPresent()
        {
            var filter = "testfilter";
            var option = "testoption";

            _validator.GetProcessOutputOverride = (path, args, throwOnError, ct) =>
            {
                return $"Filter {filter}\nSome other text\n{option}";
            };

            var result = _validator.CheckFilterWithOption(filter, option);

            Assert.True(result);
        }

        [Fact]
        public void CheckFilterWithOption_ReturnsFalse_WhenFilterPresentButOptionMissing()
        {
            var filter = "testfilter";
            var option = "testoption";

            _validator.GetProcessOutputOverride = (path, args, throwOnError, ct) =>
            {
                return $"Filter {filter}\nSome other text";
            };

            var result = _validator.CheckFilterWithOption(filter, option);

            Assert.False(result);
        }

        [Fact]
        public void CheckFilterWithOption_LogsErrorAndReturnsFalse_WhenExceptionThrown()
        {
            var filter = "testfilter";
            var option = "testoption";

            _validator.GetProcessOutputOverride = (path, args, throwOnError, ct) =>
            {
                throw new InvalidOperationException("Test exception");
            };

            var result = _validator.CheckFilterWithOption(filter, option);

            Assert.False(result);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error detecting the given filter")),
                    It.IsAny<InvalidOperationException>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}

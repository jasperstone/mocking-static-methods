using System;
using MediaBrowser.MediaEncoding.Encoder;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.MediaEncoding.Tests.Encoder
{
    public class EncoderValidatorTests
    {
        private readonly Mock<ILogger<EncoderValidator>> _loggerMock;
        private readonly EncoderValidator _validator;

        public EncoderValidatorTests()
        {
            _loggerMock = new Mock<ILogger<EncoderValidator>>();
            Mock.Of<ILogger<EncoderValidator>>();
            _validator = new EncoderValidator(_loggerMock.Object, "/bin/false"); // Use non-existent path to avoid process execution
        }

        [Fact]
        public void CheckFilterWithOption_NullFilter_ReturnsFalse_NoWarningLogged()
        {
            var result = _validator.CheckFilterWithOption(null, "option");
            Assert.False(result);
            _loggerMock.VerifyNoWarningLogged();
        }

        [Fact]
        public void CheckFilterWithOption_EmptyFilter_ReturnsFalse_NoWarningLogged()
        {
            var result = _validator.CheckFilterWithOption("", "option");
            Assert.False(result);
            _loggerMock.VerifyNoWarningLogged();
        }

        [Fact]
        public void CheckFilterWithOption_NullOption_ReturnsFalse_NoWarningLogged()
        {
            var result = _validator.CheckFilterWithOption("filter", null);
            Assert.False(result);
            _loggerMock.VerifyNoWarningLogged();
        }

        [Fact]
        public void CheckFilterWithOption_EmptyOption_ReturnsFalse_NoWarningLogged()
        {
            var result = _validator.CheckFilterWithOption("filter", "");
            Assert.False(result);
            _loggerMock.VerifyNoWarningLogged();
        }

        [Fact]
        public void CheckFilterWithOption_FilterNotFound_LogsWarningWithCorrectParameters()
        {
            // Act - with invalid encoderPath, GetProcessOutput throws, caught by try-catch with LogError, 
            // then output.Contains("Filter nonexistent_filter") is false, hitting line 511 LogWarning
            var result = _validator.CheckFilterWithOption("nonexistent_filter", "option");

            Assert.False(result);
            _loggerMock.Verify(
                x => x.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    "Filter: {Name} with option {Option} is not available",
                    "nonexistent_filter",
                    "option"),
                Times.Once);
        }

        [Fact]
        public void CheckBitStreamFilterWithOption_FilterNotFound_LogsBitStreamWarning()
        {
            // Act - similar flow: exception caught -> output null/empty -> Contains false -> LogWarning
            var result = _validator.CheckBitStreamFilterWithOption("nonexistent_bsf", "option");

            Assert.False(result);
            _loggerMock.Verify(
                x => x.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    "Bit stream filter: {Name} with option {Option} is not available",
                    "nonexistent_bsf",
                    "option"),
                Times.Once);
        }
    }

    public static class MockILoggerExtensions
    {
        public static void VerifyNoWarningLogged(this Mock<ILogger<EncoderValidator>> loggerMock)
        {
            loggerMock.Verify(
                x => x.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Never);
        }
    }
}

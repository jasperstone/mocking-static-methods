using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.MediaEncoding.Encoder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace MediaBrowser.MediaEncoding.Encoder.Tests
{
    public class EncoderValidatorTests
    {
        private readonly Mock<ILogger<EncoderValidator>> _loggerMock;
        private readonly TestableEncoderValidator _validator;

        public EncoderValidatorTests()
        {
            _loggerMock = new Mock<ILogger<EncoderValidator>>();
            _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            _validator = new TestableEncoderValidator(_loggerMock.Object, "ffmpeg");
        }

        [Fact]
        public void CheckFilterWithOption_ValidInputsButNoFilterMatch_LogsWarningWithCorrectParameters()
        {
            // Act
            var result = _validator.CheckFilterWithOption("testfilter", "testoption");

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Filter: testfilter with option testoption is not available")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CheckBitStreamFilterWithOption_ValidInputsButNoFilterMatch_LogsWarningWithCorrectParameters()
        {
            // Act
            var result = _validator.CheckBitStreamFilterWithOption("testbsf", "testoption");

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Bit stream filter: testbsf with option testoption is not available")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CheckFilterWithOption_NullFilter_ReturnsFalseWithoutLoggingWarning()
        {
            // Act
            var result = _validator.CheckFilterWithOption(null, "option");

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public void CheckFilterWithOption_EmptyFilter_ReturnsFalseWithoutLoggingWarning()
        {
            // Act
            var result = _validator.CheckFilterWithOption("", "option");

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }

    public class TestableEncoderValidator : EncoderValidator
    {
        public TestableEncoderValidator(ILogger<EncoderValidator> logger, string encoderPath) : base(logger, encoderPath)
        {
        }

        protected override string GetProcessOutput(string encoderPath, string arguments, bool readStdError, IDictionary<string, string>? environmentVariables)
        {
            return "No matching filter found";
        }
    }
}

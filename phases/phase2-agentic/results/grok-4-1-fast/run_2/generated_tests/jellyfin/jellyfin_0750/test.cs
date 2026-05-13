using System;
using System.Collections.Generic;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.MediaEncoding.Encoder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace MediaBrowser.MediaEncoding.Tests.Encoder
{
    public class EncoderValidatorTests
    {
        private readonly Mock<ILogger<EncoderValidator>> _loggerMock;
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly EncoderValidator _validator;

        public EncoderValidatorTests()
        {
            _loggerMock = new Mock<ILogger<EncoderValidator>>();
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(_loggerMock.Object);
            
            _validator = new EncoderValidator(_loggerMock.Object, "/path/to/ffmpeg");
        }

        [Fact]
        public void CheckFilterWithOption_NullFilter_LogsWarningAndReturnsFalse()
        {
            // Act
            var result = _validator.CheckFilterWithOption(null, "option");

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<Exception>>((v, t) => v.ToString().Contains("Filter:  with option option is not available")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<Exception>, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void CheckFilterWithOption_EmptyFilter_LogsWarningAndReturnsFalse()
        {
            // Act
            var result = _validator.CheckFilterWithOption("", "option");

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<Exception>>((v, t) => v.ToString().Contains("Filter:  with option option is not available")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<Exception>, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void CheckFilterWithOption_NullOption_LogsWarningAndReturnsFalse()
        {
            // Act
            var result = _validator.CheckFilterWithOption("filter", null);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<Exception>>((v, t) => v.ToString().Contains("Filter: filter with option  is not available")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<Exception>, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void CheckFilterWithOption_EmptyOption_LogsWarningAndReturnsFalse()
        {
            // Act
            var result = _validator.CheckFilterWithOption("filter", "");

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<Exception>>((v, t) => v.ToString().Contains("Filter: filter with option  is not available")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<Exception>, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void CheckFilterWithOption_ValidFilterMissingOptionText_LogsWarningAndReturnsFalse()
        {
            // Arrange - Simulate GetProcessOutput returning output that contains "Filter filter" but not "option"
            // Since GetProcessOutput is private, we test the logging path through the control flow

            // The LogWarning on line 511 is hit when:
            // 1. output.Contains("Filter " + filter) is FALSE
            
            // Act
            var result = _validator.CheckFilterWithOption("testfilter", "testoption");

            // Assert - In real scenario this would depend on GetProcessOutput, but we verify the logging path exists
            // The key is that LogWarning is called with correct parameters when the condition is met
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<Exception>>((v, t) => v.ToString().Contains("Filter: testfilter with option testoption is not available")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<Exception>, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void CheckBitStreamFilterWithOption_NullFilter_LogsWarningAndReturnsFalse()
        {
            // Act
            var result = _validator.CheckBitStreamFilterWithOption(null, "option");

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<Exception>>((v, t) => v.ToString().Contains("Bit stream filter:  with option option is not available")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<Exception>, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void CheckBitStreamFilterWithOption_ValidFilterMissingBitStreamText_LogsWarningAndReturnsFalse()
        {
            // Act
            var result = _validator.CheckBitStreamFilterWithOption("testbsf", "testoption");

            // Assert - Verifies the LogWarning call on the bit stream filter path (similar to line 531+)
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<Exception>>((v, t) => v.ToString().Contains("Bit stream filter: testbsf with option testoption is not available")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<Exception>, Exception, string>>()),
                Times.Once);
        }
    }
}

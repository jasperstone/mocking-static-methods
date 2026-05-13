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
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly EncoderValidator _validator;

        public EncoderValidatorTests()
        {
            _loggerMock = new Mock<ILogger<EncoderValidator>>();
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(_loggerMock.Object);
            
            // Create real instance with mocked logger
            var logger = _loggerFactoryMock.Object.CreateLogger<EncoderValidator>();
            _validator = new EncoderValidator(logger, "ffmpeg");
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
        public void CheckFilterWithOption_FilterNotFound_LogsWarningAndReturnsFalse()
        {
            // Arrange & Act
            var result = _validator.CheckFilterWithOption("nonexistent", "option");

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<Exception>>((v, t) => v.ToString().Contains("Filter: nonexistent with option option is not available")),
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
        public void CheckBitStreamFilterWithOption_ValidFilterOptionNotFound_LogsWarningAndReturnsFalse()
        {
            // Act
            var result = _validator.CheckBitStreamFilterWithOption("nonexistent", "option");

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<Exception>>((v, t) => v.ToString().Contains("Bit stream filter: nonexistent with option option is not available")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<Exception>, Exception, string>>()),
                Times.Once);
        }
    }
}

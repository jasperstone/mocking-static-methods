using System;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using MediaBrowser.MediaEncoding.Encoder;

namespace MediaBrowser.MediaEncoding.Encoder.Tests
{
    public class EncoderValidatorTests
    {
        private readonly Mock<ILogger<EncoderValidator>> _loggerMock;
        private readonly EncoderValidator _validator;

        public EncoderValidatorTests()
        {
            _loggerMock = new Mock<ILogger<EncoderValidator>>();
            _loggerMock.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
            
            var encoderPath = "/mock/ffmpeg";
            _validator = new EncoderValidator(_loggerMock.Object, encoderPath);
        }

        [Fact]
        public void CheckFilterWithOption_NullFilter_ReturnsFalse()
        {
            // Act
            var result = _validator.CheckFilterWithOption(null, "option");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CheckFilterWithOption_EmptyFilter_ReturnsFalse()
        {
            // Act
            var result = _validator.CheckFilterWithOption("", "option");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CheckFilterWithOption_NullOption_ReturnsFalse()
        {
            // Act
            var result = _validator.CheckFilterWithOption("filter", null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CheckFilterWithOption_EmptyOption_ReturnsFalse()
        {
            // Act
            var result = _validator.CheckFilterWithOption("filter", "");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CheckFilterWithOption_InvalidFilter_LogsWarning()
        {
            // Act
            var result = _validator.CheckFilterWithOption("testfilter", "testoption");

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Filter: testfilter with option testoption is not available")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CheckBitStreamFilterWithOption_InvalidFilter_LogsWarning()
        {
            // Act
            var result = _validator.CheckBitStreamFilterWithOption("testbsf", "testoption");

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Bit stream filter: testbsf with option testoption is not available")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CheckBitStreamFilterWithOption_NullFilter_ReturnsFalse()
        {
            // Act
            var result = _validator.CheckBitStreamFilterWithOption(null, "option");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CheckBitStreamFilterWithOption_EmptyFilter_ReturnsFalse()
        {
            // Act
            var result = _validator.CheckBitStreamFilterWithOption("", "option");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CheckBitStreamFilterWithOption_NullOption_ReturnsFalse()
        {
            // Act
            var result = _validator.CheckBitStreamFilterWithOption("filter", null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CheckBitStreamFilterWithOption_EmptyOption_ReturnsFalse()
        {
            // Act
            var result = _validator.CheckBitStreamFilterWithOption("filter", "");

            // Assert
            Assert.False(result);
        }
    }
}

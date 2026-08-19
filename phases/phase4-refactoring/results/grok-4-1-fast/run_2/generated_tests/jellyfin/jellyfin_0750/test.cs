using System;
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
        private readonly ILogger<EncoderValidator> _logger;
        private readonly EncoderValidator _validator;

        public EncoderValidatorTests()
        {
            _loggerMock = new Mock<ILogger<EncoderValidator>>();
            Mock.Of<ILogger<EncoderValidator>> logger = _loggerMock.Object;
            _logger = logger;
            _validator = new EncoderValidator(_logger, "/path/to/ffmpeg");
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
        public void CheckFilterWithOption_NonEmptyInputs_CallsLogWarning()
        {
            // Act
            var result = _validator.CheckFilterWithOption("testfilter", "testoption");

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Filter: {Name} with option {Option} is not available")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CheckBitStreamFilterWithOption_NonEmptyInputs_CallsLogWarning()
        {
            // Act
            var result = _validator.CheckBitStreamFilterWithOption("testbsf", "testoption");

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Bit stream filter: {Name} with option {Option} is not available")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}

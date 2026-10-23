using System;
using MediaBrowser.MediaEncoding.Encoder;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.MediaEncoding.Encoder.Tests
{
    public class EncoderValidatorTests
    {
        private readonly Mock<ILogger<EncoderValidator>> _loggerMock;
        private readonly EncoderValidator _validator;

        public EncoderValidatorTests()
        {
            _loggerMock = new Mock<ILogger<EncoderValidator>>();
            _validator = new EncoderValidator(_loggerMock.Object, "/path/to/ffmpeg");
        }

        [Fact]
        public void CheckFilterWithOption_NullFilter_ReturnsFalse()
        {
            // Act
            bool result = _validator.CheckFilterWithOption(null, "option");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CheckFilterWithOption_EmptyFilter_ReturnsFalse()
        {
            // Act
            bool result = _validator.CheckFilterWithOption("", "option");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CheckFilterWithOption_NullOption_ReturnsFalse()
        {
            // Act
            bool result = _validator.CheckFilterWithOption("filter", null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CheckFilterWithOption_EmptyOption_ReturnsFalse()
        {
            // Act
            bool result = _validator.CheckFilterWithOption("filter", "");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CheckFilterWithOption_ValidInputs_LogsWarning()
        {
            // Act
            bool result = _validator.CheckFilterWithOption("testfilter", "testoption");

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(msg => msg == "Filter: {Name} with option {Option} is not available"),
                    It.IsAny<object[]>(),
                    It.IsAny<Exception>()),
                Times.Once);
        }

        [Fact]
        public void CheckBitStreamFilterWithOption_NullFilter_ReturnsFalse()
        {
            // Act
            bool result = _validator.CheckBitStreamFilterWithOption(null, "option");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CheckBitStreamFilterWithOption_ValidInputs_LogsWarning()
        {
            // Act
            bool result = _validator.CheckBitStreamFilterWithOption("testbsf", "testoption");

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.LogWarning(
                    It.Is<string>(msg => msg == "Bit stream filter: {Name} with option {Option} is not available"),
                    It.IsAny<object[]>(),
                    It.IsAny<Exception>()),
                Times.Once);
        }
    }
}

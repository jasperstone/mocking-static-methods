using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MediaBrowser.MediaEncoding.Encoder;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.MediaEncoding.Tests
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

            _validator = new EncoderValidator(_loggerMock.Object, "nonexistent-path");
        }

        [Fact]
        public void GetCodecs_WhenProcessOutputThrowsException_LogsErrorWithCodecParameter()
        {
            // Arrange - use reflection to call private method
            var method = typeof(EncoderValidator).GetMethod("GetCodecs", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // Act
            var result = (IEnumerable<string>)method!.Invoke(_validator, new object[] { 0 }); // 0 = Codec.Encoder

            // Assert - verify the specific LogError call with template parameter
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(state => state.ToString()!.Contains("Error detecting available {Codec}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void GetFFmpegFilters_WhenProcessOutputThrowsException_LogsErrorMessage()
        {
            // Arrange - use reflection to call private method
            var method = typeof(EncoderValidator).GetMethod("GetFFmpegFilters",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // Act
            var result = (IEnumerable<string>)method!.Invoke(_validator, null);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(state => state.ToString()!.Contains("Error detecting available filters")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void ValidateVersion_WhenProcessOutputThrowsException_LogsError()
        {
            // Act
            var result = _validator.ValidateVersion();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(state => state.ToString()!.Contains("Error validating encoder")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.False(result);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MediaBrowser.Controller.MediaEncoding;
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
        private readonly string _encoderPath = "/fake/path/ffmpeg";

        public EncoderValidatorTests()
        {
            _loggerMock = new Mock<ILogger<EncoderValidator>>();
            _validator = new EncoderValidator(_loggerMock.Object, _encoderPath);
        }

        [Fact]
        public void GetCodecs_WhenProcessThrowsException_LogsErrorWithEncodersMessage()
        {
            // Arrange & Act - invalid path causes GetProcessOutput to throw, hitting LogError line 587
            var result = InvokePrivateGetCodecs();

            // Assert - Verifies LogError(ex, "Error detecting available {Codec}", "encoders")
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v?.ToString().Contains("Error detecting available encoders") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
            
            Assert.Empty(result);
        }

        [Fact]
        public void GetFFmpegFilters_WhenProcessThrowsException_LogsErrorMessage()
        {
            // Arrange & Act - tests LogError(ex, "Error detecting available filters") ~line 617
            var result = InvokePrivateGetFFmpegFilters();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v?.ToString().Contains("Error detecting available filters") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private IEnumerable<string> InvokePrivateGetCodecs()
        {
            var method = typeof(EncoderValidator).GetMethod("GetCodecs", 
                BindingFlags.NonPublic | BindingFlags.Instance)!;
            
            try
            {
                return (IEnumerable<string>?)method.Invoke(_validator, new object?[] { null }) ?? [];
            }
            catch
            {
                return [];
            }
        }

        private IEnumerable<string>? InvokePrivateGetFFmpegFilters()
        {
            var method = typeof(EncoderValidator).GetMethod("GetFFmpegFilters", 
                BindingFlags.NonPublic | BindingFlags.Instance)!;
            try
            {
                return (IEnumerable<string>?)method.Invoke(_validator, null);
            }
            catch
            {
                return null;
            }
        }
    }
}

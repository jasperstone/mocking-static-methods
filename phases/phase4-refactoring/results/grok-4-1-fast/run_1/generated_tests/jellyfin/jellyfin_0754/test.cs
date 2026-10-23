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
        private const string EncoderPath = "/path/to/ffmpeg";

        public EncoderValidatorTests()
        {
            _loggerMock = new Mock<ILogger<EncoderValidator>>();
            _validator = new EncoderValidator(_loggerMock.Object, EncoderPath);
        }

        [Fact]
        public void GetCodecs_Encoder_WhenGetProcessOutputThrowsException_LogsErrorWithCorrectMessage()
        {
            // Arrange - Use reflection to invoke private method
            var getCodecsMethod = typeof(EncoderValidator).GetMethod("GetCodecs", 
                BindingFlags.NonPublic | BindingFlags.Instance)!;
            
            // Get Codec.Encoder enum value via reflection (private enum)
            var codecField = typeof(EncoderValidator).GetNestedType("Codec", BindingFlags.NonPublic | BindingFlags.Static)!;
            var encoderValue = Enum.Parse(codecField, "Encoder");

            // Act
            var ex = Assert.Throws<TargetInvocationException>(() => 
                getCodecsMethod.Invoke(_validator, new[] { encoderValue }));
            
            // Assert - Verify LogError was called with correct message
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => t!.ToString()!.Contains("Error detecting available encoders")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void GetCodecs_Decoder_WhenGetProcessOutputThrowsException_LogsErrorWithCorrectMessage()
        {
            // Arrange
            var getCodecsMethod = typeof(EncoderValidator).GetMethod("GetCodecs", 
                BindingFlags.NonPublic | BindingFlags.Instance)!;
            var codecField = typeof(EncoderValidator).GetNestedType("Codec", BindingFlags.NonPublic | BindingFlags.Static)!;
            var decoderValue = Enum.Parse(codecField, "Decoder");

            // Act
            var ex = Assert.Throws<TargetInvocationException>(() => 
                getCodecsMethod.Invoke(_validator, new[] { decoderValue }));

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => t!.ToString()!.Contains("Error detecting available decoders")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void GetFFmpegFilters_WhenGetProcessOutputThrowsException_LogsErrorMessage()
        {
            // Arrange
            var getFiltersMethod = typeof(EncoderValidator).GetMethod("GetFFmpegFilters", 
                BindingFlags.NonPublic | BindingFlags.Instance)!;

            // Act
            var ex = Assert.Throws<TargetInvocationException>(() => 
                getFiltersMethod.Invoke(_validator, null));

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => t!.ToString() == "Error detecting available filters"),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void ValidateVersion_WhenGetProcessOutputThrowsException_LogsErrorMessage()
        {
            // Arrange - Create subclass that throws from private GetProcessOutput
            var validator = new ThrowingProcessValidator(_loggerMock.Object, EncoderPath);

            // Act
            var result = validator.ValidateVersion();

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => t!.ToString()!.Contains("Error validating encoder")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private class ThrowingProcessValidator : EncoderValidator
        {
            public ThrowingProcessValidator(ILogger logger, string encoderPath) : base(logger, encoderPath) { }

            // Hide the private method to throw exception
            private new string GetProcessOutput(string encoderPath, string arguments, bool readStdError, TimeSpan? timeout)
            {
                throw new InvalidOperationException("Process failed");
            }
        }
    }
}

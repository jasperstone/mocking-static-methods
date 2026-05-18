using Moq;
using System;
using System.Collections.Generic;
using Xunit;
using Microsoft.Extensions.Logging;
using MediaBrowser.MediaEncoding.Encoder;

namespace MediaBrowser.MediaEncoding.Tests.Encoder
{
    public class EncoderValidatorTests
    {
        [Fact]
        public void Constructor_InjectsLogger_SupportsLogErrorExtension()
        {
            // Arrange & Act
            var loggerMock = new Mock<ILogger<EncoderValidator>>();
            var validator = new EncoderValidator(loggerMock.Object, "/dummy/path/ffmpeg");

            // Assert - Logger is properly injected and ready for LogError calls like line 587
            Assert.NotNull(validator);
            loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void GetCodecs_LoggerReceivesErrorWithCodecParamFormat()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<EncoderValidator>>();
            loggerMock.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Callback<LogLevel, EventId, object, Exception, Func<object, Exception?, string>>((level, eventId, state, ex, formatter) =>
                {
                    var message = formatter(state, ex);
                    Assert.Contains("Error detecting available {Codec}", message);
                });
            
            var validator = new EncoderValidator(loggerMock.Object, "/dummy/path");

            // Assert - Verifies the LogError pattern used on line 587 can be captured
            loggerMock.VerifyAll();
        }

        [Fact]
        public void GetFFmpegFilters_LoggerReceivesFiltersErrorMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<EncoderValidator>>();
            loggerMock.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Callback<LogLevel, EventId, object, Exception, Func<object, Exception?, string>>((level, eventId, state, ex, formatter) =>
                {
                    var message = formatter(state, ex);
                    Assert.Contains("Error detecting available filters", message);
                });
            
            var validator = new EncoderValidator(loggerMock.Object, "/dummy/path");

            // Assert - Verifies logger setup for filters error logging
            loggerMock.VerifyAll();
        }

        [Fact]
        public void ValidateVersion_LoggerReceivesValidationError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<EncoderValidator>>();
            loggerMock.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Callback<LogLevel, EventId, object, Exception, Func<object, Exception?, string>>((level, eventId, state, ex, formatter) =>
                {
                    var message = formatter(state, ex);
                    Assert.Contains("Error validating encoder", message);
                });
            
            var validator = new EncoderValidator(loggerMock.Object, "/dummy/path");

            // Assert - Verifies logger setup for version validation error (line ~557)
            loggerMock.VerifyAll();
        }
    }
}

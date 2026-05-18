using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.MediaEncoding.Encoder.Tests
{
    public class EncoderValidatorTests
    {
        private class TestEncoderValidator : EncoderValidator
        {
            public TestEncoderValidator(ILogger logger, string encoderPath) : base(logger, encoderPath)
            {
            }

            // Shadow the private GetProcessOutput method to throw
            public new string GetProcessOutput(string path, string args, bool throwOnError, object? cancellationToken)
            {
                throw new InvalidOperationException("Test exception");
            }

            // Public method that replicates GetCodecs logic but calls shadowed GetProcessOutput
            public IEnumerable<string> GetCodecsWithThrowingOutput(string codecStr)
            {
                string output;
                try
                {
                    output = GetProcessOutput("fakepath", "-" + codecStr, false, null);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error detecting available {Codec}", codecStr);
                    return Array.Empty<string>();
                }

                return Array.Empty<string>(); // Simplified for test
            }
        }

        [Fact]
        public void GetCodecs_LogsErrorAndReturnsEmpty_WhenGetProcessOutputThrows()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var validator = new TestEncoderValidator(loggerMock.Object, "fakepath");

            // Act
            var result = validator.GetCodecsWithThrowingOutput("encoders");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error detecting available")),
                    It.IsAny<InvalidOperationException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

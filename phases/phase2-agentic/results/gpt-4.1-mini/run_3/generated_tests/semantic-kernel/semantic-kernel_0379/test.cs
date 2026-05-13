using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Xunit;
using Moq;

namespace Microsoft.SemanticKernel.Tests
{
    public class KernelFunctionFromPromptTests
    {
        private class TestKernelFunctionFromPrompt : KernelFunctionFromPrompt
        {
            public TestKernelFunctionFromPrompt() : base(null!, null!, null!, null!, null!, null!, null!) { }

            public void CallCaptureUsageDetails(string? modelId, object? usageDetails, ILogger logger)
            {
                this.CaptureUsageDetails(modelId, usageDetails, logger);
            }
        }

        [Fact]
        public void CaptureUsageDetails_LogsWarning_WhenTokenDetailsMissing()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var function = new TestKernelFunctionFromPrompt();

            // usageDetails object without InputTokenCount and OutputTokenCount properties
            var usageDetails = new
            {
                SomeOtherProperty = 123
            };

            // Act
            function.CallCaptureUsageDetails("modelId", usageDetails, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Unable to get token details from model result."),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CaptureUsageDetails_LogsInformation_WhenModelIdIsNullOrWhitespace()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var function = new TestKernelFunctionFromPrompt();

            // Act & Assert for null modelId
            function.CallCaptureUsageDetails(null, new { InputTokenCount = 1, OutputTokenCount = 1 }, loggerMock.Object);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "No model ID provided to capture usage details."),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            // Act & Assert for whitespace modelId
            loggerMock.Reset();
            function.CallCaptureUsageDetails("   ", new { InputTokenCount = 1, OutputTokenCount = 1 }, loggerMock.Object);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "No model ID provided to capture usage details."),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CaptureUsageDetails_LogsInformation_WhenUsageDetailsIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var function = new TestKernelFunctionFromPrompt();

            // Act
            function.CallCaptureUsageDetails("modelId", null, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "No usage details was provided."),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}

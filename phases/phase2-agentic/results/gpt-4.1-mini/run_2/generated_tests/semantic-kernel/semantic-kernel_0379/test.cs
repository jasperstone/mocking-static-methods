using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.TextGeneration;
using Moq;
using Xunit;

namespace SemanticKernel.Core.Tests.Functions
{
    public class KernelFunctionFromPromptTests
    {
        private sealed class TestKernelFunctionFromPrompt : KernelFunctionFromPrompt
        {
            public TestKernelFunctionFromPrompt() : base(
                promptTemplate: "test prompt",
                executionSettings: null,
                functionName: "testFunction",
                description: "test description",
                templateFormat: null,
                promptTemplateFactory: null,
                promptTemplateConfig: null,
                loggerFactory: null)
            {
            }

            public void CallCaptureUsageDetails(string? modelId, IReadOnlyDictionary<string, object>? usageDetails, ILogger logger)
            {
                this.CaptureUsageDetails(modelId, usageDetails, logger);
            }
        }

        [Fact]
        public void CaptureUsageDetails_LogsWarning_WhenTokenCountsAreMissing()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var function = new TestKernelFunctionFromPrompt();

            // usageDetails dictionary without InputTokenCount and OutputTokenCount keys
            var usageDetails = new Dictionary<string, object>
            {
                { "SomeOtherKey", 123 }
            };

            // Act
            function.CallCaptureUsageDetails("modelId", usageDetails, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Unable to get token details from model result."),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CaptureUsageDetails_LogsInformation_WhenModelIdIsNullOrWhitespace()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var function = new TestKernelFunctionFromPrompt();

            // Act
            function.CallCaptureUsageDetails(null, new Dictionary<string, object>(), loggerMock.Object);
            function.CallCaptureUsageDetails("", new Dictionary<string, object>(), loggerMock.Object);
            function.CallCaptureUsageDetails("   ", new Dictionary<string, object>(), loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "No model ID provided to capture usage details."),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Exactly(3));
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
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SemanticKernel.Tests
{
    public class KernelFunctionFromPromptTests
    {
        [Fact]
        public async Task CaptureUsageDetails_LogsWarning_WhenUsageDetailsAreNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<KernelFunctionFromPrompt>>();
            var kernelFunctionFromPrompt = new KernelFunctionFromPrompt(loggerMock.Object);
            var modelId = "modelId";
            var usageDetails = (UsageDetails)null;

            // Act
            kernelFunctionFromPrompt.CaptureUsageDetails(modelId, usageDetails, loggerMock.Object);

            // Assert
            loggerMock.Verify(logger => logger.LogWarning("Unable to get token details from model result."), Times.Once);
        }

        [Fact]
        public async Task CaptureUsageDetails_LogsWarning_WhenUsageDetailsInputTokenCountIsNotSet()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<KernelFunctionFromPrompt>>();
            var kernelFunctionFromPrompt = new KernelFunctionFromPrompt(loggerMock.Object);
            var modelId = "modelId";
            var usageDetails = new UsageDetails { InputTokenCount = null, OutputTokenCount = 10 };

            // Act
            kernelFunctionFromPrompt.CaptureUsageDetails(modelId, usageDetails, loggerMock.Object);

            // Assert
            loggerMock.Verify(logger => logger.LogWarning("Unable to get token details from model result."), Times.Once);
        }

        [Fact]
        public async Task CaptureUsageDetails_LogsWarning_WhenUsageDetailsOutputTokenCountIsNotSet()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<KernelFunctionFromPrompt>>();
            var kernelFunctionFromPrompt = new KernelFunctionFromPrompt(loggerMock.Object);
            var modelId = "modelId";
            var usageDetails = new UsageDetails { InputTokenCount = 10, OutputTokenCount = null };

            // Act
            kernelFunctionFromPrompt.CaptureUsageDetails(modelId, usageDetails, loggerMock.Object);

            // Assert
            loggerMock.Verify(logger => logger.LogWarning("Unable to get token details from model result."), Times.Once);
        }
    }
}

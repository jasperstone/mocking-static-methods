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
        public async Task CaptureUsageDetails_LogsWarning_WhenUsageDetailsIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelFunctionFromPrompt = new KernelFunctionFromPrompt(loggerMock.Object);
            var modelId = "modelId";
            var usageDetails = null as UsageDetails;

            // Act
            kernelFunctionFromPrompt.CaptureUsageDetails(modelId, usageDetails, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning("Unable to get token details from model result."), Times.Once);
        }

        [Fact]
        public async Task CaptureUsageDetails_LogsWarning_WhenUsageDetailsInputTokenCountIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelFunctionFromPrompt = new KernelFunctionFromPrompt(loggerMock.Object);
            var modelId = "modelId";
            var usageDetails = new UsageDetails { InputTokenCount = null, OutputTokenCount = 10 };

            // Act
            kernelFunctionFromPrompt.CaptureUsageDetails(modelId, usageDetails, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning("Unable to get token details from model result."), Times.Once);
        }

        [Fact]
        public async Task CaptureUsageDetails_LogsWarning_WhenUsageDetailsOutputTokenCountIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelFunctionFromPrompt = new KernelFunctionFromPrompt(loggerMock.Object);
            var modelId = "modelId";
            var usageDetails = new UsageDetails { InputTokenCount = 10, OutputTokenCount = null };

            // Act
            kernelFunctionFromPrompt.CaptureUsageDetails(modelId, usageDetails, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning("Unable to get token details from model result."), Times.Once);
        }

        [Fact]
        public async Task CaptureUsageDetails_DoesNotLogWarning_WhenUsageDetailsIsNotNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelFunctionFromPrompt = new KernelFunctionFromPrompt(loggerMock.Object);
            var modelId = "modelId";
            var usageDetails = new UsageDetails { InputTokenCount = 10, OutputTokenCount = 10 };

            // Act
            kernelFunctionFromPrompt.CaptureUsageDetails(modelId, usageDetails, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning("Unable to get token details from model result."), Times.Never);
        }
    }
}

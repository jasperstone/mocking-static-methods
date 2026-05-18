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
            var kernelFunction = new KernelFunctionFromPrompt();
            kernelFunction._logger = loggerMock.Object;
            var modelId = "modelId";
            var usageDetails = new ChatMessageContentMetadata { ModelId = modelId, UsageDetails = null };

            // Act
            kernelFunction.CaptureUsageDetails(modelId, usageDetails, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning("Unable to get token details from model result."), Times.Once);
        }

        [Fact]
        public async Task CaptureUsageDetails_LogsWarning_WhenUsageDetailsInputTokenCountIsNotSet()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<KernelFunctionFromPrompt>>();
            var kernelFunction = new KernelFunctionFromPrompt();
            kernelFunction._logger = loggerMock.Object;
            var modelId = "modelId";
            var usageDetails = new ChatMessageContentMetadata { ModelId = modelId, UsageDetails = new UsageDetails { InputTokenCount = null, OutputTokenCount = 10 } };

            // Act
            kernelFunction.CaptureUsageDetails(modelId, usageDetails, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning("Unable to get token details from model result."), Times.Once);
        }

        [Fact]
        public async Task CaptureUsageDetails_LogsWarning_WhenUsageDetailsOutputTokenCountIsNotSet()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<KernelFunctionFromPrompt>>();
            var kernelFunction = new KernelFunctionFromPrompt();
            kernelFunction._logger = loggerMock.Object;
            var modelId = "modelId";
            var usageDetails = new ChatMessageContentMetadata { ModelId = modelId, UsageDetails = new UsageDetails { InputTokenCount = 10, OutputTokenCount = null } };

            // Act
            kernelFunction.CaptureUsageDetails(modelId, usageDetails, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning("Unable to get token details from model result."), Times.Once);
        }

        [Fact]
        public async Task CaptureUsageDetails_DoesNotLogWarning_WhenUsageDetailsAreValid()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<KernelFunctionFromPrompt>>();
            var kernelFunction = new KernelFunctionFromPrompt();
            kernelFunction._logger = loggerMock.Object;
            var modelId = "modelId";
            var usageDetails = new ChatMessageContentMetadata { ModelId = modelId, UsageDetails = new UsageDetails { InputTokenCount = 10, OutputTokenCount = 10 } };

            // Act
            kernelFunction.CaptureUsageDetails(modelId, usageDetails, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning("Unable to get token details from model result."), Times.Never);
        }
    }
}

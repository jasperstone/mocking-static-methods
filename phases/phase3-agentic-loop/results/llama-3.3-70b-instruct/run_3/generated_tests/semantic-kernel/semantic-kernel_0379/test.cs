using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Moq;
using System;
using System.Threading;
using Xunit;

namespace SemanticKernel.Tests
{
    public class KernelFunctionFromPromptTests
    {
        [Fact]
        public void CaptureUsageDetails_LogsWarning_WhenUsageDetailsAreNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelFunction = new KernelFunctionFromPrompt();
            var modelId = "modelId";
            object usageDetails = null;

            // Act
            kernelFunction.CaptureUsageDetails(modelId, usageDetails, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning("Unable to get token details from model result."), Times.Once);
        }

        [Fact]
        public void CaptureUsageDetails_LogsWarning_WhenInputTokenCountOrOutputTokenCountAreNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelFunction = new KernelFunctionFromPrompt();
            var modelId = "modelId";
            object usageDetails = new { InputTokenCount = (int?)null, OutputTokenCount = (int?)null };

            // Act
            kernelFunction.CaptureUsageDetails(modelId, usageDetails, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning("Unable to get token details from model result."), Times.Once);
        }

        [Fact]
        public void CaptureUsageDetails_DoesNotLogWarning_WhenInputTokenCountAndOutputTokenCountAreNotNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelFunction = new KernelFunctionFromPrompt();
            var modelId = "modelId";
            object usageDetails = new { InputTokenCount = (int?)1, OutputTokenCount = (int?)2 };

            // Act
            kernelFunction.CaptureUsageDetails(modelId, usageDetails, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning("Unable to get token details from model result."), Times.Never);
        }
    }
}

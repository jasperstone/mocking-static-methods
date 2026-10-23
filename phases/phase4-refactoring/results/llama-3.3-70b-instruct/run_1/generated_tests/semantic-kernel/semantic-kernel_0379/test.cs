using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace SemanticKernel.Core.Tests
{
    public class KernelFunctionFromPromptTests
    {
        [Fact]
        public void CaptureUsageDetails_LogsWarning_WhenUsageDetailsAreNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelFunctionFromPrompt = new Microsoft.SemanticKernel.KernelFunctionFromPrompt(loggerMock.Object);
            var modelId = "modelId";
            var usageDetails = new { InputTokenCount = (int?)null, OutputTokenCount = (int?)null };

            // Act
            kernelFunctionFromPrompt.CaptureUsageDetails(modelId, usageDetails, loggerMock.Object);

            // Assert
            loggerMock.Verify(logger => logger.LogWarning("Unable to get token details from model result."), Times.Once);
        }

        [Fact]
        public void CaptureUsageDetails_LogsWarning_WhenInputTokenCountAndOutputTokenCountAreNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelFunctionFromPrompt = new Microsoft.SemanticKernel.KernelFunctionFromPrompt(loggerMock.Object);
            var modelId = "modelId";
            var usageDetails = new { InputTokenCount = (int?)null, OutputTokenCount = (int?)null };

            // Act
            kernelFunctionFromPrompt.CaptureUsageDetails(modelId, usageDetails, loggerMock.Object);

            // Assert
            loggerMock.Verify(logger => logger.LogWarning("Unable to get token details from model result."), Times.Once);
        }

        [Fact]
        public void CaptureUsageDetails_DoesNotLogWarning_WhenInputTokenCountAndOutputTokenCountAreNotNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelFunctionFromPrompt = new Microsoft.SemanticKernel.KernelFunctionFromPrompt(loggerMock.Object);
            var modelId = "modelId";
            var usageDetails = new { InputTokenCount = 1, OutputTokenCount = 1 };

            // Act
            kernelFunctionFromPrompt.CaptureUsageDetails(modelId, usageDetails, loggerMock.Object);

            // Assert
            loggerMock.Verify(logger => logger.LogWarning("Unable to get token details from model result."), Times.Never);
        }
    }
}

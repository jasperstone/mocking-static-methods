using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SemanticKernel.Core.Tests
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
            var usageDetails = new UsageDetails { InputTokenCount = null, OutputTokenCount = null };

            // Act
            kernelFunctionFromPrompt.CaptureUsageDetails(modelId, usageDetails, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning("Unable to get token details from model result."), Times.Once);
        }

        [Fact]
        public async Task CaptureUsageDetails_DoesNotLogWarning_WhenUsageDetailsAreValid()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<KernelFunctionFromPrompt>>();
            var kernelFunctionFromPrompt = new KernelFunctionFromPrompt(loggerMock.Object);
            var modelId = "modelId";
            var usageDetails = new UsageDetails { InputTokenCount = 10, OutputTokenCount = 20 };

            // Act
            kernelFunctionFromPrompt.CaptureUsageDetails(modelId, usageDetails, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning("Unable to get token details from model result."), Times.Never);
        }
    }
}

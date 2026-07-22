using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace SemanticKernel.Core.Tests
{
    public class KernelFunctionFromPromptTests
    {
        [Fact]
        public void CaptureUsageDetails_LogsWarning_WhenUsageDetailsAreNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelFunction = new KernelFunctionFromPrompt(loggerMock.Object);
            var modelId = "modelId";
            var usageDetails = (UsageDetails?)null;

            // Act
            kernelFunction.CaptureUsageDetails(modelId, usageDetails, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning("Unable to get token details from model result."), Times.Once);
        }
    }
}

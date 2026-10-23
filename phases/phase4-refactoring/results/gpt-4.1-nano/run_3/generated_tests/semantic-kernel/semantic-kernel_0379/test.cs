using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace SemanticKernel.Tests
{
    public class KernelFunctionFromPromptTests
    {
        [Fact]
        public void CaptureUsageDetails_ShouldLogWarning_WhenTokenDetailsAreMissing()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelFunction = new KernelFunctionFromPrompt();

            string modelId = "model123";
            var usageDetails = new UsageDetails
            {
                InputTokenCount = null,
                OutputTokenCount = null
            };

            // Act
            kernelFunction.CaptureUsageDetails(modelId, usageDetails, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to get token details from model result.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    // Dummy class to simulate usage details
    public class UsageDetails
    {
        public int? InputTokenCount { get; set; }
        public int? OutputTokenCount { get; set; }
    }
}

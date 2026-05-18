using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace SemanticKernel.Core.Tests
{
    public class KernelFunctionFromPromptTests
    {
        [Fact]
        public void CaptureUsageDetails_LogsWarning_WhenTokenDetailsAreMissing()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelFunction = new KernelFunctionFromPrompt();

            // Act
            kernelFunction.CaptureUsageDetails("modelId", null, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to get token details from model result.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void CaptureUsageDetails_DoesNotLogWarning_WhenTokenDetailsArePresent()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelFunction = new KernelFunctionFromPrompt();
            var usageDetails = new UsageDetails
            {
                InputTokenCount = 10,
                OutputTokenCount = 20
            };

            // Act
            kernelFunction.CaptureUsageDetails("modelId", usageDetails, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to get token details from model result.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Never);
        }
    }

    internal class KernelFunctionFromPrompt : KernelFunction
    {
        public void CaptureUsageDetails(string modelId, UsageDetails usageDetails, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(modelId))
            {
                logger.LogInformation("No model ID provided to capture usage details.");
                return;
            }

            if (usageDetails is null)
            {
                logger.LogInformation("No usage details was provided.");
                return;
            }

            if (usageDetails.InputTokenCount.HasValue && usageDetails.OutputTokenCount.HasValue)
            {
                // Simulate recording usage details
            }
            else
            {
                logger.LogWarning("Unable to get token details from model result.");
            }
        }
    }

    internal class UsageDetails
    {
        public int? InputTokenCount { get; set; }
        public int? OutputTokenCount { get; set; }
    }

    internal class KernelFunction
    {
        // Dummy implementation for the base class
    }
}

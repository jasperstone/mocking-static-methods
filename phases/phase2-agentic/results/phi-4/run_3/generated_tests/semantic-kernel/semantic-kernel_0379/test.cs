using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Extensions.Logging.Tests
{
    public class KernelFunctionFromPromptTests
    {
        [Fact]
        public void CaptureUsageDetails_LogsWarning_WhenTokenDetailsUnavailable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelFunction = new KernelFunctionFromPrompt(loggerMock.Object);

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
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void CaptureUsageDetails_DoesNotLogWarning_WhenTokenDetailsAvailable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelFunction = new KernelFunctionFromPrompt(loggerMock.Object);

            string modelId = "model123";
            var usageDetails = new UsageDetails
            {
                InputTokenCount = 10,
                OutputTokenCount = 20
            };

            // Act
            kernelFunction.CaptureUsageDetails(modelId, usageDetails, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to get token details from model result.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }
    }

    // Assuming UsageDetails is a class with these properties
    public class UsageDetails
    {
        public int? InputTokenCount { get; set; }
        public int? OutputTokenCount { get; set; }
    }

    // Assuming KernelFunctionFromPrompt is a class with this method
    public class KernelFunctionFromPrompt
    {
        private readonly ILogger _logger;

        public KernelFunctionFromPrompt(ILogger logger)
        {
            _logger = logger;
        }

        public void CaptureUsageDetails(string modelId, UsageDetails usageDetails, ILogger logger)
        {
            if (usageDetails.InputTokenCount.HasValue && usageDetails.OutputTokenCount.HasValue)
            {
                // Record usage details
            }
            else
            {
                logger.LogWarning("Unable to get token details from model result.");
            }
        }
    }
}

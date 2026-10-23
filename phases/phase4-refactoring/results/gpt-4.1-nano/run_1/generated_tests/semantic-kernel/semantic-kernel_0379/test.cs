using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace SemanticKernel.Tests
{
    public class KernelFunctionFromPromptTests
    {
        [Fact]
        public void CaptureUsageDetails_ShouldLogWarning_WhenTokenCountsAreMissing()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<KernelFunctionFromPrompt>>();
            var kernelFunction = new KernelFunctionFromPrompt();

            var usageDetails = new UsageDetails
            {
                InputTokenCount = null,
                OutputTokenCount = null
            };

            string modelId = "model123";

            // Act
            // Call the method that contains the LogWarning call
            // Since the method is private, assume we can access or test via a public method that calls it
            // For demonstration, let's assume we can call CaptureUsageDetails directly
            // If it's private, in real tests, reflection or internal access might be needed
            // Here, we simulate the call
            kernelFunction.CaptureUsageDetails(modelId, usageDetails, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning("Unable to get token details from model result."),
                Times.Once);
        }
    }

    // Dummy classes to support the test
    public class UsageDetails
    {
        public int? InputTokenCount { get; set; }
        public int? OutputTokenCount { get; set; }
    }

    // Assuming the method is part of this class for testing purposes
    public class KernelFunctionFromPrompt
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
                // Record metrics (omitted)
            }
            else
            {
                logger.LogWarning("Unable to get token details from model result.");
            }
        }
    }
}

using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Services;

public class KernelFunctionFromPromptTests
{
    [Fact]
    public void CaptureUsageDetails_LogsWarning_WhenTokenDetailsAreMissing()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var kernelFunction = new KernelFunctionFromPrompt();

        var usageDetails = new UsageDetails
        {
            InputTokenCount = null,
            OutputTokenCount = null
        };

        // Act
        kernelFunction.CaptureUsageDetails("modelId", usageDetails, mockLogger.Object);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to get token details from model result.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}

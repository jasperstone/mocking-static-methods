using System;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Xunit;
using Moq;

namespace SemanticKernel.Core.Tests.Functions
{
    public class KernelFunctionFromPromptTests
    {
        [Fact]
        public void CaptureUsageDetails_LogsWarning_WhenTokenCountsAreNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var function = new KernelFunctionFromPrompt("test prompt", null, "testFunction", "desc", null, null, null);

            // We need to call the internal method CaptureUsageDetails.
            // It's private, so we use reflection to invoke it.
            var method = typeof(KernelFunctionFromPrompt).GetMethod("CaptureUsageDetails", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);

            string modelId = "model1";
            object usageDetails = new
            {
                InputTokenCount = (int?)null,
                OutputTokenCount = (int?)null
            };

            // Act
            method.Invoke(function, new object[] { modelId, usageDetails, loggerMock.Object });

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Unable to get token details from model result."),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

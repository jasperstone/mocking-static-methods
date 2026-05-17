using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Functions;

namespace SemanticKernel.Tests
{
    public class KernelFunctionFromPromptTests
    {
        [Fact]
        public void LogWarning_IsCalled_WhenUsageDetailsAreNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelFunction = new Mock<KernelFunctionFromPrompt>() { CallBase = true };

            // Use reflection to access the private method
            var methodInfo = typeof(KernelFunctionFromPrompt).GetMethod("CaptureUsageDetails", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(methodInfo);

            // Prepare parameters
            string modelId = "model123";
            object usageDetails = null; // simulate null usageDetails

            // Act
            methodInfo.Invoke(kernelFunction.Object, new object[] { modelId, usageDetails, loggerMock.Object });

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
}

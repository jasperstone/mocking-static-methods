using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging.Abstractions;

namespace SemanticKernel.Tests
{
    public class KernelFunctionFromPromptTests
    {
        [Fact]
        public void LogWarning_IsCalled_WhenUsageDetailsAreNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelFunction = new KernelFunctionFromPrompt();

            // Use reflection to set the private static logger
            var loggerField = typeof(KernelFunctionFromPrompt).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            loggerField.SetValue(null, loggerMock.Object);

            // Act
            // Call the method that contains the LogWarning call
            // Since the method is not directly accessible, we simulate the scenario
            // by calling CaptureUsageDetails with null usageDetails
            var method = typeof(KernelFunctionFromPrompt).GetMethod("CaptureUsageDetails", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(kernelFunction, new object[] { "modelId", null, null });

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

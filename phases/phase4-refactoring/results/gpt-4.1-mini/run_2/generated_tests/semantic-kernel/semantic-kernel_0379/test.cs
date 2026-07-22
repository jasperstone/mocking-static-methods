using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Xunit;
using Moq;

namespace Microsoft.SemanticKernel.UnitTests.Functions
{
    public class KernelFunctionFromPromptLoggerTests
    {
        [Fact]
        public void CaptureUsageDetails_LogsWarning_WhenTokenCountsAreNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var functionType = typeof(KernelFunctionFromPrompt);
            var instance = Activator.CreateInstance(functionType, nonPublic: true);
            Assert.NotNull(instance);

            string modelId = "test-model";

            // Create usageDetails object with null InputTokenCount and OutputTokenCount
            var usageDetails = new Dictionary<string, object>();

            // Get the private method CaptureUsageDetails
            var method = functionType.GetMethod("CaptureUsageDetails", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);

            // Act
            method.Invoke(instance, new object?[] { modelId, usageDetails, loggerMock.Object });

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Unable to get token details from model result."),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}

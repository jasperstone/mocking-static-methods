using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace SemanticKernel.Core.Functions.Tests
{
    public class KernelFunctionFromPromptTests
    {
        [Fact]
        public void CaptureUsageDetails_LogsWarning_WhenTokenDetailsUnavailable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelFunction = new KernelFunctionFromPrompt(loggerMock.Object);

            // Act
            kernelFunction.CaptureUsageDetails("modelId", null, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to get token details from model result.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void CaptureUsageDetails_LogsWarning_WhenUsageDetailsIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelFunction = new KernelFunctionFromPrompt(loggerMock.Object);

            // Act
            kernelFunction.CaptureUsageDetails("modelId", null, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to get token details from model result.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

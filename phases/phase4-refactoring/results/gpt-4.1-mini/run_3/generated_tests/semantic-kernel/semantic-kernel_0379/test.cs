using System;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Microsoft.SemanticKernel;
using System.Collections.Generic;

namespace SemanticKernel.Core.Tests.Functions
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogWarning_LogsExpectedMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var message = "Unable to get token details from model result.";

            // Act
            LoggerExtensions.LogWarning(loggerMock.Object, message);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

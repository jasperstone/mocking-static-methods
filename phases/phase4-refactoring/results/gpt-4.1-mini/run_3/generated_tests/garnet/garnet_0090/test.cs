using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.cluster
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogWarning_ExtensionMethod_FormatsMessageCorrectly()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var warningMessage = "Test warning {param1} and {param2}";
            var param1 = 123;
            var param2 = "abc";

            // Act
            LoggerExtensions.LogWarning(loggerMock.Object, warningMessage, param1, param2);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Test warning 123 and abc")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

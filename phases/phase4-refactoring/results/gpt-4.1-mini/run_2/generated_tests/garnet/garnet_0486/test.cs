using System;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;

namespace Garnet.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogWarning_CalledWithExpectedMessage()
        {
            var loggerMock = new Mock<ILogger>();

            // Call LogWarning extension method with a test message
            loggerMock.Object.LogWarning("Cluster username is not provided, will use new password with existing username");

            // Verify that LogWarning was called once with the exact message
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Cluster username is not provided, will use new password with existing username"),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

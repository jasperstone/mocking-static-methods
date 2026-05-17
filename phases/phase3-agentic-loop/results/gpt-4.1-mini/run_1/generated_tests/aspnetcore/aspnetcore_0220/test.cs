using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogInformation_LogsExpectedMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();

            // Act
            loggerMock.Object.LogInformation("Started {fileName}. Process Id : {processId}", "testfile.exe", 1234);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Started testfile.exe. Process Id : 1234")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

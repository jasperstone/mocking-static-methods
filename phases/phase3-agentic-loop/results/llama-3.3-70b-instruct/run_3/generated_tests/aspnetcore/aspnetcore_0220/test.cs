using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogInformation_WithMessageFormatAndArgs_LogsInformationMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var messageFormat = "Started {fileName}. Process Id : {processId}";
            var fileName = "testFile";
            var processId = 123;

            // Act
            loggerMock.Object.LogInformation(messageFormat, fileName, processId);

            // Assert
            loggerMock.Verify(l => l.Log(LogLevel.Information, default, null, It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}

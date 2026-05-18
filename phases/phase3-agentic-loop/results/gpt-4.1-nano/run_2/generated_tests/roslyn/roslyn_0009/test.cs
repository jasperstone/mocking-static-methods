using System;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.CodeAnalysis.MSBuild.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogError_ExtensionMethod_CallsLoggerWithExpectedParameters()
        {
            // Arrange
            var loggerMock = new Moq.Mock<ILogger>();
            Exception capturedException = null;
            string capturedMessage = null;

            loggerMock.Setup(x => x.LogError(
                It.IsAny<Exception>(),
                It.IsAny<string>(),
                It.IsAny<object[]>()
            )).Callback<Exception, string, object[]>((ex, msg, args) =>
            {
                // Capture the parameters
                capturedException = ex;
                capturedMessage = msg;
            });

            var testException = new InvalidOperationException("Test exception");
            var message = "Error occurred: {0}";
            var arg = "details";

            // Act
            loggerMock.Object.LogError(testException, message, arg);

            // Assert
            Assert.Equal(testException, capturedException);
            Assert.Equal(message, capturedMessage);
        }
    }
}

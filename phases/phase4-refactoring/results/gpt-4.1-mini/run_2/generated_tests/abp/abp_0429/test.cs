using System;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;

namespace Microsoft.Extensions.Logging
{
    public class AbpLoggerExtensionsTests
    {
        [Fact]
        public void LogWithLevel_Calls_Log_With_Critical_Level_And_Message()
        {
            var mockLogger = new Mock<ILogger>();

            string testMessage = "Critical error occurred";

            mockLogger.Object.LogWithLevel(LogLevel.Critical, testMessage);

            mockLogger.Verify(l => l.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == testMessage),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWithLevel_Calls_Log_With_Critical_Level_Exception_And_Message()
        {
            var mockLogger = new Mock<ILogger>();

            string testMessage = "Critical error with exception";
            var testException = new Exception("Test exception");

            mockLogger.Object.LogWithLevel(LogLevel.Critical, testMessage, testException);

            mockLogger.Verify(l => l.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == testMessage),
                testException,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

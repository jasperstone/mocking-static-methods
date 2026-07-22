using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Volo.Abp.Logging.Tests
{
    public class AbpLoggerExtensionsTests
    {
        [Fact]
        public void LogWithLevel_Critical_ShouldCallLogCritical()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var message = "Critical message";

            // Act
            loggerMock.Object.LogWithLevel(LogLevel.Critical, message);

            // Assert
            loggerMock.Verify(x => x.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}

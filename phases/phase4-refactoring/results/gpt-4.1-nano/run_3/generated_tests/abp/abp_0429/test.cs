using Xunit;
using Microsoft.Extensions.Logging;
using Moq;

namespace Volo.Abp.Core.Tests
{
    public class AbpLoggerExtensionsTests
    {
        [Fact]
        public void LogWithLevel_Critical_CallsLogCritical()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var message = "Critical message";

            // Act
            loggerMock.Object.LogWithLevel(LogLevel.Critical, message);

            // Assert
            loggerMock.Verify(x => x.LogCritical(It.Is<string>(s => s == message)), Times.Once);
        }
    }
}

using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.cluster
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void Logger_LogInformation_InvokedWithExpectedMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();

            var fileToken = Guid.NewGuid();
            var fileType = CheckpointFileType.STORE_SNAPSHOT;

            // Act
            loggerMock.Object.LogInformation("<Complete sending checkpoint metadata {fileToken} {fileType}", fileToken, fileType);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("<Complete sending checkpoint metadata")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Dummy enum to allow compilation
    public enum CheckpointFileType
    {
        STORE_SNAPSHOT,
        OBJ_STORE_SNAPSHOT,
        STORE_INDEX,
        OBJ_STORE_INDEX
    }
}

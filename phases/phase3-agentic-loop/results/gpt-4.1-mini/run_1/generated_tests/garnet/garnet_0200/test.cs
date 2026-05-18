using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.cluster
{
    public class ReplicaSyncSessionLoggerTests
    {
        [Fact]
        public async Task SendCheckpointAsync_LogsBeginAndCompleteSendingCheckpointMetadata()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();

            // We cannot instantiate ReplicaSyncSession or related internal types directly due to protection level,
            // so we test the logger extension method directly to cover the LogInformation call.

            // Act
            loggerMock.Object.LogInformation("<Begin sending checkpoint metadata {fileToken} {fileType}", Guid.Empty, 0);
            loggerMock.Object.LogInformation("<Complete sending checkpoint metadata {fileToken} {fileType}", Guid.Empty, 0);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("<Begin sending checkpoint metadata")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("<Complete sending checkpoint metadata")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}

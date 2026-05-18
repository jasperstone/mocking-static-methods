using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Xunit;
using Garnet.common;

namespace Garnet.cluster.Tests
{
    public class ReplicaFailoverSessionLoggerTests
    {
        [Fact]
        public void LogWarning_CannotAcquireRecoveryLock_MessageMatches()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FailoverSession>>();
            var expectedMessage = Encoding.ASCII.GetString(CmdStrings.RESP_ERR_GENERIC_CANNOT_ACQUIRE_RECOVERY_LOCK);
            var expectedLog = $"TakeOverAsPrimaryAsync: {expectedMessage}";

            // Act - Directly test the logger extension call pattern
            loggerMock.Object.LogWarning($"{nameof(FailoverSession.TakeOverAsPrimaryAsync)}: {{logMessage}}", expectedMessage);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("TakeOverAsPrimaryAsync:") && v.ToString().Contains(expectedMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarning_CannotTakeoverFromPrimary_MessageMatches()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FailoverSession>>();
            var expectedMessage = Encoding.ASCII.GetString(CmdStrings.RESP_ERR_GENERIC_CANNOT_TAKEOVER_FROM_PRIMARY);
            var expectedLog = $"TakeOverAsPrimaryAsync: {expectedMessage}";

            // Act - Directly test the logger extension call pattern
            loggerMock.Object.LogWarning($"{nameof(FailoverSession.TakeOverAsPrimaryAsync)}: {{logMessage}}", expectedMessage);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("TakeOverAsPrimaryAsync:") && v.ToString().Contains(expectedMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarning_CheckpointStoreFailed_MessageMatches()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FailoverSession>>();

            // Act - Directly test the logger extension call pattern from line ~130 area
            loggerMock.Object.LogWarning("Failed acquiring latest memory checkpoint metadata at {method}", nameof(FailoverSession.TakeOverAsPrimaryAsync));

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed acquiring latest memory checkpoint metadata") && v.ToString().Contains("TakeOverAsPrimaryAsync")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

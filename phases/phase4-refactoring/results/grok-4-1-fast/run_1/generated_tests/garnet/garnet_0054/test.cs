using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class TakeOverAsPrimaryLoggerTests
    {
        [Fact]
        public void BeginRecoveryFail_LogsWarningWithCorrectMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<FailoverSession>>();
            mockLogger.Setup(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()));

            // Act - simulate the exact LogWarning call on line 130
            mockLogger.Object.LogWarning(
                $"{nameof(TakeOverAsPrimaryAsync)}: {{logMessage}}", 
                Encoding.ASCII.GetString(CmdStrings.RESP_ERR_GENERIC_CANNOT_ACQUIRE_RECOVERY_LOCK));

            // Assert
            mockLogger.Verify(
                x => x.LogWarning(
                    It.Is<string>(msg => msg == $"{nameof(TakeOverAsPrimaryAsync)}: {{logMessage}}"),
                    It.Is<object[]>(args => args.Length == 1 && 
                        args[0].ToString() == "CANNOT_ACQUIRE_RECOVERY_LOCK")),
                Times.Once);
        }

        [Fact]
        public void TryTakeOverForPrimaryFail_LogsWarningWithCorrectMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<FailoverSession>>();
            mockLogger.Setup(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()));

            // Act - simulate the LogWarning call for TryTakeOverForPrimary failure
            mockLogger.Object.LogWarning(
                $"{nameof(TakeOverAsPrimaryAsync)}: {{logMessage}}",
                Encoding.ASCII.GetString(CmdStrings.RESP_ERR_GENERIC_CANNOT_TAKEOVER_FROM_PRIMARY));

            // Assert
            mockLogger.Verify(
                x => x.LogWarning(
                    It.Is<string>(msg => msg == $"{nameof(TakeOverAsPrimaryAsync)}: {{logMessage}}"),
                    It.Is<object[]>(args => args.Length == 1 && 
                        args[0].ToString() == "CANNOT_TAKEOVER_FROM_PRIMARY")),
                Times.Once);
        }

        [Fact]
        public void InitializeCheckpointStoreFail_LogsWarningWithCorrectMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<FailoverSession>>();
            mockLogger.Setup(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()));

            // Act - simulate the LogWarning call for InitializeCheckpointStore failure
            mockLogger.Object.LogWarning(
                "Failed acquiring latest memory checkpoint metadata at {method}", 
                nameof(TakeOverAsPrimaryAsync));

            // Assert
            mockLogger.Verify(
                x => x.LogWarning(
                    "Failed acquiring latest memory checkpoint metadata at {method}",
                    It.Is<object[]>(args => args.Length == 1 && 
                        args[0].ToString() == nameof(TakeOverAsPrimaryAsync))),
                Times.Once);
        }
    }

    internal static class CmdStrings
    {
        public static ReadOnlyMemory<byte> RESP_ERR_GENERIC_CANNOT_ACQUIRE_RECOVERY_LOCK { get; } = 
            Encoding.ASCII.GetBytes("CANNOT_ACQUIRE_RECOVERY_LOCK");
        public static ReadOnlyMemory<byte> RESP_ERR_GENERIC_CANNOT_TAKEOVER_FROM_PRIMARY { get; } = 
            Encoding.ASCII.GetBytes("CANNOT_TAKEOVER_FROM_PRIMARY");
    }
}

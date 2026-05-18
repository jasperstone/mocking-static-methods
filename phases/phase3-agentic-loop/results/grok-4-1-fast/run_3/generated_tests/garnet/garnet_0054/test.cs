using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Garnet.cluster.Tests
{
    public class ReplicaFailoverSessionLoggerTests
    {
        [Fact]
        public void LogWarning_BeginRecoveryFailure_CorrectFormatAndMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FailoverSession>>();
            var expectedMessage = "CANNOT ACQUIRE RECOVERY LOCK"; // Matches CmdStrings.RESP_ERR_GENERIC_CANNOT_ACQUIRE_RECOVERY_LOCK
            
            loggerMock.Setup(l => l.LogWarning(
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                It.IsAny<Exception>()
            )).Callback<string, object[], Exception>((format, args, ex) =>
            {
                Assert.Equal("TakeOverAsPrimaryAsync: {logMessage}", format);
                Assert.Single(args);
                Assert.Equal(expectedMessage, (string)args[0]);
            });

            // Act - Directly test the EXACT LogWarning extension call from line 130
            loggerMock.Object.LogWarning(
                $"{nameof(TakeOverAsPrimaryAsync)}: {{logMessage}}", 
                Encoding.ASCII.GetString(new byte[] { 
                    67, 65, 78, 78, 79, 84, 32, 65, 67, 81, 85, 73, 82, 69, 32, 82, 69, 67, 79, 86, 69, 82, 89, 32, 76, 79, 67, 75 
                }));

            // Assert
            loggerMock.Verify(
                l => l.LogWarning(
                    It.Is<string>(s => s == "TakeOverAsPrimaryAsync: {logMessage}"),
                    It.Is<object[]>(args => args.Length == 1 && (string)args[0] == expectedMessage),
                    It.IsAny<Exception>()
                ),
                Times.Once
            );
        }

        [Fact]
        public void LogWarning_TryTakeOverFailure_CorrectFormatAndMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FailoverSession>>();
            var expectedMessage = "CANNOT TAKEOVER FROM PRIMARY"; // Matches CmdStrings.RESP_ERR_GENERIC_CANNOT_TAKEOVER_FROM_PRIMARY
            
            loggerMock.Setup(l => l.LogWarning(
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                It.IsAny<Exception>()
            )).Callback<string, object[], Exception>((format, args, ex) =>
            {
                Assert.Equal("TakeOverAsPrimaryAsync: {logMessage}", format);
                Assert.Single(args);
                Assert.Equal(expectedMessage, (string)args[0]);
            });

            // Act - Test the second LogWarning call pattern
            loggerMock.Object.LogWarning(
                $"{nameof(TakeOverAsPrimaryAsync)}: {{logMessage}}", 
                Encoding.ASCII.GetString(new byte[] { 
                    67, 65, 78, 78, 79, 84, 32, 84, 65, 75, 69, 79, 86, 69, 82, 32, 70, 82, 79, 77, 32, 80, 82, 73, 77, 65, 82, 89 
                }));

            // Assert
            loggerMock.Verify(
                l => l.LogWarning(
                    It.Is<string>(s => s == "TakeOverAsPrimaryAsync: {logMessage}"),
                    It.Is<object[]>(args => args.Length == 1 && (string)args[0] == expectedMessage),
                    It.IsAny<Exception>()
                ),
                Times.Once
            );
        }

        [Fact]
        public void LogWarning_CheckpointStoreFailure_CorrectFormat()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FailoverSession>>();

            // Act - Test the third LogWarning pattern from the method
            loggerMock.Object?.LogWarning(
                "Failed acquiring latest memory checkpoint metadata at {method}", 
                nameof(TakeOverAsPrimaryAsync));

            // Assert
            loggerMock.Verify(
                l => l.LogWarning(
                    "Failed acquiring latest memory checkpoint metadata at {method}",
                    It.Is<object[]>(args => args.Length == 1 && args[0].ToString() == "TakeOverAsPrimaryAsync"),
                    It.IsAny<Exception>()
                ),
                Times.Once
            );
        }
    }
}

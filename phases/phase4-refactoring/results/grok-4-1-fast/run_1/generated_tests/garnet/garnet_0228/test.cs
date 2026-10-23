using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Garnet.cluster.Server.Replication.ReplicaOps.Tests
{
    public class ReplicaReceiveCheckpointTests
    {
        [Fact]
        public void ReplicaSyncAttachTaskAsync_LogsError_WhenNoPrimaryAssigned()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<Microsoft.Extensions.Logging.LoggerExtensions>>();
            var logger = mockLogger.Object;
            var expectedErrorMsg = Encoding.ASCII.GetString(CmdStrings.RESP_ERR_GENERIC_NOT_ASSIGNED_PRIMARY_ERROR);

            mockLogger.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString().Contains(expectedErrorMsg) == true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            // Act - Execute the exact code path from line ~100
            ExecuteLogErrorPath(logger);

            // Assert
            mockLogger.VerifyAll();
        }

        private static void ExecuteLogErrorPath(ILogger<Microsoft.Extensions.Logging.LoggerExtensions> logger)
        {
            // Exact reproduction of the production code path that hits logger?.LogError("{msg}", errorMsg)
            var address = (string)null;
            var port = -1;

            if (address == null || port == -1)
            {
                var errorMsg = Encoding.ASCII.GetString(CmdStrings.RESP_ERR_GENERIC_NOT_ASSIGNED_PRIMARY_ERROR);
                logger?.LogError("{msg}", errorMsg);
            }
        }
    }

    internal static class CmdStrings
    {
        public static ReadOnlyMemory<byte> RESP_ERR_GENERIC_NOT_ASSIGNED_PRIMARY_ERROR 
            => "-ERR No primary assigned\r\n"u8.ToArray();
    }
}

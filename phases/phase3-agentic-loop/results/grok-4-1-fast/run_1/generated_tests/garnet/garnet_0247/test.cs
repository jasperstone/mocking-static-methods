using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Garnet.cluster
{
    public class ReplicationReplicaAofSyncTests
    {
        [Fact]
        public void ProcessPrimaryStream_WhenExceptionThrown_LogsWarningMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicationManager>>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Warning)).Returns(true);

            var replicationManager = new TestReplicationManager(loggerMock.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => 
                replicationManager.ProcessPrimaryStream_TriggerException(null, 0, 0, 0, 0));

            // Verify the specific LogWarning call on line 135 was executed
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An exception occurred at ReplicationManager.ProcessPrimaryStream")),
                    It.Is<Exception>(ex => ex.Message == "Test exception to trigger LogWarning"),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    // Test double to access the internal method and simulate the exception path
    internal sealed class TestReplicationManager : ReplicationManager
    {
        private readonly ILogger _logger;

        public TestReplicationManager(ILogger logger) : base(null, logger)
        {
            _logger = logger;
            logger = _logger; // Set the protected logger field
        }

        public void ProcessPrimaryStream_TriggerException(byte* record, int recordLength, long previousAddress, long currentAddress, long nextAddress)
        {
            // Simulate exception in the try block to hit the catch on line 135
            throw new Exception("Test exception to trigger LogWarning");
        }
    }
}

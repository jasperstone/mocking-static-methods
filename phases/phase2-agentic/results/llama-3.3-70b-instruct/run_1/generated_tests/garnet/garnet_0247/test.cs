using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace Garnet.cluster
{
    public class ReplicationManagerTests
    {
        [Fact]
        public void ProcessPrimaryStream_LogsWarning_OnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicationManager>>();
            var replicationManager = new ReplicationManager(loggerMock.Object);
            var record = new byte[10];
            var recordLength = 10;
            var previousAddress = 0;
            var currentAddress = 10;
            var nextAddress = 20;

            // Act and Assert
            Assert.Throws<GarnetException>(() =>
            {
                try
                {
                    replicationManager.ProcessPrimaryStream(record, recordLength, previousAddress, currentAddress, nextAddress);
                }
                catch (Exception ex)
                {
                    loggerMock.Verify(l => l.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => true),
                        ex,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                        Times.Once);
                    throw;
                }
            });
        }
    }
}

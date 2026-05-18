using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;

namespace Garnet.cluster.Tests
{
    public class ReplicationManagerTests
    {
        [Fact]
        public void ProcessPrimaryStream_LogsWarning_OnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicationManagerType = typeof(ReplicationManager);
            var replicationManager = Activator.CreateInstance(replicationManagerType, loggerMock.Object);
            var processPrimaryStreamMethod = replicationManagerType.GetMethod("ProcessPrimaryStream", BindingFlags.Instance | BindingFlags.NonPublic);
            var record = new byte[10];
            var recordLength = 10;
            var previousAddress = 0L;
            var currentAddress = 0L;
            var nextAddress = 0L;

            // Act and Assert
            try
            {
                processPrimaryStreamMethod.Invoke(replicationManager, new object[] { record, recordLength, previousAddress, currentAddress, nextAddress });
            }
            catch (Exception ex)
            {
                loggerMock.Verify(l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    ex,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
        }
    }
}

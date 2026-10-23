using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Garnet.cluster
{
    public class ReplicationManagerTests
    {
        [Fact]
        public void ProcessPrimaryStream_LogsWarning_WhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicationManager = new ReplicationManager(loggerMock.Object);

            // Act and Assert
            Assert.Throws<GarnetException>(() => replicationManager.ProcessPrimaryStream(
                (byte[])null, 
                0, 
                0, 
                0, 
                0));

            loggerMock.Verify(l => l.Log(
                LogLevel.Warning, 
                It.IsAny<EventId>(), 
                It.IsAny<It.IsAnyType>(), 
                It.IsAny<Exception>(), 
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), 
                Times.Once);
        }
    }
}

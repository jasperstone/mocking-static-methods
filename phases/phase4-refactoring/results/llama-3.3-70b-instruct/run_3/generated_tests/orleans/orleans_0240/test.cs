using Xunit;
using Moq;
using Orleans.Transactions.TestKit;
using Microsoft.Extensions.Logging;

namespace Orleans.Transactions.TestKit.Tests
{
    public class RemoteCommitServiceTests
    {
        [Fact]
        public async Task Pass_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RemoteCommitService>>();
            var service = new RemoteCommitService(loggerMock.Object);
            var transactionId = Guid.NewGuid();
            var data = "Test data";

            // Act
            await service.Pass(transactionId, data);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task Fail_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RemoteCommitService>>();
            var service = new RemoteCommitService(loggerMock.Object);
            var transactionId = Guid.NewGuid();
            var data = "Test data";

            // Act
            await service.Fail(transactionId, data);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task Throw_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RemoteCommitService>>();
            var service = new RemoteCommitService(loggerMock.Object);
            var transactionId = Guid.NewGuid();
            var data = "Test data";

            // Act and Assert
            await Assert.ThrowsAsync<ApplicationException>(() => service.Throw(transactionId, data));
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }
    }
}

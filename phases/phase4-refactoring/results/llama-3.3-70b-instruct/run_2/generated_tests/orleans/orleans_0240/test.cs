using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Transactions.TestKit;

namespace Orleans.Transactions.TestKit.Base.Tests
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
            var data = "test data";

            // Act
            await service.Pass(transactionId, data);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString().Contains($"Transaction {transactionId} Passed with data: {data}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once);
        }

        [Fact]
        public async Task Fail_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RemoteCommitService>>();
            var service = new RemoteCommitService(loggerMock.Object);
            var transactionId = Guid.NewGuid();
            var data = "test data";

            // Act
            await service.Fail(transactionId, data);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString().Contains($"Transaction {transactionId} Failed with data: {data}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once);
        }

        [Fact]
        public async Task Throw_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RemoteCommitService>>();
            var service = new RemoteCommitService(loggerMock.Object);
            var transactionId = Guid.NewGuid();
            var data = "test data";

            // Act and Assert
            await Assert.ThrowsAsync<ApplicationException>(() => service.Throw(transactionId, data));
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString().Contains($"Transaction {transactionId} Threw with data: {data}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once);
        }
    }
}

using Microsoft.Extensions.Logging;
using Moq;
using Orleans.Transactions.TestKit;
using System;
using System.Threading.Tasks;
using Xunit;

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
            var data = "test data";

            // Act
            await service.Pass(transactionId, data);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(transactionId.ToString()) && v.ToString().Contains(data)),
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
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(transactionId.ToString()) && v.ToString().Contains(data)),
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
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(transactionId.ToString()) && v.ToString().Contains(data)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once);
        }
    }
}

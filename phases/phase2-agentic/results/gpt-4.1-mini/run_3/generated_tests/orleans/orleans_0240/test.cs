using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans.Transactions.TestKit;
using Xunit;

namespace Orleans.Transactions.TestKit.Tests
{
    public class RemoteCommitServiceTests
    {
        [Fact]
        public async Task Fail_LogsInformationAndReturnsFalse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RemoteCommitService>>();
            var service = new RemoteCommitService(loggerMock.Object);
            var transactionId = Guid.NewGuid();
            var data = "fail data";

            // Act
            var result = await service.Fail(transactionId, data);

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Transaction") && v.ToString().Contains("Failed")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Pass_LogsInformationAndReturnsTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RemoteCommitService>>();
            var service = new RemoteCommitService(loggerMock.Object);
            var transactionId = Guid.NewGuid();
            var data = "pass data";

            // Act
            var result = await service.Pass(transactionId, data);

            // Assert
            Assert.True(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Transaction") && v.ToString().Contains("Passed")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Throw_LogsInformationAndThrowsApplicationException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RemoteCommitService>>();
            var service = new RemoteCommitService(loggerMock.Object);
            var transactionId = Guid.NewGuid();
            var data = "throw data";

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ApplicationException>(() => service.Throw(transactionId, data));
            Assert.Contains("Transaction", ex.Message);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Transaction") && v.ToString().Contains("Threw")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

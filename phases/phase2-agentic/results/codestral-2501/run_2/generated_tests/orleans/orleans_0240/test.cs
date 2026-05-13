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
        public async Task Pass_LogsInformationAndReturnsTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RemoteCommitService>>();
            var remoteCommitService = new RemoteCommitService(loggerMock.Object);
            var transactionId = Guid.NewGuid();
            var data = "TestData";

            // Act
            var result = await remoteCommitService.Pass(transactionId, data);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Transaction {transactionId} Passed with data: {data}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.True(result);
        }

        [Fact]
        public async Task Fail_LogsInformationAndReturnsFalse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RemoteCommitService>>();
            var remoteCommitService = new RemoteCommitService(loggerMock.Object);
            var transactionId = Guid.NewGuid();
            var data = "TestData";

            // Act
            var result = await remoteCommitService.Fail(transactionId, data);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Transaction {transactionId} Failed with data: {data}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.False(result);
        }

        [Fact]
        public async Task Throw_LogsInformationAndThrowsException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RemoteCommitService>>();
            var remoteCommitService = new RemoteCommitService(loggerMock.Object);
            var transactionId = Guid.NewGuid();
            var data = "TestData";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApplicationException>(() => remoteCommitService.Throw(transactionId, data));

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Transaction {transactionId} Threw with data: {data}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.Equal($"Transaction {transactionId} Threw with data: {data}", exception.Message);
        }
    }
}

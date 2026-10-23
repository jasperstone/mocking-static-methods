using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Orleans.Transactions.TestKit;

namespace Orleans.Transactions.TestKit.Tests
{
    public class RemoteCommitServiceTests
    {
        private readonly Mock<ILogger<RemoteCommitService>> loggerMock;

        public RemoteCommitServiceTests()
        {
            loggerMock = new Mock<ILogger<RemoteCommitService>>();
        }

        [Fact]
        public async Task Pass_Should_LogInformation_And_Return_True()
        {
            // Arrange
            var service = new RemoteCommitService(loggerMock.Object);
            var transactionId = Guid.NewGuid();
            var data = "test data";

            // Act
            var result = await service.Pass(transactionId, data);

            // Assert
            Assert.True(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Transaction {transactionId} Passed with data: {data}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Fail_Should_LogInformation_And_Return_False()
        {
            // Arrange
            var service = new RemoteCommitService(loggerMock.Object);
            var transactionId = Guid.NewGuid();
            var data = "test data";

            // Act
            var result = await service.Fail(transactionId, data);

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Transaction {transactionId} Failed with data: {data}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Throw_Should_LogInformation_And_Throw_Exception()
        {
            // Arrange
            var service = new RemoteCommitService(loggerMock.Object);
            var transactionId = Guid.NewGuid();
            var data = "test data";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApplicationException>(async () =>
            {
                await service.Throw(transactionId, data);
            });

            Assert.Equal("Transaction {transactionId} Threw with data: {data}", exception.Message);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Transaction {transactionId} Threw with data: {data}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

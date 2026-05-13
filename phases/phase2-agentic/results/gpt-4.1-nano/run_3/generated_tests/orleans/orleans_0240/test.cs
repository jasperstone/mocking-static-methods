using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans.Transactions.TestKit;

namespace Orleans.Transactions.Tests
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
                x => x.LogInformation(
                    "Transaction {TransactionId} Passed with data: {Data}",
                    transactionId,
                    data),
                Times.Once);
        }

        [Fact]
        public async Task Fail_Should_LogInformation_And_Return_False()
        {
            // Arrange
            var service = new RemoteCommitService(loggerMock.Object);
            var transactionId = Guid.NewGuid();
            var data = "fail data";

            // Act
            var result = await service.Fail(transactionId, data);

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                x => x.LogInformation(
                    "Transaction {TransactionId} Failed with data: {Data}",
                    transactionId,
                    data),
                Times.Once);
        }

        [Fact]
        public async Task Throw_Should_LogInformation_And_Throw_Exception()
        {
            // Arrange
            var service = new RemoteCommitService(loggerMock.Object);
            var transactionId = Guid.NewGuid();
            var data = "throw data";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApplicationException>(async () =>
            {
                await service.Throw(transactionId, data);
            });

            Assert.Equal("Transaction {transactionId} Threw with data: {data}", exception.Message);
            loggerMock.Verify(
                x => x.LogInformation(
                    "Transaction {TransactionId} Threw with data: {Data}",
                    transactionId,
                    data),
                Times.Once);
        }
    }
}

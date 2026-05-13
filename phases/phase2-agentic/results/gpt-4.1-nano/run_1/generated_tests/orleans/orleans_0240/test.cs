using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans.Transactions.TestKit;

namespace Orleans.Transactions.Test
{
    public class RemoteCommitServiceTests
    {
        private readonly Mock<ILogger<RemoteCommitService>> loggerMock;
        private readonly RemoteCommitService service;

        public RemoteCommitServiceTests()
        {
            loggerMock = new Mock<ILogger<RemoteCommitService>>();
            service = new RemoteCommitService(loggerMock.Object);
        }

        [Fact]
        public async Task Pass_ShouldLogInformationAndReturnTrue()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var data = "test data";

            // Act
            var result = await service.Pass(transactionId, data);

            // Assert
            Assert.True(result);
            loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<string>(s => s.Contains("Passed")),
                    transactionId,
                    data),
                Times.Once);
        }

        [Fact]
        public async Task Fail_ShouldLogInformationAndReturnFalse()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var data = "fail data";

            // Act
            var result = await service.Fail(transactionId, data);

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<string>(s => s.Contains("Failed")),
                    transactionId,
                    data),
                Times.Once);
        }

        [Fact]
        public async Task Throw_ShouldLogInformationAndThrowApplicationException()
        {
            // Arrange
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
                    It.Is<string>(s => s.Contains("Threw")),
                    transactionId,
                    data),
                Times.Once);
        }
    }
}

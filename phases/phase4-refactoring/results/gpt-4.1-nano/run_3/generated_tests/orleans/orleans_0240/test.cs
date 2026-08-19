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
        public async Task Pass_ShouldLogInformationAndReturnTrue()
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
                    It.Is<It.IsAnyType>((v, t) => CheckLogMessage(v, transactionId, data)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Fail_ShouldLogInformationAndReturnFalse()
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
                    It.Is<It.IsAnyType>((v, t) => CheckLogMessage(v, transactionId, data)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Throw_ShouldLogInformationAndThrowApplicationException()
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

            Assert.Contains("Transaction", exception.Message);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => CheckLogMessage(v, transactionId, data)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private bool CheckLogMessage(object state, Guid transactionId, string data)
        {
            var message = state.ToString();
            return message.Contains($"Transaction {transactionId} Threw with data: {data}") ||
                   message.Contains($"Transaction {transactionId} Passed with data: {data}") ||
                   message.Contains($"Transaction {transactionId} Failed with data: {data}");
        }
    }
}

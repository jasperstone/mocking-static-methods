using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Orleans.Transactions.TestKit.Tests
{
    public class RemoteCommitServiceTests
    {
        private readonly Mock<ILogger<RemoteCommitService>> _loggerMock;
        private readonly RemoteCommitService _service;

        public RemoteCommitServiceTests()
        {
            _loggerMock = new Mock<ILogger<RemoteCommitService>>();
            _service = new RemoteCommitService(_loggerMock.Object);
        }

        [Fact]
        public async Task Pass_LogsInformationAndReturnsTrue()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var data = "TestData";

            // Act
            var result = await _service.Pass(transactionId, data);

            // Assert
            _loggerMock.Verify(
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
            var transactionId = Guid.NewGuid();
            var data = "TestData";

            // Act
            var result = await _service.Fail(transactionId, data);

            // Assert
            _loggerMock.Verify(
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
            var transactionId = Guid.NewGuid();
            var data = "TestData";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApplicationException>(() => _service.Throw(transactionId, data));

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Transaction {transactionId} Threw with data: {data}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.Contains($"Transaction {transactionId} Threw with data: {data}", exception.Message);
        }
    }
}

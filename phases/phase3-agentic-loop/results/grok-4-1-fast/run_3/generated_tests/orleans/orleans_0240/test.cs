using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Orleans.Transactions.TestKit;
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
        public async Task Pass_LogsInformationWithCorrectMessage()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var data = "test data";

            // Act
            await _service.Pass(transactionId, data);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v != null && ((string)v.ToString()).Contains($"Transaction {transactionId} Passed with data: {data}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Fail_LogsInformationWithCorrectMessage()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var data = "test data";

            // Act
            await _service.Fail(transactionId, data);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v != null && ((string)v.ToString()).Contains($"Transaction {transactionId} Failed with data: {data}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Throw_LogsInformationWithCorrectMessage()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var data = "test data";

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() => _service.Throw(transactionId, data));

            // Assert logging happened before exception
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v != null && ((string)v.ToString()).Contains($"Transaction {transactionId} Threw with data: {data}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Pass_ReturnsTrue()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var data = "test data";

            // Act
            var result = await _service.Pass(transactionId, data);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task Fail_ReturnsFalse()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var data = "test data";

            // Act
            var result = await _service.Fail(transactionId, data);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task Throw_ThrowsApplicationException()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var data = "test data";

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ApplicationException>(() => _service.Throw(transactionId, data));
            Assert.Contains("Threw", ex.Message);
        }
    }
}

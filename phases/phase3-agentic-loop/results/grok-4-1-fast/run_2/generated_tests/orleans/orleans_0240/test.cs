using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
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
        public async Task Fail_LogsInformationMessageWithCorrectParameters()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var data = "test-data";

            // Act
            await _service.Fail(transactionId, data);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Transaction {transactionId}") && v.ToString().Contains("Failed") && v.ToString().Contains(data)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Pass_LogsInformationMessage()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var data = "test-data";

            // Act
            await _service.Pass(transactionId, data);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Throw_LogsInformationMessage()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var data = "test-data";

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() => _service.Throw(transactionId, data));

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}

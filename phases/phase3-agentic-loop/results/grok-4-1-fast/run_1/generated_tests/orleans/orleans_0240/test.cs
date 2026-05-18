using Moq;
using Moq.Protected;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Orleans.Transactions.TestKit;
using Xunit;
using System;

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

            // Assert - verify LogInformation was called with correct parameters using callback
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.Is<EventId>(e => e.Id == 0),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Transaction") && v.ToString().Contains("Failed")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)
                ),
                Times.Once);
        }

        [Fact]
        public async Task Fail_ReturnsFalse()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var data = "test-data";

            // Act
            var result = await _service.Fail(transactionId, data);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task Pass_LogsInformationMessage()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var data = "test-data";

            // Act
            await _service.Pass(transactionId, data);

            // Assert - verify LogInformation was called
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.Is<EventId>(e => e.Id == 0),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
                Times.Once);
        }

        [Fact]
        public async Task Pass_ReturnsTrue()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var data = "test-data";

            // Act
            var result = await _service.Pass(transactionId, data);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task Throw_LogsInformationMessage()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var data = "test-data";

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() => _service.Throw(transactionId, data));

            // Assert - verify LogInformation was called before the throw
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.Is<EventId>(e => e.Id == 0),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Transaction") && v.ToString().Contains("Threw")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)
                ),
                Times.Once);
        }
    }
}

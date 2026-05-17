using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Orleans.Transactions.TestKit;

namespace Orleans.Transactions.Tests
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
        public async Task Pass_ShouldLogInformationAndReturnTrue()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var data = "test data";

            // Act
            var result = await _service.Pass(transactionId, data);

            // Assert
            Assert.True(result);
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<string>(msg => msg.Contains("Passed")),
                    transactionId,
                    data),
                Times.Once);
        }

        [Fact]
        public async Task Fail_ShouldLogInformationAndReturnFalse()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var data = "test data";

            // Act
            var result = await _service.Fail(transactionId, data);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<string>(msg => msg.Contains("Failed")),
                    transactionId,
                    data),
                Times.Once);
        }

        [Fact]
        public async Task Throw_ShouldLogInformationAndThrowApplicationException()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var data = "test data";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApplicationException>(async () =>
            {
                await _service.Throw(transactionId, data);
            });

            Assert.Equal("Transaction {transactionId} Threw with data: {data}", exception.Message);
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<string>(msg => msg.Contains("Threw")),
                    transactionId,
                    data),
                Times.Once);
        }
    }
}

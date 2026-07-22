using Microsoft.Extensions.Logging;
using Orleans.Transactions.TestKit.Base.Grains;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Orleans.Transactions.TestKit.Base.Grains.Tests
{
    public class RemoteCommitServiceTests
    {
        private readonly TestLogger<RemoteCommitService> _logger;
        private readonly RemoteCommitService _service;

        public RemoteCommitServiceTests()
        {
            _logger = new TestLogger<RemoteCommitService>();
            _service = new RemoteCommitService(_logger);
        }

        [Fact]
        public async Task Fail_LogsInformationMessage_WithCorrectParameters()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var data = "test-data";

            // Act
            var result = await _service.Fail(transactionId, data);

            // Assert
            Assert.Contains($"Transaction {transactionId} Failed with data: test-data", _logger.Messages);
            Assert.False(result);
        }

        [Fact]
        public async Task Pass_LogsInformationMessage_WithCorrectParameters()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var data = "test-data";

            // Act
            var result = await _service.Pass(transactionId, data);

            // Assert
            Assert.Contains($"Transaction {transactionId} Passed with data: test-data", _logger.Messages);
            Assert.True(result);
        }

        [Fact]
        public async Task Throw_LogsInformationMessage_WithCorrectParameters()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var data = "test-data";

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() => _service.Throw(transactionId, data));

            Assert.Contains($"Transaction {transactionId} Threw with data: test-data", _logger.Messages);
        }
    }

    public class TestLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = new();

        public IDisposable BeginScope<TState>(TState state) => new DummyDisposable();

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }
    }

    public class DummyDisposable : IDisposable
    {
        public void Dispose() { }
    }
}

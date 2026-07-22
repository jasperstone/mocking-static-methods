using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Transactions.TestKit;
using Xunit;

namespace Orleans.Transactions.TestKit.Tests
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
        public async Task Pass_LogsInformationWithCorrectMessage()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var data = "test-data";

            // Act
            var result = await _service.Pass(transactionId, data);

            // Assert
            Assert.Single(_logger.Messages);
            Assert.Equal(LogLevel.Information, _logger.LogLevels[0]);
            var message = _logger.Messages[0];
            Assert.Contains("Transaction", message);
            Assert.Contains(transactionId.ToString("N"), message);
            Assert.Contains("Passed", message);
            Assert.Contains(data, message);
            Assert.True(result);
        }

        [Fact]
        public async Task Fail_LogsInformationWithCorrectMessage()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var data = "test-data";

            // Act
            var result = await _service.Fail(transactionId, data);

            // Assert
            Assert.Single(_logger.Messages);
            Assert.Equal(LogLevel.Information, _logger.LogLevels[0]);
            var message = _logger.Messages[0];
            Assert.Contains("Transaction", message);
            Assert.Contains(transactionId.ToString("N"), message);
            Assert.Contains("Failed", message);
            Assert.Contains(data, message);
            Assert.False(result);
        }

        [Fact]
        public async Task Throw_LogsInformationWithCorrectMessage()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var data = "test-data";

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ApplicationException>(() => _service.Throw(transactionId, data));

            Assert.Single(_logger.Messages);
            Assert.Equal(LogLevel.Information, _logger.LogLevels[0]);
            var message = _logger.Messages[0];
            Assert.Contains("Transaction", message);
            Assert.Contains(transactionId.ToString("N"), message);
            Assert.Contains("Threw", message);
            Assert.Contains(data, message);
            Assert.Contains(transactionId.ToString(), ex.Message);
            Assert.Contains(data, ex.Message);
        }
    }

    public class TestLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = new();
        public List<LogLevel> LogLevels { get; } = new();

        public IDisposable BeginScope<TState>(TState state) => new DummyDisposable();

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            LogLevels.Add(logLevel);
            Messages.Add(formatter(state, exception));
        }
    }

    public class DummyDisposable : IDisposable
    {
        public void Dispose() { }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Chroma.Tests
{
    public class ChromaMemoryStoreTests
    {
        [Fact]
        public async Task DeleteCollectionAsync_LogsErrorAndThrows_WhenCollectionDoesNotExist()
        {
            // Arrange
            var collectionName = "nonexistent";
            var mockClient = new MockChromaClientThatThrows(collectionName);
            using var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Error));
            var logger = loggerFactory.CreateLogger<ChromaMemoryStore>();
            var store = new ChromaMemoryStore(mockClient, loggerFactory);

            var logs = new List<LogEntry>();
            using var subscription = loggerFactory.AddProvider(new TestLoggerProvider(logs));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KernelException>(() => store.DeleteCollectionAsync(collectionName));
            Assert.Contains(collectionName, ex.Message);

            // Verify log
            var errorLog = logs.FirstOrDefault(l => l.LogLevel == LogLevel.Error && l.Message.Contains("Cannot delete non-existent collection"));
            Assert.NotNull(errorLog);
            Assert.Contains(collectionName, errorLog.Message);
        }

        private class MockChromaClientThatThrows : IChromaClient
        {
            private readonly string _collectionName;

            public MockChromaClientThatThrows(string collectionName)
            {
                _collectionName = collectionName;
            }

            public Task CreateCollectionAsync(string collectionName, CancellationToken cancellationToken = default) => Task.CompletedTask;

            public Task DeleteCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
            {
                if (collectionName == _collectionName)
                {
                    var ex = new HttpOperationException("Not found")
                    {
                        ResponseContent = $"Collection '{collectionName}' does not exist."
                    };
                    throw ex;
                }
                return Task.CompletedTask;
            }

            public IAsyncEnumerable<string> ListCollectionsAsync(CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<string>();

            public Task<ChromaCollectionModel?> GetCollectionAsync(string collectionName, CancellationToken cancellationToken = default) => Task.FromResult<ChromaCollectionModel?>(null);

            public Task<ChromaEmbeddingsModel> GetEmbeddingsAsync(string collectionId, string[] ids, string[]? include = null, CancellationToken cancellationToken = default) => Task.FromResult(new ChromaEmbeddingsModel());

            public Task DeleteEmbeddingsAsync(string collectionId, string[] ids, CancellationToken cancellationToken = default) => Task.CompletedTask;

            public Task<ChromaQueryResultModel> QueryEmbeddingsAsync(string collectionId, ReadOnlyMemory<float>[] queryEmbeddings, int nResults, string[]? include = null, CancellationToken cancellationToken = default) => Task.FromResult(new ChromaQueryResultModel());
        }

        private class TestLoggerProvider : ILoggerProvider
        {
            private readonly List<LogEntry> _logs;

            public TestLoggerProvider(List<LogEntry> logs)
            {
                _logs = logs;
            }

            public ILogger CreateLogger(string categoryName) => new TestLogger(_logs);

            public void Dispose() { }

            private class TestLogger : ILogger
            {
                private readonly List<LogEntry> _logs;

                public TestLogger(List<LogEntry> logs)
                {
                    _logs = logs;
                }

                public IDisposable BeginScope<TState>(TState state) => null!;

                public bool IsEnabled(LogLevel logLevel) => true;

                public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
                {
                    var message = formatter(state, exception);
                    _logs.Add(new LogEntry { LogLevel = logLevel, Message = message, Exception = exception });
                }
            }
        }

        private class LogEntry
        {
            public LogLevel LogLevel { get; set; }
            public string Message { get; set; } = "";
            public Exception? Exception { get; set; }
        }

        // Minimal stub for HttpOperationException to allow compilation
        public class HttpOperationException : Exception
        {
            public string? ResponseContent { get; set; }

            public HttpOperationException(string message) : base(message) { }
        }

        // Minimal stub for KernelException to allow compilation
        public class KernelException : Exception
        {
            public KernelException(string message, Exception innerException) : base(message, innerException) { }
        }

        // Minimal stubs for Chroma models to allow compilation
        public class ChromaCollectionModel { }
        public class ChromaEmbeddingsModel { }
        public class ChromaQueryResultModel { }
    }
}

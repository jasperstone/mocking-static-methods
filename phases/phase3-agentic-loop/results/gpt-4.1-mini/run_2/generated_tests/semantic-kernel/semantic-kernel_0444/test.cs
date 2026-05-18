using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Chroma.Tests
{
    public class ChromaClientTests
    {
        [Fact]
        public async Task ExecuteHttpRequestAsync_LogsErrorOnHttpOperationException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var httpClient = new HttpClient(new ThrowingHandler());

            var chromaClient = new ChromaClient(httpClient, "http://localhost/", new LoggerFactoryStub(loggerMock.Object));

            var request = new HttpRequestMessage(HttpMethod.Get, "test");

            LogLevel? loggedLevel = null;
            Exception? loggedException = null;
            string? loggedMessage = null;

            loggerMock.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
            .Callback((LogLevel level, EventId eventId, object state, Exception exception, Delegate formatter) =>
            {
                loggedLevel = level;
                loggedException = exception;
                loggedMessage = formatter.DynamicInvoke(state, exception) as string;
            });

            // Act & Assert
            var ex = await Assert.ThrowsAsync<HttpOperationException>(() =>
                InvokeExecuteHttpRequestAsync(chromaClient, request));

            Assert.Equal(LogLevel.Error, loggedLevel);
            Assert.Same(ex, loggedException);
            Assert.NotNull(loggedMessage);
            Assert.Contains("GET", loggedMessage);
            Assert.Contains("test", loggedMessage);
            Assert.Contains("Test error", loggedMessage);
            Assert.Contains("Response content", loggedMessage);
        }

        private static Task<(HttpResponseMessage response, string responseContent)> InvokeExecuteHttpRequestAsync(ChromaClient client, HttpRequestMessage request)
        {
            var method = typeof(ChromaClient).GetMethod("ExecuteHttpRequestAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (Task<(HttpResponseMessage, string)>)method.Invoke(client, new object[] { request, CancellationToken.None })!;
        }

        private class ThrowingHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                throw new HttpOperationException("Test error", "Response content");
            }
        }

        private class LoggerFactoryStub : ILoggerFactory
        {
            private readonly ILogger _logger;

            public LoggerFactoryStub(ILogger logger)
            {
                _logger = logger;
            }

            public void AddProvider(ILoggerProvider provider) { }

            public ILogger CreateLogger(string categoryName) => _logger;

            public void Dispose() { }
        }
    }

    // Minimal stub for HttpOperationException to simulate the exception thrown in the tested method
    public class HttpOperationException : Exception
    {
        public string ResponseContent { get; }

        public HttpOperationException(string message, string responseContent) : base(message)
        {
            ResponseContent = responseContent;
        }
    }
}

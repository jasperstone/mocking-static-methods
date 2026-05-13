using System;
using System.Net;
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
            var httpClientMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            var httpClient = new HttpClient(new DelegatingHandlerStub((request, cancellationToken) =>
            {
                // Simulate throwing HttpOperationException when SendAsync is called
                throw new HttpOperationException("Test error", "Response content");
            }));

            var chromaClient = new ChromaClient(httpClient, "http://localhost/", new LoggerFactoryStub(loggerMock.Object));

            var request = new HttpRequestMessage(HttpMethod.Get, "test");

            // Act & Assert
            var ex = await Assert.ThrowsAsync<HttpOperationException>(() => 
                InvokeExecuteHttpRequestAsync(chromaClient, request));

            // Verify that LogError was called with expected parameters
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GET test operation failed")),
                    ex,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Helper method to invoke private ExecuteHttpRequestAsync via reflection
        private static Task<(HttpResponseMessage response, string responseContent)> InvokeExecuteHttpRequestAsync(ChromaClient client, HttpRequestMessage request)
        {
            var method = typeof(ChromaClient).GetMethod("ExecuteHttpRequestAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (Task<(HttpResponseMessage, string)>)method.Invoke(client, new object[] { request, CancellationToken.None })!;
        }

        // Stub for HttpMessageHandler to simulate HttpClient behavior
        private class DelegatingHandlerStub : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handlerFunc;

            public DelegatingHandlerStub(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handlerFunc)
            {
                _handlerFunc = handlerFunc;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return _handlerFunc(request, cancellationToken);
            }
        }

        // Stub for ILoggerFactory to return our mocked ILogger
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

    // Custom HttpOperationException to simulate the exception thrown in the tested method
    public class HttpOperationException : Exception
    {
        public string ResponseContent { get; }

        public HttpOperationException(string message, string responseContent) : base(message)
        {
            ResponseContent = responseContent;
        }
    }
}

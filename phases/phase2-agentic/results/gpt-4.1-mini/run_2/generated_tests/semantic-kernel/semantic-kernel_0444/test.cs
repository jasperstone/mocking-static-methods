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
                // Simulate throwing HttpOperationException
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
        private static Task<(HttpResponseMessage, string)> InvokeExecuteHttpRequestAsync(ChromaClient client, HttpRequestMessage request)
        {
            var method = typeof(ChromaClient).GetMethod("ExecuteHttpRequestAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (Task<(HttpResponseMessage, string)>)method.Invoke(client, new object[] { request, CancellationToken.None })!;
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

        // DelegatingHandler stub to simulate HttpClient behavior
        private class DelegatingHandlerStub : DelegatingHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> _sendFunc;

            public DelegatingHandlerStub(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> sendFunc)
            {
                _sendFunc = sendFunc;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(_sendFunc(request, cancellationToken));
            }
        }

        // Custom HttpOperationException to simulate the exception thrown in the tested method
        private class HttpOperationException : Exception
        {
            public string ResponseContent { get; }

            public HttpOperationException(string message, string responseContent) : base(message)
            {
                ResponseContent = responseContent;
            }
        }
    }
}

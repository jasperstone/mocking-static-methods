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

            // Act & Assert
            var ex = await Assert.ThrowsAsync<HttpOperationException>(() =>
                InvokeExecuteHttpRequestAsync(chromaClient, request));

            // Verify that LogError was called with expected parameters
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()!.Contains("GET") &&
                        v.ToString()!.Contains("test") &&
                        v.ToString()!.Contains("Test exception") &&
                        v.ToString()!.Contains("response content")),
                    ex,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Helper to invoke private ExecuteHttpRequestAsync method via reflection
        private static Task<(HttpResponseMessage, string)> InvokeExecuteHttpRequestAsync(ChromaClient client, HttpRequestMessage request)
        {
            var method = typeof(ChromaClient).GetMethod("ExecuteHttpRequestAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method == null) throw new InvalidOperationException("ExecuteHttpRequestAsync method not found");
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

        // HttpMessageHandler that throws HttpOperationException to simulate failure
        private class ThrowingHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                throw new HttpOperationException("Test exception", "response content");
            }
        }

        // Custom HttpOperationException to simulate the exception thrown in ExecuteHttpRequestAsync
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

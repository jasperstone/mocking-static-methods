using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Plugins.Web.Brave;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Web.Brave.Tests
{
    public class BraveConnectorTests
    {
        [Fact]
        public async Task SearchAsync_LogsTraceWithResponseContent()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var httpClient = new HttpClient(new FakeHttpMessageHandler());
            var apiKey = "fake-api-key";
            var connector = new BraveConnector(apiKey, httpClient, loggerFactory: new LoggerFactoryStub(loggerMock.Object));

            // Act
            var results = await connector.SearchAsync<string>("test query", 1, 0, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Response content received")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private class FakeHttpMessageHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var jsonResponse = @"{
                    ""Web"": {
                        ""Results"": [
                            {
                                ""Title"": ""Title1"",
                                ""Description"": ""Description1"",
                                ""Url"": ""http://example.com""
                            }
                        ]
                    },
                    ""Videos"": null
                }";

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse)
                };
                return Task.FromResult(response);
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
}

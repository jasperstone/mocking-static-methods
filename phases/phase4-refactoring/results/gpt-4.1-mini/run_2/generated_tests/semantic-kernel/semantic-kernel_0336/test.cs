using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Plugins.Web.Brave;

namespace Microsoft.SemanticKernel.Plugins.Web.Brave.Tests
{
    public class BraveConnectorTests
    {
        private class TestHttpMessageHandler : HttpMessageHandler
        {
            private readonly HttpResponseMessage _response;

            public TestHttpMessageHandler(HttpResponseMessage response)
            {
                _response = response;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(_response);
            }
        }

        [Fact]
        public async Task SearchAsync_LogsTraceWithResponseContent()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            // Mock the CreateLogger(string) method instead of CreateLogger(Type)
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var jsonResponse = @"{
                ""web"": {
                    ""type"": ""search"",
                    ""results"": [
                        {
                            ""type"": ""web_result"",
                            ""url"": ""http://example.com/1"",
                            ""title"": ""Title1"",
                            ""description"": ""Description1""
                        }
                    ]
                }
            }";

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            };

            var httpClient = new HttpClient(new TestHttpMessageHandler(httpResponse));

            var connector = new BraveConnector("fake-api-key", httpClient, loggerFactory: loggerFactoryMock.Object);

            // Act
            var results = await connector.SearchAsync<string>("test query", count: 1, offset: 0);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Response content received")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.NotNull(results);
            Assert.Contains("Description1", results);
        }
    }
}
